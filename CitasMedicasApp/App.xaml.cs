using Xamarin.Forms;
using CitasMedicasApp.Views;

namespace CitasMedicasApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            SetMainPage();
        }

        private void SetMainPage()
        {
            try
            {
                // Verificar si el usuario ya está logueado
                if (Application.Current.Properties.ContainsKey("UserId") &&
                    Application.Current.Properties.ContainsKey("UserName"))
                {
                    // Usuario ya logueado, ir directo al menú principal
                    MainPage = CreateNavigationPage(new MenuPrincipalPage());
                }
                else
                {
                    // Usuario no logueado, mostrar login
                    MainPage = CreateNavigationPage(new LoginPage());
                }
            }
            catch (System.Exception ex)
            {
                // Fallback en caso de error
                MainPage = CreateNavigationPage(new LoginPage());
                System.Diagnostics.Debug.WriteLine($"Error en SetMainPage: {ex}");
            }
        }

        // Método helper para crear NavigationPage con estilo consistente
        private NavigationPage CreateNavigationPage(Page page)
        {
            return new NavigationPage(page)
            {
                BarBackgroundColor = Color.FromHex("#3498db"),
                BarTextColor = Color.White
            };
        }

        // MÉTODO PÚBLICO ESTÁTICO CORREGIDO
        public static void SetMainPageSafely(Page page)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    // Crear NavigationPage directamente sin acceder a instancia
                    Current.MainPage = new NavigationPage(page)
                    {
                        BarBackgroundColor = Color.FromHex("#3498db"),
                        BarTextColor = Color.White
                    };
                });
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en SetMainPageSafely: {ex}");
                // Fallback más básico sin Device.BeginInvokeOnMainThread
                try
                {
                    Current.MainPage = new NavigationPage(page);
                }
                catch (System.Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en fallback: {ex2}");
                }
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