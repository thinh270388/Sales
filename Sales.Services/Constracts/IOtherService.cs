using Sales.Models;
using Sales.Models.DTOs;

namespace Sales.Services.Constracts
{
    public interface IOtherService
    {
        Task<DtoResult<UsersDto>> GetFullUserAsync();
    }
}
