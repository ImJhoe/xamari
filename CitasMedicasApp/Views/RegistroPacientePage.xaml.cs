using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using CitasMedicasApp.Models;
using System.Linq;

namespace CitasMedicasApp.Views
{
    [QueryProperty(nameof(CedulaInicial), "cedula")]
    public partial class RegistroPacientePage : ContentPage
    {
        private readonly ApiService _apiService;
        private string _cedulaInicial;
        private bool _vieneDelFlujoCitas; // Nueva propiedad para controlar el flujo

        public string CedulaInicial
        {
            get => _cedulaInicial;
            set
            {
                _cedulaInicial = value;
                if (!string.IsNullOrEmpty(value) && CedulaEntry != null)
                {
                    CedulaEntry.Text = value;
                }
            }
        }

        // Constructor original (sin parámetros)
        public RegistroPacientePage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _vieneDelFlujoCitas = false;
            ConfigurarPagina();
        }

        // CONSTRUCTOR CORREGIDO (con 2 parámetros) - Soluciona el error CS1729
        public RegistroPacientePage(string cedula, bool vieneDelFlujoCitas)
        {
            InitializeComponent();
            _apiService = new ApiService();
            _cedulaInicial = cedula;
            _vieneDelFlujoCitas = vieneDelFlujoCitas;
            ConfigurarPagina();

            // Establecer la cédula en el Entry después de InitializeComponent
            if (!string.IsNullOrEmpty(cedula))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    CedulaEntry.Text = cedula;
                });
            }
        }

        private void ConfigurarPagina()
        {
            // Establecer fecha de nacimiento por defecto (adulto de 30 años)
            Device.BeginInvokeOnMainThread(() =>
            {
                if (FechaNacimientoDatePicker != null)
                {
                    FechaNacimientoDatePicker.Date = DateTime.Now.AddYears(-30);
                }
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Configurar mensaje según el contexto
            Device.BeginInvokeOnMainThread(() =>
            {
                if (_vieneDelFlujoCitas)
                {
                    FlujoInfoLabel.Text = "💡 PUNTO 5: Después de registrar al paciente, regresará automáticamente al formulario de citas para completar la programación.";
                    FlujoInfoLabel.TextColor = Color.FromHex("#155724");
                }
                else if (!string.IsNullOrEmpty(_cedulaInicial))
                {
                    FlujoInfoLabel.Text = "💡 Después de registrar al paciente, regresará automáticamente al formulario de citas para completar la programación.";
                }
            });
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

                    if (_vieneDelFlujoCitas || !string.IsNullOrEmpty(_cedulaInicial))
                    {
                        // PUNTO 6: Retorno automático al formulario de cita
                        await DisplayAlert("✅ Éxito",
                            $"Paciente {nuevoPaciente.NombreCompleto} registrado correctamente.\n\nAhora regresará al formulario de citas para completar la programación.",
                            "Continuar");

                        // Navegar a CrearCitaPage con la cédula del paciente recién creado
                        await Navigation.PushAsync(new CrearCitaPage(nuevoPaciente.cedula, true));

                        // Limpiar el stack de navegación para evitar loops
                        var existingPages = Navigation.NavigationStack.ToList();
                        for (int i = existingPages.Count - 2; i >= 0; i--)
                        {
                            if (existingPages[i] is BuscarPacientePage ||
                                existingPages[i] is RegistroPacientePage)
                            {
                                Navigation.RemovePage(existingPages[i]);
                            }
                        }
                    }
                    else
                    {
                        // Flujo normal - registro independiente
                        await DisplayAlert("Éxito",
                            $"El paciente {nuevoPaciente.NombreCompleto} ha sido registrado correctamente.",
                            "OK");

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
                System.Diagnostics.Debug.WriteLine($"Error registrando paciente: {ex.Message}");
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
                if (_vieneDelFlujoCitas || !string.IsNullOrEmpty(_cedulaInicial))
                {
                    // Si viene del flujo de citas, regresar a buscar paciente
                    await Navigation.PopAsync();
                }
                else
                {
                    // Si no, regresar al menú principal
                    await Navigation.PopAsync();
                }
            }
        }

        private void LimpiarFormulario()
        {
            Device.BeginInvokeOnMainThread(() =>
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
            });
        }

        private void ShowLoading(bool isLoading)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsVisible = isLoading;
                LoadingIndicator.IsRunning = isLoading;
                RegistrarPacienteButton.IsEnabled = !isLoading;
                CancelarButton.IsEnabled = !isLoading;
                RegistrarPacienteButton.Text = isLoading ? "Registrando..." : "✅ REGISTRAR PACIENTE";
            });
        }

        private async void ShowMessage(string message, bool isSuccess)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                MessageLabel.Text = message;
                MessageLabel.TextColor = isSuccess ? Color.Green : Color.Red;
                MessageLabel.IsVisible = true;
            });

            await Task.Delay(4000);

            Device.BeginInvokeOnMainThread(() =>
            {
                MessageLabel.IsVisible = false;
            });
        }
    }
}