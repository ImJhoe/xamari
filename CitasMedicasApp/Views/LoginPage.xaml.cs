// Views/LoginPage.xaml.cs
using System;
using Xamarin.Forms;

namespace CitasMedicasApp.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _apiService;

        public LoginPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                ShowError("Por favor complete todos los campos");
                return;
            }

            ShowLoading(true);

            try
            {
                var response = await _apiService.LoginAsync(EmailEntry.Text.Trim(), PasswordEntry.Text);

                if (response.success && response.data != null)
                {
                    // Guardar datos del usuario logueado
                    Application.Current.Properties["UserId"] = response.data.id;
                    Application.Current.Properties["UserName"] = response.data.nombre;
                    Application.Current.Properties["UserType"] = response.data.tipo_usuario;
                    Application.Current.Properties["Token"] = response.token;
                    await Application.Current.SavePropertiesAsync();

                    // Navegar al menú principal
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    ShowError(response.message ?? "Error al iniciar sesión");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error de conexión: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
        }

        private void ShowLoading(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
            LoginButton.IsEnabled = !isLoading;
        }
    }
}