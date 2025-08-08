using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        // Вместо вложенного объекта — просто Id
        [Required(ErrorMessage = "Не выбран тип документа")]
        public int TypeDocId { get; set; }

        [Required(ErrorMessage = "Не выбран клиент")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Не выбрано состояние")]
        public int ConditionId { get; set; }

        [Required(ErrorMessage = "Номер обязателен")]
        public string Number { get; set; } = string.Empty;

        // было: DateTime Date
        [Column(TypeName = "date")]
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        // Навигационные свойства без [Required], EF их заполнит из БД
        public TypeDoc? TypeDoc { get; set; }
        public Client? Client { get; set; }
        public Condition? Condition { get; set; }
    }
}
