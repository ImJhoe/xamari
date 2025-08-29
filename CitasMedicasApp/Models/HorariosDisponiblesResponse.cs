using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
    public class HorariosDisponiblesResponse
    {
        public DateTime fecha { get; set; }
        public string nombre_medico { get; set; }
        public string especialidad { get; set; }
        public string sucursal { get; set; }
        public int duracion_cita_minutos { get; set; }
        public List<HorarioDisponible> horarios_disponibles { get; set; } = new List<HorarioDisponible>();
        public int total_slots { get; set; }
        public string mensaje { get; set; }
    }

    public class HorarioDisponible
    {
        public string hora { get; set; }
        public bool disponible { get; set; }
        public string estado { get; set; } // "disponible", "ocupado", "no_atiende"
        public string motivo_no_disponible { get; set; }
    }
}
