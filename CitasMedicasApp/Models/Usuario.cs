using System;
using System.Collections.Generic;
using System.Text;

namespace CitasMedicasApp.Models
{
    // Models/Usuario.cs
    public class Usuario
    {
        public int id { get; set; }
        public string cedula { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
        public string especialidad { get; set; }
        public string tipo_usuario { get; set; }
        public string username { get; set; }
        public string token { get; set; }
        
        // ===== PROPIEDADES PARA ROLES =====
        public int rol_id { get; set; }
        public string rol { get; set; }
        public UserPermissions permissions { get; set; }

        // Propiedades calculadas para facilitar el uso
        public string NombreCompleto => $"{nombre} {apellido}";
        public bool EsAdministrador => rol_id == 1;
        public bool EsRecepcionista => rol_id == 72;
        public bool EsMedico => rol_id == 70;
        public bool EsPaciente => rol_id == 71;
    }
    // ===== CLASE PARA MANEJAR PERMISOS =====
    public class UserPermissions
    {
        public string role { get; set; }
        public int role_id { get; set; }
        public Dictionary<string, bool> permissions { get; set; } = new Dictionary<string, bool>();

        // Métodos helper para verificar permisos específicos
        public bool PuedeRegistrarMedicos => GetPermission("registro_medico");
        public bool PuedeConsultarMedicos => GetPermission("consulta_medicos");
        public bool PuedeGestionarHorarios => GetPermission("gestion_horarios");
        public bool PuedeCrearCitas => GetPermission("crear_citas");
        public bool PuedeBuscarPacientes => GetPermission("buscar_pacientes");
        public bool PuedeRegistrarPacientes => GetPermission("registrar_pacientes");
        public bool PuedeVerHorariosMedicos => GetPermission("ver_horarios_medicos");
        public bool PuedeVerTodasCitas => GetPermission("ver_todas_citas");
        public bool PuedeVerMisCitas => GetPermission("ver_mis_citas");
        public bool TieneAccesoCompleto => GetPermission("todas_vistas");

        private bool GetPermission(string permissionKey)
        {
            return permissions?.ContainsKey(permissionKey) == true && permissions[permissionKey];
        }
    }
}
