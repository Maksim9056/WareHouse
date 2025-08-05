using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.Models
{
    /// <summary>
    /// Справочник Ресурсов
    /// </summary>
    public class Resource
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Condition condition { get; set; }
    }
}
