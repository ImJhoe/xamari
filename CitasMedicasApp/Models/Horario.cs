using System;

namespace CitasMedicasApp.Models
{
    // Models/HorarioMedico.cs - VERSIÓN CORREGIDA SIN DUPLICADOS
    using System;

    namespace CitasMedicasApp.Models
    {
        public class HorarioMedico
        {
            // ✅ PROPIEDADES PRINCIPALES - SOLO UNA DECLARACIÓN DE CADA UNA
            public int id_horario { get; set; }
            public int id_medico { get; set; }
            public int id_sucursal { get; set; }
            public int dia_semana { get; set; }           // ✅ SOLO UNA DECLARACIÓN
            public TimeSpan hora_inicio { get; set; }     // ✅ SOLO UNA DECLARACIÓN  
            public TimeSpan hora_fin { get; set; }        // ✅ SOLO UNA DECLARACIÓN
            public int duracion_consulta { get; set; }    // ✅ SOLO UNA DECLARACIÓN
            public string observaciones { get; set; }     // ✅ SOLO UNA DECLARACIÓN
            public bool disponible { get; set; }
            public DateTime fecha_creacion { get; set; }

            // Información adicional para consultas (sin duplicar)
            public string nombre_medico { get; set; }
            public string apellido_medico { get; set; }
            public string nombre_sucursal { get; set; }
            public string especialidad { get; set; }
            public bool activo { get; set; }

            // Propiedades calculadas útiles
            public string NombreDia => GetNombreDia(dia_semana);
            public string HorarioCompleto => $"{hora_inicio:hh\\:mm} - {hora_fin:hh\\:mm}";
            public string NombreMedicoCompleto => $"{nombre_medico} {apellido_medico}";

            // Método helper para obtener el nombre del día
            private string GetNombreDia(int dia)
            {
                return dia switch
                {
                    1 => "Lunes",
                    2 => "Martes",
                    3 => "Miércoles",
                    4 => "Jueves",
                    5 => "Viernes",
                    6 => "Sábado",
                    7 => "Domingo",
                    _ => "Desconocido"
                };
            }
        }

        // Clase adicional para horarios disponibles
        public class HorarioDisponible
        {
            public int IdHorario { get; set; }
            public int IdMedico { get; set; }
            public string NombreMedico { get; set; }
            public TimeSpan HoraInicio { get; set; }
            public TimeSpan HoraFin { get; set; }
            public DateTime Fecha { get; set; }
            public bool EstaDisponible { get; set; }
            public string HorarioTexto { get; set; }
            public string EstadoTexto { get; set; }
        }

        // Clase para el detalle completo de horarios médicos
        public class HorarioMedicoDetallado : HorarioMedico
        {
            public string cedula_medico { get; set; }
            public string telefono_medico { get; set; }
            public string email_medico { get; set; }
            public string direccion_sucursal { get; set; }
            public string telefono_sucursal { get; set; }
        }
    }
}