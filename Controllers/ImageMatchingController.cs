using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Python.Runtime;
using System.Text;

namespace istWebAPI_ImageRec.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageMatchingController : ControllerBase
    {
        public ImageMatchingController()
        {
            // Ensure Python runtime is cleaned up when the application exits
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                if (PythonEngine.IsInitialized)
                    PythonEngine.Shutdown();
            };
        }

        private void InitializePythonEnvironment()
        {
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", GlobalVariables.pythonDll);
            Environment.SetEnvironmentVariable("PYTHONHOME", GlobalVariables.pythonHome);
            Environment.SetEnvironmentVariable("PYTHONPATH", GlobalVariables.pythonPath);

            PythonEngine.Initialize();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromBody] iStBase64Image input, string DomainName)
        {
            Console.WriteLine("UploadImage called");

            // Validate input
            if (input == null || string.IsNullOrEmpty(input.Base64Images))
            {
               // Console.WriteLine("Invalid input");
                return BadRequest("Image are required.");
            }
            else if (input.RefNo == 0)
            {
                return BadRequest("RefNo are required.");
            }
            else if (string.IsNullOrEmpty(DomainName))
            {
                return BadRequest(" and DomainName are required.");
            }
            
            // Extract domain from email (e.g., "haneesh@demo.com" → "demo")
            string extractedDomain = DomainName.Split('@').LastOrDefault()?.Split('.').FirstOrDefault() ?? "default";

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(input.Base64Images);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid base64 image format");
                return BadRequest("Invalid base64 image format.");
            }

            // File paths for storing uploaded images
            var directoryPath = Path.Combine(GlobalVariables.ImageStorageRootLocation, extractedDomain, "employee");
            var filePath = Path.Combine(directoryPath, $"{Guid.NewGuid()}.jpg");

            try
            {
                // Create directory if it doesn't exist
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"Directory created or already exists: {directoryPath}");

                // Save image to disk
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                Console.WriteLine($"File written: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating directory or writing file: {ex.Message}");
                return StatusCode(500, new
                {
                    Message = "Error creating directory or writing file.",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }

            // Define paths for the Python script and the image library
            string pythonScriptPath = @"C:\iStImageProcessAPI\pythonImageMatchingScript.py";
            string imageLibraryPath = Path.Combine(GlobalVariables.ImageStorageRootLocation, extractedDomain, "employee", "encodeimage");
            string inputImagePath = filePath;
            string rootFolder = Path.Combine(GlobalVariables.ImageStorageRootLocation, extractedDomain, "employee", input.RefNo.ToString());

            // Ensure the Python script file exists
            if (!System.IO.File.Exists(pythonScriptPath))
            {
                Console.WriteLine($"Python script file not found: {pythonScriptPath}");
                return StatusCode(500, new { Message = "Python script file not found.", Path = pythonScriptPath });
            }

            // Initialize the Python environment
            InitializePythonEnvironment();
            Console.WriteLine("Python environment initialized");

            try
            {
                using (Py.GIL())
                {
                    // Setup the Python script directory in sys.path
                    dynamic sys = Py.Import("sys");
                    string scriptDirectory = Path.GetDirectoryName(pythonScriptPath);
                    dynamic sysPath = sys.path;

                    if (!string.IsNullOrEmpty(scriptDirectory))
                    {
                        bool pathExists = false;
                        foreach (var path in sysPath)
                        {
                            if (path.ToString() == scriptDirectory)
                            {
                                pathExists = true;
                                break;
                            }
                        }

                        if (!pathExists)
                        {
                            sys.path.append(scriptDirectory);
                        }
                    }
                    Console.WriteLine("Python script path set");

                    // Import the Python script
                    dynamic faceRecogScript = Py.Import("pythonImageMatchingScript");
                    Console.WriteLine("Python script imported");

                    // Call the face recognition function
                    dynamic result = faceRecogScript.find_matching_faces(imageLibraryPath, inputImagePath, input.RefNo, rootFolder);
                    Console.WriteLine("Python script executed");

                    // Return result directly (handling null values)
                    return Ok(new { result = result?.ToString() ?? "Success" });
                }
            }
            catch (PythonException ex)
            {
                Console.WriteLine($"Python error: {ex.Message}");
                return StatusCode(500, new
                {
                    Message = "Python error",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Python script: {ex.Message}");
                return StatusCode(500, new
                {
                    Message = "Error executing Python script.",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
            finally
            {
                // Clean up and delete the uploaded image
                if (System.IO.File.Exists(inputImagePath))
                {
                    System.IO.File.Delete(inputImagePath);
                }
                // Ensure Python runtime is properly shut down
                if (PythonEngine.IsInitialized)
                    PythonEngine.Shutdown();
            }
        }

        public class iStBase64Image
        {
            public string Base64Images { get; set; }
            public long RefNo { get; set; }
        }
    }
}
