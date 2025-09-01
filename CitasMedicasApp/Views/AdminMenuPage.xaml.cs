using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Services;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdminMenuPage : ContentPage
    {
        public AdminMenuPage()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            WelcomeLabel.Text = $"Bienvenido, {UserSessionManager.GetUserDisplayName()}";
            RoleLabel.Text = $"{UserSessionManager.GetUserRole()} - Acceso completo al sistema";
        }

        // ============ GESTIÓN DE MÉDICOS ============
        private async void OnRegistrarMedicoClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanRegisterMedicos)
            {
                await Navigation.PushAsync(new RegistroMedicoPage());
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para registrar médicos", "OK");
            }
        }

        private async void OnConsultarMedicosClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanConsultMedicos)
            {
                await Navigation.PushAsync(new ConsultarMedicoPage());
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para consultar médicos", "OK");
            }
        }

        // ============ GESTIÓN DE CITAS ============
        private async void OnCrearCitaClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanCreateCitas)
            {
                await Navigation.PushAsync(new CrearCitaPage());
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para crear citas", "OK");
            }
        }

        private async void OnVerTodasCitasClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanViewAllCitas)
            {
                await Navigation.PushAsync(new VerCitasPage());
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para ver todas las citas", "OK");
            }
        }

        // ============ GESTIÓN DE PACIENTES ============
        private async void OnRegistrarPacienteClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanRegisterPatients)
            {
                await Navigation.PushAsync(new RegistroPacientePage());
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para registrar pacientes", "OK");
            }
        }

        private async void OnConsultarPacientesClicked(object sender, EventArgs e)
        {
            await DisplayAlert("🚧 En Desarrollo", "Funcionalidad de consulta de pacientes en desarrollo", "OK");
        }

        // ============ CONFIGURACIÓN ============
        private async void OnGestionEspecialidadesClicked(object sender, EventArgs e)
        {
            await DisplayAlert("🚧 En Desarrollo", "Gestión de especialidades en desarrollo", "OK");
        }

        private async void OnGestionSucursalesClicked(object sender, EventArgs e)
        {
            await DisplayAlert("🚧 En Desarrollo", "Gestión de sucursales en desarrollo", "OK");
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