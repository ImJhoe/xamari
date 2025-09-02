// En CitasMedicasApp/Helpers/NavigationHelper.cs
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CitasMedicasApp.Helpers
{
    public static class NavigationHelper
    {
        public static async Task NavigateToPageAsync<T>(INavigation navigation) where T : Page, new()
        {
            try
            {
                var page = new T();
                await navigation.PushModalAsync(new NavigationPage(page));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error de navegación: {ex}");
                // Fallback usando MainPage
                try
                {
                    Application.Current.MainPage = new NavigationPage(new T());
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en fallback: {ex2}");
                }
            }
        }

        public static async Task NavigateToPageAsync(INavigation navigation, Page page)
        {
            try
            {
                await navigation.PushModalAsync(new NavigationPage(page));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error de navegación: {ex}");
                // Fallback usando MainPage
                try
                {
                    Application.Current.MainPage = new NavigationPage(page);
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en fallback: {ex2}");
                }
            }
        }

    }
}