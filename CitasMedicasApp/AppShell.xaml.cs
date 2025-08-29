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
            // Rutas existentes...
            Routing.RegisterRoute("registromedico", typeof(Views.RegistroMedicoPage));
            Routing.RegisterRoute("consultarmedico", typeof(Views.ConsultarMedicoPage));
            Routing.RegisterRoute("crearcita", typeof(Views.CrearCitaPage));
            Routing.RegisterRoute("vercitas", typeof(Views.VerCitasPage));
            Routing.RegisterRoute("registropaciente", typeof(Views.RegistroPacientePage));

            // Nueva ruta para editar horarios
            Routing.RegisterRoute("editarhorarios", typeof(Views.EditarHorariosPage));
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
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
                Application.Current.MainPage = new Views.LoginPage();
            }
        }
    }
}