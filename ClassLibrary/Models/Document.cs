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
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public Client Client { get; set; }
        public Condition Condition { get; set; }
    }
}
