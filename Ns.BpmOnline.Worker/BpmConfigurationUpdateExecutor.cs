using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Configuration;
using Ns.BpmOnline.Worker.Parameters;
using Ns.BpmOnline.Worker.Executors;

namespace Ns.BpmOnline.Worker
{
    /*
    class BpmConfigurationUpdateExecutor : Executor, IExecutor
    {

        private readonly string _appPath;
        private readonly string _serverName;
        private readonly IConnection _rabbitConnection;
        private readonly UpdateExecutorRabbitSettings _rabbitSettings;

        private readonly List<string> _outputList = new List<string>();
        private readonly List<BpmConfigurationUpdateStatus> _actionList = new List<BpmConfigurationUpdateStatus>();
        private readonly int BatchSendTimeout = 5000;

        private Dictionary<string, string> InputParameters;
        private Timer BatchSendTimer;
        private BpmConfigurationUpdater Updater;
        private string BpmServerPath  => Path.Combine(_appPath, "Terrasoft.WebApp"); 
        private string WorkspaceConsoleDirectoryPath => Path.Combine(BpmServerPath, "DesktopBin", "WorkspaceConsole"); 
        private string WorkspaceConsoleExePath => Path.Combine(WorkspaceConsoleDirectoryPath, "Terrasoft.Tools.WorkspaceConsole.exe");

        private string CurrentPath => ConfigurationManager.AppSettings["tempFilesPath"];//System.AppDomain.CurrentDomain.BaseDirectory;

        private string DownloadServerPackagesPath => Path.Combine(CurrentPath, _serverName, "Download", "ServerPackages");
        private string DownloadPackagesPath => Path.Combine(CurrentPath, _serverName, "Download", "Packages");  
        private string DownloadWorkingPath => Path.Combine(CurrentPath, _serverName, "Download", "Working"); 
        private string TempUploadDirectory => Path.Combine(CurrentPath, _serverName, "Temp", "UploadWorking"); 
        private string LogsPath => Path.Combine(CurrentPath, _serverName, "Log"); 

        

        public BpmConfigurationUpdateExecutor(ServerElement server, IConnection connection) : base(server) {
            _appPath = server.Path;
            _serverName = server.Name;
            _rabbitConnection = connection;
            _rabbitSettings = new UpdateExecutorRabbitSettings();

        }

        public (DownloadPackagesFromServerSettings, DownloadPackagesFromSvnSettings, UploadPackagesToServerSettings, BuildConfigurationSettings) CreateSettings()
        {
            var svnSettings = new ProjectSvnSettings()
            {
                Uri = server.SvnUri,
                Login = server.SvnLogin,
                Password = server.SvnPassword
            };

            var downloadPackagesFromServer = new DownloadPackagesFromServerSettings()
            {
                WCpath = WorkspaceConsoleExePath,
                PackageName = DownloadPackagesPath,
                AppPath = _appPath,
                LogPath = LogsPath,
                DestinationPath = DownloadServerPackagesPath
            };

            var downloadFromSvnSettings = new DownloadPackagesFromSvnSettings()
            {
                Svn = svnSettings,
                WCpath = WorkspaceConsoleExePath,
                WorkingPath = DownloadWorkingPath,
                PackagesPath = DownloadPackagesPath,
                LogPath = LogsPath
            };

            var uploadToServerSettings = new UploadPackagesToServerSettings()
            {
                WCpath = WorkspaceConsoleExePath,
                PackagesPath = DownloadPackagesPath,
                WorkingTempPath = TempUploadDirectory,
                WebAppPath = BpmServerPath,
                AppPath = _appPath,
                LogPath = LogsPath
            };

            var buildConfigurationSettings = new BuildConfigurationSettings()
            {
                WCpath = WorkspaceConsoleExePath,
                WebAppPath = BpmServerPath,
                AppPath = _appPath,
                LogPath = LogsPath
            };

            return (downloadPackagesFromServer, downloadFromSvnSettings, uploadToServerSettings, buildConfigurationSettings);
        }

        public void Execute(byte[] data)
        {
            Dictionary<string, string> parameters;
            try
            {
                parameters = DecodeParameters(data);
            } catch (Exception e)
            {
                parameters = new Dictionary<string, string>();
                parameters.Add("Error", e.Message);
                parameters.Add("StackTrace", e.StackTrace);
            }

            Execute(parameters);
        }

        private void Execute(Dictionary<string, string> parameters)
        {

            InputParameters = parameters;
            string ID = GetByKey(parameters, "ID");
            string command = GetByKey(parameters, "Command");

            if (String.IsNullOrEmpty(ID))
            {
                Logger.Log("BpmConfigurationUpdate: Failed execution. Parameter 'ID' must be defined");
                return;
            }
            if (String.IsNullOrEmpty(command))
            {
                Logger.Log("BpmConfigurationUpdate: Failed execution. Parameter 'Command' must be defined");
                return;
            }

            switch(command) {
                case "BuildFromSVN":
                    ExecuteBuild(parameters);
                break;
                case "RestoreLastBackup":
                    ExecuteRestoreLastBackup(parameters);
                break;
                case "BuildStaticFiles":
                    BuildStaticFilesOnly(parameters);
                    break;
                case "GetPackagesFromSVN":
                    //ExecuteRestoreLastBackup(parameters);
                    break;
            }
        }

        private void ExecuteBuild(Dictionary<string, string> parameters)
        {
            InputParameters = parameters;
            string packages = GetByKey(parameters, "Packages");
            string branch = GetByKey(parameters, "Branch");
            bool needBackupConfiguration = String.IsNullOrEmpty( GetByKey(parameters, "Backup") ) ? false : true;
            bool needDownloadPackages = String.IsNullOrEmpty(GetByKey(parameters, "DownloadPackages")) ? false : true;

            if (String.IsNullOrEmpty(packages))
            {
                Logger.Log("BpmConfigurationUpdate: Failed execution. Parameter 'Packages' must be defined");
                return;
            }
            if (String.IsNullOrEmpty(branch))
            {
                Logger.Log("BpmConfigurationUpdate: Failed execution. Parameter 'Branch' must be defined");
                return;
            }

            Updater = CreateUpdater();

            Updater.SetPackages(packages);
            Updater.SetBranch(branch);

            Updater.SetNeedDownloadPackages(needDownloadPackages);

            Updater.RunUpdateFromSvn(needBackupConfiguration);

        }

        private void ExecuteRestoreLastBackup(Dictionary<string, string> parameters)
        {

            InputParameters = parameters;
            Updater = CreateUpdater();
            Updater.RestorePackagesFromLastBackup();

        }

        private void BuildStaticFilesOnly(Dictionary<string, string> parameters)
        {

            InputParameters = parameters;
            Updater = CreateUpdater();
            Updater.BuildStaticFilesOnly();

        }

        private BpmConfigurationUpdater CreateUpdater()
        {
            DownloadPackagesFromServerSettings downloadPackagesFromServer;
            DownloadPackagesFromSvnSettings downloadFromSvnSettings;
            UploadPackagesToServerSettings uploadToServerSettings;
            BuildConfigurationSettings buildConfigurationSettings;

            (downloadPackagesFromServer, downloadFromSvnSettings, uploadToServerSettings, buildConfigurationSettings) = CreateSettings();

            BatchSendTimer = new Timer() { Interval = BatchSendTimeout };
            BatchSendTimer.Elapsed += OnBatchSendTimerEvent;
            BatchSendTimer.Enabled = true;

            _outputList.Clear();
            _actionList.Clear();

            Directory.CreateDirectory(DownloadServerPackagesPath);
            Directory.CreateDirectory(DownloadPackagesPath);
            Directory.CreateDirectory(DownloadWorkingPath);
            Directory.CreateDirectory(TempUploadDirectory);
            Directory.CreateDirectory(LogsPath);

            var updater = new BpmConfigurationUpdater(BpmServerPath, downloadPackagesFromServer, downloadFromSvnSettings, uploadToServerSettings, buildConfigurationSettings);
            updater.ProcessOutput += LogOutput;
            updater.SetUpdateStatus += LogUpdateStatus;

            updater.ClearDirectory(DownloadPackagesPath);
            updater.ClearDirectory(TempUploadDirectory);

            return updater;
        }

        private void OnBatchSendTimerEvent(Object source, ElapsedEventArgs e)
        {
            BpmConfigurationUpdateStatus finishState = _actionList.ToArray().FirstOrDefault(x => x.Name == "FINISH");
            if (finishState != null)
            {
                StopAndClear();
            }

            SendResponse(_outputList, _actionList);

            _outputList.Clear();
            _actionList.Clear();
        }

        private void SendResponse(List<string> outputList, List<BpmConfigurationUpdateStatus> actionList)
        {
            Logger.Log(String.Format("[{0} {1}]", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), "SendBuildResponse"));
            var response = new BpmConfigurationUpdateResponse()
            {
                OutputList = outputList,
                ActionList = actionList,
                Id = InputParameters["ID"]
            };
            string message = JsonConvert.SerializeObject(response);

            RabbitPublisher.Publish(_rabbitConnection, _rabbitSettings.ExchangeName, _rabbitSettings.AnswerQueueName, _rabbitSettings.AnswerRoutingKey, message);
        }

        private void LogOutput(string message)
        {
            _outputList.Add(message);
            Logger.Log(message);
        }

        private void LogUpdateStatus(string action, bool success, string comment)
        {
            if (!success)
            {
                comment += " finished with errors";
            }

            var status = new BpmConfigurationUpdateStatus()
            {
                Timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"),
                Name = action,
                Success = success,
                Comment = comment
            };
            Logger.Log(String.Format("[{0} {1}]: {2}", status.Timestamp, status.Name, status.Comment));
            
            _actionList.Add(status);
        }

        private void StopAndClear()
        {
            BatchSendTimer.Enabled = false;
            Updater = null;
        }

    }
    */
    
}
