using ProjetoM2.DAO;
using System;
using System.Data;

namespace ProjetoM2.Controller
{
    public class AlunoController
    {
        public static bool CadastrarAluno(string nome, out int codigoAluno)
        {
            codigoAluno = 0;

            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException("Nome do aluno não pode ser vazio ou nulo.");
            }

            using (var bancoInstance = new BancoInstance())
            {
                try
                {
                    // Iniciar transação
                    if (!Banco.Instance.BeginTransaction())
                        throw new Exception("Não foi possível iniciar a transação");

                    // Inserir o novo aluno com saldo inicial 0
                    bool sucessoInsert = Banco.Instance.ExecuteNonQuery(
                        "INSERT INTO Alunos (nome, saldo_impressoes) VALUES (@nome, 0)",
                        "@nome", nome.Trim()
                    );

                    if (!sucessoInsert)
                        throw new Exception("Falha ao inserir aluno");

                    // Obter o código do aluno inserido
                    codigoAluno = Banco.Instance.GetIdentity();

                    if (codigoAluno == 0)
                        throw new Exception("Falha ao obter código do aluno");

                    // Registrar no histórico o cadastro com saldo inicial 0
                    bool sucessoHistorico = Banco.Instance.ExecuteNonQuery(
                        "INSERT INTO Historico (alunos_codigo, data_historico, tipo_movimentacao, quantidade_movimentada, saldo_antes, saldo_depois) " +
                        "VALUES (@aluno_codigo, GETDATE(), 1, 0, 0, 0)",
                        "@aluno_codigo", codigoAluno
                    );

                    if (!sucessoHistorico)
                        throw new Exception("Falha ao registrar histórico");

                    // Commit da transação
                    if (!Banco.Instance.CommitTransaction())
                        throw new Exception("Falha ao confirmar transação");

                    return true;
                }
                catch (Exception ex)
                {
                    // Rollback em caso de erro
                    Banco.Instance.RollbackTransaction();
                    throw new Exception($"Erro ao cadastrar aluno: {ex.Message}");
                }
            }
        }

        public static Aluno ObterAluno(int codigo)
        {
            using (var bancoInstance = new BancoInstance())
            {
                try
                {
                    DataTable dt;
                    bool sucesso = Banco.Instance.ExecuteQuery(
                        "SELECT * FROM Alunos WHERE codigo = @codigo",
                        out dt,
                        "@codigo", codigo
                    );

                    if (!sucesso)
                        throw new Exception(Banco.Instance.Erro);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        return new Aluno
                        {
                            codigo = Convert.ToInt32(row["codigo"]),
                            nome = row["nome"].ToString(),
                            saldo_impressoes = Convert.ToInt32(row["saldo_impressoes"])
                        };
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao obter aluno: {ex.Message}");
                }
            }
        }

        public static bool AdicionarCredito(int alunoCodigo, int quantidade)
        {
            using (var bancoInstance = new BancoInstance())
            {
                try
                {
                    bool sucesso = Banco.Instance.ExecuteStoredProcedure(
                        "sp_AdicionarCredito",
                        "@aluno_codigo", alunoCodigo,
                        "@quantidade", quantidade
                    );

                    if (!sucesso && !string.IsNullOrEmpty(Banco.Instance.Erro))
                    {
                        if (Banco.Instance.Erro.Contains("Aluno não encontrado"))
                            throw new Exception("Aluno não encontrado");
                        else
                            throw new Exception(Banco.Instance.Erro);
                    }

                    return sucesso;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao adicionar créditos: {ex.Message}");
                }
            }
        }

        public static bool RegistrarImpressao(int alunoCodigo, int quantidade)
        {
            using (var bancoInstance = new BancoInstance())
            {
                try
                {
                    bool sucesso = Banco.Instance.ExecuteStoredProcedure(
                        "sp_RegistrarImpressao",
                        "@aluno_codigo", alunoCodigo,
                        "@quantidade", quantidade
                    );

                    if (!sucesso && !string.IsNullOrEmpty(Banco.Instance.Erro))
                    {
                        if (Banco.Instance.Erro.Contains("Saldo insuficiente"))
                            throw new Exception("Saldo insuficiente para impressão");
                        else if (Banco.Instance.Erro.Contains("Aluno não encontrado"))
                            throw new Exception("Aluno não encontrado");
                        else
                            throw new Exception(Banco.Instance.Erro);
                    }

                    return sucesso;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao registrar impressão: {ex.Message}");
                }
            }
        }

        // Método auxiliar para verificar se aluno existe
        public static bool VerificarAlunoExistente(int alunoCodigo)
        {
            using (var bancoInstance = new BancoInstance())
            {
                try
                {
                    return Banco.Instance.Found(
                        "SELECT 1 FROM Alunos WHERE codigo = @codigo",
                        "@codigo", alunoCodigo
                    );
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao verificar aluno: {ex.Message}");
                }
            }
        }

        // Método para obter saldo do aluno
        public static int ObterSaldoAluno(int alunoCodigo)
        {
            using (var bancoInstance = new BancoInstance())
            {
                try
                {
                    int saldo = 0;
                    bool sucesso = Banco.Instance.GetColumn<int>(
                        "saldo_impressoes",
                        ref saldo,
                        "SELECT saldo_impressoes FROM Alunos WHERE codigo = @codigo",
                        "@codigo", alunoCodigo
                    );

                    if (!sucesso)
                        throw new Exception("Aluno não encontrado");

                    return saldo;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao obter saldo: {ex.Message}");
                }
            }
        }
    }
}