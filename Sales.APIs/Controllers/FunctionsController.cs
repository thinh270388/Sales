using Microsoft.AspNetCore.Mvc;
using Sales.APIs.Repositories.Constracts;
using Sales.Models.Entities;

namespace Sales.APIs.Controllers
{
    [Route("sales/[controller]")]
    [ApiController]
    public class FunctionsController : GenericController<Functions>
    {
        public FunctionsController(IGenericRepository<Functions> repository) : base(repository)
        {
        }
    }
}