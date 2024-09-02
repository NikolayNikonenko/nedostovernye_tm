using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace поиск_недостоверной_ТМ_по_корреляции
{
    public class telemetry
    {
        [Key]
        public Guid ID { get; set; }
        public double IndexTm { get; set; }
        public double CorrTm { get; set; }
        public string Status { get; set; }
        public double MaxLagranj { get; set; }  
        public double AvgLagranj { get; set; }
        public string NameTM { get; set; }


    }
}
