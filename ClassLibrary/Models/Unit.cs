using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    /// <summary>
    /// Справочник единиц измерения
    /// </summary>
    public class Unit : BaseRef
    {
        [Display(Name = "Состояние")]

        public Condition condition { get; set; }
    }
}
