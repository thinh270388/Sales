using Sales.Models.Entities;

namespace Sales.Models.Responses
{
    public class LoginResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? AccessTokenExpiryTime { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public Users? User { get; set; }
        public Roles? Role { get; set; }
        public List<Permissions>? Permissions { get; set; }
    }
}
