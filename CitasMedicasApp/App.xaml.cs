using System;
using Xamarin.Forms;
using CitasMedicasApp.Views;

namespace CitasMedicasApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Establecer el menú principal como página de inicio
            MainPage = new NavigationPage(new MenuPrincipalPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}