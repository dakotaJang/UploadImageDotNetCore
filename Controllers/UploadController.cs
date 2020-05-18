using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UploadImageDotNetCore.Helpers;

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
            int currentFileCount = Directory.GetFiles(UPLOADING_DIRECTORY).Length;
            int expectedFileCount = currentFileCount + files.Count;
            if (expectedFileCount > _MAX_FILE_CAPACITY) {
                return Conflict(new { currentFileCount, error = "reached maximum file capacity", _MAX_FILE_CAPACITY});
            }

            int uploadCount = 0;
            int failToUploadCount = 0;
            foreach (var formFile in files)
            {
                bool validFile = FileValidation.ValidateImageFile(formFile);
                if (validFile)
                {
                    var filePath = Path.Join(UPLOADING_DIRECTORY, formFile.FileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                    uploadCount++;
                } else {
                    failToUploadCount++;
                }
            }

            if (failToUploadCount > 0) {
                return Conflict(new { failToUploadCount, _MAX_FILE_CAPACITY, uploadCount, size });
            } else {
                return Ok(new { _MAX_FILE_CAPACITY, uploadCount, size });
            }
        }
    }
}
