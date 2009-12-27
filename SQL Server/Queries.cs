﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
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
                paramList.Add(new SQLParam(results["name"].ToString(), results["type"].ToString()));
            
            conn.Close();

            return paramList;
        }

        public static void CreateStoredProcedure(string procText, string server, string database)
        {
            var conn = Connections.GetConnection(server, database);
            var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = procText;

            conn.Open();
            var result = cmd.ExecuteNonQuery();
            conn.Close();
        }
        
        private static Regex ProcParsingRegex = new Regex("\\s*(create|alter)\\s+procedure|proc\\s+(\\w+)\\s*\\(?\\s*@", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex ProcCleaningRegex = new Regex("\\s*--.+$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);
        public static string TurnFileIntoProcedure(string filename, string server, string database)
        {
            string fileText = File.ReadAllText(filename);
            string procText = ProcCleaningRegex.Replace(fileText, "");

            var matchResults = ProcParsingRegex.Match(procText);

            if (matchResults.Success)
            {
                var createOrAlter = matchResults.Groups[0];
                if (createOrAlter.Value.ToLower() == "alter")
                    procText.ReplaceFirst(createOrAlter.Value, "create");

                var name = matchResults.Groups[1];
                var newName = "sprockettestrun" + "_" + 
                    (MainWindow.CurrentProcess.Id | MainWindow.CurrentProcess.MachineName.GetHashCode()).ToString() + "_" +
                    MainWindow.rndm.Next(9999).ToString() + "_" + 
                    name;
                procText.ReplaceFirst(matchResults.Groups[1].Value, newName);

                CreateStoredProcedure(procText, server, database);

                return newName;
            }
            else
            {
                throw new WTFException();
            }
        }
    }
}