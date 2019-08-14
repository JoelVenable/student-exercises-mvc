using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;


namespace StudentExercises.Models.ViewModels
{
    public class InstructorCreateViewModel
    {
        public List<SelectListItem> Cohorts { get; set; }
        public Instructor Instructor { get; set; }



        

        
    }
}