using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
    public class TipoCita
    {
        public int id_tipo_cita { get; set; }
        public string nombre_tipo { get; set; }
        public string descripcion { get; set; }
        public bool activo { get; set; } = true;
        public DateTime fecha_creacion { get; set; }
    }
}
