using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
    // Models/ApiResponse.cs
    public class ApiResponse<T>
    {
        public bool success { get; set; }
        public string message { get; set; }
        public T data { get; set; }
        public string token { get; set; }
    }
}
