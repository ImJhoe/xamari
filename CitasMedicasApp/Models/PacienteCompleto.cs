using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
    public class PacienteCompleto
    {
        public int id_paciente { get; set; }
        public int id_usuario { get; set; }
        public string cedula { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string correo { get; set; }
        public string telefono { get; set; }
        public DateTime fecha_nacimiento { get; set; }
        public string tipo_sangre { get; set; }
        public string alergias { get; set; }
        public string antecedentes_medicos { get; set; }
        public string contacto_emergencia { get; set; }
        public string telefono_emergencia { get; set; }
        public string numero_seguro { get; set; }
        public string nombre_completo => $"{nombres} {apellidos}";
        public int edad => DateTime.Now.Year - fecha_nacimiento.Year;
    }
}
