using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
    // Models/MedicoCompleto.cs
    public class MedicoCompleto
    {
        public int id_medico { get; set; }
        public int id_usuario { get; set; }
        public string cedula { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string correo { get; set; }
        public string telefono { get; set; }
        public int id_especialidad { get; set; }
        public string nombre_especialidad { get; set; }
        public string titulo_profesional { get; set; }
        public DateTime fecha_registro { get; set; }
        public int estado { get; set; }

        public string NombreCompleto => $"{nombres} {apellidos}";
    }
}
