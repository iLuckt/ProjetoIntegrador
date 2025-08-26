using ProjetoM2.Controller;
using ProjetoM2.DAO;
using System;
using System.Collections.Generic;
using System.Data;

namespace ProjetoM2
{
    class Program
    {
        static void Main(string[] args)
        {

            bool continuar = true;

            while (continuar)
            {
                ExibirMenuPrincipal();
                string opcao = Console.ReadLine();

                switch (opcao)
                {
                    case "1":
                        CadastrarNovoAluno();
                        break;
                    case "2":
                        ComprarImpressoes();
                        break;
                    case "3":
                        RealizarImpressao();
                        break;
                    case "4":
                        ConsultarSaldoTodosAlunos();
                        break;
                    case "5":
                        ConsultarHistorico();
                        break;
                    case "6":
                        continuar = false;
                        Console.WriteLine("Saindo do sistema...");
                        break;
                    default:
                        Console.WriteLine("Opção inválida. Tente novamente.");
                        break;
                }

                if (continuar)
                {
                    Console.WriteLine("\nPressione qualquer tecla para continuar...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        static void ExibirMenuPrincipal()
        {
            Console.WriteLine("\n=== SISTEMA DE IMPRESSÕES SENAC ===");
            Console.WriteLine("1. Cadastrar novo aluno");
            Console.WriteLine("2. Comprar impressões");
            Console.WriteLine("3. Realizar impressão");
            Console.WriteLine("4. Consultar saldo de todos os alunos");
            Console.WriteLine("5. Consultar histórico");
            Console.WriteLine("6. Sair");
            Console.Write("Escolha uma opção: ");
        }

        static bool Voltar()
        {
            Console.WriteLine("Pressione Enter para continuar ou Digite 0 para voltar ao menu:");
            string entrada = Console.ReadLine();
            return entrada == "0";
        }


        static void CadastrarNovoAluno()
        {
            Console.WriteLine("\n=== CADASTRAR NOVO ALUNO ===");
            if (Voltar()) return;

            Console.Write("Digite o nome do aluno: ");
            string nome = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nome))
            {
                Console.WriteLine("Nome não pode ser vazio.");
                return;
            }

            try
            {
                int codigoAluno;
                bool sucesso = Aluno.CadastrarAluno(nome, out codigoAluno);

                    Console.WriteLine($"Aluno '{nome}' cadastrado com sucesso! Código: {codigoAluno}");
                    Console.WriteLine("Saldo inicial: 0 impressões");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        static void ComprarImpressoes()
        {
            Console.WriteLine("\n=== COMPRAR IMPRESSÕES ===");
            if (Voltar()) return;

            // Buscar aluno
            var aluno = BuscarAlunoPorNome();
            if (aluno == null) return;

            Console.WriteLine($"\nAluno: {aluno.nome}");
            Console.WriteLine($"Saldo atual: {aluno.saldo_impressoes} impressões");
            Console.WriteLine("\nPacotes disponíveis: 25 ou 50");
            Console.Write("Quantas impressões deseja comprar? ");

            if (int.TryParse(Console.ReadLine(), out int quantidade))
            {
                if (quantidade != 25 && quantidade != 50)
                {
                    Console.WriteLine("Quantidade inválida! Apenas pacotes de 25 ou 50 são permitidos.");
                    return;
                }

                try
                {
                    // Definir preço unitário (apenas para registro, não usado no cálculo do saldo)
                    decimal precoUnitario = 0.10m;

                    bool sucesso = Compra.ProcessarCompra(aluno.codigo, quantidade, precoUnitario);

                    if (sucesso)
                    {
                        Console.WriteLine($"Compra realizada com sucesso!");
                        Console.WriteLine($"Novo saldo: {aluno.saldo_impressoes + quantidade} impressões");
                    }
                    else
                    {
                        Console.WriteLine("Erro ao processar compra.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Quantidade inválida.");
            }
        }

        static void RealizarImpressao()
        {
            Console.WriteLine("\n=== REALIZAR IMPRESSÃO ===");
            if (Voltar()) return;

            // Buscar aluno
            var aluno = BuscarAlunoPorNome();
            if (aluno == null) return;

            // BUSCAR SALDO ATUALIZADO (IMPORTANTE!)
            int saldoAtual = AlunoController.ObterSaldoAluno(aluno.codigo);

            Console.WriteLine($"\nAluno: {aluno.nome}");
            Console.WriteLine($"Saldo atual: {saldoAtual} impressões");
            Console.Write("Quantas páginas deseja imprimir? ");

            if (int.TryParse(Console.ReadLine(), out int quantidade) && quantidade > 0)
            {
                if (saldoAtual < quantidade)
                {
                    Console.WriteLine("Saldo insuficiente. Impressão não realizada.");
                    return;
                }

                try
                {
                    bool sucesso = Aluno.RegistrarImpressao(aluno.codigo, quantidade);

                    if (sucesso)
                    {
                        // BUSCAR NOVO SALDO ATUALIZADO APÓS A IMPRESSÃO
                        int novoSaldo = AlunoController.ObterSaldoAluno(aluno.codigo);

                        Console.WriteLine("Impressão realizada com sucesso!.");
                        Console.WriteLine($"Novo saldo: {novoSaldo} impressões");
                    }
                    else
                    {
                        Console.WriteLine("Erro ao registrar impressão.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Quantidade inválida. Deve ser um número maior que zero.");
            }
        }

            static void ConsultarSaldoTodosAlunos()
        {
            Console.WriteLine("\n=== SALDO DE IMPRESSÕES ===");
            if (Voltar()) return;

            try
            {
                using (var bancoInstance = new BancoInstance())
                {
                    DataTable dt;
                    bool sucesso = Banco.Instance.ExecuteQuery(
                        "SELECT nome, saldo_impressoes FROM Alunos ORDER BY nome",
                        out dt
                    );

                    if (!sucesso)
                    {
                        Console.WriteLine("Erro ao consultar alunos.");
                        return;
                    }

                    if (dt.Rows.Count == 0)
                    {
                        Console.WriteLine("Nenhum aluno cadastrado.");
                    }
                    else
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            string nome = row["nome"].ToString();
                            int saldo = Convert.ToInt32(row["saldo_impressoes"]);
                            Console.WriteLine($"{nome}: {saldo} impressões");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        static void ConsultarHistorico()
        {
            Console.WriteLine("\n=== CONSULTAR HISTÓRICO ===");
            if (Voltar()) return;

            // Buscar aluno
            var aluno = BuscarAlunoPorNome();
            if (aluno == null) return;

            try
            {
                // Consultar histórico do aluno
                using (var bancoInstance = new BancoInstance())
                {
                    DataTable dt;
                    bool sucesso = Banco.Instance.ExecuteQuery(@"
                        SELECT data_historico, tipo_movimentacao, quantidade_movimentada, saldo_antes, saldo_depois
                        FROM Historico 
                        WHERE alunos_codigo = @aluno_codigo
                        ORDER BY data_historico DESC",
                        out dt,
                        "@aluno_codigo", aluno.codigo
                    );

                    if (!sucesso)
                    {
                        Console.WriteLine("Erro ao consultar histórico.");
                        return;
                    }

                    Console.WriteLine($"\nHistórico de {aluno.nome}:");

                    if (dt.Rows.Count == 0)
                    {
                        Console.WriteLine("Nenhuma movimentação realizada.");
                    }
                    else
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            DateTime data = Convert.ToDateTime(row["data_historico"]);
                            bool tipoMovimentacao = Convert.ToBoolean(row["tipo_movimentacao"]);
                            int quantidade = Convert.ToInt32(row["quantidade_movimentada"]);
                            int saldoAntes = Convert.ToInt32(row["saldo_antes"]);
                            int saldoDepois = Convert.ToInt32(row["saldo_depois"]);

                            string operacao = tipoMovimentacao ? "COMPRA" : "IMPRESSÃO";

                            Console.WriteLine($"Data: {data:dd/MM/yy HH:mm} Operação: {operacao} Quantidade: {quantidade}");
                            Console.WriteLine($"  Saldo antes: {saldoAntes} | Saldo depois: {saldoDepois}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        static Aluno BuscarAlunoPorNome()

        {
            Console.Write("Digite o nome do aluno: ");
            string nome = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nome))
            {
                Console.WriteLine("Nome não pode ser vazio.");
                return null;
            }

            try
            {
                using (var bancoInstance = new BancoInstance())
                {
                    DataTable dt;
                    bool sucesso = Banco.Instance.ExecuteQuery(
                        "SELECT codigo, nome, saldo_impressoes FROM Alunos WHERE nome = @nome",
                        out dt,
                        "@nome", nome
                    );

                    if (!sucesso)
                    {
                        Console.WriteLine("Erro ao buscar aluno.");
                        return null;
                    }

                    if (dt.Rows.Count == 0)
                    {
                        Console.WriteLine("Aluno não encontrado.");
                        return null;
                    }

                    DataRow row = dt.Rows[0];
                    return new Aluno
                    {
                        codigo = Convert.ToInt32(row["codigo"]),
                        nome = row["nome"].ToString(),
                        saldo_impressoes = Convert.ToInt32(row["saldo_impressoes"])
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                return null;
            }
        }
    }
}