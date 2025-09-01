using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
    public class MedicoCompleto
    {
        // Propiedades básicas
        public int id_doctor { get; set; }
        public int id_medico { get; set; }
        public int id_usuario { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string email { get; set; }
        public bool activo { get; set; }

        // Propiedades existentes
        public string cedula { get; set; }
        public string telefono { get; set; }
        public int id_especialidad { get; set; }
        public string nombre_especialidad { get; set; }
        public DateTime fecha_registro { get; set; }

        // ✅ PROPIEDADES CALCULADAS CORREGIDAS
        public string nombre_completo => $"{nombre} {apellido}";
        public string NombreCompleto => $"{nombre} {apellido}"; // Ambas versiones para compatibilidad
        public List<HorarioMedicoDetallado> horarios { get; set; }

        // Propiedades adicionales útiles
        public string estado_texto => activo ? "Activo" : "Inactivo";
        public string especialidad_color => GetColorEspecialidad();

        // Constructor
        public MedicoCompleto()
        {
            horarios = new List<HorarioMedicoDetallado>();
        }

        // Método helper para obtener color por especialidad
        private string GetColorEspecialidad()
        {
            return nombre_especialidad?.ToLower() switch
            {
                var esp when esp.Contains("cardio") => "#e74c3c",
                var esp when esp.Contains("pediatr") => "#f39c12",
                var esp when esp.Contains("gineco") => "#9b59b6",
                var esp when esp.Contains("neurolo") => "#3498db",
                var esp when esp.Contains("dermato") => "#1abc9c",
                _ => "#34495e"
            };
        }
    }
}