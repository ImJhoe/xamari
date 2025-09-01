using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Services;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _apiService;

        public LoginPage()
        {
            InitializeComponent();
            _apiService = new ApiService();

            // Verificar si ya hay una sesión activa
            CheckExistingSession();
        }

        private async void CheckExistingSession()
        {
            try
            {
                if (UserSessionManager.IsLoggedIn)
                {
                    await NavigateToRoleBasedPage();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error verificando sesión: {ex.Message}");
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            ShowLoading(true);

            try
            {
                var response = await _apiService.LoginAsync(EmailEntry.Text.Trim(), PasswordEntry.Text);

                if (response.success && response.data != null)
                {
                    // ✅ GUARDAR SESIÓN CON PERMISOS
                    bool sessionSaved = await UserSessionManager.LoginAsync(response.data);

                    if (sessionSaved)
                    {
                        await DisplayAlert("✅ Éxito",
                            $"Bienvenido {response.data.NombreCompleto}\nRol: {response.data.rol}",
                            "Continuar");

                        // ✅ NAVEGACIÓN BASADA EN ROLES
                        await NavigateToRoleBasedPage();
                    }
                    else
                    {
                        await DisplayAlert("❌ Error", "Error al guardar la sesión", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("❌ Error de Autenticación",
                        response.message ?? "Credenciales incorrectas",
                        "Intentar de nuevo");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en login: {ex.Message}");
                await DisplayAlert("❌ Error de Conexión",
                    "No se pudo conectar con el servidor. Verifique su conexión a internet.",
                    "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // ✅ NAVEGACIÓN BASADA EN ROLES
        private async System.Threading.Tasks.Task NavigateToRoleBasedPage()
        {
            try
            {
                if (UserSessionManager.IsAdmin)
                {
                    // Administrador: Acceso completo - ir al menú principal completo
                    Application.Current.MainPage = new AdminMenuPage();
                }
                else if (UserSessionManager.IsRecepcionista)
                {
                    // Recepcionista: Puntos 3-7 de la lista de cotejo
                    Application.Current.MainPage = new RecepcionistaMenuPage();
                }
                else if (UserSessionManager.IsMedico)
                {
                    // Médico: Ver sus citas y gestionar sus horarios
                    Application.Current.MainPage = new MedicoMenuPage();
                }
                else if (UserSessionManager.IsPaciente)
                {
                    // Paciente: Solo ver sus citas
                    Application.Current.MainPage = new PacienteMenuPage();
                }
                else
                {
                    await DisplayAlert("❌ Error", "Rol de usuario no válido", "OK");
                    await UserSessionManager.LogoutAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a página basada en rol: {ex.Message}");
                await DisplayAlert("❌ Error", "Error al cargar la página principal", "OK");
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                DisplayAlert("❌ Validación", "Por favor ingrese su usuario o email", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                DisplayAlert("❌ Validación", "Por favor ingrese su contraseña", "OK");
                return false;
            }

            if (PasswordEntry.Text.Length < 3)
            {
                DisplayAlert("❌ Validación", "La contraseña debe tener al menos 3 caracteres", "OK");
                return false;
            }

            return true;
        }

        private void ShowLoading(bool isLoading)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsVisible = isLoading;
                LoadingIndicator.IsRunning = isLoading;
                LoginButton.IsEnabled = !isLoading;
                LoginButton.Text = isLoading ? "Iniciando sesión..." : "INICIAR SESIÓN";
            });
        }

        // Opcional: Método para probar con usuarios predefinidos
        private async void OnTestUserClicked(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet(
                "Seleccionar usuario de prueba",
                "Cancelar",
                null,
                "Admin (admin)",
                "Recepcionista (marco.jurado)",
                "Médico (enrique.jimenez)",
                "Paciente (Test User)"
            );

            switch (action)
            {
                case "Admin (admin)":
                    EmailEntry.Text = "admin";
                    PasswordEntry.Text = "123456";
                    break;
                case "Recepcionista (marco.jurado)":
                    EmailEntry.Text = "marco.jurado";
                    PasswordEntry.Text = "123456";
                    break;
                case "Médico (enrique.jimenez)":
                    EmailEntry.Text = "enrique.jimenez";
                    PasswordEntry.Text = "123456";
                    break;
                case "Paciente (Test User)":
                    EmailEntry.Text = "test.patient";
                    PasswordEntry.Text = "123456";
                    break;
            }
        }
    }
}