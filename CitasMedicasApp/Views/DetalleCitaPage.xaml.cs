using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Models;
using CitasMedicasApp.Services;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetalleCitaPage : ContentPage
    {
        private readonly Cita _cita;

        public DetalleCitaPage(Cita cita)
        {
            InitializeComponent();
            _cita = cita;
            CargarDetallesCita();
        }

        private void CargarDetallesCita()
        {
            if (_cita == null) return;

            var fechaCita = _cita.fecha_cita != default(DateTime) ? _cita.fecha_cita : _cita.fecha_hora;

            // Configurar título
            NumeroLabel.Text = $"Cita #{_cita.id_cita}";

            // Configurar estado
            ConfigurarEstado(_cita.estado ?? "Programada");

            // Información del paciente
            PacienteNombreLabel.Text = _cita.nombre_paciente ?? "No especificado";
            PacienteCedulaLabel.Text = $"Cédula: {_cita.cedula_paciente}";
            PacienteEmailLabel.Text = "Email: No disponible"; // TODO: Agregar email si está disponible

            // Información médica
            MedicoNombreLabel.Text = _cita.nombre_medico ?? "No especificado";
            EspecialidadLabel.Text = $"Especialidad: {_cita.nombre_especialidad ?? "No especificada"}";
            SucursalLabel.Text = $"Sucursal: {_cita.nombre_sucursal ?? "No especificada"}";

            // Fecha y hora
            FechaCitaLabel.Text = $"📅 {fechaCita:dddd, dd MMMM yyyy}";
            HoraCitaLabel.Text = $"🕒 {fechaCita:HH:mm}";

            var tiempoRestante = fechaCita - DateTime.Now;
            if (tiempoRestante.TotalDays > 1)
            {
                TiempoRestanteLabel.Text = $"⏳ Faltan {(int)tiempoRestante.TotalDays} días";
            }
            else if (tiempoRestante.TotalHours > 1)
            {
                TiempoRestanteLabel.Text = $"⏳ Faltan {(int)tiempoRestante.TotalHours} horas";
            }
            else if (tiempoRestante.TotalMinutes > 0)
            {
                TiempoRestanteLabel.Text = $"⏳ Faltan {(int)tiempoRestante.TotalMinutes} minutos";
            }
            else if (tiempoRestante.TotalMinutes > -60)
            {
                TiempoRestanteLabel.Text = "🚨 ¡Es ahora!";
                TiempoRestanteLabel.TextColor = Color.FromHex("#e74c3c");
            }
            else
            {
                TiempoRestanteLabel.Text = "⏰ Ya pasó";
                TiempoRestanteLabel.TextColor = Color.FromHex("#95a5a6");
            }

            // Motivo
            if (!string.IsNullOrEmpty(_cita.motivo))
            {
                MotivoStack.Children.Add(new Label
                {
                    Text = "📝 Motivo:",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 14
                });
                MotivoStack.Children.Add(new Frame
                {
                    BackgroundColor = Color.FromHex("#f8f9fa"),
                    CornerRadius = 8,
                    Padding = new Thickness(10),
                    Content = new Label
                    {
                        Text = _cita.motivo,
                        FontSize = 14
                    }
                });
            }

            // Tipo de cita
            if (!string.IsNullOrEmpty(_cita.tipo_cita))
            {
                TipoCitaStack.Children.Add(new Label
                {
                    Text = $"💻 Tipo de cita: {_cita.tipo_cita}",
                    FontSize = 14
                });
            }

            // Configurar botones según rol
            ConfigurarBotonesSegunRol(fechaCita);
        }

        private void ConfigurarEstado(string estado)
        {
            EstadoLabel.Text = estado;

            switch (estado.ToLower())
            {
                case "programada":
                    EstadoFrame.BackgroundColor = Color.FromHex("#f39c12");
                    break;
                case "confirmada":
                    EstadoFrame.BackgroundColor = Color.FromHex("#27ae60");
                    break;
                case "completada":
                    EstadoFrame.BackgroundColor = Color.FromHex("#3498db");
                    break;
                case "cancelada":
                    EstadoFrame.BackgroundColor = Color.FromHex("#e74c3c");
                    break;
                default:
                    EstadoFrame.BackgroundColor = Color.FromHex("#95a5a6");
                    break;
            }
        }

        private void ConfigurarBotonesSegunRol(DateTime fechaCita)
        {
            var puedeModificar = fechaCita > DateTime.Now.AddHours(2);
            var estado = _cita.estado?.ToLower() ?? "programada";

            BotonesAccionStack.Children.Clear();

            if (UserSessionManager.IsAdmin)
            {
                // Admin puede hacer todo
                if (puedeModificar && estado != "cancelada")
                {
                    BotonesAccionStack.Children.Add(new Button
                    {
                        Text = "✏️ EDITAR CITA",
                        BackgroundColor = Color.FromHex("#3498db"),
                        TextColor = Color.White,
                        CornerRadius = 25,
                        Command = new Command(async () => await EditarCita())
                    });
                }

                BotonesAccionStack.Children.Add(new Button
                {
                    Text = "📊 VER HISTORIAL",
                    BackgroundColor = Color.FromHex("#9b59b6"),
                    TextColor = Color.White,
                    CornerRadius = 25,
                    Command = new Command(async () => await VerHistorial())
                });
            }
            else if (UserSessionManager.IsRecepcionista)
            {
                // Recepcionista puede reagendar y confirmar
                if (puedeModificar && estado != "cancelada")
                {
                    BotonesAccionStack.Children.Add(new Button
                    {
                        Text = "📅 REAGENDAR",
                        BackgroundColor = Color.FromHex("#e67e22"),
                        TextColor = Color.White,
                        CornerRadius = 25,
                        Command = new Command(async () => await ReagendarCita())
                    });
                }

                if (estado == "programada")
                {
                    BotonesAccionStack.Children.Add(new Button
                    {
                        Text = "✅ CONFIRMAR CITA",
                        BackgroundColor = Color.FromHex("#27ae60"),
                        TextColor = Color.White,
                        CornerRadius = 25,
                        Command = new Command(async () => await ConfirmarCita())
                    });
                }
            }
            else if (UserSessionManager.IsMedico)
            {
                // Médico puede atender
                if (fechaCita.Date == DateTime.Now.Date && estado != "completada")
                {
                    BotonesAccionStack.Children.Add(new Button
                    {
                        Text = "🩺 INICIAR CONSULTA",
                        BackgroundColor = Color.FromHex("#27ae60"),
                        TextColor = Color.White,
                        CornerRadius = 25,
                        Command = new Command(async () => await IniciarConsulta())
                    });
                }
            }
            else if (UserSessionManager.IsPaciente)
            {
                // Paciente puede cancelar
                if (puedeModificar && estado != "cancelada")
                {
                    BotonesAccionStack.Children.Add(new Button
                    {
                        Text = "❌ CANCELAR CITA",
                        BackgroundColor = Color.FromHex("#e74c3c"),
                        TextColor = Color.White,
                        CornerRadius = 25,
                        Command = new Command(async () => await CancelarCita())
                    });
                }
            }

            // Si no hay botones disponibles
            if (BotonesAccionStack.Children.Count == 0)
            {
                AccionesFrame.IsVisible = false;
            }
        }

        // ============ ACCIONES ============
        private async System.Threading.Tasks.Task EditarCita()
        {
            await Navigation.PushAsync(new CrearCitaPage()); // TODO: Pasar cita para edición
        }

        private async System.Threading.Tasks.Task ReagendarCita()
        {
            await DisplayAlert("🚧 En Desarrollo", "Funcionalidad de reagendar en desarrollo", "OK");
        }

        private async System.Threading.Tasks.Task ConfirmarCita()
        {
            bool confirmar = await DisplayAlert("Confirmar Cita",
                "¿Confirmar esta cita médica?", "Sí", "No");

            if (confirmar)
            {
                // TODO: Implementar confirmación
                await DisplayAlert("✅ Confirmado", "Cita confirmada exitosamente", "OK");
            }
        }

        private async System.Threading.Tasks.Task IniciarConsulta()
        {
            await DisplayAlert("🚧 En Desarrollo", "Funcionalidad de consulta médica en desarrollo", "OK");
        }

        private async System.Threading.Tasks.Task CancelarCita()
        {
            bool confirmar = await DisplayAlert("Cancelar Cita",
                $"¿Está seguro que desea cancelar esta cita?\n\n⚠️ Esta acción no se puede deshacer.",
                "Sí, cancelar", "No");

            if (!confirmar) return;

            try
            {
                var apiService = new ApiService();
                var response = await apiService.CancelarCitaAsync(_cita.id_cita);

                if (response.success)
                {
                    await DisplayAlert("✅ Éxito", "Cita cancelada exitosamente", "OK");
                    await Navigation.PopAsync(); // Regresar a la lista
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
        }

        private async System.Threading.Tasks.Task VerHistorial()
        {
            await DisplayAlert("🚧 En Desarrollo", "Funcionalidad de historial médico en desarrollo", "OK");
        }
    }
}
