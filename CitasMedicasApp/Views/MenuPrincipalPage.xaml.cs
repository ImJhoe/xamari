using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPrincipalPage : ContentPage
    {
        public MenuPrincipalPage()
        {
            InitializeComponent();
        }

        private async void OnVerCitasClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new VerCitasPage());
        }

        private async void OnCrearCitaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CrearCitaPage());
        }

        private async void OnConsultarMedicoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConsultarMedicoPage());
        }

        private async void OnEditarHorariosClicked(object sender, EventArgs e)
        {
            // Necesitarás pasar el ID del médico y nombre
            // Por ahora uso valores de ejemplo
            await Navigation.PushAsync(new EditarHorariosPage(1, "Dr. Ejemplo"));
        }

        private async void OnRegistroMedicoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistroMedicoPage());
        }

        private async void OnRegistroPacienteClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistroPacientePage());
        }
    }
}