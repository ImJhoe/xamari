using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Models;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditarHorariosPage : ContentPage
    {
        private readonly ApiService _apiService;
        private int _medicoId;
        private List<Horario> _horariosOriginales;
        private List<Horario> _horariosEditados;

        public EditarHorariosPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _horariosOriginales = new List<Horario>();
            _horariosEditados = new List<Horario>();
        }

        public EditarHorariosPage(int medicoId, string nombreMedico = "") : this()
        {
            _medicoId = medicoId;
            if (!string.IsNullOrEmpty(nombreMedico))
            {
                MedicoInfoLabel.Text = $"Dr(a). {nombreMedico}";
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarHorarios();
        }

        private async void CargarHorarios()
        {
            ShowLoading(true);

            try
            {
                // Aquí deberías llamar a tu API para obtener los horarios del médico
                // var response = await _apiService.ObtenerHorariosMedicoAsync(_medicoId);

                // Por ahora, datos de ejemplo:
                _horariosOriginales = new List<Horario>
                {
                    new Horario { Id = 1, DiaSemana = "Lunes", HoraInicio = TimeSpan.FromHours(8), HoraFin = TimeSpan.FromHours(12) },
                    new Horario { Id = 2, DiaSemana = "Lunes", HoraInicio = TimeSpan.FromHours(14), HoraFin = TimeSpan.FromHours(18) },
                    new Horario { Id = 3, DiaSemana = "Martes", HoraInicio = TimeSpan.FromHours(8), HoraFin = TimeSpan.FromHours(16) }
                };

                _horariosEditados = _horariosOriginales.Select(h => new Horario
                {
                    Id = h.Id,
                    DiaSemana = h.DiaSemana,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin
                }).ToList();

                MostrarHorariosEditables();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar horarios: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void MostrarHorariosEditables()
        {
            HorariosEditablesStack.Children.Clear();

            foreach (var horario in _horariosEditados)
            {
                var horarioFrame = CrearFrameHorarioEditable(horario);
                HorariosEditablesStack.Children.Add(horarioFrame);
            }
        }

        private Frame CrearFrameHorarioEditable(Horario horario)
        {
            var frame = new Frame
            {
                BackgroundColor = Color.White,
                HasShadow = true,
                CornerRadius = 10,
                Padding = 15,
                Margin = new Thickness(0, 5)
            };

            var mainStack = new StackLayout { Spacing = 10 };

            // Header con día
            var diaLabel = new Label
            {
                Text = horario.DiaSemana,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromHex("#2c3e50")
            };

            // Grid para las horas
            var horasGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(20, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            // Labels
            var inicioLabel = new Label { Text = "Hora Inicio:", FontSize = 12 };
            var finLabel = new Label { Text = "Hora Fin:", FontSize = 12 };

            // TimePickers
            var inicioTimePicker = new TimePicker
            {
                Time = horario.HoraInicio,
                FontSize = 14
            };
            inicioTimePicker.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == TimePicker.TimeProperty.PropertyName)
                {
                    horario.HoraInicio = inicioTimePicker.Time;
                }
            };

            var finTimePicker = new TimePicker
            {
                Time = horario.HoraFin,
                FontSize = 14
            };
            finTimePicker.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == TimePicker.TimeProperty.PropertyName)
                {
                    horario.HoraFin = finTimePicker.Time;
                }
            };

            var separadorLabel = new Label
            {
                Text = "a",
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.Center,
                FontSize = 12
            };

            // Agregar a grid
            horasGrid.Children.Add(inicioLabel, 0, 0);
            horasGrid.Children.Add(finLabel, 2, 0);
            horasGrid.Children.Add(inicioTimePicker, 0, 1);
            horasGrid.Children.Add(separadorLabel, 1, 1);
            horasGrid.Children.Add(finTimePicker, 2, 1);

            mainStack.Children.Add(diaLabel);
            mainStack.Children.Add(horasGrid);

            frame.Content = mainStack;
            return frame;
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            ShowLoading(true);

            try
            {
                // Validar horarios
                if (!ValidarHorarios())
                {
                    return;
                }

                // Aquí deberías llamar a tu API para guardar los cambios
                // var response = await _apiService.ActualizarHorariosMedicoAsync(_medicoId, _horariosEditados);

                await DisplayAlert("Éxito", "Horarios actualizados correctamente", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al guardar horarios: {ex.Message}", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert("Confirmar",
                "¿Estás seguro de que deseas cancelar? Se perderán los cambios no guardados.",
                "Sí", "No");

            if (confirmar)
            {
                await Navigation.PopAsync();
            }
        }

        private bool ValidarHorarios()
        {
            foreach (var horario in _horariosEditados)
            {
                if (horario.HoraInicio >= horario.HoraFin)
                {
                    DisplayAlert("Error de Validación",
                        $"En {horario.DiaSemana}: La hora de inicio debe ser menor que la hora de fin.",
                        "OK");
                    return false;
                }
            }
            return true;
        }

        private void ShowLoading(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
            GuardarButton.IsEnabled = !isLoading;
            CancelarButton.IsEnabled = !isLoading;
        }
    }

    // Clase modelo para Horario (agrégala si no existe)
    public class Horario
    {
        public int Id { get; set; }
        public string DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }
}