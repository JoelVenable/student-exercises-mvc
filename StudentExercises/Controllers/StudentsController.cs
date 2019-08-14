using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using StudentExercises.Models;
using StudentExercises.Models.ViewModels;

namespace StudentExercises.Controllers
{
    public class StudentsController : Controller
    {

        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
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



        // GET: Students
        public async Task<IActionResult> Index()
        {
            List<Student> students = new List<Student>();
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT 
                    s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.[Name]
                    FROM Student s
                    LEFT JOIN Cohort c on s.CohortId = c.Id";

                    SqlDataReader reader = cmd.ExecuteReader();


                    while (await reader.ReadAsync())
                    {
                        students.Add(new Student()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort()
                            {
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        });
                    }
                    reader.Close();


                }
            }
            return View(students);
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int id)
        {
            return View(await GetOneStudent(id));
        }

        // GET: Students/Create
        public async Task<IActionResult> Create()
        {
            var createStudent = new StudentCreateViewModel();
            createStudent.Student = new Student();
            createStudent.Cohorts = RenderSelectOptions(await GetAllCohorts());

            return View(createStudent);
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            try
            {
                await PostStudent(student);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (await StudentExists(id))
            {
                var studentCreateModel = new StudentCreateViewModel();

                List<Task> tasks = new List<Task>()
                {
                    Task.Run(async () => studentCreateModel.Student = await GetOneStudent(id)),
                    Task.Run(async () => studentCreateModel.Cohorts = RenderSelectOptions(await GetAllCohorts()))
                };

                await Task.WhenAll(tasks);
                return View(studentCreateModel);
            }
            else return View();

        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            student.Id = id;
            try
            {

                await PutStudent(student);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Edit));
            }
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int id)
        {

            return View(await GetOneStudent(id));
        }

        // POST: Students/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                if (await StudentExists(id))
                {
                    await DeleteStudent(id);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        private async Task PostStudent(Student student)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Student (FirstName, LastName, SlackHandle, CohortId)
                        VALUES (@firstName, @lastName, @slackHandle, @cohortId)
";
                    cmd.Parameters.AddWithValue("@firstName", student.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", student.LastName);
                    cmd.Parameters.AddWithValue("@slackHandle", student.SlackHandle);
                    cmd.Parameters.AddWithValue("@cohortId", student.CohortId);

                    await cmd.ExecuteNonQueryAsync();

                }
            }
        }

        private async Task PutStudent(Student student)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            UPDATE Student
                            SET FirstName = @firstName,
                                LastName = @lastName,
                                SlackHandle = @slackHandle,
                                CohortId = @cohortId
                            WHERE Id = @id
";
                    cmd.Parameters.AddWithValue("@firstName", student.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", student.LastName);
                    cmd.Parameters.AddWithValue("@slackHandle", student.SlackHandle);
                    cmd.Parameters.AddWithValue("@cohortId", student.CohortId);
                    cmd.Parameters.AddWithValue("@id", student.Id);


                    await cmd.ExecuteNonQueryAsync();

                }
            }
        }


        private async Task<Student> GetOneStudent(int id)
        {
            Student student = null;
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT 
                    s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, 
                    c.[Name], e.[Name] AS ExerciseName, e.[Language]
                    FROM Student s
                    LEFT JOIN Cohort c on s.CohortId = c.Id
                    LEFT JOIN StudentExercise se on s.Id = se.StudentId
                    LEFT JOIN Exercise e on se.ExerciseId = e.Id
                    WHERE s.Id = @Id";

                    cmd.Parameters.Add(new SqlParameter("Id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    
                    while (await reader.ReadAsync())
                    {
                        if (student == null)
                        {
                            student = new Student()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Exercises = new List<Exercise>(),
                                Cohort = new Cohort()
                                {
                                    Name = reader.GetString(reader.GetOrdinal("Name"))
                                }
                            };
                        }
                        var exercise = ParseExercise(reader);

                        if (exercise != null) student.Exercises.Add(exercise);
                        
                    }
                    reader.Close();


                }
            }

            return student;
        }

        private async Task DeleteStudent(int id)
        {

            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Student WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                        throw new Exception("No rows affected");
                }
            }
        }


        private async Task<List<Cohort>> GetAllCohorts()
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM Cohort";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();
                    while (await reader.ReadAsync())
                    {
                        cohorts.Add(new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        });
                    }

                    reader.Close();

                    return cohorts;
                }
            }
        }


        private List<SelectListItem> RenderSelectOptions(List<Cohort> cohorts)
        {
            var options = new List<SelectListItem>();
            options.Insert(0, new SelectListItem
            {
                Text = "Choose cohort...",
                Value = "0"
            });
            cohorts.ForEach(cohort =>
            {
                options.Add(new SelectListItem()
                {
                    Text = cohort.Name,
                    Value = cohort.Id.ToString()
                });
            });

            return options;
        }

        private Exercise ParseExercise(SqlDataReader reader)
        {
            try
            {
                return new Exercise()
                {
                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                    Language = reader.GetString(reader.GetOrdinal("Language"))

                };
            }
            catch (SqlNullValueException)
            {
                return null;
            }
        }


        private async Task<bool> StudentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id FROM Student WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    return await reader.ReadAsync();
                }
            }
        }


    }
}