using System;

namespace CitasMedicasApp.Models
{
    public class Sucursal
    {
        public int id_sucursal { get; set; }
        public string nombre_sucursal { get; set; }

        // ✅ AGREGAR PROPIEDADES FALTANTES
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public string horario_atencion { get; set; }
        public bool estado { get; set; } = true;

        public DateTime fecha_creacion { get; set; }
    }
}