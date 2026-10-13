using Dapper;
using FileProcessor.Core.Constants.Database;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace FileProcessor.Infrastructure.Persistence
{
    public static class OracleConnectionExtensions
    {
        /// 1. PARA LISTAS: Ejecuta un Stored Procedure que devuelve múltiples registros a través de un RefCursor.
        public static async Task<IEnumerable<T>> QueryOracleAsync<T>(
            this IDbConnection connection,
            string storedProcedure,
            object? param = null,
            string cursorName = ParametersConstants.CURSOR)
        {
            var dyParams = BuildParametersWithCursor(param, cursorName);
            return await connection.QueryAsync<T>(storedProcedure, dyParams, commandType: CommandType.StoredProcedure);
        }

        /// 2. PARA UN SOLO OBJETO: Ejecuta un Stored Procedure y devuelve solo el primer registro (o null) desde un RefCursor.
        public static async Task<T?> QueryOracleFirstOrDefaultAsync<T>(
            this IDbConnection connection,
            string storedProcedure,
            object? param = null,
            string cursorName = ParametersConstants.CURSOR)
        {
            var dyParams = BuildParametersWithCursor(param, cursorName);
            return await connection.QueryFirstOrDefaultAsync<T>(storedProcedure, dyParams, commandType: CommandType.StoredProcedure);
        }

        /// 3. PARA MODIFICACIONES (CUD): Ejecuta operaciones puras de CREATE, UPDATE o DELETE que NO devuelven un RefCursor y retorna el número de filas afectadas en la base de datos.
        public static async Task<int> ExecuteOracleAsync(
            this IDbConnection connection,
            string storedProcedure,
            object? param = null)
        {
            OracleParameters? dyParams = null;

            if (param != null)
            {
                dyParams = new OracleParameters();
                dyParams.AddDynamicParams(param);
            }

            return await connection.ExecuteAsync(storedProcedure, dyParams, commandType: CommandType.StoredProcedure);
        }


        /// Método auxiliar privado para empaquetar los parámetros e inyectar el RefCursor automáticamente.
        private static OracleParameters BuildParametersWithCursor(object? param, string cursorName)
        {
            var dyParams = new OracleParameters();

            if (param != null)
            {
                dyParams.AddDynamicParams(param);
            }

            dyParams.Add(cursorName, OracleDbType.RefCursor, ParameterDirection.Output);

            return dyParams;
        }
    }
}