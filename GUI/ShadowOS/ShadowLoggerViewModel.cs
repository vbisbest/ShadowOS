using core.ui.data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;
using System.Xml;
using System.IO;

namespace ShadowOS
{
    public class ShadowLoggerViewModel : ViewModelBase
    {

        private TcpListener tcpListener;
        private ADB adb = null;

        delegate void ParseXMLInvoker(string response);

        public ShadowLoggerViewModel()
        {

        }

        public override void OnViewChanged(System.Windows.FrameworkElement oldValue, System.Windows.FrameworkElement newValue)
        {
            ViewModelBase.GlueView(newValue, Database);
            ViewModelBase.GlueView(newValue, Web);
            ViewModelBase.GlueView(newValue, FileSystem);

            if (oldValue != null)
            {
                oldValue.Loaded -= _Loaded;
            }

            if (newValue != null)
            {
                newValue.Loaded += _Loaded;
                Publish(new Garbage(() => { newValue.Loaded -= _Loaded; }, "ShadowLogger.Loaded"));
            }
        }

        private string GetSDKDirectory()
        {
            string path = string.Empty;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".exe";
            dlg.Filter = "Executable (*.exe)|sdk manager.exe";
            //dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif"; 
            string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android\\sdk");
            dlg.InitialDirectory = defaultPath;

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 

                if (dlg.FileName.Contains("SDK Manager.exe"))
                {
                    FileInfo fi = new FileInfo(dlg.FileName);
                    path = fi.DirectoryName;
                    if (!path.ToLower().Contains("sdk"))
                    {
                        //Eclipse stores the sdk dir in the same folder as sdk manager.exe
                        path = Path.Combine(path, "sdk");
                    }

                }
                else
                {

                }
            }

            return path;
        }

        void _Loaded(object sender, RoutedEventArgs e)
        {
#if !CIUI
            if (Window != null)
            {
                if (!Window.IsInDesignMode())
                {

                    if(!Global.IsAdministrator())
                    {
                        MessageBox.Show("You must have Administrator rights to run ShadowOS.  Try 'Run as Administrator'");
                        Application.Current.Shutdown(0);
                        return;
                    }


                    // Verify HAXM is installed
                    if(!ADB.VerifyHAXM())
                    {
                        MessageBox.Show("Intel HAXM is either not installed or the service is not running.  See the ShadowOS readme for instructions on installing");
                        Application.Current.Shutdown(0);
                        return;
                    }

                    // Check for the sdk directory
                    string sdkDir = Properties.Settings.Default["sdk_dir"].ToString();
                    if(sdkDir == string.Empty)
                    {
                        MessageBox.Show("On the next screen, please select the file 'SDK Manager.exe' from the Android SDK directory.\r\nFor Android Studio - placed by default in the current users appdata/Android directory\r\nFor Eclipse - placed in the Eclipse install folder under sdk");
                        sdkDir = GetSDKDirectory();

                        if (sdkDir == string.Empty)
                        {
                            // They did not pick the folder, shutdown the app
                            MessageBox.Show("The SDK is required for ShadowOS.  ShadowOS will now exit.");
                            Application.Current.Shutdown(0);
                            return;
                        }
                        else
                        {
                            // Do a quick check to make sure they picked the right directory
                            if (!File.Exists(Path.Combine(sdkDir, @"tools\android.bat")))
                            {
                                MessageBox.Show("This does not appear to be the SDK directory.  Please verify that the Android SDK is installed and try again.");
                                Application.Current.Shutdown(0);
                                return;
                            }
                        }

                        Properties.Settings.Default["sdk_dir"] = sdkDir;
                        Properties.Settings.Default.Save();
                        
                    }

                    try
                    {

                        adb = new ADB();
                        adb.ChromeEvent += adb_ChromeEvent;
                        adb.ShadowOSInstalled += adb_ShadowOSInstalled;
                        adb.EmulatorStarted += adb_EmulatorStarted;
                        adb.AppInstall += adb_AppInstall;
                        // Open the port and start listening for events
                        try
                        {
                            // Make sure the adb.exe is dead
                            adb.KillServer();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Could not open port: " + ex.Message);
                        }

                        WaitingMessage = "Setting up ShadowOS AVD";
                        IsWaiting = true;

                        adb.ShadowOSExists();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Error. " + ex.Message);
                    }
                }
            }
#endif
        }

        void adb_ChromeEvent(ChromeNetworkEvent message)
        {
            // parse the chrome event
            Action a = () => 
            {
                string scheme = message.parameters.request.uri.Scheme;
                try
                {
                    if (scheme == "file")
                    {
                        FileViewModel fileRequest = new FileViewModel(message, FileSystem.Files.Source.Count);
                        FileSystem.Files.Source.Add(fileRequest);
                    }
                    else if(scheme == "http" || scheme == "https")
                    {
                        WebRequestViewModel webRequest = new WebRequestViewModel(message, Web.Requests.Source.Count);
                        Web.Requests.Source.Add(webRequest);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error-" + ex.Message);
                }
            };
            a.BeginInvoke(DispatcherHelper.UIDispatcher);


        }

        void adb_AppInstall(string message)
        {
            IsWaiting = false;
            if (message != string.Empty)
            {
                MessageBox.Show(message);
            }
        }

        void adb_EmulatorStarted()
        {
            // Establish chrome debugger forwarding
            adb.BeginChromeWatcher();
            IsWaiting = false;
            
        }

        void adb_ShadowOSInstalled(string message)
        {
            IsWaiting = false;
            if (message != string.Empty)
            {
                    MessageBox.Show(message);
            }
        }

        private void OnAcceptConnection(IAsyncResult asyn)
        {
            try
            {
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)asyn.AsyncState;

                // Get the newly connected TcpClient
                TcpClient client = listener.EndAcceptTcpClient(asyn);

                byte[] buffer = new byte[1024];
                int result = 1;
                string response = string.Empty;

                NetworkStream networkStream = client.GetStream();
                while (result > 0 && client.Connected)
                {
                    result = networkStream.Read(buffer, 0, 1024);
                    response += Encoding.UTF8.GetString(buffer, 0, result);
                }

                client.Close();
                response = response.TrimEnd('\r', '\n');
                ParseXML(response);

                // Issue another connect, only do this if you want to handle multiple clients
                listener.BeginAcceptTcpClient(this.OnAcceptConnection, listener);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void ParseXML(string xml)
        {
            Action a = () => 
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    //Debug.WriteLine(xml);

                    if (doc.SelectSingleNode("//type").InnerText.ToString() == "request")
                    {
                        if (!xml.Contains("<host></host>"))
                        {
                            WebRequestViewModel webRequest = new WebRequestViewModel(doc, Web.Requests.Source.Count);
                            Web.Requests.Source.Add(webRequest);
                        }
                    }
                    else if (doc.SelectSingleNode("//type").InnerText.ToString() == "fileaccess")
                    {
                        FileViewModel fileView = new FileViewModel(doc, FileSystem.Files.Source.Count);
                        FileSystem.Files.Source.Add(fileView);
                    }
                    else if (doc.SelectSingleNode("//type").InnerText.ToString() == "sqlite")
                    {
                            TransactionViewModel transaction = new TransactionViewModel(doc, Database.Transactions.Source.Count);
                            Database.Transactions.Source.Add(transaction); 
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                    //MessageBox.Show("Error: " + ex.Message);
                }
                
            };
            a.BeginInvoke(DispatcherHelper.UIDispatcher);

        }

        public RelayCommand LearnMoreCommand
        {
            get
            {
                return new RelayCommand(p => {
                    Uri location = new Uri(@"http://www8.hp.com/us/en/software-solutions/mobile-app-security/index.html?jumpid=va_ezswn6y27r0");
                    Process.Start(new ProcessStartInfo(location.AbsoluteUri));
                });
            }
        }

        public bool ShowOpen
        {
            get
            {
                return IsFile || IsSql;
            }
        }

        public RelayCommand ClearAllCommand
        {
            get
            {
                return new RelayCommand(p => {
                    Web.ClearCommand.Execute(p);
                    FileSystem.ClearCommand.Execute(p);
                    Database.ClearCommand.Execute(p);
                });
            }
        }

        public RelayCommand ClearCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    if (IsWeb)
                    {
                        Web.ClearCommand.Execute(p);
                    }

                    if (IsFile)
                    {
                        FileSystem.ClearCommand.Execute(p);
                    }

                    if (IsSql)
                    {
                        Database.ClearCommand.Execute(p);
                    }
                });
            }
        }

        public RelayCommand OpenCommand
        {
            get
            {
                return new RelayCommand(p => {
                    if (CanOpen)
                    {
                        if (IsFile)
                        {
                            FileSystem.OpenCommand.Execute(p);
                        }

                        if (IsSql)
                        {
                            Database.OpenCommand.Execute(p);
                        }
                    }
                });
            }
        }

        public bool CanOpen
        {
            get
            {
                bool rc = false;

                if (IsFile)
                {
                    rc = FileSystem.SelectedFile != null;
                }

                if (IsSql)
                {
                    rc = Database.SelectedTransaction != null;
                }

                return rc;
            }
        }

        public DatabaseViewModel Database
        {
            get
            {
                return _Database;
            }
        }
        private DatabaseViewModel _Database = new DatabaseViewModel();

        public WebViewModel Web
        {
            get
            {
                return _Web;
            }
        }
        private WebViewModel _Web = new WebViewModel();

        public FileSystemViewModel FileSystem
        {
            get
            {
                return _FileSystem;
            }
        }
        private FileSystemViewModel _FileSystem = new FileSystemViewModel();

        public override void Disposing()
        {
#if !CIUI
            if (tcpListener != null)
            {
                tcpListener.Stop();
                tcpListener = null;
            }

            if (adb != null && adb.IsEmulatorRunning())
            {
                adb.Shutdown();
            }
#endif
        }

        #region Tabs
        public bool IsWeb
        {
            get
            {
                return _IsWeb;
            }
            set
            {
                _IsWeb = value;
                NotifyPropertyChanged(() => IsWeb);
                NotifyPropertyChanged(() => ShowOpen);
            }
        }
        private bool _IsWeb = true;

        public bool IsFile
        {
            get
            {
                return _IsFile;
            }
            set
            {
                _IsFile = value;
                NotifyPropertyChanged(() => IsFile);
                NotifyPropertyChanged(() => ShowOpen);
            }
        }
        private bool _IsFile = false;

        public bool IsSql
        {
            get
            {
                return _IsSql;
            }
            set
            {
                _IsSql = value;
                NotifyPropertyChanged(() => IsSql);
                NotifyPropertyChanged(() => ShowOpen);
            }
        }
        private bool _IsSql = false;
        #endregion

        #region Window
        public RelayCommand CloseCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    if (Window != null)
                    {
                        Window.Close();
                    }
                });
            }
        }

        public RelayCommand MaximizeCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    if (Window != null)
                    {
                        Window.WindowState = Window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                    }
                });
            }
        }

        public RelayCommand MinimizeCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    if (Window != null)
                    {
                        Window.WindowState = Window.WindowState == WindowState.Minimized ? WindowState.Normal : WindowState.Minimized;
                    }
                });
            }
        }

        public Window Window
        {
            get
            {
                return View.Find(typeof(Window)) as Window;
            }
        }
        #endregion

        #region MainMenuCommands
        public RelayCommand InstallCommand
        {
            get
            {
                return new RelayCommand(p => 
                {
                    if (!adb.IsEmulatorRunning())
                    {
                        MessageBox.Show("Emulator is not running", "ShadowOS");
                        return;
                    }

                    string path = string.Empty;
                    Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog();
                    file.Filter = "Android (*.apk)|*.apk";
                    bool? result = file.ShowDialog();
                    if (result==true)
                    {
                        path = file.FileName;
                    }
                    string output = string.Empty;

                    WaitingMessage = "Installing application";
                    IsWaiting = true;
                    adb.InstallApp(path);

                    /*

                    if (!)
                    {
                        IsWaiting = false;
                        MessageBox.Show("Failed to install app.\r\n" + output, "Install App");
                    }
                    else
                    {
                        IsWaiting = false;
                        MessageBox.Show("App installed.");
                    }
                     */
                });
            }
        }

        public RelayCommand StartCommand
        {
            get
            {
                return new RelayCommand(p => 
                {
                    try
                    {
                        WaitingMessage = "Starting emulator";
                        IsWaiting = true;

                        if (adb.IsEmulatorRunning())
                        {
                            IsWaiting = false;
                            MessageBox.Show("Emulator is already running.", "ShadowOS");
                            return;
                        }

                        // Stop just incase we are already listening
                        if (tcpListener != null)
                        {
                            tcpListener.Stop();
                        }

                        IPAddress ip = IPAddress.Parse("127.0.0.1");
                        tcpListener = new TcpListener(ip, 34345);
                        tcpListener.Start();
                        tcpListener.BeginAcceptTcpClient(this.OnAcceptConnection, tcpListener);

                        adb.StartEmulator();
                    }
                    catch(Exception ex)
                    {
                        IsWaiting = false;
                        MessageBox.Show("Error starting emulator. " + ex.Message);
                    }
                });
            }
        }
        #endregion

        #region Menu Commands
        public RelayCommand FileMenu1
        {
            get
            {
                return new RelayCommand(p =>
                {
                    IsMenuOpen = false;
                });
            }
        }

        public RelayCommand FileMenu2
        {
            get
            {
                return new RelayCommand(p =>
                {
                    IsMenuOpen = false;
                });
            }
        }

        public RelayCommand FileMenu3
        {
            get
            {
                return new RelayCommand(p =>
                {
                    IsMenuOpen = false;
                });
            }
        }

        public RelayCommand EditMenu1
        {
            get
            {
                return new RelayCommand(p =>
                {
                    IsMenuOpen = false;
                });
            }
        }

        public RelayCommand EditMenu2
        {
            get
            {
                return new RelayCommand(p =>
                {
                    IsMenuOpen = false;
                });
            }
        }

        public RelayCommand EditMenu3
        {
            get
            {
                return new RelayCommand(p =>
                {
                    IsMenuOpen = false;
                });
            }
        }

        public RelayCommand ToolsMenu1
        {
            get
            {
                return new RelayCommand(p =>
                {
                    IsMenuOpen = false;
                });
            }
        }

        public RelayCommand ToolsMenu2
        {
            get
            {
                return new RelayCommand(p =>
                {
                    ClearAllCommand.Execute(p);
                    IsMenuOpen = false;
                });
            }
        }

        public RelayCommand ToolsMenu3
        {
            get
            {
                return new RelayCommand(p =>
                {
                    IsMenuOpen = false;
                });
            }
        }

        public RelayCommand AboutMenu1
        {
            get
            {
                return new RelayCommand(p =>
                {
                    IsMenuOpen = false;

                    SplashScreen ss = new SplashScreen();
                    ss.Owner = this.Window;
                    ss.ShowDialog();
                });
            }
        }
        #endregion

        #region Wait Cursor
        public bool IsWaiting
        {
            get
            {
                return _IsWaiting;
            }
            set
            {
                _IsWaiting = value;
                NotifyPropertyChanged(() => IsWaiting);
            }
        }
        private bool _IsWaiting = false;

        public string WaitingMessage
        {
            get
            {
                return _WaitingMessage;
            }

            set
            {
                _WaitingMessage = value;
                NotifyPropertyChanged(() => WaitingMessage);
            }
        }
        private string _WaitingMessage = "Waiting ...";
        #endregion

        #region Menu Popup
        public bool IsMenuOpen
        {
            get
            {
                return _IsMenuOpen;
            }
            set
            {
                _IsMenuOpen = value;
                NotifyPropertyChanged(() => IsMenuOpen);
            }
        }
        private bool _IsMenuOpen = false;

        public RelayCommand ClosePopup
        {
            get
            {
                return new RelayCommand(p =>
                {
                    IsMenuOpen = false;
                });
            }
        }
        #endregion
    }

    public class WebViewModel : ViewModelBase
    {
        public WebViewModel()
        {
#if CIUI
            for (int i = 0; i < 10; i++)
            {
                Requests.Source.Add(new WebRequestViewModel() { Event = i, Headers = string.Format("Headers {0}", i), Host = string.Format("Host {0}", i), PostData = string.Format("Post Data {0}", i), RequestLine = string.Format("Request Line {0}", i), URL = string.Format("Url {0}", i) });
            }
#endif

            Requests.Sort("Event", ListSortDirection.Descending);
            Publish(new Garbage(Requests, "Web.Requests"));
        }

        public RelayCommand ClearCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    this.Requests.Source.Clear();
                });
            }
        }

        public WebRequestViewModel SelectedRequest
        {
            get
            {
                return _SelectedRequest;
            }
            set
            {
                _SelectedRequest = value;
                NotifyPropertyChanged(() => SelectedRequest);
            }
        }
        private WebRequestViewModel _SelectedRequest = null;

        public SortableViewModelCollection<WebRequestViewModel> Requests
        {
            get
            {
                return _Requests;
            }
        }
        private SortableViewModelCollection<WebRequestViewModel> _Requests = new SortableViewModelCollection<WebRequestViewModel>();

        public override void Disposing()
        {
        }
    }

    public class WebRequestViewModel : ViewModelBase
    {

#if CIUI
        public WebRequestViewModel()
        {
        }
#endif 

        public WebRequestViewModel(XmlDocument doc, int eventId)
        {
            try
            {
                string classs = doc.SelectSingleNode("//class").InnerText.ToString();
                byte[] classBytes = System.Convert.FromBase64String(classs);
                classs = Encoding.UTF8.GetString(classBytes);
                Debug.WriteLine("Class: " + classs);

                string host = doc.SelectSingleNode("//host").InnerText.ToString();
                byte[] hostBytes = System.Convert.FromBase64String(host);
                _Host = Encoding.UTF8.GetString(hostBytes);

                string headers = doc.SelectSingleNode("//headers").InnerText.ToString();
                byte[] headersBytes = System.Convert.FromBase64String(headers);
                _Headers = Encoding.UTF8.GetString(headersBytes);

                string postdata = doc.SelectSingleNode("//postdata").InnerText.ToString();
                byte[] postdataBytes = System.Convert.FromBase64String(postdata);
                _PostData = Encoding.UTF8.GetString(postdataBytes);

                string requestline = doc.SelectSingleNode("//requestline").InnerText.ToString();
                byte[] requestlineBytes = System.Convert.FromBase64String(requestline);
                _RequestLine = Encoding.UTF8.GetString(requestlineBytes);
                _Event = eventId;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Cannot parse request: " + ex.Message);
            }
        }

        public WebRequestViewModel(ChromeNetworkEvent networkEvent, int eventId)
        {
            try
            {                
                _Host = String.Format("{0}://{1}", networkEvent.parameters.request.uri.Scheme, networkEvent.parameters.request.uri.Host);
                _Headers = networkEvent.parameters.request.HeaderString;
                _URL = networkEvent.parameters.request.url;
                _RequestLine = String.Format("{0} {1} HTTP/1.1", networkEvent.parameters.request.method, networkEvent.parameters.request.uri.PathAndQuery);
                _PostData = networkEvent.parameters.request.postData;
                _Event = eventId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot parse request: " + ex.Message);
            }
        }

        public int Event
        {
            get
            {
                return _Event;
            }
            set
            {
                _Event = value;
                NotifyPropertyChanged(() => Event);
            }
        }
        private int _Event = 0;

        public string Headers
        {
            get
            {
                return _Headers;
            }
            set
            {
                _Headers = value;
                NotifyPropertyChanged(() => Headers);
            }
        }
        private string _Headers = string.Empty;

        public string PostData
        {
            get
            {
                return _PostData;
            }
            set
            {
                _PostData = value;
                NotifyPropertyChanged(() => PostData);
            }
        }
        private string _PostData = string.Empty;

        public string RequestLine
        {
            get
            {
                return _RequestLine;
            }
            set
            {
                _RequestLine = value;
                NotifyPropertyChanged(() => PostData);
            }
        }
        private string _RequestLine = string.Empty;

        public string Request
        {
            get
            {
                return _RequestLine + "\r\n" + _Headers + "\r\n" + _PostData;
            }
            set
            {
                // ignore
            }
        }

        public string URL
        {
            get
            {
                return _URL;
            }
            set
            {
                _URL = value;
                NotifyPropertyChanged(() => URL);
            }
        }
        private string _URL = string.Empty;

        public string Host
        {
            get
            {
                return _Host;
            }
            set
            {
                _Host = value;
                NotifyPropertyChanged(() => Host);
            }
        }
        private string _Host = string.Empty;

        public override void Disposing()
        {
        }
    }

    public class FileSystemViewModel : ViewModelBase
    {
        public FileSystemViewModel()
        {
#if CIUI
            for (int i = 0; i < 10; i++)
            {
                Files.Source.Add(new FileViewModel() { Event = i, Mode = string.Format("Mode {0}", i), Path = string.Format("Path {0}", i) });
            }
#endif

            Files.Sort("Event", ListSortDirection.Descending);

            Publish(new Garbage(Files, "FileSystem.Files"));
        }

        public RelayCommand ClearCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    this.Files.Source.Clear();
                });
            }
        }

        public RelayCommand OpenCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    string filePath = SelectedFile.Path;
                    string fileName = Path.GetFileName(filePath);
                    string target = Path.Combine(Path.GetTempPath(), fileName);

                    ADB adb = new ADB();
                    adb.GetFile(filePath, fileName, target);
                    if (!File.Exists(target))
                    {
                        MessageBox.Show("Could not open file");
                    }
                    else
                    {
                        Global.OpenNotepad(target);
                    }
                    
                });
            }
        }

        public FileViewModel SelectedFile
        {
            get
            {
                return _SelectedFile;
            }
            set
            {
                _SelectedFile = value;
                NotifyPropertyChanged(() => SelectedFile);
            }
        }
        private FileViewModel _SelectedFile = null;

        public SortableViewModelCollection<FileViewModel> Files
        {
            get
            {
                return _Files;
            }
        }
        private SortableViewModelCollection<FileViewModel> _Files = new SortableViewModelCollection<FileViewModel>();

        public override void Disposing()
        {
        }
    }

    public class FileViewModel : ViewModelBase
    {

#if CIUI
        public FileViewModel()
        {
        }
#endif 

        public FileViewModel(XmlDocument doc, int eventId)
        {
            try
            {
                string filepath = doc.SelectSingleNode("//filepath").InnerText.ToString();
                byte[] hostFilePath = System.Convert.FromBase64String(filepath);
                _Path = Encoding.UTF8.GetString(hostFilePath);
                _Mode = doc.SelectSingleNode("//access").InnerText.ToString();
                _Event = eventId;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Cannot parse File Access: " + ex.Message);
            }
        }

        
        public FileViewModel(ChromeNetworkEvent networkEvent, int eventId)
        {
            try
            {
                _Path = networkEvent.parameters.request.uri.AbsolutePath;
                _Mode = "write";
                _Event = eventId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot parse File Access: " + ex.Message);
            }
        }

        public int Event
        {
            get
            {
                return _Event;
            }
            set
            {
                _Event = value;
                NotifyPropertyChanged(() => Event);
            }
        }
        private int _Event = 0;

        public string Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                _Mode = value;
                NotifyPropertyChanged(() => Mode);
            }
        }
        private string _Mode = string.Empty;

        public string Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
                NotifyPropertyChanged(() => Path);
            }
        }
        private string _Path = string.Empty;

        public override void Disposing()
        {
        }
    }

    public class DatabaseViewModel : ViewModelBase
    {
        public DatabaseViewModel()
        {
#if CIUI
            for (int i = 0; i < 10; i++)
            {
                Transactions.Source.Add(new TransactionViewModel() { Event = i, Action = string.Format("Action {0}", i), Parameters = string.Format("Parameters {0}", i), Value = string.Format("Value {0}", i) });
            }
#endif

            Transactions.Sort("Event", ListSortDirection.Descending);
            Publish(new Garbage(Transactions, "Database.Transactions"));
        }

        public TransactionViewModel SelectedTransaction
        {
            get
            {
                return _SelectedTransaction;
            }
            set
            {
                _SelectedTransaction = value;
                NotifyPropertyChanged(() => SelectedTransaction);
            }
        }
        private TransactionViewModel _SelectedTransaction = null;

        public RelayCommand ClearCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    this.Transactions.Source.Clear();
                });
            }
        }

        public RelayCommand OpenCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    string filePath = SelectedTransaction.Path;
                    string fileName = Path.GetFileName(filePath);
                    string target = Path.Combine(Path.GetTempPath(), fileName);
                    ADB adb = new ADB();
                    adb.GetFile(filePath, fileName, target);
                    Global.OpenDatabase(target);

                    SelectedTransaction.ToString();
                });
            }
        }

        public SortableViewModelCollection<TransactionViewModel> Transactions
        {
            get
            {
                return _Transactions;
            }
        }
        private SortableViewModelCollection<TransactionViewModel> _Transactions = new SortableViewModelCollection<TransactionViewModel>();

        public override void Disposing()
        {
        }
    }

    public class TransactionViewModel : ViewModelBase
    {

#if CIUI
        public TransactionViewModel()
        {
        }
#endif

        public TransactionViewModel(XmlDocument doc, int eventId)
        {
            try
            {
                string value = doc.SelectSingleNode("//value").InnerText.ToString();
                byte[] byteValue = System.Convert.FromBase64String(value);
                _Value = Encoding.UTF8.GetString(byteValue);

                string paramsValue = doc.SelectSingleNode("//params").InnerText.ToString();
                byte[] byteParams = System.Convert.FromBase64String(paramsValue);
                _Parameters = Encoding.UTF8.GetString(byteParams);

                string path = doc.SelectSingleNode("//path").InnerText.ToString();
                byte[] bytePath = System.Convert.FromBase64String(path);
                _Path = Encoding.UTF8.GetString(bytePath);


                _Action = doc.SelectSingleNode("//action").InnerText.ToString();
                if(_Action == "Open")
                {
                    _Value = _Path;
                }

                _Event = eventId;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Cannot parse SQLite: " + ex.Message);
            }

        }

        public int Event
        {
            get
            {
                return _Event;
            }
            set
            {
                _Event = value;
                NotifyPropertyChanged(() => Event);
            }
        }

        private int _Event = 0;

        public string Action
        {
            get
            {
                return _Action;
            }
            set
            {
                _Action = value;
                NotifyPropertyChanged(() => Action);
            }
        }
        
        private string _Path = string.Empty;

        public string Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
                NotifyPropertyChanged(() => Path);
            }
        }

        private string _Action = string.Empty;

        public string Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                NotifyPropertyChanged(() => Value);
            }
        }
        private string _Value = string.Empty;

        public string Parameters
        {
            get
            {
                return _Parameters;
            }
            set
            {
                _Parameters = value;
                NotifyPropertyChanged(() => Parameters);
            }
        }
        private string _Parameters = string.Empty;

        public override void Disposing()
        {
        }
    }

    public class SortableViewModelCollection<T> : IDisposable where T : ViewModelBase
    {
        public void Sort(string path, ListSortDirection direction)
        {
            InitializeViewSource();

            SortDescription sort = new SortDescription(path, direction);

            _ViewSource.View.SortDescriptions.Clear();
            _SortDescriptions.Clear();

            _ViewSource.View.SortDescriptions.Add(sort);

            _ViewSource.View.Refresh();
        }

        void InitializeViewSource()
        {
            if (_ViewSource == null)
            {
                _ViewSource = new CollectionViewSource();
                _ViewSource.Source = _Source;
            }
        }

        public ICollectionView ViewSource
        {
            get
            {
                InitializeViewSource();

                return _ViewSource.View;
            }
        }
        private CollectionViewSource _ViewSource = null;
        private Dictionary<GridViewColumnHeader, SortDescription> _SortDescriptions = new Dictionary<GridViewColumnHeader, SortDescription>();

        public ObservableCollection<T> Source
        {
            get
            {
                return _Source;
            }
        }
        private ObservableCollection<T> _Source = new ObservableCollection<T>();

        public RelayCommand SortCommand
        {
            get
            {
                return new RelayCommand(p =>
                {
                    GridViewColumnHeader header = p as GridViewColumnHeader;

                    if (header.Column != null)
                    {
                        SortDescription asc = new SortDescription(((System.Windows.Data.Binding)(header.Column.DisplayMemberBinding)).Path.Path, ListSortDirection.Ascending);
                        SortDescription dsc = new SortDescription(((System.Windows.Data.Binding)(header.Column.DisplayMemberBinding)).Path.Path, ListSortDirection.Descending);

                        if (_SortDescriptions.Keys.Contains(header))
                        {
                            ListSortDirection d = _SortDescriptions[header].Direction;
                            _ViewSource.View.SortDescriptions.Clear();
                            _SortDescriptions.Clear();

                            if (d == ListSortDirection.Ascending)
                            {
                                _SortDescriptions.Add(header, dsc);
                                _ViewSource.View.SortDescriptions.Add(dsc);
                            }
                            else if (d == ListSortDirection.Descending)
                            {
                                _SortDescriptions.Add(header, asc);
                                _ViewSource.View.SortDescriptions.Add(asc);
                            }
                        }
                        else
                        {
                            _ViewSource.View.SortDescriptions.Clear();
                            _SortDescriptions.Clear();

                            _SortDescriptions.Add(header, dsc);
                            _ViewSource.View.SortDescriptions.Add(dsc);
                        }

                        _ViewSource.View.Refresh();
                    }
                });
            }
        }

        public void Dispose()
        {
            _Source.Clear();
            _SortDescriptions.Clear();

            _ViewSource.SortDescriptions.Clear();
            _ViewSource.Source = null;
            
        }
    }
}
