using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using CitasMedicasApp.Models;

namespace CitasMedicasApp.Views
{
    public partial class CrearCitaPage : ContentPage
    {
        private readonly ApiService _apiService;
        private PacienteCompleto _pacienteSeleccionado;
        private List<Especialidad> _especialidades;
        private List<MedicoCompleto> _medicos;
        private List<MedicoCompleto> _medicosFiltrados;
        private List<Sucursal> _sucursalesDelMedico;
        private List<TipoCita> _tiposCita;

        public CrearCitaPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            CargarDatosIniciales();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Solo cargar datos si es necesario
            if (_especialidades == null || _especialidades.Count == 0)
            {
                CargarDatosIniciales();
            }
        }

        private async void CargarDatosIniciales()
        {
            ShowLoading(true);

            try
            {
                // Cargar especialidades
                var responseEspecialidades = await _apiService.ObtenerEspecialidadesAsync();
                if (responseEspecialidades.success && responseEspecialidades.data != null)
                {
                    _especialidades = responseEspecialidades.data;
                    EspecialidadPicker.ItemsSource = _especialidades;
                }

                // Cargar todos los médicos
                var responseMedicos = await _apiService.ObtenerTodosMedicosAsync();
                if (responseMedicos.success && responseMedicos.data != null)
                {
                    _medicos = responseMedicos.data;
                }

                // Cargar tipos de cita
                var responseTipos = await _apiService.ObtenerTiposCitaAsync();
                if (responseTipos.success && responseTipos.data != null)
                {
                    _tiposCita = responseTipos.data;
                    TipoCitaPicker.ItemsSource = _tiposCita;
                }

                // Establecer fecha mínima como hoy
                FechaCitaDatePicker.Date = DateTime.Today.AddDays(1); // Mínimo mañana
            }
            catch (Exception ex)
            {
                ShowMessage($"Error al cargar datos iniciales: {ex.Message}", false);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void OnBuscarPacienteClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CedulaPacienteEntry.Text))
            {
                ShowMessage("Por favor ingrese una cédula", false);
                return;
            }

            if (CedulaPacienteEntry.Text.Trim().Length != 10)
            {
                ShowMessage("La cédula debe tener 10 dígitos", false);
                return;
            }

            ShowLoading(true);
            LimpiarResultadosBusqueda();

            try
            {
                var response = await _apiService.BuscarPacientePorCedulaAsync(CedulaPacienteEntry.Text.Trim());

                if (response.success && response.data != null)
                {
                    // Paciente encontrado
                    _pacienteSeleccionado = response.data;
                    MostrarDatosPacienteEncontrado();
                    ShowMessage("✅ Paciente encontrado", true);
                }
                else
                {
                    // Paciente no encontrado
                    _pacienteSeleccionado = null;
                    MostrarPacienteNoEncontrado();
                    ShowMessage("⚠️ Paciente no encontrado", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error en la búsqueda: {ex.Message}", false);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void MostrarDatosPacienteEncontrado()
        {
            ResultadoBusquedaStack.Children.Clear();
            PacienteNoEncontradoStack.IsVisible = false;

            var pacienteFrame = new Frame
            {
                BackgroundColor = Color.FromHex("#d5edda"),
                CornerRadius = 8,
                Padding = 15,
                HasShadow = false
            };

            var pacienteStack = new StackLayout
            {
                Children =
                {
                    new Label
                    {
                        Text = "✅ PACIENTE ENCONTRADO",
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromHex("#155724"),
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = $"Nombre: {_pacienteSeleccionado.nombre_completo}",
                        FontSize = 14,
                        TextColor = Color.FromHex("#155724")
                    },
                    new Label
                    {
                        Text = $"Cédula: {_pacienteSeleccionado.cedula}",
                        FontSize = 14,
                        TextColor = Color.FromHex("#155724")
                    },
                    new Label
                    {
                        Text = $"Email: {_pacienteSeleccionado.correo}",
                        FontSize = 14,
                        TextColor = Color.FromHex("#155724")
                    },
                    new Label
                    {
                        Text = $"Teléfono: {_pacienteSeleccionado.telefono}",
                        FontSize = 14,
                        TextColor = Color.FromHex("#155724")
                    },
                    new Label
                    {
                        Text = $"Edad: {_pacienteSeleccionado.edad} años",
                        FontSize = 14,
                        TextColor = Color.FromHex("#155724")
                    }
                }
            };

            pacienteFrame.Content = pacienteStack;
            ResultadoBusquedaStack.Children.Add(pacienteFrame);
            ResultadoBusquedaStack.IsVisible = true;

            // Mostrar formulario de cita
            DatosCitaFrame.IsVisible = true;
            BotonesAccionStack.IsVisible = true;
        }

        private void MostrarPacienteNoEncontrado()
        {
            ResultadoBusquedaStack.IsVisible = false;
            PacienteNoEncontradoStack.IsVisible = true;
            DatosCitaFrame.IsVisible = false;
            BotonesAccionStack.IsVisible = false;
        }

        private void LimpiarResultadosBusqueda()
        {
            ResultadoBusquedaStack.IsVisible = false;
            PacienteNoEncontradoStack.IsVisible = false;
            DatosCitaFrame.IsVisible = false;
            BotonesAccionStack.IsVisible = false;
        }

        private void OnEspecialidadChanged(object sender, EventArgs e)
        {
            var especialidadSeleccionada = (Especialidad)EspecialidadPicker.SelectedItem;
            if (especialidadSeleccionada != null)
            {
                // Filtrar médicos por especialidad
                _medicosFiltrados = _medicos?.Where(m => m.id_especialidad == especialidadSeleccionada.id_especialidad).ToList();
                MedicoPicker.ItemsSource = _medicosFiltrados;
                MedicoPicker.Title = "Seleccione un médico";

                // Limpiar selecciones posteriores
                MedicoPicker.SelectedItem = null;
                SucursalPicker.ItemsSource = null;
                SucursalPicker.SelectedItem = null;
                SucursalPicker.Title = "Primero seleccione un médico";
                HoraDisponibleStack.IsVisible = false;
            }
        }

        private void OnMedicoChanged(object sender, EventArgs e)
        {
            var medicoSeleccionado = (MedicoCompleto)MedicoPicker.SelectedItem;
            if (medicoSeleccionado != null)
            {
                // Cargar sucursales del médico seleccionado
                _sucursalesDelMedico = medicoSeleccionado.sucursales;
                SucursalPicker.ItemsSource = _sucursalesDelMedico;
                SucursalPicker.Title = "Seleccione una sucursal";

                // Limpiar selecciones posteriores
                SucursalPicker.SelectedItem = null;
                HoraDisponibleStack.IsVisible = false;
            }
        }

        private void OnSucursalChanged(object sender, EventArgs e)
        {
            var sucursalSeleccionada = (Sucursal)SucursalPicker.SelectedItem;
            if (sucursalSeleccionada != null)
            {
                // Cargar horarios cuando cambie la fecha
                CargarHorariosDisponibles();
            }
        }

        private void OnFechaSelected(object sender, DateChangedEventArgs e)
        {
            if (MedicoPicker.SelectedItem != null && SucursalPicker.SelectedItem != null)
            {
                CargarHorariosDisponibles();
            }
        }

        private async void CargarHorariosDisponibles()
        {
            var medicoSeleccionado = (MedicoCompleto)MedicoPicker.SelectedItem;
            var sucursalSeleccionada = (Sucursal)SucursalPicker.SelectedItem;

            if (medicoSeleccionado == null || sucursalSeleccionada == null)
                return;

            LoadingHorariosIndicator.IsVisible = true;
            LoadingHorariosIndicator.IsRunning = true;

            try
            {
                // Primero intentar con la API detallada
                var responseDetallado = await _apiService.ObtenerHorariosDisponiblesDetalladosAsync(
                    medicoSeleccionado.id_doctor,
                    sucursalSeleccionada.id_sucursal,
                    FechaCitaDatePicker.Date
                );

                if (responseDetallado.success && responseDetallado.data != null)
                {
                    MostrarHorariosDetallados(responseDetallado.data);
                }
                else
                {
                    // Fallback: usar método simple con el nombre correcto
                    var response = await _apiService.ObtenerHorariosDisponiblesAsync(
                        medicoSeleccionado.id_doctor,
                        sucursalSeleccionada.id_sucursal,
                        FechaCitaDatePicker.Date
                    );

                    if (response.success && response.data != null && response.data.Count > 0)
                    {
                        HoraPicker.ItemsSource = response.data;
                        HoraPicker.Title = "Seleccione una hora";
                        HoraPicker.SelectedItem = null; // Limpiar selección anterior
                        HoraDisponibleStack.IsVisible = true;
                        ShowMessage($"✅ {response.data.Count} horarios disponibles encontrados", true);
                    }
                    else
                    {
                        ShowMessage("❌ No hay horarios disponibles para la fecha seleccionada", false);
                        HoraDisponibleStack.IsVisible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error al cargar horarios: {ex.Message}", false);
                HoraDisponibleStack.IsVisible = false;
            }
            finally
            {
                LoadingHorariosIndicator.IsVisible = false;
                LoadingHorariosIndicator.IsRunning = false;
            }
        }

        private void MostrarHorariosDetallados(HorariosDisponiblesResponse horarios)
        {
            // Limpiar el picker anterior
            HoraPicker.ItemsSource = null;

            if (horarios.horarios_disponibles == null || horarios.horarios_disponibles.Count == 0)
            {
                ShowMessage($"❌ {horarios.mensaje ?? "No hay horarios disponibles"}", false);
                HoraDisponibleStack.IsVisible = false;
                return;
            }

            // Filtrar solo horarios disponibles
            var horariosDisponibles = horarios.horarios_disponibles
                .Where(h => h.disponible)
                .Select(h => h.hora)
                .ToList();

            if (horariosDisponibles.Count > 0)
            {
                HoraPicker.ItemsSource = horariosDisponibles;
                HoraPicker.Title = "Seleccione una hora";
                HoraPicker.SelectedItem = null; // Limpiar selección anterior
                HoraDisponibleStack.IsVisible = true;

                ShowMessage($"✅ {horariosDisponibles.Count} horarios disponibles (duración: {horarios.duracion_cita_minutos}min)", true);
            }
            else
            {
                ShowMessage("❌ Todos los horarios están ocupados para esta fecha", false);
                HoraDisponibleStack.IsVisible = false;
            }
        }

        private void OnTipoCitaChanged(object sender, EventArgs e)
        {
            var tipoSeleccionado = (TipoCita)TipoCitaPicker.SelectedItem;
            if (tipoSeleccionado != null)
            {
                // Mostrar enlace virtual solo si es cita virtual
                EnlaceVirtualStack.IsVisible = tipoSeleccionado.nombre_tipo.ToLower().Contains("virtual");

                // Limpiar el enlace si cambia a presencial
                if (!tipoSeleccionado.nombre_tipo.ToLower().Contains("virtual"))
                {
                    EnlaceVirtualEntry.Text = "";
                }
            }
        }

        private async void OnAñadirPacienteClicked(object sender, EventArgs e)
        {
            // Navegar a registro de paciente con la cédula precargada
            await Shell.Current.GoToAsync($"registropaciente?cedula={CedulaPacienteEntry.Text}");
        }

        private async void OnCrearCitaClicked(object sender, EventArgs e)
        {
            if (!ValidarFormularioCita())
                return;

            ShowLoading(true);

            try
            {
                var medicoSeleccionado = (MedicoCompleto)MedicoPicker.SelectedItem;
                var sucursalSeleccionada = (Sucursal)SucursalPicker.SelectedItem;
                var tipoSeleccionado = (TipoCita)TipoCitaPicker.SelectedItem;
                var horaSeleccionada = (string)HoraPicker.SelectedItem;

                // Combinar fecha y hora
                var fechaHora = FechaCitaDatePicker.Date.Add(TimeSpan.Parse(horaSeleccionada));

                var nuevaCita = new CitaCreacion
                {
                    id_paciente = _pacienteSeleccionado.id_paciente,
                    id_doctor = medicoSeleccionado.id_doctor,
                    id_sucursal = sucursalSeleccionada.id_sucursal,
                    id_tipo_cita = tipoSeleccionado.id_tipo_cita,
                    fecha_hora = fechaHora,
                    motivo = MotivoEditor.Text?.Trim() ?? "",
                    tipo_cita = tipoSeleccionado.nombre_tipo.ToLower().Contains("virtual") ? "virtual" : "presencial",
                    notas = NotasEditor.Text?.Trim() ?? "",
                    enlace_virtual = EnlaceVirtualEntry.Text?.Trim() ?? ""
                };

                var response = await _apiService.CrearCitaAsync(nuevaCita);

                if (response.success)
                {
                    ShowMessage("✅ Cita creada exitosamente", true);
                    await DisplayAlert("Éxito", $"La cita ha sido programada para el {fechaHora:dd/MM/yyyy} a las {fechaHora:HH:mm}", "OK");
                    LimpiarFormulario();
                }
                else
                {
                    ShowMessage($"❌ Error al crear la cita: {response.message}", false);
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

        private bool ValidarFormularioCita()
        {
            if (_pacienteSeleccionado == null)
            {
                ShowMessage("❌ Debe buscar y seleccionar un paciente primero", false);
                return false;
            }

            if (EspecialidadPicker.SelectedItem == null)
            {
                ShowMessage("❌ Debe seleccionar una especialidad", false);
                return false;
            }

            if (MedicoPicker.SelectedItem == null)
            {
                ShowMessage("❌ Debe seleccionar un médico", false);
                return false;
            }

            if (SucursalPicker.SelectedItem == null)
            {
                ShowMessage("❌ Debe seleccionar una sucursal", false);
                return false;
            }

            if (HoraPicker.SelectedItem == null)
            {
                ShowMessage("❌ Debe seleccionar una hora disponible", false);
                return false;
            }

            if (TipoCitaPicker.SelectedItem == null)
            {
                ShowMessage("❌ Debe seleccionar el tipo de cita", false);
                return false;
            }

            if (string.IsNullOrWhiteSpace(MotivoEditor.Text))
            {
                ShowMessage("❌ Debe especificar el motivo de la consulta", false);
                return false;
            }

            // Validar enlace virtual si es necesario
            var tipoSeleccionado = (TipoCita)TipoCitaPicker.SelectedItem;
            if (tipoSeleccionado.nombre_tipo.ToLower().Contains("virtual") &&
                string.IsNullOrWhiteSpace(EnlaceVirtualEntry.Text))
            {
                ShowMessage("❌ Debe proporcionar un enlace virtual para citas virtuales", false);
                return false;
            }

            return true;
        }

        private void OnLimpiarFormClicked(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            // Limpiar búsqueda de paciente
            CedulaPacienteEntry.Text = "";
            _pacienteSeleccionado = null;
            LimpiarResultadosBusqueda();

            // Limpiar formulario de cita
            EspecialidadPicker.SelectedItem = null;
            MedicoPicker.SelectedItem = null;
            MedicoPicker.ItemsSource = null;
            MedicoPicker.Title = "Primero seleccione una especialidad";
            SucursalPicker.SelectedItem = null;
            SucursalPicker.ItemsSource = null;
            SucursalPicker.Title = "Primero seleccione un médico";
            FechaCitaDatePicker.Date = DateTime.Today.AddDays(1);
            HoraPicker.SelectedItem = null;
            HoraPicker.ItemsSource = null;
            TipoCitaPicker.SelectedItem = null;
            MotivoEditor.Text = "";
            NotasEditor.Text = "";
            EnlaceVirtualEntry.Text = "";

            // Ocultar secciones
            HoraDisponibleStack.IsVisible = false;
            EnlaceVirtualStack.IsVisible = false;
            MessageLabel.IsVisible = false;
        }

        private void ShowLoading(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
            BuscarPacienteButton.IsEnabled = !isLoading;
            CrearCitaButton.IsEnabled = !isLoading;
        }

        private async void ShowMessage(string message, bool isSuccess)
        {
            MessageLabel.Text = message;
            MessageLabel.TextColor = isSuccess ? Color.Green : Color.Red;
            MessageLabel.IsVisible = true;

            // Ocultar el mensaje después de 4 segundos
            await Task.Delay(4000);
            MessageLabel.IsVisible = false;
        }
    }
}