using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    /// <summary>
    /// Таблица ресурсов (позиций) документа 
    /// </summary>
    public  class Document_resource
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Дата/время")]
        public DateTime DateTime { get; set; } = DateTime.Now;
        public Document Document { get; set; }
        public Resource Resource { get; set; }
        public Unit Unit { get; set; }
        [Range(1, int.MaxValue)]
        public int Count { get; set; }
    }
}
