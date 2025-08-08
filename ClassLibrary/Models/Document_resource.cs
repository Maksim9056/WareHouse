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
    /// Таблица ресурсов (позиций) документа 
    /// </summary>
    public class Document_resource
    {
        [Key]
        public int Id { get; set; }

        // момент времени в UTC → маппим на timestamptz
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset DateTime { get; set; } = DateTimeOffset.UtcNow;

        [Required(ErrorMessage = "Не выбран документ")]
        public int DocumentId { get; set; }

        [Required(ErrorMessage = "Не выбран ресурс")]
        public int ResourceId { get; set; }

        [Required(ErrorMessage = "Не выбрана единица измерения")]
        public int UnitId { get; set; }

        public int Count { get; set; }

        // Навигации
        public Document? Document { get; set; }
        public Resource? Resource { get; set; }
        public Unit? Unit { get; set; }
    }
}
