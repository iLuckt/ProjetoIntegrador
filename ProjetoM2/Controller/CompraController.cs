using ProjetoM2.DAO;
using System;
using System.Collections.Generic;
using System.Data;

namespace ProjetoM2.Controller
{
    public static class CompraController
    {
        public static bool RegistrarCompra(CompraModel compra)
        {

            try
            {
                using (var bancoInstance = new BancoInstance())
                {
                    bool sucesso = Banco.Instance.ExecuteStoredProcedure(
                        "sp_RegistrarCompra",
                        "@aluno_codigo", compra.AlunoCodigo,
                        "@quantidade_comprada", compra.QuantidadeComprada
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
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao registrar compra: {ex.Message}");
            }
        }

        public static bool RegistrarCompra()
        {
            throw new NotImplementedException();
        }

        public static List<HistoricoCompraModel> ObterHistoricoCompras(int alunoCodigo)
        {
            try
            {
                var historico = new List<HistoricoCompraModel>();

                using (var bancoInstance = new BancoInstance())
                {
                    DataTable dt;
                    bool sucesso = Banco.Instance.ExecuteQuery(@"
                        SELECT c.codigo, c.data_compra, c.quantidade_comprada, a.nome as aluno_nome
                        FROM Compras c
                        INNER JOIN Alunos a ON c.alunos_codigo = a.codigo
                        WHERE c.alunos_codigo = @aluno_codigo
                        ORDER BY c.data_compra DESC",
                        out dt,
                        "@aluno_codigo", alunoCodigo
                    );

                    if (!sucesso)
                        throw new Exception(Banco.Instance.Erro);

                    foreach (DataRow row in dt.Rows)
                    {
                        historico.Add(new HistoricoCompraModel
                        {
                            Codigo = Convert.ToInt32(row["codigo"]),
                            DataCompra = Convert.ToDateTime(row["data_compra"]),
                            QuantidadeComprada = Convert.ToInt32(row["quantidade_comprada"]),
                            AlunoNome = row["aluno_nome"].ToString()
                        });
                    }
                }

                return historico;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter histórico: {ex.Message}");
            }
        }

        public static SaldoModel ObterSaldoAluno(int alunoCodigo)
        {
            try
            {
                using (var bancoInstance = new BancoInstance())
                {
                    DataTable dt;
                    bool sucesso = Banco.Instance.ExecuteQuery(
                        "SELECT nome, saldo_impressoes FROM Alunos WHERE codigo = @aluno_codigo",
                        out dt,
                        "@aluno_codigo", alunoCodigo
                    );

                    if (!sucesso)
                        throw new Exception(Banco.Instance.Erro);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        return new SaldoModel
                        {
                            Aluno = row["nome"].ToString(),
                            Saldo = Convert.ToInt32(row["saldo_impressoes"])
                        };
                    }
                    else
                    {
                        throw new Exception("Aluno não encontrado");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao obter saldo: {ex.Message}");
            }
        }

        public static decimal CalcularTotalCompra(int quantidade, decimal precoUnitario)
        {
            if (quantidade <= 0 || precoUnitario <= 0)
            {
                throw new ArgumentException("Quantidade e preço unitário devem ser valores positivos.");
            }

            return quantidade * precoUnitario;
        }

        public static bool ProcessarCompra(int alunoCodigo, int quantidade, decimal precoUnitario)
        {
            if (!VerificarAlunoExistente(alunoCodigo))
            {
                throw new ArgumentException("Aluno não encontrado.");
            }

            decimal totalCompra = CalcularTotalCompra(quantidade, precoUnitario);

            using (var bancoInstance = new BancoInstance())
            {
                try
                {
                    // Iniciar transação
                    if (!Banco.Instance.BeginTransaction())
                        throw new Exception("Não foi possível iniciar a transação");

                    // Registrar a compra
                    bool sucessoCompra = Banco.Instance.ExecuteNonQuery(
                        "INSERT INTO Compras (alunos_codigo, data_compra, quantidade_comprada, valor_total) VALUES (@aluno_codigo, GETDATE(), @quantidade, @valor_total)",
                        "@aluno_codigo", alunoCodigo,
                        "@quantidade", quantidade,
                        "@valor_total", totalCompra
                    );

                    if (!sucessoCompra)
                        throw new Exception("Falha ao registrar compra");

                    // Atualizar saldo do aluno
                    bool sucessoSaldo = Banco.Instance.ExecuteNonQuery(
                        "UPDATE Alunos SET saldo_impressoes = saldo_impressoes + @quantidade WHERE codigo = @aluno_codigo",
                        "@aluno_codigo", alunoCodigo,
                        "@quantidade", quantidade
                    );

                    if (!sucessoSaldo)
                        throw new Exception("Falha ao atualizar saldo");

                    // Registrar no histórico
                    bool sucessoHistorico = Banco.Instance.ExecuteNonQuery(
                        "INSERT INTO Historico (alunos_codigo, data_historico, tipo_movimentacao, quantidade_movimentada, saldo_antes, saldo_depois) " +
                        "SELECT @aluno_codigo, GETDATE(), @tipo_movimentacao, @quantidade, saldo_impressoes, saldo_impressoes + @quantidade " +
                        "FROM Alunos WHERE codigo = @aluno_codigo",
                        "@aluno_codigo", alunoCodigo,
                        "@tipo_movimentacao", 1, // 1 para compra
                        "@quantidade", quantidade
                    );

                    if (!sucessoHistorico)
                        throw new Exception("Falha ao registrar histórico");

                    // Commit da transação
                    if (!Banco.Instance.CommitTransaction())
                        throw new Exception("Falha ao confirmar transação");

                    Console.WriteLine($"Compra processada com sucesso para o aluno {alunoCodigo}:");
                    Console.WriteLine($"Quantidade: {quantidade}, Preço Unitário: {precoUnitario:C}, Total: {totalCompra:C}");

                    return true;
                }
                catch (Exception ex)
                {
                    // Rollback em caso de erro
                    Banco.Instance.RollbackTransaction();
                    Console.WriteLine($"Erro ao processar compra: {ex.Message}");
                    return false;
                }
            }
        }

        public static bool VerificarAlunoExistente(int alunoCodigo)
        {
            using (var bancoInstance = new BancoInstance())
            {
                try
                {
                    return Banco.Instance.Found(
                        "SELECT 1 FROM Alunos WHERE codigo = @aluno_codigo",
                        "@aluno_codigo", alunoCodigo
                    );
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao verificar aluno: {ex.Message}");
                }
            }
        }
    }

    // Model classes
    public class CompraModel
    {
        public int AlunoCodigo { get; set; }
        public int QuantidadeComprada { get; set; }
    }

    public class HistoricoCompraModel
    {
        public int Codigo { get; set; }
        public DateTime DataCompra { get; set; }
        public int QuantidadeComprada { get; set; }
        public string AlunoNome { get; set; }
    }

    public class SaldoModel
    {
        public string Aluno { get; set; }
        public int Saldo { get; set; }
    }
}