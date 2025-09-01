// Models/Cita.cs
using System;

namespace CitasMedicasApp.Models
{
    public class Cita
    {
        public int id_cita { get; set; }
        public int id_paciente { get; set; }
        public int id_medico { get; set; }
        public int id_doctor { get; set; } // Para compatibilidad con base de datos
        public int id_sucursal { get; set; }
        public DateTime fecha_hora { get; set; }
        public DateTime fecha_cita { get; set; }
        public TimeSpan hora_inicio { get; set; } // ✅ AGREGAR ESTA PROPIEDAD
        public string hora_cita { get; set; }
        public string motivo { get; set; }
        public string tipo_cita { get; set; }
        public string estado { get; set; }
        public string cedula_paciente { get; set; }
        public DateTime fecha_creacion { get; set; }

        // ===== INFORMACIÓN ADICIONAL PARA CONSULTAS =====
        public string nombre_paciente { get; set; }
        public string nombre_medico { get; set; }
        public string nombre_especialidad { get; set; }
        public string especialidad { get; set; }
        public string nombre_sucursal { get; set; }
        public string direccion_sucursal { get; set; }
        public string nombre_tipo { get; set; }

        // Propiedades calculadas
        public DateTime FechaHoraCompleta => fecha_cita != default(DateTime)
            ? fecha_cita.Date.Add(TimeSpan.Parse(hora_cita ?? "00:00"))
            : fecha_hora;
    }
}