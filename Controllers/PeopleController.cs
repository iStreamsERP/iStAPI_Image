using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PeopleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private static string pythonExePath = @"C:\Users\Administrator\AppData\Local\Programs\Python\Python312\python.exe";
        private static string pythonScriptPath = Path.Combine(AppContext.BaseDirectory, "people.py");

        private List<Person> ReadPeopleFromPython()
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = pythonExePath,
                Arguments = $"\"{pythonScriptPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();

                // 🔹 Capture Python Errors
                string errors = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(errors))
                {
                    Console.WriteLine("Python Error: " + errors);
                }

                // 🔹 Debugging - Print Raw Output
                Console.WriteLine("Python Output: " + result);

                // 🔹 Ensure JSON parsing works
                return JsonSerializer.Deserialize<List<Person>>(result, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }

        // API to get all people
        [HttpGet]
        public IActionResult GetAllPeople()
        {
            var people = ReadPeopleFromPython();
            return Ok(people);
        }

        // API to get a person by roll_no
        [HttpGet("{rollNo}")]
        public IActionResult GetPerson(string rollNo)
        {
            var people = ReadPeopleFromPython();
            var person = people.Find(p => p.RollNo == rollNo);

            if (person == null)
                return NotFound(new { message = "Person not found" });

            return Ok(person);
        }

        // 🔹 Debugging Endpoint: Check Python Output
        [HttpGet("test-python")]
        public IActionResult TestPython()
        {
            var people = ReadPeopleFromPython();
            return Ok(new { count = people?.Count, data = people });
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Degree { get; set; }

        [JsonPropertyName("roll_no")]  // 🔹 Ensure JSON matches Python output
        public string RollNo { get; set; }
    }
}
