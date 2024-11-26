using Sales.Models;
using Sales.Models.DTOs;

namespace Sales.APIs.Repositories.Constracts
{
    public interface IOtherRepository
    {
        DtoResult<UsersDto> GetFullUser();
    }
}
