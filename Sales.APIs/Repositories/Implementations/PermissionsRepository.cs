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
	public class PermissionsRepository : IGenericRepository<Permissions>
	{
		private readonly string _connectionString;
		private readonly IGenericRepository<Logs> _logsRepository;
		private readonly ILogger<PermissionsRepository> _logger;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public PermissionsRepository(IConnectionRepository connectionRepository, ILogger<PermissionsRepository> logger, IGenericRepository<Logs> logsRepository, IHttpContextAccessor httpContextAccessor)
		{
            _connectionString = connectionRepository.GetConnectionString();
            _logger = logger;
            _logsRepository = logsRepository;
            _httpContextAccessor = httpContextAccessor;
		}
		private DtoResult<Permissions> GetPermissions(Permissions Permissions, bool ExactFind = false)
		{
			DtoResult<Permissions> result = new();
			string condStr = "";
			foreach (PropertyInfo prop in Permissions.GetType().GetProperties())
			{
				if (prop.Name == "TypeList")
					continue;
				if (prop.GetValue(Permissions) != null)
				{
					int index = Array.IndexOf(Permissions.GetType().GetProperties(), prop);
					if(!ExactFind)
						condStr += Permissions.TypeList[index] switch
						{
							"varchar" => $" AND {prop.Name} LIKE '%{prop.GetValue(Permissions)}%'",
							"nvarchar" => $" AND {prop.Name} LIKE N'%{prop.GetValue(Permissions)}%'",
							"bit" or "date" or "datetime" => $" AND {prop.Name} = '{prop.GetValue(Permissions)}'",
							_ => $" AND {prop.Name} LIKE '%{prop.GetValue(Permissions)}%'",
						};
					else
						condStr += Permissions.TypeList[index] switch
						{
							"varchar" => $" AND {prop.Name} = '{prop.GetValue(Permissions)}'",
							"nvarchar" => $" AND {prop.Name} = N'{prop.GetValue(Permissions)}'",
							"bit" or "date" or "datetime" => $" AND {prop.Name} = '{prop.GetValue(Permissions)}'",
							_ => $" AND {prop.Name} = {prop.GetValue(Permissions)}",
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
					string sql = "SELECT * FROM Permissions" + condStr;
					SqlDataAdapter adapter = new(sql, conn);
					DataTable dt = new();
 					adapter.Fill(dt);
					conn.Close();
					List<Permissions> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Permissions
					{
						Id = x.Field<int?>("Id"),
						RoleId = x.Field<int?>("RoleId"),
						FunctionId = x.Field<int?>("FunctionId"),
						IsAccess = x.Field<bool?>("IsAccess"),
						IsAdd = x.Field<bool?>("IsAdd"),
						IsUpdate = x.Field<bool?>("IsUpdate"),
						IsDelete = x.Field<bool?>("IsDelete"),
						IsPrint = x.Field<bool?>("IsPrint"),
					}).ToList();

					result.Success = true;
					if (!ExactFind)
					{
						result.Message = $"{user} GetAll thành công trong bảng Permissions";
						result.Results = rs;
					}
					else
					{
                        result.Message = $"{user} GetOne thành công trong bảng Permissions";
                        result.Result = rs.FirstOrDefault()!;
					}
				}
			}
			catch (Exception ex)
			{
				result.Message = ExactFind ? $"{user} GetAll thất bại trong bảng Permissions vì lỗi: {ex.Message}" : $"{user} GetOne thất bại trong bảng Permissions vì lỗi: {ex.Message}";
                result.Success = false;
			}

			return result;
		}
		public DtoResult<Permissions> GetAll()
		{
			return GetPermissions(new Permissions());
		}
		public DtoResult<Permissions> GetOne(Permissions Permissions)
		{
			return GetPermissions(Permissions, true);
		}
		public DtoResult<Permissions> Find(Permissions Permissions)
		{
			return GetPermissions(Permissions);
		}
		public DtoResult<Permissions> Add(Permissions Permissions)
		{
			DtoResult<Permissions>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("PermissionsAdd", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@RoleId", SqlDbType.Int).Value = Permissions.RoleId;
					cmd.Parameters.AddWithValue("@FunctionId", SqlDbType.Int).Value = Permissions.FunctionId;
					cmd.Parameters.AddWithValue("@IsAccess", SqlDbType.Bit).Value = Permissions.IsAccess;
					cmd.Parameters.AddWithValue("@IsAdd", SqlDbType.Bit).Value = Permissions.IsAdd;
					cmd.Parameters.AddWithValue("@IsUpdate", SqlDbType.Bit).Value = Permissions.IsUpdate;
					cmd.Parameters.AddWithValue("@IsDelete", SqlDbType.Bit).Value = Permissions.IsDelete;
					cmd.Parameters.AddWithValue("@IsPrint", SqlDbType.Bit).Value = Permissions.IsPrint;
					conn.Open();
					SqlDataAdapter adapt = new(cmd);
					DataTable dt = new();
					adapt.Fill(dt);
					conn.Close();
					if (dt.Rows.Count > 0)
					{
						List<Permissions> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Permissions
						{
							Id = x.Field<int?>("Id"),
							RoleId = x.Field<int?>("RoleId"),
							FunctionId = x.Field<int?>("FunctionId"),
							IsAccess = x.Field<bool?>("IsAccess"),
							IsAdd = x.Field<bool?>("IsAdd"),
							IsUpdate = x.Field<bool?>("IsUpdate"),
							IsDelete = x.Field<bool?>("IsDelete"),
							IsPrint = x.Field<bool?>("IsPrint"),
						}).ToList();

						result.Message = $"{user} đã thêm thành công {JsonConvert.SerializeObject(rs[0])} vào bảng Permissions";
						result.Success = true;
						result.Result = rs[0];

                        _logsRepository.Add(new Logs()
						{
							TableName = "Permissions",
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
						result.Message = $"{user} đã thêm thất bại {JsonConvert.SerializeObject(Permissions)} vào bảng Permissions vì lỗi xung đột";
						result.Success = false;
                        _logger.LogWarning(result.Message);
					}
				}
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã thêm thất bại {JsonConvert.SerializeObject(Permissions)} vào bảng Permissions vì lỗi: {ex.Message}";
                result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
		public DtoResult<Permissions> Update(Permissions Permissions)
		{
			DtoResult<Permissions>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				var rsPermissions = GetOne(new Permissions() { Id = Permissions.Id });
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("PermissionsUpdate", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = Permissions.Id;
					cmd.Parameters.AddWithValue("@RoleId", SqlDbType.Int).Value = Permissions.RoleId;
					cmd.Parameters.AddWithValue("@FunctionId", SqlDbType.Int).Value = Permissions.FunctionId;
					cmd.Parameters.AddWithValue("@IsAccess", SqlDbType.Bit).Value = Permissions.IsAccess;
					cmd.Parameters.AddWithValue("@IsAdd", SqlDbType.Bit).Value = Permissions.IsAdd;
					cmd.Parameters.AddWithValue("@IsUpdate", SqlDbType.Bit).Value = Permissions.IsUpdate;
					cmd.Parameters.AddWithValue("@IsDelete", SqlDbType.Bit).Value = Permissions.IsDelete;
					cmd.Parameters.AddWithValue("@IsPrint", SqlDbType.Bit).Value = Permissions.IsPrint;
					conn.Open();
					SqlDataAdapter adapt = new(cmd);
					DataTable dt = new();
					adapt.Fill(dt);
					conn.Close();
					if (dt.Rows.Count > 0)
					{
						List<Permissions> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Permissions
						{
							Id = x.Field<int?>("Id"),
							RoleId = x.Field<int?>("RoleId"),
							FunctionId = x.Field<int?>("FunctionId"),
							IsAccess = x.Field<bool?>("IsAccess"),
							IsAdd = x.Field<bool?>("IsAdd"),
							IsUpdate = x.Field<bool?>("IsUpdate"),
							IsDelete = x.Field<bool?>("IsDelete"),
							IsPrint = x.Field<bool?>("IsPrint"),
						}).ToList();

						result.Message = $"{user} đã sửa thành công {JsonConvert.SerializeObject(Permissions)} thành {JsonConvert.SerializeObject(rs[0])} trong bảng Permissions";
						result.Success = true;
						result.Result = rs[0];

                        _logsRepository.Add(new Logs()
                        {
                            TableName = "Permissions",
                            RecordId = rs[0].Id,
                            ChangeType = "Sửa",
							OldValue = JsonConvert.SerializeObject(rsPermissions.Result),
                            NewValue = JsonConvert.SerializeObject(rs[0]),
                            ChangeAt = DateTime.Now,
                            ChangeBy = user
                        });
                        _logger.LogInformation(result.Message);
					}
					else
					{
						result.Message = $"{user} đã sửa thất bại {JsonConvert.SerializeObject(Permissions)} trong bảng Permissions vì không tìm thấy";
						result.Success = false;
                        _logger.LogWarning(result.Message);
                    }
				}
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã sửa thất bại {JsonConvert.SerializeObject(Permissions)} trong bảng Permissions vì lỗi: {ex.Message}";
				result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
		public DtoResult<Permissions> Delete(Permissions Permissions)
		{
			DtoResult<Permissions>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("PermissionsDelete", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = Permissions.Id;
					conn.Open();
					int count = cmd.ExecuteNonQuery();
					conn.Close();
					if (count > 0)
					{
						result.Message = $"{user} đã xóa thành công {JsonConvert.SerializeObject(Permissions)} trong bảng Permissions";
						result.Success = true;

                        _logsRepository.Add(new Logs()
                        {
                            TableName = "Permissions",
                            RecordId = Permissions.Id,
                            ChangeType = "Xóa",
                            OldValue = JsonConvert.SerializeObject(Permissions),
                            ChangeAt = DateTime.Now,
                            ChangeBy = user
                        });
                        _logger.LogInformation(result.Message);
					}
                    else
                    {
                        result.Message = $"{user} đã xóa thất bại {JsonConvert.SerializeObject(Permissions)} trong bảng Permissions vì không tìm thấy";
                        result.Success = false;
                        _logger.LogWarning(result.Message);
                    }
                }
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã xóa thất bại {JsonConvert.SerializeObject(Permissions)} trong bảng Permissions vì lỗi: {ex.Message}";
				result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
	}
}