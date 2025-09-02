using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Services;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MedicoMenuPage : ContentPage
    {
        public MedicoMenuPage()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            WelcomeLabel.Text = $"Bienvenido, Dr. {UserSessionManager.GetUserDisplayName()}";
            RoleLabel.Text = $"{UserSessionManager.GetUserRole()} - Punto 2 y consultas médicas";
        }

        // ============ PUNTO 2: MI PERFIL ============
        private async void OnMiPerfilClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanConsultMedicos)
            {
                await Navigation.PushModalAsync(new NavigationPage(new ConsultarMedicoPage()));
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para consultar información médica", "OK");
            }
        }

        // ============ PUNTO 2: MIS HORARIOS ============
        private async void OnMisHorariosClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanManageSchedules)
            {
                await Navigation.PushModalAsync(new NavigationPage(new GestionHorariosPage()));
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para gestionar horarios", "OK");
            }
        }

        // ============ MIS CITAS ============
        private async void OnMisCitasHoyClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanViewMyCitas)
            {
                await Navigation.PushModalAsync(new NavigationPage(new VerCitasPage(filtrarPorMedico: true, soloHoy: true)));
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para ver citas", "OK");
            }
        }

        private async void OnMisCitasSemanaClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanViewMyCitas)
            {
                await Navigation.PushModalAsync(new NavigationPage(new VerCitasPage(filtrarPorMedico: true, soloHoy: false)));
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para ver citas", "OK");
            }
        }

        private async void OnHistorialPacientesClicked(object sender, EventArgs e)
        {
            await DisplayAlert("🚧 En Desarrollo", "Historial de pacientes en desarrollo", "OK");
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