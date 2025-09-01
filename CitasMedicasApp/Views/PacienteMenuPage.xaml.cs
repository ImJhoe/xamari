using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Services;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PacienteMenuPage : ContentPage
    {
        public PacienteMenuPage()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            WelcomeLabel.Text = $"Bienvenido, {UserSessionManager.GetUserDisplayName()}";
            RoleLabel.Text = $"{UserSessionManager.GetUserRole()} - Solo consulta de citas";
        }

        // ============ MIS CITAS ============
        private async void OnMisCitasClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanViewMyCitas)
            {
                await Navigation.PushAsync(new VerCitasPage(soloMisCitas: true));
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para ver citas", "OK");
            }
        }

        private async void OnMiProximaCitaClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanViewMyCitas)
            {
                await Navigation.PushAsync(new ProximaCitaPage());
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para ver citas", "OK");
            }
        }

        // ============ MI INFORMACIÓN ============
        private async void OnMiPerfilClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PerfilPacientePage());
        }

        private async void OnMiHistorialClicked(object sender, EventArgs e)
        {
            await DisplayAlert("🚧 En Desarrollo", "Historial médico en desarrollo", "OK");
        }

        // ============ INFORMACIÓN DE LA CLÍNICA ============
        private async void OnUbicacionClicked(object sender, EventArgs e)
        {
            await DisplayAlert("📍 Ubicación",
                "Clínica Médica\n\n" +
                "📍 Dirección: Av. Principal 123\n" +
                "📞 Teléfono: (593) 2-234-5678\n" +
                "📧 Email: info@clinicamedica.ec\n" +
                "🕒 Horario: Lunes a Viernes 8:00 - 18:00",
                "OK");
        }

        private async void OnEspecialidadesClicked(object sender, EventArgs e)
        {
            await DisplayAlert("🏥 Especialidades",
                "Especialidades disponibles:\n\n" +
                "• Cardiología\n" +
                "• Medicina General\n" +
                "• Pediatría\n" +
                "• Ginecología\n" +
                "• Traumatología\n" +
                "• Dermatología\n" +
                "• Oftalmología",
                "OK");
        }

        // ============ CERRAR SESIÓN ============
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Cerrar Sesión",
                                           "¿Está seguro que desea cerrar sesión?",
                                           "Sí", "No");
            if (answer)
            {
                await UserSessionManager.LogoutAsync();
                Application.Current.MainPage = new LoginPage();
            }
        }
    }
}