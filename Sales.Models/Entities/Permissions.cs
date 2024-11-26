using System.Reflection;

namespace Sales.Models.Entities
{
	public class Permissions
	{
		public Permissions()
		{
			List<PropertyInfo> properties = GetType().GetProperties().ToList();
			foreach (PropertyInfo property in properties)
			{
				property.SetValue(this, null);
			}
			TypeList = new(){ "int", "int", "int", "bit", "bit", "bit", "bit", "bit" };
		}
		public Permissions(int? id, int? roleid, int? functionid, bool? isaccess, bool? isadd, bool? isupdate, bool? isdelete, bool? isprint)
		{
			Id = id;
			RoleId = roleid;
			FunctionId = functionid;
			IsAccess = isaccess;
			IsAdd = isadd;
			IsUpdate = isupdate;
			IsDelete = isdelete;
			IsPrint = isprint;
			TypeList = new(){ "int", "int", "int", "bit", "bit", "bit", "bit", "bit" };
		}
		public int? Id { get; set; }
		public int? RoleId { get; set; }
		public int? FunctionId { get; set; }
		public bool? IsAccess { get; set; }
		public bool? IsAdd { get; set; }
		public bool? IsUpdate { get; set; }
		public bool? IsDelete { get; set; }
		public bool? IsPrint { get; set; }
		public List<string> TypeList { get; set; }
	}
}
