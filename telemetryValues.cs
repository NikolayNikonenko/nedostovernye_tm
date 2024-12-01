using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace поиск_недостоверной_ТМ_по_корреляции
{
    public class telemetryValues
    {
        public Guid ID { get; set; }  
        public double IndexTM { get; set; }
        public double IzmerValue { get; set; }
        public double OcenValue { get; set; }
        public int OrderIndex { get; set; }     
        public string Privyazka { get; set; }
        public int Id1 { get; set; }
        public double DeltaOcenIzmer { get; set; }
        public string NameTM { get; set; }
        public string NumberOfSrez { get; set; } // Новое поле для значения среза
        public Guid SliceID { get; set; }
        public double Lagranj { get; set; }
        public string experiment_label { get; set; }



    }
}
