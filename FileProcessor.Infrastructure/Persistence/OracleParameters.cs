using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace FileProcessor.Infrastructure.Persistence
{
    public class OracleParameters : SqlMapper.IDynamicParameters
    {
        // 1. Usamos el contenedor nativo de Dapper para los parámetros estándar (int, string, etc.)
        private readonly DynamicParameters _dynamicParameters = new();

        // 2. Usamos una lista interna para capturar los parámetros puramente nativos de Oracle
        private readonly List<OracleParameter> _oracleParameters = [];

        /// Absorbe todas las propiedades de un objeto de C# (como un DTO o un objeto anónimo) y las convierte automáticamente en parámetros estándar.
        public void AddDynamicParams(object param)
        {
            _dynamicParameters.AddDynamicParams(param);
        }

        /// Permite agregar manualmente parámetros específicos de Oracle usando el enum oficial OracleDbType.
        public void Add(string name, OracleDbType dbType, ParameterDirection direction, object? value = null, int? size = null)
        {
            var oracleParameter = new OracleParameter(name, dbType, value, direction);
            if (size.HasValue)
            {
                oracleParameter.Size = size.Value;
            }
            _oracleParameters.Add(oracleParameter);
        }

        /// Este método viene de la interfaz de Dapper. Se dispara en un segundo plano inmediatamente antes de ejecutar el Stored Procedure.
        public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            // A. Primero le decimos a Dapper que inyecte los parámetros del DTO que ya procesó
            ((SqlMapper.IDynamicParameters)_dynamicParameters).AddParameters(command, identity);

            // B. Segundo, interceptamos el comando y le inyectamos los parámetros exclusivos de Oracle (el RefCursor)
            if (command is OracleCommand oracleCommand)
            {
                oracleCommand.BindByName = true;
                oracleCommand.Parameters.AddRange(_oracleParameters.ToArray());
            }
        }
    }
}