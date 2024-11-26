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
	public class LogsRepository : IGenericRepository<Logs>
	{
		private readonly string _connectionString;
		private readonly ILogger<LogsRepository> _logger;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public LogsRepository(IConnectionRepository connectionRepository, ILogger<LogsRepository> logger, IHttpContextAccessor httpContextAccessor)
		{
            _connectionString = connectionRepository.GetConnectionString();
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
		}
		private DtoResult<Logs> GetLogs(Logs Logs, bool ExactFind = false)
		{
			DtoResult<Logs> result = new();
			string condStr = "";
			foreach (PropertyInfo prop in Logs.GetType().GetProperties())
			{
				if (prop.Name == "TypeList")
					continue;
				if (prop.GetValue(Logs) != null)
				{
					int index = Array.IndexOf(Logs.GetType().GetProperties(), prop);
					if(!ExactFind)
						condStr += Logs.TypeList[index] switch
						{
							"varchar" => $" AND {prop.Name} LIKE '%{prop.GetValue(Logs)}%'",
							"nvarchar" => $" AND {prop.Name} LIKE N'%{prop.GetValue(Logs)}%'",
							"bit" or "date" or "datetime" => $" AND {prop.Name} = '{prop.GetValue(Logs)}'",
							_ => $" AND {prop.Name} LIKE '%{prop.GetValue(Logs)}%'",
						};
					else
						condStr += Logs.TypeList[index] switch
						{
							"varchar" => $" AND {prop.Name} = '{prop.GetValue(Logs)}'",
							"nvarchar" => $" AND {prop.Name} = N'{prop.GetValue(Logs)}'",
							"bit" or "date" or "datetime" => $" AND {prop.Name} = '{prop.GetValue(Logs)}'",
							_ => $" AND {prop.Name} = {prop.GetValue(Logs)}",
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
					string sql = "SELECT * FROM Logs" + condStr;
					SqlDataAdapter adapter = new(sql, conn);
					DataTable dt = new();
 					adapter.Fill(dt);
					conn.Close();
					List<Logs> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Logs
					{
						Id = x.Field<int?>("Id"),
						TableName = x.Field<string?>("TableName"),
						RecordId = x.Field<int?>("RecordId"),
						ChangeType = x.Field<string?>("ChangeType"),
						OldValue = x.Field<string?>("OldValue"),
						NewValue = x.Field<string?>("NewValue"),
						ChangeAt = x.Field<DateTime?>("ChangeAt"),
						ChangeBy = x.Field<string?>("ChangeBy"),
					}).ToList();

					result.Success = true;
					if (!ExactFind)
					{
						result.Message = $"{user} GetAll thành công trong bảng Logs";
						result.Results = rs;
					}
					else
					{
                        result.Message = $"{user} GetOne thành công trong bảng Logs";
                        result.Result = rs.FirstOrDefault()!;
					}
				}
			}
			catch (Exception ex)
			{
				result.Message = ExactFind ? $"{user} GetAll thất bại trong bảng Logs vì lỗi: {ex.Message}" : $"{user} GetOne thất bại trong bảng Logs vì lỗi: {ex.Message}";
                result.Success = false;
			}

			return result;
		}
		public DtoResult<Logs> GetAll()
		{
			return GetLogs(new Logs());
		}
		public DtoResult<Logs> GetOne(Logs Logs)
		{
			return GetLogs(Logs, true);
		}
		public DtoResult<Logs> Find(Logs Logs)
		{
			return GetLogs(Logs);
		}
		public DtoResult<Logs> Add(Logs Logs)
		{
			DtoResult<Logs>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("LogsAdd", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@TableName", SqlDbType.NVarChar).Value = Logs.TableName == null ? DBNull.Value : Logs.TableName;
					cmd.Parameters.AddWithValue("@RecordId", SqlDbType.Int).Value = Logs.RecordId == null ? DBNull.Value : Logs.RecordId;
					cmd.Parameters.AddWithValue("@ChangeType", SqlDbType.NVarChar).Value = Logs.ChangeType == null ? DBNull.Value : Logs.ChangeType;
					cmd.Parameters.AddWithValue("@OldValue", SqlDbType.NVarChar).Value = Logs.OldValue == null ? DBNull.Value : Logs.OldValue;
					cmd.Parameters.AddWithValue("@NewValue", SqlDbType.NVarChar).Value = Logs.NewValue == null ? DBNull.Value : Logs.NewValue;
					cmd.Parameters.AddWithValue("@ChangeAt", SqlDbType.DateTime).Value = Logs.ChangeAt == null ? DBNull.Value : Logs.ChangeAt;
					cmd.Parameters.AddWithValue("@ChangeBy", SqlDbType.NVarChar).Value = Logs.ChangeBy == null ? DBNull.Value : Logs.ChangeBy;
					conn.Open();
					SqlDataAdapter adapt = new(cmd);
					DataTable dt = new();
					adapt.Fill(dt);
					conn.Close();
					if (dt.Rows.Count > 0)
					{
						List<Logs> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Logs
						{
							Id = x.Field<int?>("Id"),
							TableName = x.Field<string?>("TableName"),
							RecordId = x.Field<int?>("RecordId"),
							ChangeType = x.Field<string?>("ChangeType"),
							OldValue = x.Field<string?>("OldValue"),
							NewValue = x.Field<string?>("NewValue"),
							ChangeAt = x.Field<DateTime?>("ChangeAt"),
							ChangeBy = x.Field<string?>("ChangeBy"),
						}).ToList();

						result.Message = $"{user} đã thêm thành công {JsonConvert.SerializeObject(rs[0])} vào bảng Logs";
						result.Success = true;
						result.Result = rs[0];

                        _logger.LogInformation(result.Message);
					}
					else
					{
						result.Message = $"{user} đã thêm thất bại {JsonConvert.SerializeObject(Logs)} vào bảng Logs vì lỗi xung đột";
						result.Success = false;
                        _logger.LogWarning(result.Message);
					}
				}
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã thêm thất bại {JsonConvert.SerializeObject(Logs)} vào bảng Logs vì lỗi: {ex.Message}";
                result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
		public DtoResult<Logs> Update(Logs Logs)
		{
			DtoResult<Logs>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				var rsLogs = GetOne(new Logs() { Id = Logs.Id });
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("LogsUpdate", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = Logs.Id;
					cmd.Parameters.AddWithValue("@TableName", SqlDbType.NVarChar).Value = Logs.TableName == null ? DBNull.Value : Logs.TableName;
					cmd.Parameters.AddWithValue("@RecordId", SqlDbType.Int).Value = Logs.RecordId == null ? DBNull.Value : Logs.RecordId;
					cmd.Parameters.AddWithValue("@ChangeType", SqlDbType.NVarChar).Value = Logs.ChangeType == null ? DBNull.Value : Logs.ChangeType;
					cmd.Parameters.AddWithValue("@OldValue", SqlDbType.NVarChar).Value = Logs.OldValue == null ? DBNull.Value : Logs.OldValue;
					cmd.Parameters.AddWithValue("@NewValue", SqlDbType.NVarChar).Value = Logs.NewValue == null ? DBNull.Value : Logs.NewValue;
					cmd.Parameters.AddWithValue("@ChangeAt", SqlDbType.DateTime).Value = Logs.ChangeAt == null ? DBNull.Value : Logs.ChangeAt;
					cmd.Parameters.AddWithValue("@ChangeBy", SqlDbType.NVarChar).Value = Logs.ChangeBy == null ? DBNull.Value : Logs.ChangeBy;
					conn.Open();
					SqlDataAdapter adapt = new(cmd);
					DataTable dt = new();
					adapt.Fill(dt);
					conn.Close();
					if (dt.Rows.Count > 0)
					{
						List<Logs> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Logs
						{
							Id = x.Field<int?>("Id"),
							TableName = x.Field<string?>("TableName"),
							RecordId = x.Field<int?>("RecordId"),
							ChangeType = x.Field<string?>("ChangeType"),
							OldValue = x.Field<string?>("OldValue"),
							NewValue = x.Field<string?>("NewValue"),
							ChangeAt = x.Field<DateTime?>("ChangeAt"),
							ChangeBy = x.Field<string?>("ChangeBy"),
						}).ToList();

						result.Message = $"{user} đã sửa thành công {JsonConvert.SerializeObject(Logs)} thành {JsonConvert.SerializeObject(rs[0])} trong bảng Logs";
						result.Success = true;
						result.Result = rs[0];

                        _logger.LogInformation(result.Message);
					}
					else
					{
						result.Message = $"{user} đã sửa thất bại {JsonConvert.SerializeObject(Logs)} trong bảng Logs vì không tìm thấy";
						result.Success = false;
                        _logger.LogWarning(result.Message);
                    }
				}
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã sửa thất bại {JsonConvert.SerializeObject(Logs)} trong bảng Logs vì lỗi: {ex.Message}";
				result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
		public DtoResult<Logs> Delete(Logs Logs)
		{
			DtoResult<Logs>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("LogsDelete", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = Logs.Id;
					conn.Open();
					int count = cmd.ExecuteNonQuery();
					conn.Close();
					if (count > 0)
					{
						result.Message = $"{user} đã xóa thành công {JsonConvert.SerializeObject(Logs)} trong bảng Logs";
						result.Success = true;

                        _logger.LogInformation(result.Message);
					}
                    else
                    {
                        result.Message = $"{user} đã xóa thất bại {JsonConvert.SerializeObject(Logs)} trong bảng Logs vì không tìm thấy";
                        result.Success = false;
                        _logger.LogWarning(result.Message);
                    }
                }
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã xóa thất bại {JsonConvert.SerializeObject(Logs)} trong bảng Logs vì lỗi: {ex.Message}";
				result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
	}
}