using System.Reflection;

namespace Sales.Models.Entities
{
	public class Roles
	{
		public Roles()
		{
			List<PropertyInfo> properties = GetType().GetProperties().ToList();
			foreach (PropertyInfo property in properties)
			{
				property.SetValue(this, null);
			}
			TypeList = new(){ "int", "nvarchar", "bit", "nvarchar" };
		}
		public Roles(int? id, string? name, bool? issystem, string? description)
		{
			Id = id;
			Name = name;
			IsSystem = issystem;
			Description = description;
			TypeList = new(){ "int", "nvarchar", "bit", "nvarchar" };
		}
		public int? Id { get; set; }
		public string? Name { get; set; }
		public bool? IsSystem { get; set; }
		public string? Description { get; set; }
		public List<string> TypeList { get; set; }
	}
}
