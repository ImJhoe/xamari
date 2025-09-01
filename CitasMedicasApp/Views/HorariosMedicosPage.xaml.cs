using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Models;
using CitasMedicasApp.Services;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HorariosMedicosPage : ContentPage
    {
        private readonly ApiService _apiService;
        private List<MedicoCompleto> _medicos;
        private List<Sucursal> _sucursales;
        private ObservableCollection<HorarioDisponible> _horariosDisponibles;

        public ICommand AgendarHorarioCommand { get; set; }

        public HorariosMedicosPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _horariosDisponibles = new ObservableCollection<HorarioDisponible>();
            HorariosCollectionView.ItemsSource = _horariosDisponibles;

            AgendarHorarioCommand = new Command<HorarioDisponible>(async (horario) => await OnAgendarHorarioClicked(horario));
            BindingContext = this;

            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            ShowLoading(true);

            try
            {
                // Cargar médicos
                await LoadMedicos();

                // Cargar sucursales
                await LoadSucursales();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando datos iniciales: {ex.Message}");
                await DisplayAlert("❌ Error", "Error cargando datos iniciales", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task LoadMedicos()
        {
            try
            {
                var response = await _apiService.ObtenerTodosMedicosAsync();
                if (response.success && response.data != null)
                {
                    _medicos = response.data;
                    MedicoPicker.ItemsSource = _medicos;
                    MedicoPicker.ItemDisplayBinding = new Binding("NombreCompleto");
                }
                else
                {
                    await DisplayAlert("❌ Error", "No se pudieron cargar los médicos", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando médicos: {ex.Message}");
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

                    // Seleccionar la primera sucursal por defecto
                    if (_sucursales.Any())
                    {
                        SucursalPicker.SelectedItem = _sucursales.First();
                    }
                }
                else
                {
                    await DisplayAlert("❌ Error", "No se pudieron cargar las sucursales", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando sucursales: {ex.Message}");
            }
        }

        private async void OnBuscarHorariosClicked(object sender, EventArgs e)
        {
            if (MedicoPicker.SelectedItem == null)
            {
                await DisplayAlert("❌ Validación", "Por favor seleccione un médico", "OK");
                return;
            }

            if (SucursalPicker.SelectedItem == null)
            {
                await DisplayAlert("❌ Validación", "Por favor seleccione una sucursal", "OK");
                return;
            }

            ShowLoading(true);

            try
            {
                var medicoSeleccionado = (MedicoCompleto)MedicoPicker.SelectedItem;
                var sucursalSeleccionada = (Sucursal)SucursalPicker.SelectedItem;
                var fechaSeleccionada = FechaPicker.Date;

                // ✅ PUNTO 7: Obtener horarios disponibles del médico
                var response = await _apiService.ObtenerHorariosDisponiblesAsync(
                    medicoSeleccionado.id_medico,
                    fechaSeleccionada,
                    sucursalSeleccionada.id_sucursal
                );

                if (response.success && response.data != null)
                {
                    MostrarHorarios(response.data, medicoSeleccionado, fechaSeleccionada);
                }
                else
                {
                    MostrarSinHorarios();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo horarios: {ex.Message}");
                await DisplayAlert("❌ Error", "Error al buscar horarios disponibles", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void MostrarHorarios(List<HorarioMedico> horarios, MedicoCompleto medico, DateTime fecha)
        {
            _horariosDisponibles.Clear();

            foreach (var horario in horarios)
            {
                _horariosDisponibles.Add(new HorarioDisponible
                {
                    IdHorario = horario.id_horario,
                    IdMedico = medico.id_medico,
                    NombreMedico = medico.NombreCompleto,
                    HoraInicio = horario.hora_inicio,
                    HoraFin = horario.hora_fin,
                    Fecha = fecha,
                    EstaDisponible = horario.disponible,
                    HorarioTexto = $"{horario.hora_inicio:HH:mm} - {horario.hora_fin:HH:mm}",
                    EstadoTexto = horario.disponible ? "✅ Disponible" : "❌ Ocupado"
                });
            }

            HorariosFrame.IsVisible = true;
            HorariosTitulo.Text = $"HORARIOS DE {medico.NombreCompleto.ToUpper()} - {fecha:dd/MM/yyyy}";
            NoHorariosLabel.IsVisible = !_horariosDisponibles.Any();
        }

        private void MostrarSinHorarios()
        {
            _horariosDisponibles.Clear();
            HorariosFrame.IsVisible = true;
            NoHorariosLabel.IsVisible = true;
            HorariosTitulo.Text = "SIN HORARIOS DISPONIBLES";
        }

        private async Task OnAgendarHorarioClicked(HorarioDisponible horario)
        {
            bool confirmar = await DisplayAlert(
                "Confirmar Cita",
                $"¿Desea agendar cita con {horario.NombreMedico}?\n" +
                $"📅 Fecha: {horario.Fecha:dd/MM/yyyy}\n" +
                $"🕒 Hora: {horario.HorarioTexto}",
                "Sí", "No");

            if (confirmar)
            {
                // Navegar a crear cita con los datos preseleccionados
                await Navigation.PushAsync(new CrearCitaPage(null, horario));
            }
        }

        private void OnMedicoSeleccionado(object sender, EventArgs e)
        {
            HorariosFrame.IsVisible = false;
        }

        private void OnFechaSeleccionada(object sender, DateChangedEventArgs e)
        {
            HorariosFrame.IsVisible = false;
        }

        private void OnSucursalSeleccionada(object sender, EventArgs e)
        {
            HorariosFrame.IsVisible = false;
        }

        private void ShowLoading(bool isLoading)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsVisible = isLoading;
                LoadingIndicator.IsRunning = isLoading;
                BuscarHorariosButton.IsEnabled = !isLoading;
                BuscarHorariosButton.Text = isLoading ? "Buscando..." : "🔍 BUSCAR HORARIOS DISPONIBLES";
            });
        }
    }

    // ===== MODELOS AUXILIARES =====
    public class HorarioDisponible
    {
        public int IdHorario { get; set; }
        public int IdMedico { get; set; }
        public string NombreMedico { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public DateTime Fecha { get; set; }
        public bool EstaDisponible { get; set; }
        public string HorarioTexto { get; set; }
        public string EstadoTexto { get; set; }
    }
}