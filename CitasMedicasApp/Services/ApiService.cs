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
    private readonly string _baseUrl = "http://192.168.21.111:8081/webservice-slim";

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    // ============ AUTENTICACIÓN ============
    // Nota: No hay endpoint de login en tu API, necesitarás implementarlo
    // Por ahora simulo una respuesta exitosa para pruebas
    // ============ AUTENTICACIÓN REAL ============
    // ============ AUTENTICACIÓN REAL - SIN DYNAMIC ============
    public async Task<ApiResponse<Usuario>> LoginAsync(string email, string password)
    {
        try
        {
            var data = new
            {
                username = email,  // Tu backend usa 'username', no 'email'
                password = password
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // ✅ USANDO JsonConvert en lugar de dynamic
                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                if (loginResponse != null && loginResponse.success)
                {
                    return new ApiResponse<Usuario>
                    {
                        success = true,
                        message = loginResponse.message,
                        data = new Usuario
                        {
                            id = loginResponse.data.user.id,
                            nombre = loginResponse.data.user.nombres,
                            apellido = loginResponse.data.user.apellidos,
                            email = loginResponse.data.user.correo,
                            username = loginResponse.data.user.username,
                            tipo_usuario = loginResponse.data.user.rol,
                            token = loginResponse.data.token
                        }
                    };
                }
                else
                {
                    return new ApiResponse<Usuario>
                    {
                        success = false,
                        message = loginResponse?.message ?? "Credenciales incorrectas"
                    };
                }
            }
            else
            {
                return new ApiResponse<Usuario>
                {
                    success = false,
                    message = "Error de autenticación. Verificar credenciales."
                };
            }
        }
        catch (HttpRequestException)
        {
            return new ApiResponse<Usuario>
            {
                success = false,
                message = "No se pudo conectar con el servidor. Verifique su conexión."
            };
        }
        catch (TaskCanceledException)
        {
            return new ApiResponse<Usuario>
            {
                success = false,
                message = "La solicitud tardó demasiado. Intente nuevamente."
            };
        }
        catch (JsonException)
        {
            return new ApiResponse<Usuario>
            {
                success = false,
                message = "Error al procesar la respuesta del servidor."
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Usuario>
            {
                success = false,
                message = $"Error inesperado: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<List<Especialidad>>> ObtenerEspecialidadesAsync()
    {
        try
        {
            // Intentar API primero
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/especialidades");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApiResponse<List<Especialidad>>>(responseContent);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API falló: {ex.Message}");
        }

        // Si API falla, usar base de datos directa
        try
        {
            var dbService = new DatabaseService();
            var especialidades = await dbService.ObtenerEspecialidadesAsync();

            return new ApiResponse<List<Especialidad>>
            {
                success = true,
                message = "Datos obtenidos desde base de datos local",
                data = especialidades
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<Especialidad>>
            {
                success = false,
                message = $"Error en API y BD: {ex.Message}"
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

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/medicos", content);
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

    public async Task<ApiResponse<List<MedicoCompleto>>> ObtenerTodosMedicosAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/medicos");
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

    // ============ PACIENTES ============
    public async Task<ApiResponse<PacienteCompleto>> BuscarPacientePorCedulaAsync(string cedula)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/pacientes/{cedula}");
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

    public async Task<ApiResponse<PacienteCompleto>> RegistrarPacienteAsync(PacienteCompleto paciente)
    {
        try
        {
            var data = new
            {
                cedula = paciente.cedula,
                nombres = paciente.nombres,
                apellidos = paciente.apellidos,
                correo = paciente.correo,
                username = paciente.cedula,
                password = paciente.cedula,
                id_rol = 71,
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

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/pacientes", content);
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

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/horarios", content);
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

    public async Task<ApiResponse<List<HorarioMedicoDetallado>>> ObtenerHorariosMedicoDetalladosAsync(int idMedico)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/horarios/medico/{idMedico}");
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

    public async Task<ApiResponse<List<string>>> ObtenerHorariosDisponiblesAsync(int idMedico, int idSucursal, DateTime fecha)
    {
        try
        {
            var fechaStr = fecha.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/horarios/medico/{idMedico}/disponibles?fecha={fechaStr}&sucursal={idSucursal}");
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

    // ============ CITAS ============
    public async Task<ApiResponse<Cita>> CrearCitaAsync(CitaCreacion cita)
    {
        try
        {
            var json = JsonConvert.SerializeObject(cita);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/citas", content);
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
            var data = new { id_medico = idMedico };
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/citas/consultar", content);
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

    // ============ MÉTODOS FALTANTES (implementación temporal) ============
    public async Task<ApiResponse<List<Sucursal>>> ObtenerSucursalesAsync()
    {
        // No está en tu API, simulo datos
        var sucursales = new List<Sucursal>
        {
            new Sucursal { id_sucursal = 1, nombre_sucursal = "Sucursal Principal", direccion = "Av. Principal 123" },
            new Sucursal { id_sucursal = 2, nombre_sucursal = "Sucursal Norte", direccion = "Av. Norte 456" }
        };

        return new ApiResponse<List<Sucursal>>
        {
            success = true,
            data = sucursales
        };
    }

    public async Task<ApiResponse<List<TipoCita>>> ObtenerTiposCitaAsync()
    {
        // No está en tu API, simulo datos
        var tipos = new List<TipoCita>
        {
            new TipoCita { id_tipo_cita = 1, nombre_tipo = "Presencial" },
            new TipoCita { id_tipo_cita = 2, nombre_tipo = "Virtual" }
        };

        return new ApiResponse<List<TipoCita>>
        {
            success = true,
            data = tipos
        };
    }
    // ============ HORARIOS DISPONIBLES DETALLADOS ============
    public async Task<ApiResponse<HorariosDisponiblesResponse>> ObtenerHorariosDisponiblesDetalladosAsync(int idMedico, int idSucursal, DateTime fecha)
    {
        try
        {
            var fechaStr = fecha.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/horarios/medico/{idMedico}/disponibles?fecha={fechaStr}&sucursal={idSucursal}");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Como no tienes este endpoint específico, devuelvo null para usar el fallback
            return new ApiResponse<HorariosDisponiblesResponse>
            {
                success = false,
                message = "Endpoint no implementado, usar método simple"
            };
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
    private void SetAuthorizationHeader(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}