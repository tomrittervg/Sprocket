using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Sprocket.SQL
{
    public static class Connections
    {
        public static SqlConnection GetConnection(string serverName, string database)
        {
            SqlConnectionStringBuilder consb = new SqlConnectionStringBuilder();
            consb.DataSource = serverName;
            consb.IntegratedSecurity = true;//Use Windows Authentication
            consb.InitialCatalog = database;

            var conn = new SqlConnection(consb.ConnectionString);
            return conn;
        }

    }
}

