using Microsoft.Data.SqlClient;
using Sales.APIs.Repositories.Constracts;
using Sales.Models;
using Sales.Models.DTOs;
using System.Data;
using System.Security.Claims;

namespace Sales.APIs.Repositories.Implementations
{
    public class OtherRepository : IOtherRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<OtherRepository> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OtherRepository(IConnectionRepository connectionRepository, ILogger<OtherRepository> logger, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = connectionRepository.GetConnectionString();
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public DtoResult<UsersDto> GetFullUser()
        {
            DtoResult<UsersDto> result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "SELECT u.Id, u.FullName, u.UserName, u.Email, u.EmailConfirmed, u.PasswordHash, u.PhoneNumber, u.PhoneNumberConfirmed, u.LockoutEnd, u.LockedEnable, u.AccessFailedCount, u.RefreshToken, u.RefreshTokenExpiryTime, u.IsSystem, u.RoleId, r.Name as RoleName\r\nFROM dbo.Users u\r\nJOIN dbo.Roles r\r\nON u.RoleId = r.Id";
                    SqlDataAdapter adapter = new(sql, conn);
                    DataTable dt = new();
                    adapter.Fill(dt);
                    conn.Close();
                    List<UsersDto> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new UsersDto
                    {
                        Id = x.Field<int?>("Id"),
                        FullName = x.Field<string?>("FullName"),
                        UserName = x.Field<string?>("UserName"),
                        Email = x.Field<string?>("Email"),
                        EmailConfirmed = x.Field<bool?>("EmailConfirmed"),
                        PasswordHash = x.Field<string?>("PasswordHash"),
                        PhoneNumber = x.Field<string?>("PhoneNumber"),
                        PhoneNumberConfirmed = x.Field<bool?>("PhoneNumberConfirmed"),
                        LockoutEnd = x.Field<DateTime?>("LockoutEnd"),
                        LockedEnable = x.Field<bool?>("LockedEnable"),
                        AccessFailedCount = x.Field<int?>("AccessFailedCount"),
                        RefreshToken = x.Field<string?>("RefreshToken"),
                        RefreshTokenExpiryTime = x.Field<DateTime?>("RefreshTokenExpiryTime"),
                        IsSystem = x.Field<bool?>("IsSystem"),
                        RoleId = x.Field<int?>("RoleId"),
                        RoleName = x.Field<string?>("RoleName")
                    }).ToList();

                    result.Success = true;
                    result.Results = rs;
                    result.Message = $"{user} GetFull thành công trong bảng Users";
                    _logger.LogInformation(result.Message);
                }
            }
            catch (Exception ex)
            {
                result.Message = $"{user} GetFull thất bại trong bảng Users vì lỗi: {ex.Message}";
                result.Success = false;
                _logger.LogInformation(result.Message);
            }

            return result;
        }
    }
}
