using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    public sealed class BalanceRowDto
    {
        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = "";
        public int UnitId { get; set; }
        public string UnitName { get; set; } = "";
        public long Quantity { get; set; }
    }
}
