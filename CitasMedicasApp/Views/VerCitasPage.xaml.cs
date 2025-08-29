using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using CitasMedicasApp.Models;

namespace CitasMedicasApp.Views
{
    public partial class VerCitasPage : ContentPage
    {
        private readonly ApiService _apiService;
        private List<Cita> _todasCitas;
        private List<Especialidad> _especialidades;

        public VerCitasPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _todasCitas = new List<Cita>();

            // Configurar fechas por defecto
            FechaDesdeDate.Date = DateTime.Now;
            FechaHastaDate.Date = DateTime.Now.AddDays(7);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarDatos();
        }

        private async void CargarDatos()
        {
            ShowLoading(true);

            try
            {
                // Cargar especialidades para filtros
                var responseEspecialidades = await _apiService.ObtenerEspecialidadesAsync();
                if (responseEspecialidades.success && responseEspecialidades.data != null)
                {
                    _especialidades = responseEspecialidades.data;
                    var especialidadesConTodas = new List<string> { "Todas" };
                    especialidadesConTodas.AddRange(_especialidades.Select(e => e.nombre_especialidad));
                    EspecialidadFiltro.ItemsSource = especialidadesConTodas;
                }

                // Cargar citas
                await CargarCitas();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar datos: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task CargarCitas()
        {
            try
            {
                var response = await _apiService.ObtenerCitasAsync();

                if (response.success && response.data != null)
                {
                    _todasCitas = response.data;
                    MostrarCitas(_todasCitas);
                    MostrarResumen();
                }
                else
                {
                    NoDataLabel.IsVisible = true;
                    ResumenFrame.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar citas: {ex.Message}", "OK");
            }
        }

        private void MostrarCitas(List<Cita> citas)
        {
            // Limpiar contenedor
            var citasParaMostrar = CitasStackLayout.Children.Where(c => c is Frame && ((Frame)c).ClassId == "cita-item").ToList();
            foreach (var item in citasParaMostrar)
            {
                CitasStackLayout.Children.Remove(item);
            }

            NoDataLabel.IsVisible = false;

            if (citas == null || citas.Count == 0)
            {
                NoDataLabel.IsVisible = true;
                return;
            }

            // Agrupar por fecha
            var citasAgrupadas = citas.GroupBy(c => c.fecha_cita.Date).OrderBy(g => g.Key);

            foreach (var grupo in citasAgrupadas)
            {
                // Header de fecha
                var fechaHeader = new Label
                {
                    Text = $"📅 {grupo.Key:dddd, dd MMMM yyyy}",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromHex("#2c3e50"),
                    Margin = new Thickness(0, 15, 0, 5)
                };
                CitasStackLayout.Children.Add(fechaHeader);

                // Citas de esa fecha
                foreach (var cita in grupo.OrderBy(c => c.hora_inicio))
                {
                    var citaFrame = CrearTarjetaCita(cita);
                    CitasStackLayout.Children.Add(citaFrame);
                }
            }
        }

        private Frame CrearTarjetaCita(Cita cita)
        {
            var frame = new Frame
            {
                BackgroundColor = Color.White,
                HasShadow = true,
                CornerRadius = 10,
                Padding = 15,
                Margin = new Thickness(0, 5),
                ClassId = "cita-item"
            };

            var mainStack = new StackLayout { Spacing = 8 };

            // Header con hora y estado
            var headerStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
        {
            new Label
            {
                Text = $"🕐 {cita.hora_formateada}",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromHex("#2c3e50"),
                VerticalOptions = LayoutOptions.Center
            },
            new Label
            {
                Text = ObtenerEmojiEstado(cita.estado_formateado) + " " + cita.estado_formateado,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = ObtenerColorEstado(cita.estado_formateado),
                HorizontalOptions = LayoutOptions.EndAndExpand
            }
        }
            };

            // Información del paciente
            var pacienteLabel = new Label
            {
                Text = $"👤 Paciente: {cita.nombre_paciente ?? "N/A"}",
                FontSize = 14,
                TextColor = Color.FromHex("#2c3e50")
            };

            // Información del médico
            var medicoLabel = new Label
            {
                Text = $"👨‍⚕️ Dr(a). {cita.nombre_medico ?? "N/A"} - {cita.especialidad ?? cita.nombre_especialidad ?? "N/A"}",
                FontSize = 14,
                TextColor = Color.FromHex("#3498db")
            };

            // Motivo
            var motivoLabel = new Label
            {
                Text = $"📋 Motivo: {cita.motivo ?? "No especificado"}",
                FontSize = 13,
                TextColor = Color.FromHex("#555")
            };

            // Tipo y sucursal
            var tipoSucursalStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 15,
                Children =
        {
            new Label
            {
                Text = $"📍 {cita.tipo_cita ?? cita.nombre_tipo ?? "Presencial"}",
                FontSize = 12,
                TextColor = Color.FromHex("#666")
            },
            new Label
            {
                Text = $"🏥 {cita.nombre_sucursal ?? "N/A"}",
                FontSize = 12,
                TextColor = Color.FromHex("#666")
            }
        }
            };

            mainStack.Children.Add(headerStack);
            mainStack.Children.Add(pacienteLabel);
            mainStack.Children.Add(medicoLabel);
            mainStack.Children.Add(motivoLabel);
            mainStack.Children.Add(tipoSucursalStack);

            // Observaciones si existen
            var observaciones = cita.observaciones ?? cita.notas;
            if (!string.IsNullOrEmpty(observaciones))
            {
                var observacionesLabel = new Label
                {
                    Text = $"💭 Notas: {observaciones}",
                    FontSize = 12,
                    TextColor = Color.FromHex("#7f8c8d"),
                    FontAttributes = FontAttributes.Italic
                };
                mainStack.Children.Add(observacionesLabel);
            }

            frame.Content = mainStack;
            return frame;
        }

        private string ObtenerEmojiEstado(string estado)
        {
            return estado.ToLower() switch
            {
                "pendiente" => "⏳",
                "confirmada" => "✅",
                "completada" => "✅",
                "cancelada" => "❌",
                _ => "📋"
            };
        }

        private Color ObtenerColorEstado(string estado)
        {
            return estado.ToLower() switch
            {
                "pendiente" => Color.FromHex("#f39c12"),
                "confirmada" => Color.FromHex("#27ae60"),
                "completada" => Color.FromHex("#2ecc71"),
                "cancelada" => Color.FromHex("#e74c3c"),
                _ => Color.FromHex("#7f8c8d")
            };
        }

        private void MostrarResumen()
        {
            if (_todasCitas == null || _todasCitas.Count == 0)
            {
                ResumenFrame.IsVisible = false;
                return;
            }

            var totalCitas = _todasCitas.Count;
            var citasPendientes = _todasCitas.Count(c => (c.estado ?? "Pendiente").ToLower() == "pendiente");
            var citasCompletadas = _todasCitas.Count(c => (c.estado ?? "").ToLower() == "completada");
            var citasCanceladas = _todasCitas.Count(c => (c.estado ?? "").ToLower() == "cancelada");

            ResumenDetalle.Text = $"Total: {totalCitas} | Pendientes: {citasPendientes} | Completadas: {citasCompletadas} | Canceladas: {citasCanceladas}";
            ResumenFrame.IsVisible = true;
        }

        private void OnFiltrosClicked(object sender, EventArgs e)
        {
            FiltrosFrame.IsVisible = !FiltrosFrame.IsVisible;
        }

        private async void OnAplicarFiltrosClicked(object sender, EventArgs e)
        {
            ShowLoading(true);

            try
            {
                var citasFiltradas = _todasCitas.Where(c =>
                {
                    // Filtro por fecha
                    var fechaCita = c.fecha_cita != default(DateTime) ? c.fecha_cita.Date : c.fecha_hora.Date;
                    if (fechaCita < FechaDesdeDate.Date || fechaCita > FechaHastaDate.Date)
                        return false;

                    // Filtro por especialidad
                    var especialidadSeleccionada = (string)EspecialidadFiltro.SelectedItem;
                    if (!string.IsNullOrEmpty(especialidadSeleccionada) &&
                        especialidadSeleccionada != "Todas")
                    {
                        var especialidadCita = c.especialidad ?? c.nombre_especialidad ?? "";
                        if (especialidadCita != especialidadSeleccionada)
                            return false;
                    }

                    // Filtro por estado
                    var estadoSeleccionado = (string)EstadoFiltro.SelectedItem;
                    if (!string.IsNullOrEmpty(estadoSeleccionado) &&
                        estadoSeleccionado != "Todos")
                    {
                        var estadoCita = c.estado ?? "Pendiente";
                        if (estadoCita != estadoSeleccionado)
                            return false;
                    }

                    return true;
                }).ToList();

                MostrarCitas(citasFiltradas);
                FiltrosFrame.IsVisible = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al aplicar filtros: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }
        private void OnLimpiarFiltrosClicked(object sender, EventArgs e)
        {
            FechaDesdeDate.Date = DateTime.Now;
            FechaHastaDate.Date = DateTime.Now.AddDays(7);
            EspecialidadFiltro.SelectedItem = null;
            EstadoFiltro.SelectedItem = null;
            MostrarCitas(_todasCitas);
            FiltrosFrame.IsVisible = false;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            CargarDatos();
        }

        private void ShowLoading(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
        }
    }
}