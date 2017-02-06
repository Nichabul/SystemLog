using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SystemLog.Models
{
    public class Companys
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CompanyId { get; set; }

        [MaxLength(255)]
        public string CompanyName { get; set; }

        public virtual ICollection<Departments> Departments { get; set; }
    }
}
