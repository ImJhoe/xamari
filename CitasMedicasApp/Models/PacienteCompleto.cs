using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
    public class PacienteCompleto
    {
        public int id_paciente { get; set; }
        public int id_usuario { get; set; }
        public DateTime? fecha_nacimiento { get; set; }
        public string tipo_sangre { get; set; }
        public string alergias { get; set; }
        public string antecedentes_medicos { get; set; }
        public string contacto_emergencia { get; set; }
        public string telefono_emergencia { get; set; }
        public string numero_seguro { get; set; }
        public string telefono { get; set; }

        // Información del usuario relacionado
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string cedula { get; set; }
        public string correo { get; set; }

        public string NombreCompleto => $"{nombres} {apellidos}";
        public int Edad => fecha_nacimiento.HasValue
            ? DateTime.Now.Year - fecha_nacimiento.Value.Year
            : 0;
    }
}
