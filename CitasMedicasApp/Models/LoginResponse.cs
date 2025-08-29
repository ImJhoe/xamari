using Newtonsoft.Json;

namespace CitasMedicasApp.Models
{
    public class LoginResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        public LoginData data { get; set; }
    }

    public class LoginData
    {
        public UserData user { get; set; }
        public string token { get; set; }
        public int expires_in { get; set; }
    }

    public class UserData
    {
        public int id { get; set; }
        public string username { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string rol { get; set; }
        public string correo { get; set; }
    }
}