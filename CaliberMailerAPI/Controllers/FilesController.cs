using Microsoft.AspNetCore.Mvc;

namespace CaliberMailerAPI.Controllers
{
    public class FilesController : Controller
    {
        [HttpGet("getbytes")]
        public IActionResult GetFileBytes([FromQuery] string filePath)
        {
            try
            {
                // Check if the file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File not found.");
                }

                // Read the file into a byte array
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                // Return the file content as bytes
                return Ok(fileBytes);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve file: {ex.Message}");
            }
        }
    }
}
