using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercises.Models
{
    public class BasePerson
    {
        [Display(Name = "Id")]
        public int? Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [MaxLength(20)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(20)]

        public string LastName { get; set; }

        [Display(Name = "Name")]
        public string FullName {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }


        [Required]
        [Display(Name = "Slack Handle")]
        [MaxLength(20)]
        [MinLength(4)]


        public string SlackHandle { get; set; }

        [Required]
        [Display(Name = "Cohort Id")]

        public int CohortId { get; set; }


        [Display(Name = "Cohort")]

        public Cohort Cohort { get; set; }
    }
}
