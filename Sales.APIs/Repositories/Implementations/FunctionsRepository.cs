using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Sales.APIs.Repositories.Constracts;
using Sales.Models;
using Sales.Models.Entities;
using System.Data;
using System.Reflection;
using System.Security.Claims;

namespace Sales.APIs.Repositories.Implementations
{
	public class FunctionsRepository : IGenericRepository<Functions>
	{
		private readonly string _connectionString;
		private readonly IGenericRepository<Logs> _logsRepository;
		private readonly ILogger<FunctionsRepository> _logger;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public FunctionsRepository(IConnectionRepository connectionRepository, ILogger<FunctionsRepository> logger, IGenericRepository<Logs> logsRepository, IHttpContextAccessor httpContextAccessor)
		{
            _connectionString = connectionRepository.GetConnectionString();
            _logger = logger;
            _logsRepository = logsRepository;
            _httpContextAccessor = httpContextAccessor;
		}
		private DtoResult<Functions> GetFunctions(Functions Functions, bool ExactFind = false)
		{
			DtoResult<Functions> result = new();
			string condStr = "";
			foreach (PropertyInfo prop in Functions.GetType().GetProperties())
			{
				if (prop.Name == "TypeList")
					continue;
				if (prop.GetValue(Functions) != null)
				{
					int index = Array.IndexOf(Functions.GetType().GetProperties(), prop);
					if(!ExactFind)
						condStr += Functions.TypeList[index] switch
						{
							"varchar" => $" AND {prop.Name} LIKE '%{prop.GetValue(Functions)}%'",
							"nvarchar" => $" AND {prop.Name} LIKE N'%{prop.GetValue(Functions)}%'",
							"bit" or "date" or "datetime" => $" AND {prop.Name} = '{prop.GetValue(Functions)}'",
							_ => $" AND {prop.Name} LIKE '%{prop.GetValue(Functions)}%'",
						};
					else
						condStr += Functions.TypeList[index] switch
						{
							"varchar" => $" AND {prop.Name} = '{prop.GetValue(Functions)}'",
							"nvarchar" => $" AND {prop.Name} = N'{prop.GetValue(Functions)}'",
							"bit" or "date" or "datetime" => $" AND {prop.Name} = '{prop.GetValue(Functions)}'",
							_ => $" AND {prop.Name} = {prop.GetValue(Functions)}",
						};
					}
			}
			if (condStr.Length > 0)
				condStr = string.Concat(" WHERE ", condStr.AsSpan(5, condStr.Length - 5));

            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string sql = "SELECT * FROM Functions" + condStr;
					SqlDataAdapter adapter = new(sql, conn);
					DataTable dt = new();
 					adapter.Fill(dt);
					conn.Close();
					List<Functions> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Functions
					{
						Id = x.Field<int?>("Id"),
						Name = x.Field<string?>("Name"),
						FunctionId = x.Field<int?>("FunctionId"),
					}).ToList();

					result.Success = true;
					if (!ExactFind)
					{
						result.Message = $"{user} GetAll thành công trong bảng Functions";
						result.Results = rs;
					}
					else
					{
                        result.Message = $"{user} GetOne thành công trong bảng Functions";
                        result.Result = rs.FirstOrDefault()!;
					}
				}
			}
			catch (Exception ex)
			{
				result.Message = ExactFind ? $"{user} GetAll thất bại trong bảng Functions vì lỗi: {ex.Message}" : $"{user} GetOne thất bại trong bảng Functions vì lỗi: {ex.Message}";
                result.Success = false;
			}

			return result;
		}
		public DtoResult<Functions> GetAll()
		{
			return GetFunctions(new Functions());
		}
		public DtoResult<Functions> GetOne(Functions Functions)
		{
			return GetFunctions(Functions, true);
		}
		public DtoResult<Functions> Find(Functions Functions)
		{
			return GetFunctions(Functions);
		}
		public DtoResult<Functions> Add(Functions Functions)
		{
			DtoResult<Functions>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("FunctionsAdd", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@Name", SqlDbType.NVarChar).Value = Functions.Name == null ? DBNull.Value : Functions.Name;
					cmd.Parameters.AddWithValue("@FunctionId", SqlDbType.Int).Value = Functions.FunctionId == null ? DBNull.Value : Functions.FunctionId;
					conn.Open();
					SqlDataAdapter adapt = new(cmd);
					DataTable dt = new();
					adapt.Fill(dt);
					conn.Close();
					if (dt.Rows.Count > 0)
					{
						List<Functions> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Functions
						{
							Id = x.Field<int?>("Id"),
							Name = x.Field<string?>("Name"),
							FunctionId = x.Field<int?>("FunctionId"),
						}).ToList();

						result.Message = $"{user} đã thêm thành công {JsonConvert.SerializeObject(rs[0])} vào bảng Functions";
						result.Success = true;
						result.Result = rs[0];

                        _logsRepository.Add(new Logs()
						{
							TableName = "Functions",
							RecordId = rs[0].Id,
							ChangeType = "Thêm",
							NewValue = JsonConvert.SerializeObject(rs[0]),
							ChangeAt = DateTime.Now,
							ChangeBy = user
                        });
                        _logger.LogInformation(result.Message);
					}
					else
					{
						result.Message = $"{user} đã thêm thất bại {JsonConvert.SerializeObject(Functions)} vào bảng Functions vì lỗi xung đột";
						result.Success = false;
                        _logger.LogWarning(result.Message);
					}
				}
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã thêm thất bại {JsonConvert.SerializeObject(Functions)} vào bảng Functions vì lỗi: {ex.Message}";
                result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
		public DtoResult<Functions> Update(Functions Functions)
		{
			DtoResult<Functions>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				var rsFunctions = GetOne(new Functions() { Id = Functions.Id });
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("FunctionsUpdate", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = Functions.Id;
					cmd.Parameters.AddWithValue("@Name", SqlDbType.NVarChar).Value = Functions.Name == null ? DBNull.Value : Functions.Name;
					cmd.Parameters.AddWithValue("@FunctionId", SqlDbType.Int).Value = Functions.FunctionId == null ? DBNull.Value : Functions.FunctionId;
					conn.Open();
					SqlDataAdapter adapt = new(cmd);
					DataTable dt = new();
					adapt.Fill(dt);
					conn.Close();
					if (dt.Rows.Count > 0)
					{
						List<Functions> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Functions
						{
							Id = x.Field<int?>("Id"),
							Name = x.Field<string?>("Name"),
							FunctionId = x.Field<int?>("FunctionId"),
						}).ToList();

						result.Message = $"{user} đã sửa thành công {JsonConvert.SerializeObject(Functions)} thành {JsonConvert.SerializeObject(rs[0])} trong bảng Functions";
						result.Success = true;
						result.Result = rs[0];

                        _logsRepository.Add(new Logs()
                        {
                            TableName = "Functions",
                            RecordId = rs[0].Id,
                            ChangeType = "Sửa",
							OldValue = JsonConvert.SerializeObject(rsFunctions.Result),
                            NewValue = JsonConvert.SerializeObject(rs[0]),
                            ChangeAt = DateTime.Now,
                            ChangeBy = user
                        });
                        _logger.LogInformation(result.Message);
					}
					else
					{
						result.Message = $"{user} đã sửa thất bại {JsonConvert.SerializeObject(Functions)} trong bảng Functions vì không tìm thấy";
						result.Success = false;
                        _logger.LogWarning(result.Message);
                    }
				}
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã sửa thất bại {JsonConvert.SerializeObject(Functions)} trong bảng Functions vì lỗi: {ex.Message}";
				result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
		public DtoResult<Functions> Delete(Functions Functions)
		{
			DtoResult<Functions>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("FunctionsDelete", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = Functions.Id;
					conn.Open();
					int count = cmd.ExecuteNonQuery();
					conn.Close();
					if (count > 0)
					{
						result.Message = $"{user} đã xóa thành công {JsonConvert.SerializeObject(Functions)} trong bảng Functions";
						result.Success = true;

                        _logsRepository.Add(new Logs()
                        {
                            TableName = "Functions",
                            RecordId = Functions.Id,
                            ChangeType = "Xóa",
                            OldValue = JsonConvert.SerializeObject(Functions),
                            ChangeAt = DateTime.Now,
                            ChangeBy = user
                        });
                        _logger.LogInformation(result.Message);
					}
                    else
                    {
                        result.Message = $"{user} đã xóa thất bại {JsonConvert.SerializeObject(Functions)} trong bảng Functions vì không tìm thấy";
                        result.Success = false;
                        _logger.LogWarning(result.Message);
                    }
                }
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã xóa thất bại {JsonConvert.SerializeObject(Functions)} trong bảng Functions vì lỗi: {ex.Message}";
				result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
	}
}