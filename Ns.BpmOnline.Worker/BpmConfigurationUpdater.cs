using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Ns.BpmOnline.Worker.Parameters;

namespace Ns.BpmOnline.Worker
{

    public class BpmConfigurationUpdater
    {
        private string _webAppPath;
        private DownloadPackagesFromServerSettings _downloadPackagesFromServer;
        private DownloadPackagesFromSvnSettings _downloadFromSvnSettings;
        private UploadPackagesToServerSettings _uploadToServerSettings;
        private BuildConfigurationSettings _buildConfigurationSettings;

        private bool _needDownloadPackages = false;

        private delegate void ProcessExitHandler(string processName, int exitCode);
        private event ProcessExitHandler ProcessExit;

        public delegate void UpdateStatusHandler(string action, bool success = true, string comment = "");
        public delegate void ProcessOutputHandler(string message);

        public event ProcessOutputHandler ProcessOutput;
        public event UpdateStatusHandler SetUpdateStatus;

        public BpmConfigurationUpdater(
            string webAppPath,
            DownloadPackagesFromServerSettings downloadPackagesFromServer,
            DownloadPackagesFromSvnSettings downloadFromSvnSettings,
            UploadPackagesToServerSettings uploadToServerSettings,
            BuildConfigurationSettings buildConfigurationSettings)
        {
            _webAppPath = webAppPath;
            _downloadPackagesFromServer = downloadPackagesFromServer;
            _downloadFromSvnSettings = downloadFromSvnSettings;
            _uploadToServerSettings = uploadToServerSettings;
            _buildConfigurationSettings = buildConfigurationSettings;

            this.ProcessExit += RunNextProcess;

        }

        public void Run(bool needConfigurationBackup)
        {
            
        }

        public void RunUpdateFromSvn(bool needConfigurationBackup = false)
        {
            SetUpdateStatus("START");
            if (needConfigurationBackup)
            {
                RunProcessAsync(_downloadPackagesFromServer.Operation, _webAppPath, _downloadPackagesFromServer.GetCmdParametersStr());
            } else
            {
                RunProcessAsync(_downloadFromSvnSettings.Operation, _webAppPath, _downloadFromSvnSettings.GetCmdParametersStr());
            }
            
        }

        public void RestorePackagesFromLastBackup()
        {
            SetUpdateStatus("START");
            _uploadToServerSettings.PackagesPath = _downloadPackagesFromServer.DestinationPath;
            RunProcessAsync(_uploadToServerSettings.Operation, _webAppPath, _uploadToServerSettings.GetCmdParametersStr());

        }

        public void BuildStaticFilesOnly()
        {
            SetUpdateStatus("START");
            RunProcessAsync(_buildConfigurationSettings.Operation, _webAppPath, _buildConfigurationSettings.GetCmdParametersStr());

        }

        private void RemovePackagesAvterSaveDBContent(string path, string packages)
        {
            string[] filePaths = Directory.GetFiles(path);
            var packagesList = packages.Split(',');

            foreach (string filePath in filePaths)
            {
                var name = Path.GetFileNameWithoutExtension((new FileInfo(filePath).Name));
                if (!packagesList.Contains(name))
                {
                    File.Delete(filePath);
                }

            }
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
                case "SaveDBContent":
                    RemovePackagesAvterSaveDBContent(_downloadPackagesFromServer.DestinationPath, _downloadPackagesFromServer.PackageName);
                    SetUpdateStatus("FINISHSTEP", true, "SaveDBContent");
                    RunProcessAsync(_downloadFromSvnSettings.Operation, _webAppPath, _downloadFromSvnSettings.GetCmdParametersStr());
                break;
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
            _downloadPackagesFromServer.PackageName = packageName;
            _downloadFromSvnSettings.PackageName = packageName;
            _uploadToServerSettings.PackageName = packageName;
        }

        public void SetBranch(string branchName)
        {
            _downloadFromSvnSettings.Branch = branchName;
        }

        public void SetNeedDownloadPackages(bool isNeed)
        {
            _needDownloadPackages = isNeed;
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
                startInfo.StandardOutputEncoding = Encoding.GetEncoding(866);

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

   
}
