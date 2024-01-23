using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Reflection;

namespace EmploymentLoyaltySentinel.Models
{
    public class BaseModel
    {
        public static string MyMessage { get; set; }
        private string ConnString { get; set; }
        public BaseModel()
        {
            GenerateConn();
        }
        private void GenerateConn()
        {
            OracleConnectionStringBuilder connString = new()
            {
                UserID = "схема",
                Password = "пароль",
                DataSource = "сервер",
                ConnectionTimeout = 30
            };
            ConnString = connString.ToString();
        }
        public List<UserAccount> GetUsers(string query)
        {
            List<UserAccount> list = new();
            using (OracleConnection con = new(ConnString))
            {
                con.Open();
                var cmd = new OracleCommand(query, con);
                OracleDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    UserAccount user = new()
                    {
                        Id = dr.GetInt32("Id"),
                        UserName = dr.GetString("Login"),
                        Password = Convert.ToString(dr.GetValue("Password")),
                        Role = dr.GetString("Cod"),
                        Deleted = dr.GetString("Deleted")
                    };
                    list.Add(user);
                }
            }
            return list;
        }
        public List<T> GetTList<T>(string query) where T : new()
        {
            List<T> list = new();
            using OracleConnection con = new(ConnString);
            con.Open();
            var cmd = new OracleCommand(query, con);
            OracleDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (dr.Read())
            {
                T t = new();
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    Type type = t.GetType();
                    string column = dr.GetName(i).ToLower();
                    string propName = column.First().ToString().ToUpper() + column.Substring(1);
                    PropertyInfo info = type.GetProperty(propName);
                    var obj = dr.GetValue(i);
                    if (obj == DBNull.Value)
                    {
                        obj = obj.ToString();
                    }
                    info?.SetValue(t, obj, null);
                }
                list.Add(t);
            }
            dr.Close();
            con.Close();
            con.Dispose();
            return list;
        }
        public DataTable GetTable(string query)
        {
            using (OracleConnection con = new(ConnString))
            {
                DataTable dt = new();
                con.Open();
                var cmd = new OracleCommand(query, con);
                OracleDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                dt.Load(dr);
                con.Close();
                con.Dispose();
                return dt;
            }
        }
        public List<string> GetList(string query)
        {
            List<string> list = new();
            using (OracleConnection con = new(ConnString))
            {
                con.Open();
                var cmd = new OracleCommand(query, con);
                OracleDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    list.Add(dr.GetValue(0).ToString());
                }
            }
            return list;
        }
        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string query)
        {
            Dictionary<TKey, TValue> list = new();
            using (OracleConnection con = new(ConnString))
            {
                con.Open();
                var cmd = new OracleCommand(query, con);
                OracleDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    TKey key = dr.GetFieldValue<TKey>(0);
                    TValue value = dr.GetFieldValue<TValue>(1);
                    list.Add(key, value);
                }
            }
            return list;
        }
        public object GetSingleResult(string query)
        {
            using (OracleConnection con = new(ConnString))
            {
                con.Open();
                var cmd = new OracleCommand(query, con).ExecuteScalar();
                con.Close();
                con.Dispose();
                return cmd;
            }
        }
        public int SetQuery(string query, byte[] bytes = null)
        {
            using (OracleConnection con = new(ConnString))
            {
                con.Open();
                var cmd = new OracleCommand(query, con);
                OracleParameter id = new();
                if (bytes != null)
                {
                    var blob = cmd.Parameters.Add("BLOB", OracleDbType.Blob);
                    id = cmd.Parameters.Add("ID", OracleDbType.Decimal);
                    blob.Direction = ParameterDirection.Input;
                    id.Direction = ParameterDirection.Output;
                    blob.Value = bytes;
                }
                else
                {
                    id.Direction = ParameterDirection.Output;
                    id = cmd.Parameters.Add("ID", OracleDbType.Decimal);
                }
                cmd.ExecuteNonQuery();
                con.Close();
                con.Dispose();               
                return id.Value != null ? Convert.ToInt32(id.Value.ToString()) : 0;
            }
        }
    }
}
