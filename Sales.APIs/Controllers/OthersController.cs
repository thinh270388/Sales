using Microsoft.AspNetCore.Mvc;
using Sales.APIs.Repositories.Constracts;
using Sales.Models;
using Sales.Models.DTOs;

namespace Sales.APIs.Controllers
{
    [Route("sales/[controller]")]
    [ApiController]
    public class OthersController : ControllerBase
    {
        private readonly IOtherRepository _otherRepository;
        public OthersController(IOtherRepository otherRepository)
        { 
            _otherRepository = otherRepository;
        }

        [HttpGet("get-full-user")]
        public ActionResult<DtoResult<UsersDto>> GetFullUser()
        {
            try
            {
                var result = _otherRepository.GetFullUser();
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DtoResult<UsersDto>()
                {
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    Success = false
                });
            }
        }
    }
}
