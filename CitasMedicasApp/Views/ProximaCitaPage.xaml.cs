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
    public partial class ProximaCitaPage : ContentPage
    {
        private readonly ApiService _apiService;
        private Cita _proximaCita;

        public ProximaCitaPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            LoadProximaCita();
        }

        private async void LoadProximaCita()
        {
            ShowLoading(true);

            try
            {
                var idPaciente = UserSessionManager.CurrentUser?.id ?? 0;
                var response = await _apiService.ConsultarCitasAsync(idPaciente);

                if (response.success && response.data != null && response.data.Any())
                {
                    // Buscar la próxima cita (la más cercana en el futuro)
                    var ahora = DateTime.Now;
                    _proximaCita = response.data
                        .Where(c => DateTime.Parse($"{c.fecha_cita} {c.hora_cita}") > ahora)
                        .OrderBy(c => DateTime.Parse($"{c.fecha_cita} {c.hora_cita}"))
                        .FirstOrDefault();

                    if (_proximaCita != null)
                    {
                        MostrarProximaCita(_proximaCita);
                    }
                    else
                    {
                        MostrarSinProximaCita();
                    }
                }
                else
                {
                    MostrarSinProximaCita();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando próxima cita: {ex.Message}");
                await DisplayAlert("❌ Error", "Error cargando la información de su próxima cita", "OK");
                MostrarSinProximaCita();
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void MostrarProximaCita(Cita cita)
        {
            CitaFrame.IsVisible = true;
            SinCitaFrame.IsVisible = false;

            var fechaCita = DateTime.Parse($"{cita.fecha_cita} {cita.hora_cita}");
            var tiempoRestante = fechaCita - DateTime.Now;

            CitaTituloLabel.Text = "🎯 PRÓXIMA CITA CONFIRMADA";

            // Información del médico
            MedicoLabel.Text = cita.nombre_medico ?? "Médico no especificado";
            EspecialidadLabel.Text = $"Especialidad: {cita.especialidad ?? "No especificada"}";

            // Fecha y hora
            FechaLabel.Text = $"📅 {fechaCita:dddd, dd MMMM yyyy}";
            HoraLabel.Text = $"🕒 {fechaCita:HH:mm}";

            if (tiempoRestante.TotalDays > 1)
            {
                TiempoRestanteLabel.Text = $"⏳ Faltan {(int)tiempoRestante.TotalDays} días";
            }
            else if (tiempoRestante.TotalHours > 1)
            {
                TiempoRestanteLabel.Text = $"⏳ Faltan {(int)tiempoRestante.TotalHours} horas";
            }
            else if (tiempoRestante.TotalMinutes > 0)
            {
                TiempoRestanteLabel.Text = $"⏳ Faltan {(int)tiempoRestante.TotalMinutes} minutos";
            }
            else
            {
                TiempoRestanteLabel.Text = "🚨 ¡Es ahora!";
                TiempoRestanteLabel.TextColor = Color.FromHex("#e74c3c");
            }

            // Ubicación
            SucursalLabel.Text = cita.nombre_sucursal ?? "Sucursal no especificada";
            DireccionLabel.Text = cita.direccion_sucursal ?? "Dirección no disponible";

            // Estado
            ConfigurarEstado(cita.estado ?? "Programada");

            // Mostrar botones si la cita se puede modificar
            var puedeModificar = tiempoRestante.TotalHours > 2; // Solo si faltan más de 2 horas
            ReagendarButton.IsVisible = puedeModificar;
            CancelarButton.IsVisible = puedeModificar;
        }

        private void ConfigurarEstado(string estado)
        {
            EstadoLabel.Text = $"📊 {estado}";

            switch (estado.ToLower())
            {
                case "programada":
                case "confirmada":
                    EstadoFrame.BackgroundColor = Color.FromHex("#d5f4e6");
                    EstadoLabel.TextColor = Color.FromHex("#27ae60");
                    break;
                case "cancelada":
                    EstadoFrame.BackgroundColor = Color.FromHex("#fadbd8");
                    EstadoLabel.TextColor = Color.FromHex("#e74c3c");
                    break;
                case "completada":
                    EstadoFrame.BackgroundColor = Color.FromHex("#d6eaf8");
                    EstadoLabel.TextColor = Color.FromHex("#3498db");
                    break;
                default:
                    EstadoFrame.BackgroundColor = Color.FromHex("#fef9e7");
                    EstadoLabel.TextColor = Color.FromHex("#f39c12");
                    break;
            }
        }

        private void MostrarSinProximaCita()
        {
            CitaFrame.IsVisible = false;
            SinCitaFrame.IsVisible = true;
        }

        private async void OnReagendarClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert(
                "Reagendar Cita",
                "¿Está seguro que desea reagendar esta cita?\n\nSe le dirigirá al proceso de selección de nueva fecha y hora.",
                "Sí, reagendar", "Cancelar");

            if (confirmar)
            {
                await DisplayAlert("🚧 En Desarrollo", "La funcionalidad de reagendar está en desarrollo", "OK");
                // TODO: Implementar reagendar
                // await Navigation.PushAsync(new ReagendarCitaPage(_proximaCita));
            }
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert(
                "Cancelar Cita",
                "¿Está seguro que desea cancelar esta cita?\n\n⚠️ Esta acción no se puede deshacer.",
                "Sí, cancelar", "No");

            if (confirmar)
            {
                ShowLoading(true);

                try
                {
                    var response = await _apiService.CancelarCitaAsync(_proximaCita.id_cita);

                    if (response.success)
                    {
                        await DisplayAlert("✅ Éxito", "Su cita ha sido cancelada exitosamente", "OK");
                        LoadProximaCita(); // Recargar datos
                    }
                    else
                    {
                        await DisplayAlert("❌ Error", response.message ?? "No se pudo cancelar la cita", "OK");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error cancelando cita: {ex.Message}");
                    await DisplayAlert("❌ Error", "Error al cancelar la cita", "OK");
                }
                finally
                {
                    ShowLoading(false);
                }
            }
        }

        private async void OnVerTodasCitasClicked(object sender, EventArgs e)
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