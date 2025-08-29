// Services/ApiService.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CitasMedicasApp.Models;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "http://10.0.2.2:8081"; // Para emulador Android
                                                               // Para dispositivo físico usar: "http://TU_IP_LOCAL:8081"

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    // ============ AUTENTICACIÓN ============
    public async Task<ApiResponse<Usuario>> LoginAsync(string email, string password)
    {
        try
        {
            var loginData = new { email, password };
            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<Usuario>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<Usuario>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    // ============ MÉDICOS ============
    public async Task<ApiResponse<Usuario>> RegistrarMedicoAsync(Usuario medico, string password)
    {
        try
        {
            var data = new
            {
                cedula = medico.cedula,
                nombre = medico.nombre,
                apellido = medico.apellido,
                email = medico.email,
                telefono = medico.telefono,
                especialidad = medico.especialidad,
                password = password,
                tipo_usuario = "medico"
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/usuarios/registrar", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<Usuario>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<Usuario>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<List<HorarioMedico>>> ObtenerHorariosMedicoAsync(int idMedico)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/horarios/medico/{idMedico}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<List<HorarioMedico>>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<HorarioMedico>>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    // ============ PACIENTES ============
    public async Task<ApiResponse<Paciente>> RegistrarPacienteAsync(Paciente paciente)
    {
        try
        {
            var json = JsonConvert.SerializeObject(paciente);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/pacientes/registrar", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<Paciente>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<Paciente>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    // ============ CITAS ============
    public async Task<ApiResponse<Cita>> CrearCitaAsync(Cita cita)
    {
        try
        {
            var json = JsonConvert.SerializeObject(cita);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/citas/crear", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<Cita>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<Cita>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<List<Cita>>> ObtenerCitasAsync(int? idMedico = null)
    {
        try
        {
            var url = $"{_baseUrl}/citas/consultar";
            if (idMedico.HasValue)
            {
                url += $"?id_medico={idMedico.Value}";
            }

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<List<Cita>>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<Cita>>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }
    // ============ ESPECIALIDADES ============
    public async Task<ApiResponse<List<Especialidad>>> ObtenerEspecialidadesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/especialidades");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<List<Especialidad>>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<Especialidad>>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    // ============ SUCURSALES ============
    public async Task<ApiResponse<List<Sucursal>>> ObtenerSucursalesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/sucursales");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<List<Sucursal>>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<Sucursal>>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    // ============ HORARIOS ============
    public async Task<ApiResponse<object>> AsignarHorariosMedicoAsync(int idMedico, List<HorarioMedico> horarios)
    {
        try
        {
            var data = new
            {
                id_medico = idMedico,
                horarios = horarios
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/horarios/asignar", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<object>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }
    // ============ CONSULTA DE MÉDICOS ============
    public async Task<ApiResponse<List<MedicoCompleto>>> ObtenerTodosMedicosAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/medicos/consultar");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<List<MedicoCompleto>>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<MedicoCompleto>>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<MedicoCompleto>> ObtenerMedicoPorIdAsync(int idMedico)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/medicos/{idMedico}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<MedicoCompleto>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<MedicoCompleto>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<List<HorarioMedicoDetallado>>> ObtenerHorariosMedicoDetalladosAsync(int idMedico)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/horarios/medico/{idMedico}/detallado");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<List<HorarioMedicoDetallado>>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<HorarioMedicoDetallado>>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<object>> ActualizarHorarioMedicoAsync(int idHorario, HorarioMedicoDetallado horario)
    {
        try
        {
            var data = new
            {
                dia_semana = horario.dia_semana,
                hora_inicio = horario.hora_inicio,
                hora_fin = horario.hora_fin,
                duracion_cita = horario.duracion_cita,
                activo = horario.activo
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/horarios/{idHorario}/actualizar", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<object>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<object>> EliminarHorarioMedicoAsync(int idHorario)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/horarios/{idHorario}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<object>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }
    // ============ BUSCAR PACIENTES ============
    public async Task<ApiResponse<PacienteCompleto>> BuscarPacientePorCedulaAsync(string cedula)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/pacientes/buscar/{cedula}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<PacienteCompleto>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<PacienteCompleto>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    // ============ TIPOS DE CITA ============
    public async Task<ApiResponse<List<TipoCita>>> ObtenerTiposCitaAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/tipos-cita");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<List<TipoCita>>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<TipoCita>>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    // ============ HORARIOS DISPONIBLES MEJORADOS ============
    public async Task<ApiResponse<HorariosDisponiblesResponse>> ObtenerHorariosDisponiblesDetalladosAsync(int idMedico, int idSucursal, DateTime fecha)
    {
        try
        {
            var fechaStr = fecha.ToString("yyyy-MM-dd");
            var diaSemana = ((int)fecha.DayOfWeek == 0) ? 7 : (int)fecha.DayOfWeek; // Convertir domingo de 0 a 7

            var response = await _httpClient.GetAsync($"{_baseUrl}/horarios/disponibles/detallado?id_medico={idMedico}&id_sucursal={idSucursal}&fecha={fechaStr}&dia_semana={diaSemana}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<HorariosDisponiblesResponse>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<HorariosDisponiblesResponse>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }
    // ============ HORARIOS DISPONIBLES SIMPLES ============
    public async Task<ApiResponse<List<string>>> ObtenerHorariosDisponiblesAsync(int idMedico, int idSucursal, DateTime fecha)
    {
        try
        {
            var fechaStr = fecha.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"{_baseUrl}/horarios/disponibles?id_medico={idMedico}&id_sucursal={idSucursal}&fecha={fechaStr}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<List<string>>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<string>>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }

    // ============ CREAR CITA ============
    public async Task<ApiResponse<Cita>> CrearCitaAsync(CitaCreacion cita)
    {
        try
        {
            var json = JsonConvert.SerializeObject(cita);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/citas/crear", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<Cita>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<Cita>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }
    // ============ REGISTRO DE PACIENTES ============
    public async Task<ApiResponse<PacienteCompleto>> RegistrarPacienteAsync(PacienteCompleto paciente)
    {
        try
        {
            var data = new
            {
                // Datos del usuario
                cedula = paciente.cedula,
                nombres = paciente.nombres,
                apellidos = paciente.apellidos,
                correo = paciente.correo,
                username = paciente.cedula, // Usar cédula como username
                password = paciente.cedula, // Password temporal (cédula)
                id_rol = 71, // ID del rol paciente

                // Datos específicos del paciente
                telefono = paciente.telefono,
                fecha_nacimiento = paciente.fecha_nacimiento.ToString("yyyy-MM-dd"),
                tipo_sangre = paciente.tipo_sangre,
                alergias = paciente.alergias,
                antecedentes_medicos = paciente.antecedentes_medicos,
                contacto_emergencia = paciente.contacto_emergencia,
                telefono_emergencia = paciente.telefono_emergencia,
                numero_seguro = paciente.numero_seguro
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/pacientes/registrar", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<PacienteCompleto>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<PacienteCompleto>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }
}