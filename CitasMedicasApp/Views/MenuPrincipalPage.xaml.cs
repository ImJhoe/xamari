using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CitasMedicasApp.Views
{
    public partial class MenuPrincipalPage : ContentPage
    {
        public MenuPrincipalPage()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            if (Application.Current.Properties.ContainsKey("UserName"))
            {
                string userName = Application.Current.Properties["UserName"].ToString();
                string userType = Application.Current.Properties.ContainsKey("UserType") ?
                    Application.Current.Properties["UserType"].ToString() : "Usuario";
                WelcomeLabel.Text = $"Bienvenido {userName} ({userType})";
            }
        }

        // MÉTODO CORREGIDO PARA NAVEGACIÓN - REEMPLAZA COMPLETAMENTE LA PÁGINA
        private void NavigateToPage<T>() where T : Page, new()
        {
            try
            {
                var page = new T();
                Application.Current.MainPage = new NavigationPage(page);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Error al navegar: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Error de navegación: {ex}");
            }
        }

        // MÉTODO ALTERNATIVO PARA NAVEGACIÓN MODAL (mantiene el stack)
        private async Task NavigateToPageModalAsync<T>() where T : Page, new()
        {
            try
            {
                var page = new T();
                await Application.Current.MainPage.Navigation.PushModalAsync(new NavigationPage(page));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al navegar: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Error de navegación: {ex}");
            }
        }

        // ============ NAVEGACIÓN A MÉDICOS ============
        private void OnRegistroMedicoTapped(object sender, EventArgs e)
        {
            NavigateToPage<RegistroMedicoPage>();
        }

        private void OnConsultarMedicoTapped(object sender, EventArgs e)
        {
            NavigateToPage<ConsultarMedicoPage>();
        }

        // ============ NAVEGACIÓN A CITAS ============
        private void OnCrearCitaTapped(object sender, EventArgs e)
        {
            NavigateToPage<CrearCitaPage>();
        }

        private void OnVerCitasTapped(object sender, EventArgs e)
        {
            NavigateToPage<VerCitasPage>();
        }

        // ============ NAVEGACIÓN A PACIENTES ============
        private void OnRegistroPacienteTapped(object sender, EventArgs e)
        {
            NavigateToPage<RegistroPacientePage>();
        }

        // ============ CONFIGURACIÓN ============
        private async void OnConfiguracionTapped(object sender, EventArgs e)
        {
            await DisplayAlert("Configuración",
                "Funcionalidad en desarrollo.\n\n" +
                "Próximas características:\n" +
                "• Cambiar contraseña\n" +
                "• Configurar notificaciones\n" +
                "• Preferencias de usuario\n" +
                "• Exportar datos",
                "OK");
        }

        // ============ CERRAR SESIÓN ============
        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            try
            {
                bool answer = await DisplayAlert("Cerrar Sesión",
                                               "¿Está seguro que desea cerrar sesión?",
                                               "Sí", "No");
                if (answer)
                {
                    // Limpiar datos guardados
                    Application.Current.Properties.Clear();
                    await Application.Current.SavePropertiesAsync();

                    // USAR EL MÉTODO ESTÁTICO DE App.cs
                    App.SetMainPageSafely(new LoginPage());
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cerrar sesión: {ex.Message}", "OK");
            }
        }
    }
}