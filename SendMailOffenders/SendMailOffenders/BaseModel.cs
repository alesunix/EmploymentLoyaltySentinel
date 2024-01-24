using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace SendMailOffenders
{
    internal class BaseModel
    {
        private string ConnString { get; set; }
        private static string[] line = File.ReadAllLines("Config.ini");
        private string userId = line[0].Split('=')[1].Trim();
        private string password = line[1].Split('=')[1].Trim();
        private string dataSource = line[2].Split('=')[1].Trim();
        public static int Hour = Convert.ToInt32(line[3].Split('=')[1].Trim());
        public static int Minute = Convert.ToInt32(line[4].Split('=')[1].Trim());
        public BaseModel()
        {
            GenerateConn();
        }
        private void GenerateConn()
        {
            var connString = new OracleConnectionStringBuilder()
            {
                UserID = userId,
                Password = password,
                DataSource = dataSource,
                ConnectionTimeout = 30
            };
            ConnString = connString.ToString();
        }
        public Dictionary<string, string> GetDictionary(string query)
        {
            var list = new Dictionary<string, string>();
            using (OracleConnection con = new OracleConnection(ConnString))
            {
                con.Open();
                var cmd = new OracleCommand(query, con);
                OracleDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    list.Add(dr.GetValue(0).ToString(), dr.GetValue(1).ToString());
                }
                con.Close();
                con.Dispose();
                return list;
            }
        }
        public List<string> GetList(string query)
        {
            var list = new List<string>();
            using (var con = new OracleConnection(ConnString))
            {
                con.Open();
                var cmd = new OracleCommand(query, con);
                OracleDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    list.Add(dr.GetValue(0).ToString());
                }
                con.Close();
                con.Dispose();
                return list;
            }
        }
        public List<OffendersCheck> GetListOffendersCheck(string query)
        {
            var list = new List<OffendersCheck>();
            using (var con = new OracleConnection(ConnString))
            {
                con.Open();
                var cmd = new OracleCommand(query, con);
                OracleDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    var offenders = new OffendersCheck()
                    {
                        Employee = dr.GetValue(0).ToString(),
                        Dr = dr.GetDateTime(1),
                        Dol = dr.GetValue(2).ToString(),
                        Grpprg = dr.GetValue(3).ToString(),
                    };
                    list.Add(offenders);
                }
                con.Close();
                con.Dispose();
                return list;
            }
        }
    }
}
