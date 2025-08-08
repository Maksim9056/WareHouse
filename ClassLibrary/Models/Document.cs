using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    /// <summary>
    /// Таблица докумнетов
    /// </summary>
    public class Document
    {
        [Key]
        public int Id { get; set; }
        public TypeDoc TypeDoc { get; set; }
        [Display(Name = "Номер")]
        public string? Number { get; set; }
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }
        public Client Client { get; set; }
        public Condition Condition { get; set; }
    }
}
