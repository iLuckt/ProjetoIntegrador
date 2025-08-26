namespace ProjetoM2.DAO
{
    public class Aluno
    {
        public Aluno(int codigo, string nome, int saldo_impressoes)
        {
            this.codigo = codigo;
            this.nome = nome;
            this.saldo_impressoes = saldo_impressoes;
        }

        public Aluno ()
        {
        }

        public int codigo { get; set; }
        public string nome { get; set; }
        public int saldo_impressoes { get; set; }

        public static bool CadastrarAluno(string nome, out int codigoAluno)
           => Controller.AlunoController.CadastrarAluno(nome, out codigoAluno);
        public static Aluno ObterAluno(int codigo)
            => Controller.AlunoController.ObterAluno(codigo);
        public static bool AdicionarCredito(int alunoCodigo, int quantidade)
            => Controller.AlunoController.AdicionarCredito(alunoCodigo, quantidade);
        public static bool RegistrarImpressao(int alunoCodigo, int quantidade)
            => Controller.AlunoController.RegistrarImpressao(alunoCodigo, quantidade);
    }
}