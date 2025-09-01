using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Models;
using CitasMedicasApp.Services;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BuscarPacientePage : ContentPage
    {
        private readonly ApiService _apiService;
        private Usuario _pacienteEncontrado;

        public BuscarPacientePage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void OnBuscarClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CedulaEntry.Text))
            {
                await DisplayAlert("❌ Validación", "Por favor ingrese el número de cédula", "OK");
                return;
            }

            if (CedulaEntry.Text.Length < 10)
            {
                await DisplayAlert("❌ Validación", "La cédula debe tener 10 dígitos", "OK");
                return;
            }

            ShowLoading(true);

            try
            {
                // Buscar paciente por cédula
                var response = await _apiService.BuscarPacientePorCedulaAsync(CedulaEntry.Text.Trim());

                if (response.success && response.data != null)
                {
                    // ✅ PACIENTE ENCONTRADO - PUNTO 4A
                    MostrarPacienteEncontrado(response.data);
                }
                else
                {
                    // ❌ PACIENTE NO ENCONTRADO - PUNTO 4B
                    MostrarPacienteNoEncontrado();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error buscando paciente: {ex.Message}");
                await DisplayAlert("❌ Error", "Error al buscar el paciente", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // ✅ PUNTO 4A: Si paciente existe, muestra sus datos
        private void MostrarPacienteEncontrado(Usuario paciente)
        {
            _pacienteEncontrado = paciente;

            ResultadoFrame.IsVisible = true;
            ResultadoTitulo.Text = "✅ PACIENTE ENCONTRADO";
            ResultadoTitulo.TextColor = Color.FromHex("#27ae60");

            DatosPacienteLayout.IsVisible = true;
            AgregarPacienteButton.IsVisible = false;

            NombrePacienteLabel.Text = $"👤 Nombre: {paciente.NombreCompleto}";
            CedulaPacienteLabel.Text = $"🆔 Cédula: {paciente.cedula}";
            EmailPacienteLabel.Text = $"📧 Email: {paciente.email}";
            TelefonoPacienteLabel.Text = $"📞 Teléfono: {paciente.telefono ?? "No registrado"}";
        }

        // ❌ PUNTO 4B: Si paciente no existe, aparece botón "Añadir paciente"
        private void MostrarPacienteNoEncontrado()
        {
            _pacienteEncontrado = null;

            ResultadoFrame.IsVisible = true;
            ResultadoTitulo.Text = "❌ PACIENTE NO ENCONTRADO";
            ResultadoTitulo.TextColor = Color.FromHex("#e74c3c");

            DatosPacienteLayout.IsVisible = false;
            AgregarPacienteButton.IsVisible = true;
        }

        // Crear cita para paciente existente
        private async void OnCrearCitaClicked(object sender, EventArgs e)
        {
            if (_pacienteEncontrado != null)
            {
                await Navigation.PushAsync(new CrearCitaPage(_pacienteEncontrado));
            }
        }

        // ✅ PUNTO 5: Ir a registrar nuevo paciente
        private async void OnAgregarPacienteClicked(object sender, EventArgs e)
        {
            var cedulaIngresada = CedulaEntry.Text?.Trim();

            // Pasar la cédula ingresada al formulario de registro
            await Navigation.PushAsync(new RegistroPacientePage(cedulaIngresada, true)); // true = viene del flujo de citas
        }

        private void ShowLoading(bool isLoading)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsVisible = isLoading;
                LoadingIndicator.IsRunning = isLoading;
                BuscarButton.IsEnabled = !isLoading;
                BuscarButton.Text = isLoading ? "Buscando..." : "🔍 BUSCAR PACIENTE";
            });
        }
    }
}