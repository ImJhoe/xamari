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
    private readonly string _baseUrl = "http://192.168.1.8:8081/webservice-slim";

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    // ============ AUTENTICACIÓN ============
    // Nota: No hay endpoint de login en tu API, necesitarás implementarlo
    // Por ahora simulo una respuesta exitosa para pruebas
    public async Task<ApiResponse<Usuario>> LoginAsync(string email, string password)
    {
        try
        {
            // Tu API no tiene login, simulamos por ahora
            if (email == "admin@clinicamedica.ec" && password == "123")
            {
                return new ApiResponse<Usuario>
                {
                    success = true,
                    message = "Login exitoso",
                    data = new Usuario
                    {
                        id = 1,
                        nombre = "Admin",
                        email = email,
                        tipo_usuario = "admin"
                    }
                };
            }
            else
            {
                return new ApiResponse<Usuario>
                {
                    success = false,
                    message = "Credenciales incorrectas"
                };
            }
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

    // ============ ESPECIALIDADES ============
    public async Task<ApiResponse<List<Especialidad>>> ObtenerEspecialidadesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/especialidades");
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
}