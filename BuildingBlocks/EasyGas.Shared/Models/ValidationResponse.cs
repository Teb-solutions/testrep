using System;
using System.Collections.Generic;
using System.Text;

namespace EasyGas.Shared.Models
{
    public class ApiResponse
    {
        public int? Status { get; set; }
        public string Detail { get; set; }

        public ApiResponse(string detail)
        {
            Status = 200;
            Detail = detail;
        }
    }
}
