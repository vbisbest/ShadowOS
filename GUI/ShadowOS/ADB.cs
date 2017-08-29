using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Timers;

namespace ShadowOS
{
    public class ADB
    {
        private string SDK_PATH = string.Empty;

        public delegate void ShadowOSInstalledEventHandler(string message);
        public event ShadowOSInstalledEventHandler ShadowOSInstalled;

        public delegate void EmulatorStartedEventHandler();
        public event EmulatorStartedEventHandler EmulatorStarted;

        public delegate void AppInstallEventHandler(string message);
        public event AppInstallEventHandler AppInstall;

        public delegate void ChromeEventHandler(ChromeNetworkEvent message);
        public event ChromeEventHandler ChromeEvent;

        private System.Timers.Timer timer = null;
        private System.Timers.Timer chromeTimer = null;
        private ChromeListener chromeListener = new ChromeListener();
        private int currentForwardPort = 9222;

        private List<int> attachedProcesses = new List<int>();

        public ADB()
        {
            SDK_PATH = Properties.Settings.Default["sdk_dir"].ToString();
            chromeListener.ChromeEvent += chromeListener_ChromeEvent;
        }

        void chromeListener_ChromeEvent(ChromeNetworkEvent message)
        {
            if (ChromeEvent != null)
            {
                ChromeEvent(message);
            }
        }

        public static bool VerifyHAXM()
        {
            // Get a list of existing avd's
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "sc";
            startInfo.Arguments = "query intelhaxm";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            StringBuilder output = new StringBuilder();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.ErrorDataReceived += (sender, errorLine) => { if (errorLine.Data != null) Debug.WriteLine(errorLine.Data); };
                    exeProcess.OutputDataReceived += (sender, outputLine) => { if (outputLine.Data != null) output.AppendLine(outputLine.Data); };
                    exeProcess.BeginErrorReadLine();
                    exeProcess.BeginOutputReadLine();

                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (!output.ToString().Contains("RUNNING"))
            {
                return false;
            }

            return true;

        }


        public void GetFile(string filePath, string fileName, string target)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = SDK_PATH + "\\platform-tools\\adb.exe";
            startInfo.Arguments = "pull " + filePath + @" " + target;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch(Exception ex)
            {
                throw ex;               
            }
        }

        public void KillServer()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = SDK_PATH + "\\platform-tools\\adb.exe";
            startInfo.Arguments = "kill-server";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                // Ignore
                //throw ex;
            }
        }

        private void CheckVersion()
        {
            // Get the current version
            StreamReader stream = null;
            stream = File.OpenText("version.txt");

            string version = stream.ReadToEnd();
            stream.Close();

            if (version != GetShadowOSVersion())
            {
                try
                {
                    Directory.Delete(SDK_PATH + "\\platforms\\ShadowOS", true);
                }
                catch
                {
                    //ignore
                }
                try
                {
                    Directory.Delete(SDK_PATH + "\\system-images\\ShadowOS", true);
                }
                catch
                {
                    //ignore
                }

                ShadowOSTargetExists();
            }

        }


        public void ShadowOSExists()
        {
            try
            {
                Thread thread = new Thread(new ThreadStart(DoShadowOSExists));
                thread.Start();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void DoShadowOSExists()
        {
            // Get a list of existing avd's
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = SDK_PATH + "\\tools\\android.bat";
            startInfo.Arguments = "list avd";
            startInfo.WorkingDirectory = SDK_PATH + "\\tools\\";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            StringBuilder output = new StringBuilder();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.ErrorDataReceived += (sender, errorLine) => { if (errorLine.Data != null) Debug.WriteLine(errorLine.Data); };
                    exeProcess.OutputDataReceived += (sender, outputLine) => { if (outputLine.Data != null) output.AppendLine(outputLine.Data); };
                    exeProcess.BeginErrorReadLine();
                    exeProcess.BeginOutputReadLine();

                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                ShadowOSInstalled("Error - " + ex.Message);
            }

            try
            {
                GetEmulatorStatus();

                // Look for ShadowOS444
                bool exists = output.ToString().Contains("ShadowOS");

                if (exists)
                {
                    // See if we need to update
                    CheckVersion();
                }
                else
                {
                    CreateShadowOS();
                }

                if (ShadowOSInstalled != null)
                {
                    ShadowOSInstalled(string.Empty);
                }
            }
            catch(Exception e)
            {
                ShadowOSInstalled("Error - " + e.Message);
            }

        }

        private void EstablishChromeDebugger(int pid)
        {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = SDK_PATH + "\\platform-tools\\adb.exe";
            startInfo.WorkingDirectory = SDK_PATH + "\\tools\\";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            StringBuilder consoleOut = new StringBuilder();

            startInfo.Arguments = "forward tcp:" + currentForwardPort.ToString() + " localabstract:webview_devtools_remote_" + pid.ToString();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.ErrorDataReceived += (sender, errorLine) => { if (errorLine.Data != null) Debug.WriteLine(errorLine.Data); };
                    exeProcess.OutputDataReceived += (sender, outputLine) => { if (outputLine.Data != null) consoleOut.AppendLine(outputLine.Data); };
                    exeProcess.BeginErrorReadLine();
                    exeProcess.BeginOutputReadLine();

                    exeProcess.WaitForExit();
                }

                Console.WriteLine(consoleOut.ToString());
                chromeListener.Connect(currentForwardPort);
                currentForwardPort++;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetDebuggableProcesses(object source, ElapsedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = SDK_PATH + "\\platform-tools\\adb.exe";
            startInfo.Arguments = "shell cat /proc/net/unix";
            startInfo.WorkingDirectory = SDK_PATH + "\\tools\\";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            StringBuilder consoleOut = new StringBuilder();
            List<string> debuggableApps = new List<string>();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.ErrorDataReceived += (sender, errorLine) => { if (errorLine.Data != null) Debug.WriteLine(errorLine.Data); };
                    exeProcess.OutputDataReceived += (sender, outputLine) => 
                    {
                        if (outputLine.Data != null && outputLine.Data.Contains("devtools"))
                        {
                            //consoleOut.AppendLine(outputLine.Data);
                            debuggableApps.Add(outputLine.Data);
                        }
                    };

                    exeProcess.BeginErrorReadLine();
                    exeProcess.BeginOutputReadLine();

                    exeProcess.WaitForExit();
                }

                foreach (string app in debuggableApps)
                {
                    int startPos = app.LastIndexOf("_");
                    if(startPos > 0)
                    {
                        int pid = Convert.ToInt32(app.Substring(startPos + 1));
                        if(!attachedProcesses.Contains(pid))
                        {
                            // new process we need to attach
                            Console.WriteLine("Attaching " + app);
                            attachedProcesses.Add(pid);
                            EstablishChromeDebugger(pid);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }

        private string GetEmulatorStatus()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = SDK_PATH + "\\platform-tools\\adb.exe";
            startInfo.Arguments = "devices";
            startInfo.WorkingDirectory = SDK_PATH + "\\tools\\";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            StringBuilder consoleOut = new StringBuilder();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.ErrorDataReceived += (sender, errorLine) => { if (errorLine.Data != null) Debug.WriteLine(errorLine.Data); };
                    exeProcess.OutputDataReceived += (sender, outputLine) => { if (outputLine.Data != null) consoleOut.AppendLine(outputLine.Data); };
                    exeProcess.BeginErrorReadLine();
                    exeProcess.BeginOutputReadLine();

                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                ShadowOSInstalled("Error GetEmulatorStatus - " + ex.Message);
            }

            return consoleOut.ToString();
            
        }

        public bool IsEmulatorRunning()
        {
            string status = GetEmulatorStatus();
            return status.Contains("emulator-");
        }

        private void CreateShadowOS()
        {

            int targetID = ShadowOSTargetExists();
            if(targetID == 0)
            {
                //throw new Exception("The ShadowOS target does not exist");
                ShadowOSInstalled("The ShadowOS target does not exist");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = SDK_PATH + "\\tools\\android.bat";
            startInfo.Arguments = "create avd -n ShadowOS -t " + targetID.ToString() + " --abi x86";
            startInfo.WorkingDirectory = SDK_PATH + "\\tools\\";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;

            StringBuilder output = new StringBuilder();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.ErrorDataReceived += (sender, errorLine) => { if (errorLine.Data != null) Debug.WriteLine(errorLine.Data); };
                    exeProcess.OutputDataReceived += (sender, outputLine) => { if (outputLine.Data != null) output.AppendLine(outputLine.Data); };
                    exeProcess.BeginErrorReadLine();
                    exeProcess.BeginOutputReadLine();
                    exeProcess.StandardInput.Write("no");
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                //throw new Exception("Could not execute 'create avd' command. " + ex.Message);
                if(ShadowOSInstalled!=null)
                {
                    ShadowOSInstalled("Could not execute 'create avd' command. " + ex.Message);                  
                }
                return;
            }

            try
            {
                // copy the ini file
                System.IO.File.Copy("config.ini", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.android\\avd\\ShadowOS.avd\\config.ini", true);
            }
            catch (Exception exe)
            {
                //throw new Exception("Could not copy config.ini to AVD image. " + exe.Message);
                if (ShadowOSInstalled != null)
                {
                    ShadowOSInstalled("Could not copy config.ini to AVD image. " + exe.Message);   
                }
                return;
            }

            if (ShadowOSInstalled != null)
            {
                ShadowOSInstalled(string.Empty);
            }
        }

        public void Shutdown()
        {
            KillServer();
            KillTask();
        }

        private void KillTask()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "taskkill";
            startInfo.Arguments = "/IM emulator-x86.exe";
            //startInfo.WorkingDirectory = SDK_PATH + "\\tools\\";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            StringBuilder consoleOut = new StringBuilder();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.ErrorDataReceived += (sender, errorLine) => { if (errorLine.Data != null) Debug.WriteLine(errorLine.Data); };
                    exeProcess.OutputDataReceived += (sender, outputLine) => { if (outputLine.Data != null) consoleOut.AppendLine(outputLine.Data); };
                    exeProcess.BeginErrorReadLine();
                    exeProcess.BeginOutputReadLine();

                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                // ignore
                return;
            }
        }

        public void WaitForEmulator()
        {
            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 1000;
            timer.Enabled = true;
        }

        public void BeginChromeWatcher()
        {
            chromeTimer = new System.Timers.Timer();
            chromeTimer.Elapsed += new ElapsedEventHandler(GetDebuggableProcesses);
            chromeTimer.Interval = 3000;
            chromeTimer.Enabled = true;
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //Console.WriteLine("check");

            timer.Stop();
            string status = GetEmulatorStatus();

            if(status.Contains("\tdevice"))
            {
                EmulatorStarted();
            }
            else
            {
                timer.Start();
            }
        }

        private void DoWaitForEmulator()
        {

        }

        public void InstallApp(string filePath)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(DoInstallApp));
            thread.Start(filePath);
        }

        private void DoInstallApp(object filePath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = SDK_PATH + "\\platform-tools\\adb.exe";
            startInfo.Arguments = String.Format("install \"{0}\"",filePath);
            //startInfo.WorkingDirectory = SDK_PATH + "\\tools\\";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            StringBuilder consoleOut = new StringBuilder();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.ErrorDataReceived += (sender, errorLine) => 
                    {
                        if (errorLine.Data != null)
                        {
                            consoleOut.AppendLine(errorLine.Data); 
                        }
                    };

                    exeProcess.OutputDataReceived += (sender, outputLine) => 
                    {
                        if (outputLine.Data != null)
                        {
                            consoleOut.AppendLine(outputLine.Data);
                        }
                    };

                    exeProcess.BeginErrorReadLine();
                    exeProcess.BeginOutputReadLine();

                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                //throw ex;
                AppInstall("Failed to install app. " + ex.Message);
                return;
            }

            if (consoleOut.ToString().Contains("Success"))
            {
                AppInstall(string.Empty);
            }
            else
            {
                AppInstall("Failed to install app. " + consoleOut.ToString());
            }


            //Debug.WriteLine(consoleOut.ToString());
            //output = consoleOut.ToString();
            //return consoleOut.ToString().Contains("Success");
        }

        public string GetShadowOSVersion()
        {
            StreamReader stream = null;

            try
            {
                stream = File.OpenText(SDK_PATH + "\\platforms\\ShadowOS\\version.txt");
            }
            catch
            {
                return "0";
            }

            string version = stream.ReadToEnd();
            stream.Close();
            return version;            
        }

        public int ShadowOSTargetExists()
        {
            int targetID = 0;

            // First make sure the image and platform directorys exist
            if(!Directory.Exists(SDK_PATH + "\\platforms\\ShadowOS"))
            {
                Global.DirectoryCopy("platforms", SDK_PATH + "\\platforms", true);
                Global.DirectoryCopy("system-images", SDK_PATH + "\\system-images", true);
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = SDK_PATH + "\\tools\\android.bat";
            startInfo.Arguments = "list targets";
            startInfo.WorkingDirectory = SDK_PATH + "\\tools\\";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            StringBuilder output = new StringBuilder();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.ErrorDataReceived += (sender, errorLine) => { if (errorLine.Data != null) Debug.WriteLine(errorLine.Data); };
                    exeProcess.OutputDataReceived += (sender, outputLine) => { if (outputLine.Data != null) output.AppendLine(outputLine.Data); };
                    exeProcess.BeginErrorReadLine();
                    exeProcess.BeginOutputReadLine();

                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            int end = output.ToString().IndexOf("ShadowOS");
            if (end > 1)
            {
                int start = output.ToString().LastIndexOf("id: ", end);
                string temp = output.ToString().Substring(start + "id: ".Length, 2);
                temp = temp.Trim();
                targetID = Convert.ToInt32(temp);
            }
            
            return targetID;
        }

        public void StartEmulator()
        {
            Thread thread = new Thread(new ThreadStart(DoStartEmulator));
            thread.Start();
        }

        private void DoStartEmulator()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = SDK_PATH + "\\tools\\emulator";
            startInfo.Arguments = "-avd ShadowOS";
            //startInfo.Arguments = "-avd ShadowOSARM -partition-size 1024 -scale 0.65";
            startInfo.WorkingDirectory = SDK_PATH + "\\tools\\";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            StringBuilder output = new StringBuilder();

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.ErrorDataReceived += (sender, errorLine) => { if (errorLine.Data != null) Debug.WriteLine(errorLine.Data); };
                    exeProcess.OutputDataReceived += (sender, outputLine) => { if (outputLine.Data != null) output.AppendLine(outputLine.Data); };
                    exeProcess.BeginErrorReadLine();
                    exeProcess.BeginOutputReadLine();

                    //exeProcess.WaitForExit();
                    //exeProcess.Start();
                }
            }
            catch (Exception ex)
            {
                if(ShadowOSInstalled != null)
                {
                    ShadowOSInstalled("Error DoStartEmulator - " + ex.Message);
                }
                
            }

            string eval = output.ToString();

            if (eval.ToString().Contains("ERROR") == true)
            {
                ShadowOSInstalled("Failed to start emulator. " + output.ToString());
            }
            else
            {
                WaitForEmulator();
            }

            Debug.WriteLine(output.ToString());

            //return output.ToString().Contains("Name: Android 4.4.4");
        }

    }
}
