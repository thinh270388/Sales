using Microsoft.AspNetCore.Mvc;
using Sales.APIs.Repositories.Constracts;
using Sales.Models.Entities;

namespace Sales.APIs.Controllers
{
    [Route("sales/[controller]")]
    [ApiController]
    public class UsersController : GenericController<Users>
    {
        public UsersController(IGenericRepository<Users> repository) : base(repository)
        {
        }
    }
}