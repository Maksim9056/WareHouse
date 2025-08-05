using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    /// <summary>
    /// Таблица баланса
    /// </summary>
    public class Balance
    {
        [Key]
        public int Id { get; set; }
        public Resource Resource { get; set; }
        public Unit Unit { get; set; }
        public int Count { get; set; }  
    }
}
