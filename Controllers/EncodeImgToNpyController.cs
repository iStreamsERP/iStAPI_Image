using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Linq;
using System.Text;

namespace istWebAPI_ImageRec.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncodeImgToNpyController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] string DomainName, [FromForm] string base64Image, [FromForm] string outputNpyFileName)
        {
            if (string.IsNullOrEmpty(base64Image) || string.IsNullOrEmpty(DomainName) || string.IsNullOrEmpty(outputNpyFileName))
            {
                return BadRequest("Base64Image, DomainName, and outputNpyFileName are required.");
            }

            // Extract domain from email (e.g., "haneesh@demo.com" → "demo")
            string extractedDomain = DomainName.Split('@').LastOrDefault()?.Split('.').FirstOrDefault() ?? "default";

            // Define directory paths
            string baseDirectory = Path.Combine(GlobalVariables.ImageStorageRootLocation, extractedDomain, "employee");
            string sourceImagePath = Path.Combine(baseDirectory, "sourceimage");
            string encodedImagePath = Path.Combine(baseDirectory, "encodeimage");

            // Ensure directories exist
            Directory.CreateDirectory(sourceImagePath);
            Directory.CreateDirectory(encodedImagePath);

            // Define file paths
            string tempFileName = $"{outputNpyFileName}.jpg";
            string tempFilePath = Path.Combine(sourceImagePath, tempFileName);
            string encodedFilePath = Path.Combine(encodedImagePath, $"{outputNpyFileName}"); // Target for encoded image

            // Decode the base64 string to byte array
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            // Save the decoded image in sourceimage folder
            try
            {
                await System.IO.File.WriteAllBytesAsync(tempFilePath, imageBytes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error saving decoded file.", Details = ex.Message });
            }

            // Path to Python script and Python executable
            string pythonScriptPath = @"C:\iStImageProcessAPI\face.py";
            string pythonExePath = GlobalVariables.pythonExePath; // Ensure this is correctly set

            // Ensure Python script exists
            if (!System.IO.File.Exists(pythonScriptPath))
            {
                return StatusCode(500, new { Message = "Python script file not found.", Path = pythonScriptPath });
            }

            // Create process start info for Python script execution
            var processStartInfo = new ProcessStartInfo
            {
                FileName = pythonExePath,
                Arguments = $"\"{pythonScriptPath}\" \"{tempFilePath}\" \"{encodedFilePath}\"",
                WorkingDirectory = @"C:\iStImageProcessAPI",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    string result = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    if (!string.IsNullOrEmpty(error))
                    {
                        return StatusCode(500, new { Message = "Python script error", Details = error });
                    }
                }

                // Return success response
                return Ok(new
                {
                    SourceImagePath = tempFilePath,
                    EncodedImagePath = encodedFilePath,
                    Message = "Image uploaded, saved in sourceimage, and encoded image stored in encodeimage folder successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error executing Python script.", Details = ex.Message });
            }
        }
    }
}
