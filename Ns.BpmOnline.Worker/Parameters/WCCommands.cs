using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ns.BpmOnline.Worker.Parameters
{
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
                //"-packageName=\"{2}\" " +
                "{2}" +
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
                , WCpath, Operation, String.Empty, WorkspaceName, PackagesPath, WorkingTempPath,
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

    public class DownloadPackagesFromServerSettings
    {
        private string WorkspaceName { get; } = "Default";
        private string AutoExit { get; } = "true";

        public string WCpath { get; set; }
        public string Operation { get; } = "SaveDBContent";
        public string DestinationPath { get; set; }
        public string AppPath { get; set; }
        public string LogPath { get; set; }
        public string ContentTypes { get; } = "Repository";
        public string PackageName { get; set; }

        public string GetCmdParametersStr()
        {
            return String.Format(
               "{0} -operation={1} " +
               "-workspaceName={2} " +
               "-destinationPath=\"{3}\" " +
               "-webApplicationPath=\"{4}\" " +
               "-contentTypes={5} " +
               "-packageName={6} " +
               "-logPath=\"{7}\" " +
               "-autoExit={8}"
               , WCpath, Operation, WorkspaceName, DestinationPath, AppPath, ContentTypes, PackageName, LogPath, AutoExit);
        }
    }
}
