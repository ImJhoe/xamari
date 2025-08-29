using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using CitasMedicasApp.Models;

namespace CitasMedicasApp.Views
{
    [QueryProperty(nameof(CedulaInicial), "cedula")]
    public partial class RegistroPacientePage : ContentPage
    {
        private readonly ApiService _apiService;
        private string _cedulaInicial;

        public string CedulaInicial
        {
            get => _cedulaInicial;
            set
            {
                _cedulaInicial = value;
                if (!string.IsNullOrEmpty(value))
                {
                    CedulaEntry.Text = value;
                }
            }
        }

        public RegistroPacientePage()
        {
            InitializeComponent();
            _apiService = new ApiService();

            // Establecer fecha de nacimiento por defecto (adulto de 30 años)
            FechaNacimientoDatePicker.Date = DateTime.Now.AddYears(-30);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Si viene de crear cita, mostrar información del flujo
            if (!string.IsNullOrEmpty(_cedulaInicial))
            {
                FlujoInfoLabel.Text = "💡 Después de registrar al paciente, regresará automáticamente al formulario de citas para completar la programación.";
            }
        }

        private async void OnRegistrarPacienteClicked(object sender, EventArgs e)
        {
            if (!ValidarFormulario())
                return;

            ShowLoading(true);

            try
            {
                // Crear el objeto paciente
                var nuevoPaciente = new PacienteCompleto
                {
                    cedula = CedulaEntry.Text.Trim(),
                    nombres = NombresEntry.Text.Trim(),
                    apellidos = ApellidosEntry.Text.Trim(),
                    correo = EmailEntry.Text.Trim(),
                    telefono = TelefonoEntry.Text.Trim(),
                    fecha_nacimiento = FechaNacimientoDatePicker.Date,
                    tipo_sangre = (string)TipoSangrePicker.SelectedItem,
                    alergias = AlergiasEditor.Text?.Trim() ?? "Ninguna",
                    antecedentes_medicos = AntecedentesEditor.Text?.Trim() ?? "Ninguno",
                    contacto_emergencia = ContactoEmergenciaEntry.Text?.Trim() ?? "",
                    telefono_emergencia = TelefonoEmergenciaEntry.Text?.Trim() ?? "",
                    numero_seguro = NumeroSeguroEntry.Text?.Trim() ?? ""
                };

                var response = await _apiService.RegistrarPacienteAsync(nuevoPaciente);

                if (response.success)
                {
                    ShowMessage("✅ Paciente registrado exitosamente", true);
                    await DisplayAlert("Éxito",
                        $"El paciente {nuevoPaciente.nombre_completo} ha sido registrado correctamente.",
                        "OK");

                    // Punto 7: Regresar automáticamente al formulario de citas
                    if (!string.IsNullOrEmpty(_cedulaInicial))
                    {
                        await DisplayAlert("Información",
                            "Ahora será redirigido al formulario de citas para completar la programación.",
                            "Continuar");

                        // Navegar de vuelta a crear cita
                        await Shell.Current.GoToAsync("//crearcita");
                    }
                    else
                    {
                        // Si no viene del flujo de citas, limpiar formulario
                        LimpiarFormulario();
                    }
                }
                else
                {
                    ShowMessage($"❌ Error al registrar paciente: {response.message}", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Error de conexión: {ex.Message}", false);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(CedulaEntry.Text))
            {
                ShowMessage("❌ La cédula es requerida", false);
                return false;
            }

            if (CedulaEntry.Text.Trim().Length != 10)
            {
                ShowMessage("❌ La cédula debe tener 10 dígitos", false);
                return false;
            }

            if (string.IsNullOrWhiteSpace(NombresEntry.Text))
            {
                ShowMessage("❌ Los nombres son requeridos", false);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ApellidosEntry.Text))
            {
                ShowMessage("❌ Los apellidos son requeridos", false);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailEntry.Text) || !EmailEntry.Text.Contains("@"))
            {
                ShowMessage("❌ Ingrese un email válido", false);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TelefonoEntry.Text))
            {
                ShowMessage("❌ El teléfono es requerido", false);
                return false;
            }

            if (FechaNacimientoDatePicker.Date >= DateTime.Now)
            {
                ShowMessage("❌ La fecha de nacimiento debe ser anterior a hoy", false);
                return false;
            }

            if (TipoSangrePicker.SelectedItem == null)
            {
                ShowMessage("❌ Debe seleccionar el tipo de sangre", false);
                return false;
            }

            return true;
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Confirmar",
                "¿Está seguro que desea cancelar el registro? Se perderán los datos ingresados.",
                "Sí", "No");

            if (answer)
            {
                if (!string.IsNullOrEmpty(_cedulaInicial))
                {
                    // Si viene del flujo de citas, regresar a crear cita
                    await Shell.Current.GoToAsync("//crearcita");
                }
                else
                {
                    // Si no, regresar al menú principal
                    await Shell.Current.GoToAsync("//main");
                }
            }
        }

        private void LimpiarFormulario()
        {
            CedulaEntry.Text = "";
            NombresEntry.Text = "";
            ApellidosEntry.Text = "";
            EmailEntry.Text = "";
            TelefonoEntry.Text = "";
            FechaNacimientoDatePicker.Date = DateTime.Now.AddYears(-30);
            TipoSangrePicker.SelectedItem = null;
            AlergiasEditor.Text = "";
            AntecedentesEditor.Text = "";
            ContactoEmergenciaEntry.Text = "";
            TelefonoEmergenciaEntry.Text = "";
            NumeroSeguroEntry.Text = "";
            MessageLabel.IsVisible = false;
        }

        private void ShowLoading(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
            RegistrarPacienteButton.IsEnabled = !isLoading;
            CancelarButton.IsEnabled = !isLoading;
        }

        private async void ShowMessage(string message, bool isSuccess)
        {
            MessageLabel.Text = message;
            MessageLabel.TextColor = isSuccess ? Color.Green : Color.Red;
            MessageLabel.IsVisible = true;

            await Task.Delay(4000);
            MessageLabel.IsVisible = false;
        }
    }
}