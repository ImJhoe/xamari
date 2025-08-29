using Xamarin.Forms;
using CitasMedicasApp.Views;

namespace CitasMedicasApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Verificar si el usuario ya está logueado
            if (Application.Current.Properties.ContainsKey("UserId") &&
                Application.Current.Properties.ContainsKey("UserName"))
            {
                // Usuario ya logueado, ir directo al menú principal
                MainPage = new NavigationPage(new MenuPrincipalPage())
                {
                    BarBackgroundColor = Color.FromHex("#3498db"),
                    BarTextColor = Color.White
                };
            }
            else
            {
                // Usuario no logueado, mostrar login
                MainPage = new LoginPage();
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}