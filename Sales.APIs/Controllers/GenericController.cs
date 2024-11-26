using Microsoft.AspNetCore.Mvc;
using Sales.APIs.Repositories.Constracts;
using Sales.Models;

namespace Sales.APIs.Controllers
{
    public class GenericController<T> : ControllerBase
    {
        private readonly IGenericRepository<T> _repository;

        public GenericController(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        [HttpGet("get-all")]
        public ActionResult<DtoResult<T>> GetAll()
        {
            try
            {
                var result = _repository.GetAll();
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DtoResult<T>()
                {
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    Success = false
                });
            }
        }

        [HttpPost("get-one")]
        public ActionResult<DtoResult<T>> GetOne(T dto)
        {
            try
            {
                var result = _repository.GetOne(dto);
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DtoResult<T>()
                {
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    Success = false
                });
            }
        }

        [HttpPost("find")]
        public ActionResult<DtoResult<T>> Find(T dto)
        {
            try
            {
                var result = _repository.Find(dto);
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DtoResult<T>()
                {
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    Success = false
                });
            }

        }

        [HttpPost("add")]
        public ActionResult<DtoResult<T>> Add(T dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new DtoResult<T>()
                    {
                        Message = "Đã xảy ra lỗi xác thực",
                        Success = false
                    });
                }
                var result = _repository.Add(dto);
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DtoResult<T>()
                {
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    Success = false
                });
            }
        }

        [HttpPost("update")]
        public ActionResult<DtoResult<T>> Update(T dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new DtoResult<T>()
                    {
                        Message = "Đã xảy ra lỗi xác thực",
                        Success = false
                    });
                }
                var result = _repository.Update(dto);
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DtoResult<T>()
                {
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    Success = false
                });
            }
        }

        [HttpPost("delete")]
        public ActionResult<DtoResult<T>> Delete(T dto)
        {
            try
            {
                var result = _repository.Delete(dto);
                if (result.Success)
                    return Ok(result);
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DtoResult<T>()
                {
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    Success = false
                });
            }
        }
    }
}