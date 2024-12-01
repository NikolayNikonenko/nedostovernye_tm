using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace поиск_недостоверной_ТМ_по_корреляции
{
    internal class Slices
    {
        [Key]
        public Guid SliceID { get; set; }
        public string SliceName { get; set; }
        public string SlicePath { get; set; }
        public string experiment_label { get; set; }

    }
}
