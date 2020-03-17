using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DigitaizerDart.WebApp.Services.Classes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitaizerDart.WebApp.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {

        private readonly IHostingEnvironment env;
        private readonly DataBaseContext dbContext;
        private readonly FileProviderClass file;

        public ApiController(IHostingEnvironment env,
            DataBaseContext dbContext,
            FileProviderClass file)
        {
            this.env = env;
            this.dbContext = dbContext;
            this.file = file;
        }

        [HttpPost("file/upload/{folderName}/{filename}")]
        public async Task<IActionResult> UploadJsonTest(string folderName, string filename)
        {
            var path = Path.Combine(env.WebRootPath, $"videos/{folderName}");
            //var fileName = folderName;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (Directory.Exists(path))
            {
                using (var stream = new FileStream(Path.Combine(path, $"{filename}.json"), FileMode.CreateNew))
                {
                    await Request.Body.CopyToAsync(stream);
                }
                return Ok();
            }
            return NotFound("Директория с таким именем не найдена");
        }
    }
}