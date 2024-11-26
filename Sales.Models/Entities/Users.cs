using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Sales.Models.Entities
{
	public class Users
	{
		public Users()
		{
			List<PropertyInfo> properties = GetType().GetProperties().ToList();
			foreach (PropertyInfo property in properties)
			{
				property.SetValue(this, null);
			}
			TypeList = new(){ "int", "nvarchar", "nvarchar", "nvarchar", "bit", "nvarchar", "nvarchar", "bit", "datetime", "bit", "int", "nvarchar", "datetime", "bit", "int" };
		}
		public Users(int? id, string? fullname, string? username, string? email, bool? emailconfirmed, string? passwordhash, string? phonenumber, bool? phonenumberconfirmed, DateTime? lockoutend, bool? lockedenable, int? accessfailedcount, string? refreshtoken, DateTime? refreshtokenexpirytime, bool? issystem, int? roleid)
		{
			Id = id;
			FullName = fullname;
			UserName = username;
			Email = email;
			EmailConfirmed = emailconfirmed;
			PasswordHash = passwordhash;
			PhoneNumber = phonenumber;
			PhoneNumberConfirmed = phonenumberconfirmed;
			LockoutEnd = lockoutend;
			LockedEnable = lockedenable;
			AccessFailedCount = accessfailedcount;
			RefreshToken = refreshtoken;
			RefreshTokenExpiryTime = refreshtokenexpirytime;
			IsSystem = issystem;
			RoleId = roleid;
			TypeList = new(){ "int", "nvarchar", "nvarchar", "nvarchar", "bit", "nvarchar", "nvarchar", "bit", "datetime", "bit", "int", "nvarchar", "datetime", "bit", "int" };
		}
		public int? Id { get; set; }
        [Required(ErrorMessage = "Tên không được để trống.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 256 ký tự.")]
        public string? FullName { get; set; }
		public string? UserName { get; set; }
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
		public string? Email { get; set; }
		public bool? EmailConfirmed { get; set; }
        [MinLength(5, ErrorMessage = "Độ dài tối thiểu là 8 ký tự.")]
        public string? PasswordHash { get; set; }
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? PhoneNumber { get; set; }
		public bool? PhoneNumberConfirmed { get; set; }
		public DateTime? LockoutEnd { get; set; }
		public bool? LockedEnable { get; set; }
		public int? AccessFailedCount { get; set; }
		public string? RefreshToken { get; set; }
		public DateTime? RefreshTokenExpiryTime { get; set; }
		public bool? IsSystem { get; set; }
		public int? RoleId { get; set; }
		public List<string> TypeList { get; set; }
	}
}
