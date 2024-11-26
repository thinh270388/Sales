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
	public class RolesRepository : IGenericRepository<Roles>
	{
		private readonly string _connectionString;
		private readonly IGenericRepository<Logs> _logsRepository;
		private readonly ILogger<RolesRepository> _logger;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public RolesRepository(IConnectionRepository connectionRepository, ILogger<RolesRepository> logger, IGenericRepository<Logs> logsRepository, IHttpContextAccessor httpContextAccessor)
		{
            _connectionString = connectionRepository.GetConnectionString();
            _logger = logger;
            _logsRepository = logsRepository;
            _httpContextAccessor = httpContextAccessor;
		}
		private DtoResult<Roles> GetRoles(Roles Roles, bool ExactFind = false)
		{
			DtoResult<Roles> result = new();
			string condStr = "";
			foreach (PropertyInfo prop in Roles.GetType().GetProperties())
			{
				if (prop.Name == "TypeList")
					continue;
				if (prop.GetValue(Roles) != null)
				{
					int index = Array.IndexOf(Roles.GetType().GetProperties(), prop);
					if(!ExactFind)
						condStr += Roles.TypeList[index] switch
						{
							"varchar" => $" AND {prop.Name} LIKE '%{prop.GetValue(Roles)}%'",
							"nvarchar" => $" AND {prop.Name} LIKE N'%{prop.GetValue(Roles)}%'",
							"bit" or "date" or "datetime" => $" AND {prop.Name} = '{prop.GetValue(Roles)}'",
							_ => $" AND {prop.Name} LIKE '%{prop.GetValue(Roles)}%'",
						};
					else
						condStr += Roles.TypeList[index] switch
						{
							"varchar" => $" AND {prop.Name} = '{prop.GetValue(Roles)}'",
							"nvarchar" => $" AND {prop.Name} = N'{prop.GetValue(Roles)}'",
							"bit" or "date" or "datetime" => $" AND {prop.Name} = '{prop.GetValue(Roles)}'",
							_ => $" AND {prop.Name} = {prop.GetValue(Roles)}",
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
					string sql = "SELECT * FROM Roles" + condStr;
					SqlDataAdapter adapter = new(sql, conn);
					DataTable dt = new();
 					adapter.Fill(dt);
					conn.Close();
					List<Roles> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Roles
					{
						Id = x.Field<int?>("Id"),
						Name = x.Field<string?>("Name"),
						IsSystem = x.Field<bool?>("IsSystem"),
						Description = x.Field<string?>("Description"),
					}).ToList();

					result.Success = true;
					if (!ExactFind)
					{
						result.Message = $"{user} GetAll thành công trong bảng Roles";
						result.Results = rs;
					}
					else
					{
                        result.Message = $"{user} GetOne thành công trong bảng Roles";
                        result.Result = rs.FirstOrDefault()!;
					}
				}
			}
			catch (Exception ex)
			{
				result.Message = ExactFind ? $"{user} GetAll thất bại trong bảng Roles vì lỗi: {ex.Message}" : $"{user} GetOne thất bại trong bảng Roles vì lỗi: {ex.Message}";
                result.Success = false;
			}

			return result;
		}
		public DtoResult<Roles> GetAll()
		{
			return GetRoles(new Roles());
		}
		public DtoResult<Roles> GetOne(Roles Roles)
		{
			return GetRoles(Roles, true);
		}
		public DtoResult<Roles> Find(Roles Roles)
		{
			return GetRoles(Roles);
		}
		public DtoResult<Roles> Add(Roles Roles)
		{
			DtoResult<Roles>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("RolesAdd", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@Name", SqlDbType.NVarChar).Value = Roles.Name;
					cmd.Parameters.AddWithValue("@IsSystem", SqlDbType.Bit).Value = Roles.IsSystem == null ? DBNull.Value : Roles.IsSystem;
					cmd.Parameters.AddWithValue("@Description", SqlDbType.NVarChar).Value = Roles.Description == null ? DBNull.Value : Roles.Description;
					conn.Open();
					SqlDataAdapter adapt = new(cmd);
					DataTable dt = new();
					adapt.Fill(dt);
					conn.Close();
					if (dt.Rows.Count > 0)
					{
						List<Roles> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Roles
						{
							Id = x.Field<int?>("Id"),
							Name = x.Field<string?>("Name"),
							IsSystem = x.Field<bool?>("IsSystem"),
							Description = x.Field<string?>("Description"),
						}).ToList();

						result.Message = $"{user} đã thêm thành công {JsonConvert.SerializeObject(rs[0])} vào bảng Roles";
						result.Success = true;
						result.Result = rs[0];

                        _logsRepository.Add(new Logs()
						{
							TableName = "Roles",
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
						result.Message = $"{user} đã thêm thất bại {JsonConvert.SerializeObject(Roles)} vào bảng Roles vì lỗi xung đột";
						result.Success = false;
                        _logger.LogWarning(result.Message);
					}
				}
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã thêm thất bại {JsonConvert.SerializeObject(Roles)} vào bảng Roles vì lỗi: {ex.Message}";
                result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
		public DtoResult<Roles> Update(Roles Roles)
		{
			DtoResult<Roles>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				var rsRoles = GetOne(new Roles() { Id = Roles.Id });
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("RolesUpdate", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = Roles.Id;
					cmd.Parameters.AddWithValue("@Name", SqlDbType.NVarChar).Value = Roles.Name;
					cmd.Parameters.AddWithValue("@IsSystem", SqlDbType.Bit).Value = Roles.IsSystem == null ? DBNull.Value : Roles.IsSystem;
					cmd.Parameters.AddWithValue("@Description", SqlDbType.NVarChar).Value = Roles.Description == null ? DBNull.Value : Roles.Description;
					conn.Open();
					SqlDataAdapter adapt = new(cmd);
					DataTable dt = new();
					adapt.Fill(dt);
					conn.Close();
					if (dt.Rows.Count > 0)
					{
						List<Roles> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Roles
						{
							Id = x.Field<int?>("Id"),
							Name = x.Field<string?>("Name"),
							IsSystem = x.Field<bool?>("IsSystem"),
							Description = x.Field<string?>("Description"),
						}).ToList();

						result.Message = $"{user} đã sửa thành công {JsonConvert.SerializeObject(Roles)} thành {JsonConvert.SerializeObject(rs[0])} trong bảng Roles";
						result.Success = true;
						result.Result = rs[0];

                        _logsRepository.Add(new Logs()
                        {
                            TableName = "Roles",
                            RecordId = rs[0].Id,
                            ChangeType = "Sửa",
							OldValue = JsonConvert.SerializeObject(rsRoles.Result),
                            NewValue = JsonConvert.SerializeObject(rs[0]),
                            ChangeAt = DateTime.Now,
                            ChangeBy = user
                        });
                        _logger.LogInformation(result.Message);
					}
					else
					{
						result.Message = $"{user} đã sửa thất bại {JsonConvert.SerializeObject(Roles)} trong bảng Roles vì không tìm thấy";
						result.Success = false;
                        _logger.LogWarning(result.Message);
                    }
				}
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã sửa thất bại {JsonConvert.SerializeObject(Roles)} trong bảng Roles vì lỗi: {ex.Message}";
				result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
		public DtoResult<Roles> Delete(Roles Roles)
		{
			DtoResult<Roles>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("RolesDelete", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = Roles.Id;
					conn.Open();
					int count = cmd.ExecuteNonQuery();
					conn.Close();
					if (count > 0)
					{
						result.Message = $"{user} đã xóa thành công {JsonConvert.SerializeObject(Roles)} trong bảng Roles";
						result.Success = true;

                        _logsRepository.Add(new Logs()
                        {
                            TableName = "Roles",
                            RecordId = Roles.Id,
                            ChangeType = "Xóa",
                            OldValue = JsonConvert.SerializeObject(Roles),
                            ChangeAt = DateTime.Now,
                            ChangeBy = user
                        });
                        _logger.LogInformation(result.Message);
					}
                    else
                    {
                        result.Message = $"{user} đã xóa thất bại {JsonConvert.SerializeObject(Roles)} trong bảng Roles vì không tìm thấy";
                        result.Success = false;
                        _logger.LogWarning(result.Message);
                    }
                }
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã xóa thất bại {JsonConvert.SerializeObject(Roles)} trong bảng Roles vì lỗi: {ex.Message}";
				result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
	}
}