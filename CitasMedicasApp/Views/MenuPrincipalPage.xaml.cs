using System;
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

        // ============ NAVEGACIÓN A MÉDICOS ============
        private async void OnRegistroMedicoTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistroMedicoPage());
        }

        private async void OnConsultarMedicoTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConsultarMedicoPage());
        }

        // ============ NAVEGACIÓN A CITAS ============
        private async void OnCrearCitaTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CrearCitaPage());
        }

        private async void OnVerCitasTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new VerCitasPage());
        }

        // ============ NAVEGACIÓN A PACIENTES ============
        private async void OnRegistroPacienteTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistroPacientePage());
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
            bool answer = await DisplayAlert("Cerrar Sesión",
                                           "¿Está seguro que desea cerrar sesión?",
                                           "Sí", "No");
            if (answer)
            {
                // Limpiar datos guardados
                Application.Current.Properties.Clear();
                await Application.Current.SavePropertiesAsync();

                // Volver al login
                Application.Current.MainPage = new LoginPage();
            }
        }
    }
}