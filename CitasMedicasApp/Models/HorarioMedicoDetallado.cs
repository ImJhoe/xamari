using System;

namespace CitasMedicasApp.Models
{
    public class HorarioMedicoDetallado
    {
        public int id_horario { get; set; }
        public int id_doctor { get; set; }
        public int id_sucursal { get; set; }
        public string nombre_sucursal { get; set; }
        public int dia_semana { get; set; }
        public string nombre_dia { get; set; }
        public TimeSpan hora_inicio { get; set; }  // ✅ Cambiado a TimeSpan
        public TimeSpan hora_fin { get; set; }     // ✅ Cambiado a TimeSpan  
        public int duracion_cita { get; set; }
        public bool activo { get; set; }
        public DateTime fecha_creacion { get; set; }

        // Propiedades calculadas
        public string horario_completo => $"{hora_inicio:hh\\:mm} - {hora_fin:hh\\:mm}";
    }
}