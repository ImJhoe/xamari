using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
    // Models/Especialidad.cs
    public class Especialidad
    {
        public int id_especialidad { get; set; }
        public string nombre_especialidad { get; set; }
        public string descripcion { get; set; }
    }

    // Models/Sucursal.cs
    public class Sucursal
    {
        public int id_sucursal { get; set; }
        public string nombre_sucursal { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public string horario_atencion { get; set; }
        public bool estado { get; set; }
    }

    // Models/DiasSemana.cs
    public static class DiasSemana
    {
        public static Dictionary<int, string> Dias = new Dictionary<int, string>
    {
        {1, "Lunes"},
        {2, "Martes"},
        {3, "Miércoles"},
        {4, "Jueves"},
        {5, "Viernes"},
        {6, "Sábado"},
        {7, "Domingo"}
    };
    }
}
