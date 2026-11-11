using System.Data;
using Microsoft.Data.SqlClient;

namespace SGH.Infrastructure.Data;

public sealed class AD_Global
{
    private static string? _cadena;
    private SqlConnection? _connection;
    private SqlCommand? _command;

    public static void UsarCadena(string? cadena) => _cadena = cadena;

    public async Task<LIB_ControlResultado> DBOpenAsync(int timeOut = -1, CancellationToken cancellationToken = default)
    {
        if (_connection?.State == ConnectionState.Open)
            return new LIB_ControlResultado();

        try
        {
            if (string.IsNullOrWhiteSpace(_cadena))
            {
                var fallo = new LIB_ControlResultado();
                fallo.SetBadResult("Conexión no establecida con el origen de datos", "Falta ConnectionStrings:Hoteldb.");
                return fallo;
            }

            _connection = new SqlConnection(_cadena);
            _command = _connection.CreateCommand();
            if (timeOut >= 0)
                _command.CommandTimeout = timeOut;

            await _connection.OpenAsync(cancellationToken);
            return new LIB_ControlResultado();
        }
        catch (SqlException ex)
        {
            var fallo = new LIB_ControlResultado();
            fallo.SetBadResult("Conexión no establecida con el origen de datos", $"({ex.Number}){ex.Message}");
            fallo.Codigo = ex.Number.ToString();
            return fallo;
        }
        catch (Exception ex)
        {
            var fallo = new LIB_ControlResultado();
            fallo.SetBadResult("Conexión no establecida con el origen de datos", ex.Message);
            return fallo;
        }
    }

    public string NameProcedure
    {
        set
        {
            if (_command is null || _connection is null)
                throw new InvalidOperationException("DBOpen no dejó una conexión abierta.");

            _command.Connection = _connection;
            _command.CommandType = CommandType.StoredProcedure;
            _command.CommandText = value;
            _command.Parameters.Clear();
        }
    }

    public LIB_ControlResultado AddParameter(string nombre, object? valor)
    {
        var tipo = valor switch
        {
            int => SqlDbType.Int,
            long => SqlDbType.BigInt,
            decimal => SqlDbType.Decimal,
            bool => SqlDbType.Bit,
            DateTime => SqlDbType.DateTime,
            _ => SqlDbType.NVarChar
        };
        var largo = valor is string texto ? texto.Length : -1;
        return AddParameter(nombre, valor, ParameterDirection.Input, tipo, largo);
    }

    public LIB_ControlResultado AddParameter(
        string nombre,
        object? valor,
        ParameterDirection direction,
        SqlDbType tipo,
        int largo = -1)
    {
        if (_command is null || _connection?.State != ConnectionState.Open)
        {
            var cerrado = new LIB_ControlResultado();
            cerrado.SetBadResult("La conexión con el origen de datos no ha sido establecida", "objConn.State no es Open");
            return cerrado;
        }

        try
        {
            var parametro = new SqlParameter(nombre, tipo)
            {
                Direction = direction,
                Value = valor ?? DBNull.Value
            };
            if (largo >= 0)
                parametro.Size = largo;

            _command.Parameters.Add(parametro);
            return new LIB_ControlResultado();
        }
        catch (Exception ex)
        {
            var fallo = new LIB_ControlResultado();
            fallo.SetBadResult("Excepción ocurrida al agregar un parámetro", ex.Message);
            return fallo;
        }
    }

    public async Task<LIB_ControlResultado> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
    {
        if (_command is null || _connection?.State != ConnectionState.Open)
        {
            var cerrado = new LIB_ControlResultado();
            cerrado.SetBadResult("La conexión con el origen de datos no ha sido establecida", "objConn.State no es Open");
            return cerrado;
        }

        try
        {
            await _command.ExecuteNonQueryAsync(cancellationToken);
            return new LIB_ControlResultado();
        }
        catch (SqlException ex)
        {
            var fallo = new LIB_ControlResultado();
            fallo.SetBadResult(
                "No se pudo ejecutar el procedimiento almacenado " + _command.CommandText,
                $"({ex.Number}) {ex.Message}");
            fallo.Codigo = ex.Number.ToString();
            return fallo;
        }
    }

    public int? LeerSalidaInt(string nombre)
    {
        var parametro = _command?.Parameters
            .Cast<SqlParameter>()
            .FirstOrDefault(item => item.ParameterName == nombre);
        if (parametro?.Value is null or DBNull)
            return null;

        return parametro.Value is int codigo ? codigo : Convert.ToInt32(parametro.Value);
    }

    public bool LeerSalidaBool(string nombre)
    {
        var parametro = _command?.Parameters
            .Cast<SqlParameter>()
            .FirstOrDefault(item => item.ParameterName == nombre);
        var valor = parametro?.Value;
        return valor is bool bandera && bandera
            || valor is byte octeto && octeto != 0
            || valor is int numero && numero != 0;
    }

    public string? LeerSalidaString(string nombre)
    {
        var parametro = _command?.Parameters
            .Cast<SqlParameter>()
            .FirstOrDefault(item => item.ParameterName == nombre);
        return parametro?.Value as string;
    }

    public void DBClose()
    {
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;
        _command?.Dispose();
        _command = null;
    }
}
