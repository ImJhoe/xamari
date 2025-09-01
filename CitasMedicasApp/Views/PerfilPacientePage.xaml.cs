using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Models;
using CitasMedicasApp.Services;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PerfilPacientePage : ContentPage
    {
        private readonly ApiService _apiService;

        public PerfilPacientePage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            LoadPerfilData();
        }

        private async void LoadPerfilData()
        {
            ShowLoading(true);

            try
            {
                // Cargar información básica del usuario
                LoadBasicInfo();

                // Cargar información médica adicional
                await LoadMedicalInfo();

                // Cargar estadísticas de citas
                await LoadCitasStats();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando perfil: {ex.Message}");
                await DisplayAlert("❌ Error", "Error cargando información del perfil", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void LoadBasicInfo()
        {
            var usuario = UserSessionManager.CurrentUser;
            if (usuario != null)
            {
                NombreLabel.Text = $"{usuario.NombreCompleto}";
                CedulaLabel.Text = $"{usuario.cedula}";
                EmailLabel.Text = $"{usuario.email}";
                TelefonoLabel.Text = $"{usuario.telefono ?? "No registrado"}";
                RolLabel.Text = $"{usuario.rol}";
            }
        }

        private async Task LoadMedicalInfo()
        {
            try
            {
                var idPaciente = UserSessionManager.CurrentUser?.id ?? 0;
                var response = await _apiService.ObtenerPacienteAsync(idPaciente);

                if (response.success && response.data != null)
                {
                    var paciente = response.data;
                    InfoMedicaFrame.IsVisible = true;

                    FechaNacimientoLabel.Text = paciente.fecha_nacimiento?.ToString("dd/MM/yyyy") ?? "No registrada";
                    TipoSangreLabel.Text = paciente.tipo_sangre ?? "No registrado";
                    AlergiasLabel.Text = !string.IsNullOrEmpty(paciente.alergias) ? paciente.alergias : "Ninguna registrada";
                    ContactoEmergenciaLabel.Text = !string.IsNullOrEmpty(paciente.contacto_emergencia)
                        ? $"{paciente.contacto_emergencia} - {paciente.telefono_emergencia}"
                        : "No registrado";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando info médica: {ex.Message}");
                InfoMedicaFrame.IsVisible = false;
            }
        }

        private async Task LoadCitasStats()
        {
            try
            {
                var idPaciente = UserSessionManager.CurrentUser?.id ?? 0;
                var response = await _apiService.ConsultarCitasAsync(idPaciente);

                if (response.success && response.data != null)
                {
                    var citas = response.data;

                    TotalCitasLabel.Text = citas.Count.ToString();
                    CitasCompletadasLabel.Text = citas.Count(c => c.estado?.ToLower() == "completada").ToString();
                    CitasPendientesLabel.Text = citas.Count(c => c.estado?.ToLower() == "programada" || c.estado?.ToLower() == "confirmada").ToString();

                    // Última cita
                    var ultimaCita = citas
                        .Where(c => DateTime.TryParse($"{c.fecha_cita} {c.hora_cita}", out _))
                        .OrderByDescending(c => DateTime.Parse($"{c.fecha_cita} {c.hora_cita}"))
                        .FirstOrDefault();

                    if (ultimaCita != null)
                    {
                        var fechaUltima = DateTime.Parse($"{ultimaCita.fecha_cita} {ultimaCita.hora_cita}");
                        UltimaCitaLabel.Text = $"Última cita: {fechaUltima:dd/MM/yyyy} con {ultimaCita.nombre_medico}";
                    }
                }
                else
                {
                    TotalCitasLabel.Text = "0";
                    CitasCompletadasLabel.Text = "0";
                    CitasPendientesLabel.Text = "0";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando stats de citas: {ex.Message}");
                TotalCitasLabel.Text = "-";
                CitasCompletadasLabel.Text = "-";
                CitasPendientesLabel.Text = "-";
            }
        }

        private async void OnActualizarInfoClicked(object sender, EventArgs e)
        {
            await DisplayAlert("🚧 En Desarrollo", "Funcionalidad de actualización de información en desarrollo", "OK");
            // TODO: Implementar página de actualización de información
            // await Navigation.PushAsync(new ActualizarPerfilPage());
        }

        private async void OnCambiarPasswordClicked(object sender, EventArgs e)
        {
            await DisplayAlert("🚧 En Desarrollo", "Funcionalidad de cambio de contraseña en desarrollo", "OK");
            // TODO: Implementar página de cambio de contraseña
            // await Navigation.PushAsync(new CambiarPasswordPage());
        }

        private async void OnVerMisCitasClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new VerCitasPage(soloMisCitas: true));
        }

        private void ShowLoading(bool isLoading)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsVisible = isLoading;
                LoadingIndicator.IsRunning = isLoading;
            });
        }
    }
}