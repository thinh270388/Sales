using Microsoft.IdentityModel.Tokens;
using Sales.APIs.Helpers;
using Sales.APIs.Repositories.Constracts;
using Sales.Models;
using Sales.Models.Entities;
using Sales.Models.Requests;
using Sales.Models.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Sales.APIs.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly Encryption encryption = new();
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthRepository> _logger;
        private readonly IGenericRepository<Users> _usersRepository;
        private readonly IGenericRepository<Roles> _rolesRepository;
        private readonly IGenericRepository<Permissions> _permissionsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthRepository(
            IConfiguration configuration,
            ILogger<AuthRepository> logger,
            IGenericRepository<Users> usersRepository,
            IGenericRepository<Roles> rolesRepository,
            IGenericRepository<Permissions> permissionsRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _logger = logger;
            _usersRepository = usersRepository;
            _rolesRepository = rolesRepository;
            _permissionsRepository = permissionsRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        public DtoResult<LoginResponse> Login(Sales.Models.Requests.LoginRequest request)
        {
            var result = new DtoResult<LoginResponse>();
            try
            {
                var rsUsers = _usersRepository.GetOne(new Users() { Email = request.Email, PasswordHash = encryption.Encrypt(request.Password) });
                if (rsUsers.Success && rsUsers.Result != null)
                {
                    var user = rsUsers.Result;
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()!),
                        new Claim(ClaimTypes.Email, user.Email!),
                        new Claim(ClaimTypes.GivenName, user!.FullName!)
                    };

                    var rsRoles = _rolesRepository.GetOne(new Roles() { Id = user.RoleId });
                    if (rsRoles.Success && rsRoles.Result != null)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, rsRoles.Result!.Name!));
                    }
                    var rsPermissions = _permissionsRepository.Find(new Permissions() { RoleId = user.RoleId });

                    user.RefreshToken = BuildRefreshToken();
                    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JWT:RefreshTokenExpiryInDays"]));
                    _usersRepository.Update(user);

                    result.Message = "Đăng nhập thành công";
                    result.Success = true;
                    _logger.LogInformation(result.Message);
                    result.Result = new LoginResponse()
                    {
                        AccessToken = BuildAccessToken(authClaims),
                        RefreshToken = BuildRefreshToken(),
                        AccessTokenExpiryTime = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JWT:AccessTokenExpiryInMinutes"])),
                        RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
                        User = user,
                        Role = rsRoles.Result,
                        Permissions = rsPermissions.Results
                    };
                }
                else
                {
                    result.Message = "Đăng nhập không thành công. Vui lòng kiểm tra Email/Password";
                    result.Success = false;
                    _logger.LogError(result.Message);
                }    
            }
            catch (Exception ex)
            {
                result.Message = $"Đăng nhập không thành công. Xảy ra lỗi: {ex.Message}";
                result.Success = false;
                _logger.LogError(result.Message);
            }

           return result;
        }
        public DtoResult<LoginResponse> RefreshAccessToken(TokenRequest request)
        {
            string message = string.Empty;
            try
            {
                var principal = GetPrincipalFromExpiredToken(request.AccessToken);
                if (principal?.Identity?.Name == null)
                {
                    message = "Không thể tạo mã Access Token vì không tìm thấy người dùng";
                    _logger.LogError(message);
                    return new DtoResult<LoginResponse>() { Success = false, Message = message };
                }

                var rsUsers = _usersRepository.GetOne(new Users() { Email = principal?.Identity?.Name });
                if (rsUsers.Success && (rsUsers.Result == null || rsUsers.Result!.RefreshToken != request.RefreshToken || rsUsers.Result.RefreshTokenExpiryTime <= DateTime.Now))
                {
                    message = "Không thể tạo mã Access Token vì người dùng chưa đăng nhập";
                    _logger.LogError(message);
                    return new DtoResult<LoginResponse>() { Success = false, Message = message };
                }

                var user = rsUsers.Result;
                user!.RefreshToken = BuildRefreshToken();
                user!.RefreshTokenExpiryTime = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JWT:RefreshTokenExpiryInDays"]));
                var updateUser = _usersRepository.Update(user);
                var rsRoles = _rolesRepository.GetOne(new Roles() { Id = user.RoleId });
                var rsPermissions = _permissionsRepository.Find(new Permissions() { RoleId = user.RoleId });

                message = "Tạo mã Access Token thành công";
                _logger.LogInformation(message);
                return new DtoResult<LoginResponse>()
                {
                    Message = message,
                    Success = true,
                    Result = new LoginResponse()
                    {
                        AccessToken = BuildAccessToken(principal!.Claims.ToList()),
                        RefreshToken = user.RefreshToken,
                        AccessTokenExpiryTime = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JWT:AccessTokenExpiryInMinutes"])),
                        RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
                        User = updateUser.Result,
                        Role = rsRoles.Result,
                        Permissions = rsPermissions.Results != null ? rsPermissions.Results : null
                    }
                };
            }
            catch (Exception ex)
            {
                message = $"Không thể tạo mã Access Token vì xảy ra lỗi: {ex.Message}";
                _logger.LogError(message);
                return new DtoResult<LoginResponse>() { Success = false, Message = message };
            }
        }
        public DtoResult<LoginResponse> RevokeAccessTokenAll()
        {
            string message = string.Empty;
            try
            {
                var rs = _usersRepository.GetAll();
                if (rs.Success)
                {
                    foreach (var user in rs.Results!)
                    {
                        user.RefreshToken = null;
                        user.RefreshTokenExpiryTime = null;

                        rs = _usersRepository.Update(user);
                    }
                    message = "Thu hồi tất cả Refresh Token thành công";
                    _logger.LogInformation(message);
                    return new DtoResult<LoginResponse>() { Success = true, Message = message };
                }
                message = "Thu hồi tất cả Refresh Token thất bại";
                _logger.LogError(message);
                return new DtoResult<LoginResponse>() { Success = false, Message = message };
            }
            catch (Exception ex)
            {
                message = $"Thu hồi tất cả Refresh Token thất bại vì xảy ra lỗi: {ex.Message}";
                _logger.LogError(message);
                return new DtoResult<LoginResponse>() { Success = false, Message = message };
            }
            
        }
        public DtoResult<LoginResponse> RevokeAccessTokenByEmail(string email)
        {
            string message = string.Empty;
            try
            {
                var rs = _usersRepository.GetOne(new Users() { Email = email });
                if (rs.Success && rs.Result != null)
                {
                    var user = rs.Result;
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;

                    rs = _usersRepository.Update(user);

                    message = $"Thu hồi Refresh Token của {email} thành công";
                    _logger.LogInformation(message);
                    return new DtoResult<LoginResponse>() { Success = true, Message = message };
                }

                message = $"Thu hồi Refresh Token của {email} thất bại";
                _logger.LogError(message);
                return new DtoResult<LoginResponse>() { Success = false, Message = message };
            }
            catch (Exception ex)
            {
                message = $"Thu hồi Refresh Token của {email} thất bại vì có lỗi xảy ra: {ex.Message}";
                _logger.LogError(message);
                return new DtoResult<LoginResponse>() { Success = false, Message = message };
            }
            
        }

        private string BuildAccessToken(List<Claim> authClaims)
        {
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JWT:AccessTokenExpiryInMinutes"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!)), SecurityAlgorithms.HmacSha256Signature)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string BuildRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!)),
                ValidateLifetime = false // Đặt thành false để cho phép kiểm tra token đã hết hạn
            };
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                // Kiểm tra xem token có đúng định dạng JWT không
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null; // Token không hợp lệ
                }
                return principal; // Trả về ClaimsPrincipal hợp lệ
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Token validation failed: {ex.Message}");
                return null;
            }
        }
    }
}