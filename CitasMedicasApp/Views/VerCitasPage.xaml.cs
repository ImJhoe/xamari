using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Models;
using CitasMedicasApp.Services;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VerCitasPage : ContentPage
    {
        private readonly ApiService _apiService;
        private List<Cita> _todasCitas;
        private List<Cita> _citasFiltradas;
        private List<MedicoCompleto> _medicos;
        private List<Especialidad> _especialidades;

        // Parámetros de filtro basados en rol
        private readonly bool _filtrarPorMedico;
        private readonly bool _soloHoy;
        private readonly bool _soloMisCitas;

        // Constructor por defecto
        public VerCitasPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            ConfigurarSegunRol();
            LoadInitialData();
        }

        // Constructor para médico con filtros específicos
        public VerCitasPage(bool filtrarPorMedico = false, bool soloHoy = false)
        {
            InitializeComponent();
            _apiService = new ApiService();
            _filtrarPorMedico = filtrarPorMedico;
            _soloHoy = soloHoy;
            ConfigurarSegunRol();
            LoadInitialData();
        }

        // Constructor para paciente
        public VerCitasPage(bool soloMisCitas = false)
        {
            InitializeComponent();
            _apiService = new ApiService();
            _soloMisCitas = soloMisCitas;
            ConfigurarSegunRol();
            LoadInitialData();
        }

        private void ConfigurarSegunRol()
        {
            // Configurar según el rol del usuario
            if (UserSessionManager.IsAdmin)
            {
                ConfigurarParaAdmin();
            }
            else if (UserSessionManager.IsRecepcionista)
            {
                ConfigurarParaRecepcionista();
            }
            else if (UserSessionManager.IsMedico)
            {
                ConfigurarParaMedico();
            }
            else if (UserSessionManager.IsPaciente)
            {
                ConfigurarParaPaciente();
            }

            // Configurar fechas por defecto
            var hoy = DateTime.Now;
            FechaDesdeDate.Date = _soloHoy ? hoy : hoy.AddDays(-30);
            FechaHastaDate.Date = _soloHoy ? hoy : hoy.AddDays(30);

            // Configurar estados
            EstadoFiltro.ItemsSource = new List<string>
            {
                "Todos", "Programada", "Confirmada", "Completada", "Cancelada"
            };
            EstadoFiltro.SelectedItem = "Todos";
        }

        private void ConfigurarParaAdmin()
        {
            HeaderFrame.BackgroundColor = Color.FromHex("#2c3e50");
            HeaderIcon.Text = "👨‍💼";
            HeaderTitulo.Text = "TODAS LAS CITAS - ADMIN";
            HeaderSubtitulo.Text = "Vista completa del sistema";

            // Mostrar todos los filtros
            EspecialidadFiltroStack.IsVisible = true;
            MedicoFiltroStack.IsVisible = true;

            // ✅ CORRECTO: Agregar/remover ToolbarItems dinámicamente
            if (!ToolbarItems.Contains(FiltrosToolbar))
            {
                ToolbarItems.Add(FiltrosToolbar);
            }
        }

        private void ConfigurarParaRecepcionista()
        {
            HeaderFrame.BackgroundColor = Color.FromHex("#e67e22");
            HeaderIcon.Text = "🏥";
            HeaderTitulo.Text = "GESTIÓN DE CITAS";
            HeaderSubtitulo.Text = "Puntos 3-7 Lista de Cotejo";

            // Mostrar filtros de especialidad
            EspecialidadFiltroStack.IsVisible = true;
            MedicoFiltroStack.IsVisible = false;

            // ✅ CORRECTO: Agregar/remover ToolbarItems dinámicamente
            if (!ToolbarItems.Contains(FiltrosToolbar))
            {
                ToolbarItems.Add(FiltrosToolbar);
            }
        }

        private void ConfigurarParaMedico()
        {
            HeaderFrame.BackgroundColor = Color.FromHex("#3498db");
            HeaderIcon.Text = "👨‍⚕️";
            HeaderTitulo.Text = _soloHoy ? "MIS CITAS DE HOY" : "MIS CITAS";
            HeaderSubtitulo.Text = $"Dr. {UserSessionManager.GetUserDisplayName()}";

            // Solo filtros básicos para médico
            EspecialidadFiltroStack.IsVisible = false;
            MedicoFiltroStack.IsVisible = false;

            // ✅ CORRECTO: Agregar/remover ToolbarItems dinámicamente
            if (!ToolbarItems.Contains(FiltrosToolbar))
            {
                ToolbarItems.Add(FiltrosToolbar);
            }
        }

        private void ConfigurarParaPaciente()
        {
            HeaderFrame.BackgroundColor = Color.FromHex("#27ae60");
            HeaderIcon.Text = "🧑‍🦱";
            HeaderTitulo.Text = "MIS CITAS MÉDICAS";
            HeaderSubtitulo.Text = UserSessionManager.GetUserDisplayName();

            // Sin filtros para paciente
            EspecialidadFiltroStack.IsVisible = false;
            MedicoFiltroStack.IsVisible = false;
            FiltrosFrame.IsVisible = false;

            // ✅ CORRECTO: Remover ToolbarItems
            if (ToolbarItems.Contains(FiltrosToolbar))
            {
                ToolbarItems.Remove(FiltrosToolbar);
            }
        }

        private async void LoadInitialData()
        {
            ShowLoading(true);

            try
            {
                // Cargar filtros si es necesario
                if (UserSessionManager.IsAdmin || UserSessionManager.IsRecepcionista)
                {
                    await LoadFiltros();
                }

                // Cargar citas
                await LoadCitas();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando datos: {ex.Message}");
                await DisplayAlert("❌ Error", "Error cargando las citas", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task LoadFiltros()
        {
            try
            {
                // Cargar especialidades
                if (EspecialidadFiltroStack.IsVisible)
                {
                    var especialidadesResponse = await _apiService.ObtenerEspecialidadesAsync();
                    if (especialidadesResponse.success && especialidadesResponse.data != null)
                    {
                        _especialidades = especialidadesResponse.data;
                        var especialidadesNombres = new List<string> { "Todas" };
                        especialidadesNombres.AddRange(_especialidades.Select(e => e.nombre_especialidad));
                        EspecialidadFiltro.ItemsSource = especialidadesNombres;
                        EspecialidadFiltro.SelectedItem = "Todas";
                    }
                }

                // Cargar médicos (solo para admin)
                if (MedicoFiltroStack.IsVisible)
                {
                    var medicosResponse = await _apiService.ObtenerTodosMedicosAsync();
                    if (medicosResponse.success && medicosResponse.data != null)
                    {
                        _medicos = medicosResponse.data;
                        var medicosNombres = new List<string> { "Todos" };
                        medicosNombres.AddRange(_medicos.Select(m => m.NombreCompleto));
                        MedicoFiltro.ItemsSource = medicosNombres;
                        MedicoFiltro.SelectedItem = "Todos";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando filtros: {ex.Message}");
            }
        }

        private async Task LoadCitas()
        {
            try
            {
                ApiResponse<List<Cita>> response;

                if (UserSessionManager.IsPaciente || _soloMisCitas)
                {
                    // Paciente: solo sus citas
                    var idPaciente = UserSessionManager.CurrentUser?.id ?? 0;
                    response = await _apiService.ConsultarCitasAsync(idPaciente);
                }
                else if (UserSessionManager.IsMedico || _filtrarPorMedico)
                {
                    // Médico: solo sus citas
                    var idMedico = UserSessionManager.CurrentUser?.id ?? 0;
                    response = await _apiService.ConsultarCitasAsync(idMedico: idMedico);
                }
                else
                {
                    // Admin/Recepcionista: todas las citas
                    response = await _apiService.ConsultarCitasAsync();
                }

                if (response.success && response.data != null)
                {
                    _todasCitas = response.data;

                    // Aplicar filtro de fecha si es necesario
                    if (_soloHoy)
                    {
                        var hoy = DateTime.Now.Date;
                        _todasCitas = _todasCitas.Where(c =>
                        {
                            var fechaCita = c.fecha_cita != default(DateTime) ? c.fecha_cita.Date : c.fecha_hora.Date;
                            return fechaCita == hoy;
                        }).ToList();
                    }

                    AplicarFiltrosYMostrarCitas();
                }
                else
                {
                    MostrarSinCitas("No se pudieron cargar las citas");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando citas: {ex.Message}");
                MostrarSinCitas("Error cargando las citas");
            }
        }

        private void AplicarFiltrosYMostrarCitas()
        {
            if (_todasCitas == null || !_todasCitas.Any())
            {
                MostrarSinCitas();
                return;
            }

            _citasFiltradas = _todasCitas.Where(c =>
            {
                // Filtro por fecha
                var fechaCita = c.fecha_cita != default(DateTime) ? c.fecha_cita.Date : c.fecha_hora.Date;
                if (fechaCita < FechaDesdeDate.Date || fechaCita > FechaHastaDate.Date)
                    return false;

                // Filtro por especialidad
                if (EspecialidadFiltroStack.IsVisible)
                {
                    var especialidadSeleccionada = (string)EspecialidadFiltro.SelectedItem;
                    if (!string.IsNullOrEmpty(especialidadSeleccionada) && especialidadSeleccionada != "Todas")
                    {
                        var especialidadCita = c.especialidad ?? c.nombre_especialidad ?? "";
                        if (especialidadCita != especialidadSeleccionada)
                            return false;
                    }
                }

                // Filtro por estado
                var estadoSeleccionado = (string)EstadoFiltro.SelectedItem;
                if (!string.IsNullOrEmpty(estadoSeleccionado) && estadoSeleccionado != "Todos")
                {
                    var estadoCita = c.estado ?? "Programada";
                    if (estadoCita != estadoSeleccionado)
                        return false;
                }

                // Filtro por médico (solo para admin)
                if (MedicoFiltroStack.IsVisible)
                {
                    var medicoSeleccionado = (string)MedicoFiltro.SelectedItem;
                    if (!string.IsNullOrEmpty(medicoSeleccionado) && medicoSeleccionado != "Todos")
                    {
                        var medicoCita = c.nombre_medico ?? "";
                        if (medicoCita != medicoSeleccionado)
                            return false;
                    }
                }

                return true;
            }).OrderByDescending(c => c.fecha_cita != default(DateTime) ? c.fecha_cita : c.fecha_hora).ToList();

            if (_citasFiltradas.Any())
            {
                MostrarCitas(_citasFiltradas);
                MostrarResumen(_citasFiltradas);
            }
            else
            {
                MostrarSinCitas("No se encontraron citas con los filtros aplicados");
            }
        }

        private void MostrarCitas(List<Cita> citas)
        {
            CitasStack.Children.Clear();
            SinCitasFrame.IsVisible = false;

            foreach (var cita in citas)
            {
                var citaFrame = CrearCitaFrame(cita);
                CitasStack.Children.Add(citaFrame);
            }
        }

        private Frame CrearCitaFrame(Cita cita)
        {
            var fechaCita = cita.fecha_cita != default(DateTime) ? cita.fecha_cita : cita.fecha_hora;
            var estadoColor = ObtenerColorEstado(cita.estado ?? "Programada");

            // ✅ CÓDIGO CORREGIDO - Crear lista de elementos primero
            var children = new List<View>
    {
        // Header con fecha y estado
        new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Children =
            {
                new Label
                {
                    Text = $"📅 {fechaCita:dddd, dd/MM/yyyy} - {fechaCita:HH:mm}",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16
                }.Apply(l => Grid.SetColumn(l, 0)),

                new Frame
                {
                    BackgroundColor = estadoColor,
                    CornerRadius = 12,
                    Padding = new Thickness(8, 4),
                    Content = new Label
                    {
                        Text = cita.estado ?? "Programada",
                        TextColor = Color.White,
                        FontSize = 10,
                        FontAttributes = FontAttributes.Bold
                    }
                }.Apply(f => Grid.SetColumn(f, 1))
            }
        },

        // Información del paciente (si no es paciente el que ve)
        !UserSessionManager.IsPaciente ? new Label
        {
            Text = $"👤 Paciente: {cita.nombre_paciente}",
            FontSize = 14,
            TextColor = Color.FromHex("#2c3e50")
        } : null,

        // Información del médico (si no es médico el que ve)
        !UserSessionManager.IsMedico ? new Label
        {
            Text = $"👨‍⚕️ Médico: {cita.nombre_medico}",
            FontSize = 14,
            TextColor = Color.FromHex("#2c3e50")
        } : null,

        // Especialidad y sucursal
        new Label
        {
            Text = $"🏥 {cita.nombre_especialidad} - {cita.nombre_sucursal}",
            FontSize = 12,
            TextColor = Color.FromHex("#7f8c8d")
        },

        // Motivo
        !string.IsNullOrEmpty(cita.motivo) ? new Label
        {
            Text = $"📝 Motivo: {cita.motivo}",
            FontSize = 12,
            TextColor = Color.FromHex("#555")
        } : null,

        // Botones de acción según rol
        CrearBotonesAccion(cita)
    };

            // Filtrar elementos nulos y crear StackLayout
            var stackLayout = new StackLayout
            {
                Spacing = 10
            };

            foreach (var child in children.Where(c => c != null))
            {
                stackLayout.Children.Add(child);
            }

            // Crear y retornar el Frame
            var frame = new Frame
            {
                BackgroundColor = Color.White,
                HasShadow = true,
                CornerRadius = 10,
                Margin = new Thickness(0, 5),
                Content = stackLayout
            };

            return frame;
        }

        private StackLayout CrearBotonesAccion(Cita cita)
        {
            var buttonsStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10
            };

            var fechaCita = cita.fecha_cita != default(DateTime) ? cita.fecha_cita : cita.fecha_hora;
            var puedeModificar = fechaCita > DateTime.Now.AddHours(2); // Solo si faltan más de 2 horas

            if (UserSessionManager.IsAdmin || UserSessionManager.IsRecepcionista)
            {
                // Admin/Recepcionista pueden ver detalles y reagendar
                buttonsStack.Children.Add(new Button
                {
                    Text = "👁️ Ver",
                    BackgroundColor = Color.FromHex("#3498db"),
                    TextColor = Color.White,
                    FontSize = 10,
                    CornerRadius = 15,
                    Command = new Command(async () => await VerDetalleCita(cita))
                });

                if (puedeModificar && (cita.estado ?? "").ToLower() != "cancelada")
                {
                    buttonsStack.Children.Add(new Button
                    {
                        Text = "✏️ Editar",
                        BackgroundColor = Color.FromHex("#27ae60"),
                        TextColor = Color.White,
                        FontSize = 10,
                        CornerRadius = 15,
                        Command = new Command(async () => await EditarCita(cita))
                    });
                }
            }
            else if (UserSessionManager.IsMedico)
            {
                // Médico puede ver detalles y atender
                buttonsStack.Children.Add(new Button
                {
                    Text = "👁️ Ver",
                    BackgroundColor = Color.FromHex("#3498db"),
                    TextColor = Color.White,
                    FontSize = 10,
                    CornerRadius = 15,
                    Command = new Command(async () => await VerDetalleCita(cita))
                });

                if (fechaCita.Date == DateTime.Now.Date && (cita.estado ?? "").ToLower() != "completada")
                {
                    buttonsStack.Children.Add(new Button
                    {
                        Text = "🩺 Atender",
                        BackgroundColor = Color.FromHex("#27ae60"),
                        TextColor = Color.White,
                        FontSize = 10,
                        CornerRadius = 15,
                        Command = new Command(async () => await AtenderCita(cita))
                    });
                }
            }
            else if (UserSessionManager.IsPaciente)
            {
                // Paciente puede ver detalles y reagendar/cancelar
                buttonsStack.Children.Add(new Button
                {
                    Text = "👁️ Ver",
                    BackgroundColor = Color.FromHex("#3498db"),
                    TextColor = Color.White,
                    FontSize = 10,
                    CornerRadius = 15,
                    Command = new Command(async () => await VerDetalleCita(cita))
                });

                if (puedeModificar && (cita.estado ?? "").ToLower() != "cancelada")
                {
                    buttonsStack.Children.Add(new Button
                    {
                        Text = "❌ Cancelar",
                        BackgroundColor = Color.FromHex("#e74c3c"),
                        TextColor = Color.White,
                        FontSize = 10,
                        CornerRadius = 15,
                        Command = new Command(async () => await CancelarCita(cita))
                    });
                }
            }

            return buttonsStack;
        }

        private Color ObtenerColorEstado(string estado)
        {
            return estado.ToLower() switch
            {
                "programada" => Color.FromHex("#f39c12"),
                "confirmada" => Color.FromHex("#27ae60"),
                "completada" => Color.FromHex("#3498db"),
                "cancelada" => Color.FromHex("#e74c3c"),
                _ => Color.FromHex("#95a5a6")
            };
        }

        private void MostrarSinCitas(string mensaje = null)
        {
            CitasStack.Children.Clear();
            SinCitasFrame.IsVisible = true;
            ResumenFrame.IsVisible = false;

            if (!string.IsNullOrEmpty(mensaje))
            {
                SinCitasTexto.Text = "NO HAY CITAS";
                SinCitasDetalle.Text = mensaje;
            }
            else
            {
                SinCitasTexto.Text = UserSessionManager.IsPaciente ? "NO TIENES CITAS" : "NO HAY CITAS DISPONIBLES";
                SinCitasDetalle.Text = UserSessionManager.IsPaciente
                    ? "Cuando tengas citas programadas aparecerán aquí."
                    : "Pruebe ajustando los filtros de búsqueda.";
            }
        }

        private void MostrarResumen(List<Cita> citas)
        {
            var totalCitas = citas.Count;

            var citasPendientes = citas.Count(c =>
            {
                var estado = (c.estado ?? "Programada").ToLower();
                return estado == "programada" || estado == "confirmada";
            });

            var citasCompletadas = citas.Count(c => (c.estado ?? "").ToLower() == "completada");
            var citasCanceladas = citas.Count(c => (c.estado ?? "").ToLower() == "cancelada");

            ResumenDetalle.Text = $"📊 Total: {totalCitas} | ⏳ Pendientes: {citasPendientes} | ✅ Completadas: {citasCompletadas} | ❌ Canceladas: {citasCanceladas}";
            ResumenFrame.IsVisible = true;
        }

        // ============ EVENTOS ============
        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadCitas();
        }

        private void OnFiltrosClicked(object sender, EventArgs e)
        {
            FiltrosFrame.IsVisible = !FiltrosFrame.IsVisible;
        }

        private void OnAplicarFiltrosClicked(object sender, EventArgs e)
        {
            AplicarFiltrosYMostrarCitas();
            FiltrosFrame.IsVisible = false;
        }

        private void OnLimpiarFiltrosClicked(object sender, EventArgs e)
        {
            // Restaurar filtros por defecto
            var hoy = DateTime.Now;
            FechaDesdeDate.Date = _soloHoy ? hoy : hoy.AddDays(-30);
            FechaHastaDate.Date = _soloHoy ? hoy : hoy.AddDays(30);

            if (EspecialidadFiltro.ItemsSource != null)
                EspecialidadFiltro.SelectedItem = "Todas";

            EstadoFiltro.SelectedItem = "Todos";

            if (MedicoFiltro.ItemsSource != null)
                MedicoFiltro.SelectedItem = "Todos";

            AplicarFiltrosYMostrarCitas();
        }

        // ============ ACCIONES DE CITAS ============
        private async Task VerDetalleCita(Cita cita)
        {
            await Navigation.PushAsync(new DetalleCitaPage(cita));
        }

        private async Task EditarCita(Cita cita)
        {
            // Asumo que tienes un constructor que acepta una cita para editar
            await DisplayAlert("🚧 En Desarrollo", "Funcionalidad de edición en desarrollo", "OK");
            // TODO: Implementar página de edición o modificar CrearCitaPage
            // await Navigation.PushAsync(new CrearCitaPage(cita));
        }

        private async Task AtenderCita(Cita cita)
        {
            await DisplayAlert("🚧 En Desarrollo", "Funcionalidad de atención médica en desarrollo", "OK");
            // TODO: await Navigation.PushAsync(new AtencionMedicaPage(cita));
        }

        private async Task CancelarCita(Cita cita)
        {
            bool confirmar = await DisplayAlert(
                "Cancelar Cita",
                $"¿Está seguro que desea cancelar la cita del {cita.fecha_cita:dd/MM/yyyy} a las {cita.fecha_cita:HH:mm}?\n\n⚠️ Esta acción no se puede deshacer.",
                "Sí, cancelar", "No");

            if (!confirmar) return;

            ShowLoading(true);

            try
            {
                var response = await _apiService.CancelarCitaAsync(cita.id_cita);

                if (response.success)
                {
                    await DisplayAlert("✅ Éxito", "Cita cancelada exitosamente", "OK");
                    await LoadCitas(); // Recargar datos
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

        private void ShowLoading(bool isLoading)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingStack.IsVisible = isLoading;
                LoadingIndicator.IsRunning = isLoading;
            });
        }
    }
}

// ============ EXTENSION HELPER ============
public static class ViewExtensions
{
    public static T Apply<T>(this T view, System.Action<T> action) where T : View
    {
        action(view);
        return view;
    }
}
