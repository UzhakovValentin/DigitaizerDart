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
using System.Runtime.Serialization;
using System.Net.Http;
using DigitaizerDart.WebApp.Services.Classes;
using System.IO.MemoryMappedFiles;

namespace DigitaizerDart.WebApp.Controllers
{
    [Route("video")]
    public class VideoController : Controller
    {
        private readonly IHostingEnvironment env;
        private readonly DataBaseContext dbContext;
        private readonly FileProviderClass file;

        public VideoController(IHostingEnvironment env,
            DataBaseContext dbContext,
            FileProviderClass file)
        {
            this.env = env;
            this.dbContext = dbContext;
            this.file = file;
        }

        /// <summary>
        /// Загрузка отснятых и не размеченных видео на сервер
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        [HttpPost("upload/{folderName}")]
        public async Task<IActionResult> UploadVideo(string folderName)
        {
            var files = Request.Form.Files;
            var path = Path.Combine(env.WebRootPath, $"videos/{folderName}");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var file in files)
            {
                using (var fileStream = new FileStream(Path.Combine(path, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                //File video = new File
                //{
                //    FileName = file.FileName,
                //    Path = Path.Combine(path, file.FileName)
                //};
                //await dbContext.Files.AddAsync(video);
            }

            //await dbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Загрузка на сервер json-файлов с трёхмерными координатами
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        [HttpPost("json/upload/{folderName}")]
        public async Task<IActionResult> UploadJson(string folderName)
        {
            var path = Path.Combine(env.WebRootPath, @$"videos\{folderName}");
            var fileName = folderName;

            if (Directory.Exists(path))
            {
                using (var stream = new FileStream(Path.Combine(path, $"{fileName}.json"), FileMode.CreateNew))
                {
                    await Request.Body.CopyToAsync(stream);
                }
                return Ok();
            }

            return NotFound("Директория с таким именем не найдена");


            //HttpClient http = new HttpClient();
            //var responce = await http.GetAsync("http://localhost:5000/api/video/videos");
            //return Json(await responce.Content.ReadAsAsync<string[]>());
        }

        /// <summary>
        /// Получение путей к не размеченным видео и json-файлам с трёхмерными координатами
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        [HttpGet("{folderName}")]
        public IActionResult GetFolderFilesPaths(string folderName)
        {
            var folderPath = Path.Combine(env.WebRootPath, @$"videos\{folderName}");
            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath);
                return Json(files);
            }
            return NotFound("Директория с таким именем не найдена");
        }

        /// <summary>
        /// Загрузка размеченных видео на сервер
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        [HttpPost("marked/upload/{folderName}")]
        public async Task<IActionResult> UploadMarkedVideo(string folderName)
        {
            var files = Request.Form.Files;
            var path = Path.Combine(env.WebRootPath, $"videos/{folderName}/alphapose");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var file in files)
            {
                using (var fileStream = new FileStream(Path.Combine(path, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                //File video = new File
                //{
                //    FileName = file.FileName,
                //    Path = Path.Combine(path, file.FileName)
                //};
                //await dbContext.Files.AddAsync(video);
            }

            //await dbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Загрузка на сервер json-файлов формата alphapose
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        [HttpPost("alphapose/{folderName}")]
        public async Task<IActionResult> UploadAlphaPoseJson(string folderName)
        {
            var path = Path.Combine(env.WebRootPath, $"videos/{folderName}/alphapose");
            var fileName = folderName;

            if (Directory.Exists(path))
            {
                using (var stream = new FileStream(Path.Combine(path, $"{fileName}.json"), FileMode.CreateNew))
                {
                    await Request.Body.CopyToAsync(stream);
                }
                return Ok();
            }

            return NotFound("Директория с таким именем не найдена");
        }

        /// <summary>
        /// Получение путей к размеченным видео и json-файлов формата alphapose
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        [HttpGet("alphapose/download/{folderName}")]
        public IActionResult GetAlphaPoseFolderFiles(string folderName)
        {
            var folderPath = Path.Combine(env.WebRootPath, $"videos/{folderName}/alphapose");
            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath);
                return Json(files);
            }
            return NotFound("Директория с таким именем не найдена");
        }

        /// <summary>
        /// Открытие потока на редактирование определенного json-файла формата alphapose
        /// </summary>
        /// <param name="username"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpPost("startedit")]
        public IActionResult StartEditFile([FromBody] OpenFileRequest request)
        {
            //if (dbContext.Users.FirstOrDefault(u => u.Name == request.Username) != null)
            //{
            var filePath = request.FullFilePath;
            if (System.IO.File.Exists(filePath))
            {
                file.OpenFile(filePath, request.Username);
                return Ok("Файл открыт для редактирования");
            }
            return BadRequest("Файл с таким именем не найден");
            //}
            return BadRequest("Пользователь с таким именем не найден");
        }

        /// <summary>
        /// Закрытие потока на редактирование файла
        /// </summary>
        /// <returns></returns>
        [HttpPost("finishedit")]
        public IActionResult CloseEditFile()
        {
            if (file.fileStream != null)
            {
                file.CloseFile();
                return Ok("Файл закрыт для редактирования");
            }
            return Ok("Файл уже закрыт");
        }

        /// <summary>
        /// Редактирование координат точек в json-файле формата alphapose
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("alphapose/editpoints")]
        public async Task<IActionResult> EditPoints([FromBody] EditRequest request)
        {
            if (file.fileStream != null)
            {
                string data;
                string json;
                using (var stream = new StreamReader(System.IO.File.Open(file.CopiedFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite), Encoding.UTF8))
                {
                    data = await stream.ReadToEndAsync();
                }
                // var json1 = EditPoints(data, request);
                file.fileStream.SetLength(0);
                json = EditPoints(data, request).ToString();
                var bytes = Encoding.UTF8.GetBytes(json, 0, json.Length);
                await file.fileStream.WriteAsync(bytes, 0, bytes.Length);
                file.fileStream.Position = 0;
                return Ok("Файл изменен");
            }
            return BadRequest("Ошибка при редактировании");
        }

        /// <summary>
        /// Добавление точек в json-файле формата alphapose
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("alphapose/addpoints")]
        public async Task<IActionResult> AddPoint([FromBody] AddRequest request)
        {
            if (file.fileStream != null)
            {
                string data;
                string json;
                using (var stream = new StreamReader(System.IO.File.Open(file.CopiedFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite), Encoding.UTF8))
                {
                    data = await stream.ReadToEndAsync();
                }
                file.fileStream.SetLength(0);
                // var json1 = AddPoint(data, request);
                json = AddPoint(data, request).ToString();
                var bytes = Encoding.UTF8.GetBytes(json, 0, json.Length);
                await file.fileStream.WriteAsync(bytes, 0, bytes.Length);
                file.fileStream.Position = 0;
                return Ok("Файл изменен");
            }
            return BadRequest("Ошибка при редактировании");
        }

        /// <summary>
        /// Удаление точек в json-файле формата alphapose
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete("alphapose/deletepoints")]
        public async Task<IActionResult> DeletePoint([FromBody] EditRequest request)
        {
            if (file.fileStream != null)
            {
                string data;
                string json;
                using (var stream = new StreamReader(System.IO.File.Open(file.CopiedFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite), Encoding.UTF8))
                {
                    data = await stream.ReadToEndAsync();
                }
                // var json1 = DeletePoint(data, request);
                file.fileStream.SetLength(0);
                json = DeletePoint(data, request).ToString();
                var bytes = Encoding.UTF8.GetBytes(json, 0, json.Length);
                await file.fileStream.WriteAsync(bytes, 0, bytes.Length);
                file.fileStream.Position = 0;
                return Ok("Файл изменен");
            }
            return BadRequest("Ошибка при редактировании");
        }



        //[HttpPut("alphapose/editpoints")]
        //public IActionResult EditPoints([FromBody] EditRequest request)
        //{
        //    string data;
        //    JObject json;


        //    using (var stream = new StreamReader(Path.Combine(env.WebRootPath, "test.json"), Encoding.UTF8))
        //    {
        //        data = stream.ReadToEnd();
        //    }

        //    json = Editing(data, request);

        //    using (var stream = new StreamWriter(System.IO.File.OpenWrite(Path.Combine(env.WebRootPath, "Edited.json")), Encoding.UTF8))
        //    {
        //        stream.WriteLine(json);
        //    }
        //    return Json(json);
        //}
        //[HttpGet("meta/{fileName}")]
        //public IActionResult GetFileMetaData(string fileName)
        //{
        //    var file = dbContext.Files.FirstOrDefault(x => x.FileName == fileName);
        //    return Json(file.MarkArray);
        //}

        //[HttpGet("meta")]
        //public IActionResult GetMetaData()
        //{
        //    string data;
        //    using (var stream = new StreamReader(Path.Combine(env.WebRootPath, "test.json"), Encoding.UTF8))
        //    {
        //        data = stream.ReadToEnd();
        //    }

        //    return Json(Filtering(data));
        //}

        //[HttpGet("versions/{userId}/{fileId}")]
        //public IActionResult GetVersions(Guid userId, Guid fileId)
        //{
        //    var versions = dbContext
        //        .Versions
        //        .Where(x => x.UserId == userId && x.FileId == fileId)
        //        .Select(x => new
        //        {
        //            Meta = x.MarkArray,
        //            Date = x.DateTime
        //        })
        //        .ToList();

        //    return Ok(versions);
        //}


        //[HttpPost("remark")]
        //public async Task<IActionResult> Remark(RemarkRequest request)
        //{
        //    var user = await dbContext.Users.FindAsync(request.UserId);
        //    var file = await dbContext.Files.FindAsync(request.FileId);


        //    var json = Bytes2Json<MarkJsonObject>(file.MarkArray);

        //    var result = json.DeviceData.Find(x => x.CameraAt == request.CameraRacurs)
        //        .Data.Find(x => x.Type == request.Bone)
        //        .PointInfo.Find(x => x.Frame == request.Frame);

        //    result.Position.X = request.X;
        //    result.Position.Y = request.Y;
        //    result.Position.Z = request.Z;

        //    var bytes = Serialize2Bytes(result);

        //    user.Versions.Add(new Version
        //    {
        //        UserId = request.UserId,
        //        FileId = request.FileId,
        //        MarkArray = bytes,
        //        DateTime = DateTime.Now
        //    });

        //    await dbContext.SaveChangesAsync();
        //    return Ok();
        //}

        //[HttpGet("download/{videoName}")]
        //public IActionResult DownloadVideo(string videoName)
        //{
        //    string videoPath = Path.Combine(env.WebRootPath, @$"videos\{videoName}.avi");
        //    if (System.IO.File.Exists(videoPath))
        //    {
        //        string videoType = "video/x-msvideo";
        //        var result = PhysicalFile(videoPath, videoType, $"{videoName}.avi", true);
        //        return result;
        //    }
        //    return NotFound("Видео с таким названием не найдено");
        //}

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
        private JObject EditPoints(string jsonString, EditRequest request)
        {
            JObject obj = JObject.Parse(jsonString);
            var mass = obj[$"{ request.Frame}.jpg"]["people"][request.PeopleIndex]["pose_keypoints_2d"];
            mass[request.PointsIndex] = request.X;
            mass[request.PointsIndex + 1] = request.Y;
            return obj;

            //JObject obj = JObject.Parse(jsonString);
            //JArray koord = new JArray();


            //JObject kadr = (JObject)obj[$"{request.Frame}.jpg"];
            //JArray people = (JArray)kadr["people"];
            //JObject points = (JObject)people[request.PeopleIndex];
            //koord = (JArray)points["pose_keypoints_2d"];
            //koord[request.PointsIndex] = request.X;
            //koord[request.PointsIndex + 1] = request.Y;

            //obj[$"{request.Frame}.jpg"]["people"][request.PeopleIndex]["pose_keypoints_2d"] = koord;
            //return obj;
        }
        private JObject AddPoint(string jsonString, AddRequest request)
        {
            JObject obj = JObject.Parse(jsonString);
            JArray koord = new JArray();
            JObject kadr = (JObject)obj[$"{request.Frame}.jpg"];
            JArray people = (JArray)kadr["people"];

            JObject points = (JObject)people[request.PeopleIndex];
            koord = (JArray)points["pose_keypoints_2d"];

            koord.Add(request.X);
            koord.Add(request.Y);

            obj[$"{request.Frame}.jpg"]["people"][request.PeopleIndex]["pose_keypoints_2d"] = koord;
            return obj;
        }
        private JObject DeletePoint(string jsonString, EditRequest request)
        {
            JToken token;
            JObject obj = JObject.Parse(jsonString);
            JArray koord = new JArray();
            JObject kadr = (JObject)obj[$"{request.Frame}.jpg"];
            JArray people = (JArray)kadr["people"];

            JObject points = (JObject)people[request.PeopleIndex];
            koord = (JArray)points["pose_keypoints_2d"];
            var arr = koord.ToList();
            koord.RemoveAt(request.PointsIndex);
            koord.RemoveAt(request.PointsIndex);
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