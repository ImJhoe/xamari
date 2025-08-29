using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
    public class CitaCreacion
    {
        public int id_paciente { get; set; }
        public int id_doctor { get; set; }
        public int id_medico { get; set; }
        public int id_sucursal { get; set; }
        public int id_tipo_cita { get; set; }
        public DateTime fecha_hora { get; set; }
        public string motivo { get; set; }
        public string tipo_cita { get; set; } // presencial, virtual
        public string notas { get; set; }
        public string observaciones { get; set; }
        public string enlace_virtual { get; set; }
        public string sala_virtual { get; set; }
    }
}
