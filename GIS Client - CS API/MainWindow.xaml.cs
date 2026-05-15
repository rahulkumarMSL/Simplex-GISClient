using Gis.Utils.TCP_IP;
using Gis.Utils.Threading;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;




namespace GIS_Client___CS_API
{
    /*
     * This is the class which implements the main window.
     */
    public partial class MainWindow : System.Windows.Window
    {

        private Client m_client = new Client("localhost", 2000);     // The TCP/IP client.




        // added by Rahul
        private FileSystemWatcher fsw1 = new FileSystemWatcher();
        private FileSystemWatcher fsw2 = new FileSystemWatcher();

        private string sMessage;
        private string[] m_sMessageLog = new string[0];
        private string m_sConnectionStatus;

        private string tmprecordno;
        private string[] recordarr;
        private int tmpnumrecno;

        private BackgroundWorker BackgroundWorker6 = new BackgroundWorker();
        private BackgroundWorker BackgroundWorker1 = new BackgroundWorker();
        private DispatcherTimer Timer1 = new DispatcherTimer();


        private string totgetpagecnt = "";
        private string getpagecnt = "";
        private string processfilenm = "";
        private string[] strlog;
        private string fName = @"C:\print_data";   // change if needed


        private System.Drawing.Image Dataimg;
        private Bitmap Databitmap;
        private Bitmap barcodeImage;
        private float tmprotate;

        private string tmpripimgname;

        private System.Drawing.Image tmpimg;

        private BackgroundWorker backgroundWorker3 = new BackgroundWorker();
        private BackgroundWorker backgroundWorker6 = new BackgroundWorker();
        private BackgroundWorker backgroundWorker4 = new BackgroundWorker();
        private BackgroundWorker backgroundWorker5 = new BackgroundWorker();
        private BackgroundWorker backgroundWorker7 = new BackgroundWorker();








        private bool m_DialogInitialised = false;       // Whether the dialog has been initialised or not.
        private bool m_DialogClosed = false;            // Whether the dialog has been closed or not.


        private WorkerThread m_ConnectToServerThread = null;            // The connected to server thread.
        private WorkerThread m_DisconnectFromServerThread = null;       // The disconnected from server thread.

        // Comment by Rahul
        //private WorkerThread m_SendCommandsThread = null;               // The send commands thread.

        private WorkerThread m_StopThread = null;                       // The stop thread.

        private volatile bool m_DisconnectingFromServer = false;        // Whether the client is disconnecting from the server or not.


        // Comment by Rahul

        //private Mutex m_CommandsListLock = new Mutex();                                         // The commands list lock.
        //private ManualResetEvent m_CommandsListNotEmptyEvent = new ManualResetEvent(false);     // The event which signals when there is a command to sent.

        private ManualResetEvent m_ReadyToPrintReceivedEvent = new ManualResetEvent(false);     // The event which signals when the ready to print signal has been received.


        enum LogType { LT_SEND, LT_RECEIVE, LT_SEND_ERROR, LT_INFO, LT_WARNING, LT_ERROR };       // The types of log message.


        private string logClientRegistryKey = "CSAPILogClient";         // The log client registry key.
        private string logFileRegistryKey = "CSAPILogFile";             // The log file registry key.
        private string addressRegistryKey = "CSAPIAddress";             // The server TCP/IP address registry key.
        private string portRegistryKey = "CSAPIPort";                   // The server TCP/IP port registry key.

        //Comment by Rahul

        //private string commandRegistryKey = "CSAPICommand";             // The command registry key.
        //private string commandsFileRegistryKey = "CSAPIFile";           // The command file registry key.
        //private string repeatQueueRegistryKey = "CSAPIRepeatQueue";     // The repeat queue registry key.


        #region Dialog

        /*
	     * This is the constructor for the dialog which initialises all controls.
	     */



    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Get working area (without taskbar)
        var workingArea = SystemParameters.WorkArea;

        //// Set window size (small floating mode)
        //this.Width = 350;
        //this.Height = 250;

        // Bottom-right corner position
        //this.Left = workingArea.Right - this.Width - 10;
        //this.Top = workingArea.Bottom - this.Height - 10;
    }





    public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

        // for shift X1

            backgroundWorker4.WorkerReportsProgress = true;

            backgroundWorker4.DoWork += BackgroundWorker4_DoWork;
            backgroundWorker4.ProgressChanged += BackgroundWorker4_ProgressChanged;
            backgroundWorker4.RunWorkerCompleted += BackgroundWorker4_RunWorkerCompleted;


            // for shift Y1

            backgroundWorker5.WorkerReportsProgress = true;

            backgroundWorker5.DoWork += BackgroundWorker5_DoWork;
            backgroundWorker5.ProgressChanged += BackgroundWorker5_ProgressChanged;
            backgroundWorker5.RunWorkerCompleted += BackgroundWorker5_RunWorkerCompleted;




            backgroundWorker7.WorkerReportsProgress = false;
            backgroundWorker7.WorkerSupportsCancellation = true;

            backgroundWorker7.DoWork += BackgroundWorker7_DoWork;
            backgroundWorker7.RunWorkerCompleted += BackgroundWorker7_RunWorkerCompleted;



            backgroundWorker3.WorkerReportsProgress = true;
            backgroundWorker3.WorkerSupportsCancellation = true;

            backgroundWorker3.DoWork += BackgroundWorker3_DoWork;
            backgroundWorker3.ProgressChanged += BackgroundWorker3_ProgressChanged;
            backgroundWorker3.RunWorkerCompleted += BackgroundWorker3_RunWorkerCompleted;

            setupTCP_IP(); // attach callbacks
        }



        // for shift X1
        private void BackgroundWorker4_DoWork(object sender, DoWorkEventArgs e)
        {
            string shiftValue = e.Argument.ToString();

            string sMessage = "P,C,P,Buffer_X," + shiftValue;

            m_client.sendMessageToServer(sMessage);

            backgroundWorker4.ReportProgress(10);
        }



        private void BackgroundWorker4_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            m_client.testexample("Buffer_X");
        }



        private void BackgroundWorker4_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Nothing here (same as VB)
            // 🔹 Get values from client
            string bufferValue = m_client.testexample("Buffer_X");


            // 🔹 Update UI
            LblPrintingpageX1.Text = "Current value: " + bufferValue;

        }








        // for shift Y1
        private void BackgroundWorker5_DoWork(object sender, DoWorkEventArgs e)
        {
            string shiftValue = e.Argument.ToString();

            string sMessage = "P,C,P,Buffer_Y," + shiftValue;

            m_client.sendMessageToServer(sMessage);

            backgroundWorker5.ReportProgress(10);
        }



        private void BackgroundWorker5_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            m_client.testexample("Buffer_Y");
        }



        private void BackgroundWorker5_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Nothing here (same as VB)
            // 🔹 Get values from client
            string bufferValue = m_client.testexample("Buffer_Y");


            // 🔹 Update UI
            LblPrintingpageY1.Text = "Current value: " + bufferValue;

        }



        /*
	     * This is the constructor for the dialog which performs all of the clean up.
	     */
        ~MainWindow()
        {

        }



       
        private bool setupTCP_IP()
        {
            // Create the callback pointers.
            m_client.Connected += new Client.ConnectedHandler(ConnectedCallback);
            m_client.Disconnected += new Client.DisconnectedHandler(DisconnectCallback);
            m_client.MessageSent += new Client.MessageSentHandler(MessageSentCallback);
            m_client.MessageReceived += new Client.MessageReceivedHandler(MessageReceivedCallback);
            m_client.Status += new Client.StatusHandler(StatusCallback);
            m_client.TrafficLight += new Client.TrafficLightHandler(TrafficLightCallback);
            m_client.SwatheProcessed += new Client.SwatheProcessedHandler(SwatheProcessedCallback);
            m_client.LabelNumber += new Client.LabelNumberHandler(LabelNumberCallback);
            m_client.Position += new Client.PositionHandler(PositionCallback);
            m_client.InkSystemParameterValue += new Client.InkSystemParameterValueHandler(InkSystemParameterValueCallback);
            m_client.PrintheadStatus += new Client.PrintheadStatusHandler(PrintHeadStatusCallback);
            m_client.PrintStarted += new Client.PrintStartedHandler(PrintStartedCallback);
            m_client.ReadyToPrint += new Client.ReadyToPrintHandler(ReadyToPrintCallback);
            m_client.EndOfPrinting += new Client.EndOfPrintingHandler(EndOfPrintingCallback);
            m_client.PrintServerReporter += new Client.PrintServerReporterHandler(PrintServerReporterCallback);
            m_client.InterfaceLog += new Client.InterfaceLogHandler(InterfaceLogCallback);

            m_ConnectToServerThread = new WorkerThread(ConnectToServer);
            m_DisconnectFromServerThread = new WorkerThread(DisconnectFromServer);

            // Comment by Rahul

            //m_SendCommandsThread = new WorkerThread(SendCommands);

            m_StopThread = new WorkerThread(Stop);

            // Inititalise the TCP/IP client.
            bool logToFileEnabled = false;
            //if (logClientCheckBox.IsChecked == true)
            //{
            //    logToFileEnabled = true;
            //}
            //if (!m_client.Initialise("", "", logToFileEnabled, logFileTextBox.Text, true))
            //{
            //    return false;
            //}

            return true;
        }

        #endregion


        #region Interface Events

        /*
	     * This is called when the log file text box loses focus.
	     */
        private void logFileTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!m_DialogInitialised)
            {
                return;
            }

            //writeRegistrySetting(logFileRegistryKey, logFileTextBox.Text);

            UpdateLogSettings();
        }
        /*
	     * This is called when the log file path button is pressed.
	     */
        private void logFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            //string logFilePath = logFileTextBox.Text;

            // Display the file dialog.
            System.Windows.Forms.SaveFileDialog fileDialog = new System.Windows.Forms.SaveFileDialog();
            //fileDialog.InitialDirectory = logFilePath;
            fileDialog.Filter = "txt files (*.txt)|*.txt";
            fileDialog.FilterIndex = 2;
            fileDialog.RestoreDirectory = true;
            //if (fileDialog.ShowDialog() == true)
            //{
            //    logFilePath = fileDialog.FileName;
            //}

            //logFileTextBox.Text = logFilePath;
            //writeRegistrySetting(logFileRegistryKey, logFilePath);

            UpdateLogSettings();
        }
        /*
	     * This is called when the log client check box is checked.
	     */
        private void logClientCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!m_DialogInitialised)
            {
                return;
            }

            writeRegistrySetting(logClientRegistryKey, "True");

            UpdateLogSettings();
        }
        /*
	     * This is called when the log client check box is unchecked.
	     */
        private void logClientCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!m_DialogInitialised)
            {
                return;
            }

            writeRegistrySetting(logClientRegistryKey, "False");

            UpdateLogSettings();
        }

        


        private DateTime _connectedTime;

        private DispatcherTimer _speedTimer;
        private bool _isSpeedRunning = false;
        private bool _isConnected = false;
        private CancellationTokenSource _speedCancellationTokenSource;

        private async void connectButton1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sAddress = addressTextBox1.Text;
                int iPort = Convert.ToInt32(portTextBox1.Text);

                //  CONNECT ONLY ONCE
                bool isConnected = m_client.ConnectToServer(sAddress, iPort, 0);

                if (!isConnected)
                {
                    MessageBox.Show("Connection Failed");
                    return;
                }

                MessageBox.Show("Connected Successfully");

                _isConnected = true;

                if (trafficLightCallback(0))
                {
                    _connectedTime = DateTime.Now;

                    LblConnectionTime.Text = "Connected since: " + _connectedTime.ToString("HH:mm:ss");
                    LblConnectionTime.Visibility = Visibility.Visible;

                    statusText1.Text = "Connected";
                    statusText1.Foreground = System.Windows.Media.Brushes.Green;
                    ConnectionIndicator.Fill = System.Windows.Media.Brushes.Green;
                }

                // Initial Data Fetch
                string bufferValue = m_client.testexample("Buffer_X");
                string bufferValueY = m_client.testexample("Buffer_Y");
                List<string> modes = m_client.testexampleSysMode("TmpPrintMode");
                string currentMode = m_client.testexamplePrintMode("TmpPrintMode");
                string speed = m_client.testexampleEncoder("TmpEncoderSpeed");

                LblPrintingpageX1.Text = "Current value: " + bufferValue;
                LblPrintingpageY1.Text = "Current value: " + bufferValueY;

                TxtPrintShift1.Text = bufferValue ?? "0.0";
                TxtPrintShiftY1.Text = bufferValueY ?? "0.0";

                LblSpeed1.Text = speed ?? "0";

                CboPrintMode1.Items.Clear();
                foreach (var mode in modes)
                {
                    CboPrintMode1.Items.Add(mode);
                }

                LblPrintMode1.Text = "Current Print Mode : " + currentMode;

                sMessage = "P,L";
                m_client.sendMessageToServer(sMessage);

                // START LIVE SPEED POLLING
                StartLiveSpeed();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        

        private void StartLiveSpeed()
        {
            _speedCancellationTokenSource?.Cancel();
            _speedCancellationTokenSource = new CancellationTokenSource();

            var token = _speedCancellationTokenSource.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested && _isConnected)
                {
                    try
                    {
                        string speed = m_client.testexampleEncoder("TmpEncoderSpeed");

                        if (!string.IsNullOrEmpty(speed))
                        {
                            if (double.TryParse(speed, out double speedInMmPerSec))
                            {
                                // Convert mm/sec → m/min
                                double speedInMeterPerMinute = speedInMmPerSec * 0.06;

                                Dispatcher.Invoke(() =>
                                {
                                    LblSpeed1.Text = speedInMeterPerMinute.ToString("F2") + " m/min";
                                });
                            }
                        }
                    }
                    catch
                    {
                        // optional logging
                    }

                    await Task.Delay(500);
                }

            }, token);
        }

        private void StopSpeedLive()
        {
            _isSpeedRunning = false;

            if (_speedTimer != null)
            {
                _speedTimer.Stop();
                _speedTimer = null;
            }
        }



        /*
 * This is called when the disconnect button is pressed.
 */
        private void disconnectButton1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Are you sure you want to Disconnect ?",
                    "Confirm Stop",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                bool isDisconnected = m_client.disconnectFromServer();

                if (isDisconnected)
                {
                    _isConnected = false;

                    // 🔥 STOP BACKGROUND POLLING
                    _speedCancellationTokenSource?.Cancel();

                    MessageBox.Show("Disconnected Successfully");

                    statusText1.Text = "Not Connected";
                    statusText1.Foreground = System.Windows.Media.Brushes.Red;
                    ConnectionIndicator.Fill = System.Windows.Media.Brushes.Red;

                    LblConnectionTime.Visibility = Visibility.Collapsed;
                    LblConnectionTime.Text = "Connected since: --";

                    LblPrintingpageX1.Text = "Current value: 0.0";
                    LblPrintingpageY1.Text = "Current value: 0.0";

                    TxtPrintShift1.Text = "0.0";
                    TxtPrintShiftY1.Text = "0.0";

                    LblSpeed1.Text = "0";

                    LblPrintMode1.Text = "Current Print Mode : -";
                    CboPrintMode1.Items.Clear();

                    StopSpeedLive();
                }
                else
                {
                    MessageBox.Show("Disconnection Failed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }










        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }









        private bool trafficLightCallback(int p_iState)
        {
            return true;
        }








        private void disconnectButton2_Click(object sender, RoutedEventArgs e)
        {
            m_client.disconnectFromServer();
        }

      
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            m_StopThread.Start();
        }

        /*
         * This is called when the window is closed.
         */
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_DialogClosed = true;

            Abort();
        }

        #endregion


        #region Messages

       
        private bool AddMessageToLog(string logMessage, LogType logType)
        {
            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::AddMessageToLog() : Not adding the log message as the dialog has been closed.");

                return true;
            }

            // Format the message into the full message.
            string time = DateTime.Now.ToString("T");
            string logTypeMessage = "";
            if (logType == LogType.LT_SEND)
            {
                logTypeMessage = "-->";
            }
            else if (logType == LogType.LT_RECEIVE)
            {
                logTypeMessage = "<--";
            }
            else if (logType == LogType.LT_SEND_ERROR)
            {
                logTypeMessage = "Send Error -";
            }
            else if (logType == LogType.LT_INFO)
            {
                logTypeMessage = "Info -";
            }
            else if (logType == LogType.LT_WARNING)
            {
                logTypeMessage = "Warning -";
            }
            else if (logType == LogType.LT_ERROR)
            {
                logTypeMessage = "Error -";
            }
            string fullMessage = time + ": " + logTypeMessage + " " + logMessage;

            if (!AddLogMessageToList(this, fullMessage))
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::AddMessageToLog() : Failed to add the log message to the list.");

                return false;
            }

            return true;
        }

        #endregion


        #region Client

        /*
	     * This updates the client log settings with the current values of the dialog controls.
	     * \return Whether the log settings were updated successfully or not.
	     */
        private bool UpdateLogSettings()
        {
            if (!m_DialogInitialised)
            {
                return true;
            }

            bool logToFileEnabled = false;
            //if (logClientCheckBox.IsChecked == true)
            //{
            //    logToFileEnabled = true;
            //}
            //if (!m_client.UpdateLogSettings(logToFileEnabled, logFileTextBox.Text, true))
            //{
            //    return false;
            //}

            return true;
        }


        /*
         * This is the thread which connects to the server.
         * \param isStopped - The pointer to the function which indicates whether the thread should stop or not.
         * \param threadParameters - The parameters for the thread.
         * \return Whether the thread executed successfully or not.
         */
        private bool ConnectToServer(WorkerThread.isStoppedDelegate isStopped, ThreadParameters threadParameters)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::ConnectToServer()");

            bool result = true;

            // Cast the parameters to the ConnectToServerThreadParameters type.
            if (result && !isStopped())
            {
                if (threadParameters == null)
                {
                    m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::ConnectToServer() : The thread parameters are null.");

                    result = false;
                }
            }
            ConnectToServerThreadParameters connectToServerThreadParameters = null;
            if (result && !isStopped())
            {
                connectToServerThreadParameters = (ConnectToServerThreadParameters)threadParameters;
                if (connectToServerThreadParameters == null)
                {
                    m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::ConnectToServer() : The connect to server thread parameters are null.");

                    result = false;
                }
            }

            // Connect to the server.
            if (result && !isStopped())
            {
                if (!m_client.ConnectToServer(0, true, true, true))
                {
                    m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::ConnectToServer() : Failed to connect to the server.");

                    result = false;
                }
            }

            // Registerfor print server information, to make sure all log messages come back.
            if (result && !isStopped())
            {
                if (!m_client.RegisterForPrintServerInformation(true))
                {
                    m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::ConnectToServer() : Failed to register for print server information.");

                    result = false;
                }
            }

            

            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::ConnectToServer() : Connection finished.");

            return result;
        }
        /*
         * This class holds the partameters which need to be passed to the connect to server thread.
         */
        private class ConnectToServerThreadParameters : ThreadParameters
        {
            public string m_Address { get; set; }
            public ushort m_Port { get; set; }

            public ConnectToServerThreadParameters(string address, ushort port)
            {
                m_Address = address;
                m_Port = port;
            }
        }
        /*
         * This is the thread which disconnects from the server.
         * \param isStopped - The pointer to the function which indicates whether the thread should stop or not.
         * \param threadParameters - The parameters for the thread.
         * \return Whether the thread executed successfully or not.
         */
        private bool DisconnectFromServer(WorkerThread.isStoppedDelegate isStopped, ThreadParameters threadParameters)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::DisconnectFromServer()");

            bool result = true;

            m_DisconnectingFromServer = true;

            // Stop the send commands thread.

            // Comment by Rahul

            //if (!StopSendCommandsThread())
            //{
            //    m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::DisconnectFromServer() : Failed to stop the send commands thread.");

            //    result = false;
            //}

            // Disconnect from the server.
            if (!m_client.DisconnectFromServer())
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::DisconnectFromServer() : Failed to disconnect from the server.");

                result = false;
            }

            if (!SetConnectionStatus(this, "Not Connected"))
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::DisconnectFromServer() : Failed to set the connection status.");

                result = false;
            }

            m_DisconnectingFromServer = false;

            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::DisconnectFromServer() : Disconnection finished.");

            return result;
        }

        /*
         * This is called when the client has fully successfully connected to the server.
         * \return Whether the operations were performed successfully or not.
         */
        private bool ConnectedToServer()
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::ConnectedToServer()");

            // Set the connection status.
            if (!SetConnectionStatus(this, "Connected"))
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::ConnectedToServer() : Failed to set the connection status.");

                return false;
            }

            return true;
        }
        /*
         * This is called when the client has fully successfully disconnected to the server.
         * \return Whether the operations were performed successfully or not.
         */
        private bool DisconnectedFromServer()
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::DisconnectedFromServer()");

            bool result = true;

            // Stop the send commands thread. This is necessary because this function will be called if the server initiates the disconnection,
            // in which case the DisconnectFromServer() function will not be called.
            //if (!StopSendCommandsThread())
            //{
            //    m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::DisconnectedFromServer() : Failed to stop the send commands thread.");

            //    result = false;
            //}

            // Set the connection status.
            if (!SetConnectionStatus(this, "Not Connected"))
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::DisconnectedFromServer() : Failed to set the connection status.");

                result = false;
            }

            return result;
        }

        /*
         * This is the thread which stops the server.
         * \param isStopped - The pointer to the function which indicates whether the thread should stop or not.
         * \param threadParameters - The parameters for the thread.
         * \return Whether the thread executed successfully or not.
         */
        private bool Stop(WorkerThread.isStoppedDelegate isStopped, ThreadParameters threadParameters)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::Stop()");

            bool result = true;

            // Remove all the commands which are due to be sent.

            // Comment by Rahul 

            //if (!RemoveAllCommandsFromList(this))
            //{
            //    m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::Stop() : Failed to remove all of the commands from the list.");

            //    result = false;
            //}

            // Abort the engines in the server.
            if (!m_client.AbortRenderEngine(true))
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::Stop() : Failed to abort the Render Engine.");

                // Do not fail as the render engine may not exist.
            }
            if (!m_client.AbortPrintController(true))
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::Stop() : Failed to abort the Print Controller.");

                // Do not fail as the print controller may not exist.
            }

            // Stop the connect and disconnect threads.
            if (!m_ConnectToServerThread.Stop())
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::Stop() : Failed to stop the connect to server thread.");

                result = false;
            }
            if (!m_DisconnectFromServerThread.Stop())
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::Stop() : Failed to abort the disconnect from server thread.");

                result = false;
            }

            return result;
        }
        /*
         * This aborts the client.
         * \return Whether the abort completed successfully or not.
         */
        private bool Abort()
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::Abort()");

            bool result = false;

            // Stop the client.
            if (!m_StopThread.Run())
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::Abort() : Failed to rub the stop thread.");

                result = false;
            }

            // Disconnect from the server.
            if (!m_DisconnectFromServerThread.Run())
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::Abort() : Failed to run the disconnect from server thread.");

                result = false;
            }

            // Abort the TCP/IP client.
            if (!m_client.Abort())
            {
                result = false;
            }

            return result;
        }

        #endregion


        #region Interface Access

        /*
	     * This gets the information about the MainWindow object given.
         * \param window - The window to retrieve the information about.
         * \param windowInfo - This object is populated with the information.
         * \return Whether the information was retrieved successfully or not.
	     */
        public static bool GetWindowInfo(MainWindow window, ref MainWindowInfo windowInfo)
        {
            if (!window.m_DialogInitialised)
            {
                return false;
            }
            if (window.m_DialogClosed)
            {
                return false;
            }

            // Make sure the information is only retireved by the main thread.
            if (window.Dispatcher.CheckAccess())
            {
                //GetWindowInfoCall(window, windowInfo);
            }
            else
            {
                // window.Dispatcher.Invoke(DispatcherPriority.Normal, new QueryWindowInfoDelegate(GetWindowInfoCall), window, windowInfo);
            }

            return true;
        }
        /*
         * The delegate which wraps the function for retreiving information about the main window.
         */
        private delegate bool QueryWindowInfoDelegate(MainWindow window, MainWindowInfo windowInfo);
        /*
	     * This gets the information about the MainWindow object given.
         * This can only be called by the main thread.
         * \param window - The window to retrieve the information about.
         * \param windowInfo - This object is populated with the information.
         * \return Whether the information was retrieved successfully or not.
	     */
        //private static bool GetWindowInfoCall(MainWindow window, MainWindowInfo windowInfo)
        //{
        //    //windowInfo.logClient = window.logClientCheckBox.IsChecked.Value;
        //    //windowInfo.logFile = window.logFileTextBox.Text;
        //    windowInfo.address = window.addressTextBox.Text;
        //    windowInfo.port = window.portTextBox.Text;

        //    // Comment by Rahul

        //    //windowInfo.command = window.commandTextBox.Text;
        //    //windowInfo.commandsFile = window.commandsFileTextBox.Text;
        //    //windowInfo.repeatQueue = window.repeatCommandQueueCheckBox.IsChecked.Value;

        //    return true;
        //}

        /*
	     * This sets the connection status in the window specified.
         * \param window - The window to set the status in.
         * \param connectionStatus - The new connection status.
         * \return Whether the connection status was set successfully or not.
	     */
        public static bool SetConnectionStatus(MainWindow window, string connectionStatus)
        {
            if (!window.m_DialogInitialised)
            {
                return false;
            }
            if (window.m_DialogClosed)
            {
                return false;
            }

            // Make sure the connection status is only set by the main thread.
            if (window.Dispatcher.CheckAccess())
            {
                //if (!SetConnectionStatusCall(window, connectionStatus))
                //{
                //    return false;
                //}
            }
            else
            {
                // window.Dispatcher.Invoke(DispatcherPriority.Normal, new SetConnectionStatusDelegate(SetConnectionStatusCall), window, connectionStatus);
            }

            return true;
        }
        /*
         * The delegate which wraps the function for setting the connection status.
         */
        private delegate bool SetConnectionStatusDelegate(MainWindow window, string connectionStatus);
        /*
	     * This sets the connection status in the window specified.
         * This can only be called by the main thread.
         * \param window - The window to set the status in.
         * \param connectionStatus - The new connection status.
         * \return Whether the connection status was set successfully or not.
	     */
        //private static bool SetConnectionStatusCall(MainWindow window, string connectionStatus)
        //{
        //    window.statusText.Text = connectionStatus;

        //    return true;
        //}

        /*
	     * This adds a command to the list of commands to send to the server.
         * \param window - The window to add ther command to.
         * \param command - The command to add.
         * \return Whether the command was added set successfully or not.
	     */

        // Comment by Rahul

        //public static bool AddCommandToList(MainWindow window, string command)
        //{
        //    if (!window.m_DialogInitialised)
        //    {
        //        return false;
        //    }
        //    if (window.m_DialogClosed)
        //    {
        //        return false;
        //    }

        //    // Make sure the command is only added by the main thread.
        //    if (window.Dispatcher.CheckAccess())
        //    {
        //        if (!AddCommandToListCall(window, command))
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        window.Dispatcher.Invoke(DispatcherPriority.Normal, new AddCommandToListDelegate(AddCommandToListCall), window, command);
        //    }

        //    return true;
        //}
        /*
         * The delegate which wraps the function for adding a command to send.
         */
        private delegate bool AddCommandToListDelegate(MainWindow window, string command);
        /*
	     * This adds a command to the list of commands to send to the server.
         * This can only be called by the main thread.
         * \param window - The window to add ther command to.
         * \param command - The command to add.
         * \return Whether the command was added set successfully or not.
	     */
        private static bool AddCommandToListCall(MainWindow window, string command)
        {
            //window.m_CommandsListLock.WaitOne();

            //window.commandsListBox.Items.Add(command);

            //window.m_CommandsListNotEmptyEvent.Set();

            //window.m_CommandsListLock.ReleaseMutex();

            return true;
        }

        /*
	     * This gets the next command from the list of commandsto send to the server.
         * \param window - The window to add ther command to.
         * \param command - The next command is returned in this parameter.
         * \return Whether the next command was retrieved successfully or not.
	     */


        // Comment by Rahul

        //public static bool GetNextCommandFromList(MainWindow window, ref string command)
        //{
        //    if (!window.m_DialogInitialised)
        //    {
        //        return false;
        //    }
        //    if (window.m_DialogClosed)
        //    {
        //        return false;
        //    }

        //    NextCommand nextCommand = new NextCommand();

        //    // Make sure the command is only retrieved by the main thread.
        //    if (window.Dispatcher.CheckAccess())
        //    {
        //        if (!GetNextCommandCall(window, nextCommand))
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        window.Dispatcher.Invoke(DispatcherPriority.Normal, new GetNextCommandDelegate(GetNextCommandCall), window, nextCommand);
        //    }

        //    command = nextCommand.command;

        //    return true;
        //}
        /*
         * The delegate which wraps the function for getting the next command to send.
         */
        //private delegate bool GetNextCommandDelegate(MainWindow window, NextCommand nextCommand);
        /*
	     * This gets the next command from the list of commands to send to the server.
         * This can only be called by the main thread.
         * \param window - The window to add the command to.
         * \param command - The next command is returned in this parameter.
         * \return Whether the next command was retrieved successfully or not.
	     */
        //private static bool GetNextCommandCall(MainWindow window, NextCommand nextCommand)
        //{
        //    window.m_CommandsListLock.WaitOne();

        //    if (window.commandsListBox.Items.Count == 0)
        //    {
        //        window.m_CommandsListNotEmptyEvent.Reset();
        //    }
        //    else
        //    {
        //        nextCommand.command = window.commandsListBox.Items.GetItemAt(0).ToString();
        //        window.commandsListBox.Items.RemoveAt(0);

        //        if (window.commandsListBox.Items.Count == 0)
        //        {
        //            window.m_CommandsListNotEmptyEvent.Reset();
        //        }
        //    }

        //    window.m_CommandsListLock.ReleaseMutex();

        //    return true;
        //}

        /*
	     * This removes all of the commands from the list of commands to send to the server.
         * \param window - The window to remove the commands from.
         * \return Whether the command were removed successfully or not.
	     */


        // Comment by Rahul


        //public static bool RemoveAllCommandsFromList(MainWindow window)
        //{
        //    if (!window.m_DialogInitialised)
        //    {
        //        return false;
        //    }
        //    if (window.m_DialogClosed)
        //    {
        //        return false;
        //    }

        //    // Make sure the command is only removed by the main thread.
        //    if (window.Dispatcher.CheckAccess())
        //    {
        //        if (!RemoveAllCommandsFromListCall(window))
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        window.Dispatcher.Invoke(DispatcherPriority.Normal, new RemoveAllCommandsFromListDelegate(RemoveAllCommandsFromListCall), window);
        //    }

        //    return true;
        //}
        /*
         * The delegate which wraps the function for removing all commands from the commands to send list.
         */
        private delegate bool RemoveAllCommandsFromListDelegate(MainWindow window);
        /*
	     * This removes all of the commands from the list of commands to send to the server.
         * This can only be called by the main thread.
         * \param window - The window to remove the commands from.
         * \return Whether the command were removed successfully or not.
	     */
        //private static bool RemoveAllCommandsFromListCall(MainWindow window)
        //{
        //    window.m_CommandsListLock.WaitOne();

        //    window.commandsListBox.Items.Clear();

        //    window.m_CommandsListNotEmptyEvent.Reset();

        //    window.m_CommandsListLock.ReleaseMutex();

        //    return true;
        //}

        /*
	     * This adds a message to the log list.
         * \param window - The window to add the message to.
         * \param logMessage - The message to add to the log
         * \return Whether the message was added successfully or not.
	     */
        public static bool AddLogMessageToList(MainWindow window, string logMessage)
        {
            if (!window.m_DialogInitialised)
            {
                return false;
            }
            if (window.m_DialogClosed)
            {
                return false;
            }

            // Make sure the message is only added by the main thread.
            if (window.Dispatcher.CheckAccess())
            {
                //if (!AddLogMessageToListCall(window, logMessage))
                //{
                //    return false;
                //}
            }
            else
            {
                // window.Dispatcher.Invoke(DispatcherPriority.Normal, new AddLogMessageToListDelegate(AddLogMessageToListCall), window, logMessage);
            }

            return true;
        }
        /*
         * The delegate which wraps the function for adding a message to the log list.
         */
        private delegate bool AddLogMessageToListDelegate(MainWindow window, string logMessage);
        /*
	     * This adds a message to the log list.
         * This can only be called by the main thread.
         * \param window - The window to add the message to.
         * \param logMessage - The message to add to the log
         * \return Whether the message was added successfully or not.
	     */

        // Comment by Rahul

        //private static bool AddLogMessageToListCall(MainWindow window, string logMessage)
        //{
        //    window.messageListBox.Items.Insert(0, logMessage);

        //    return true;
        //}


        //private static bool AddLogMessageToListCall(MainWindow window, string logMessage)
        //{
        //    window.messageListBox.Items.Insert(0, logMessage);
        //    window.messageListBox.ScrollIntoView(window.messageListBox.Items[0]);
        //    return true;
        //}

        #endregion


        #region Callbacks

        /*
         * This is called when the TCP/IP client performs the connected callback.
         * Whether the callback executed successfully or not.
         */
        private void ConnectedCallback(object sender, Client.ClientEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::ConnectedCallback()");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::ConnectedCallback() : The dialog has been closed.");
            }

            if (!ConnectedToServer())
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::ConnectedCallback() : Failed to signal that the client has connected to the server.");
                e.OK = false;
                return;
            }
        }
        /*
         * This is called when the TCP/IP client performs the disconnected callback.
         * Whether the callback executed successfully or not.
         */
        private void DisconnectCallback(object sender, Client.ClientEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::DisconnectCallback()");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::ConnectedCallback() : The dialog has been closed.");
            }

            // Signal that the server has disconnected. This is not necessary if this dialog initiated the disconnection because
            // it will already know about this and will have handled it appropriately.
            if (!m_DisconnectingFromServer)
            {
                if (!DisconnectedFromServer())
                {
                    m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::DisconnectedFromServer() : Failed to signal that the client has disconnected from the server.");
                    e.OK = false;
                    return;
                }
            }
        }
        /*
         * This is called when the TCP/IP client performs the message sent callback.
         * \param message -  The message sent.
         * Whether the callback executed successfully or not.
         */
        private void MessageSentCallback(object sender, Client.MessageEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::MessageSentCallback() : Message \"" + e.Message + "\" sent.");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::MessageSentCallback() : The dialog has been closed.");
                return;
            }

            // Add the message to the log.
            if (!AddMessageToLog(e.Message, LogType.LT_SEND))
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::MessageSentCallback() : Failed to add the message to the log.");
                e.OK = false;
                return;
            }
        }
        /*
         * This is called when the TCP/IP client performs the message retrieved callback.
         * \param message -  The message recieved.
         * Whether the callback executed successfully or not.
         */
        private void MessageReceivedCallback(object sender, Client.MessageEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::MessageReceivedCallback() : Message \"" + e.Message + "\" received.");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::MessageReceivedCallback() : The dialog has been closed.");
                return;
            }

            // Add the message to the log.
            if (!AddMessageToLog(e.Message, LogType.LT_RECEIVE))
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::MessageReceivedCallback() : Failed to add the message to the log.");
                e.OK = false;
                return;
            }
        }
        /*
         * This is called when the TCP/IP client performs the status recieved callback.
         * \param engineCode -  The engine code.
         * \param status - The status.
         * \param statusCode- The status code.
         * Whether the callback executed successfully or not.
         */
        private void StatusCallback(object sender, Client.StatusEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::StatusCallback() : Engine code = \"" + e.EngineCode + "\", status = \"" + e.Message + "\", status code = " + e.StatusCode.ToString() + ".");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::StatusCallback() : The dialog has been closed.");
            }
        }
        /*
         * This is called when the TCP/IP client performs the traffic light recieved callback.
         * \param state - The traffic light state.
         * Whether the callback executed successfully or not.
         */
        private void TrafficLightCallback(object sender, Client.TrafficLightEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::TrafficLightCallback() : Traffic light state is " + e.State.ToString() + ".");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::TrafficLightCallback() : The dialog has been closed.");
            }
        }
        /*
         * This is called when the TCP/IP client performs the swathe processed recieved callback.
         * \param swatheNumber - The swathe number.
         * Whether the callback executed successfully or not.
         */
        private void SwatheProcessedCallback(object sender, Client.SwatheProcessedEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::SwatheProcessedCallback() : Swathe " + e.Swathe.ToString() + " has been processed.");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::SwatheProcessedCallback() : The dialog has been closed.");
            }
        }
        /*
         * This is called when the TCP/IP client performs the label number callback.
         * \param labelNumber - The label number.
         * Whether the callback executed successfully or not.
         */
        private void LabelNumberCallback(object sender, Client.LabelNumberEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::LabelNumberCallback() : Label number is " + e.LabelNumber.ToString() + ".");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::LabelNumberCallback() : The dialog has been closed.");
            }
        }
        /*
         * This is called when the TCP/IP client performs the position callback.
         * \param x - The X position.
         * \param y - The Y position.
         * \param z - The Z position.
         * Whether the callback executed successfully or not.
         */
        private void PositionCallback(object sender, Client.PositionEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::PositionCallback() : X = " + e.Position.X.ToString() + ", Y = " + e.Position.Y.ToString() + ", Z = " + e.Position.Z.ToString() + ".");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::PositionCallback() : The dialog has been closed.");
            }
        }
        /*
         * This is called when the TCP/IP client performs the ink system parameter value callback.
         * \param name - The parameter name.
         * \param value - The parameter value.
         * Whether the callback executed successfully or not.
         */
        private void InkSystemParameterValueCallback(object sender, Client.InkSystemParameterValueEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::InkSystemParameterValueCallback() : Parameter \"" + e.ParameterName + "\" value is \"" + e.ParameterValue + "\".");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::InkSystemParameterValueCallback() : The dialog has been closed.");
            }
        }
        /*
         * This is called when the TCP/IP client performs the printhead status callback.
         * \param printheadStatusInformation - The printhead status information.
         * Whether the callback executed successfully or not.
         */
        private void PrintHeadStatusCallback(object sender, Client.ClientEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::PrintHeadStatusCallback()");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::PrintHeadStatusCallback() : The dialog has been closed.");
            }
        }
        /*
         * This is called when the TCP/IP client performs the print started callback.
         * Whether the callback executed successfully or not.
         */
        private void PrintStartedCallback(object sender, Client.ClientEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::PrintStartedCallback()");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::PrintStartedCallback() : The dialog has been closed.");
            }

            m_ReadyToPrintReceivedEvent.Reset();
        }
        /*
         * This is called when the TCP/IP client performs the ready to print callback.
         * \param numberOfSwathes - The number of sweathes in the print.
         * Whether the callback executed successfully or not.
         */
        private void ReadyToPrintCallback(object sender, Client.ReadyToPrintEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::ReadyToPrintCallback() : Ready to print " + e.SwathesToPrint.ToString() + " swathes.");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::ReadyToPrintCallback() : The dialog has been closed.");
            }

            m_ReadyToPrintReceivedEvent.Set();
        }
        /*
         * This is called when the TCP/IP client performs the end of printing callback.
         * Whether the callback executed successfully or not.
         */
        private void EndOfPrintingCallback(object sender, Client.ClientEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::EndOfPrintingCallback()");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::EndOfPrintingCallback() : The dialog has been closed.");
            }
        }
        /*
         * This is called when the TCP/IP client performs the print server reporter callback.
         * \param message - the print server reporter message.
         * Whether the callback executed successfully or not.
         */
        private void PrintServerReporterCallback(object sender, Client.MessageEventArgs e)
        {
            m_client.Log(m_client.GetLowLogLevel(), "MainWindow::PrintReporterCallback()");

            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::PrintReporterCallback() : The dialog has been closed.");
            }
        }
        /*
         * This is called when the TCP/IP client performs the interface log callback.
         * \param loggingLevel - The logging level.
         * \param logMessage - The message to log.
         * Whether the callback executed successfully or not.
         */
        private void InterfaceLogCallback(object sender, Client.InterfaceLogEventArgs e)
        {
            if (m_DialogClosed)
            {
                m_client.Log(m_client.GetLowLogLevel(), "MainWindow::InterfaceLogCallback() : The dialog has been closed.");
                return;
            }

            LogType logType = LogType.LT_ERROR;
            if (e.LogLevel == m_client.GetUIInfoLogLevel())
            {
                logType = LogType.LT_INFO;
            }
            else if (e.LogLevel == m_client.GetUIWarningLogLevel())
            {
                logType = LogType.LT_WARNING;
            }
            else if (e.LogLevel == m_client.GetUIErrorLogLevel())
            {
                logType = LogType.LT_ERROR;
            }
            else
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::InterfaceLogCallback() : The logging level " + e.LogLevel.ToString() + " is invalid.");
                e.OK = false;
                return;
            }

            if (!AddMessageToLog(e.Message, logType))
            {
                m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::InterfaceLogCallback() : Failed to add a message to the log.");
                e.OK = false;
                return;
            }
        }

        #endregion


        #region Registry Functions

        /*
	     * This reads the value to the given registry key.
         * \param key - The registry key to read from.
         * \param value - The value in the registry is returned in this parameter.
         * \return Whether the read was successful or not.
	     */
        private bool readRegistrySetting(string key, ref string value)
        {
            value = "";

            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("Global Inkjet Systems").OpenSubKey("Inkjet OS 1.5");
                if (registryKey == null)
                {
                    if (m_DialogInitialised)
                    {
                        m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::readRegistrySetting() : Failed to read registry key \"" + key + "\" as the key is null.");
                    }

                    return false;
                }
                else
                {

                    object valueKey = registryKey.GetValue(key);
                    if (valueKey == null)
                    {
                        if (m_DialogInitialised)
                        {
                            m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::readRegistrySetting() : The value key for registry key \"" + key + "\" s null.");
                        }

                        return false;
                    }
                    else
                    {
                        value = valueKey.ToString();
                    }

                }
            }
            catch (Exception exception)
            {
                if (m_DialogInitialised)
                {
                    m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::readRegistrySetting() : Failed to read registry key \"" + key + "\" with exception " + exception.ToString() + "\".");
                }

                return false;
            }

            return true;
        }
        /*
	     * This writes the given value to the registry key specified.
         * \param key - The registry key to write to.
         * \param value - The value to write to the registry.
         * \return Whether the write was successful or not.
	     */
        private bool writeRegistrySetting(string key, string value)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE").CreateSubKey("Global Inkjet Systems").CreateSubKey("Inkjet OS 1.5");
                if (registryKey == null)
                {
                    if (m_DialogInitialised)
                    {
                        m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::writeRegistrySetting() : Failed to write registry key \"" + key + "\" as the key is null.");
                    }

                    return false;
                }
                else
                {

                    registryKey.SetValue(key, value);

                }
            }
            catch (Exception exception)
            {
                if (m_DialogInitialised)
                {
                    m_client.Log(m_client.GetErrorLogLevel(), "MainWindow::writeRegistrySetting() : Failed to write registry key \"" + key + "\" with exception " + exception.ToString() + "\".");
                }

                return false;
            }

            return true;
        }






        // All Methods is Working fine but I do comment for the Matrix.net.dll is required.



        //private void BrowseButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
        //    folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;

        //    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        string fName = folderDialog.SelectedPath;

        //        // Set selected path in TextBox
        //        HotFolderTextBox1.Text = fName;

        //        // Clear DataGrid
        //        DataGrid1.Items.Clear();

        //        DirectoryInfo di = new DirectoryInfo(fName);
        //        FileInfo[] aryFi = di.GetFiles("*.txt");

        //        foreach (FileInfo fi in aryFi)
        //        {
        //            lblinfo.Visibility = Visibility.Visible;
        //            lblinfo.Text = "Wait: Adding csv/text data file...";

        //            int csvrecordcounter = File.ReadAllLines(fi.FullName).Length;

        //            lblinfo.Text = "Info: Ready";

        //            string doneFilePath = Path.Combine(
        //                fi.DirectoryName,
        //                "done",
        //                Path.GetFileNameWithoutExtension(fi.Name) + "_Printed.csv"
        //            );

        //            if (File.Exists(doneFilePath))
        //            {
        //                var result = System.Windows.Forms.MessageBox.Show(
        //                    $"File: {fi.Name} already printed. Add again?",
        //                    "JETSCI Warning",
        //                    MessageBoxButtons.YesNo);

        //                if (result == System.Windows.Forms.DialogResult.Yes)
        //                {
        //                    DataGrid1.Items.Add(new
        //                    {
        //                        File_name = fi.Name,
        //                        Print_Status = "In Queue",
        //                        Total_records = csvrecordcounter,
        //                        Start_No = 0,
        //                        Labels_to_be_printed = csvrecordcounter,
        //                        No_Of_Copies = 1
        //                    });
        //                }
        //            }
        //            else
        //            {
        //                DataGrid1.Items.Add(new
        //                {
        //                    File_name = fi.Name,
        //                    Print_Status = "In Queue",
        //                    Total_records = csvrecordcounter,
        //                    Start_No = 0,
        //                    Labels_to_be_printed = csvrecordcounter,
        //                    No_Of_Copies = 1
        //                });
        //            }
        //        }
        //    }
        //}





        //private void ConvertToImage(string pclfile)
        //{
        //    if (string.IsNullOrWhiteSpace(TxtresX.Text) ||
        //        string.IsNullOrWhiteSpace(TxtResY.Text))
        //        return;

        //    string tmppclfile = System.IO.Path.GetFileNameWithoutExtension(pclfile);

        //    string exePath = @"C:\Program Files (x86)\VeryPDF PCL Converter v2.7\pcltool.exe";

        //    if (!File.Exists(exePath))
        //    {
        //        System.Windows.MessageBox.Show("PCL Converter not found.");
        //        return;
        //    }

        //    string arguments =
        //        $" -xres {TxtresX.Text} -yres {TxtResY.Text} " +
        //        $"\"{System.IO.Path.Combine(hotFolderTextBox.Text, pclfile)}\" " +
        //        $"\"{System.IO.Path.Combine(hotFolderTextBox.Text, tmppclfile)}_%06d.bmp\"";

        //    var processInfo = new System.Diagnostics.ProcessStartInfo
        //    {
        //        FileName = exePath,
        //        Arguments = arguments,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    System.Diagnostics.Process.Start(processInfo);
        //}

        private void BtnSpit1_Click(object sender, RoutedEventArgs e)
        {
            string sMessage;

            // Use correct XAML control names
            sMessage = "P,G,T," + Txtfre1.Text + "," + Txtspit1.Text;

            m_client.sendMessageToServer(sMessage);

            sMessage = "P,GW,F";

            m_client.sendMessageToServer(sMessage);
        }














        //private void ApplyButton1_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!BackgroundWorker6.IsBusy)
        //    {
        //        BackgroundWorker6.RunWorkerAsync();
        //    }
        //}


        private void ApplyButton1_Click(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker7.IsBusy)
            {
                // UI thread se selected value le lo
                string selectedMode = CboPrintMode1.Text;

                backgroundWorker7.RunWorkerAsync(selectedMode);
            }
        }



        private void ApplyButton2_Click(object sender, RoutedEventArgs e)
        {
            if (!BackgroundWorker6.IsBusy)
            {
                BackgroundWorker6.RunWorkerAsync();
            }
        }


        private void BackgroundWorker7_DoWork(object sender, DoWorkEventArgs e)
        {
            string selectedMode = e.Argument.ToString();

            string sMessage;

            sMessage = "S,T,M," + selectedMode;
            m_client.sendMessageToServer(sMessage);

            sMessage = "R,T,M," + selectedMode;
            m_client.sendMessageToServer(sMessage);

            sMessage = "P,T,M," + selectedMode;
            m_client.sendMessageToServer(sMessage);

            // UI update yaha nahi karenge (background thread hai)
        }
        private void BackgroundWorker7_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // UI safe zone

            string selectedMode = CboPrintMode1.Text;

            LblPrintMode1.Text = "Current Print Mode : " + selectedMode;

            System.Windows.MessageBox.Show("Print mode has been changed successfully");
        }

        //private void timer_Tick(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // Update connection status
        //        statusText.Text = m_sConnectionStatus;

        //        if (m_sMessageLog != null && m_sMessageLog.Length >= 3)
        //        {
        //            for (int iDataItem = m_sMessageLog.Length - 3;
        //                 iDataItem < m_sMessageLog.Length;
        //                 iDataItem++)
        //            {
        //                if (m_sMessageLog[iDataItem].Contains(",L,") &&
        //                    m_sMessageLog[iDataItem].Contains("I,") &&
        //                    !m_sMessageLog[iDataItem].Contains(",L,0"))
        //                {
        //                    recordarr = m_sMessageLog[iDataItem].Split(',');

        //                    if (recordarr.Length > 3)
        //                    {
        //                        if (recordarr[3] != tmprecordno)
        //                        {
        //                            // WPF DataGrid Selected Row
        //                            if (DataGrid1.SelectedItem != null)
        //                            {
        //                                var row = DataGrid1.SelectedItem;

        //                                // If bound to DataTable
        //                                var dataRow = row as System.Data.DataRowView;

        //                                if (dataRow != null)
        //                                {
        //                                    tmpnumrecno =
        //                                        Convert.ToInt32(recordarr[3]) +
        //                                        Convert.ToInt32(dataRow.Row[3]);

        //                                    LblPrintingpage.Text = tmpnumrecno.ToString();
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return;
        //    }
        //}

        //private void StartAsyncButton_Click(object sender, RoutedEventArgs e)
        //{
        //    Lblcopycnt.Text = "";
        //    tmprecordno = "";

        //    if (!System.IO.Directory.Exists(@"c:\print_data\"))
        //    {
        //        System.IO.Directory.CreateDirectory(@"c:\print_data\");
        //    }

        //    if (!BackgroundWorker1.IsBusy)
        //    {
        //        BackgroundWorker1.RunWorkerAsync();
        //    }
        //}

        //private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    foreach (var item in DataGrid1.Items)
        //    {
        //        var row = item as System.Data.DataRowView;
        //        if (row == null) continue;

        //        string fileName = row.Row[0].ToString();
        //        string logfile = fName + @"\done\" +
        //                         System.IO.Path.GetFileNameWithoutExtension(fileName) +
        //                         "_Printed.csv";

        //        getpagecnt = "";
        //        totgetpagecnt = "";
        //        processfilenm = fileName;

        //        if (!Directory.Exists(fName + @"\done\"))
        //        {
        //            Directory.CreateDirectory(fName + @"\done\");
        //        }

        //        Addlblinfo("info: Preparing for log file. Wait...");

        //        if (!File.Exists(logfile))
        //        {
        //            if (ChkLogMethod.IsChecked == false)
        //            {
        //                File.Create(logfile).Dispose();
        //            }
        //            else
        //            {
        //                File.Create(logfile).Dispose();

        //                strlog = new string[Convert.ToInt32(row.Row[2]) + 1];

        //                for (int ij = 1; ij <= Convert.ToInt32(row.Row[2]); ij++)
        //                {
        //                    string tmpstrarr =
        //                        ij.ToString() + "," +
        //                        File.ReadAllLines(fName + @"\" + fileName)[ij - 1] + "0";

        //                    strlog[ij] = tmpstrarr;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (ChkLogMethod.IsChecked == true)
        //            {
        //                strlog = new string[Convert.ToInt32(row.Row[2]) + 1];

        //                for (int ij = 1; ij <= Convert.ToInt32(row.Row[2]); ij++)
        //                {
        //                    string tmpstrarr =
        //                        File.ReadAllLines(logfile)[ij - 1];

        //                    strlog[ij] = tmpstrarr;
        //                }

        //                File.WriteAllText(logfile, "");
        //            }

        //            string sFileContents = File.ReadAllText(logfile);

        //            if (sFileContents.Contains(row.Row[3].ToString()))
        //            {
        //                var result = System.Windows.MessageBox.Show(
        //                    "Label: " + row.Row[3].ToString() +
        //                    " already printed. Do you want to reprint???",
        //                    "JETSCI Warning",
        //                    MessageBoxButton.YesNo);

        //                if (result != MessageBoxResult.Yes)
        //                {
        //                    goto abortmission;
        //                }
        //            }
        //        }

        //        AddlblinfoB("Info: Prepared for Print log. Printing now");

        //        Dispatcher.Invoke(() =>
        //        {
        //            row.Row[1] = "Printing...";
        //        });

        //        Generateprint(
        //            row.Row[2].ToString(),
        //            row.Row[3].ToString(),
        //            (Convert.ToInt32(getpagecnt) *
        //             Convert.ToInt32(row.Row[5])).ToString(),
        //            fName + @"\" + fileName
        //        );

        //        Dispatcher.Invoke(() =>
        //        {
        //            row.Row[1] = "Printed";
        //        });

        //        if (ChkLogMethod.IsChecked == true)
        //        {
        //            using (StreamWriter sw = new StreamWriter(logfile, true))
        //            {
        //                for (int ij = 1; ij <= Convert.ToInt32(row.Row[2]); ij++)
        //                {
        //                    sw.WriteLine(strlog[ij]);
        //                }
        //            }
        //        }
        //    }

        //    Dispatcher.Invoke(() =>
        //    {
        //        Lblcopycnt.Foreground = System.Windows.Media.Brushes.Red;
        //        Lblcopycnt.Text = "Creating LOG wait....";
        //    });

        //abortmission:

        //    sMessage = "R,A";
        //    m_client.sendMessageToServer(sMessage);

        //    sMessage = "P,A";
        //    m_client.sendMessageToServer(sMessage);

        //    createloginlast();

        //    Dispatcher.Invoke(() =>
        //    {
        //        Lblcopycnt.Foreground = System.Windows.Media.Brushes.Green;
        //        Lblcopycnt.Text = "Log has been created";
        //    });

        //    AddlblinfoB("Info: Printing Stopped");
        //}

        //private void AddlblinfoB(string sinB)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        Timer1.IsEnabled = false;

        //        lblinfo.Visibility = Visibility.Visible;
        //        lblinfo.Foreground = System.Windows.Media.Brushes.Green;
        //        lblinfo.Text = sinB;
        //    });
        //}

        //private void Addlblinfo(string sin)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        lblinfo.Visibility = Visibility.Visible;
        //        lblinfo.Foreground = System.Windows.Media.Brushes.Blue;
        //        lblinfo.Text = sin;
        //    });
        //}

        //private void createloginlast()
        //{
        //    try
        //    {
        //        if (m_sMessageLog != null && m_sMessageLog.Length > 0)
        //        {
        //            if (DataGrid1.SelectedItem == null)
        //                return;

        //            var row = DataGrid1.SelectedItem as System.Data.DataRowView;
        //            if (row == null)
        //                return;

        //            string fileName = row.Row[0].ToString();

        //            string printedFile =
        //                fName + @"\done\" +
        //                System.IO.Path.GetFileNameWithoutExtension(fileName) +
        //                "_Printed.csv";

        //            using (StreamWriter sw = new StreamWriter(printedFile, true))
        //            {
        //                for (int iDataItem = 0; iDataItem < m_sMessageLog.Length; iDataItem++)
        //                {
        //                    if (m_sMessageLog[iDataItem].Contains(",L,") &&
        //                        m_sMessageLog[iDataItem].Contains("I,") &&
        //                        !m_sMessageLog[iDataItem].Contains(",L,0"))
        //                    {
        //                        recordarr = m_sMessageLog[iDataItem].Split(',');

        //                        if (recordarr.Length > 3)
        //                        {
        //                            if (recordarr[3] != tmprecordno)
        //                            {
        //                                tmprecordno = recordarr[3];

        //                                addtofilerecno(
        //                                    fName + @"\" + fileName,
        //                                    Convert.ToDouble(row.Row[3]),
        //                                    Convert.ToDouble(row.Row[4]),
        //                                    printedFile,
        //                                    Convert.ToInt32(row.Row[5]),
        //                                    DataGrid1.SelectedIndex,
        //                                    Convert.ToDouble(recordarr[3])
        //                                );
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            // Clear message log (VB: ReDim m_sMessageLog(-1))
        //            m_sMessageLog = new string[0];
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Windows.MessageBox.Show(ex.ToString());
        //        return;
        //    }
        //}































        //public void Generateprint(string totnum, string startnum, string cntnum, string tmpfname)
        //{
        //    string tmpimgname = "";
        //    long i = 0;

        //    long tmpnum = Convert.ToInt64(startnum);
        //    long tmpcount = Convert.ToInt64(cntnum);
        //    long tmptotnum = Convert.ToInt64(totnum);

        //    try
        //    {
        //        using (StreamReader strReader =
        //            new StreamReader(tmpfname, System.Text.Encoding.GetEncoding(1252)))
        //        {
        //            sMessage = "P,Q,P";
        //            m_client.sendMessageToServer(sMessage);

        //            while (i <= tmptotnum)
        //            {
        //                if (i >= tmpnum && i < tmpnum + tmpcount)
        //                {
        //                    string aLine = strReader.ReadLine();
        //                    if (aLine == null)
        //                        break;

        //                    string fileWithoutExt =
        //                        Path.GetFileNameWithoutExtension(tmpfname);

        //                    tmpimgname = fileWithoutExt + "_" + i.ToString();

        //                    GenerateBarcode(aLine, tmpimgname);

        //                    printimagebycsv(tmpimgname + ".bmp");
        //                }

        //                i++;
        //            }
        //        }

        //        sMessage = "P,Q,E";
        //        m_client.sendMessageToServer(sMessage);

        //        sMessage = "R,A";
        //        m_client.sendMessageToServer(sMessage);

        //        sMessage = "P,A";
        //        m_client.sendMessageToServer(sMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Windows.MessageBox.Show(ex.Message);
        //    }
        //}

        //public void GenerateBarcode(string inputData, string nm)
        //{
        //    int width = 300;
        //    int height = 150;

        //    Bitmap bitmap = new Bitmap(width, height);

        //    using (Graphics g = Graphics.FromImage(bitmap))
        //    {
        //        g.Clear(Color.White);

        //        using (Font font = new Font("Arial", 20))
        //        {
        //            g.DrawString(inputData, font, Brushes.Black, new PointF(10, 50));
        //        }
        //    }

        //    Selectedrotation(0);

        //    Bitmap rotatedImage = RotateImg(bitmap, tmprotate);

        //    string folderPath = @"C:\print_data\";

        //    if (!Directory.Exists(folderPath))
        //        Directory.CreateDirectory(folderPath);

        //    rotatedImage.Save(folderPath + nm + ".bmp", ImageFormat.Bmp);

        //    bitmap.Dispose();
        //    rotatedImage.Dispose();
        //}

        public Bitmap RotateImg(Bitmap bmpimage, float angle)
        {
            int w = bmpimage.Width;
            int h = bmpimage.Height;

            System.Drawing.Imaging.PixelFormat pf = bmpimage.PixelFormat;

            Bitmap tempImg = new Bitmap(w, h, pf);
            Graphics g = Graphics.FromImage(tempImg);

            g.DrawImageUnscaled(bmpimage, 1, 1);
            g.Dispose();

            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(new RectangleF(0.0f, 0.0f, w, h));

            System.Drawing.Drawing2D.Matrix mtrx = new System.Drawing.Drawing2D.Matrix();
            mtrx.Rotate(angle);

            RectangleF rct = path.GetBounds(mtrx);

            Bitmap newImg = new Bitmap(
                Convert.ToInt32(rct.Width),
                Convert.ToInt32(rct.Height),
                pf);

            g = Graphics.FromImage(newImg);
            g.Clear(System.Drawing.Color.White);

            g.TranslateTransform(-rct.X, -rct.Y);
            g.RotateTransform(angle);

            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.DrawImageUnscaled(tempImg, 0, 0);

            g.Dispose();
            tempImg.Dispose();

            return newImg;
        }



        //private void Selectedrotation(int n)
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        if (Cborotate.SelectedItem != null)
        //        {
        //            tmprotate = Convert.ToSingle(Cborotate.SelectedItem.ToString());
        //        }
        //    });
        //}











        //public int addtofilerecno(string tmpdatafile, double initnum, double endnum, string filename, int copy, int rowcnt, double recordno)
        //{
        //    double tmpnum;

        //    try
        //    {
        //        tmpnum = initnum + recordno;

        //        if (ChkLogMethod.IsChecked == false)
        //        {
        //            // Equivalent to PrintLine(2, tmpnum.ToString)
        //            File.AppendAllText(filename, tmpnum.ToString() + Environment.NewLine);
        //        }
        //        else
        //        {
        //            if (strlog != null)
        //            {
        //                int index = Convert.ToInt32(tmpnum);

        //                if (index >= 0 && index < strlog.Length)
        //                {
        //                    string tmpline = strlog[index];

        //                    string[] tmplinearr = tmpline.Split(',');

        //                    double recnt = Convert.ToDouble(tmplinearr[2]) + 1;

        //                    strlog[index] =
        //                        tmplinearr[0] + "," +
        //                        tmplinearr[1] + "," +
        //                        recnt.ToString();
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return 0;
        //    }

        //    return 1;
        //}

        public void printimagebycsv(string imgpath)
        {
            string sourcePath = @"C:\print_data\" + imgpath;

            string ripPath =
                @"C:\ProgramData\Global Inkjet Systems\GIS Inkjet OS 2\Logging\RIP Engine\Rendered Images\"
                + imgpath;

            sMessage = "R,B," + sourcePath + "," + ripPath;
            m_client.sendMessageToServer(sMessage);

            sMessage = "P,Q,A," + ripPath + ",1";
            m_client.sendMessageToServer(sMessage);

            tmpripimgname = ripPath;
        }

        //private void CancelAsyncButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (BackgroundWorker1.WorkerSupportsCancellation)
        //    {
        //        sMessage = "P,Q,E";
        //        m_client.sendMessageToServer(sMessage);

        //        Dispatcher.Invoke(() =>
        //        {
        //            Lblcopycnt.Foreground = System.Windows.Media.Brushes.Red;
        //            Lblcopycnt.Text = "Creating LOG wait....";
        //        });

        //        sMessage = "R,A";
        //        m_client.sendMessageToServer(sMessage);

        //        sMessage = "P,A";
        //        m_client.sendMessageToServer(sMessage);

        //        createloginlast();

        //        Dispatcher.Invoke(() =>
        //        {
        //            Lblcopycnt.Foreground = System.Windows.Media.Brushes.Green;
        //            Lblcopycnt.Text = "Log has been created";
        //        });

        //        Deletefiles(@"C:\ProgramData\Global Inkjet Systems\GIS Inkjet OS 2\Logging\RIP Engine\Rendered Images\");
        //        Deletefiles(@"C:\Print_data\");

        //        BackgroundWorker1.CancelAsync();
        //    }
        //}

        private void Deletefiles(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (string filepath in Directory.GetFiles(path))
                {
                    try
                    {
                        File.Delete(filepath);
                    }
                    catch
                    {
                        // ignore locked files
                    }
                }
            }
        }

        //private void CboPCL_CheckedChanged(object sender, RoutedEventArgs e)
        //{
        //    if (CboPCL.IsChecked == true)
        //    {
        //        TxtresX.Visibility = Visibility.Visible;
        //        TxtResY.Visibility = Visibility.Visible;
        //        LblXRes.Visibility = Visibility.Visible;
        //        LblYRes.Visibility = Visibility.Visible;
        //        LblRotate.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        TxtresX.Visibility = Visibility.Collapsed;
        //        TxtResY.Visibility = Visibility.Collapsed;
        //        LblXRes.Visibility = Visibility.Collapsed;
        //        LblYRes.Visibility = Visibility.Collapsed;
        //        LblRotate.Visibility = Visibility.Collapsed;
        //    }
        //}

        //private void BrowseButton2_Click(object sender, RoutedEventArgs e)
        //{
        //    using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        //    {
        //        dialog.RootFolder = Environment.SpecialFolder.MyComputer;

        //        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //        {
        //            hotFolderTextBox.Text = dialog.SelectedPath;
        //            fName = dialog.SelectedPath;
        //        }
        //        else
        //        {
        //            return;
        //        }
        //    }

        //    DirectoryInfo di = new DirectoryInfo(fName);
        //    FileInfo[] aryFi;

        //    if (CboPCL.IsChecked == true)
        //    {
        //        aryFi = di.GetFiles("*.pcl", SearchOption.TopDirectoryOnly);
        //    }
        //    else
        //    {
        //        aryFi = di.GetFiles("*.bmp", SearchOption.TopDirectoryOnly);
        //    }

        //    fsw1.Path = hotFolderTextBox.Text;
        //    fsw2.Path = @"C:\ProgramData\Global Inkjet Systems\GIS Inkjet OS 2\Logging\RIP Engine\Rendered Images";

        //    if (!Directory.Exists(hotFolderTextBox.Text + @"\done"))
        //    {
        //        Directory.CreateDirectory(hotFolderTextBox.Text + @"\done");
        //    }

        //    string strFile = hotFolderTextBox.Text + @"\done\" +
        //                     "Printlog" +
        //                     DateTime.Now.ToString("dd-MM-yyyy") +
        //                     ".txt";

        //    if (!File.Exists(strFile))
        //    {
        //        File.Create(strFile).Close();
        //    }

        //    string[] lines = File.ReadAllLines(strFile);

        //    if (lines.Length > 0)
        //    {
        //        Label14.Text = lines.Length.ToString();

        //        string[] strArr = lines[lines.Length - 1].Split(',');
        //        Label13.Text = strArr[1];
        //    }

        //    if (CboPCL.IsChecked == true)
        //    {
        //        foreach (FileInfo fi in aryFi)
        //        {
        //            ListBox3.Items.Add(fi.Name);
        //            ConvertToImage(fi.Name);
        //        }
        //    }
        //    else
        //    {
        //        foreach (FileInfo fi in aryFi)
        //        {
        //            ListBox1.Items.Add(fi.Name);
        //        }
        //    }
        //}

        //private void BtnClear_Click(object sender, RoutedEventArgs e)
        //{
        //    // Clear first listbox
        //    ListBox1.Items.Clear();

        //    // Delete .bmp files from selected folder
        //    foreach (string deleteFile in
        //        Directory.GetFiles(hotFolderTextBox.Text,
        //                           "*.bmp",
        //                           SearchOption.TopDirectoryOnly))
        //    {
        //        File.Delete(deleteFile);
        //    }

        //    // Clear second listbox
        //    ListBox3.Items.Clear();

        //    // Delete .bmp files from done folder
        //    string donePath = System.IO.Path.Combine(hotFolderTextBox.Text, "done");

        //    if (Directory.Exists(donePath))
        //    {
        //        foreach (string deleteDoneFile in
        //            Directory.GetFiles(donePath,
        //                               "*.bmp",
        //                               SearchOption.TopDirectoryOnly))
        //        {
        //            File.Delete(deleteDoneFile);
        //        }
        //    }
        //}

        private void StartPrintImages_Click(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker3.IsBusy)
            {
                backgroundWorker3.RunWorkerAsync();
            }


            /*
           if (!backgroundWorker6.IsBusy)
           {
               backgroundWorker6.RunWorkerAsync();
           }
           */

        }


        public bool checkFileInUse(string sFile)
        {
            if (!File.Exists(sFile))
                return false;

            try
            {
                using (FileStream fs = File.Open(sFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                }
                return false;
            }
            catch
            {
                return true;
            }
        }

        public void printimage(string fullImagePath)
        {
            if (!File.Exists(fullImagePath))
                return;

            string ripPath = @"C:\ProgramData\Global Inkjet Systems\GIS Inkjet OS 2\Logging\RIP Engine\Rendered Images\"
                             + System.IO.Path.GetFileName(fullImagePath);

            string sMessage;

            sMessage = "R,B," + fullImagePath + "," + ripPath;
            m_client?.sendMessageToServer(sMessage);

            sMessage = "P,Q,A," + ripPath + ",1";
            m_client?.sendMessageToServer(sMessage);
        }


        private void BackgroundWorker3_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //string speed = m_client.testexampleEncoder("TmpEncoderSpeed");

            //if (!string.IsNullOrEmpty(speed))
            //{
            //    LblSpeed1.Text = speed;
            //}
        }

        private void BackgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            try
            {
                m_client?.sendMessageToServer("P,Q,P");

                while (!worker.CancellationPending)
                {
                    Thread.Sleep(100);
                   // worker.ReportProgress(1);
                }

                e.Cancel = true;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }


        private void BackgroundWorker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                System.Windows.MessageBox.Show("Printing Stopped");
            }
            else if (e.Result is Exception ex)
            {
                System.Windows.MessageBox.Show("Error: " + ex.Message);
            }
        }



        //private async void StopButton1_Click(object sender, RoutedEventArgs e)
        //{
        //    BtnStopImages1.IsEnabled = false;

        //    m_client.sendMessageToServer("R,A");   // reset first
        //    await Task.Delay(150);

        //    m_client.sendMessageToServer("P,A");   // abort print
        //    await Task.Delay(150);

        //    m_client.sendMessageToServer("P,Q,E"); // end queue

        //    BtnStopImages1.IsEnabled = true;
        //}





        //private async void StopButton1_Click(object sender, RoutedEventArgs e)
        //{
        //    var result = MessageBox.Show(
        //        "Are you sure you want to stop printing?",
        //        "Confirm Stop",
        //        MessageBoxButton.YesNo,
        //        MessageBoxImage.Warning);

        //    if (result != MessageBoxResult.Yes)
        //        return;

        //    BtnStopImages1.IsEnabled = false;

        //    try
        //    {
        //        await Task.Run(() =>
        //        {
        //            m_client.sendMessageToServerForStop("R,A");
        //            Thread.Sleep(200);

        //            m_client.sendMessageToServerForStop("P,A");
        //            Thread.Sleep(200);

        //            m_client.sendMessageToServerForStop("P,Q,E");
        //        });

        //        MessageBox.Show("Printing Stopped Successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Stop failed: " + ex.Message);
        //    }
        //    finally
        //    {
        //        BtnStopImages1.IsEnabled = true;
        //    }
        //}




        private async void StopButton1_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to stop printing?",
                "Confirm Stop",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            BtnStopImages1.IsEnabled = false;

            try
            {
                bool finalResult = await Task.Run(() =>
                {
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        bool r1 = m_client.sendMessageToServerForStop("R,A");
                        Thread.Sleep(150);

                        bool r2 = m_client.sendMessageToServerForStop("P,A");
                        Thread.Sleep(150);

                        bool r3 = m_client.sendMessageToServerForStop("P,Q,E");

                        if (r1 && r2 && r3)
                            return true;

                        // 🔹 If failed, wait a little before retry
                        Thread.Sleep(300);
                    }

                    return false;
                });

                if (finalResult)
                    MessageBox.Show("Printing Stopped Successfully");
                else
                    MessageBox.Show("Stop command sent but machine did not confirm.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Stop failed: " + ex.Message);
            }
            finally
            {
                BtnStopImages1.IsEnabled = true;
            }
        }





        private async void StartButton1_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to start printing?",
                "Confirm Start",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            BtnStartImages1.IsEnabled = false;
            try
            {
                bool finalResult = await Task.Run(() =>
                {
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        bool r1 = m_client.sendMessageToServerForStart("S,A");
                        Thread.Sleep(150);
                        bool r2 = m_client.sendMessageToServerForStart("P,S");
                        Thread.Sleep(150);
                        bool r3 = m_client.sendMessageToServerForStart("P,Q,S");

                        if (r1 && r2 && r3)
                            return true;

                        Thread.Sleep(300);
                    }
                    return false;
                });

                if (finalResult)
                    MessageBox.Show("Printing Started Successfully");
                else
                    MessageBox.Show("Start command sent but machine did not confirm.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Start failed: " + ex.Message);
            }
            finally
            {
                BtnStartImages1.IsEnabled = true;
            }
        }







        // Working 

        //private async void StopButton1_Click(object sender, RoutedEventArgs e)
        //{

        //    var result = MessageBox.Show(
        //"Are you sure you want to stop printing?",
        //"Confirm Stop",
        //MessageBoxButton.YesNo,
        //MessageBoxImage.Warning);

        //    if (result != MessageBoxResult.Yes)
        //        return;


        //    BtnStopImages1.IsEnabled = false;

        //    m_client.sendMessageToServer("R,A");
        //    await Task.Delay(150);

        //    m_client.sendMessageToServer("P,A");
        //    await Task.Delay(150);

        //    m_client.sendMessageToServer("P,Q,E");

        //    // 🔴 Thoda wait for machine to settle
        //    await Task.Delay(200);

        //    System.Windows.MessageBox.Show("Printing Stopped");

        //    BtnStopImages1.IsEnabled = true;
        //}





        private void StopButton2_Click(object sender, RoutedEventArgs e)
        {
            if (backgroundWorker3.WorkerSupportsCancellation)
            {
                string sMessage;

                sMessage = "P,Q,E";
                m_client.sendMessageToServer(sMessage);

                sMessage = "R,A";
                m_client.sendMessageToServer(sMessage);

                sMessage = "P,A";
                m_client.sendMessageToServer(sMessage);

                // Cancel background workers
                backgroundWorker3.CancelAsync();
                backgroundWorker7.CancelAsync();

                // Traffic light logic
                //if (trafficLightCallback(0) == true)
                //{
                //    BtnStopImages1.Background = System.Windows.Media.Brushes.Orange;
                //}
                //else
                //{
                //    BtnStopImages1.Background = System.Windows.Media.Brushes.White;
                //}

                System.Windows.MessageBox.Show("Printing Stopped");
            }
        }






        private void BtnShiftPlusX1_Click(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker4.IsBusy)
            {
                // UI thread me value read karo
                string value = TxtPrintShift1.Text;

                // Worker ko pass karo
                backgroundWorker4.RunWorkerAsync(value);
            }
        }


        private void BtnShiftPlusY1_Click(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker5.IsBusy)
            {
                // UI thread se value read karo
                string value = TxtPrintShiftY1.Text;

                // Worker ko value pass karo
                backgroundWorker5.RunWorkerAsync(value);
            }
        }





        




        private double smallStep = 1; // you can change to 0.1 if needed

        private void BtnShiftPlusSmallX1_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TxtPrintShift1.Text, out double value))
            {
                value += smallStep;
                TxtPrintShift1.Text = value.ToString("0.0");
            }
        }

        private void BtnShiftMinusX1_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TxtPrintShift1.Text, out double value))
            {
                value -= smallStep;
                TxtPrintShift1.Text = value.ToString("0.0");
            }
        }

        private void BtnShiftPlusSmallY1_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TxtPrintShiftY1.Text, out double value))
            {
                value += smallStep;
                TxtPrintShiftY1.Text = value.ToString("0.0");
            }
        }

        private void BtnShiftMinusY1_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TxtPrintShiftY1.Text, out double value))
            {
                value -= smallStep;
                TxtPrintShiftY1.Text = value.ToString("0.0");
            }
        }






        //private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        //{
        //    var workingArea = SystemParameters.WorkArea;

        //    this.Width = 300;
        //    this.Height = 200;

        //    this.Left = workingArea.Right - this.Width - 10;
        //    this.Top = workingArea.Bottom - this.Height - 10;
        //}











        #endregion
    }


    /*
     * This class holds information about a MainWindow object.
     */
    public class MainWindowInfo
    {
        public bool logClient { get; set; }
        public string logFile { get; set; }
        public string address { get; set; }
        public string port { get; set; }

        // Comment by Rahul

        //public string command { get; set; }
        //public string commandsFile { get; set; }
        //public bool repeatQueue { get; set; }
    }



    /*
     * This class holds information about the next command.
     */

    // Comment by Rahul

    //public class NextCommand
    //{
    //    public string command { get; set; }
    //} 





}


