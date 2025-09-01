using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using CitasMedicasApp.Models;

namespace CitasMedicasApp.Views
{
    public partial class ConsultarMedicoPage : ContentPage
    {
        private readonly ApiService _apiService;
        private List<MedicoCompleto> _todosMedicos;
        private List<MedicoCompleto> _medicosFiltrados;

        public ConsultarMedicoPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _todosMedicos = new List<MedicoCompleto>();
            _medicosFiltrados = new List<MedicoCompleto>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarMedicos();
        }

        private async void CargarMedicos()
        {
            ShowLoading(true);
            NoDataLabel.IsVisible = false;

            try
            {
                var response = await _apiService.ObtenerTodosMedicosAsync();

                if (response.success && response.data != null)
                {
                    _todosMedicos = response.data;
                    _medicosFiltrados = new List<MedicoCompleto>(_todosMedicos);

                    // Cargar horarios para cada médico
                    foreach (var medico in _medicosFiltrados)
                    {
                        await CargarHorariosMedico(medico);
                    }

                    MostrarMedicos();
                }
                else
                {
                    ShowMessage($"Error al cargar médicos: {response.message}", false);
                    NoDataLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error de conexión: {ex.Message}", false);
                NoDataLabel.IsVisible = true;
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // En ConsultarMedicoPage.xaml.cs, método CargarHorariosMedico
        private async Task CargarHorariosMedico(MedicoCompleto medico)
        {
            try
            {
                var response = await _apiService.ObtenerHorariosMedicoDetalladosAsync(medico.id_doctor);
                if (response.success && response.data != null)
                {
                    // Convertir strings a TimeSpan si es necesario
                    foreach (var horario in response.data)
                    {
                        // Si la API devuelve strings, convertirlas
                        if (horario.hora_inicio == default(TimeSpan) && !string.IsNullOrEmpty(horario.hora_inicio.ToString()))
                        {
                            // La conversión ya está hecha si usas TimeSpan en el modelo
                        }
                    }
                    medico.horarios = response.data;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando horarios del médico {medico.id_doctor}: {ex.Message}");
                medico.horarios = new List<HorarioMedicoDetallado>(); // Lista vacía para evitar null reference
            }
        }

        private void MostrarMedicos()
        {
            MedicosStackLayout.Children.Clear();

            if (_medicosFiltrados == null || _medicosFiltrados.Count == 0)
            {
                NoDataLabel.IsVisible = true;
                return;
            }

            foreach (var medico in _medicosFiltrados)
            {
                var medicoFrame = CrearTarjetaMedico(medico);
                MedicosStackLayout.Children.Add(medicoFrame);
            }
        }

        private Frame CrearTarjetaMedico(MedicoCompleto medico)
        {
            var frame = new Frame
            {
                BackgroundColor = Color.White,
                HasShadow = true,
                CornerRadius = 12,
                Padding = 15,
                Margin = new Thickness(0, 5)
            };

            var mainStack = new StackLayout { Spacing = 12 };

            // Header del médico
            var headerStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    new Label
                    {
                        Text = "👨‍⚕️",
                        FontSize = 30,
                        VerticalOptions = LayoutOptions.Center
                    },
                    new StackLayout
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Children =
                        {
                            new Label
                            {
                                Text = medico.nombre_completo,
                                FontSize = 18,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromHex("#2c3e50")
                            },
                            new Label
                            {
                                Text = $"Cédula: {medico.cedula}",
                                FontSize = 14,
                                TextColor = Color.FromHex("#7f8c8d")
                            },
                            new Label
                            {
                                Text = $"Especialidad: {medico.nombre_especialidad}",
                                FontSize = 14,
                                TextColor = Color.FromHex("#3498db"),
                                FontAttributes = FontAttributes.Bold
                            }
                        }
                    },
                    new StackLayout
                    {
                        Children =
                        {
                            new Label
                            {
                                Text = medico.activo ? "🟢 Activo" : "🔴 Inactivo",
                                FontSize = 12,
                                HorizontalOptions = LayoutOptions.End
                            }
                        }
                    }
                }
            };

            mainStack.Children.Add(headerStack);

            // Información de contacto
            var contactoStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 15,
                Children =
                {
                    new Label
                    {
                        Text = $"📧 {medico.email}",
                        FontSize = 12,
                        TextColor = Color.FromHex("#555")
                    },
                    new Label
                    {
                        Text = $"📱 {medico.telefono}",
                        FontSize = 12,
                        TextColor = Color.FromHex("#555")
                    }
                }
            };
            mainStack.Children.Add(contactoStack);

            // Sección de horarios
            if (medico.horarios != null && medico.horarios.Count > 0)
            {
                var horariosLabel = new Label
                {
                    Text = "📅 HORARIOS DE ATENCIÓN:",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromHex("#34495e"),
                    Margin = new Thickness(0, 10, 0, 5)
                };
                mainStack.Children.Add(horariosLabel);

                var horariosFrame = new Frame
                {
                    BackgroundColor = Color.FromHex("#ecf0f1"),
                    CornerRadius = 8,
                    Padding = 10,
                    HasShadow = false
                };

                var horariosStack = new StackLayout { Spacing = 5 };

                var horariosAgrupados = medico.horarios
                    .Where(h => h.activo)
                    .GroupBy(h => h.id_sucursal)
                    .ToList();

                foreach (var sucursalGroup in horariosAgrupados)
                {
                    var sucursalLabel = new Label
                    {
                        Text = $"🏥 {sucursalGroup.First().nombre_sucursal}:",
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 12,
                        TextColor = Color.FromHex("#2c3e50")
                    };
                    horariosStack.Children.Add(sucursalLabel);

                    foreach (var horario in sucursalGroup.OrderBy(h => h.dia_semana))
                    {
                        var horarioLabel = new Label
                        {
                            Text = $"   • {DiasSemana.Dias[horario.dia_semana]}: {horario.hora_inicio} - {horario.hora_fin} ({horario.duracion_cita}min)",
                            FontSize = 11,
                            TextColor = Color.FromHex("#555"),
                            Margin = new Thickness(10, 0, 0, 0)
                        };
                        horariosStack.Children.Add(horarioLabel);
                    }
                }

                horariosFrame.Content = horariosStack;
                mainStack.Children.Add(horariosFrame);
            }
            else
            {
                var noHorariosLabel = new Label
                {
                    Text = "⚠️ Sin horarios asignados",
                    FontSize = 12,
                    TextColor = Color.FromHex("#e74c3c"),
                    FontAttributes = FontAttributes.Italic
                };
                mainStack.Children.Add(noHorariosLabel);
            }

            // Botones de acción
            var botonesStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.End,
                Spacing = 10,
                Margin = new Thickness(0, 10, 0, 0),
                Children =
                {
                    new Button
                    {
                        Text = "Ver Detalles",
                        BackgroundColor = Color.FromHex("#3498db"),
                        TextColor = Color.White,
                        CornerRadius = 15,
                        FontSize = 12,
                        Padding = new Thickness(15, 8),
                        CommandParameter = medico
                    },
                    new Button
                    {
                        Text = "Editar Horarios",
                        BackgroundColor = Color.FromHex("#f39c12"),
                        TextColor = Color.White,
                        CornerRadius = 15,
                        FontSize = 12,
                        Padding = new Thickness(15, 8),
                        CommandParameter = medico
                    }
                }
            };

            // Asignar eventos a los botones
            ((Button)botonesStack.Children[0]).Clicked += (s, e) => OnVerDetallesClicked(medico);
            ((Button)botonesStack.Children[1]).Clicked += (s, e) => OnEditarHorariosClicked(medico);

            mainStack.Children.Add(botonesStack);
            frame.Content = mainStack;

            return frame;
        }

        private async void OnVerDetallesClicked(MedicoCompleto medico)
        {
            await DisplayAlert("Detalles del Médico",
                $"Nombre: {medico.nombre_completo}\n" +
                $"Cédula: {medico.cedula}\n" +
                $"Email: {medico.email}\n" +
                $"Teléfono: {medico.telefono}\n" +
                $"Especialidad: {medico.nombre_especialidad}\n" +
                $"Estado: {(medico.activo ? "Activo" : "Inactivo")}\n" +
                $"Total Horarios: {medico.horarios?.Count ?? 0}",
                "Cerrar");
        }

        private async void OnEditarHorariosClicked(MedicoCompleto medico)
        {
            await Shell.Current.GoToAsync($"editarhorarios?medicoId={medico.id_doctor}&nombreMedico={medico.nombre_completo}");
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                _medicosFiltrados = new List<MedicoCompleto>(_todosMedicos);
            }
            else
            {
                var textoBusqueda = e.NewTextValue.ToLower();
                _medicosFiltrados = _todosMedicos.Where(m =>
                    m.nombre_completo.ToLower().Contains(textoBusqueda) ||
                    m.cedula.Contains(textoBusqueda) ||
                    m.email.ToLower().Contains(textoBusqueda) ||
                    m.nombre_especialidad.ToLower().Contains(textoBusqueda)
                ).ToList();
            }

            MostrarMedicos();
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            CargarMedicos();
        }

        private void ShowLoading(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
        }

        private async void ShowMessage(string message, bool isSuccess)
        {
            MessageLabel.Text = message;
            MessageLabel.TextColor = isSuccess ? Color.Green : Color.Red;
            MessageFrame.IsVisible = true;

            // Ocultar el mensaje después de 3 segundos
            await Task.Delay(3000);
            MessageFrame.IsVisible = false;
        }
    }
}