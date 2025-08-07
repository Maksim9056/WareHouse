using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    /// <summary>
    /// Справочник Клиентов
    /// </summary>
    public class Client : BaseRef
    {
        public string Address { get; set; }
        public Condition condition { get; set; }
    }
}
