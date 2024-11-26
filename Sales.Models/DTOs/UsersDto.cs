using Sales.Models.Entities;

namespace Sales.Models.DTOs
{
    public class UsersDto : Users
    {
        public string? RoleName { get; set; }
        public UsersDto Clone() { return (UsersDto)MemberwiseClone(); }
    }
}
