using System.Collections.Generic;
using System.Data.SqlClient;

namespace StudentExercises.Models.ViewModels
{
    public class StudentInstructorViewModel
    {

        public List<Student> Students { get; set; }
        public List<Instructor> Instructors { get; set; }


    }
}