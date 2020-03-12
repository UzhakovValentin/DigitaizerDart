using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitaizerDart.WebApp.Services.Classes
{
    public class FileProviderClass
    {
        public FileStream fileStream { get; private set; }
        private readonly IHostingEnvironment env;
        public string CopiedFilePath { get; private set; }
        public string OriginFilePath { get; private set; }
        public FileProviderClass(IHostingEnvironment env)
        {
            this.env = env;
        }
        public void OpenFile(string path, string username)
        {
            var fileName = $"{username}_{DateTime.Now.Hour}-{DateTime.Now.Minute}_{DateTime.Now.Day}.{DateTime.Now.Month}.{DateTime.Now.Year}.json";
            File.Copy(path, Path.Combine(env.WebRootPath, fileName));
            fileStream = new FileStream(Path.Combine(env.WebRootPath, fileName), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            CopiedFilePath = Path.Combine(env.WebRootPath, fileName);
            OriginFilePath = path;
        }
        public void CloseFile()
        {
            fileStream.Close();
            fileStream = null;
        }
    }
}
