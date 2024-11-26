using Microsoft.AspNetCore.Mvc;
using Sales.APIs.Repositories.Constracts;
using Sales.Models.Entities;

namespace Sales.APIs.Controllers
{
    [Route("sales/[controller]")]
    [ApiController]
    public class RolesController : GenericController<Roles>
    {
        public RolesController(IGenericRepository<Roles> repository) : base(repository)
        {
        }
    }
}