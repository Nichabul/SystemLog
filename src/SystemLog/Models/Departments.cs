using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SystemLog.Models
{
    public class Departments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepartmentsId { get; set; }

        [MaxLength(50)]
        public string DepartmentsName { get; set; }

        [ForeignKey("Companys")]
        public int DeptCompanyId { get; set; }

        public virtual Companys Companys { get; set; }

        public virtual ICollection<ApplicationUser> User { get; set; }
    }
}
