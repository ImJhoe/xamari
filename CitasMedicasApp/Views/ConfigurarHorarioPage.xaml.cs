using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Models;
using CitasMedicasApp.Services;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfigurarHorarioPage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly HorarioMedicoDetalle _horarioExistente;
        private List<Sucursal> _sucursales;
        private bool _isEditing = false;

        // Constructor para nuevo horario
        public ConfigurarHorarioPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            InitializeData();
        }

        // Constructor para editar horario existente
        public ConfigurarHorarioPage(HorarioMedicoDetalle horario)
        {
            InitializeComponent();
            _apiService = new ApiService();
            _horarioExistente = horario;
            _isEditing = true;

            TituloLabel.Text = "EDITAR HORARIO";
            EliminarButton.IsVisible = true;
            GuardarButton.Text = "💾 ACTUALIZAR HORARIO";

            InitializeData();
        }

        private async void InitializeData()
        {
            ShowLoading(true);

            try
            {
                // Configurar días de la semana
                DiaPicker.ItemsSource = new List<string>
                {
                    "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"
                };

                // Configurar duraciones de consulta
                DuracionPicker.ItemsSource = new List<string>
                {
                    "15 minutos", "20 minutos", "30 minutos", "45 minutos", "60 minutos"
                };
                DuracionPicker.SelectedItem = "30 minutos"; // Por defecto

                // Cargar sucursales
                await LoadSucursales();

                // Si es edición, cargar datos existentes
                if (_isEditing && _horarioExistente != null)
                {
                    LoadExistingData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando datos: {ex.Message}");
                await DisplayAlert("❌ Error", "Error cargando datos iniciales", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task LoadSucursales()
        {
            try
            {
                var response = await _apiService.ObtenerSucursalesAsync();
                if (response.success && response.data != null)
                {
                    _sucursales = response.data;
                    SucursalPicker.ItemsSource = _sucursales;
                    SucursalPicker.ItemDisplayBinding = new Binding("nombre_sucursal");

                    // Seleccionar la primera por defecto
                    if (_sucursales.Count > 0)
                    {
                        SucursalPicker.SelectedItem = _sucursales[0];
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando sucursales: {ex.Message}");
            }
        }

        private void LoadExistingData()
        {
            // Seleccionar día
            var dias = new[] { "", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
            if (_horarioExistente.DiaSemana >= 1 && _horarioExistente.DiaSemana <= 7)
            {
                DiaPicker.SelectedItem = dias[_horarioExistente.DiaSemana];
            }

            // Seleccionar horas
            HoraInicioPicker.Time = _horarioExistente.HoraInicio;
            HoraFinPicker.Time = _horarioExistente.HoraFin;

            // Seleccionar sucursal
            if (_sucursales != null)
            {
                var sucursal = _sucursales.Find(s => s.nombre_sucursal == _horarioExistente.SucursalNombre);
                if (sucursal != null)
                {
                    SucursalPicker.SelectedItem = sucursal;
                }
            }
        }

        private void OnActualizarPrevClicked(object sender, EventArgs e)
        {
            if (ValidateForm(showAlert: false))
            {
                ActualizarPrevisualizacion();
            }
        }

        private void ActualizarPrevisualizacion()
        {
            try
            {
                var diaSeleccionado = DiaPicker.SelectedItem?.ToString();
                var sucursalSeleccionada = (Sucursal)SucursalPicker.SelectedItem;
                var duracionSeleccionada = DuracionPicker.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(diaSeleccionado) || sucursalSeleccionada == null)
                    return;

                var horaInicio = HoraInicioPicker.Time;
                var horaFin = HoraFinPicker.Time;
                var duracionMinutos = ExtractMinutes(duracionSeleccionada);

                // Calcular número de consultas posibles
                var tiempoTotal = (int)(horaFin - horaInicio).TotalMinutes;
                var numeroConsultas = tiempoTotal / duracionMinutos;

                PrevDiaLabel.Text = $"📅 {diaSeleccionado}";
                PrevHorarioLabel.Text = $"🕒 {horaInicio:HH:mm} - {horaFin:HH:mm}";
                PrevSucursalLabel.Text = $"🏥 {sucursalSeleccionada.nombre_sucursal}";
                PrevConsultasLabel.Text = $"👥 Aproximadamente {numeroConsultas} consultas de {duracionMinutos} min c/u";

                PrevisualizacionFrame.IsVisible = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando previsualización: {ex.Message}");
            }
        }

        private int ExtractMinutes(string duracionText)
        {
            if (string.IsNullOrEmpty(duracionText)) return 30;

            var parts = duracionText.Split(' ');
            if (parts.Length > 0 && int.TryParse(parts[0], out int minutos))
            {
                return minutos;
            }
            return 30;
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            if (!ValidateForm(showAlert: true))
                return;

            bool confirmar = await DisplayAlert(
                _isEditing ? "Actualizar Horario" : "Guardar Horario",
                _isEditing
                    ? "¿Está seguro que desea actualizar este horario?"
                    : "¿Está seguro que desea guardar este nuevo horario?",
                "Sí", "No");

            if (!confirmar) return;

            ShowLoading(true);

            try
            {
                var horario = CrearObjetoHorario();
                ApiResponse<HorarioMedico> response;

                if (_isEditing)
                {
                    response = await _apiService.ActualizarHorarioMedicoAsync(horario);
                }
                else
                {
                    response = await _apiService.CrearHorarioMedicoAsync(horario);
                }

                if (response.success)
                {
                    await DisplayAlert("✅ Éxito",
                        _isEditing ? "Horario actualizado correctamente" : "Horario creado correctamente",
                        "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("❌ Error", response.message ?? "Error al guardar el horario", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando horario: {ex.Message}");
                await DisplayAlert("❌ Error", "Error al guardar el horario", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private HorarioMedico CrearObjetoHorario()
        {
            var diaSeleccionado = DiaPicker.SelectedItem.ToString();
            var diaSemanaNumero = Array.IndexOf(new[] { "", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" }, diaSeleccionado);
            var sucursalSeleccionada = (Sucursal)SucursalPicker.SelectedItem;
            var duracionMinutos = ExtractMinutes(DuracionPicker.SelectedItem?.ToString());

            return new HorarioMedico
            {
                id_horario = _isEditing ? _horarioExistente.IdHorario : 0,
                id_medico = UserSessionManager.CurrentUser?.id ?? 0,
                id_sucursal = sucursalSeleccionada.id_sucursal,
                dia_semana = diaSemanaNumero,
                hora_inicio = HoraInicioPicker.Time,
                hora_fin = HoraFinPicker.Time,
                duracion_consulta = duracionMinutos,
                observaciones = ObservacionesEditor.Text?.Trim()
            };
        }

        private async void OnEliminarClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert(
                "Eliminar Horario",
                "¿Está seguro que desea eliminar este horario?\n\n⚠️ Esta acción no se puede deshacer y cancelará todas las citas futuras programadas en este horario.",
                "Sí, eliminar", "No");

            if (!confirmar) return;

            ShowLoading(true);

            try
            {
                var response = await _apiService.EliminarHorarioMedicoAsync(_horarioExistente.IdHorario);

                if (response.success)
                {
                    await DisplayAlert("✅ Éxito", "Horario eliminado correctamente", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("❌ Error", response.message ?? "Error al eliminar el horario", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error eliminando horario: {ex.Message}");
                await DisplayAlert("❌ Error", "Error al eliminar el horario", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private bool ValidateForm(bool showAlert)
        {
            if (DiaPicker.SelectedItem == null)
            {
                if (showAlert) DisplayAlert("❌ Validación", "Por favor seleccione un día de la semana", "OK");
                return false;
            }

            if (SucursalPicker.SelectedItem == null)
            {
                if (showAlert) DisplayAlert("❌ Validación", "Por favor seleccione una sucursal", "OK");
                return false;
            }

            if (HoraInicioPicker.Time >= HoraFinPicker.Time)
            {
                if (showAlert) DisplayAlert("❌ Validación", "La hora de inicio debe ser menor que la hora de fin", "OK");
                return false;
            }

            if (DuracionPicker.SelectedItem == null)
            {
                if (showAlert) DisplayAlert("❌ Validación", "Por favor seleccione la duración por consulta", "OK");
                return false;
            }

            return true;
        }

        private void ShowLoading(bool isLoading)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsVisible = isLoading;
                LoadingIndicator.IsRunning = isLoading;
                GuardarButton.IsEnabled = !isLoading;
            });
        }
    }
}