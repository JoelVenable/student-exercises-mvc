using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercises.Models
{
    public class StudentExercise
    {

        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int InstructorId { get; set; }

        [Required]
        public int ExerciseId { get; set; }



    }
}
