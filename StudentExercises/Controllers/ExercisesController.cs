using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercises.Models;

namespace StudentExercises.Controllers
{
    public class ExercisesController : Controller
    {

        private readonly IConfiguration _config;

        public ExercisesController(IConfiguration config)
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
        // GET: Exercises
        public async Task<IActionResult> Index()
        {
            return View(await GetAllExercises());
        }

        // GET: Exercises/Details/5
        public async Task<IActionResult> Details(int id)
        {
            return View(await GetOneExercise(id));
        }

        // GET: Exercises/Create
        public ActionResult Create()
        {
            return View(new Exercise());
        }

        // POST: Exercises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Exercise exercise)
        {
            try
            {
                await PostExercise(exercise);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: Exercises/Edit/5
        public async Task<IActionResult> Edit(int id)
        {

            return View(await GetOneExercise(id));
        }

        // POST: Exercises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Exercise exercise)
        {
            exercise.Id = id;
            try
            {
                await PutExercise(exercise);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Edit));
            }
        }

        // GET: Exercises/Delete/5
        public async Task<IActionResult> Delete(int id)
        {

            return View(await GetOneExercise(id));
        }

        // POST: Exercises/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, Exercise exercise)
        {
            exercise.Id = id;
            try
            {
                await DeleteExercise(exercise);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Delete));
            }
        }

        private async Task<List<Exercise>> GetAllExercises()
        {
            var exercises = new List<Exercise>();
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, [Name], [Language] FROM Exercise";

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (await reader.ReadAsync())
                    {
                        exercises.Add(new Exercise()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        });

                    }
                }
            }
            return exercises;
        }



        private async Task<Exercise> GetOneExercise(int id)
        {
            Exercise exercise = null;
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, [Name], [Language] FROM Exercise WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (await reader.ReadAsync())
                    {
                        exercise = new Exercise()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Language = reader.GetString(reader.GetOrdinal("Language"))
                        };

                    }
                }
            }
            return exercise;
        }


        private async Task PostExercise(Exercise exercise)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Exercise 
                            ([Name], [Language]) 
                            VALUES (@name, @language)";
                    cmd.Parameters.AddWithValue("@name", exercise.Name);
                    cmd.Parameters.AddWithValue("@language", exercise.Language);

                    await cmd.ExecuteNonQueryAsync();

                }
            }
        }

        private async Task PutExercise(Exercise exercise)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Exercise 
                            SET [Name] = @name, [Language] = @language WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@name", exercise.Name);
                    cmd.Parameters.AddWithValue("@language", exercise.Language);
                    cmd.Parameters.AddWithValue("@id", exercise.Id);


                    await cmd.ExecuteNonQueryAsync();

                }
            }
        }

        private async Task DeleteExercise(Exercise exercise)
        {
            using (SqlConnection conn = Connection)
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"DELETE FROM Exercise WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", exercise.Id);


                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0) throw new Exception("No rows affected");

                }
            }
        }

    }
}