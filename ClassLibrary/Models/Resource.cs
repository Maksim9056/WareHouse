using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.Models
{
    /// <summary>
    /// Справочник Ресурсов
    /// </summary>
    public class Resource : BaseRef
    {
        [Display(Name = "Состояние")]

        public Condition condition { get; set; }
    }
}
