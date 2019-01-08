using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Ns.BpmOnline.Worker
{

    public class BpmConfigurationUpdater
    {
        private string _webAppPath;
        private DownloadPackagesFromSvnSettings _downloadFromSvnSettings;
        private UploadPackagesToServerSettings _uploadToServerSettings;
        private BuildConfigurationSettings _buildConfigurationSettings;

        private delegate void ProcessExitHandler(string processName, int exitCode);
        private event ProcessExitHandler ProcessExit;

        public delegate void UpdateStatusHandler(string action, bool success = true, string comment = "");
        public delegate void ProcessOutputHandler(string message);

        public event ProcessOutputHandler ProcessOutput;
        public event UpdateStatusHandler SetUpdateStatus;

        public BpmConfigurationUpdater(
            string webAppPath,
            DownloadPackagesFromSvnSettings downloadFromSvnSettings,
            UploadPackagesToServerSettings uploadToServerSettings,
            BuildConfigurationSettings buildConfigurationSettings)
        {
            _webAppPath = webAppPath;
            _downloadFromSvnSettings = downloadFromSvnSettings;
            _uploadToServerSettings = uploadToServerSettings;
            _buildConfigurationSettings = buildConfigurationSettings;

            this.ProcessExit += RunNextProcess;

        }

        public void Run()
        {
            SetUpdateStatus("START");
            RunProcessAsync(_downloadFromSvnSettings.Operation, _webAppPath, _downloadFromSvnSettings.GetCmdParametersStr());
        }

        private void RunNextProcess(string processName, int exitCode)
        {
            Logger.Log("finish " + processName + " "+ exitCode.ToString());
            if (exitCode != 0)
            {
                SetUpdateStatus("FINISH", false);
                return;
            }

            switch (processName)
            {
                case "SaveVersionSvnContent":
                    
                    SetUpdateStatus("FINISHSTEP", true, "SaveVersionSvnContent");
                    RunProcessAsync(_uploadToServerSettings.Operation, _webAppPath, _uploadToServerSettings.GetCmdParametersStr());
                break;
                case "InstallFromRepository":
                    SetUpdateStatus("FINISHSTEP", true, "InstallFromRepository");
                    RunProcessAsync(_buildConfigurationSettings.Operation, _webAppPath, _buildConfigurationSettings.GetCmdParametersStr());
                    break;
                case "BuildConfiguration":
                    SetUpdateStatus("FINISH", true);
                    break;
                default:
                    SetUpdateStatus("FINISH", false);
                    break;
            }

        }

        public void ClearDirectory(string path)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            foreach (FileInfo file in di.EnumerateFiles())
            {
                file.Delete();
            }
        }

        public void SetPackages(string packageName)
        {
            _downloadFromSvnSettings.PackageName = packageName;
            _uploadToServerSettings.PackageName = packageName;
        }

        public void SetBranch(string branchName)
        {
            _downloadFromSvnSettings.Branch = branchName;
        }

        private static ProcessStartInfo GetProcessInfo(string workingDirectory, string cmdCommand)
        {
            // int ExitCode;
            ProcessStartInfo ProcessInfo;

            //Process process;
            ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + cmdCommand);
            ProcessInfo.CreateNoWindow = false;
            ProcessInfo.UseShellExecute = false;
            ProcessInfo.WorkingDirectory = workingDirectory;

            // *** Redirect the output ***
            ProcessInfo.RedirectStandardError = true;
            ProcessInfo.RedirectStandardOutput = true;

            return ProcessInfo;
        }

        public async void RunProcessAsync(string processName, string workingDirectory, string cmdCommand)
        {
            cmdCommand = cmdCommand.Replace(@"\", @"\\");

            try
            {
                SetUpdateStatus("STARTSTEP", true, processName);
                ProcessOutput(cmdCommand);
               
                ProcessStartInfo startInfo = GetProcessInfo(workingDirectory, cmdCommand);

                using (var process = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                })
                {
                    await RunProcessAsync(process, processName).ConfigureAwait(false);
                }
            } catch (Exception e)
            {
                SetUpdateStatus("FINISH", false, e.Message);
            }

        }

        private Task<int> RunProcessAsync(Process process, string processName)
        {

            var tcs = new TaskCompletionSource<int>();
            process.Exited += (s, ea) => {
                ProcessExit(processName, process.ExitCode);
                tcs.SetResult(process.ExitCode);

            };

            process.OutputDataReceived += (s, ea) => {
                string outputStr = String.Format("output: {0}", ea.Data);
                ProcessOutput(outputStr);
            };
            process.ErrorDataReceived += (s, ea) => {
                if (String.IsNullOrEmpty(ea.Data) == false)
                {
                    string outputStr = String.Format("error: {0}", ea.Data);
                    ProcessOutput(outputStr);
                }
            };

            bool started = process.Start();

            if (!started)
            {
                //you may allow for the process to be re-used (started = false) 
                //but I'm not sure about the guarantees of the Exited event in such a case
                throw new InvalidOperationException("Could not start process: " + process);
            }

            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;

        }

    }

    public class ProjectSvnSettings
    {
        public string Uri { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class DownloadPackagesFromSvnSettings
    {
        private string CultureName { get; set; } = "ru-RU";
        private string AutoExit { get; } = "true";

        public string WCpath { get; set; }
        public string Operation { get; } = "SaveVersionSvnContent";
        public string PackagesPath { get; set; }
        public string WorkingPath { get; set; }
        public string PackageName { get; set; }
        public string Branch { get; set; }
        public string ExcludeDependentPackages { get; set; } = "true";
        public ProjectSvnSettings Svn { get; set; }
        public string LogPath { get; set; }

        public string GetCmdParametersStr()
        {
            return String.Format(
                "{0} -operation={1} " +
                "-destinationPath=\"{2}\" " +
                "-workingCopyPath=\"{3}\" " +
                "-sourcePath={4} " +
                "-packageName={5} " +
                "-packageVersion={6} " +
                "-sourceControlLogin={7} " +
                "-sourceControlPassword={8} " +
                "-cultureName={9} " +
                "-logPath=\"{10}\" " +
                "-excludeDependentPackages={11} " +
                "-autoExit={12}"
                , WCpath, Operation, PackagesPath, WorkingPath, Svn.Uri, PackageName, Branch, Svn.Login, Svn.Password, CultureName, LogPath, ExcludeDependentPackages, AutoExit);
        }
    }

    public class UploadPackagesToServerSettings
    {
        private string WorkspaceName { get; set; } = "Default";
        private string SkipConstraints { get; } = "false";
        private string SkipValidateActions { get; } = "true";
        private string RegenerateSchemaSources { get; } = "true";
        private string UpdateDBStructure { get; } = "true";
        private string UpdateSystemDBStructure { get; } = "false";
        private string InstallPackageSqlScript { get; } = "true";
        private string InstallPackageData { get; } = "true";
        private string ContinueIfError { get; } = "true";
        private string AutoExit { get; } = "true";

        public string WCpath { get; set; }
        public string AppPath { get; set; }
        public string WebAppPath { get; set; }
        public string Operation { get; } = "InstallFromRepository";
        public string PackageName { get; set; }
        public string WorkingTempPath { get; set; }
        public string PackagesPath { get; set; }
        public string LogPath { get; set; }
        
        public string GetCmdParametersStr()
        {
            return String.Format(
                "{0} -operation={1} " +
                "-packageName=\"{2}\" " +
                "-workspaceName={3} " +
                "-sourcePath=\"{4}\" " +
                "-destinationPath=\"{5}\" " +
                "-skipConstraints={6} " +
                "-skipValidateActions={7} " +
                "-regenerateSchemaSources={8} " +
                "-updateDBStructure={9} " +
                "-updateSystemDBStructure={10} " +
                "-installPackageSqlScript={11} " +
                "-installPackageData={12} " +
                "-continueIfError={13} " +
                "-webApplicationPath=\"{14}\" " +
                "-confRuntimeParentDirectory=\"{15}\" " +
                "-logPath=\"{16}\" " +
                "-autoExit={17}"
                , WCpath, Operation, PackageName, WorkspaceName, PackagesPath, WorkingTempPath,
                SkipConstraints, SkipValidateActions, RegenerateSchemaSources, UpdateDBStructure, UpdateSystemDBStructure, InstallPackageSqlScript, InstallPackageData, ContinueIfError,
                AppPath, WebAppPath, LogPath, AutoExit);
        }
    }

    public class BuildConfigurationSettings
    {
        private string WorkspaceName { get; } = "Default";
        private string Force { get; } = "false";
        private string AutoExit { get; } = "true";

        public string WCpath { get; set; }
        public string Operation { get; } = "BuildConfiguration";
        public string AppPath { get; set; }
        public string WebAppPath { get; set; }
        public string LogPath { get; set; }
        
        public string GetCmdParametersStr()
        {
            return String.Format(
               "{0} -operation={1} " +
               "-workspaceName={2} " +
               "-destinationPath=\"{3}\" " +
               "-webApplicationPath=\"{4}\" " +
               "-force={5} " +
               "-logPath=\"{6}\" " +
               "-autoExit={7}"
               , WCpath, Operation, WorkspaceName, WebAppPath, AppPath, Force, LogPath, AutoExit);
        }
    }
}
