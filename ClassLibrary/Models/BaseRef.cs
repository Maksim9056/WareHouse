using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    /// <summary>
    /// Базовый абстрактный класс для всех справочников
    /// </summary>
        public abstract class BaseRef
        {
            [Key]
            public int Id { get; set; }
            public string? Code { get; set; }
            public string Name { get; set; }
            public override string ToString() => $"{Id}: {Name}";
    }
}
