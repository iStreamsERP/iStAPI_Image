using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace istWebAPI_ImageRec.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewController : ControllerBase
    {
        private readonly ILogger<ViewController> _logger;

        public ViewController(ILogger<ViewController> logger)
        {
            _logger = logger;
        }

        [HttpGet("get-folder-images/{RefNo}")]
        public IActionResult GetFolderImages(string RefNo)
        {
            if (string.IsNullOrWhiteSpace(RefNo))
            {
                return BadRequest(new { message = "RefNo" });
            }

            try
            {
                // Dynamically get the base folder path using GlobalVariables
                string extractedDomain = "Istreams_Lead"; // Set this dynamically if needed
                string baseFolder = Path.Combine(GlobalVariables.ImageStorageRootLocation, extractedDomain, "employee", RefNo);
                string[] subFolders = { "1_uploaded_images", "3_matching_faces", "4_non_matching_faces" };

                var foldersData = new Dictionary<string, List<ImageInfo>>();

                foreach (var folder in subFolders)
                {
                    string folderPath = Path.Combine(baseFolder, folder);
                    List<ImageInfo> images = new();

                    if (Directory.Exists(folderPath))
                    {
                        var files = Directory.GetFiles(folderPath, "*.*")
                            .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                        f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                        f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        foreach (var imgPath in files)
                        {
                            try
                            {
                                byte[] imageBytes = System.IO.File.ReadAllBytes(imgPath);
                                string base64String = Convert.ToBase64String(imageBytes);
                                string extension = Path.GetExtension(imgPath).Replace(".", "");

                                images.Add(new ImageInfo
                                {
                                    Name = Path.GetFileName(imgPath),
                                    Base64Data = $"data:image/{extension};base64,{base64String}"
                                });
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error processing image {imgPath}: {ex.Message}");
                            }
                        }
                    }

                    foldersData[folder] = images;
                }

                return Ok(foldersData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching images for RefNo {RefNo}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving images." });
            }
        }

        public class ImageInfo
        {
            public string Name { get; set; }
            public string Base64Data { get; set; } // Base64 image string
        }
    }
}
