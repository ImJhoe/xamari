// Models/DiasSemana.cs
using System.Collections.Generic;

namespace CitasMedicasApp.Models
{
    public static class DiasSemana
    {
        public static Dictionary<int, string> Dias = new Dictionary<int, string>
        {
            { 1, "Lunes" },
            { 2, "Martes" },
            { 3, "Miércoles" },
            { 4, "Jueves" },
            { 5, "Viernes" },
            { 6, "Sábado" },
            { 7, "Domingo" }
        };
    }
}