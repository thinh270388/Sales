using Microsoft.AspNetCore.Mvc;
using Sales.APIs.Repositories.Constracts;
using Sales.Models.Entities;

namespace Sales.APIs.Controllers
{
    [Route("sales/[controller]")]
    [ApiController]
    public class PermissionsController : GenericController<Permissions>
    {
        public PermissionsController(IGenericRepository<Permissions> repository) : base(repository)
        {
        }
    }
}