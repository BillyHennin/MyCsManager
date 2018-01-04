// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

using System;
using System.Data;
using System.Linq;

namespace MANAGER.Connection
{
    public class Connection
    {
        public static string Database { private get; set; }

        public static IDbCommand Command(string query)
        {
            IDbCommand command;
            switch(Database)
            {
                case "Oracle":
                    command = ConnectionOracle.Command(query);
                    break;
                case "SqlServer":
                    command = ConnectionSqlServer.Command(query);
                    break;
                case "MySql":
                    command = ConnectionMySql.Command(query);
                    break;
                default:
                    command = ConnectionOracle.Command(query);
                    break;
            }
            command.Prepare();
            return command;
        }

        public static IDbCommand CommandStored(string query)
        {
            IDbCommand command;
            switch(Database)
            {
                case "Oracle":
                    command = ConnectionOracle.CommandStored(query);
                    break;
                case "SqlServer":
                    command = ConnectionSqlServer.CommandStored(query);
                    break;
                case "MySql":
                    command = ConnectionMySql.CommandStored(query);
                    break;
                default:
                    command = ConnectionOracle.CommandStored(query);
                    break;
            }
            command.Prepare();
            return command;
        }

        public static IDbCommand GetAll(string tableQuery) 
            => Command($"SELECT * FROM {tableQuery}");
        public static int SizeOf(IDbCommand command) 
            => Convert.ToInt32(command.ExecuteScalar());
        public static int SizeOf(string query) 
            => SizeOf(Command($"SELECT COUNT(*) FROM ({query})"));
        public static object GetUniqueCell(string query) 
            => Command(query).ExecuteScalar();
        public static void Insert(string tableQuery, params object[] value)
        {
            var query = value.Aggregate(string.Empty, (current, field) => current + ("'" + field) + "',");
            query = query.Substring(0, query.Length - 1);
            Command($"INSERT INTO {tableQuery} VALUES ({query})").ExecuteNonQuery();
        }
        public static void Delete(string tableQuery, object id, string param = null)
            => Command($"DELETE FROM {tableQuery} WHERE ID_{param ?? tableQuery} = {id}").ExecuteNonQuery();

        public static void Update(string tableQuery, int id, string[,] value)
        {
            var query = string.Empty;
            var size = value.Length / 2;
            for(var i = 0; i < size; i++)
            {
                query += $"{value[i, 0]} = '{value[i, 1]}' ,";
            }
            query = string.Format("UPDATE {0} SET {2} WHERE ID_{0} = {1}", tableQuery, id, query.Substring(0, query.Length - 1));
            Command(query).ExecuteNonQuery();
        }
    }
}