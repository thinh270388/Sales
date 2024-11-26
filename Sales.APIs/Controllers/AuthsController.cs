using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sales.APIs.Repositories.Constracts;
using Sales.Models;
using Sales.Models.Entities;
using Sales.Models.Requests;
using Sales.Models.Responses;

namespace Sales.APIs.Controllers
{
    [Route("sales/[controller]")]
    [ApiController]
    public class AuthsController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IGenericRepository<Users> _usersRepository;
        public AuthsController(IAuthRepository authRepository, IGenericRepository<Users> usersRepository)
        {
            _authRepository = authRepository;
            _usersRepository = usersRepository;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult<DtoResult<LoginResponse>> Login(Sales.Models.Requests.LoginRequest request)
        {
            try
            {
                var result = _authRepository.Login(request);
                if (result.Success)
                    return Ok(result);
                else
                    return Unauthorized(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DtoResult<LoginResponse>()
                {
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    Success = false
                });
            } 
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public ActionResult<DtoResult<LoginResponse>> RefreshAccessToken(TokenRequest request)
        {
            try
            {
                var result = _authRepository.RefreshAccessToken(request);
                if (result.Success)
                    return Ok(result);
                else
                    return Unauthorized(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DtoResult<LoginResponse>()
                {
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    Success = false
                });
            }
        }

        [HttpPost("revoke-all")]
        [AllowAnonymous]
        public ActionResult<DtoResult<LoginResponse>> RevokeAccessTokenAll()
        {
            try
            {
                var result = _authRepository.RevokeAccessTokenAll();
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DtoResult<LoginResponse>()
                {
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    Success = false
                });
            }
        }

        [HttpPost("revoke")]
        [AllowAnonymous]
        public ActionResult<DtoResult<LoginResponse>> RevokeAccessTokenByEmail(string email)
        {
            try
            {
                var result = _authRepository.RevokeAccessTokenByEmail(email);
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DtoResult<LoginResponse>()
                {
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    Success = false
                });
            }
        }
    }    
}