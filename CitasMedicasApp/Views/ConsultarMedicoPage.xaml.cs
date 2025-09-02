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

        private async Task CargarMedicos()
        {
            ShowLoading(true);
            NoDataLabel.IsVisible = false;
            MedicosStackLayout.Children.Clear();

            try
            {
                var response = await _apiService.ObtenerTodosMedicosAsync();
                if (response.success && response.data != null && response.data.Any())
                {
                    _todosMedicos = response.data;
                    _medicosFiltrados = new List<MedicoCompleto>(_todosMedicos);

                    // Cargar horarios para cada médico
                    foreach (var medico in _todosMedicos)
                    {
                        try
                        {
                            await CargarHorariosMedico(medico);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error cargando horarios para médico {medico.id_doctor}: {ex.Message}");
                            // Asegurarse de que horarios no sea null
                            if (medico.horarios == null)
                            {
                                medico.horarios = new List<HorarioMedicoDetallado>();
                            }
                        }
                    }

                    MostrarMedicos();
                }
                else
                {
                    ShowMessage($"Error al cargar médicos: {response?.message ?? "Sin datos"}", false);
                    NoDataLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando médicos: {ex.Message}");
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
                if (medico?.id_doctor == null)
                {
                    medico.horarios = new List<HorarioMedicoDetallado>();
                    return;
                }

                var response = await _apiService.ObtenerHorariosMedicoAsync(medico.id_doctor);
                if (response.success && response.data != null)
                {
                    // Convertir a HorarioMedicoDetallado
                    medico.horarios = response.data.Select(h => new HorarioMedicoDetallado
                    {
                        id_horario = h.id_horario,
                        id_doctor = h.id_medico,
                        dia_semana = h.dia_semana,
                        hora_inicio = h.hora_inicio,
                        hora_fin = h.hora_fin,
                        duracion_cita = h.duracion_consulta,
                        nombre_sucursal = h.nombre_sucursal ?? "Sucursal N/A",
                        // 🔥 CORREGIDO: usar nombre_dia del API o calcular desde dia_semana
                        nombre_dia = GetNombreDia(h.dia_semana)
                    }).ToList();

                    System.Diagnostics.Debug.WriteLine($"Médico {medico.nombre_completo} tiene {medico.horarios.Count} horarios cargados");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Error cargando horarios: {response?.message}");
                    medico.horarios = new List<HorarioMedicoDetallado>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando horarios para médico {medico?.nombre_completo}: {ex.Message}");
                medico.horarios = new List<HorarioMedicoDetallado>();
            }
        }

        // 🔥 AGREGAR ESTE MÉTODO HELPER AL FINAL DE LA CLASE
        private string GetNombreDia(int dia)
        {
            return dia switch
            {
                1 => "Lunes",
                2 => "Martes",
                3 => "Miércoles",
                4 => "Jueves",
                5 => "Viernes",
                6 => "Sábado",
                7 => "Domingo",
                _ => "Desconocido"
            };
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
                        Text = medico.nombre_completo ?? $"{medico.nombre} {medico.apellido}",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromHex("#2c3e50")
                    },
                    new Label
                    {
                        Text = $"🆔 Cédula: {medico.cedula ?? "N/A"}",
                        FontSize = 12,
                        TextColor = Color.FromHex("#7f8c8d")
                    }
                }
            },
            new Label
            {
                Text = medico.activo ? "✅" : "❌",
                FontSize = 20,
                VerticalOptions = LayoutOptions.Center
            }
        }
            };
            mainStack.Children.Add(headerStack);

            // Información del médico
            var infoStack = new StackLayout
            {
                Spacing = 5,
                Children =
        {
            new Label
            {
                Text = $"📧 Email: {medico.email ?? "No especificado"}",
                FontSize = 14,
                TextColor = Color.FromHex("#34495e")
            },
            new Label
            {
                Text = $"📞 Teléfono: {medico.telefono ?? "No especificado"}",
                FontSize = 14,
                TextColor = Color.FromHex("#34495e")
            },
            new Label
            {
                Text = $"🩺 Especialidad: {medico.nombre_especialidad ?? "No especificada"}",
                FontSize = 14,
                TextColor = Color.FromHex("#3498db"),
                FontAttributes = FontAttributes.Bold
            }
        }
            };
            mainStack.Children.Add(infoStack);

            // Información de horarios
            var horariosCount = medico.horarios?.Count ?? 0;
            var horariosLabel = new Label
            {
                Text = horariosCount > 0
                    ? $"🕒 Horarios asignados: {horariosCount}"
                    : "⚠️ Sin horarios asignados",
                FontSize = 14,
                TextColor = horariosCount > 0 ? Color.FromHex("#27ae60") : Color.FromHex("#e74c3c"),
                FontAttributes = FontAttributes.Bold
            };
            mainStack.Children.Add(horariosLabel);

            // Botones de acción
            var botonesStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10,
                HorizontalOptions = LayoutOptions.Center,
                Children =
        {
            new Button
            {
                Text = "👁️ Ver Detalles",
                BackgroundColor = Color.FromHex("#3498db"),
                TextColor = Color.White,
                CornerRadius = 20,
                FontSize = 12,
                Padding = new Thickness(15, 8),
                CommandParameter = medico
            },
            new Button
            {
                Text = "🕒 Editar Horarios",
                BackgroundColor = Color.FromHex("#f39c12"),
                TextColor = Color.White,
                CornerRadius = 20,
                FontSize = 12,
                Padding = new Thickness(15, 8),
                CommandParameter = medico
            }
        }
            };

            // Asignar eventos a los botones
            ((Button)botonesStack.Children[0]).Clicked += (s, e) =>
            {
                var btn = s as Button;
                var medicoParam = btn.CommandParameter as MedicoCompleto;
                OnVerDetallesClicked(medicoParam);
            };

            ((Button)botonesStack.Children[1]).Clicked += (s, e) =>
            {
                var btn = s as Button;
                var medicoParam = btn.CommandParameter as MedicoCompleto;
                OnEditarHorariosClicked(medicoParam);
            };

            mainStack.Children.Add(botonesStack);
            frame.Content = mainStack;

            return frame;
        }

        private async void OnVerDetallesClicked(MedicoCompleto medico)
        {
            if (medico == null) return;

            var horariosTexto = "";
            if (medico.horarios != null && medico.horarios.Any())
            {
                horariosTexto = "\n\n📋 HORARIOS:\n";
                foreach (var horario in medico.horarios.Take(5)) // Mostrar máximo 5 horarios
                {
                    horariosTexto += $"• {horario.nombre_dia ?? "Día N/A"}: {horario.hora_inicio:hh\\:mm} - {horario.hora_fin:hh\\:mm} ({horario.nombre_sucursal ?? "Sucursal N/A"})\n";
                }
                if (medico.horarios.Count > 5)
                {
                    horariosTexto += $"... y {medico.horarios.Count - 5} horarios más";
                }
            }
            else
            {
                horariosTexto = "\n\n⚠️ Sin horarios asignados";
            }

            await DisplayAlert(
                $"👨‍⚕️ {medico.nombre_completo}",
                $"🆔 Cédula: {medico.cedula ?? "N/A"}\n" +
                $"📧 Email: {medico.email ?? "No especificado"}\n" +
                $"📞 Teléfono: {medico.telefono ?? "No especificado"}\n" +
                $"🩺 Especialidad: {medico.nombre_especialidad ?? "No especificada"}\n" +
                $"📊 Estado: {(medico.activo ? "Activo" : "Inactivo")}\n" +
                $"🕒 Total Horarios: {medico.horarios?.Count ?? 0}" +
                horariosTexto,
                "Cerrar");
        }

        private async void OnEditarHorariosClicked(MedicoCompleto medico)
        {
            try
            {
                if (medico?.id_doctor == null)
                {
                    await DisplayAlert("Error", "ID del médico no válido", "OK");
                    return;
                }

                // Usar la propiedad calculada directamente (no asignar)
                var nombreCompleto = medico.nombre_completo;
                if (string.IsNullOrEmpty(nombreCompleto))
                {
                    nombreCompleto = $"{medico.nombre} {medico.apellido}".Trim();
                }

                // Usar PushModalAsync en lugar de Shell navigation
                var editarHorariosPage = new EditarHorariosPage(medico.id_doctor, nombreCompleto);
                await Navigation.PushModalAsync(new NavigationPage(editarHorariosPage));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a editar horarios: {ex.Message}");
                await DisplayAlert("Error", $"Error al abrir edición de horarios: {ex.Message}", "OK");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(e.NewTextValue))
                {
                    _medicosFiltrados = _todosMedicos?.ToList() ?? new List<MedicoCompleto>();
                }
                else
                {
                    var textoBusqueda = e.NewTextValue.ToLower();
                    var medicosParaFiltrar = _todosMedicos ?? new List<MedicoCompleto>();
                    _medicosFiltrados = medicosParaFiltrar.Where(m =>
                        (m.nombre_completo?.ToLower().Contains(textoBusqueda) ?? false) ||
                        (m.cedula?.Contains(textoBusqueda) ?? false) ||
                        (m.email?.ToLower().Contains(textoBusqueda) ?? false) ||
                        (m.nombre_especialidad?.ToLower().Contains(textoBusqueda) ?? false)
                    ).ToList();
                }

                MostrarMedicos();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en búsqueda: {ex.Message}");
                ShowMessage("Error al filtrar médicos", false);
            }
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