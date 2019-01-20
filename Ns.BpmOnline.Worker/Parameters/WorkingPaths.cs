using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace Ns.BpmOnline.Worker.Parameters
{
     class WorkingPaths
     {
        private readonly string _serverName;
        public string TempPath => ConfigurationManager.AppSettings["tempFilesPath"];

        public string DownloadServerPackagesPath => Path.Combine(TempPath, _serverName, "Download", "ServerPackages");
        public string DownloadPackagesPath => Path.Combine(TempPath, _serverName, "Download", "Packages");
        public string DownloadWorkingPath => Path.Combine(TempPath, _serverName, "Download", "Working");
        public string DownloadSvnPackagesPath => Path.Combine(TempPath, _serverName, "Download", "Working", "SvnPackages");
        public string TempUploadDirectory => Path.Combine(TempPath, _serverName, "Temp", "UploadWorking");
        public string LogsPath => Path.Combine(TempPath, _serverName, "Log");

        public WorkingPaths(string serverName)
        {
            _serverName = serverName;
            CreateFolders();
        }

        public void CreateFolders()
        {
            Directory.CreateDirectory(DownloadServerPackagesPath);
            Directory.CreateDirectory(DownloadPackagesPath);
            Directory.CreateDirectory(DownloadWorkingPath);
            Directory.CreateDirectory(TempUploadDirectory);
            Directory.CreateDirectory(LogsPath);
        }
     }
}
