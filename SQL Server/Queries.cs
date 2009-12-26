using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Sprocket.SQL
{
    public static class Queries
    {
        public static List<SQLParam> GetStoredProcParameters(string serverName, string database, string procName)
        {
            var conn = Connections.GetConnection(serverName, database);
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"SELECT parm.name AS [name],        
                                typ.name AS [type]
                                FROM sys.procedures sp with (nolock) 
                                INNER JOIN sys.parameters parm with (nolock) ON sp.object_id = parm.object_id
                                INNER JOIN sys.types typ with (nolock) ON parm.system_type_id = typ.system_type_id
                                WHERE sp.name = @procName";
            cmd.Parameters.AddWithValue("@procName", procName);

            conn.Open();
            var results = cmd.ExecuteReader();
            
            List<SQLParam> paramList = new List<SQLParam>(5);
            while (results.Read())
            {
                paramList.Add(new SQLParam(results["name"].ToString(), results["type"].ToString()));
            }
            conn.Close();

            return paramList;
        }
    }
}
