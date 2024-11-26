using System.Reflection;

namespace Sales.Models.Entities
{
	public class Logs
	{
		public Logs()
		{
			List<PropertyInfo> properties = GetType().GetProperties().ToList();
			foreach (PropertyInfo property in properties)
			{
				property.SetValue(this, null);
			}
			TypeList = new(){ "int", "nvarchar", "int", "nvarchar", "nvarchar", "nvarchar", "datetime", "nvarchar" };
		}
		public Logs(int? id, string? tablename, int? recordid, string? changetype, string? oldvalue, string? newvalue, DateTime? changeat, string? changeby)
		{
			Id = id;
			TableName = tablename;
			RecordId = recordid;
			ChangeType = changetype;
			OldValue = oldvalue;
			NewValue = newvalue;
			ChangeAt = changeat;
			ChangeBy = changeby;
			TypeList = new(){ "int", "nvarchar", "int", "nvarchar", "nvarchar", "nvarchar", "datetime", "nvarchar" };
		}
		public int? Id { get; set; }
		public string? TableName { get; set; }
		public int? RecordId { get; set; }
		public string? ChangeType { get; set; }
		public string? OldValue { get; set; }
		public string? NewValue { get; set; }
		public DateTime? ChangeAt { get; set; }
		public string? ChangeBy { get; set; }
		public List<string> TypeList { get; set; }
	}
}
