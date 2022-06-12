using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SQLDB
{
    public class Table
    {
        public string[] ColumnsName { get; private set; }
        public string Name { get; private set; }
        private SqlConnection _connection;

        public static Table CreateTable(SqlConnection connection, string name, params string[] columns)
        {
            var query = @"CREATE TABLE " + name + " (" + string.Join(", ", columns) + ");";
            var command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();
            return GetTable(connection, name);
        }

        public static Table GetTable(SqlConnection connection, string name)
        {
            var restrictions = new string[4] { null, null, name, null };
            var columnList = connection.GetSchema("Columns", restrictions).AsEnumerable().Select(s => s.Field<string>("Column_Name")).ToList();
            var table = new Table();
            var colList = new List<string>();
            var dataTable = new DataTable();
            var cmdString = $"SELECT TOP 0 * FROM {name}";
            using (SqlDataAdapter dataContent = new SqlDataAdapter(cmdString, connection))
            {
                dataContent.Fill(dataTable);
                foreach (DataColumn col in dataTable.Columns)
                {
                    colList.Add(col.ColumnName);
                }
            }
            table._connection = connection;
            table.Name = name;
            table.ColumnsName = colList.ToArray();
            return table;
        }

        public void UpdateWhole(List<List<object>> data)
        {
            Clear();
            foreach (var row in data)
            {
                Append(row);
            }
        }

        public IList<IList<object>> Read(string condition = "")
        {
            var result = new List<IList<object>>();
            var query = @"SELECT * FROM " + Name + (condition.Length > 0 ? " WHERE " + condition : "");
            var command = new SqlCommand(query, _connection);
            var reader = command.ExecuteReader();
            if (!reader.HasRows)
                return null;
            while (reader.Read())
            {
                var row = new List<object>();
                var columns = reader.FieldCount;
                for (var column = 0; column < columns; column++)
                {
                    var value = reader.GetValue(column);
                    row.Add(value);
                }
                result.Add(row);
            }
            reader.Close();
            return result;
        }

        public void Append(List<object> data)
        {
            var query = @"INSERT INTO " + Name + " VALUES(@" + string.Join(", @", ColumnsName) + ");";
            var command = new SqlCommand(query, _connection);
            for (var i = 0; i < ColumnsName.Length; i++)
            {
                var rowName = "@" + ColumnsName[i];
                command.Parameters.AddWithValue(rowName, data[i]);
            }
            command.ExecuteNonQuery();
        }

        public void Update(string condition, List<object> data, params string[] rowNames)
        {
            var set = " SET ";
            for (int i = 0; i < rowNames.Length; i++)
            {
                if (i != 0)
                    set += ", ";
                set += rowNames[i] + " = @" + rowNames[i];
            }
            var query = @"UPDATE " + Name + set + " WHERE " + condition + ";";
            var command = new SqlCommand(query, _connection);
            for (var i = 0; i < data.Count; i++)
            {
                var rowName = "@" + rowNames[i];
                command.Parameters.AddWithValue(rowName, data[i]);
            }
            command.ExecuteNonQuery();
        }

        public void Clear()
        {
            var query = @"TRUNCATE TABLE " + Name + ";";
            var command = new SqlCommand(query, _connection);
            command.ExecuteNonQuery();
        }
    }
}