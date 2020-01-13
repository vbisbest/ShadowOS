using System;
using System.Diagnostics;
using AppKit;
using Foundation;
using Newtonsoft.Json;
using System.IO;

namespace ShadowOSMonitor
{
    public partial class ViewController : NSViewController
    {

        EventTableDataSource dataSource = new EventTableDataSource();
        //Process adb = new Process();

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ButtonCapture.Activated += ButtonCapture_Activated;
            ButtonClearEvents.Activated += ButtonClearEvents_Activated;
            TableEvents.DataSource = dataSource;
            TableEvents.Delegate = new EventTableDelegate(dataSource);
            TableEvents.DoubleClick += TableEvents_DoubleClick;
            // Do any additional setup after loading the view.
        }

        private void ButtonClearEvents_Activated(object sender, EventArgs e)
        {
            if(dataSource!=null)
            {
                dataSource.Events.Clear();
                TableEvents.ReloadData();
            }
        }

        private void TableEvents_DoubleClick(object sender, EventArgs e)
        {
            // Double clicked a row
            try
            {
                var row = TableEvents.ClickedRow;
                var selectedEvent = dataSource.Events[Convert.ToInt32(row)];

                var tempFolder = Path.GetTempPath();
                //string tempFolder = Directory.GetCurrentDirectory();
                string fileName = Path.GetFileName(selectedEvent.Details);
                string tempFileName = Path.Combine(tempFolder, fileName);
                
                GetFile(selectedEvent.Details, tempFileName);
                LaunchFile(tempFileName);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            
        }

        private void ButtonCapture_Activated(object sender, EventArgs e)
        {
            try
            {
                var adb = new Process();
                adb.StartInfo.FileName = "/Users/raykelly/Library/Developer/Xamarin/android-sdk-macosx/platform-tools/adb";
                adb.StartInfo.Arguments = "logcat *:s \"ShadowOS\"";
                adb.StartInfo.UseShellExecute = false;
                adb.StartInfo.RedirectStandardOutput = true;
                adb.EnableRaisingEvents = true;
                adb.OutputDataReceived += Adb_OutputDataReceived;
                adb.OutputDataReceived += (object s, System.Diagnostics.DataReceivedEventArgs args) => Console.WriteLine("out: " + args.Data);
                adb.Start();
                adb.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Adb_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if(e.Data == null)
            {
                return;
            }

            if(!e.Data.Contains("ShadowOS"))
            {
                return;
            }

            // Got an event from ShadowOS
            Console.WriteLine("received output: {0}", e.Data);

            try
            {
                // Parse out the details
                int start = e.Data.IndexOf("{", StringComparison.CurrentCulture);
                string data = e.Data.Substring(start);
                var shadowOSEvent = JsonConvert.DeserializeObject<ShadowOSEvent>(data);

                string details = string.Empty;

                if(shadowOSEvent.EventType == "http")
                {
                    details = shadowOSEvent.Data.Uri;
                }
                else
                {
                    details = shadowOSEvent.Data.Data;
                }

                dataSource.Events.Insert(0, new Event(shadowOSEvent.EventType, shadowOSEvent.Data.Action, details));

                this.InvokeOnMainThread(() =>
                {
                    TableEvents.ReloadData();
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        private void LaunchFile(string fileName)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = fileName;
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);
        }

        public void GetFile(string filePath, string target)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "/Users/raykelly/Library/Developer/Xamarin/android-sdk-macosx/platform-tools/adb";
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
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
