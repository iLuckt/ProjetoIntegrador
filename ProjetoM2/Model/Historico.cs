using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoM2.DAO
{
    public class Historico
    {
        public int codigo { get; set; }
        public int alunos_codigo { get; set; }
        public DateTime data_historico { get; set; }
        public bool tipo_movimentacao { get; set; }
        public int quantidade_movimentada { get; set; }
        public int saldo_antes { get; set; }
        public int saldo_depois { get; set; }
    }
}
