using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
    // Models/MedicoCompleto.cs
    public class MedicoCompleto
    {
        public int id_doctor { get; set; }
        public int id_usuario { get; set; }
        public string cedula { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
        public int id_especialidad { get; set; }
        public string nombre_especialidad { get; set; }
        public string numero_colegiado { get; set; }
        public bool activo { get; set; }
        public string nombre_completo => $"{nombre} {apellido}";
        public List<HorarioMedicoDetallado> horarios { get; set; } = new List<HorarioMedicoDetallado>();
        public List<Sucursal> sucursales { get; set; } = new List<Sucursal>();
    }

    // Models/HorarioMedicoDetallado.cs
    public class HorarioMedicoDetallado
    {
        public int id_horario { get; set; }
        public int id_doctor { get; set; }
        public int id_sucursal { get; set; }
        public string nombre_sucursal { get; set; }
        public int dia_semana { get; set; }
        public string nombre_dia { get; set; }
        public string hora_inicio { get; set; }
        public string hora_fin { get; set; }
        public int duracion_cita { get; set; }
        public bool activo { get; set; }
        public DateTime fecha_creacion { get; set; }
    }
}
