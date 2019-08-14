using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercises.Models;

namespace StudentExercises.Controllers
{
    public class CohortsController : Controller
    {

        private readonly IConfiguration _config;

        public CohortsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }


        // GET: Cohorts
        public async Task<IActionResult> Index()
        {
            var cohorts = await GetAllCohorts();
            return View(cohorts);
        }

        // GET: Cohorts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var cohort = await GetOneCohort(id);
            return View(cohort);
        }

        // GET: Cohorts/Create
        public ActionResult Create()
        {
            return View(new Cohort());
        }

        // POST: Cohorts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cohort cohort)
        {
            try
            {
                await PostCohort(cohort);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Cohorts/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Cohorts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Cohorts/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Cohorts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        private async Task<List<Cohort>> GetAllCohorts()
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT c.Id, c.Name, 
                        s.Id AS StudentId, 
                        s.FirstName AS StudentFirstName, 
                        s.LastName AS StudentLastName, 
                        s.SlackHandle AS StudentSlackHandle,
                        i.Id AS InstructorId,
                        i.FirstName AS InstructorFirstName, 
                        i.LastName AS InstructorLastName, 
                        i.SlackHandle AS InstructorSlackHandle, 
                        i.Specialty
                    FROM Cohort c
                    LEFT JOIN Student s ON c.Id = s.CohortId
                    LEFT JOIN Instructor i on c.Id = i.CohortId
                    ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();
                    while (await reader.ReadAsync())
                    {
                        var Id = reader.GetInt32(reader.GetOrdinal("Id"));
                        Cohort cohort = cohorts.Find(c => c.Id == Id);
                        if (cohort == null)
                        {
                            cohort = new Cohort()
                            {
                                Id = Id,
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            };
                            cohorts.Add(cohort);
                        }

                        var student = ParseStudent(reader);
                        if (student != null) cohort.Students.Add(student);

                        var instructor = ParseInstructor(reader);
                        if (instructor != null) cohort.Instructors.Add(instructor);



                    }

                    reader.Close();

                    return cohorts;
                }
            }
        }

        private async Task<Cohort> GetOneCohort(int id)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT c.Id, c.Name, 
                        s.Id AS StudentId, 
                        s.FirstName AS StudentFirstName, 
                        s.LastName AS StudentLastName, 
                        s.SlackHandle AS StudentSlackHandle,
                        i.Id AS InstructorId,
                        i.FirstName AS InstructorFirstName, 
                        i.LastName AS InstructorLastName, 
                        i.SlackHandle AS InstructorSlackHandle, 
                        i.Specialty
                    FROM Cohort c
                    LEFT JOIN Student s ON c.Id = s.CohortId
                    LEFT JOIN Instructor i on c.Id = i.CohortId
                    WHERE c.Id = @id
                    ";
                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;
                    while (await reader.ReadAsync())
                    {
                        if (cohort == null)
                        {
                            cohort = new Cohort()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            };
                        }

                        var student = ParseStudent(reader);
                        if (student != null) cohort.Students.Add(student);

                        var instructor = ParseInstructor(reader);
                        if (instructor != null) cohort.Instructors.Add(instructor);



                    }

                    reader.Close();

                    return cohort;
                }
            }
        }

        private async Task PostCohort(Cohort cohort)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Cohort ([Name])
                        VALUES (@name)
";
                    cmd.Parameters.AddWithValue("@name", cohort.Name);

                    await cmd.ExecuteNonQueryAsync();

                }
            }
        }


        private Student ParseStudent(SqlDataReader reader)
        {
            try
            {
                return new Student()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle"))
                };
            }
            catch (SqlNullValueException)
            {
                return null;
            }
        }

        private Instructor ParseInstructor(SqlDataReader reader)
        {
            try
            {
                return new Instructor()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                    Specialty = reader.GetString(reader.GetOrdinal("Specialty"))

                };
            }
            catch (SqlNullValueException)
            {
                return null;
            }
        }
    }
}