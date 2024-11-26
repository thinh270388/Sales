using System.Reflection;

namespace Sales.Models.Entities
{
	public class Functions
	{
		public Functions()
		{
			List<PropertyInfo> properties = GetType().GetProperties().ToList();
			foreach (PropertyInfo property in properties)
			{
				property.SetValue(this, null);
			}
			TypeList = new(){ "int", "nvarchar", "int" };
		}
		public Functions(int? id, string? name, int? functionid)
		{
			Id = id;
			Name = name;
			FunctionId = functionid;
			TypeList = new(){ "int", "nvarchar", "int" };
		}
		public int? Id { get; set; }
		public string? Name { get; set; }
		public int? FunctionId { get; set; }
		public List<string> TypeList { get; set; }
	}
}
