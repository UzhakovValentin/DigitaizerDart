using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DigitaizerDart.WebApp.Models;
using DigitaizerDart.WebApp.ModelViews;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using Version = DigitaizerDart.WebApp.Models.Version;
using File = DigitaizerDart.WebApp.Models.File;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.FileProviders;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace DigitaizerDart.WebApp.Controllers
{
    [Route("api/File")]
    public class FileController : Controller
    {
        private readonly IHostingEnvironment env;
        private readonly DataBaseContext dbContext;

        public FileController(IHostingEnvironment env, DataBaseContext dbContext)
        {
            this.env = env;
            this.dbContext = dbContext;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, byte[] bytes)
        {
            if (file == null)
            {
                return BadRequest();
            }

            if (!Directory.Exists(Path.Combine(env.WebRootPath, Path.GetFileNameWithoutExtension(file.FileName))))
            {
                Directory.CreateDirectory(Path.Combine(env.WebRootPath, Path.GetFileNameWithoutExtension(file.FileName)));
            }

            using (var fileStream = new FileStream(Path.Combine(env.WebRootPath, Path.GetFileNameWithoutExtension(file.FileName), file.FileName), FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            File video = new File
            {
                FileName = file.FileName,
                MarkArray = bytes,
                Path = Path.Combine(env.WebRootPath, Path.GetFileNameWithoutExtension(file.FileName), file.FileName)
            };

            await dbContext.Files.AddAsync(video);
            await dbContext.SaveChangesAsync();
            return Ok(video.FileId);
        }


        [HttpPost("remark")]
        public async Task<IActionResult> Remark(RemarkRequest request)
        {
            var user = await dbContext.Users.FindAsync(request.UserId);
            var file = await dbContext.Files.FindAsync(request.FileId);


            var json = Bytes2Json<MarkJsonObject>(file.MarkArray);

            var result = json.DeviceData.Find(x => x.CameraAt == request.CameraRacurs)
                .Data.Find(x => x.Type == request.Bone)
                .PointInfo.Find(x => x.Frame == request.Frame);

            result.Position.X = request.X;
            result.Position.Y = request.Y;
            result.Position.Z = request.Z;

            var bytes = Serialize2Bytes(result);

            user.Versions.Add(new Version
            {
                UserId = request.UserId,
                FileId = request.FileId,
                MarkArray = bytes,
                DateTime = DateTime.Now
            });

            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("names")]
        public IActionResult GetFileNames()
        {
            List<string> paths = new List<string>();
            var files = new DirectoryInfo(env.WebRootPath).GetFiles();

            foreach (var file in files)
            {
                paths.Add(file.FullName);
            }

            return Json(paths);
        }

        [HttpGet("meta/{fileName}")]
        public IActionResult GetFileMetaData(string fileName)
        {
            var file = dbContext.Files.FirstOrDefault(x => x.FileName == fileName);
            return Json(file.MarkArray);
        }

        [HttpGet("versions/{userId}/{fileId}")]
        public IActionResult GetVersions(Guid userId, Guid fileId)
        {
            var versions = dbContext
                .Versions
                .Where(x => x.UserId == userId && x.FileId == fileId)
                .Select(x => new
                {
                    meta = x.MarkArray
                })
                .ToList();

            return Ok(versions);
        }


        private byte[] Serialize2Bytes(object data)
        {
            if (data == null)
            {
                return new byte[0];
            }
            else
            {
                MemoryStream streamMemory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(streamMemory, data);
                return streamMemory.GetBuffer();
            }
        }

        private T Bytes2Json<T>(byte[] byteArray)
        {
            var byteString = Encoding.UTF8.GetString(byteArray);
            var json = JsonConvert.DeserializeObject<T>(byteString);
            return json;
        }
    }
}