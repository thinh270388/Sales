namespace Sales.Models
{
	public class DtoResult<T>
	{
		public T? Result { get; set; }
		public List<T>? Results { get; set; }
		public string? Message { get; set; }
		public bool Success { get; set; }
	}
}