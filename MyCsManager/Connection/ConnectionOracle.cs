// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

using System.Data;

using Oracle.ManagedDataAccess.Client;

namespace MANAGER.Connection
{
    public class ConnectionOracle : Connection
    {
        private static OracleConnection _connection;
        private static bool _connectionIsStarted;

        private ConnectionOracle()
        {
            _connection = new OracleConnection(Properties.Connection.Default.DatabaseConnectionString);
            _connection.Open();
            _connectionIsStarted = true;
        }

        private static OracleConnection GetConnection()
        {
            if(!_connectionIsStarted)
            {
                new ConnectionOracle();
            }
            return _connection;
        }

        public new static IDbCommand Command(string query) 
            => new OracleCommand {Connection = GetConnection(), CommandText = query, BindByName = true};

        public new static IDbCommand CommandStored(string query) 
            => new OracleCommand {CommandType = CommandType.StoredProcedure, Connection = GetConnection(), CommandText = query, BindByName = true};
    }
}