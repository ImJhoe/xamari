// Models/Cita.cs
using System;

public class Cita
{
    public int id { get; set; }
    public int id_cita { get; set; }
    public int id_paciente { get; set; }
    public int id_medico { get; set; }
    public int id_sucursal { get; set; }
    public int id_tipo_cita { get; set; }
    public DateTime fecha_hora { get; set; }
    public DateTime fecha_cita { get; set; }
    public string hora_inicio { get; set; }
    public string hora_fin { get; set; }
    public string estado { get; set; }
    public string motivo { get; set; }
    public string observaciones { get; set; }
    public string notas { get; set; }
    public string tipo_cita { get; set; }
    public string enlace_virtual { get; set; }
    public string sala_virtual { get; set; }
    public DateTime fecha_creacion { get; set; }

    // Propiedades adicionales para mostrar información completa
    public string nombre_paciente { get; set; }
    public string cedula_paciente { get; set; }
    public string nombre_medico { get; set; }
    public string especialidad { get; set; }
    public string nombre_especialidad { get; set; }
    public string nombre_sucursal { get; set; }
    public string nombre_tipo { get; set; }

    // Propiedades calculadas
    public string fecha_formateada => fecha_cita.ToString("dd/MM/yyyy");
    public string hora_formateada => hora_inicio ?? fecha_hora.ToString("HH:mm");
    public string estado_formateado => estado ?? "Pendiente";
}