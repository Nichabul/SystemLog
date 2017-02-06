using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SystemLog.Models
{
    public class Details
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DetailsID { get; set; }

        public DateTime DetailsCreatedate { get; set; }

        [MaxLength(50)]
        public String DetailsName { get; set; }

        [MaxLength(50)]
        public String DetailWork { get; set; }

        public DateTime DetailsDueDate { get; set; }

        [MaxLength(3)]
        public int DetailsStatus { get; set; }

        [MaxLength(255)]
        public string DetailsNoteProblem { get; set; }

        [MaxLength(255)]
        public string DetailsNoteSolve { get; set; }

        [ForeignKey("Users")]
        public string DetailsUsersId { get; set; }

        public virtual ApplicationUser Users { get; set; }
    }
}
