using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace поиск_недостоверной_ТМ_по_корреляции
{
    internal class ActivePowerImbalance
    {
        public Guid ID { get; set; }
        public int n_nach_p { get; set; }
        public int n_kon_p { get; set; }
        public string name_p { get; set; }
        public double p_neb_p { get; set; }
        public Guid SliceID_p { get; set; }
        public int orderIndexP { get; set; }
    }
}
