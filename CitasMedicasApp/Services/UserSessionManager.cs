using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using CitasMedicasApp.Models;
using Newtonsoft.Json;

namespace CitasMedicasApp.Services
{
    public static class UserSessionManager
    {
        private const string USER_KEY = "current_user";
        private const string PERMISSIONS_KEY = "user_permissions";
        private const string TOKEN_KEY = "auth_token";

        private static Usuario _currentUser;
        private static UserPermissions _currentPermissions;

        // ============ PROPIEDADES ============
        public static Usuario CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    LoadUserFromStorage();
                }
                return _currentUser;
            }
            private set
            {
                _currentUser = value;
            }
        }

        public static UserPermissions CurrentPermissions
        {
            get
            {
                if (_currentPermissions == null)
                {
                    LoadPermissionsFromStorage();
                }
                return _currentPermissions;
            }
            private set
            {
                _currentPermissions = value;
            }
        }

        public static bool IsLoggedIn => CurrentUser != null && !string.IsNullOrEmpty(CurrentUser.token);

        // ============ MÉTODOS DE AUTENTICACIÓN ============
        public static async Task<bool> LoginAsync(Usuario user)
        {
            try
            {
                CurrentUser = user;
                CurrentPermissions = user.permissions;

                // Guardar en almacenamiento persistente
                await SaveUserToStorageAsync(user);
                await SavePermissionsToStorageAsync(user.permissions);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en LoginAsync: {ex.Message}");
                return false;
            }
        }

        public static async Task LogoutAsync()
        {
            try
            {
                CurrentUser = null;
                CurrentPermissions = null;

                // Limpiar almacenamiento
                if (Application.Current.Properties.ContainsKey(USER_KEY))
                    Application.Current.Properties.Remove(USER_KEY);

                if (Application.Current.Properties.ContainsKey(PERMISSIONS_KEY))
                    Application.Current.Properties.Remove(PERMISSIONS_KEY);

                if (Application.Current.Properties.ContainsKey(TOKEN_KEY))
                    Application.Current.Properties.Remove(TOKEN_KEY);

                await Application.Current.SavePropertiesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en LogoutAsync: {ex.Message}");
            }
        }

        // ============ VERIFICACIÓN DE PERMISOS ============
        public static bool HasPermission(string permission)
        {
            return CurrentPermissions?.permissions?.ContainsKey(permission) == true &&
                   CurrentPermissions.permissions[permission];
        }

        // Métodos de conveniencia para verificar roles
        public static bool IsAdmin => CurrentUser?.EsAdministrador == true;
        public static bool IsRecepcionista => CurrentUser?.EsRecepcionista == true;
        public static bool IsMedico => CurrentUser?.EsMedico == true;
        public static bool IsPaciente => CurrentUser?.EsPaciente == true;

        // Métodos de conveniencia para permisos específicos
        public static bool CanRegisterMedicos => HasPermission("registro_medico");
        public static bool CanConsultMedicos => HasPermission("consulta_medicos");
        public static bool CanManageSchedules => HasPermission("gestion_horarios");
        public static bool CanCreateCitas => HasPermission("crear_citas");
        public static bool CanSearchPatients => HasPermission("buscar_pacientes");
        public static bool CanRegisterPatients => HasPermission("registrar_pacientes");
        public static bool CanViewMedicSchedules => HasPermission("ver_horarios_medicos");
        public static bool CanViewAllCitas => HasPermission("ver_todas_citas");
        public static bool CanViewMyCitas => HasPermission("ver_mis_citas");
        public static bool HasFullAccess => HasPermission("todas_vistas");

        // ============ ALMACENAMIENTO PERSISTENTE ============
        private static async Task SaveUserToStorageAsync(Usuario user)
        {
            try
            {
                var userJson = JsonConvert.SerializeObject(user);
                Application.Current.Properties[USER_KEY] = userJson;
                Application.Current.Properties[TOKEN_KEY] = user.token;
                await Application.Current.SavePropertiesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando usuario: {ex.Message}");
            }
        }

        private static async Task SavePermissionsToStorageAsync(UserPermissions permissions)
        {
            try
            {
                var permissionsJson = JsonConvert.SerializeObject(permissions);
                Application.Current.Properties[PERMISSIONS_KEY] = permissionsJson;
                await Application.Current.SavePropertiesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando permisos: {ex.Message}");
            }
        }

        private static void LoadUserFromStorage()
        {
            try
            {
                if (Application.Current.Properties.ContainsKey(USER_KEY))
                {
                    var userJson = Application.Current.Properties[USER_KEY] as string;
                    if (!string.IsNullOrEmpty(userJson))
                    {
                        _currentUser = JsonConvert.DeserializeObject<Usuario>(userJson);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando usuario: {ex.Message}");
                _currentUser = null;
            }
        }

        private static void LoadPermissionsFromStorage()
        {
            try
            {
                if (Application.Current.Properties.ContainsKey(PERMISSIONS_KEY))
                {
                    var permissionsJson = Application.Current.Properties[PERMISSIONS_KEY] as string;
                    if (!string.IsNullOrEmpty(permissionsJson))
                    {
                        _currentPermissions = JsonConvert.DeserializeObject<UserPermissions>(permissionsJson);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando permisos: {ex.Message}");
                _currentPermissions = null;
            }
        }

        // ============ UTILIDADES ============
        public static string GetCurrentUserToken()
        {
            return CurrentUser?.token ?? (Application.Current.Properties.ContainsKey(TOKEN_KEY)
                ? Application.Current.Properties[TOKEN_KEY] as string
                : null);
        }

        public static string GetUserDisplayName()
        {
            return CurrentUser?.NombreCompleto ?? "Usuario";
        }

        public static string GetUserRole()
        {
            return CurrentUser?.rol ?? "Sin rol";
        }
    }
}