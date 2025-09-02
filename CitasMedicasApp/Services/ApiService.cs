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

    // ============ AUTENTICACIÓN CON ROLES ============
    public async Task<ApiResponse<Usuario>> LoginAsync(string email, string password)
    {
        try
        {
            var data = new
            {
                username = email,
                password = password
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Login Response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
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
                            rol = loginResponse.data.user.rol,           // ✅ NUEVO
                            rol_id = loginResponse.data.user.rol_id,     // ✅ NUEVO
                            permissions = loginResponse.data.permissions, // ✅ NUEVO
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en LoginAsync: {ex.Message}");
            return new ApiResponse<Usuario>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }
    // ============ NUEVOS MÉTODOS PARA ROLES ============
    public async Task<ApiResponse<UserPermissions>> GetRolePermissionsAsync(int roleId, string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/roles/{roleId}/permissions");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<UserPermissions>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserPermissions>
            {
                success = false,
                message = $"Error obteniendo permisos: {ex.Message}"
            };
        }
    }

    // ============ ESPECIALIDADES - CORRECCIÓN ============
    public async Task<ApiResponse<List<Especialidad>>> ObtenerEspecialidadesAsync()
    {
        try
        {
            // ✅ USAR API REAL EN LUGAR DE DATOS SIMULADOS
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/especialidades");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApiResponse<List<Especialidad>>>(responseContent);
            }
            else
            {
                // Si API falla, usar base de datos directa como fallback
                System.Diagnostics.Debug.WriteLine("API especialidades falló, usando BD directa");
                var dbService = new DatabaseService();
                var especialidades = await dbService.ObtenerEspecialidadesAsync();

                return new ApiResponse<List<Especialidad>>
                {
                    success = true,
                    message = "Datos obtenidos desde base de datos local",
                    data = especialidades
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error API especialidades: {ex.Message}");

            // Fallback a base de datos directa
            try
            {
                var dbService = new DatabaseService();
                var especialidades = await dbService.ObtenerEspecialidadesAsync();

                return new ApiResponse<List<Especialidad>>
                {
                    success = true,
                    message = "Datos obtenidos desde base de datos local (API falló)",
                    data = especialidades
                };
            }
            catch (Exception dbEx)
            {
                return new ApiResponse<List<Especialidad>>
                {
                    success = false,
                    message = $"Error en API y BD: {ex.Message} | {dbEx.Message}"
                };
            }
        }
    }

    // ============ MÉDICOS ============
    public async Task<ApiResponse<Usuario>> RegistrarMedicoAsync(Usuario medico, string password)
    {
        try
        {
            var data = new
            {
                nombres = medico.nombre,
                apellidos = medico.apellido,
                cedula = medico.cedula,
                correo = medico.email,
                contrasena = password,
                telefono = medico.telefono,
                especialidad = medico.especialidad,
                tipo_usuario = medico.tipo_usuario
            };

            System.Diagnostics.Debug.WriteLine($"=== REGISTRANDO MÉDICO ===");
            System.Diagnostics.Debug.WriteLine($"Datos enviados: {JsonConvert.SerializeObject(data, Formatting.Indented)}");

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/medicos", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Respuesta API registro: {responseContent}");

            return JsonConvert.DeserializeObject<ApiResponse<Usuario>>(responseContent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error registrando médico: {ex}");
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
    public async Task<ApiResponse<Usuario>> BuscarPacientePorCedulaAsync(string cedula)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/pacientes/cedula/{cedula}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ApiResponse<Usuario>>(responseContent);
            }
            else
            {
                return new ApiResponse<Usuario>
                {
                    success = false,
                    message = "Paciente no encontrado"
                };
            }
        }
        catch (Exception ex)
        {
            return new ApiResponse<Usuario>
            {
                success = false,
                message = $"Error buscando paciente: {ex.Message}"
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
                // ✅ CORRECTO:
                fecha_nacimiento = paciente.fecha_nacimiento.ToString(),
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

    // ============ HORARIOS - VERSIONES CORREGIDAS ============
    public async Task<ApiResponse<List<HorarioMedico>>> ObtenerHorariosDisponiblesAsync(int idMedico, DateTime fecha, int idSucursal)
    {
        try
        {
            var fechaStr = fecha.ToString("yyyy-MM-dd"); // ✅ CORREGIDO: Sin argumentos extra
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/horarios/medico/{idMedico}/disponibles?fecha={fechaStr}&id_sucursal={idSucursal}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<List<HorarioMedico>>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<HorarioMedico>>
            {
                success = false,
                message = $"Error obteniendo horarios disponibles: {ex.Message}"
            };
        }
    }

    // ============ CITAS ============
    public async Task<ApiResponse<Cita>> CrearCitaAsync(Cita cita)
    {
        try
        {
            var data = new
            {
                id_paciente = cita.id_paciente,
                id_medico = cita.id_medico,
                id_sucursal = cita.id_sucursal,
                fecha_hora = cita.fecha_hora.ToString("yyyy-MM-dd HH:mm:ss"), // ✅ CORREGIDO: Sin argumentos en ToString()
                motivo = cita.motivo,
                tipo_cita = cita.tipo_cita,
                cedula_paciente = cita.cedula_paciente
            };

            var json = JsonConvert.SerializeObject(data);
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
                message = $"Error creando cita: {ex.Message}"
            };
        }
    }
    // ✅ CORREGIDO: ConsultarCitasAsync con tipos anónimos apropiados
    public async Task<ApiResponse<List<Cita>>> ConsultarCitasAsync(int? idPaciente = null, int? idMedico = null)
    {
        try
        {
            object data; // ✅ CAMBIAR a object en lugar de anonymous type vacío

            if (idPaciente.HasValue)
            {
                data = new { id_paciente = idPaciente.Value };
            }
            else if (idMedico.HasValue)
            {
                data = new { id_medico = idMedico.Value };
            }
            else
            {
                data = new { }; // ✅ Objeto vacío apropiado
            }

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
                message = $"Error consultando citas: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> CancelarCitaAsync(int idCita)
    {
        try
        {
            var data = new { estado = "Cancelada" };
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/api/citas/{idCita}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<bool>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                success = false,
                message = $"Error cancelando cita: {ex.Message}"
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

    // ============ SUCURSALES - CORRECCIÓN ============ 
    public async Task<ApiResponse<List<Sucursal>>> ObtenerSucursalesAsync()
    {
        try
        {
            // ✅ USAR API REAL EN LUGAR DE DATOS SIMULADOS
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/sucursales");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApiResponse<List<Sucursal>>>(responseContent);
            }
            else
            {
                // Si API falla, usar base de datos directa como fallback
                System.Diagnostics.Debug.WriteLine("API sucursales falló, usando BD directa");
                var dbService = new DatabaseService();
                var sucursales = await dbService.ObtenerSucursalesAsync();

                return new ApiResponse<List<Sucursal>>
                {
                    success = true,
                    message = "Datos obtenidos desde base de datos local",
                    data = sucursales
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error API sucursales: {ex.Message}");

            // Fallback a base de datos directa
            try
            {
                var dbService = new DatabaseService();
                var sucursales = await dbService.ObtenerSucursalesAsync();

                return new ApiResponse<List<Sucursal>>
                {
                    success = true,
                    message = "Datos obtenidos desde base de datos local (API falló)",
                    data = sucursales
                };
            }
            catch (Exception dbEx)
            {
                return new ApiResponse<List<Sucursal>>
                {
                    success = false,
                    message = $"Error en API y BD: {ex.Message} | {dbEx.Message}"
                };
            }
        }
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
    // ============ TIPOS DE CITA - CORRECCIÓN ============
    public async Task<ApiResponse<List<TipoCita>>> ObtenerTiposCitaAsync()
    {
        try
        {
            // ✅ Intentar obtener desde la API primero
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/tipos-cita");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApiResponse<List<TipoCita>>>(responseContent);
            }
            else
            {
                // ✅ Si la API falla, usar datos hardcoded basados en tu BD
                System.Diagnostics.Debug.WriteLine("API tipos-cita falló, usando datos predeterminados");
                var tipos = new List<TipoCita>
            {
                new TipoCita { id_tipo_cita = 1, nombre_tipo = "Presencial", descripcion = "Cita médica presencial en consultorio o sucursal" },
                new TipoCita { id_tipo_cita = 2, nombre_tipo = "Virtual", descripcion = "Cita médica por videollamada o telemedicina" }
            };

                return new ApiResponse<List<TipoCita>>
                {
                    success = true,
                    message = "Tipos de cita obtenidos desde datos predeterminados",
                    data = tipos
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error tipos de cita: {ex.Message}");

            // ✅ Fallback con datos hardcoded
            var tipos = new List<TipoCita>
        {
            new TipoCita { id_tipo_cita = 1, nombre_tipo = "Presencial", descripcion = "Cita médica presencial en consultorio o sucursal" },
            new TipoCita { id_tipo_cita = 2, nombre_tipo = "Virtual", descripcion = "Cita médica por videollamada o telemedicina" }
        };

            return new ApiResponse<List<TipoCita>>
            {
                success = true,
                message = "Tipos de cita obtenidos desde datos predeterminados (error de conexión)",
                data = tipos
            };
        }
    }

    // ============ MÉTODO INDIVIDUAL PARA ASIGNAR HORARIO - CORREGIDO ============
    public async Task<ApiResponse<object>> AsignarHorarioIndividualAsync(HorarioMedico horario)
    {
        try
        {
            var data = new
            {
                id_medico = horario.id_medico,
                id_sucursal = horario.id_sucursal,
                dia_semana = horario.dia_semana,
                hora_inicio = horario.hora_inicio.ToString(@"hh\:mm\:ss"),
                hora_fin = horario.hora_fin.ToString(@"hh\:mm\:ss"),
                duracion_cita = horario.duracion_consulta // 🔥 CAMBIAR: usar duracion_cita (no duracion_consulta)
            };

            // Debug para ver qué se está enviando
            System.Diagnostics.Debug.WriteLine($"Enviando horario: {JsonConvert.SerializeObject(data, Formatting.Indented)}");

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/horarios", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Respuesta API: {responseContent}");

            return JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en AsignarHorarioIndividualAsync: {ex}");
            return new ApiResponse<object>
            {
                success = false,
                message = $"Error de conexión: {ex.Message}"
            };
        }
    }
    public async Task<ApiResponse<Paciente>> ObtenerPacienteAsync(int idPaciente)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/pacientes/{idPaciente}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<Paciente>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<Paciente>
            {
                success = false,
                message = $"Error obteniendo paciente: {ex.Message}"
            };
        }
    }
    public async Task<ApiResponse<List<HorarioMedico>>> ObtenerHorariosMedicoAsync(int idMedico)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/horarios/medico/{idMedico}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<List<HorarioMedico>>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<HorarioMedico>>
            {
                success = false,
                message = $"Error obteniendo horarios: {ex.Message}"
            };
        }
    }
    // ✅ CORREGIDO: CrearHorarioMedicoAsync - ajustar propiedades del HorarioMedico
    public async Task<ApiResponse<HorarioMedico>> CrearHorarioMedicoAsync(HorarioMedico horario)
    {
        try
        {
            var data = new
            {
                id_medico = horario.id_medico,
                id_sucursal = horario.id_sucursal,
                dia_semana = horario.dia_semana,
                hora_inicio = horario.hora_inicio.ToString(@"hh\:mm\:ss"), // ✅ CORREGIDO: Formato correcto
                hora_fin = horario.hora_fin.ToString(@"hh\:mm\:ss"),       // ✅ CORREGIDO: Formato correcto
                duracion_consulta = horario.duracion_consulta,             // ✅ USAR duracion_consulta
                observaciones = horario.observaciones
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/horarios", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<HorarioMedico>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<HorarioMedico>
            {
                success = false,
                message = $"Error creando horario: {ex.Message}"
            };
        }
    }
    // ✅ CORREGIDO: ActualizarHorarioMedicoAsync
   public async Task<ApiResponse<HorarioMedico>> ActualizarHorarioMedicoAsync(HorarioMedico horario)
{
    try
    {
        var data = new
        {
            id_medico = horario.id_medico,
            id_sucursal = horario.id_sucursal,
            dia_semana = horario.dia_semana,
            hora_inicio = horario.hora_inicio.ToString(@"hh\:mm\:ss"), // ✅ CORREGIDO
            hora_fin = horario.hora_fin.ToString(@"hh\:mm\:ss"),       // ✅ CORREGIDO
            duracion_consulta = horario.duracion_consulta,             // ✅ CORREGIDO
            observaciones = horario.observaciones
        };

        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"{_baseUrl}/api/horarios/{horario.id_horario}", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<ApiResponse<HorarioMedico>>(responseContent);
    }
    catch (Exception ex)
    {
        return new ApiResponse<HorarioMedico>
        {
            success = false,
            message = $"Error actualizando horario: {ex.Message}"
        };
    }
}
    public async Task<ApiResponse<bool>> EliminarHorarioMedicoAsync(int idHorario)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/horarios/{idHorario}");
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse<bool>>(responseContent);
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                success = false,
                message = $"Error eliminando horario: {ex.Message}"
            };
        }
    }

}