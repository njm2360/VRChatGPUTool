using System;
using System.IO;
using System.Windows.Forms;
using TaskScheduler;

namespace VRCGPUTool.Util
{
    internal class StartupTask
    {
        private const string TASK_NAME = "VRChatGPUTool";
        private const string AUTHOR = "njm2360";
        private const string DESCRIPTION = "";

        public void registerTask()
        {
            ITaskService taskservice = null;
            ITaskFolder taskfolder = null;

            try
            {
                taskservice = new TaskScheduler.TaskScheduler();
                taskservice.Connect();
                taskfolder = taskservice.GetFolder("\\");
                var path = "\\" + TASK_NAME;

                ITaskDefinition taskDefinition = taskservice.NewTask(0);
                IRegistrationInfo registrationInfo = taskDefinition.RegistrationInfo;
                IActionCollection actionCollection = taskDefinition.Actions;
                IExecAction execAction = (IExecAction)actionCollection.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                ITriggerCollection triggerCollection = taskDefinition.Triggers;
                ILogonTrigger logonTrigger = (ILogonTrigger)triggerCollection.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);
                ITaskSettings taskSettings = taskDefinition.Settings;
                IPrincipal principal = taskDefinition.Principal;

                taskSettings.ExecutionTimeLimit = "PT0S";
                taskSettings.DisallowStartIfOnBatteries = true;
                taskSettings.Priority = 7;

                logonTrigger.Enabled = true;
                logonTrigger.UserId = $@"{Environment.UserDomainName}\{Environment.UserName}";

                registrationInfo.Author = AUTHOR;
                registrationInfo.Description = DESCRIPTION;

                principal.UserId = $@"{Environment.UserDomainName}\{Environment.UserName}";
                principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN;
                principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;

                execAction.Path = Application.ExecutablePath;
                execAction.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath);

                taskfolder.RegisterTaskDefinition(
                    path,
                    taskDefinition,
                    (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE,
                    null,
                    null,
                    _TASK_LOGON_TYPE.TASK_LOGON_NONE,
                    null
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラー\n{ex.Message}");
            }
            finally
            {
                if (taskservice != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(taskservice);
                }
                if (taskfolder != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(taskfolder);
                }
            }
        }

        public void removeTask()
        {
            ITaskService taskservice = null;
            ITaskFolder taskfolder = null;

            try
            {
                taskservice = new TaskScheduler.TaskScheduler();
                taskservice.Connect();
                taskfolder = taskservice.GetFolder("\\");

                taskfolder.DeleteTask(TASK_NAME, 0);
            }
            catch
            {
                //未登録
            }
            finally
            {
                if (taskservice != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(taskservice);
                }
                if (taskfolder != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(taskfolder);
                }
            }
        }
    }
}
