using System.Collections.Generic;
using System.Data.SqlClient;

namespace SQLDB
{
    public class IO
    {
        private SqlConnection _connection;
        public List<Table> Tables { get; private set; }

        public IO(string serverName, string database)
        {
            var arguments = @"Server =.\" + serverName + "; Database = " + database + ";" + "Trusted_Connection = True;";
            _connection = new SqlConnection(arguments);
            _connection.Open();
            Tables = new List<Table>();
        }

        public SqlCommand SendNativeQuery(string query)
        {
            return new SqlCommand(query, _connection);
        }

        public bool IsTableExists(string tableName)
        {
            var query = "SELECT count(*) as IsExists FROM dbo.sysobjects where id = object_id('[dbo].[" + tableName + "]')";
            var cmd = new SqlCommand(query, _connection);
            var result = (int)cmd.ExecuteScalar();
            return result == 1;
        }

        public Table CreateTable(string tableName, params string[] columns)
        {
            var table = Table.CreateSQLTable(_connection, tableName, columns);
            Tables.Add(table);
            return table;
        }

        public Table LoadTable(string tableName)
        {
            if (IsAlreadyLoaded(tableName, out Table table))
                return table;
            table = Table.LoadSQLTable(_connection, tableName);
            Tables.Add(table);
            return table;
        }

        private bool IsAlreadyLoaded(string tableName, out Table sqlTable)
        {
            foreach (var table in Tables)
            {
                if (table.Name == tableName)
                {
                    sqlTable = table;
                    return true;
                }
            }
            sqlTable = null;
            return false;
        }

        public void DeleteTable(string tableName)
        {
            var query = @"DROP TABLE " + tableName;
            var command = new SqlCommand(query, _connection);
            command.ExecuteNonQuery();
            var index = 0;
            foreach (var table in Tables)
            {
                if (table.Name == tableName)
                {
                    Tables.RemoveAt(index);
                    break;
                }
                index++;
            }
        }
    }
}