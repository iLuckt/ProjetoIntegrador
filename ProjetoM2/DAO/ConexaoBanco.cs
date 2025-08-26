using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace ProjetoM2.DAO
{
    public class BancoInstance : IDisposable
    {
        public Banco Banco { get { return Banco.Instance; } }

        public BancoInstance()
        {
            Banco.Instance.Connect();
        }

        public void Dispose()
        {
            Banco.Instance.Disconnect();
        }
    }

    public class Banco
    {
        private static string _strCon = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppContext.BaseDirectory.Split("\\bin")[0]}\\DAO\\MeuBanco.mdf;Integrated Security=True";
        private static volatile Banco instance;
        private static object syncRoot = new Object();
        private static int utilizacoes = 0;
        private static SqlConnection con = null;
        private static SqlTransaction trans = null;
        public string Erro { get; set; }

        private Banco()
        {
            utilizacoes = 0;
        }

        public static Banco Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Banco();
                    }
                }
                return instance;
            }
        }

        public bool Connect()
        {
            bool resultado = false;
            Erro = "";
            try
            {
                lock (syncRoot)
                {
                    if (utilizacoes == 0)
                    {
                        con = new SqlConnection(_strCon);
                        con.Open();
                    }
                    utilizacoes++;
                    resultado = true;
                }
            }
            catch (Exception e)
            {
                Erro = "Não foi possível conectar ao banco de dados: " + e.Message;
                Console.Out.WriteLine("Erro Connect(): " + e.Message);
            }
            return resultado;
        }

        public void Disconnect()
        {
            try
            {
                lock (syncRoot)
                {
                    utilizacoes--;
                    if ((utilizacoes == 0) && (con != null) && (con.State == System.Data.ConnectionState.Open))
                    {
                        con.Close();
                        con = null;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Erro Disconnect(): " + e.Message);
            }
        }

        public bool BeginTransaction()
        {
            Erro = "";
            try
            {
                if ((con != null) && (con.State == System.Data.ConnectionState.Open))
                {
                    trans = con.BeginTransaction();
                    return true;
                }
            }
            catch (Exception e)
            {
                Erro = "Não foi possível iniciar a transação: " + e.Message;
                Console.Out.WriteLine("Erro BeginTransaction(): " + e.Message);
            }
            return false;
        }

        public bool CommitTransaction()
        {
            Erro = "";
            try
            {
                if ((con != null) && (trans != null) && (con.State == System.Data.ConnectionState.Open))
                {
                    trans.Commit();
                    trans = null;
                    return true;
                }
            }
            catch (Exception e)
            {
                Erro = "Não foi possível concluir a transação: " + e.Message;
                Console.Out.WriteLine("Erro CommitTransaction(): " + e.Message);
            }
            return false;
        }

        public bool RollbackTransaction()
        {
            Erro = "";
            try
            {
                if ((con != null) && (trans != null) && (con.State == System.Data.ConnectionState.Open))
                {
                    trans.Rollback();
                    trans = null;
                    return true;
                }
            }
            catch (Exception e)
            {
                Erro = "Não foi possível desfazer a transação: " + e.Message;
                Console.Out.WriteLine("Erro RollbackTransaction(): " + e.Message);
            }
            return false;
        }

        public int GetIdentity()
        {
            int identity = 0;
            GetColumn<int>(0, ref identity, "SELECT @@IDENTITY");
            return identity;
        }

        public bool ExecuteQuery(String sql, out DataTable dt, params Object[] parametros)
        {
            dt = new DataTable();
            Erro = "";
            try
            {
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Transaction = trans;
                for (int i = 0; i < parametros.Length; i += 2)
                    cmd.Parameters.AddWithValue(parametros[i].ToString(), parametros[i + 1]);
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
                dr.Close();
                return true;
            }
            catch (Exception e)
            {
                Erro = "Erro ao executar comando: " + e.Message;
                Console.Out.WriteLine("Erro ExecuteQuery(): " + e.Message);
                return false;
            }
        }

        public bool ExecuteNonQuery(String sql, params Object[] parametros)
        {
            Erro = "";
            try
            {
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Transaction = trans;
                for (int i = 0; i < parametros.Length; i += 2)
                    cmd.Parameters.AddWithValue(parametros[i].ToString(), parametros[i + 1]);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception e)
            {
                Erro = "Erro ao executar comando: " + e.Message;
                Console.Out.WriteLine("Erro ExecuteNonQuery(): " + e.Message);
                return false;
            }
        }

        public bool Found(String sql, params Object[] parametros)
        {
            bool found = false;
            try
            {
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Transaction = trans;
                for (int i = 0; i < parametros.Length; i += 2)
                    cmd.Parameters.AddWithValue(parametros[i].ToString(), parametros[i + 1]);
                SqlDataReader dr = cmd.ExecuteReader();
                found = dr.HasRows;
                dr.Close();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Erro Found(): " + e.Message);
            }
            return found;
        }

        public bool GetColumn<tiporetorno>(dynamic columnname, ref tiporetorno valor, String sql, params Object[] parametros)
        {
            bool result = false;
            try
            {
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Transaction = trans;
                for (int i = 0; i < parametros.Length; i += 2)
                    cmd.Parameters.AddWithValue(parametros[i].ToString(), parametros[i + 1]);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    valor = (tiporetorno)dr[columnname];
                    result = true;
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Erro GetColumn(): " + ex.Message);
            }
            return result;
        }

        // Métodos adicionais para compatibilidade
        public SqlConnection GetConnection()
        {
            return con;
        }

        public bool ExecuteStoredProcedure(string storedProcedure, params Object[] parametros)
        {
            Erro = "";
            try
            {
                SqlCommand cmd = new SqlCommand(storedProcedure, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Transaction = trans;
                for (int i = 0; i < parametros.Length; i += 2)
                    cmd.Parameters.AddWithValue(parametros[i].ToString(), parametros[i + 1]);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception e)
            {
                Erro = "Erro ao executar stored procedure: " + e.Message;
                Console.Out.WriteLine("Erro ExecuteStoredProcedure(): " + e.Message);
                return false;
            }
        }
    }
}