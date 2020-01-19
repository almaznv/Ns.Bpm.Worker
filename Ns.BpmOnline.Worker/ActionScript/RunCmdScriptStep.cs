using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Ns.BpmOnline.Worker.ActionScript
{
    public class RunCmdScriptStep : ActionScriptStep, IActionScriptStep
    {
        public override event ActionScriptStepExitHandler StepExit;
        public override event ActionScriptStepOutputHandler StepOutput;

        private string _workingDirectory;
        private string _cmdCommand;

        public RunCmdScriptStep() : base() {

        }

        public override void DoStep()
        {
            RunProcessAsync(_workingDirectory, _cmdCommand);
        }

        public override string GetName()
        {
            return String.Empty;
        }

        protected void SetWorkingDirectory(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        protected void SetCmdCommand(string cmdCommand)
        {
            _cmdCommand = cmdCommand;
        }

        private async void RunProcessAsync(string workingDirectory, string cmdCommand)
        {
            cmdCommand = cmdCommand.Replace(@"\", @"\\");

            try
            {
                StepOutput(cmdCommand);
               
                ProcessStartInfo startInfo = GetProcessInfo(workingDirectory, cmdCommand);
                startInfo.StandardOutputEncoding = Encoding.GetEncoding(866);

                using (var process = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                })
                {
                    await RunProcessAsync(process).ConfigureAwait(false);
                }
            } catch (Exception e)
            {
                StepExit(-1, e.Message);
            }

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

        private Task<int> RunProcessAsync(Process process)
        {

            var tcs = new TaskCompletionSource<int>();
            process.Exited += (s, ea) => {
                StepExit(process.ExitCode, String.Empty);
                tcs.SetResult(process.ExitCode);

            };

            process.OutputDataReceived += (s, ea) => {
                string outputStr = String.Format("output: {0}", ea.Data);
                StepOutput(outputStr);
            };
            process.ErrorDataReceived += (s, ea) => {
                if (String.IsNullOrEmpty(ea.Data) == false)
                {
                    string outputStr = String.Format("error: {0}", ea.Data);
                    StepOutput(outputStr);
                }
            };

            bool started = process.Start();

            if (!started)
            {
                throw new InvalidOperationException("Could not start process: " + process);
            }

            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;

        }

        
    }
}
