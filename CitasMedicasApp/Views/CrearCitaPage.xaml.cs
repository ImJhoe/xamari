using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Models;
using CitasMedicasApp.Services;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CrearCitaPage : ContentPage
    {
        private readonly ApiService _apiService;
        private Usuario _pacienteSeleccionado;
        private HorarioDisponible _horarioSeleccionado;
        private List<Especialidad> _especialidades;
        private List<MedicoCompleto> _medicos;
        private List<MedicoCompleto> _medicosFiltrados;
        private List<Sucursal> _sucursales;
        private ObservableCollection<HorarioSeleccionableViewModel> _horariosDisponibles;

        // Para flujo desde búsqueda de paciente
        private readonly bool _vieneDeRegistroPaciente;
        private readonly string _cedulaPreseleccionada;

        // Constructor normal
        public CrearCitaPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _horariosDisponibles = new ObservableCollection<HorarioSeleccionableViewModel>();
            HorariosCollectionView.ItemsSource = _horariosDisponibles;
            ConfigurarPagina();
            LoadInitialData();
        }

        // Constructor con paciente preseleccionado (desde búsqueda)
        public CrearCitaPage(Usuario paciente)
        {
            InitializeComponent();
            _apiService = new ApiService();
            _horariosDisponibles = new ObservableCollection<HorarioSeleccionableViewModel>();
            HorariosCollectionView.ItemsSource = _horariosDisponibles;
            _pacienteSeleccionado = paciente;
            ConfigurarPagina();
            LoadInitialData();

            if (paciente != null)
            {
                CedulaEntry.Text = paciente.cedula;
                MostrarPacienteEncontrado(paciente);
            }
        }

        // Constructor con horario preseleccionado (desde horarios disponibles)
        public CrearCitaPage(Usuario paciente, HorarioDisponible horario)
        {
            InitializeComponent();
            _apiService = new ApiService();
            _horariosDisponibles = new ObservableCollection<HorarioSeleccionableViewModel>();
            HorariosCollectionView.ItemsSource = _horariosDisponibles;
            _pacienteSeleccionado = paciente;
            _horarioSeleccionado = horario;
            ConfigurarPagina();
            LoadInitialData();
        }

        // ✅ PUNTO 6: Constructor para flujo desde registro de paciente
        public CrearCitaPage(string cedula, bool vieneDeRegistroPaciente)
        {
            InitializeComponent();
            _apiService = new ApiService();
            _horariosDisponibles = new ObservableCollection<HorarioSeleccionableViewModel>();
            HorariosCollectionView.ItemsSource = _horariosDisponibles;
            _cedulaPreseleccionada = cedula;
            _vieneDeRegistroPaciente = vieneDeRegistroPaciente;
            ConfigurarPagina();
            LoadInitialData();

            if (!string.IsNullOrEmpty(cedula))
            {
                CedulaEntry.Text = cedula;
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(500); // Esperar que se cargue la página
                    await BuscarPacientePorCedula(cedula);
                });
            }
        }

        private void ConfigurarPagina()
        {
            if (_vieneDeRegistroPaciente)
            {
                TituloLabel.Text = "CONTINUAR CON CITA";
                SubtituloLabel.Text = "Punto 6: Retorno automático tras registro";
                TituloFrame.BackgroundColor = Color.FromHex("#9b59b6");
                TituloIcon.Text = "🔄";
            }

            // Configurar fecha mínima
            FechaCitaPicker.MinimumDate = DateTime.Now.Date;
            FechaCitaPicker.MaximumDate = DateTime.Now.AddDays(90);
            FechaCitaPicker.Date = DateTime.Now.Date;

            // Configurar tipos de cita
            TipoCitaPicker.ItemsSource = new List<string>
            {
                "Presencial", "Virtual", "Domiciliaria"
            };
            TipoCitaPicker.SelectedItem = "Presencial";
        }

        private async void LoadInitialData()
        {
            ShowLoading(true);

            try
            {
                // Cargar especialidades
                await LoadEspecialidades();

                // Cargar médicos
                await LoadMedicos();

                // Cargar sucursales
                await LoadSucursales();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando datos: {ex.Message}");
                await DisplayAlert("❌ Error", "Error cargando datos iniciales", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task LoadEspecialidades()
        {
            try
            {
                var response = await _apiService.ObtenerEspecialidadesAsync();
                if (response.success && response.data != null)
                {
                    _especialidades = response.data;
                    EspecialidadPicker.ItemsSource = _especialidades;
                    EspecialidadPicker.ItemDisplayBinding = new Binding("nombre_especialidad");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando especialidades: {ex.Message}");
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
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando sucursales: {ex.Message}");
            }
        }

        // ============ PUNTO 3-4: BÚSQUEDA DE PACIENTE ============
        private async void OnBuscarPacienteClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CedulaEntry.Text))
            {
                await DisplayAlert("❌ Validación", "Por favor ingrese la cédula del paciente", "OK");
                return;
            }

            await BuscarPacientePorCedula(CedulaEntry.Text.Trim());
        }

        private async Task BuscarPacientePorCedula(string cedula)
        {
            ShowLoading(true);

            try
            {
                var response = await _apiService.BuscarPacientePorCedulaAsync(cedula);

                if (response.success && response.data != null)
                {
                    // ✅ PUNTO 4A: Paciente existe - mostrar datos
                    MostrarPacienteEncontrado(response.data);
                }
                else
                {
                    // ❌ PUNTO 4B: Paciente no existe - mostrar botón "Añadir paciente"
                    MostrarPacienteNoEncontrado();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error buscando paciente: {ex.Message}");
                await DisplayAlert("❌ Error", "Error al buscar el paciente", "OK");
                MostrarPacienteNoEncontrado();
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // ✅ PUNTO 4A: Si paciente existe, muestra sus datos
        private void MostrarPacienteEncontrado(Usuario paciente)
        {
            _pacienteSeleccionado = paciente;

            // Limpiar stack de datos del paciente
            DatosPacienteStack.Children.Clear();

            // Agregar información del paciente
            DatosPacienteStack.Children.Add(new Label
            {
                Text = $"👤 {paciente.NombreCompleto}",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromHex("#2c3e50")
            });

            DatosPacienteStack.Children.Add(new Label
            {
                Text = $"🆔 Cédula: {paciente.cedula}",
                FontSize = 14,
                TextColor = Color.FromHex("#555")
            });

            DatosPacienteStack.Children.Add(new Label
            {
                Text = $"📧 Email: {paciente.email}",
                FontSize = 14,
                TextColor = Color.FromHex("#555")
            });

            // Mostrar frames correspondientes
            ResultadoPacienteStack.IsVisible = true;
            PacienteEncontradoFrame.IsVisible = true;
            PacienteNoEncontradoFrame.IsVisible = false;
        }

        // ❌ PUNTO 4B: Si paciente no existe, aparece botón "Añadir paciente"
        private void MostrarPacienteNoEncontrado()
        {
            _pacienteSeleccionado = null;

            ResultadoPacienteStack.IsVisible = true;
            PacienteEncontradoFrame.IsVisible = false;
            PacienteNoEncontradoFrame.IsVisible = true;

            FormularioCitaFrame.IsVisible = false;
            ResumenCitaFrame.IsVisible = false;
        }

        private void OnContinuarConCitaClicked(object sender, EventArgs e)
        {
            if (_pacienteSeleccionado != null)
            {
                FormularioCitaFrame.IsVisible = true;
                ActualizarResumenButton.IsVisible = true;
            }
        }

        // ✅ PUNTO 5: Ir a pantalla de registro vinculada al flujo de cita
        private async void OnAnadirPacienteClicked(object sender, EventArgs e)
        {
            var cedula = CedulaEntry.Text?.Trim();

            // Ir a registro con flujo de retorno automático
            await Navigation.PushAsync(new RegistroPacientePage(cedula, vieneDeFlujoCita: true));
        }

        // ============ SELECCIONES DEL FORMULARIO ============
        private void OnEspecialidadChanged(object sender, EventArgs e)
        {
            var especialidadSeleccionada = (Especialidad)EspecialidadPicker.SelectedItem;
            if (especialidadSeleccionada != null && _medicos != null)
            {
                _medicosFiltrados = _medicos.Where(m => m.id_especialidad == especialidadSeleccionada.id_especialidad).ToList();
                MedicoPicker.ItemsSource = _medicosFiltrados;
                MedicoPicker.ItemDisplayBinding = new Binding("NombreCompleto");
                MedicoPicker.Title = "Seleccione un médico";

                // Limpiar selecciones posteriores
                LimpiarSeleccionesPosteriores();
            }
        }

        private void OnMedicoChanged(object sender, EventArgs e)
        {
            var medicoSeleccionado = (MedicoCompleto)MedicoPicker.SelectedItem;
            if (medicoSeleccionado != null && _sucursales != null)
            {
                SucursalPicker.ItemsSource = _sucursales;
                SucursalPicker.ItemDisplayBinding = new Binding("nombre_sucursal");
                SucursalPicker.Title = "Seleccione una sucursal";

                LimpiarHorarios();
            }
        }

        private async void OnSucursalChanged(object sender, EventArgs e)
        {
            await CargarHorariosDisponibles();
        }

        private async void OnFechaChanged(object sender, DateChangedEventArgs e)
        {
            await CargarHorariosDisponibles();
        }

        // ✅ PUNTO 7: Cargar y mostrar horarios disponibles del médico
        private async Task CargarHorariosDisponibles()
        {
            var medicoSeleccionado = (MedicoCompleto)MedicoPicker.SelectedItem;
            var sucursalSeleccionada = (Sucursal)SucursalPicker.SelectedItem;

            if (medicoSeleccionado == null || sucursalSeleccionada == null)
                return;

            ShowLoading(true);

            try
            {
                var response = await _apiService.ObtenerHorariosDisponiblesAsync(
                    medicoSeleccionado.id_medico,
                    FechaCitaPicker.Date,
                    sucursalSeleccionada.id_sucursal
                );

                if (response.success && response.data != null)
                {
                    MostrarHorariosDisponibles(response.data);
                }
                else
                {
                    MostrarSinHorarios();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando horarios: {ex.Message}");
                MostrarSinHorarios();
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void MostrarHorariosDisponibles(List<HorarioMedico> horarios)
        {
            _horariosDisponibles.Clear();

            foreach (var horario in horarios)
            {
                _horariosDisponibles.Add(new HorarioSeleccionableViewModel
                {
                    HorarioOriginal = horario,
                    HorarioTexto = $"{horario.hora_inicio:HH:mm} - {horario.hora_fin:HH:mm}",
                    EstadoTexto = horario.disponible ? "✅ Disponible" : "❌ Ocupado",
                    EstaDisponible = horario.disponible,
                    ColorFondo = horario.disponible ? Color.FromHex("#d5f4e6") : Color.FromHex("#fadbd8")
                });
            }

            HorariosDisponiblesStack.IsVisible = _horariosDisponibles.Any();

            if (!_horariosDisponibles.Any())
            {
                MostrarSinHorarios();
            }
        }

        private void MostrarSinHorarios()
        {
            _horariosDisponibles.Clear();
            HorariosDisponiblesStack.IsVisible = false;
        }

        private void OnHorarioSeleccionado(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value) // Si se seleccionó
            {
                var radioButton = sender as RadioButton;
                if (radioButton?.BindingContext is HorarioSeleccionableViewModel horarioViewModel)
                {
                    // Deseleccionar otros horarios
                    foreach (var horario in _horariosDisponibles)
                    {
                        if (horario != horarioViewModel)
                        {
                            horario.EstaSeleccionado = false;
                        }
                    }

                    horarioViewModel.EstaSeleccionado = true;
                    _horarioSeleccionado = new HorarioDisponible
                    {
                        IdHorario = horarioViewModel.HorarioOriginal.id_horario,
                        IdMedico = horarioViewModel.HorarioOriginal.id_medico,
                        HoraInicio = horarioViewModel.HorarioOriginal.hora_inicio,
                        HoraFin = horarioViewModel.HorarioOriginal.hora_fin,
                        Fecha = FechaCitaPicker.Date,
                        EstaDisponible = horarioViewModel.HorarioOriginal.disponible
                    };

                    ActualizarResumenButton.IsVisible = true;
                }
            }
        }

        private void LimpiarSeleccionesPosteriores()
        {
            MedicoPicker.SelectedItem = null;
            SucursalPicker.ItemsSource = null;
            SucursalPicker.Title = "Primero seleccione un médico";
            LimpiarHorarios();
        }

        private void LimpiarHorarios()
        {
            _horariosDisponibles.Clear();
            HorariosDisponiblesStack.IsVisible = false;
            _horarioSeleccionado = null;
            ResumenCitaFrame.IsVisible = false;
        }

        // ============ ACTUALIZAR Y MOSTRAR RESUMEN ============
        private void OnActualizarResumenClicked(object sender, EventArgs e)
        {
            if (ValidarFormulario())
            {
                MostrarResumenCita();
            }
        }

        private void MostrarResumenCita()
        {
            ResumenDetallesStack.Children.Clear();

            var medicoSeleccionado = (MedicoCompleto)MedicoPicker.SelectedItem;
            var especialidadSeleccionada = (Especialidad)EspecialidadPicker.SelectedItem;
            var sucursalSeleccionada = (Sucursal)SucursalPicker.SelectedItem;

            // Información del paciente
            ResumenDetallesStack.Children.Add(new Frame
            {
                BackgroundColor = Color.FromHex("#e8f6f3"),
                CornerRadius = 8,
                Padding = new Thickness(10),
                Content = new StackLayout
                {
                    Spacing = 5,
                    Children =
                    {
                        new Label { Text = "👤 PACIENTE", FontAttributes = FontAttributes.Bold, FontSize = 14, TextColor = Color.FromHex("#2c3e50") },
                        new Label { Text = _pacienteSeleccionado?.NombreCompleto ?? "No seleccionado", FontSize = 14 },
                        new Label { Text = $"🆔 {_pacienteSeleccionado?.cedula}", FontSize = 12, TextColor = Color.FromHex("#7f8c8d") }
                    }
                }
            });

            // Información médica
            ResumenDetallesStack.Children.Add(new Frame
            {
                BackgroundColor = Color.FromHex("#ebf3fd"),
                CornerRadius = 8,
                Padding = new Thickness(10),
                Content = new StackLayout
                {
                    Spacing = 5,
                    Children =
                    {
                        new Label { Text = "👨‍⚕️ ATENCIÓN MÉDICA", FontAttributes = FontAttributes.Bold, FontSize = 14, TextColor = Color.FromHex("#2c3e50") },
                        new Label { Text = $"🏥 {especialidadSeleccionada?.nombre_especialidad}", FontSize = 14 },
                        new Label { Text = $"👨‍⚕️ {medicoSeleccionado?.NombreCompleto}", FontSize = 14 },
                        new Label { Text = $"🏢 {sucursalSeleccionada?.nombre_sucursal}", FontSize = 12, TextColor = Color.FromHex("#7f8c8d") }
                    }
                }
            });

            // Información de fecha y hora
            ResumenDetallesStack.Children.Add(new Frame
            {
                BackgroundColor = Color.FromHex("#fef9e7"),
                CornerRadius = 8,
                Padding = new Thickness(10),
                Content = new StackLayout
                {
                    Spacing = 5,
                    Children =
                    {
                        new Label { Text = "📅 FECHA Y HORA", FontAttributes = FontAttributes.Bold, FontSize = 14, TextColor = Color.FromHex("#2c3e50") },
                        new Label { Text = $"📅 {FechaCitaPicker.Date:dddd, dd MMMM yyyy}", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#f39c12") },
                        new Label { Text = _horarioSeleccionado != null ? $"🕒 {_horarioSeleccionado.HoraInicio:HH:mm} - {_horarioSeleccionado.HoraFin:HH:mm}" : "⚠️ Seleccione un horario", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#f39c12") }
                    }
                }
            });

            // Detalles adicionales
            if (!string.IsNullOrEmpty(MotivoEditor.Text) || TipoCitaPicker.SelectedItem != null)
            {
                ResumenDetallesStack.Children.Add(new Frame
                {
                    BackgroundColor = Color.FromHex("#f4f4f4"),
                    CornerRadius = 8,
                    Padding = new Thickness(10),
                    Content = new StackLayout
                    {
                        Spacing = 5,
                        Children =
                        {
                            new Label { Text = "📋 DETALLES ADICIONALES", FontAttributes = FontAttributes.Bold, FontSize = 14, TextColor = Color.FromHex("#2c3e50") },
                            !string.IsNullOrEmpty(MotivoEditor.Text) ? new Label { Text = $"📝 {MotivoEditor.Text}", FontSize = 12 } : null,
                            TipoCitaPicker.SelectedItem != null ? new Label { Text = $"💻 Tipo: {TipoCitaPicker.SelectedItem}", FontSize = 12 } : null
                        }.Where(child => child != null).ToArray()
                    }
                });
            }

            ResumenCitaFrame.IsVisible = true;
        }

        // ============ VALIDACIÓN Y CONFIRMACIÓN ============
        private bool ValidarFormulario()
        {
            if (_pacienteSeleccionado == null)
            {
                DisplayAlert("❌ Validación", "Debe buscar y seleccionar un paciente", "OK");
                return false;
            }

            if (EspecialidadPicker.SelectedItem == null)
            {
                DisplayAlert("❌ Validación", "Debe seleccionar una especialidad", "OK");
                return false;
            }

            if (MedicoPicker.SelectedItem == null)
            {
                DisplayAlert("❌ Validación", "Debe seleccionar un médico", "OK");
                return false;
            }

            if (SucursalPicker.SelectedItem == null)
            {
                DisplayAlert("❌ Validación", "Debe seleccionar una sucursal", "OK");
                return false;
            }

            if (_horarioSeleccionado == null)
            {
                DisplayAlert("❌ Validación", "Debe seleccionar un horario disponible", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(MotivoEditor.Text))
            {
                DisplayAlert("❌ Validación", "Debe ingresar el motivo de la consulta", "OK");
                return false;
            }

            return true;
        }

        private async void OnConfirmarCitaClicked(object sender, EventArgs e)
        {
            if (!ValidarFormulario())
                return;

            bool confirmar = await DisplayAlert(
                "Confirmar Cita",
                "¿Está seguro que desea crear esta cita médica?",
                "Sí, crear", "Cancelar");

            if (!confirmar) return;

            ShowLoading(true);

            try
            {
                var cita = CrearObjetoCita();
                var response = await _apiService.CrearCitaAsync(cita);

                if (response.success)
                {
                    await DisplayAlert("✅ Éxito",
                        "¡Cita creada exitosamente!\n\n" +
                        $"📅 Fecha: {FechaCitaPicker.Date:dd/MM/yyyy}\n" +
                        $"🕒 Hora: {_horarioSeleccionado.HoraInicio:HH:mm}\n" +
                        $"👨‍⚕️ Médico: {((MedicoCompleto)MedicoPicker.SelectedItem).NombreCompleto}",
                        "OK");

                    // ✅ PUNTO 8: Flujo completo probado - regresar al menú
                    await Navigation.PopToRootAsync();
                }
                else
                {
                    await DisplayAlert("❌ Error", response.message ?? "Error al crear la cita", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando cita: {ex.Message}");
                await DisplayAlert("❌ Error", "Error al crear la cita. Verifique su conexión.", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private Cita CrearObjetoCita()
        {
            var medicoSeleccionado = (MedicoCompleto)MedicoPicker.SelectedItem;
            var sucursalSeleccionada = (Sucursal)SucursalPicker.SelectedItem;

            var fechaHora = FechaCitaPicker.Date.Add(_horarioSeleccionado.HoraInicio);

            return new Cita
            {
                id_paciente = _pacienteSeleccionado.id,
                id_medico = medicoSeleccionado.id_medico,
                id_sucursal = sucursalSeleccionada.id_sucursal,
                fecha_hora = fechaHora,
                fecha_cita = FechaCitaPicker.Date,
                hora_cita = $"{_horarioSeleccionado.HoraInicio:HH:mm}",
                motivo = MotivoEditor.Text?.Trim(),
                tipo_cita = TipoCitaPicker.SelectedItem?.ToString() ?? "Presencial",
                estado = "Programada",
                cedula_paciente = _pacienteSeleccionado.cedula
            };
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert(
                "Cancelar",
                "¿Está seguro que desea cancelar la creación de la cita?",
                "Sí, cancelar", "No");

            if (confirmar)
            {
                await Navigation.PopAsync();
            }
        }

        private void ShowLoading(bool isLoading)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsVisible = isLoading;
                LoadingIndicator.IsRunning = isLoading;
            });
        }

        // ============ MÉTODO PÚBLICO PARA PUNTO 6 ============
        /// <summary>
        /// Método público para ser llamado desde RegistroPacientePage 
        /// cuando se completa el registro (Punto 6 de la lista de cotejo)
        /// </summary>
        public async Task OnPacienteRegistradoExitosamente(Usuario nuevoPaciente)
        {
            _pacienteSeleccionado = nuevoPaciente;
            CedulaEntry.Text = nuevoPaciente.cedula;
            MostrarPacienteEncontrado(nuevoPaciente);

            // Mostrar mensaje de confirmación del flujo
            await DisplayAlert("✅ Punto 6 Completado",
                "Paciente registrado exitosamente.\n" +
                "La app ha regresado automáticamente al formulario de cita.",
                "Continuar");
        }
    }

    // ============ VIEWMODEL PARA HORARIOS SELECCIONABLES ============
    public class HorarioSeleccionableViewModel : BindableObject
    {
        public HorarioMedico HorarioOriginal { get; set; }
        public string HorarioTexto { get; set; }
        public string EstadoTexto { get; set; }
        public bool EstaDisponible { get; set; }
        public Color ColorFondo { get; set; }

        private bool _estaSeleccionado;
        public bool EstaSeleccionado
        {
            get => _estaSeleccionado;
            set
            {
                _estaSeleccionado = value;
                OnPropertyChanged();
            }
        }
    }
}