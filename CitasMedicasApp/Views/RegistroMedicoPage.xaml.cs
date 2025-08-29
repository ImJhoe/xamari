using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using CitasMedicasApp.Models;

namespace CitasMedicasApp.Views
{
    public partial class RegistroMedicoPage : ContentPage
    {
        private readonly ApiService _apiService;
        private List<Especialidad> _especialidades;
        private List<Sucursal> _sucursales;
        private List<HorarioControl> _horariosControles;

        public RegistroMedicoPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _horariosControles = new List<HorarioControl>();

            // ✅ CORRECCIÓN: Llamar después de InitializeComponent
            Device.BeginInvokeOnMainThread(async () =>
            {
                await CargarDatosIniciales();
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Device.BeginInvokeOnMainThread(async () =>
            {
                await CargarDatosIniciales();
            });
        }

        // ✅ MÉTODO CORREGIDO
        private async System.Threading.Tasks.Task CargarDatosIniciales()
        {
            ShowLoading(true);

            try
            {
                // ✅ CORRECCIÓN 1: Cargar especialidades
                var responseEspecialidades = await _apiService.ObtenerEspecialidadesAsync();
                if (responseEspecialidades.success && responseEspecialidades.data != null)
                {
                    _especialidades = responseEspecialidades.data;

                    // ✅ IMPORTANTE: Asignar en el hilo principal
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        EspecialidadPicker.ItemsSource = _especialidades;
                        EspecialidadPicker.ItemDisplayBinding = new Binding("nombre_especialidad");
                    });
                }
                else
                {
                    ShowMessage($"Error al cargar especialidades: {responseEspecialidades.message}", false);
                }

                // ✅ CORRECCIÓN 2: Cargar sucursales
                var responseSucursales = await _apiService.ObtenerSucursalesAsync();
                if (responseSucursales.success && responseSucursales.data != null)
                {
                    _sucursales = responseSucursales.data;

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        SucursalPicker.ItemsSource = _sucursales;
                        SucursalPicker.ItemDisplayBinding = new Binding("nombre_sucursal");
                    });
                }
                else
                {
                    ShowMessage($"Error al cargar sucursales: {responseSucursales.message}", false);
                }

                // ✅ CORRECCIÓN 3: Crear controles de horarios
                Device.BeginInvokeOnMainThread(() =>
                {
                    CrearControlesHorarios();
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Error al cargar datos: {ex.Message}", false);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // ✅ MÉTODO CORREGIDO - CrearControlesHorarios
        private void CrearControlesHorarios()
        {
            // Limpiar controles existentes
            HorariosStackLayout.Children.Clear();
            _horariosControles.Clear();

            var dias = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

            foreach (var dia in dias)
            {
                var horarioControl = new HorarioControl { Dia = dia };

                var frame = new Frame
                {
                    BackgroundColor = Color.White,
                    HasShadow = true,
                    CornerRadius = 8,
                    Padding = 15,
                    Margin = new Thickness(0, 5)
                };

                var stackLayout = new StackLayout { Spacing = 10 };

                // Header con checkbox y día
                var headerStack = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Spacing = 10
                };

                var checkbox = new CheckBox();
                horarioControl.CheckBox = checkbox;
                checkbox.CheckedChanged += (s, e) => OnDiaCheckedChanged(horarioControl, e.Value);

                var labelDia = new Label
                {
                    Text = dia,
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.FromHex("#34495e")
                };

                headerStack.Children.Add(checkbox);
                headerStack.Children.Add(labelDia);

                // Grid para horarios
                var horariosGrid = new Grid
                {
                    RowDefinitions = { new RowDefinition(), new RowDefinition() },
                    ColumnDefinitions = {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    },
                    IsVisible = false
                };
                horarioControl.HorariosGrid = horariosGrid;

                // Hora inicio
                var lblInicio = new Label { Text = "Hora Inicio:", FontSize = 12 };
                var timeInicio = new TimePicker { Time = new TimeSpan(8, 0, 0) };
                horarioControl.HoraInicio = timeInicio;

                // Hora fin
                var lblFin = new Label { Text = "Hora Fin:", FontSize = 12 };
                var timeFin = new TimePicker { Time = new TimeSpan(17, 0, 0) };
                horarioControl.HoraFin = timeFin;

                horariosGrid.Children.Add(lblInicio, 0, 0);
                horariosGrid.Children.Add(timeInicio, 0, 1);
                horariosGrid.Children.Add(lblFin, 1, 0);
                horariosGrid.Children.Add(timeFin, 1, 1);

                stackLayout.Children.Add(headerStack);
                stackLayout.Children.Add(horariosGrid);
                frame.Content = stackLayout;

                HorariosStackLayout.Children.Add(frame);
                _horariosControles.Add(horarioControl);
            }
        }

        private void OnDiaCheckedChanged(HorarioControl horarioControl, bool isChecked)
        {
            horarioControl.HorariosGrid.IsVisible = isChecked;
        }

        private void OnEspecialidadChanged(object sender, EventArgs e)
        {
            // ✅ CORRECCIÓN 1: Cargar especialidades
            var responseEspecialidades = await _apiService.ObtenerEspecialidadesAsync();
            if (responseEspecialidades.success && responseEspecialidades.data != null)
            {
                // Se cargan las especialidades...
            }
            else
            {
                ShowMessage($"Error al cargar especialidades: {responseEspecialidades.message}", false);
            }
        }

        // ✅ MÉTODO CORREGIDO - Validar Formulario
        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(CedulaEntry.Text))
            {
                ShowMessage("❌ La cédula es obligatoria", false);
                return false;
            }

            if (string.IsNullOrWhiteSpace(NombreEntry.Text))
            {
                ShowMessage("❌ El nombre es obligatorio", false);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ApellidoEntry.Text))
            {
                ShowMessage("❌ El apellido es obligatorio", false);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                ShowMessage("❌ El email es obligatorio", false);
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                ShowMessage("❌ La contraseña es obligatoria", false);
                return false;
            }

            if (EspecialidadPicker.SelectedItem == null)
            {
                ShowMessage("❌ Debe seleccionar una especialidad", false);
                return false;
            }

            if (SucursalPicker.SelectedItem == null)
            {
                ShowMessage("❌ Debe seleccionar una sucursal", false);
                return false;
            }

            return true;
        }

        // ✅ AGREGAR ESTOS MÉTODOS FALTANTES
        private void ShowLoading(bool show)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsVisible = show;
                LoadingIndicator.IsRunning = show;
                RegistrarButton.IsEnabled = !show;
                LimpiarButton.IsEnabled = !show;
            });
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert(isSuccess ? "Éxito" : "Error", message, "OK");
            });
        }

        private void LimpiarFormulario()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                CedulaEntry.Text = "";
                NombreEntry.Text = "";
                ApellidoEntry.Text = "";
                EmailEntry.Text = "";
                TelefonoEntry.Text = "";
                PasswordEntry.Text = "";
                EspecialidadPicker.SelectedItem = null;
                SucursalPicker.SelectedItem = null;

                // Limpiar horarios
                foreach (var control in _horariosControles)
                {
                    if (control.CheckBox != null)
                        control.CheckBox.IsChecked = false;
                    if (control.HorariosGrid != null)
                        control.HorariosGrid.IsVisible = false;
                }
            });
        }

        private async void OnRegistrarClicked(object sender, EventArgs e)
        {
            if (!ValidarFormulario())
                return;

            ShowLoading(true);

            try
            {
                // 1. Registrar médico
                var medico = new Usuario
                {
                    cedula = CedulaEntry.Text.Trim(),
                    nombre = NombreEntry.Text.Trim(),
                    apellido = ApellidoEntry.Text.Trim(),
                    email = EmailEntry.Text.Trim(),
                    telefono = TelefonoEntry.Text?.Trim(),
                    especialidad = ((Especialidad)EspecialidadPicker.SelectedItem)?.nombre_especialidad,
                    tipo_usuario = "medico"
                };

                var responseRegistro = await _apiService.RegistrarMedicoAsync(medico, PasswordEntry.Text);

                if (responseRegistro.success && responseRegistro.data != null)
                {
                    // 2. Asignar horarios
                    var horariosSeleccionados = ObtenerHorariosSeleccionados(responseRegistro.data.id);

                    if (horariosSeleccionados.Count > 0)
                    {
                        var responseHorarios = await _apiService.AsignarHorariosMedicoAsync(responseRegistro.data.id, horariosSeleccionados);

                        if (responseHorarios.success)
                        {
                            ShowMessage("✅ Médico registrado exitosamente con sus horarios asignados", true);
                            LimpiarFormulario();
                        }
                        else
                        {
                            ShowMessage($"⚠️ Médico registrado, pero error al asignar horarios: {responseHorarios.message}", false);
                        }
                    }
                    else
                    {
                        ShowMessage("✅ Médico registrado exitosamente (sin horarios asignados)", true);
                        LimpiarFormulario();
                    }
                }
                else
                {
                    ShowMessage($"❌ Error al registrar médico: {responseRegistro.message}", false);
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

        private List<HorarioMedico> ObtenerHorariosSeleccionados(int idMedico)
        {
            var horarios = new List<HorarioMedico>();
            var sucursalSeleccionada = (Sucursal)SucursalPicker.SelectedItem;

            if (sucursalSeleccionada == null) return horarios;

            foreach (var control in _horariosControles)
            {
                if (control.CheckBox != null && control.CheckBox.IsChecked)
                {
                    horarios.Add(new HorarioMedico
                    {
                        id_medico = idMedico,
                        id_sucursal = sucursalSeleccionada.id_sucursal,
                        dia_semana = control.Dia,
                        hora_inicio = control.HoraInicio?.Time.ToString(@"hh\:mm") ?? "08:00",
                        hora_fin = control.HoraFin?.Time.ToString(@"hh\:mm") ?? "17:00"
                    });
                }
            }

            return horarios;
        }
        private void OnLimpiarClicked(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }
    }

    // ✅ CLASE AUXILIAR
    public class HorarioControl
    {
        public string Dia { get; set; }
        public CheckBox CheckBox { get; set; }
        public Grid HorariosGrid { get; set; }
        public TimePicker HoraInicio { get; set; }
        public TimePicker HoraFin { get; set; }
    }

}
