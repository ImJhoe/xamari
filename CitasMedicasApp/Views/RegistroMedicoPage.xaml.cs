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
            CargarDatosIniciales();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarDatosIniciales();
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
                    EspecialidadPicker.ItemDisplayBinding = new Binding("nombre_especialidad");
                }

                // Cargar sucursales
                var responseSucursales = await _apiService.ObtenerSucursalesAsync();
                if (responseSucursales.success && responseSucursales.data != null)
                {
                    _sucursales = responseSucursales.data;
                    SucursalPicker.ItemsSource = _sucursales;
                    SucursalPicker.ItemDisplayBinding = new Binding("nombre_sucursal");
                }

                // Crear controles de horarios
                CrearControlesHorarios();
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

        private void CrearControlesHorarios()
        {
            HorariosStackLayout.Children.Clear();
            _horariosControles.Clear();

            foreach (var dia in DiasSemana.Dias)
            {
                var horarioControl = new HorarioControl
                {
                    DiaSemana = dia.Key,
                    NombreDia = dia.Value
                };

                var frame = new Frame
                {
                    BackgroundColor = Color.FromHex("#ecf0f1"),
                    CornerRadius = 8,
                    Padding = 10,
                    Margin = new Thickness(0, 5)
                };

                var stackLayout = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    Spacing = 8
                };

                // Checkbox para habilitar el día
                var checkBox = new CheckBox
                {
                    IsChecked = false
                };
                checkBox.CheckedChanged += (s, e) => OnDiaCheckedChanged(horarioControl, e.Value);
                horarioControl.CheckBox = checkBox;

                var headerStack = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children = { checkBox, new Label { Text = dia.Value, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center } }
                };

                // Controles de horario
                var horariosGrid = new Grid
                {
                    RowDefinitions = { new RowDefinition(), new RowDefinition() },
                    ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
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
            // Aquí podrías filtrar sucursales según la especialidad si es necesario
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
                    telefono = TelefonoEntry.Text.Trim(),
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

            if (sucursalSeleccionada == null)
                return horarios;

            foreach (var control in _horariosControles)
            {
                if (control.CheckBox.IsChecked)
                {
                    horarios.Add(new HorarioMedico
                    {
                        id_medico = idMedico,
                        dia_semana = control.DiaSemana.ToString(),
                        hora_inicio = control.HoraInicio.Time.ToString(@"hh\:mm"),
                        hora_fin = control.HoraFin.Time.ToString(@"hh\:mm"),
                        activo = true
                    });
                }
            }

            return horarios;
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(CedulaEntry.Text) ||
                string.IsNullOrWhiteSpace(NombreEntry.Text) ||
                string.IsNullOrWhiteSpace(ApellidoEntry.Text) ||
                string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(TelefonoEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                ShowMessage("❌ Por favor complete todos los campos obligatorios", false);
                return false;
            }

            if (CedulaEntry.Text.Trim().Length != 10)
            {
                ShowMessage("❌ La cédula debe tener 10 dígitos", false);
                return false;
            }

            if (PasswordEntry.Text.Length < 6)
            {
                ShowMessage("❌ La contraseña debe tener al menos 6 caracteres", false);
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

        private void OnLimpiarClicked(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            CedulaEntry.Text = "";
            NombreEntry.Text = "";
            ApellidoEntry.Text = "";
            EmailEntry.Text = "";
            TelefonoEntry.Text = "";
            PasswordEntry.Text = "";
            EspecialidadPicker.SelectedItem = null;
            SucursalPicker.SelectedItem = null;

            foreach (var control in _horariosControles)
            {
                control.CheckBox.IsChecked = false;
                control.HorariosGrid.IsVisible = false;
                control.HoraInicio.Time = new TimeSpan(8, 0, 0);
                control.HoraFin.Time = new TimeSpan(17, 0, 0);
            }

            MessageLabel.IsVisible = false;
        }

        private void ShowLoading(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
            RegistrarButton.IsEnabled = !isLoading;
            LimpiarButton.IsEnabled = !isLoading;
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            MessageLabel.Text = message;
            MessageLabel.TextColor = isSuccess ? Color.Green : Color.Red;
            MessageLabel.IsVisible = true;
        }

        // Clase auxiliar para manejar controles de horarios
        private class HorarioControl
        {
            public int DiaSemana { get; set; }
            public string NombreDia { get; set; }
            public CheckBox CheckBox { get; set; }
            public Grid HorariosGrid { get; set; }
            public TimePicker HoraInicio { get; set; }
            public TimePicker HoraFin { get; set; }
        }
    }
}