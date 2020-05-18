using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UploadImageDotNetCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        public UploadController(ILogger<UploadController> logger)
        {
            _logger = logger;
        }

        [HttpGet][Route("/")]
        public ContentResult Get()
        {
            const string SINGLE_FILE_FORM = "<form action=\"/upload\" enctype=\"multipart/form-data\" method=\"post\"><input name=\"files\" type=\"file\"/><input type=\"submit\" value=\"Upload\"/></form>";
            const string MULTI_FILES_FORM = "<form action=\"/upload\" enctype=\"multipart/form-data\" method=\"post\"><input name=\"files\" type=\"file\" multiple/><input type=\"submit\" value=\"Upload\"/></form>";
            return new ContentResult{
                ContentType = "text/html",
                StatusCode = (int) HttpStatusCode.OK,
                Content = "<html><body>" + SINGLE_FILE_FORM + MULTI_FILES_FORM + "</body></html>"
            };
        }

        [HttpPost]
        public async Task<IActionResult> OnPostUploadAsync(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);
            string UPLOADING_DIRECTORY = Path.Join(Directory.GetCurrentDirectory(), "upload");
            int _MAX_FILE_CAPACITY = 10;

            // check if directory exists
            // if not create the directory
            if (!Directory.Exists(UPLOADING_DIRECTORY)) {
                Directory.CreateDirectory(UPLOADING_DIRECTORY);
            }

            // prevent too many file upload
            int expectedFileCount = Directory.GetFiles(UPLOADING_DIRECTORY).Length + files.Count;
            if (expectedFileCount > _MAX_FILE_CAPACITY) {
                return Conflict(new { error = "reached maximum file capacity"});
            }

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.Join(UPLOADING_DIRECTORY, formFile.FileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = files.Count, size });
        }
    }
}
