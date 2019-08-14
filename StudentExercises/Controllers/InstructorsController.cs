using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    public class InstructorsController : Controller
    {
        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config)
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

        // GET: Instructors
        public async Task<IActionResult> Index()
        {
            return View(await GetAllInstructors());
        }

        // GET: Instructors/Details/5
        public async Task<IActionResult> Details(int id)
        {
            return View(await GetOneInstructor(id));
        }

        // GET: Instructors/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new InstructorCreateViewModel() {
                Instructor = new Instructor(),
                Cohorts = RenderSelectOptions(await GetAllCohorts())
            };

            return View(viewModel);
        }

        // POST: Instructors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Instructor instructor)
        {
            try
            {
                await PostInstructor(instructor);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Instructors/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var viewModel = new InstructorCreateViewModel();

            List<Task> tasks = new List<Task>()
            {
                Task.Run(async () => viewModel.Instructor = await GetOneInstructor(id)),
                Task.Run(async () => viewModel.Cohorts = RenderSelectOptions(await GetAllCohorts()))
            };
            await Task.WhenAll(tasks);
            return View(viewModel);
        }

        // POST: Instructors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Instructor instructor)
        {
            try
            {
                await PutInstructor(instructor);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Instructors/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            return View(await GetOneInstructor(id));
        }

        // POST: Instructors/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                await DeleteInstructor(id);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }



        private async Task<List<Instructor>> GetAllInstructors()
        {
            var instructors = new List<Instructor>();
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT 
                    i.Id, i.FirstName, i.LastName, i.SlackHandle, i.CohortId, i.Specialty, c.[Name]
                    FROM Instructor i
                    LEFT JOIN Cohort c on i.CohortId = c.Id";

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (await reader.ReadAsync())
                    {
                        instructors.Add(new Instructor()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort()
                            {
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        });
                    }
                }

            }

            return instructors;
        }

        private async Task<Instructor> GetOneInstructor(int id)
        {
            Instructor instructor = null;
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT 
                    i.Id, i.FirstName, i.LastName, i.SlackHandle, i.CohortId, i.Specialty, c.[Name]
                    FROM Instructor i
                    LEFT JOIN Cohort c on i.CohortId = c.Id
                    WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (await reader.ReadAsync())
                    {
                        instructor = new Instructor()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort()
                            {
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        };
                    }
                }

            }

            return instructor;
        }

        private async Task<List<Cohort>> GetAllCohorts()
        {
            var cohorts = new List<Cohort>();
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, [Name] FROM Cohort";
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (await reader.ReadAsync())
                    {
                        cohorts.Add(new Cohort()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        });
                    }

                    reader.Close();
                }
            }
            return cohorts;
        }

        private async Task DeleteInstructor(int id)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Instructor WHERE Id = @id";

                    cmd.Parameters.AddWithValue("@id", id);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                    {
                        throw new Exception("No rows affected");
                    }
                }
            }
        }

        private async Task PostInstructor(Instructor instructor)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Instructor (FirstName, LastName, SlackHandle, Specialty, CohortId)
                        VALUES (@firstName, @lastName, @slackHandle, @specialty, @cohortId)";
                    cmd.Parameters.AddWithValue("@firstName", instructor.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", instructor.LastName);
                    cmd.Parameters.AddWithValue("@slackHandle", instructor.SlackHandle);
                    cmd.Parameters.AddWithValue("@specialty", instructor.Specialty);
                    cmd.Parameters.AddWithValue("@cohortId", instructor.CohortId);



                }
            }
        }

        private async Task PutInstructor(Instructor instructor)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE Instructor SET 
                            FirstName = @firstName,
                            LastName = @lastName,
                            SlackHandle = @slackHandle,
                            Specialty = @specialty,
                            CohortId = @cohortId
                        WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@firstName", instructor.FirstName);
                    cmd.Parameters.AddWithValue("@lastName", instructor.LastName);
                    cmd.Parameters.AddWithValue("@slackHandle", instructor.SlackHandle);
                    cmd.Parameters.AddWithValue("@specialty", instructor.Specialty);
                    cmd.Parameters.AddWithValue("@cohortId", instructor.CohortId);
                    cmd.Parameters.AddWithValue("@id", instructor.Id);

                    await cmd.ExecuteNonQueryAsync();


                }
            }
        }

        private async Task<bool> InstructorExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Instructor WHERE Id = @id";

                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    return await reader.ReadAsync();
                }
            }
        }

        private List<SelectListItem> RenderSelectOptions (List<Cohort> cohorts)
        {
            var options = new List<SelectListItem>();

            options.Add(new SelectListItem()
            {
                Text = "Choose cohort...",
                Value = "0"
            });

            cohorts.ForEach(cohort => options.Add(new SelectListItem()
            {
                Text = cohort.Name,
                Value = cohort.Id.ToString()
            }));

            return options;
        }
    }
}