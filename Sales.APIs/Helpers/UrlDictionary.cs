using Sales.Models.DTOs;
using Sales.Models.Entities;

namespace Sales.APIs.Helpers
{
    public class UrlDictionary
    {
        public Dictionary<Type, string> UrlGetAll = new()
        {
            {typeof(Functions),"https://localhost:7037/sales/functions/get-all" },
            {typeof(Logs),"https://localhost:7037/sales/logs/get-all" },
            {typeof(Permissions),"https://localhost:7037/sales/permissions/get-all" },
            {typeof(Roles),"https://localhost:7037/sales/roles/get-all" },
            {typeof(Users),"https://localhost:7037/sales/users/get-all" }
        };
        public Dictionary<Type, string> UrlGetOne = new()
        {
            {typeof(Functions),"https://localhost:7037/sales/functions/get-one" },
            {typeof(Logs),"https://localhost:7037/sales/logs/get-one" },
            {typeof(Permissions),"https://localhost:7037/sales/permissions/get-one" },
            {typeof(Roles),"https://localhost:7037/sales/roles/get-one" },
            {typeof(Users),"https://localhost:7037/sales/users/get-one" }
        };
        public Dictionary<Type, string> UrlFind = new()
        {
            {typeof(Functions),"https://localhost:7037/sales/functions/find" },
            {typeof(Logs),"https://localhost:7037/sales/logs/find" },
            {typeof(Permissions),"https://localhost:7037/sales/permissions/find" },
            {typeof(Roles),"https://localhost:7037/sales/roles/find" },
            {typeof(Users),"https://localhost:7037/sales/users/find" }
        };
        public Dictionary<Type, string> UrlAdd = new()
        {
            {typeof(Functions),"https://localhost:7037/sales/functions/add" },
            {typeof(Logs),"https://localhost:7037/sales/logs/add" },
            {typeof(Permissions),"https://localhost:7037/sales/permissions/add" },
            {typeof(Roles),"https://localhost:7037/sales/roles/add" },
            {typeof(Users),"https://localhost:7037/sales/users/add" }
        };
        public Dictionary<Type, string> UrlUpdate = new()
        {
            {typeof(Functions),"https://localhost:7037/sales/functions/update" },
            {typeof(Logs),"https://localhost:7037/sales/logs/update" },
            {typeof(Permissions),"https://localhost:7037/sales/permissions/update" },
            {typeof(Roles),"https://localhost:7037/sales/roles/update" },
            {typeof(Users),"https://localhost:7037/sales/users/update" }
        };
        public Dictionary<Type, string> UrlDelete = new()
        {
            {typeof(Functions),"https://localhost:7037/sales/functions/delete" },
            {typeof(Logs),"https://localhost:7037/sales/logs/delete" },
            {typeof(Permissions),"https://localhost:7037/sales/permissions/delete" },
            {typeof(Roles),"https://localhost:7037/sales/roles/delete" },
            {typeof(Users),"https://localhost:7037/sales/users/delete" }
        };
        public Dictionary<string, string> UrAuth = new()
        {
            {"login","https://localhost:7037/sales/auths/login" },
            {"refresh","https://localhost:7037/sales/auths/refresh" },
            {"revoke-all","https://localhost:7037/sales/auths/revoke-all" },
            {"revoke","https://localhost:7037/sales/auths/revoke" }
        };
        public Dictionary<Type, string> UrlGetFull = new()
        {
            {typeof(UsersDto),"https://localhost:7037/sales/others/get-full-user" }
        };
    }
}
