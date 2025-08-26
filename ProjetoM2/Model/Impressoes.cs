using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoM2.DAO
{
    public class Impressao
    {
        public int codigo { get; set; }
        public int alunos_codigo { get; set; }
        public DateTime data_impressao { get; set; }
        public int quantidade_impressa { get; set; }

    }
}
