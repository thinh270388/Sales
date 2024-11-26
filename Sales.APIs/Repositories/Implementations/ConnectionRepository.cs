using Sales.APIs.Helpers;
using Sales.APIs.Repositories.Constracts;

namespace Sales.APIs.Repositories.Implementations
{
    public class ConnectionRepository : IConnectionRepository
    {
        private readonly IConfiguration _configuration;

        public ConnectionRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GetConnectionString()
        {
            var encryption = new Encryption();
            return encryption.Decrypt(_configuration.GetConnectionString("DefaultConnection")!);
        }
    }
}