using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercises.Models.ViewModels
{
    public class StudentExerciseViewModel
    {
        public StudentExerciseViewModel(int studentId)
        {
            StudentId = studentId;
        }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int ExerciseId { get; set; }

        [Required]
        public int InstructorId { get; set; }


        public Student Student { get; set; }

        public List<SelectListItem> Instructors { get; set; }

        public List<SelectListItem> Exercises { get; set; }






    }
}
