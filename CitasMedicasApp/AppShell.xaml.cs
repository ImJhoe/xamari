using System;
using Xamarin.Forms;

namespace CitasMedicasApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Ruta del login
            Routing.RegisterRoute("login", typeof(Views.LoginPage));

            // Rutas existentes
            Routing.RegisterRoute("registromedico", typeof(Views.RegistroMedicoPage));
            Routing.RegisterRoute("consultarmedico", typeof(Views.ConsultarMedicoPage));
            Routing.RegisterRoute("crearcita", typeof(Views.CrearCitaPage));
            Routing.RegisterRoute("vercitas", typeof(Views.VerCitasPage));
            Routing.RegisterRoute("registropaciente", typeof(Views.RegistroPacientePage));
            Routing.RegisterRoute("editarhorarios", typeof(Views.EditarHorariosPage));

            // Rutas adicionales que podrías necesitar
            Routing.RegisterRoute("adminmenu", typeof(Views.AdminMenuPage));
            Routing.RegisterRoute("medicomenu", typeof(Views.MedicoMenuPage));
            Routing.RegisterRoute("pacientemenu", typeof(Views.PacienteMenuPage));
            Routing.RegisterRoute("recepcionistamenu", typeof(Views.RecepcionistaMenuPage));
            Routing.RegisterRoute("menuprincipal", typeof(Views.MenuPrincipalPage));
            Routing.RegisterRoute("perfilpaciente", typeof(Views.PerfilPacientePage));
            Routing.RegisterRoute("proximacita", typeof(Views.ProximaCitaPage));
            Routing.RegisterRoute("configurarhorario", typeof(Views.ConfigurarHorarioPage));
            Routing.RegisterRoute("gestionhorarios", typeof(Views.GestionHorariosPage));
            Routing.RegisterRoute("horariosmedicos", typeof(Views.HorariosMedicosPage));
            Routing.RegisterRoute("detallecita", typeof(Views.DetalleCitaPage));
            Routing.RegisterRoute("buscarpaciente", typeof(Views.BuscarPacientePage));
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Cerrar Sesión",
                                           "¿Está seguro que desea cerrar sesión?",
                                           "Sí", "No");
            if (answer)
            {
                try
                {
                    // Limpiar datos guardados
                    Application.Current.Properties.Clear();
                    await Application.Current.SavePropertiesAsync();

                    // Limpiar sesión de usuario si tienes un servicio para eso
                    // UserSessionManager.ClearSession(); // Descomenta si tienes este servicio

                    // Navegar al login usando Shell navigation
                    await Shell.Current.GoToAsync("//login");
                }
                catch (Exception ex)
                {
                    // Manejo de errores
                    await DisplayAlert("Error", $"Error al cerrar sesión: {ex.Message}", "OK");
                }
            }
        }
    }
}