using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
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
