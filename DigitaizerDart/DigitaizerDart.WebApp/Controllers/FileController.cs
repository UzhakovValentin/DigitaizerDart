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
using Newtonsoft.Json.Linq;

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

        //[HttpPost("upload")]
        //public async Task<IActionResult> UploadFile()
        //{
        //    var file = Request.Form.Files;



        //    if (file == null)
        //    {
        //        return BadRequest();
        //    }

        //    if (!Directory.Exists(Path.Combine(env.WebRootPath, Path.GetFileNameWithoutExtension(file.FileName))))
        //    {
        //        Directory.CreateDirectory(Path.Combine(env.WebRootPath, Path.GetFileNameWithoutExtension(file.FileName)));
        //    }

        //    using (var fileStream = new FileStream(Path.Combine(env.WebRootPath, Path.GetFileNameWithoutExtension(file.FileName), file.FileName), FileMode.Create))
        //    {
        //        await file.CopyToAsync(fileStream);
        //    }

        //    File video = new File
        //    {
        //        FileName = file.FileName,
        //        MarkArray = bytes,
        //        Path = Path.Combine(env.WebRootPath, Path.GetFileNameWithoutExtension(file.FileName), file.FileName)
        //    };

        //    await dbContext.Files.AddAsync(video);
        //    await dbContext.SaveChangesAsync();
        //    return Ok(video.FileId);
        //}


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
                if (Path.GetExtension(file.FullName) != ".json")
                {
                    paths.Add(Path.GetFileName(file.FullName));
                }
            }

            return Json(paths);
        }

        [HttpGet("meta/{fileName}")]
        public IActionResult GetFileMetaData(string fileName)
        {
            var file = dbContext.Files.FirstOrDefault(x => x.FileName == fileName);
            return Json(file.MarkArray);
        }

        [HttpGet("meta")]
        public IActionResult GetMetaData()
        {
            string data;
            using (var stream = new StreamReader(Path.Combine(env.WebRootPath, "test.json"), Encoding.UTF8))
            {
                data = stream.ReadToEnd();
            }

            return Json(Filtering(data));
        }


        [HttpGet("versions/{userId}/{fileId}")]
        public IActionResult GetVersions(Guid userId, Guid fileId)
        {
            var versions = dbContext
                .Versions
                .Where(x => x.UserId == userId && x.FileId == fileId)
                .Select(x => new
                {
                    Meta = x.MarkArray,
                    Date = x.DateTime
                })
                .ToList();

            return Ok(versions);
        }


        [HttpPut("edit")]
        public IActionResult EditPoints([FromBody] EditRequest request)
        {
            string data;
            JObject json;

            using (var stream = new StreamReader(Path.Combine(env.WebRootPath, "test.json"), Encoding.UTF8))
            {
                data = stream.ReadToEnd();
            }

            json = Editing(data, request);

            using (var stream = new StreamWriter(System.IO.File.OpenWrite(Path.Combine(env.WebRootPath, "Edited.json")), Encoding.UTF8))
            {
                stream.WriteLine(json);
            }
            return Json(json);
        }



        private JObject Filtering(string jsonString)
        {
            JObject obj = JObject.Parse(jsonString);
            JArray koord = new JArray();

            for (int i = 0; i < obj.Count; i++)
            {
                JObject kadr = (JObject)obj[$"{i}.jpg"];
                JArray people = (JArray)kadr["people"];

                for (int j = 0; j < people.Count; j++)
                {
                    JArray temp = new JArray();
                    JObject points = (JObject)people[j];
                    koord = (JArray)points["pose_keypoints_2d"];

                    for (int k = 0; k < koord.Count; k += 2)
                    {
                        if ((float)koord[k] > 1 && (float)koord[k + 1] > 1)
                        {
                            temp.Add(koord[k]);
                            temp.Add(koord[k + 1]);
                        }
                    }
                    koord = temp;
                    obj[$"{i}.jpg"]["people"][j]["pose_keypoints_2d"] = koord;
                }
            }

            //using (var stream = new StreamWriter(Path.Combine(env.WebRootPath, "test.json"),false, Encoding.UTF8))
            //{
            //    stream.WriteLine(obj);
            //}
            return obj;
        }

        private JObject Editing(string jsonString, EditRequest request)
        {
            JObject obj = JObject.Parse(jsonString);
            obj[request.Frame]["people"][request.PeopleIndex]["pose_keypoints_2d"][request.PointsIndex] = request.X;
            obj[request.Frame]["people"][request.PeopleIndex]["pose_keypoints_2d"][request.PointsIndex + 1] = request.Y;
            return obj;
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