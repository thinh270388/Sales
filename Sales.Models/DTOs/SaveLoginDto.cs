using Sales.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sales.Models.DTOs
{
    public class SaveLoginDto : LoginRequest
    {
        public bool IsRemember { get; set; } = false;
    }
}
