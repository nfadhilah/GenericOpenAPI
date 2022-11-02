using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericOpenAPI.Models
{
    public class BaseResponse<T>
    {
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public string? Message { get; set; } = "Success";
        public T? Data { get; set; }
    }
}
