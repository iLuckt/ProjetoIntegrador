using ProjetoM2.Controller;
using System;
using System.Collections.Generic;

namespace ProjetoM2.DAO
{
    public class Compra
    {
        public Compra(int codigo, int aluno_codigo, DateTime data_compra, int quantidade_comprada, decimal valor_total)
        {
            this.codigo = codigo;
            this.aluno_codigo = aluno_codigo;
            this.data_compra = data_compra;
            this.quantidade_comprada = quantidade_comprada;
            this.valor_total = valor_total;
        }

        public Compra() { }

        public int codigo { get; set; }
        public int aluno_codigo { get; set; }
        public DateTime data_compra { get; set; }
        public int quantidade_comprada { get; set; }
        public decimal valor_total { get; set; }


        public static bool RegistrarCompra(CompraModel compra)
                => CompraController.RegistrarCompra(compra);

        public static List<HistoricoCompraModel> ObterHistoricoCompras(int alunoCodigo)
                => CompraController.ObterHistoricoCompras(alunoCodigo);

        public static SaldoModel ObterSaldoAluno(int alunoCodigo)
                => CompraController.ObterSaldoAluno(alunoCodigo);

        public static decimal CalcularTotalCompra(int quantidade, decimal precoUnitario)
                => CompraController.CalcularTotalCompra(quantidade, precoUnitario);

        public static bool ProcessarCompra(int alunoCodigo, int quantidade, decimal precoUnitario)
                => CompraController.ProcessarCompra(alunoCodigo, quantidade, precoUnitario);

        public static bool VerificarAlunoExistente(int alunoCodigo)
                => CompraController.VerificarAlunoExistente(alunoCodigo);
    }
}