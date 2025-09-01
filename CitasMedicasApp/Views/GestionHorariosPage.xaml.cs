using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CitasMedicasApp.Models;
using CitasMedicasApp.Services;

namespace CitasMedicasApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GestionHorariosPage : ContentPage
    {
        private readonly ApiService _apiService;
        private ObservableCollection<HorarioMedicoDetalle> _misHorarios;

        public ICommand EditarHorarioCommand { get; set; }

        public GestionHorariosPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _misHorarios = new ObservableCollection<HorarioMedicoDetalle>();
            MisHorariosCollectionView.ItemsSource = _misHorarios;

            EditarHorarioCommand = new Command<HorarioMedicoDetalle>(async (horario) => await OnEditarHorarioClicked(horario));
            BindingContext = this;

            LoadMisHorarios();
        }

        private async void LoadMisHorarios()
        {
            ShowLoading(true);

            try
            {
                var idMedico = UserSessionManager.CurrentUser?.id ?? 0;
                var response = await _apiService.ObtenerHorariosMedicoAsync(idMedico);

                if (response.success && response.data != null)
                {
                    _misHorarios.Clear();
                    foreach (var horario in response.data)
                    {
                        _misHorarios.Add(new HorarioMedicoDetalle
                        {
                            IdHorario = horario.id_horario,
                            DiaSemana = horario.dia_semana,
                            HoraInicio = horario.hora_inicio,
                            HoraFin = horario.hora_fin,
                            SucursalNombre = horario.nombre_sucursal,
                            DiaTexto = ObtenerNombreDia(horario.dia_semana),
                            HorarioCompleto = $"{horario.hora_inicio:HH:mm} - {horario.hora_fin:HH:mm}",
                            SucursalTexto = $"📍 {horario.nombre_sucursal}"
                        });
                    }

                    NoHorariosLabel.IsVisible = _misHorarios.Count == 0;
                }
                else
                {
                    NoHorariosLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando horarios: {ex.Message}");
                await DisplayAlert("❌ Error", "Error cargando sus horarios", "OK");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void OnAgregarHorarioClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ConfigurarHorarioPage());
        }

        private async Task OnEditarHorarioClicked(HorarioMedicoDetalle horario)
        {
            await Navigation.PushAsync(new ConfigurarHorarioPage(horario));
        }

        private string ObtenerNombreDia(int diaSemana)
        {
            return diaSemana switch
            {
                1 => "🗓️ Lunes",
                2 => "🗓️ Martes",
                3 => "🗓️ Miércoles",
                4 => "🗓️ Jueves",
                5 => "🗓️ Viernes",
                6 => "🗓️ Sábado",
                7 => "🗓️ Domingo",
                _ => "🗓️ Día no válido"
            };
        }

        private void ShowLoading(bool isLoading)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsVisible = isLoading;
                LoadingIndicator.IsRunning = isLoading;
            });
        }
    }

    // ===== MODELO AUXILIAR =====
    public class HorarioMedicoDetalle
    {
        public int IdHorario { get; set; }
        public int DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string SucursalNombre { get; set; }
        public string DiaTexto { get; set; }
        public string HorarioCompleto { get; set; }
        public string SucursalTexto { get; set; }
    }
}