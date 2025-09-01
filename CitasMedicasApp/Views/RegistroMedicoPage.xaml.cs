using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using CitasMedicasApp.Models;

namespace CitasMedicasApp.Views
{
    public partial class RegistroMedicoPage : ContentPage
    {
        private readonly ApiService _apiService;
        private List<Especialidad> _especialidades;
        private List<Sucursal> _sucursales;
        private List<HorarioDinamico> _horariosAgregados;
        private int _contadorHorarios = 0;

        public RegistroMedicoPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _horariosAgregados = new List<HorarioDinamico>();

            Device.BeginInvokeOnMainThread(async () =>
            {
                await CargarDatosIniciales();
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ActualizarContadorHorarios();
        }

        private async Task CargarDatosIniciales()
        {
            ShowLoading(true);

            try
            {
                // Cargar especialidades
                var responseEspecialidades = await _apiService.ObtenerEspecialidadesAsync();
                if (responseEspecialidades.success && responseEspecialidades.data != null)
                {
                    _especialidades = responseEspecialidades.data;

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        EspecialidadPicker.ItemsSource = _especialidades;
                        EspecialidadPicker.ItemDisplayBinding = new Binding("nombre_especialidad");
                        ShowMessage("✅ Especialidades cargadas correctamente", true);
                    });
                }
                else
                {
                    ShowMessage($"⚠️ Error al cargar especialidades: {responseEspecialidades.message}", false);
                }

                // Cargar sucursales
                var responseSucursales = await _apiService.ObtenerSucursalesAsync();
                if (responseSucursales.success && responseSucursales.data != null)
                {
                    _sucursales = responseSucursales.data;
                }
                else
                {
                    ShowMessage($"⚠️ Error al cargar sucursales: {responseSucursales.message}", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"❌ Error al cargar datos: {ex.Message}", false);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void OnAgregarHorarioClicked(object sender, EventArgs e)
        {
            if (_sucursales == null || _sucursales.Count == 0)
            {
                ShowMessage("❌ No hay sucursales disponibles", false);
                return;
            }

            // Crear el popup para agregar horario
            await MostrarPopupAgregarHorario();
        }

        private async Task MostrarPopupAgregarHorario()
        {
            var popup = new ContentPage
            {
                Title = "Agregar Horario",
                BackgroundColor = Color.FromHex("#f8f9fa")
            };

            var scrollView = new ScrollView();
            var mainStack = new StackLayout { Padding = 20, Spacing = 15 };

            // Título del popup
            var titulo = new Label
            {
                Text = "🕒 NUEVO HORARIO",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromHex("#2c3e50"),
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainStack.Children.Add(titulo);

            // Frame principal
            var frame = new Frame
            {
                BackgroundColor = Color.White,
                HasShadow = true,
                CornerRadius = 10,
                Padding = 20
            };

            var frameStack = new StackLayout { Spacing = 15 };

            // Sucursal
            var sucursalLabel = new Label { Text = "Sucursal:", FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#555") };
            var sucursalPicker = new Picker
            {
                Title = "Seleccione sucursal",
                ItemsSource = _sucursales,
                ItemDisplayBinding = new Binding("nombre_sucursal")
            };

            // Día de la semana
            var diaLabel = new Label { Text = "Día de la Semana:", FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#555") };
            var diasSemana = new List<DiasSemanaItem>
    {
        new DiasSemanaItem { Numero = 1, Nombre = "Lunes" },
        new DiasSemanaItem { Numero = 2, Nombre = "Martes" },
        new DiasSemanaItem { Numero = 3, Nombre = "Miércoles" },
        new DiasSemanaItem { Numero = 4, Nombre = "Jueves" },
        new DiasSemanaItem { Numero = 5, Nombre = "Viernes" },
        new DiasSemanaItem { Numero = 6, Nombre = "Sábado" },
        new DiasSemanaItem { Numero = 7, Nombre = "Domingo" }
    };
            var diaPicker = new Picker
            {
                Title = "Seleccione día",
                ItemsSource = diasSemana,
                ItemDisplayBinding = new Binding("Nombre")
            };

            // Hora inicio
            var horaInicioLabel = new Label { Text = "Hora Inicio:", FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#555") };
            var horaInicioPicker = new TimePicker { Time = new TimeSpan(8, 0, 0) };

            // Hora fin
            var horaFinLabel = new Label { Text = "Hora Fin:", FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#555") };
            var horaFinPicker = new TimePicker { Time = new TimeSpan(17, 0, 0) };

            // Duración de cita
            var duracionLabel = new Label { Text = "Duración por Cita (minutos):", FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#555") };
            var duracionPicker = new Picker
            {
                Title = "Seleccione duración",
                ItemsSource = new List<int> { 15, 20, 30, 45, 60 },
                SelectedItem = 30
            };

            // Agregar elementos al frame
            frameStack.Children.Add(sucursalLabel);
            frameStack.Children.Add(sucursalPicker);
            frameStack.Children.Add(diaLabel);
            frameStack.Children.Add(diaPicker);
            frameStack.Children.Add(horaInicioLabel);
            frameStack.Children.Add(horaInicioPicker);
            frameStack.Children.Add(horaFinLabel);
            frameStack.Children.Add(horaFinPicker);
            frameStack.Children.Add(duracionLabel);
            frameStack.Children.Add(duracionPicker);

            frame.Content = frameStack;
            mainStack.Children.Add(frame);

            // Botones
            var botonesStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 20,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var guardarButton = new Button
            {
                Text = "✅ GUARDAR HORARIO",
                BackgroundColor = Color.FromHex("#27ae60"),
                TextColor = Color.White,
                CornerRadius = 25,
                Padding = new Thickness(20, 10),
                FontAttributes = FontAttributes.Bold
            };

            var cancelarButton = new Button
            {
                Text = "❌ CANCELAR",
                BackgroundColor = Color.FromHex("#e74c3c"),
                TextColor = Color.White,
                CornerRadius = 25,
                Padding = new Thickness(20, 10),
                FontAttributes = FontAttributes.Bold
            };

            guardarButton.Clicked += async (s, args) =>
            {
                if (ValidarDatosHorario(sucursalPicker, diaPicker, horaInicioPicker, horaFinPicker, duracionPicker))
                {
                    var nuevoHorario = new HorarioDinamico
                    {
                        Id = ++_contadorHorarios,
                        Sucursal = (Sucursal)sucursalPicker.SelectedItem,
                        DiaSemanaNumero = ((DiasSemanaItem)diaPicker.SelectedItem).Numero,
                        DiaSemanaNombre = ((DiasSemanaItem)diaPicker.SelectedItem).Nombre,
                        HoraInicio = horaInicioPicker.Time,
                        HoraFin = horaFinPicker.Time,
                        DuracionCita = (int)duracionPicker.SelectedItem
                    };

                    _horariosAgregados.Add(nuevoHorario);
                    AgregarHorarioALista(nuevoHorario);
                    ActualizarContadorHorarios();

                    // CAMBIO CRÍTICO: Usar PopModalAsync en lugar de PopAsync
                    await Navigation.PopModalAsync();
                    ShowMessage($"✅ Horario agregado: {nuevoHorario.DiaSemanaNombre} {nuevoHorario.HoraInicio:hh\\:mm} - {nuevoHorario.HoraFin:hh\\:mm}", true);
                }
            };

            cancelarButton.Clicked += async (s, args) =>
            {
                // CAMBIO CRÍTICO: Usar PopModalAsync en lugar de PopAsync
                await Navigation.PopModalAsync();
            };

            botonesStack.Children.Add(guardarButton);
            botonesStack.Children.Add(cancelarButton);
            mainStack.Children.Add(botonesStack);

            scrollView.Content = mainStack;
            popup.Content = scrollView;

            // CAMBIO CRÍTICO: Usar PushModalAsync en lugar de PushAsync
            await Navigation.PushModalAsync(popup);
        }
        private bool ValidarDatosHorario(Picker sucursalPicker, Picker diaPicker, TimePicker horaInicioPicker, TimePicker horaFinPicker, Picker duracionPicker)
        {
            if (sucursalPicker.SelectedItem == null)
            {
                DisplayAlert("Error", "Debe seleccionar una sucursal", "OK");
                return false;
            }

            if (diaPicker.SelectedItem == null)
            {
                DisplayAlert("Error", "Debe seleccionar un día", "OK");
                return false;
            }

            if (horaInicioPicker.Time >= horaFinPicker.Time)
            {
                DisplayAlert("Error", "La hora de inicio debe ser menor que la hora de fin", "OK");
                return false;
            }

            if (duracionPicker.SelectedItem == null)
            {
                DisplayAlert("Error", "Debe seleccionar la duración de cita", "OK");
                return false;
            }

            // Verificar si ya existe un horario similar
            var sucursal = (Sucursal)sucursalPicker.SelectedItem;
            var dia = ((DiasSemanaItem)diaPicker.SelectedItem).Numero;

            var existeConflicto = _horariosAgregados.Any(h =>
                h.Sucursal.id_sucursal == sucursal.id_sucursal &&
                h.DiaSemanaNumero == dia &&
                ((horaInicioPicker.Time >= h.HoraInicio && horaInicioPicker.Time < h.HoraFin) ||
                 (horaFinPicker.Time > h.HoraInicio && horaFinPicker.Time <= h.HoraFin) ||
                 (horaInicioPicker.Time <= h.HoraInicio && horaFinPicker.Time >= h.HoraFin))
            );

            if (existeConflicto)
            {
                DisplayAlert("Error", "Ya existe un horario que se superpone con este en la misma sucursal y día", "OK");
                return false;
            }

            return true;
        }

        private void AgregarHorarioALista(HorarioDinamico horario)
        {
            var horarioFrame = new Frame
            {
                BackgroundColor = Color.FromHex("#ecf0f1"),
                HasShadow = true,
                CornerRadius = 8,
                Padding = 15,
                Margin = new Thickness(0, 5)
            };

            var horarioStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            var infoStack = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Spacing = 2
            };

            var tituloLabel = new Label
            {
                Text = $"🏥 {horario.Sucursal.nombre_sucursal}",
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Color.FromHex("#2c3e50")
            };

            var diaHoraLabel = new Label
            {
                Text = $"📅 {horario.DiaSemanaNombre}: {horario.HoraInicio:hh\\:mm} - {horario.HoraFin:hh\\:mm}",
                FontSize = 12,
                TextColor = Color.FromHex("#34495e")
            };

            var duracionLabel = new Label
            {
                Text = $"⏱️ Duración: {horario.DuracionCita} min",
                FontSize = 11,
                TextColor = Color.FromHex("#7f8c8d")
            };

            var eliminarButton = new Button
            {
                Text = "🗑️",
                BackgroundColor = Color.FromHex("#e74c3c"),
                TextColor = Color.White,
                CornerRadius = 15,
                WidthRequest = 40,
                HeightRequest = 40,
                FontSize = 16,
                CommandParameter = horario
            };

            eliminarButton.Clicked += OnEliminarHorarioClicked;

            infoStack.Children.Add(tituloLabel);
            infoStack.Children.Add(diaHoraLabel);
            infoStack.Children.Add(duracionLabel);

            horarioStack.Children.Add(infoStack);
            horarioStack.Children.Add(eliminarButton);

            horarioFrame.Content = horarioStack;
            HorariosListaStack.Children.Add(horarioFrame);
        }

        private void OnEliminarHorarioClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var horario = button.CommandParameter as HorarioDinamico;

            if (horario != null)
            {
                _horariosAgregados.Remove(horario);

                // Remover el frame visual
                var frameARemover = HorariosListaStack.Children
                    .OfType<Frame>()
                    .FirstOrDefault(f =>
                    {
                        var stack = f.Content as StackLayout;
                        var eliminarBtn = stack?.Children?.OfType<Button>()?.FirstOrDefault();
                        return eliminarBtn?.CommandParameter == horario;
                    });

                if (frameARemover != null)
                {
                    HorariosListaStack.Children.Remove(frameARemover);
                }

                ActualizarContadorHorarios();
                ShowMessage($"❌ Horario eliminado: {horario.DiaSemanaNombre}", false);
            }
        }

        private void ActualizarContadorHorarios()
        {
            ContadorHorariosLabel.Text = $"{_horariosAgregados.Count} horario(s) agregado(s)";
            ContadorHorariosLabel.TextColor = _horariosAgregados.Count > 0 ? Color.FromHex("#27ae60") : Color.FromHex("#7f8c8d");
        }

        private async void OnRegistrarMedicoClicked(object sender, EventArgs e)
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
                    // 2. Asignar horarios si existen
                    if (_horariosAgregados.Count > 0)
                    {
                        await AsignarHorariosMedico(responseRegistro.data.id);
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

        private async Task AsignarHorariosMedico(int idMedico)
        {
            int horariosExitosos = 0;
            int horariosConError = 0;

            foreach (var horario in _horariosAgregados)
            {
                try
                {
                    var horarioMedico = new HorarioMedico
                    {
                        id_medico = idMedico,
                        id_sucursal = horario.Sucursal.id_sucursal,
                        dia_semana = horario.DiaSemanaNumero, // ✅ CORRECTO: int en lugar de string
                        hora_inicio = horario.HoraInicio,     // ✅ CORRECTO: TimeSpan directo
                        hora_fin = horario.HoraFin,           // ✅ CORRECTO: TimeSpan directo
                        duracion_consulta = horario.DuracionCita // ✅ CORRECTO: usar duracion_consulta
                    };

                    var responseHorario = await _apiService.AsignarHorarioIndividualAsync(horarioMedico);

                    if (responseHorario.success)
                    {
                        horariosExitosos++;
                    }
                    else
                    {
                        horariosConError++;
                        System.Diagnostics.Debug.WriteLine($"Error asignando horario: {responseHorario.message}");
                    }
                }
                catch (Exception ex)
                {
                    horariosConError++;
                    System.Diagnostics.Debug.WriteLine($"Excepción asignando horario: {ex.Message}");
                }
            }

            if (horariosConError == 0)
            {
                ShowMessage($"✅ Médico registrado con {horariosExitosos} horarios asignados exitosamente", true);
            }
            else
            {
                ShowMessage($"⚠️ Médico registrado. {horariosExitosos} horarios OK, {horariosConError} con errores", false);
            }

            LimpiarFormulario();
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(CedulaEntry.Text))
            {
                ShowMessage("❌ La cédula es obligatoria", false);
                return false;
            }

            if (CedulaEntry.Text.Length != 10)
            {
                ShowMessage("❌ La cédula debe tener 10 dígitos", false);
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

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text) || PasswordEntry.Text.Length < 6)
            {
                ShowMessage("❌ La contraseña debe tener al menos 6 caracteres", false);
                return false;
            }

            if (EspecialidadPicker.SelectedItem == null)
            {
                ShowMessage("❌ Debe seleccionar una especialidad", false);
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
            TituloProfesionalEntry.Text = "";
            EspecialidadPicker.SelectedItem = null;

            // Limpiar horarios
            _horariosAgregados.Clear();
            HorariosListaStack.Children.Clear();
            _contadorHorarios = 0;
            ActualizarContadorHorarios();

            MessageFrame.IsVisible = false;
        }

        private void ShowLoading(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
            RegistrarButton.IsEnabled = !isLoading;
            AgregarHorarioButton.IsEnabled = !isLoading;
        }

        private async void ShowMessage(string message, bool isSuccess)
        {
            MessageLabel.Text = message;
            MessageLabel.TextColor = isSuccess ? Color.Green : Color.Red;
            MessageFrame.IsVisible = true;

            await Task.Delay(4000);
            MessageFrame.IsVisible = false;
        }
    }

    // ============ MODELOS AUXILIARES ============
    public class HorarioDinamico
    {
        public int Id { get; set; }
        public Sucursal Sucursal { get; set; }
        public int DiaSemanaNumero { get; set; }
        public string DiaSemanaNombre { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public int DuracionCita { get; set; }
    }

    public class DiasSemanaItem
    {
        public int Numero { get; set; }
        public string Nombre { get; set; }
    }
}