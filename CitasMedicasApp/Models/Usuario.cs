using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
    // Models/Usuario.cs
    public class Usuario
    {
        public int id { get; set; }
        public string cedula { get; set; }
        public string username { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
        public string especialidad { get; set; }
        public string tipo_usuario { get; set; }
        public string token { get; set; }  // ✅ AGREGAR ESTO

        // Propiedad computed para nombre completo
        public string nombre_completo => $"{nombre} {apellido}";
    }

    // Models/Paciente.cs
    public class Paciente
    {
        public int id { get; set; }
        public string cedula { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public DateTime fecha_nacimiento { get; set; }
        public string direccion { get; set; }
        public string genero { get; set; }
    }

    // Models/Cita.cs
    public class Cita
    {
        public int id { get; set; }
        public int id_paciente { get; set; }
        public int id_medico { get; set; }
        public DateTime fecha_cita { get; set; }
        public string hora_inicio { get; set; }
        public string hora_fin { get; set; }
        public string estado { get; set; }
        public string motivo { get; set; }
        public string observaciones { get; set; }

        // Propiedades adicionales para mostrar información
        public string nombre_paciente { get; set; }
        public string nombre_medico { get; set; }
        public string especialidad { get; set; }
    }

    // Models/HorarioMedico.cs
    public class HorarioMedico
    {
        public int id_horario { get; set; }
        public int id_medico { get; set; }
        public int id_doctor { get; set; } // Alias para id_medico
        public int id_sucursal { get; set; }
        public string dia_semana { get; set; } // Como string para envío
        public int dia_semana_numero { get; set; } // Como número
        public string hora_inicio { get; set; }
        public string hora_fin { get; set; }
        public int duracion_cita { get; set; } = 30; // ← AGREGAR ESTA PROPIEDAD
        public bool activo { get; set; } = true;
        public DateTime fecha_creacion { get; set; }
    }

    // Models/ApiResponse.cs
    public class ApiResponse<T>
    {
        public bool success { get; set; }
        public string message { get; set; }
        public T data { get; set; }
        public string token { get; set; }
    }
}
