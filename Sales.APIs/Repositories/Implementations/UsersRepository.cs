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
	public class UsersRepository : IGenericRepository<Users>
	{
		private readonly string _connectionString;
		private readonly IGenericRepository<Logs> _logsRepository;
		private readonly ILogger<UsersRepository> _logger;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public UsersRepository(IConnectionRepository connectionRepository, ILogger<UsersRepository> logger, IGenericRepository<Logs> logsRepository, IHttpContextAccessor httpContextAccessor)
		{
            _connectionString = connectionRepository.GetConnectionString();
            _logger = logger;
            _logsRepository = logsRepository;
            _httpContextAccessor = httpContextAccessor;
		}
		private DtoResult<Users> GetUsers(Users Users, bool ExactFind = false)
		{
			DtoResult<Users> result = new();
			string condStr = "";
			foreach (PropertyInfo prop in Users.GetType().GetProperties())
			{
				if (prop.Name == "TypeList")
					continue;
				if (prop.GetValue(Users) != null)
				{
					int index = Array.IndexOf(Users.GetType().GetProperties(), prop);
					if(!ExactFind)
						condStr += Users.TypeList[index] switch
						{
							"varchar" => $" AND {prop.Name} LIKE '%{prop.GetValue(Users)}%'",
							"nvarchar" => $" AND {prop.Name} LIKE N'%{prop.GetValue(Users)}%'",
							"bit" or "date" or "datetime" => $" AND {prop.Name} = '{prop.GetValue(Users)}'",
							_ => $" AND {prop.Name} LIKE '%{prop.GetValue(Users)}%'",
						};
					else
						condStr += Users.TypeList[index] switch
						{
							"varchar" => $" AND {prop.Name} = '{prop.GetValue(Users)}'",
							"nvarchar" => $" AND {prop.Name} = N'{prop.GetValue(Users)}'",
							"bit" or "date" or "datetime" => $" AND {prop.Name} = '{prop.GetValue(Users)}'",
							_ => $" AND {prop.Name} = {prop.GetValue(Users)}",
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
					string sql = "SELECT * FROM Users" + condStr;
					SqlDataAdapter adapter = new(sql, conn);
					DataTable dt = new();
 					adapter.Fill(dt);
					conn.Close();
					List<Users> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Users
					{
						Id = x.Field<int?>("Id"),
						FullName = x.Field<string?>("FullName"),
						UserName = x.Field<string?>("UserName"),
						Email = x.Field<string?>("Email"),
						EmailConfirmed = x.Field<bool?>("EmailConfirmed"),
						PasswordHash = x.Field<string?>("PasswordHash"),
						PhoneNumber = x.Field<string?>("PhoneNumber"),
						PhoneNumberConfirmed = x.Field<bool?>("PhoneNumberConfirmed"),
						LockoutEnd = x.Field<DateTime?>("LockoutEnd"),
						LockedEnable = x.Field<bool?>("LockedEnable"),
						AccessFailedCount = x.Field<int?>("AccessFailedCount"),
						RefreshToken = x.Field<string?>("RefreshToken"),
						RefreshTokenExpiryTime = x.Field<DateTime?>("RefreshTokenExpiryTime"),
						IsSystem = x.Field<bool?>("IsSystem"),
						RoleId = x.Field<int?>("RoleId"),
					}).ToList();

					result.Success = true;
					if (!ExactFind)
					{
						result.Message = $"{user} GetAll thành công trong bảng Users";
						result.Results = rs;
					}
					else
					{
                        result.Message = $"{user} GetOne thành công trong bảng Users";
                        result.Result = rs.FirstOrDefault()!;
					}
				}
			}
			catch (Exception ex)
			{
				result.Message = ExactFind ? $"{user} GetAll thất bại trong bảng Users vì lỗi: {ex.Message}" : $"{user} GetOne thất bại trong bảng Users vì lỗi: {ex.Message}";
                result.Success = false;
			}

			return result;
		}
		public DtoResult<Users> GetAll()
		{
			return GetUsers(new Users());
		}
		public DtoResult<Users> GetOne(Users Users)
		{
			return GetUsers(Users, true);
		}
		public DtoResult<Users> Find(Users Users)
		{
			return GetUsers(Users);
		}
		public DtoResult<Users> Add(Users Users)
		{
			DtoResult<Users>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("UsersAdd", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@FullName", SqlDbType.NVarChar).Value = Users.FullName == null ? DBNull.Value : Users.FullName;
					cmd.Parameters.AddWithValue("@UserName", SqlDbType.NVarChar).Value = Users.UserName == null ? DBNull.Value : Users.UserName;
					cmd.Parameters.AddWithValue("@Email", SqlDbType.NVarChar).Value = Users.Email;
					cmd.Parameters.AddWithValue("@EmailConfirmed", SqlDbType.Bit).Value = Users.EmailConfirmed;
					cmd.Parameters.AddWithValue("@PasswordHash", SqlDbType.NVarChar).Value = Users.PasswordHash == null ? DBNull.Value : Users.PasswordHash;
					cmd.Parameters.AddWithValue("@PhoneNumber", SqlDbType.NVarChar).Value = Users.PhoneNumber == null ? DBNull.Value : Users.PhoneNumber;
					cmd.Parameters.AddWithValue("@PhoneNumberConfirmed", SqlDbType.Bit).Value = Users.PhoneNumberConfirmed;
					cmd.Parameters.AddWithValue("@LockoutEnd", SqlDbType.DateTime).Value = Users.LockoutEnd == null ? DBNull.Value : Users.LockoutEnd;
					cmd.Parameters.AddWithValue("@LockedEnable", SqlDbType.Bit).Value = Users.LockedEnable;
					cmd.Parameters.AddWithValue("@AccessFailedCount", SqlDbType.Int).Value = Users.AccessFailedCount == null ? DBNull.Value : Users.AccessFailedCount;
					cmd.Parameters.AddWithValue("@RefreshToken", SqlDbType.NVarChar).Value = Users.RefreshToken == null ? DBNull.Value : Users.RefreshToken;
					cmd.Parameters.AddWithValue("@RefreshTokenExpiryTime", SqlDbType.DateTime).Value = Users.RefreshTokenExpiryTime == null ? DBNull.Value : Users.RefreshTokenExpiryTime;
					cmd.Parameters.AddWithValue("@IsSystem", SqlDbType.Bit).Value = Users.IsSystem;
					cmd.Parameters.AddWithValue("@RoleId", SqlDbType.Int).Value = Users.RoleId;
					conn.Open();
					SqlDataAdapter adapt = new(cmd);
					DataTable dt = new();
					adapt.Fill(dt);
					conn.Close();
					if (dt.Rows.Count > 0)
					{
						List<Users> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Users
						{
							Id = x.Field<int?>("Id"),
							FullName = x.Field<string?>("FullName"),
							UserName = x.Field<string?>("UserName"),
							Email = x.Field<string?>("Email"),
							EmailConfirmed = x.Field<bool?>("EmailConfirmed"),
							PasswordHash = x.Field<string?>("PasswordHash"),
							PhoneNumber = x.Field<string?>("PhoneNumber"),
							PhoneNumberConfirmed = x.Field<bool?>("PhoneNumberConfirmed"),
							LockoutEnd = x.Field<DateTime?>("LockoutEnd"),
							LockedEnable = x.Field<bool?>("LockedEnable"),
							AccessFailedCount = x.Field<int?>("AccessFailedCount"),
							RefreshToken = x.Field<string?>("RefreshToken"),
							RefreshTokenExpiryTime = x.Field<DateTime?>("RefreshTokenExpiryTime"),
							IsSystem = x.Field<bool?>("IsSystem"),
							RoleId = x.Field<int?>("RoleId"),
						}).ToList();

						result.Message = $"{user} đã thêm thành công {JsonConvert.SerializeObject(rs[0])} vào bảng Users";
						result.Success = true;
						result.Result = rs[0];

                        _logsRepository.Add(new Logs()
						{
							TableName = "Users",
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
						result.Message = $"{user} đã thêm thất bại {JsonConvert.SerializeObject(Users)} vào bảng Users vì lỗi xung đột";
						result.Success = false;
                        _logger.LogWarning(result.Message);
					}
				}
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã thêm thất bại {JsonConvert.SerializeObject(Users)} vào bảng Users vì lỗi: {ex.Message}";
                result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
		public DtoResult<Users> Update(Users Users)
		{
			DtoResult<Users>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				var rsUsers = GetOne(new Users() { Id = Users.Id });
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("UsersUpdate", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = Users.Id;
					cmd.Parameters.AddWithValue("@FullName", SqlDbType.NVarChar).Value = Users.FullName == null ? DBNull.Value : Users.FullName;
					cmd.Parameters.AddWithValue("@UserName", SqlDbType.NVarChar).Value = Users.UserName == null ? DBNull.Value : Users.UserName;
					cmd.Parameters.AddWithValue("@Email", SqlDbType.NVarChar).Value = Users.Email;
					cmd.Parameters.AddWithValue("@EmailConfirmed", SqlDbType.Bit).Value = Users.EmailConfirmed;
					cmd.Parameters.AddWithValue("@PasswordHash", SqlDbType.NVarChar).Value = Users.PasswordHash == null ? DBNull.Value : Users.PasswordHash;
					cmd.Parameters.AddWithValue("@PhoneNumber", SqlDbType.NVarChar).Value = Users.PhoneNumber == null ? DBNull.Value : Users.PhoneNumber;
					cmd.Parameters.AddWithValue("@PhoneNumberConfirmed", SqlDbType.Bit).Value = Users.PhoneNumberConfirmed;
					cmd.Parameters.AddWithValue("@LockoutEnd", SqlDbType.DateTime).Value = Users.LockoutEnd == null ? DBNull.Value : Users.LockoutEnd;
					cmd.Parameters.AddWithValue("@LockedEnable", SqlDbType.Bit).Value = Users.LockedEnable;
					cmd.Parameters.AddWithValue("@AccessFailedCount", SqlDbType.Int).Value = Users.AccessFailedCount == null ? DBNull.Value : Users.AccessFailedCount;
					cmd.Parameters.AddWithValue("@RefreshToken", SqlDbType.NVarChar).Value = Users.RefreshToken == null ? DBNull.Value : Users.RefreshToken;
					cmd.Parameters.AddWithValue("@RefreshTokenExpiryTime", SqlDbType.DateTime).Value = Users.RefreshTokenExpiryTime == null ? DBNull.Value : Users.RefreshTokenExpiryTime;
					cmd.Parameters.AddWithValue("@IsSystem", SqlDbType.Bit).Value = Users.IsSystem;
					cmd.Parameters.AddWithValue("@RoleId", SqlDbType.Int).Value = Users.RoleId;
					conn.Open();
					SqlDataAdapter adapt = new(cmd);
					DataTable dt = new();
					adapt.Fill(dt);
					conn.Close();
					if (dt.Rows.Count > 0)
					{
						List<Users> rs = dt.Rows.Cast<DataRow>().ToList().Select(x => new Users
						{
							Id = x.Field<int?>("Id"),
							FullName = x.Field<string?>("FullName"),
							UserName = x.Field<string?>("UserName"),
							Email = x.Field<string?>("Email"),
							EmailConfirmed = x.Field<bool?>("EmailConfirmed"),
							PasswordHash = x.Field<string?>("PasswordHash"),
							PhoneNumber = x.Field<string?>("PhoneNumber"),
							PhoneNumberConfirmed = x.Field<bool?>("PhoneNumberConfirmed"),
							LockoutEnd = x.Field<DateTime?>("LockoutEnd"),
							LockedEnable = x.Field<bool?>("LockedEnable"),
							AccessFailedCount = x.Field<int?>("AccessFailedCount"),
							RefreshToken = x.Field<string?>("RefreshToken"),
							RefreshTokenExpiryTime = x.Field<DateTime?>("RefreshTokenExpiryTime"),
							IsSystem = x.Field<bool?>("IsSystem"),
							RoleId = x.Field<int?>("RoleId"),
						}).ToList();

						result.Message = $"{user} đã sửa thành công {JsonConvert.SerializeObject(Users)} thành {JsonConvert.SerializeObject(rs[0])} trong bảng Users";
						result.Success = true;
						result.Result = rs[0];

                        _logsRepository.Add(new Logs()
                        {
                            TableName = "Users",
                            RecordId = rs[0].Id,
                            ChangeType = "Sửa",
							OldValue = JsonConvert.SerializeObject(rsUsers.Result),
                            NewValue = JsonConvert.SerializeObject(rs[0]),
                            ChangeAt = DateTime.Now,
                            ChangeBy = user
                        });
                        _logger.LogInformation(result.Message);
					}
					else
					{
						result.Message = $"{user} đã sửa thất bại {JsonConvert.SerializeObject(Users)} trong bảng Users vì không tìm thấy";
						result.Success = false;
                        _logger.LogWarning(result.Message);
                    }
				}
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã sửa thất bại {JsonConvert.SerializeObject(Users)} trong bảng Users vì lỗi: {ex.Message}";
				result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
		public DtoResult<Users> Delete(Users Users)
		{
			DtoResult<Users>? result = new();
            var claim = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var user = claim != null ? claim.Value : string.Empty;

			try
			{
				using (var conn = new SqlConnection(_connectionString))
				{
					using SqlCommand cmd = new("UsersDelete", conn) { CommandType = CommandType.StoredProcedure };
					cmd.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = Users.Id;
					conn.Open();
					int count = cmd.ExecuteNonQuery();
					conn.Close();
					if (count > 0)
					{
						result.Message = $"{user} đã xóa thành công {JsonConvert.SerializeObject(Users)} trong bảng Users";
						result.Success = true;

                        _logsRepository.Add(new Logs()
                        {
                            TableName = "Users",
                            RecordId = Users.Id,
                            ChangeType = "Xóa",
                            OldValue = JsonConvert.SerializeObject(Users),
                            ChangeAt = DateTime.Now,
                            ChangeBy = user
                        });
                        _logger.LogInformation(result.Message);
					}
                    else
                    {
                        result.Message = $"{user} đã xóa thất bại {JsonConvert.SerializeObject(Users)} trong bảng Users vì không tìm thấy";
                        result.Success = false;
                        _logger.LogWarning(result.Message);
                    }
                }
			}
			catch (Exception ex)
			{
				result.Message = $"{user} đã xóa thất bại {JsonConvert.SerializeObject(Users)} trong bảng Users vì lỗi: {ex.Message}";
				result.Success = false;
                _logger.LogError(result.Message);
			}

			return result;
		}
	}
}