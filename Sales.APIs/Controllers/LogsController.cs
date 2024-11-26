using Microsoft.AspNetCore.Mvc;
using Sales.APIs.Repositories.Constracts;
using Sales.Models.Entities;

namespace Sales.APIs.Controllers
{
    [Route("sales/[controller]")]
    [ApiController]
    public class LogsController : GenericController<Logs>
    {
        public LogsController(IGenericRepository<Logs> repository) : base(repository)
        {
        }
    }
}