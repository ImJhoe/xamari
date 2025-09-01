// Services/DatabaseService.cs - VERSIÓN CORREGIDA
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using CitasMedicasApp.Models;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        _connectionString = "Server=192.168.21.111;Port=3306;Database=menudinamico;Uid=root;Pwd=;";
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error conexión DB: {ex.Message}");
            return false;
        }
    }

    // ============ USUARIOS/LOGIN ============
    public async Task<Usuario> ValidarLoginAsync(string email, string password)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT u.*, r.nombre_rol 
                FROM usuarios u 
                LEFT JOIN roles r ON u.id_rol = r.id_rol 
                WHERE u.correo = @email AND u.password = @password AND u.id_estado = 1";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@password", password);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Usuario
                {
                    id = Convert.ToInt32(reader["id_usuario"]),
                    nombre = reader["nombres"].ToString(),
                    apellido = reader["apellidos"].ToString(),
                    email = reader["correo"].ToString(),
                    cedula = reader["cedula"].ToString(),
                    tipo_usuario = reader["nombre_rol"] != DBNull.Value ? reader["nombre_rol"].ToString() : "usuario"
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error login DB: {ex.Message}");
            return null;
        }
    }

    // ============ ESPECIALIDADES ============
    public async Task<List<Especialidad>> ObtenerEspecialidadesAsync()
    {
        var especialidades = new List<Especialidad>();
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT * FROM especialidades ORDER BY nombre_especialidad";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                especialidades.Add(new Especialidad
                {
                    id_especialidad = Convert.ToInt32(reader["id_especialidad"]),
                    nombre_especialidad = reader["nombre_especialidad"].ToString(),
                    descripcion = reader["descripcion"] != DBNull.Value ? reader["descripcion"].ToString() : ""
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error especialidades DB: {ex.Message}");
        }
        return especialidades;
    }

    // ============ SUCURSALES ============
    public async Task<List<Sucursal>> ObtenerSucursalesAsync()
    {
        var sucursales = new List<Sucursal>();
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT * FROM sucursales WHERE estado = 1 ORDER BY nombre_sucursal";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                sucursales.Add(new Sucursal
                {
                    id_sucursal = Convert.ToInt32(reader["id_sucursal"]),
                    nombre_sucursal = reader["nombre_sucursal"].ToString(),
                    direccion = reader["direccion"].ToString(),
                    telefono = reader["telefono"].ToString(),
                    email = reader["email"] != DBNull.Value ? reader["email"].ToString() : ""
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sucursales DB: {ex.Message}");
        }
        return sucursales;
    }

    // ============ MÉDICOS ============
    public async Task<List<MedicoCompleto>> ObtenerMedicosAsync()
    {
        var medicos = new List<MedicoCompleto>();
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT d.*, u.nombres, u.apellidos, u.correo, u.cedula, u.telefono,
                       e.nombre_especialidad
                FROM doctores d
                INNER JOIN usuarios u ON d.id_usuario = u.id_usuario
                INNER JOIN especialidades e ON d.id_especialidad = e.id_especialidad
                WHERE d.activo = 1";

            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                medicos.Add(new MedicoCompleto
                {
                    id_doctor = Convert.ToInt32(reader["id_doctor"]),
                    id_usuario = Convert.ToInt32(reader["id_usuario"]),
                    cedula = reader["cedula"].ToString(),
                    nombre = reader["nombres"].ToString(),
                    apellido = reader["apellidos"].ToString(),
                    email = reader["correo"].ToString(),
                    telefono = reader["telefono"] != DBNull.Value ? reader["telefono"].ToString() : "",
                    id_especialidad = Convert.ToInt32(reader["id_especialidad"]),
                    nombre_especialidad = reader["nombre_especialidad"].ToString(),
                    activo = Convert.ToBoolean(reader["activo"])
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error médicos DB: {ex.Message}");
        }
        return medicos;
    }

    // ============ PACIENTES ============
    public async Task<PacienteCompleto> BuscarPacientePorCedulaAsync(string cedula)
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT p.*, u.nombres, u.apellidos, u.correo, u.cedula
                FROM pacientes p
                INNER JOIN usuarios u ON p.id_usuario = u.id_usuario
                WHERE u.cedula = @cedula";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@cedula", cedula);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new PacienteCompleto
                {
                    id_paciente = Convert.ToInt32(reader["id_paciente"]),
                    id_usuario = Convert.ToInt32(reader["id_usuario"]),
                    cedula = reader["cedula"].ToString(),
                    nombres = reader["nombres"].ToString(),
                    apellidos = reader["apellidos"].ToString(),
                    correo = reader["correo"].ToString(),
                    telefono = reader["telefono"] != DBNull.Value ? reader["telefono"].ToString() : "",
                    fecha_nacimiento = Convert.ToDateTime(reader["fecha_nacimiento"]),
                    tipo_sangre = reader["tipo_sangre"] != DBNull.Value ? reader["tipo_sangre"].ToString() : ""
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error buscar paciente DB: {ex.Message}");
            return null;
        }
    }

    // ============ CITAS ============
    public async Task<List<Cita>> ObtenerCitasAsync()
    {
        var citas = new List<Cita>();
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
    SELECT c.*, 
           TIME(c.fecha_hora) as hora_inicio,  -- ✅ EXTRAER HORA DE fecha_hora
           CONCAT(up.nombres, ' ', up.apellidos) as nombre_paciente,
           CONCAT(ud.nombres, ' ', ud.apellidos) as nombre_medico,
           e.nombre_especialidad,
           s.nombre_sucursal
    FROM citas c
    -- ... resto de la consulta
";

            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            // En DatabaseService.cs, método ObtenerCitasAsync()
            while (await reader.ReadAsync())
            {
                var fechaHora = Convert.ToDateTime(reader["fecha_hora"]);

                // ✅ Manejar hora_inicio que puede ser null
                TimeSpan horaInicio = TimeSpan.Zero;
                if (reader["hora_inicio"] != DBNull.Value && !string.IsNullOrEmpty(reader["hora_inicio"].ToString()))
                {
                    horaInicio = TimeSpan.Parse(reader["hora_inicio"].ToString());
                }
                else
                {
                    // Si no hay hora_inicio, usar la hora de fecha_hora
                    horaInicio = fechaHora.TimeOfDay;
                }

                citas.Add(new Cita
                {
                    id_cita = Convert.ToInt32(reader["id_cita"]),
                    id_paciente = Convert.ToInt32(reader["id_paciente"]),
                    id_medico = Convert.ToInt32(reader["id_doctor"]),
                    fecha_hora = fechaHora,
                    fecha_cita = fechaHora.Date,
                    hora_inicio = horaInicio, // ✅ USAR LA VARIABLE SEGURA
                    estado = reader["estado"].ToString(),
                    motivo = reader["motivo"].ToString(),
                    nombre_paciente = reader["nombre_paciente"].ToString(),
                    nombre_medico = reader["nombre_medico"].ToString(),
                    especialidad = reader["nombre_especialidad"].ToString(),
                    nombre_sucursal = reader["nombre_sucursal"].ToString(),
                    tipo_cita = reader["tipo_cita"] != DBNull.Value ? reader["tipo_cita"].ToString() : "presencial"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error citas DB: {ex.Message}");
        }
        return citas;
    }
}