// Models/Paciente.cs
using System;

namespace CitasMedicasApp.Models
{
    public class Paciente
    {
        // Propiedades básicas
        public int id_paciente { get; set; }
        public int id_usuario { get; set; }

        // ✅ PROPIEDADES FALTANTES QUE NECESITAS
        public DateTime? fecha_nacimiento { get; set; }  // ✅ Nullable DateTime
        public string tipo_sangre { get; set; }
        public string alergias { get; set; }
        public string antecedentes_medicos { get; set; }
        public string contacto_emergencia { get; set; }
        public string telefono_emergencia { get; set; }
        public string numero_seguro { get; set; }
        public string telefono { get; set; }

        // Información del usuario relacionado (si viene del API)
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string cedula { get; set; }
        public string correo { get; set; }

        // Propiedades calculadas
        public string NombreCompleto => $"{nombres} {apellidos}";
        public int Edad => fecha_nacimiento.HasValue
            ? DateTime.Now.Year - fecha_nacimiento.Value.Year
            : 0;
    }
}