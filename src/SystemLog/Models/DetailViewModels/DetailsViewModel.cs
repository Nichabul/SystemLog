using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SystemLog.Models.DetailViewModels
{
    public class DetailsViewModel
    {
        [Required]
        [Display(Name = "DetailsID")]
        public int DetailsID { get; set; }

        [Required]
        [Display (Name = "DetailsCreatedate")]
        public DateTime DetailsCreatedate { get; set; }

        [Required]
        [Display (Name = "DetailsName")]
        public String DetailsName { get; set; }

        [Required]
        [Display(Name = "DetailWork")]
        public String DetailWork { get; set; }

        [Required]
        [Display(Name = "DetailsDueDate")]
        public DateTime DetailsDueDate { get; set; }

        [Required]
        [Display(Name = "DetailsStatus")]
        public int DetailsStatus { get; set; }

        
        [Display(Name = "DetailsNoteProblem")]
        public String DetailsNoteProblem { get; set; }

       
        [Display(Name = "DetailsNoteSolve")]
        public String DetailsNoteSolve { get; set; }

        public string DetailsUsersId { get; set; }
    }
}
