using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Services;
using System.Threading.Tasks;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecepcionistaMenuPage : ContentPage
    {
        public RecepcionistaMenuPage()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            WelcomeLabel.Text = $"Bienvenido, {UserSessionManager.GetUserDisplayName()}";
            RoleLabel.Text = $"{UserSessionManager.GetUserRole()} - Puntos 3-7 Lista de Cotejo";
        }

        // ============ PUNTO 3: CREAR CITA ============
        private async void OnCrearCitaClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanCreateCitas)
            {
                await Navigation.PushModalAsync(new NavigationPage(new CrearCitaPage()));
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para crear citas", "OK");
            }
        }

        // ============ PUNTO 4: BUSCAR PACIENTE ============
        // ✅ CORREGIDO: Cambiado de 'async Task' a 'async void'
        private async void OnBuscarPacienteClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanSearchPatients)
            {
                await Navigation.PushModalAsync(new NavigationPage(new BuscarPacientePage()));
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para buscar pacientes", "OK");
            }
        }

        // ============ PUNTO 5: REGISTRAR PACIENTE ============
        private async void OnRegistrarPacienteClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanRegisterPatients)
            {
                await Navigation.PushModalAsync(new NavigationPage(new RegistroPacientePage()));
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para registrar pacientes", "OK");
            }
        }

        // ============ PUNTO 6: FLUJO COMPLETO ============
        private async void OnFlujoCompletoClicked(object sender, EventArgs e)
        {
            await DisplayAlert("ℹ️ Flujo Completo",
                "Este flujo se activa automáticamente:\n\n" +
                "1. Buscar paciente por cédula\n" +
                "2. Si no existe → Registrar nuevo paciente\n" +
                "3. Automáticamente regresar a crear cita\n" +
                "4. Seleccionar horario disponible\n" +
                "5. Confirmar cita",
                "Entendido");

            // Iniciar con búsqueda de paciente
            OnBuscarPacienteClicked(sender, e);
        }

        // ============ PUNTO 7: VER HORARIOS ============
        private async void OnVerHorariosClicked(object sender, EventArgs e)
        {
            if (UserSessionManager.CanViewMedicSchedules)
            {
              //  await Navigation.PushModalAsync(new NavigationPage(new RegistroPacientePage()));
                await Navigation.PushModalAsync(new  NavigationPage(new  HorariosMedicosPage()));
            }
            else
            {
                await DisplayAlert("❌ Acceso Denegado", "No tiene permisos para ver horarios médicos", "OK");
            }
        }

        // ============ VER CITAS ============
        private async void OnVerCitasClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new VerCitasPage()));
           // await Navigation.PushAsync(new VerCitasPage());
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