using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;


namespace Gis.Utils.TCP_IP
{
    /*
     * This class implements a TCP/IP client which can be used to communictae with the Print Server.
     * 
     * This implementation can be used in a C# application.
      */
    public class Client
    {
        protected IntPtr m_handle;
        private ushort m_port;
        private string m_address;

        
        private IntPtr m_pvTCP_IPClientObject = IntPtr.Zero;

        private void CheckConnectionState(string propertyName, bool state)
        {
            if (state != IsConnectedToServer())
            {
                throw new System.InvalidOperationException(string.Format("Cannot change the {0} property when ", propertyName, state ? "disconnected" : "connected"));
            }
        }

        // Really we should only allow values to be set while the server is disconnected.
        public ushort Port
        {
            get { return m_port; }
            set { CheckConnectionState("Port", false); m_port = value; }
        }

        public string Address
        {
            get { return m_address; }
            set { CheckConnectionState("Address", false); m_address = value; }
        }

        /*
	     * The constructor for the CCSTCpClient class, which initialises the variables.
	     */


        [DllImport("GIS Utility - TCP-IP Comms.dll",
CallingConvention = CallingConvention.StdCall,
CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool initialiseVBWrapper(
    IntPtr p_pvTCP_IPClientObject,
    bool p1,
    string p2,
    bool p3,
    bool p4,
    IntPtr cb1,
    IntPtr cb2,
    IntPtr cb3,
    IntPtr cb4,
    IntPtr cb5,
    IntPtr cb6,
    IntPtr cb7,
    IntPtr cb8,
    IntPtr cb9,
    IntPtr cb10,
    IntPtr cb11,
    IntPtr cb12,
    IntPtr cb13,
    IntPtr cb14,
    IntPtr cb15,
    IntPtr cb16);



        public Client(string address = "localhost", ushort port = 2000)
        {
            m_handle = IntPtr.Zero;
            m_address = address;
            m_port = port;

            bool created = createVBTCP_IPClient(ref m_pvTCP_IPClientObject);

            if (!created || m_pvTCP_IPClientObject == IntPtr.Zero)
            {
                MessageBox.Show("Failed to create TCP/IP Client object");
            }


            // 🔥 THIS IS WHERE initialiseVBWrapper MUST BE CALLED
            bool initialised = initialiseVBWrapper(
                m_pvTCP_IPClientObject,
                false,
                "",
                false,
                false,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (!initialised)
            {
                MessageBox.Show("Failed to initialise TCP/IP comms");
            }


        }

        /*
	     * The destructor for the CCSTCpClient class.
	     */
        ~Client()
        {

        }

        #region .NET events

        public class ClientEventArgs
        {
            public bool OK;

            public ClientEventArgs()
            {
                OK = true;
            }
        }

        public class MessageEventArgs : ClientEventArgs
        {
            public readonly string Message;
            public MessageEventArgs(string message)
                : base()
            {
                Message = message;
            }
        }

        public class InkSystemParameterValueEventArgs : ClientEventArgs
        {
            public readonly string ParameterName;
            public readonly string ParameterValue;

            public InkSystemParameterValueEventArgs(string parameterName, string parameterValue)
                : base()
            {
                ParameterName = parameterName;
                ParameterValue = parameterValue;
            }
        }

        public class TrafficLightEventArgs : ClientEventArgs
        {
            public readonly int State;

            public TrafficLightEventArgs(int state)
                : base()
            {
                State = state;
            }
        }

        public class SwatheProcessedEventArgs : ClientEventArgs
        {
            public readonly int Swathe;

            public SwatheProcessedEventArgs(int swathe)
                : base()
            {
                Swathe = swathe;
            }
        }

        public class StatusEventArgs : MessageEventArgs
        {
            public readonly string EngineCode;
            public readonly int StatusCode;

            public StatusEventArgs(string message, string engineCode, int statusCode)
                : base(message)
            {
                EngineCode = engineCode;
                StatusCode = statusCode;
            }
        }

        public class ReadyToPrintEventArgs : ClientEventArgs
        {
            public readonly int SwathesToPrint;

            public ReadyToPrintEventArgs(int swathesToPrint)
                : base()
            {
                SwathesToPrint = swathesToPrint;
            }
        }

        public class InterfaceLogEventArgs : MessageEventArgs
        {
            public readonly int LogLevel;

            public InterfaceLogEventArgs(string message, int logLevel)
                : base(message)
            {
                LogLevel = logLevel;
            }
        }

        public struct Point3D
        {
            public double X, Y, Z;
        }

        public class PositionEventArgs : ClientEventArgs
        {
            public readonly Point3D Position;

            public PositionEventArgs(Point3D position)
                : base()
            {
                Position = position;
            }
        }

        public class LabelNumberEventArgs : ClientEventArgs
        {
            public readonly int LabelNumber;

            public LabelNumberEventArgs(int labelNumber)
                : base()
            {
                LabelNumber = labelNumber;
            }
        }

        // TODO: Move to appropriate subclass: Take enormous list out of constructor. This needs to be done as
        // well on the server side, since we pass the whole lot through to initialiseWrapper. Instead we should
        // have various addCallback wrappers each taking one callback. (If some alwazs come in pairs, we can
        // reduce the actual number of functions required).
        public delegate void ConnectedHandler(object sender, ClientEventArgs e);
        public delegate void DisconnectedHandler(object sender, ClientEventArgs e);
        public delegate void MessageSentHandler(object sender, MessageEventArgs e);
        public delegate void MessageReceivedHandler(object sender, MessageEventArgs e);
        public delegate void StatusHandler(object sender, StatusEventArgs e);
        public delegate void TrafficLightHandler(object sender, TrafficLightEventArgs e);
        public delegate void SwatheProcessedHandler(object sender, SwatheProcessedEventArgs e);
        public delegate void LabelNumberHandler(object sender, LabelNumberEventArgs e);
        public delegate void PositionHandler(object sender, PositionEventArgs e);
        public delegate void InkSystemParameterValueHandler(object sender, InkSystemParameterValueEventArgs e);
        public delegate void PrintheadStatusHandler(object sender, MessageEventArgs e);
        public delegate void PrintStartedHandler(object sender, ClientEventArgs e);
        public delegate void ReadyToPrintHandler(object sender, ReadyToPrintEventArgs e);
        public delegate void EndOfPrintingHandler(object sender, ClientEventArgs e);
        public delegate void PrintServerReporterHandler(object sender, MessageEventArgs e);
        public delegate void InterfaceLogHandler(object sender, InterfaceLogEventArgs e);

        public event ConnectedHandler Connected;
        public event DisconnectedHandler ConnectionFailed;
        public event DisconnectedHandler Disconnected;
        public event MessageSentHandler MessageSent;
        public event MessageReceivedHandler MessageReceived;
        public event StatusHandler Status;
        public event TrafficLightHandler TrafficLight;
        public event SwatheProcessedHandler SwatheProcessed;
        public event LabelNumberHandler LabelNumber;
        public event PositionHandler Position;
        public event InkSystemParameterValueHandler InkSystemParameterValue;
        public event PrintheadStatusHandler PrintheadStatus;
        public event PrintStartedHandler PrintStarted;
        public event ReadyToPrintHandler ReadyToPrint;
        public event EndOfPrintingHandler EndOfPrinting;
        public event PrintServerReporterHandler PrintServerReporter;
        public event InterfaceLogHandler InterfaceLog;

        #endregion .NET events

        #region Translate native callbacks into .NET events

        protected class NativeCallbackHolder
        {
            public delegate bool ConnectedCallback();
            public delegate bool DisconnectedCallback();
            public delegate bool MessageSentCallback(string message);
            public delegate bool MessageReceivedCallback(string message);
            public delegate bool StatusCallback(string engineCode, string status, int statusCode);
            public delegate bool TrafficLightCallback(int state);
            public delegate bool SwatheProcessedCallback(int swatheProcessed);
            public delegate bool LabelNumberCallback(int labelNumber);
            public delegate bool PositionCallback(double xPosition, double yPosition, double zPosition);
            public delegate bool InkSystemParameterValueCallback(string parameterName, string parameterValue);
            public delegate bool PrintheadStatusCallback(string printheadStatusInformation);
            public delegate bool PrintStartedCallback();
            public delegate bool ReadyToPrintCallback(int numberOfSwathes);
            public delegate bool EndOfPrintingCallback();
            public delegate bool PrintServerReporterCallback(string message);
            public delegate bool InterfaceLogCallback(int level, string logMessage);

            public ConnectedCallback m_nativeConnectedCallback;
            public DisconnectedCallback m_nativeDisconnectedCallback;
            public MessageSentCallback m_nativeMessageSentCallback;
            public MessageReceivedCallback m_nativeMessageReceivedCallback;
            public StatusCallback m_nativeStatusCallback;
            public TrafficLightCallback m_nativeTrafficLightCallback;
            public SwatheProcessedCallback m_nativeSwatheProcessedCallback;
            public LabelNumberCallback m_nativeLabelNumberCallback;
            public PositionCallback m_nativePositionCallback;
            public InkSystemParameterValueCallback m_nativeInkSystemParameterValueCallback;
            public PrintheadStatusCallback m_nativePrintheadStatusCallback;
            public PrintStartedCallback m_nativePrintStartedCallback;
            public ReadyToPrintCallback m_nativeReadyToPrintCallback;
            public EndOfPrintingCallback m_nativeEndOfPrintingCallback;
            public PrintServerReporterCallback m_nativePrintServerReporterCallback;
            public InterfaceLogCallback m_nativeInterfaceLogCallback;
        }

        // Create callbacks in this instance to prevent premature GC.
        NativeCallbackHolder m_callbackHolder; 

        protected bool OnNativeConnectedCallback()
        {
            if (Connected != null)
            {
                var e = new ClientEventArgs();
                Connected(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativeDisconnectedCallback()
        {
            // We'll need to re-register when we reconnect
            m_bRegisteredForPrintControllerInformation = false;
            m_bRegisteredForNetworkControllerInformation = false;
            m_bRegisteredForPrintServerInformation = false;
            m_bRegisteredForRenderEngineInformation = false;

            if (Disconnected != null)
            {
                var e = new ClientEventArgs();
                Disconnected(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativeMessageSentCallback(string message)
        {
            if (MessageSent != null)
            {
                var e = new MessageEventArgs(message);
                MessageSent(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativeMessageReceivedCallback(string message)
        {
            if (MessageReceived != null)
            {
                var e = new MessageEventArgs(message);
                MessageReceived(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativeStatusCallback(string engineCode, string status, int statusCode)
        {
            if (Status != null)
            {
                var e = new StatusEventArgs(status, engineCode, statusCode);
                Status(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativeTrafficLightCallback(int state)
        {
            if (TrafficLight != null)
            {
                var e = new TrafficLightEventArgs(state);
                TrafficLight(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativeSwatheProcessedCallback(int swatheProcessed)
        {
            if (SwatheProcessed != null)
            {
                var e = new SwatheProcessedEventArgs(swatheProcessed);
                SwatheProcessed(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativeLabelNumberCallback(int labelNumber)
        {
            if (LabelNumber != null)
            {
                var e = new LabelNumberEventArgs(labelNumber);
                LabelNumber(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativePositionCallback(double xPosition, double yPosition, double zPosition)
        {
            if (Position != null)
            {
                Point3D pos;
                pos.X = xPosition;
                pos.Y = yPosition;
                pos.Z = zPosition;
                var e = new PositionEventArgs(pos);
                Position(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativeInkSystemParameterValueCallback(string parameterName, string parameterValue)
        {
            if (InkSystemParameterValue != null)
            {
                var e = new InkSystemParameterValueEventArgs(parameterName, parameterValue);
                InkSystemParameterValue(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativePrintheadStatusCallback(string printheadStatusInformation)
        {
            if (PrintheadStatus != null)
            {
                var e = new MessageEventArgs(printheadStatusInformation);
                PrintheadStatus(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativePrintStartedCallback()
        {
            if (PrintStarted != null)
            {
                var e = new ClientEventArgs();
                PrintStarted(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativeReadyToPrintCallback(int numberOfSwathes)
        {
            if (ReadyToPrint != null)
            {
                var e = new ReadyToPrintEventArgs(numberOfSwathes);
                ReadyToPrint(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativeEndOfPrintingCallback()
        {
            if (EndOfPrinting != null)
            {
                var e = new ClientEventArgs();
                EndOfPrinting(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativePrintServerReporterCallback(string message)
        {
            if (PrintServerReporter != null)
            {
                var e = new MessageEventArgs(message);
                PrintServerReporter(this, e);
                return e.OK;
            }

            return true;
        }

        protected bool OnNativeInterfaceLogCallback(int level, string logMessage)
        {
            if (InterfaceLog != null)
            {
                var e = new InterfaceLogEventArgs(logMessage, level);
                InterfaceLog(this, e);
                return e.OK;
            }

            return true;
        }

        #endregion

        /*
         * This initialises the TCP/IP client, and automatically creates a log manager (which must be deleted).
         * This must be called only one, before any other functions are called.
         * \param securityKeyA - The A security key used in the security manager. This is not required if connecting to a Print Server which has TCP/IP enabled.
         * \param securityKeyA - The B security key used in the security manager. This is not required if connecting to a Print Server which has TCP/IP enabled.
         * \param logToFileEnabled - Whether logging to file is enabled or not.
         * \param logFile - The path to thr log file.
         * \param logToInterfaceEnabled - Whether messages should be logged to the interface or not.
         * \param connectedCallback - The connected callback function.
         * \param disconnectedCallback - The disconnected callback function.
         * \param messageSentCallback - The message sent callback function.
         * \param messageReceivedCallback - The message received callback function.
         * \param statusCallback - The status callback function.
         * \param trafficLightCallback - The traffic light callback function.
         * \param swatheProcessedCallback - The swathe processed callback function.
         * \param labelNumberCallback - The label number callback function.
         * \param positionCallback - The position callback function.
         * \param inkSystemParameterValueCallback - The ink system parameter value callback function.
         * \param printheadStatusCallback - The printhead status callback function.
         * \param printStartedCallback - The print started callback function.
         * \param readyToPrintCallback - The ready to print callback function.
         * \param endOfPrintingCallback - The end of printing callback function.
         * \param printServerReporterCallback - The print server reporter callback function.
         * \param interfaceLogCallback - The interface log callback function.
         * \return Whether the initialisation was successful or not.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "createTCP_IPClient")]
        private static extern bool CreateTCP_IPClient(ref IntPtr tcp_ipClientObject);
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "initialiseWrapper")]
        private static extern bool InitialiseTCP_IPClient(
            IntPtr tcp_ipClientObject,
            [MarshalAs(UnmanagedType.LPWStr)]string securityKeyA,
            [MarshalAs(UnmanagedType.LPWStr)]string securityKeyB,
            bool logToFileEnabled, [MarshalAs(UnmanagedType.LPWStr)]string logFile,
            bool logToInterfaceEnabled, bool extendedNetworkLogging,
            NativeCallbackHolder.ConnectedCallback connectedCallback,
            NativeCallbackHolder.DisconnectedCallback disconnectedCallback,
            NativeCallbackHolder.MessageSentCallback messageSentCallback,
            NativeCallbackHolder.MessageReceivedCallback messageReceivedCallback,
            NativeCallbackHolder.StatusCallback statusCallback,
            NativeCallbackHolder.TrafficLightCallback trafficLightCallback,
            NativeCallbackHolder.SwatheProcessedCallback swatheProcessedCallback,
            NativeCallbackHolder.LabelNumberCallback labelNumberCallback,
            NativeCallbackHolder.PositionCallback positionCallback,
            NativeCallbackHolder.InkSystemParameterValueCallback inkSystemParameterValueCallback,
            NativeCallbackHolder.PrintheadStatusCallback printheadStatusCallback,
            NativeCallbackHolder.PrintStartedCallback printStartedCallback,
            NativeCallbackHolder.ReadyToPrintCallback readyToPrintCallback,
            NativeCallbackHolder.EndOfPrintingCallback endOfPrintingCallback,
            NativeCallbackHolder.PrintServerReporterCallback printServerReporterCallback,
            NativeCallbackHolder.InterfaceLogCallback interfaceLogCallback);

        public bool Initialise(string securityKeyA, string securityKeyB, bool logToFileEnabled, string logFile, bool logToInterfaceEnabled)
        {
            if (!CreateTCP_IPClient(ref m_handle))
            {
                return false;
            }

            // TODO: We only really need to add most of the native callbacks when we have delegates connected up to our own event members.
            // And we can probably remove them when the last delegate is removed. But not sure we ever know the last delegte is removed...
            // nor have a native API to remove them.
            m_callbackHolder = new NativeCallbackHolder();

            m_callbackHolder.m_nativeConnectedCallback = new NativeCallbackHolder.ConnectedCallback(OnNativeConnectedCallback);
            m_callbackHolder.m_nativeDisconnectedCallback = new NativeCallbackHolder.DisconnectedCallback(OnNativeDisconnectedCallback);
            m_callbackHolder.m_nativeMessageSentCallback = new NativeCallbackHolder.MessageSentCallback(OnNativeMessageSentCallback);
            m_callbackHolder.m_nativeMessageReceivedCallback = new NativeCallbackHolder.MessageReceivedCallback(OnNativeMessageReceivedCallback);
            m_callbackHolder.m_nativeStatusCallback = new NativeCallbackHolder.StatusCallback(OnNativeStatusCallback);
            m_callbackHolder.m_nativeTrafficLightCallback = new NativeCallbackHolder.TrafficLightCallback(OnNativeTrafficLightCallback);
            m_callbackHolder.m_nativeSwatheProcessedCallback = new NativeCallbackHolder.SwatheProcessedCallback(OnNativeSwatheProcessedCallback);
            m_callbackHolder.m_nativeLabelNumberCallback = new NativeCallbackHolder.LabelNumberCallback(OnNativeLabelNumberCallback);
            m_callbackHolder.m_nativePositionCallback = new NativeCallbackHolder.PositionCallback(OnNativePositionCallback);
            m_callbackHolder.m_nativeInkSystemParameterValueCallback = new NativeCallbackHolder.InkSystemParameterValueCallback(OnNativeInkSystemParameterValueCallback);
            m_callbackHolder.m_nativePrintheadStatusCallback = new NativeCallbackHolder.PrintheadStatusCallback(OnNativePrintheadStatusCallback);
            m_callbackHolder.m_nativePrintStartedCallback = new NativeCallbackHolder.PrintStartedCallback(OnNativePrintStartedCallback);
            m_callbackHolder.m_nativeReadyToPrintCallback = new NativeCallbackHolder.ReadyToPrintCallback(OnNativeReadyToPrintCallback);
            m_callbackHolder.m_nativeEndOfPrintingCallback = new NativeCallbackHolder.EndOfPrintingCallback(OnNativeEndOfPrintingCallback);
            m_callbackHolder.m_nativePrintServerReporterCallback = new NativeCallbackHolder.PrintServerReporterCallback(OnNativePrintServerReporterCallback);
            m_callbackHolder.m_nativeInterfaceLogCallback = new NativeCallbackHolder.InterfaceLogCallback(OnNativeInterfaceLogCallback);

            if (!InitialiseTCP_IPClient(m_handle, securityKeyA, securityKeyB, logToFileEnabled, logFile, logToInterfaceEnabled, false,
                m_callbackHolder.m_nativeConnectedCallback,
                m_callbackHolder.m_nativeDisconnectedCallback,
                m_callbackHolder.m_nativeMessageSentCallback,
                m_callbackHolder.m_nativeMessageReceivedCallback,
                m_callbackHolder.m_nativeStatusCallback,
                m_callbackHolder.m_nativeTrafficLightCallback,
                m_callbackHolder.m_nativeSwatheProcessedCallback,
                m_callbackHolder.m_nativeLabelNumberCallback,
                m_callbackHolder.m_nativePositionCallback,
                m_callbackHolder.m_nativeInkSystemParameterValueCallback,
                m_callbackHolder.m_nativePrintheadStatusCallback,
                m_callbackHolder.m_nativePrintStartedCallback,
                m_callbackHolder.m_nativeReadyToPrintCallback,
                m_callbackHolder.m_nativeEndOfPrintingCallback,
                m_callbackHolder.m_nativePrintServerReporterCallback,
                m_callbackHolder.m_nativeInterfaceLogCallback))
            {
                return false;
            }

            Log(GetLowLogLevel(), "CCSTCP_IPClient::Initialise() : Initialised.");

            return true;
        }
	    /*
	     * This aborts the TCP/IP client.
	     * This must be called only once, and no other functions must be called after it is called.
	     * \return Whether the abort was successful or not.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "destroyTCP_IPClient")]
        private static extern bool DestroyTCP_IPClient(IntPtr tcp_ipClientObject);
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "abortWrapper")]
        private static extern bool AbortTCP_IPClient(IntPtr tcp_ipClientObject);
        public bool Abort()
        {
            Log(GetLowLogLevel(), "CCSTCP_IPClient::Abort()");

            bool returnValue = true;

            if (IsConnectedToServer())
            {
                if (!DisconnectFromServer())
                {
                    Log(GetErrorLogLevel(), "CCSTCP_IPClient::Abort() : Failed to disconnect from the server.");

                    returnValue = false;
                }
            }

            if (!AbortTCP_IPClient(m_handle))
            {
                returnValue = false;
            }

            if (!DestroyTCP_IPClient(m_handle))
            {
                returnValue = false;
            }

            m_handle = IntPtr.Zero;

            return returnValue;
        }


        /*
         * This connects to the server with the properties specified.
         * A successful connection must be made before any function which communicates with the server is called.
         * This is only valid when not connected to a server.
         * \param ipAddress - The IP address of the server to connect to. Use "localhost" if the server is on the same computer.
         * \param port - The network port of the server to connect to.
         * \return Whether the connection was successful or not.
         */
        /*[DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "connectToServerWrapper", CharSet = CharSet.Unicode)]
        private static extern bool ConnectToServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string serverAddress, int port);
        public bool ConnectToServer(string ipAddress, ushort port)
        {
            Log(GetLowLogLevel(), "CCSTCP_IPClient::ConnectToServer() : Connecting to server \"" + ipAddress + "\" on port " + port.ToString() + ".");

            if (!ConnectToServer(m_TCP_IPClientObject, ipAddress, port))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ConnectToServer() : Failed to connect to the server.");

                return false;
            }

            return true;
        } */
        /*
         * This connects to the server with the properties specified.
         * A successful connection must be made before any function which communicates with the server is called.
         * This is only valid when not connected to a server.
         * \param ipAddress - The IP address of the server to connect to. Use "localhost" if the server is on the same computer.
         * \param port - The network port of the server to connect to.
         * \param connectionTimeout - The maximum time in milliseconds the function will wait until the connection has been established. This parameter should be set to -1 to have no timeout i.e. the function will wait until the connection is established or it is stopped. This parameter should be set to 0 to have no wait i.e. the functon will fail if a connection cannot be made immediately.
	     * \param waitUntilConnectionComplete - If this is true, the function will not return until the connection process is complete.
         * \param continuouslyMonitorServer - If this is true, the client will continuously monitor the server to see if there is anything to read from it and to check if it has disconnected. If this is false, an attempt to read or check the server will only be made when the appropriate function is called.
         * \param automaticallyReconnectToServer - If this is true, the client will automatically try to reconnect to the server if it detects that the server has disconnected.
         * \return Whether the connection was successful or not.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "connectToServerFullWrapper", CharSet = CharSet.Unicode)]
	    [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ConnectToServerFull(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string serverAddress, int port, int connectionTimeout, bool waitUntilConnectionComplete, bool continuouslyMonitorServer, bool automaticallyReconnectToServer);
        public bool ConnectToServer(int connectionTimeout = 0, bool waitUntilConnectionComplete = true, bool continuouslyMonitorServer = true, bool automaticallyReconnectToServer = false)
        {
            Log(GetLowLogLevel(), "CCSTCP_IPClient::ConnectToServer() : Connecting to server \"" + m_address + "\" on port " + m_port.ToString() + ".");

            if (!ConnectToServerFull(m_handle, m_address, m_port, connectionTimeout, waitUntilConnectionComplete, continuouslyMonitorServer, automaticallyReconnectToServer))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ConnectToServer() : Failed to connect to the server.");

                return false;
            }

            return true;
        }
        /*
	     * This launches the server executable at the patch specified and then connects to the server with the properties specified.
	     * The server must be located on the same computer as* This client, so the IP address can only be "localhost".
	     * A successful connection must be made before any function which communicates with the server is called.
	     * This is only valid when not connected to a server.
	     * \param executablePath - The path to the executable to launch. Leave blank to launch the "GIS Print Server 2.exe" executable contain within the current working folder.
	     * \param serverID - The ID of the server.
	     * \param configurationPath - The configuration the server will load after it starts.
	     * \param port - The network port of the server to connect to.
         * \param startMinimised - If true, the server will be minimised when it starts.
	     * \return Whether the connection was successful or not.
	     */
        /*[DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "launchAndConnectToServerWrapper", CharSet = CharSet.Unicode)]
        private static extern bool LaunchAndConnectToServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string executablePath, [MarshalAs(UnmanagedType.LPWStr)]string serverID, string configurationPath, int port, bool startMinimised);
        public bool LaunchAndConnectToServer(string executablePath, string serverID, string configurationPath, int port, bool startMinimised)
        {
            Log(GetLowLogLevel(), "CCSTCP_IPClient::LaunchAndConnectToServer() : Connecting to server at \"" + executablePath + "\" on port " + port.ToString() + ".");

            if (!LaunchAndConnectToServer(m_TCP_IPClientObject, executablePath, serverID, configurationPath, port, startMinimised))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ConnectToServer() : Failed to launch and connect to the server.");

                return false;
            }

            return true;
        } */
        /*
         * This launches the server executable at the patch specified and then connects to the server with the properties specified.
         * The server must be located on the same computer as* This client, so the IP address can only be "localhost".
         * A successful connection must be made before any function which communicates with the server is called.
         * This is only valid when not connected to a server.
         * \param executablePath - The path to the executable to launch. Leave blank to launch the "GIS Print Server 2.exe" executable contain within the current working folder.
         * \param serverID - The ID of the server.
         * \param configurationPath - The configuration the server will load after it starts.
         * \param port - The network port of the server to connect to.
         * \param startMinimised - If true, the server will be minimised when it starts.
         * \param connectionTimeout - The maximum time in milliseconds the function will wait until the connection has been established. This parameter should be set to -1 to have no timeout i.e. the function will wait until the connection is established or it is stopped. This parameter should be set to 0 to have no wait i.e. the functon will fail if a connection cannot be made immediately.
         * \param waitUntilConnectionComplete - If this is true, the function will not return until the connection process is complete.
         * \param continuouslyMonitorServer - If this is true, the client will continuously monitor the server to see if there is anything to read from it and to check if it has disconnected. If this is false, an attempt to read or check the server will only be made when the appropriate function is called.
         * \param automaticallyReconnectToServer - If this is true, the client will automatically try to reconnect to the server if it detects that the server has disconnected.
         * \return Whether the connection was successful or not.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "launchAndConnectToServerFullWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool LaunchAndConnectToServerFull(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string executablePath, [MarshalAs(UnmanagedType.LPWStr)]string serverID, [MarshalAs(UnmanagedType.LPWStr)]string configurationPath, int port, bool startMinimised, int connectionTimeout, bool waitUntilConnectionComplete, bool continuouslyMonitorServer, bool automaticallyReconnectToServer);
        public bool LaunchAndConnectToServer(string executablePath, string serverID, string configurationPath, bool startMinimised, int connectionTimeout = 0, bool waitUntilConnectionComplete = true, bool continuouslyMonitorServer = true, bool automaticallyReconnectToServer = false)
        {
            Log(GetLowLogLevel(), "CCSTCP_IPClient::LaunchAndConnectToServer() : Connecting to server at \"" + executablePath + "\" on port " + m_port.ToString() + ".");

            if (!LaunchAndConnectToServerFull(m_handle, executablePath, serverID, configurationPath, m_port, startMinimised, connectionTimeout, waitUntilConnectionComplete, continuouslyMonitorServer, automaticallyReconnectToServer))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::LaunchAndConnectToServer() : Failed to launch and connect to the server.");

                return false;
            }

            return true;
        }
        /*
         * This connects to a print server monitor, launches the server with the properties specified and then connects to it.
	     * A successful connection must be made before any function which communicates with the server is called.
	     * This is only valid when not connected to a server or print server monitor.
	     * \param ipAddress - The IP address of the print server monitor and print server server to connect to. Use "localhost" if the server is on the same computer.
	     * \param monitorPort - The network port of the monitor to connect to.
	     * \param serverPort - The network port of the server to connect to.
	     * \param serverID - The ID of the server.
	     * \param configurationPath - The configuration the server will load after it starts. 
	     * \param startMinimised - If true, the server will be minimised when it starts.
         * \param connectedToPMB - Whether this server will communicate with a PMB or not.
	     * \return Whether the connection was successful or not.
         */
        /*[DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "launchAndConnectToServerThroughMonitorWrapper", CharSet = CharSet.Unicode)]
        private static extern bool LaunchAndConnectToServerThroughMonitor(IntPtr tcp_ipClientObject, IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string ipAddress, int monitorPort, int serverPort, [MarshalAs(UnmanagedType.LPWStr)]string serverID, [MarshalAs(UnmanagedType.LPWStr)]string configurationPath, bool startMinimised, bool connectedToPMB);
        public bool LaunchAndConnectToServerThroughMonitor(string ipAddress, int monitorPort, int serverPort, string serverID, string configurationPath, bool startMinimised, bool connectedToPMB)
        {
            Log(GetLowLogLevel(), "CCSTCP_IPClient::LaunchAndConnectToServerThroughMonitor() : Connecting to server at \"" + address + "\" on monitor port " + monitorPort.ToString() + " and server port " + serverPort.ToString() + ".");

            if (!LaunchAndConnectToServerThroughMonitor(m_TCP_IPClientObject, executablePath, serverID, configurationPath, port, startMinimised, connectedToPMB))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::LaunchAndConnectToServerThroughMonitor() : Failed to launch and connect to the server.");

                return false;
            }

            return true;
        } */
        /*
         * This connects to a print server monitor, launches the server with the properties specified and then connects to it.
	     * A successful connection must be made before any function which communicates with the server is called.
	     * This is only valid when not connected to a server or print server monitor.
	     * \param ipAddress - The IP address of the print server monitor and print server server to connect to. Use "localhost" if the server is on the same computer.
	     * \param monitorPort - The network port of the monitor to connect to.
	     * \param serverPort - The network port of the server to connect to.
	     * \param serverID - The ID of the server.
	     * \param configurationPath - The configuration the server will load after it starts. 
	     * \param startMinimised - If true, the server will be minimised when it starts.
         * \param connectedToPMB - Whether this server will communicate with a PMB or not.
	     * \param connectionTimeout - The maximum time in milliseconds the function will wait until the connection has been established. This parameter should be set to -1 to have no timeout i.e. the function will wait until the connection is established or it is stopped. This parameter should be set to 0 to have no wait i.e. the functon will fail if a connection cannot be made immediately.
	     * \param waitUntilConnectionComplete - If this is true, the function will not return until the connection process is complete.
	     * \param continuouslyMonitorServer - If this is true, the client will continuously monitor the server to see if there is anything to read from it and to check if it has disconnected. If this is false, an attempt to read or check the server will only be made when the appropriate function is called.
	     * \param automaticallyReconnectToServer - If this is true, the client will automatically try to reconnect to the server if it detects that the server has disconnected.
	     * \return Whether the connection was successful or not.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "launchAndConnectToServerThroughMonitorFullWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool LaunchAndConnectToServerThroughMonitorFull(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string ipAddress, int monitorPort, int serverPort, [MarshalAs(UnmanagedType.LPWStr)]string serverID, [MarshalAs(UnmanagedType.LPWStr)]string configurationPath, bool startMinimised, bool connectedToPMB, int connectionTimeout, bool waitUntilConnectionComplete, bool continuouslyMonitorServer, bool automaticallyReconnectToServer);
        public bool LaunchAndConnectToServerThroughMonitor(int monitorPort, string serverID, string configurationPath, bool startMinimised, bool connectedToPMB, int connectionTimeout = 0, bool waitUntilConnectionComplete = true, bool continuouslyMonitorServer = true, bool automaticallyReconnectToServer = false)
        {
            Log(GetLowLogLevel(), "CCSTCP_IPClient::LaunchAndConnectToServerThroughMonitor() : Connecting to server at \"" + m_address + "\" on monitor port " + monitorPort.ToString() + " and server port " + m_port.ToString() + ".");

            if (!LaunchAndConnectToServerThroughMonitorFull(m_handle, m_address, monitorPort, m_port, serverID, configurationPath, startMinimised, connectedToPMB, connectionTimeout, waitUntilConnectionComplete, continuouslyMonitorServer, automaticallyReconnectToServer))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::LaunchAndConnectToServerThroughMonitor() : Failed to launch and connect to the server.");

                return false;
            }

            return true;
        }
	    /*
         * This connects to the server with the properties specified.
         * A successful connection must be made before any function which communicates with the server is called.
         * This is only valid when not connected to a server.
         * \param ipAddress - The IP address of the server to connect to. Use "localhost" if the server is on the same computer.
         * \param port - The network port of the server to connect to.
         * \return Whether the connection was successful or not.
         */
        /*[DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "reconnectToServerWrapper", CharSet = CharSet.Unicode)]
        private static extern bool ReconnectToServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string serverAddress, int port);
        public bool ReconnectToServer(string ipAddress, ushort port)
        {
            Log(GetLowLogLevel(), "CCSTCP_IPClient::ReconnectToServer() : Reconnecting to server \"" + ipAddress + "\" on port " + port.ToString() + ".");

            if (!ReconnectToServer(m_TCP_IPClientObject, ipAddress, port))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ReconnectToServer() : Failed to reconnect to the server.");

                return false;
            }

            return true;
        } */
        /*
         * This connects to the server with the properties specified.
         * A successful connection must be made before any function which communicates with the server is called.
         * This is only valid when not connected to a server.
         * \param ipAddress - The IP address of the server to connect to. Use "localhost" if the server is on the same computer.
         * \param port - The network port of the server to connect to.
         * \param connectionTimeout - The maximum time in milliseconds the function will wait until the connection has been established. This parameter should be set to -1 to have no timeout i.e. the function will wait until the connection is established or it is stopped. This parameter should be set to 0 to have no wait i.e. the functon will fail if a connection cannot be made immediately.
	     * \param waitUntilConnectionComplete - If this is true, the function will not return until the connection process is complete.
         * \param continuouslyMonitorServer - If this is true, the client will continuously monitor the server to see if there is anything to read from it and to check if it has disconnected. If this is false, an attempt to read or check the server will only be made when the appropriate function is called.
         * \param automaticallyReconnectToServer - If this is true, the client will automatically try to reconnect to the server if it detects that the server has disconnected.
         * \return Whether the connection was successful or not.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "reconnectToServerFullWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ReconnectToServerFull(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string serverAddress, int port, int connectionTimeout, bool waitUntilConnectionComplete, bool continuouslyMonitorServer, bool automaticallyReconnectToServer);
        public bool ReconnectToServer(int connectionTimeout = 0, bool waitUntilConnectionComplete = true, bool continuouslyMonitorServer = true, bool automaticallyReconnectToServer = false)
        {
            Log(GetLowLogLevel(), "CCSTCP_IPClient::ReconnectToServer() : Reconnecting to server \"" + m_address + "\" on port " + m_port.ToString() + ".");

            if (!ReconnectToServerFull(m_handle, m_address, m_port, connectionTimeout, waitUntilConnectionComplete, continuouslyMonitorServer, automaticallyReconnectToServer))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ReconnectToServer() : Failed to reconnect to the server.");

                return false;
            }

            return true;
        }
	    /*
	     * This disconnects from the server the client is currently connected to.
	     * This is only valid when connected to a server.
	     * \return Whether the disconnection was successful or not.
	     */
        /*[DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "disconnectFromServerWrapper")]
        private static extern bool DisconnectFromServer(IntPtr tcp_ipClientObject);
        public bool DisconnectFromServer()
        {
            Log(GetLowLogLevel(), "CCSTCP_IPClient::DisconnectFromServer()");

            if (!DisconnectFromServer(m_TCP_IPClientObject))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::DisconnectFromServer() : Failed to disconnect from the server.");

                return false;
            }

            return true;
        }*/ 
        /*
	     * This disconnects from the server the client is currently connected to.
	     * This is only valid when connected to a server.
         * \param shutdownPrintServer - Whether to shutdown the server after disconnecting from it. This is only valid when connected to a server and when this client launched the server.
	     * \return Whether the disconnection was successful or not.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "disconnectFromServerFullWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool DisconnectFromServer(IntPtr tcp_ipClientObject, bool shutdownPrintServer);
        public bool DisconnectFromServer(bool shutdownPrintServer = true)
        {
            Log(GetLowLogLevel(), "CCSTCP_IPClient::DisconnectFromServer()");

            if (!DisconnectFromServer(m_handle, shutdownPrintServer))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::DisconnectFromServer() : Failed to disconnect from the server.");

                return false;
            }
            
            return true;
        }
	    /*
	     * This returns whether the client is currently connected to a server or not.
	     * \return Whether the client is currently connected to a server or not.
	     */
	    [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "isConnectedToServerWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool IsConnectedToServer(IntPtr tcp_ipClientObject);
        public bool IsConnectedToServer()
        {
            return IsConnectedToServer(m_handle);
        }


	    /*
	     * This sends a message to the server the client is connected to.
	     * This is only valid when connected to a server.
	     * \param message - The message to send.
	     * \param waitForComplete - If this is true the command will not return until the complete command associated with the command being sent is returned.
	     * \return Whether the send was successful or not.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "sendMessageFullWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SendMessageToServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string message, bool waitForComplete, IntPtr messageID, IntPtr commandSuccess, [MarshalAs(UnmanagedType.BStr)]ref string returnValue, int responseTimeout);
	    public bool SendMessage(string message, bool waitForComplete = true)
        {
            string returnValue = "";
            if (!SendMessageToServer(m_handle, message, waitForComplete, IntPtr.Zero, IntPtr.Zero, ref returnValue, -1))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SendMessage() : Failed to send message to server.");

                return false;
            }

            return true;
        }
        /*
         * This sends a message to the server the client is connected to.
         * As This function records the returned data, it will always wait until the complete message for the command sent is returned from the server.
         * This is only valid when connected to a server.
         * \param message - The message to send.
         * \param returnValue - If the command returns data, it will all be returned in this parameter.
         * \return Whether the send was successful or not.
         */
        public bool SendMessage(string message, ref string returnValue)
        {
            if (!SendMessageToServer(m_handle, message, true, IntPtr.Zero, IntPtr.Zero, ref returnValue, -1))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SendMessage() : Failed to send message to server.");

                return false;
            }

            return true;
        }
        /*
	     * This sends a message to the server the client is connected to.
	     * This is only valid when connected to a server.
	     * \param message - The message to send.
	     * \param waitForComplete - If this is true the command will not return until the complete command associated with the command being sent is returned.
         * \param messageID - The ID of the command that was sent will be returned in this parameter. This parameter should be NULL if the ID is not to be recorded. This is only relavent if waitForComplete is false.
         * \param commandSuccess - Whether the complete command associated with the command that was sent is returned in this parameter. This parameter should be NULL If This parameter is not to be recorded. This function will always return true if the command being sent has a non-successful error code.
         * \param returnValue - If the command returns data, it will all be returned in this parameter. This parameter should be NULL if nothing is to be returned or if the return values are not to be recorded.
         * \param responseTimeout - The amount of time (in milliseconds) that the function will wait for a complete command to be returned. This parameter should be set to -1 to have no timeout. This is only relavent if waitForComplete is true.
	     * \return Whether the send was successful or not.
	     */
        public bool SendMessage(string message, bool waitForComplete, int messageID, bool commandSuccess, ref string returnValue, int responseTimeout)
        {
            IntPtr messageIDPtr = new IntPtr();
            IntPtr commandSuccessPtr = new IntPtr();
            if (!SendMessageToServer(m_handle, message, waitForComplete, messageIDPtr, commandSuccessPtr, ref returnValue, -1))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SendMessage() : Failed to send message to server.");

                return false;
            }

            messageID = messageIDPtr.ToInt32();
            if (commandSuccessPtr.ToInt32() == 1)
            {
                commandSuccess = true;
            }
            else
            {
                commandSuccess = false;
            }

            return true;
        }
	    /*
	     * This sends a message to the server, but by creating a new client (using the same settings as the current client). This may be needed as the Print Server will only process once command from
	     * a client at a time, but some commands (such as aborts) may need to be sent in parallel.
	     * This is only valid when connected to a server.
	     * \param message - The message to send.
	     * \return Whether the send was successful or not.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "sendMessageInNewClientWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SendMessageInNewClientToServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string message, [MarshalAs(UnmanagedType.BStr)]ref string returnValue);
        public bool SendMessageInNewClient(string message)
        {
            string returnValue = "";
            if (!SendMessageInNewClientToServer(m_handle, message, ref returnValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SendMessageInNewClient() : Failed to send message to server.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends a message to the server, but by creating a new client (using the same settings as the current client). This may be needed as the Print Server will only process once command from
	     * a client at a time, but some commands (such as aborts) may need to be sent in parallel.
	     * This is only valid when connected to a server.
	     * \param message - The message to send.
	     * \param returnValue - If the command returns data, it will all be returned in this parameter.
	     * \return Whether the send was successful or not.
	     */
	    public bool SendMessageInNewClient(string message, ref string returnValue)
        {
            if (!SendMessageInNewClientToServer(m_handle, message, ref returnValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SendMessageInNewClient() : Failed send message to server.");

                return false;
            }

            return true;
        }
        /*
	     * This signals that the next message should be sent in a new client.
         * This is only valid when connected to a server.
	     * \return Whether the signal was sent successful or not.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "sendNextMessageInNewClientWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SendNextMessageInNewClientToServer(IntPtr tcp_ipClientObject);
        public bool SendNextMessageInNewClient()
        {
            if (!SendNextMessageInNewClientToServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SendNextMessageInNewClient() : Failed send the signal to server.");

                return false;
            }

            return true;
        }
        
	    /*
	     * This reads a response from the server the client is connected to.
	     * This is only valid when connected to a server.
	     * \param response - The data read will be returned in this parameter.
	     * \return Whether the read was successful or not.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "readFromServerWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ReadFromServerToServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.BStr)]ref string returnValue, int timeout);
        public bool ReadFromServer(ref string response)
        {
            if (!ReadFromServerToServer(m_handle, ref response, -1))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ReadFromServer() : Failed to read from server.");

                return false;
            }

            return true;
        }


	    /*
	     * This takes the data from a printhead status information message and returns a map which links the printhead name to the information.
	     * The IPrintheadInformationInterface objects returned must be deleted by the calling function.
	     * \param printheadStatusInformation - The printhead status information.
	     * \param printheadInformation - The printheads and associated information. The map is of the form <Printhead Name, Information>.
	     * \return Whether the parse was successful or not.
	     */
	    /*public bool ParsePrintheadInformation(string printheadStatusInformation, ref map<string, IPrintheadInformationInterface *> printheadInformation)
        {
            return true;
        } */
        
        
        /*
         * This upadtes the log settings.
         * \param logToFileEnabled - Whether logging to file is enabled or not.
         * \param logFile - The path to thr log file.
         * \param logToInterfaceEnabled - Whether messages should be logged to the interface or not.
         * \return Whether the log settings were updated successfully or not.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "updateLogSettingsWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool UpdateLogSettingsInServer(IntPtr tcp_ipClientObject, bool logToFileEnabled, [MarshalAs(UnmanagedType.LPWStr)]string logFile, bool logToInterfaceEnabled, bool extendedNetworkLogging);
        public bool UpdateLogSettings(bool logToFileEnabled, string logFile, bool logToInterfaceEnabled)
        {
            if (!UpdateLogSettingsInServer(m_handle, logToFileEnabled, logFile, logToInterfaceEnabled, false))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::UpdateLogSettings() : Failed to call InServer.");

                return false;
            }

            return true;
        }
        /*
         * This logs a message.
         * \param logLevel - The log level.
         * \param logMessage - The message to log.
         * \return Whether the log was successful or not.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "logWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool LogToServer(IntPtr tcp_ipClientObject, int logLevel, [MarshalAs(UnmanagedType.LPWStr)]string logMessage);
        public bool Log(int logLevel, string logMessage)
        {
            if (!LogToServer(m_handle, logLevel, logMessage))
            {
                return false;
            }

            return true;
        }
        /*
         * This returns the low log level.
         * \return The low log level.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getLowLogLevelWrapper")]
        private static extern int GetLowLogLevelFromServer(IntPtr tcp_ipClientObject);
        public int GetLowLogLevel()
        {
            return GetLowLogLevelFromServer(m_handle);
        }
        /*
         * This returns the warning log level.
         * \return The warning log level.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getWarningLogLevelWrapper")]
        private static extern int GetWarningLogLevelFromServer(IntPtr tcp_ipClientObject);
        public int GetWarningLogLevel()
        {
            return GetWarningLogLevelFromServer(m_handle);
        }
        /*
         * This returns the error log level.
         * \return The error log level.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getErrorLogLevelWrapper")]
        private static extern int GetErrorLogLevelFromServer(IntPtr tcp_ipClientObject);
        public int GetErrorLogLevel()
        {
            return GetErrorLogLevelFromServer(m_handle);
        }
        /*
         * This returns the UI info log level.
         * \return The UI info log level.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getUIInfoLogLevelWrapper")]
        private static extern int GetUIInfoLogLevelFromServer(IntPtr tcp_ipClientObject);
        public int GetUIInfoLogLevel()
        {
            return GetUIInfoLogLevelFromServer(m_handle);
        }
        /*
         * This returns the UI warning log level.
         * \return The UI warning log level.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getUIWarningLogLevelWrapper")]
        private static extern int GetUIWarningLogLevelFromServer(IntPtr tcp_ipClientObject);
        public int GetUIWarningLogLevel()
        {
            return GetUIWarningLogLevelFromServer(m_handle);
        }
        /*
         * This returns the UI error log level.
         * \return The UI error log level.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getUIErrorLogLevelWrapper")]
        private static extern int GetUIErrorLogLevelFromServer(IntPtr tcp_ipClientObject);
        public int GetUIErrorLogLevel()
        {
            return GetUIErrorLogLevelFromServer(m_handle);
        }

//    }
//
//    public class PrintServerClient: Client
//    {
	    /////////////////////////////////////
	    // Print Server
	    /////////////////////////////////////

	    /*
	     * This sends the command which initialises the Print Server with the configuration file specified.
	     * This is only valid when connected to a server.
	     * \param configurationFile - The configuration file the Prin Server will be initialised with.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "initialisePrintServerWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool InitialisePrintServerInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string configurationFile);
        public bool InitialisePrintServer(string configurationFile)
        {
            if (!InitialisePrintServerInServer(m_handle, configurationFile))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::InitialisePrintServer() : Failed to call InServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which waits for the Print Server to finish initialising.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "waitUntilPrintServerInitialisedWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool WaitUntilPrintServerInitialisedInServer(IntPtr tcp_ipClientObject);
        public bool WaitUntilPrintServerInitialised()
        {
            if (!WaitUntilPrintServerInitialisedInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::WaitUntilPrintServerInitialised() : Failed to call WaitUntilPrintServerInitialisedInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This sends the command which registers* This command for information from the Print Server.
	     * This is only valid when connected to a server.
	     * \param returnExistingInformation - Whether to return the existing information or not.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "registerForPrintServerInformationWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool RegisterForPrintServerInformationInServer(IntPtr tcp_ipClientObject, bool returnExistingInformation);
        private bool m_bRegisteredForPrintServerInformation = false;
        public bool RegisterForPrintServerInformation(bool returnExistingInformation)
        {
            if (!m_bRegisteredForPrintServerInformation)
            {
                m_bRegisteredForPrintServerInformation = RegisterForPrintServerInformationInServer(m_handle, returnExistingInformation);
                if (!m_bRegisteredForPrintServerInformation)
                {
                    Log(GetErrorLogLevel(), "CCSTCP_IPClient::SaveConfigurationAs() : Failed to call RegisterForPrintServerInformationInServer.");
                }
            }
            return m_bRegisteredForPrintServerInformation;
        }


	    /*
	     * This sends the command which saves the Print Server configuration to the folder specified.
	     * This is only valid when connected to a server.
	     * \param configurationFolder - The folder where the print server configuration will be saved.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "saveConfigurationAsWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SaveConfigurationAsInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string configurationFolder);
        public bool SaveConfigurationAs(string configurationFolder)
        {
            if (!SaveConfigurationAsInServer(m_handle, configurationFolder))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SaveConfigurationAs() : Failed to call SaveConfigurationAsInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which saves the Print Server configuration to the current folder.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "saveConfigurationWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SaveConfigurationInServer(IntPtr tcp_ipClientObject);
        public bool SaveConfiguration()
        {
            if (!SaveConfigurationInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SaveConfiguration() : Failed to call SaveConfigurationInServer.");

                return false;
            }

            return true;
        }


        /*
         * This sends the command which applys the system mode with the name specified.
         * This is only valid when connected to a server.
         * \param systemModeName - The name of the system mode to apply.
         * \param applySubModes - Whether to aply the sub modes or not.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "applySystemModeWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ApplySystemModeInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string systemModeName, bool applySubModes);
        public bool ApplySystemMode(string systemModeName, bool applySubModes)
        {
            if (!ApplySystemModeInServer(m_handle, systemModeName, applySubModes))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ApplySystemMode() : Failed to call ApplySystemModeInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which gets the name of all system modes.
	     * This is only valid when connected to a server.
	     * \param systemModeNames - The system mode names will be returned in this parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getSystemModeNamesWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetSystemModeNamesInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.BStr)]ref string systemModeNames);
        public bool GetSystemModeNames(ref string systemModeNames)
        {
            if (!GetSystemModeNamesInServer(m_handle, ref systemModeNames))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetSystemModeNames() : Failed to call GetSystemModeNamesInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which gets the information for the system mode specified.
	     * This is only valid when connected to a server.
	     * \param systemModeName - The name of the system mode to retrieve information for.
	     * \param renderModeName - The name of the render mode for the system mode specified is returned in this parameter.
	     * \param printModeName - The name of the print mode for the system mode specified is returned in this parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getSystemModeInformationWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetSystemModeInformationInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string systemModeName, [MarshalAs(UnmanagedType.BStr)]ref string renderModeName, [MarshalAs(UnmanagedType.BStr)]ref string printModeName);
        public bool GetSystemModeInformation(string systemModeName, ref string renderModeName, ref string printModeName)
        {
            if (!GetSystemModeInformationInServer(m_handle, systemModeName, ref renderModeName, ref printModeName))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetSystemModeInformation() : Failed to call GetSystemModeInformationInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which sets the parameter in the Print Server with the name specified to the
	     * value specified.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to change.
	     * \param parameterValue - The new value of the parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setPrintServerParameterValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetPrintServerParameterValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName, [MarshalAs(UnmanagedType.LPWStr)]string parameterValue);
        public bool SetPrintServerParameterValue(string parameterName, string parameterValue)
        {
            if (!SetPrintServerParameterValueInServer(m_handle, parameterName, parameterValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetPrintServerParameterValue() : Failed to call SetPrintServerParameterValueInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which sets a number of parameters in the Print Server at the same time.
	     * This is only valid when connected to a server.
	     * \param parameterNames - A collection of the parameter names to change. The parameters will be changed in the defined order.
	     * \param parameterValues - A collection of the new parameter values. Each entry corresponds to the equalivent entry in parameterNames and so the size of parameterValues must equal the size of parameterNames.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setPrintServerParameterValuesWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetPrintServerParameterValuesInServer(IntPtr tcp_ipClientObject, int numberOfParametersToChange, List<string> parameterNames, List<string> parameterValues);
        public bool SetPrintServerParameterValues(List<string> parameterNames, List<string> parameterValues)
        {
            if (!SetPrintServerParameterValuesInServer(m_handle, parameterNames.Count, parameterNames, parameterValues))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetPrintServerParameterValues() : Failed to call SetPrintServerParameterValuesInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This sends the command which sets the information about the Print Server node specified.
	     * This is only valid when connected to a server.
	     * \param nodePath - The name of the node to get the informtion for.
	     * \param nodeInformation - The information for the node.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setPrintServerNodeInformationWrapper", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetPrintServerNodeInformationInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string nodePath, [MarshalAs(UnmanagedType.LPWStr)]string nodeInformation);
        public bool SetPrintServerNodeInformation(string nodePath, string nodeInformation)
        {
            if (!SetPrintServerNodeInformationInServer(m_handle, nodePath, nodeInformation))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetPrintServerNodeInformation() : Failed to call SetPrintServerNodeInformation.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which gets the value of the parameter in the Print Server with the name specified.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to change.
	     * \param parameterValue - The value of the parameter is returned in this parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getPrintServerParameterValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetPrintServerParameterValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName, [MarshalAs(UnmanagedType.BStr)]ref string parameterValue);
        public bool GetPrintServerParameterValue(string parameterName, ref string parameterValue)
        {
            if (!GetPrintServerParameterValueInServer(m_handle, parameterName, ref parameterValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetPrintServerParameterValue() : Failed to call GetPrintServerParameterValueInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This sends the command which gets the information about the Print Server node specified.
	     * This is only valid when connected to a server.
	     * \param nodePath - The name of the node to get the informtion for.
	     * \param excludedNodeTypes - A comma separated list of node types to be excluded from the information.
	     * \param nodeInformation - The information for the node.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getPrintServerNodeInformationWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetPrintServerNodeInformationInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string nodePath, [MarshalAs(UnmanagedType.LPWStr)]string excludedNodeTypes, [MarshalAs(UnmanagedType.BStr)]ref string nodeInformation);
        public bool GetPrintServerNodeInformation(string nodePath, string excludedNodeTypes, ref string nodeInformation)
        {
            if (!GetPrintServerNodeInformationInServer(m_handle, nodePath, excludedNodeTypes, ref nodeInformation))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetPrintServerNodeInformation() : Failed to call GetPrintServerNodeInformation.");

                return false;
            }

            return true;
        }



        /*
	     * This sets the parameter in the paramter store with the name specified to the value given.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to set.
	     * \param parameterValue - The new value of the parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setParameterStoreValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetParameterStoreValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName, [MarshalAs(UnmanagedType.LPWStr)]string parameterValue);
        public bool SetParameterStoreValue(string parameterName, string parameterValue)
        {
            if (!SetParameterStoreValueInServer(m_handle, parameterName, parameterValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetParameterStoreValue() : Failed to call SetParameterStoreValueInServer.");

                return false;
            }

            return true;
        }

        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setParameterStoreValueByXmlWrapper", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool setParameterStoreValueByXmlInServer(IntPtr tcp_ipClientObject, string xmlString);
        public bool SetParameterStoreValueByXml(string xmlString)
        {
            if (!setParameterStoreValueByXmlInServer(m_handle, xmlString))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetParameterStoreValueByXml() : Failed to call SetParameterStoreValueInServer.");

                return false;
            }

            return true;
        }

        /*
	     * This gets the value of the parameter in the paramter store.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to get.
	     * \param parameterValue - The value of the parameter is returned in this parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getParameterStoreValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetParameterStoreValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName, [MarshalAs(UnmanagedType.BStr)]ref string parameterValue);
        public bool GetParameterStoreValue(string parameterName, ref string parameterValue)
        {
            if (!GetParameterStoreValueInServer(m_handle, parameterName, ref parameterValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetParameterStoreValue() : Failed to call GetParameterStoreValueInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This deletes the parameter in the paramter store with the name specified.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to delete.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "deleteParameterStoreValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool DeleteParameterStoreValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName);
        public bool DeleteParameterStoreValue(string parameterName)
        {
            if (!DeleteParameterStoreValueInServer(m_handle, parameterName))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::DeleteParameterStoreValue() : Failed to call DeleteParameterStoreValueInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which aborts the Print Server.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "abortOperationPrintServerWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AbortOperationPrintServerInServer(IntPtr tcp_ipClientObject);
        public bool AbortOperationPrintServer()
        {
            if (!AbortOperationPrintServerInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::AbortOperationPrintServer() : Failed to call AbortOperationPrintServerInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which initialises the Print Server.
	     * This is only valid when connected to a server.
	     * \param configurationFolder - The configuration folder used in the initialisation.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "initialiseOperationPrintServerWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool InitialiseOperationPrintServerInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string configurationFolder);
        public bool InitialiseOperationPrintServer(string configurationFolder)
        {
            if (!InitialiseOperationPrintServerInServer(m_handle, configurationFolder))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::InitialiseOperationPrintServer() : Failed to call InitialiseOperationPrintServerInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which shuts down the Print Server.
	     * This is only valid when connected to a server.
	     * \param pcOperationCode - The code of the operation to perform on the PC after the PC has finished shutting down.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "shutdownOperationPrintServerWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ShutdownOperationPrintServerInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string pcOperationCode);
        public bool ShutdownOperationPrintServer(string pcOperationCode)
        {
            if (!ShutdownOperationPrintServerInServer(m_handle, pcOperationCode))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ShutdownOperationPrintServer() : Failed to call ShutdownOperationPrintServerInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which gets information about the Print Server.
	     * This is only valid when connected to a server.
	     * \param systemInformation - The system information is returned in this parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getSystemInformationWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetSystemInformationInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.BStr)]ref string systemInformation);
        public bool GetSystemInformation(ref string systemInformation)
        {
            if (!GetSystemInformationInServer(m_handle, ref systemInformation))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetSystemInformation() : Failed to call GetSystemInformationInServer.");

                return false;
            }

            return true;
        }

//    }
//
//    public class RenderEngineClient: Client
//    {
	    /////////////////////////////////////
	    // Render Engine
	    /////////////////////////////////////

	    /*
	     * This sends the command which initialises the Render Engine with the configuration file specified.
	     * This is only valid when connected to a server.
	     * \param configurationFile - The configuration file the Render Engine will be initialised with.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "initialiseRenderEngineWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool InitialiseRenderEngineInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string configurationFile);
        public bool InitialiseRenderEngine(string configurationFile)
        {
            if (!InitialiseRenderEngineInServer(m_handle, configurationFile))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::InitialiseRenderEngine() : Failed to call InitialiseRenderEngineInServer.");

                return false;
            }

            return true;
        }

        /*
         * Sends a command to retrieve the current configuration used by the render engine
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "renderEngineGetConfigurationPathWrapper", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool RenderEngineGetConfigurationPath(IntPtr pClient, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pBuffer, int buflen);
        public string RenderEngineGetConfigPath()
        {
            StringBuilder buffer = new StringBuilder(1024);
            if (!RenderEngineGetConfigurationPath(m_handle, buffer, buffer.Capacity))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::RenderEngineGetConfigPath() : Failed to call RenderEngineGetConfigPath.");
                return null;
            }
            return buffer.ToString();
        }

	    /*
	     * This sends the command which registers* This command for information from the Render Engine.
	     * This is only valid when connected to a server.
	     * \param returnExistingInformation - Whether to return the existing information or not.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "registerForRenderEngineInformationWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool RegisterForRenderEngineInformationInServer(IntPtr tcp_ipClientObject, bool returnExistingInformation);
        private bool m_bRegisteredForRenderEngineInformation;
        public bool RegisterForRenderEngineInformation(bool returnExistingInformation)
        {
            if (!m_bRegisteredForRenderEngineInformation)
            {
                m_bRegisteredForRenderEngineInformation = RegisterForRenderEngineInformationInServer(m_handle, returnExistingInformation);
                if (!m_bRegisteredForRenderEngineInformation)
                {
                    Log(GetErrorLogLevel(), "CCSTCP_IPClient::RegisterForRenderEngineInformation() : Failed to call RegisterForRenderEngineInformationInServer.");
                }
            }
            return m_bRegisteredForRenderEngineInformation;
        }


	    /*
	     * This sends the command which applys the render mode with the name specified.
	     * This is only valid when connected to a server.
	     * \param renderMode - The name of the render mode to apply.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "applyRenderModeWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ApplyRenderModeInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string renderMode);
        public bool ApplyRenderMode(string renderMode)
        {
            if (!ApplyRenderModeInServer(m_handle, renderMode))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ApplyRenderMode() : Failed to call ApplyRenderModeInServer.");

                return false;
            }

            return true;
        }
        /*
         * This sends the command which gets the name of all render modes.
         * This is only valid when connected to a server.
         * \param renderModeNames - The render mode names will be returned in this parameter.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getRenderModeNamesWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetRenderModeNamesInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.BStr)]ref string renderModeNames);
        public bool GetRenderModeNames(ref string renderModeNames)
        {
            if (!GetRenderModeNamesInServer(m_handle, ref renderModeNames))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetRenderModeNames() : Failed to call GetRenderModeNamesInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This sends the command which sets the parameter in the Render Engine with the name specified to the
	     * value specified.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to change.
	     * \param parameterValue - The new value of the parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setRenderEngineParameterValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetRenderEngineParameterValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName, [MarshalAs(UnmanagedType.LPWStr)]string parameterValue);
        public bool SetRenderEngineParameterValue(string parameterName, string parameterValue)
        {
            if (!SetRenderEngineParameterValueInServer(m_handle, parameterName, parameterValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetRenderEngineParameterValue() : Failed to call SetRenderEngineParameterValueInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which sets a number of parameters in the Render Engine at the same time.
	     * This is only valid when connected to a server.
	     * \param parameterNames - A collection of the parameter names to change. The parameters will be changed in the defined order.
	     * \param parameterValues - A collection of the new parameter values. Each entry corresponds to the equalivent entry in parameterNames and so the size of parameterValues must equal the size of parameterNames.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setRenderEngineParameterValuesWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetRenderEngineParameterValuesInServer(IntPtr tcp_ipClientObject, List<string> parameterNames, List<string> parameterValues);
        public bool SetRenderEngineParameterValues(List<string> parameterNames, List<string> parameterValues)
        {
            if (!SetRenderEngineParameterValuesInServer(m_handle, parameterNames, parameterValues))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetRenderEngineParameterValues() : Failed to call SetRenderEngineParameterValuesInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This sends the command which sets the information about the Render Engine node specified.
	     * This is only valid when connected to a server.
	     * \param nodePath - The name of the node to get the informtion for.
	     * \param nodeInformation - The information for the node.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setRenderEngineNodeInformationWrapper", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetRenderEngineNodeInformationInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string nodePath, [MarshalAs(UnmanagedType.LPWStr)]string nodeInformation);
        public bool SetRenderEngineNodeInformation(string nodePath, string nodeInformation)
        {
            if (!SetRenderEngineNodeInformationInServer(m_handle, nodePath, nodeInformation))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetRenderEngineNodeInformation() : Failed to call SetPrintServerNodeInformation.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which gets the value of the parameter in the Render Engine with the name specified.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to change.
	     * \param parameterValue - The value of the parameter is returned in this parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getRenderEngineParameterValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetRenderEngineParameterValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName, [MarshalAs(UnmanagedType.BStr)]ref string parameterValue);
        public bool GetRenderEngineParameterValue(string parameterName, ref string parameterValue)
        {
            if (!GetRenderEngineParameterValueInServer(m_handle, parameterName, ref parameterValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetRenderEngineParameterValue() : Failed to call GetRenderEngineParameterValueInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This sends the command which gets the information about the Render Engine node specified.
	     * This is only valid when connected to a server.
	     * \param nodePath - The name of the node to get the informtion for.
	     * \param excludedNodeTypes - A comma separated list of node types to be excluded from the information.
	     * \param nodeInformation - The information for the node.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getRenderEngineNodeInformationWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetRenderEngineNodeInformationInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string nodePath, [MarshalAs(UnmanagedType.LPWStr)]string excludedNodeTypes, [MarshalAs(UnmanagedType.BStr)]ref string nodeInformation);
        public bool GetRenderEngineNodeInformation(string nodePath, string excludedNodeTypes, ref string nodeInformation)
        {
            if (!GetRenderEngineNodeInformationInServer(m_handle, nodePath, excludedNodeTypes, ref nodeInformation))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetRenderEngineNodeInformation() : Failed to call GetRenderEngineNodeInformation.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which aborts the Render Engine.
	     * This is only valid when connected to a server.
	     * \param sendInNewClient - If this is true, a new client connection will be made to the Print Server and the abort will be sent using this. This is necessary if a command from This client is already being processed by the Print Server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "abortRenderEngineWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AbortRenderEngineInServer(IntPtr tcp_ipClientObject, bool sendInNewClien);
        public bool AbortRenderEngine(bool sendInNewClient = false)
        {
            if (!AbortRenderEngineInServer(m_handle, sendInNewClient))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::AbortRenderEngine() : Failed to call AbortRenderEngineInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which loads the document at the path specified.
	     * This is only valid when connected to a server.
	     * \param documentPath - The path to the document to load.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "loadDocumentWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool LoadDocumentInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string documentPath);
        public bool LoadDocument(string documentPath)
        {
            if (!LoadDocumentInServer(m_handle, documentPath))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::LoadDocument() : Failed to call LoadDocumentInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This primes the currently loaded document for rendering.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "primeForRenderingWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool PrimeForRenderingInServer(IntPtr tcp_ipClientObject);
        public bool PrimeForRendering()
        {
            if (!PrimeForRenderingInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::PrimeForRendering() : Failed to call PrimeForRenderingInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This sends the command which renders the loaded document to a set of bitmaps.
	     * This can only be sent after a document has been loaded.
	     * This is only valid when connected to a server.
	     * \param renderOutputPath - The path to the base file name that the output will be saved to.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "startRenderWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool StartRenderInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string renderOutputPath);
        public bool StartRender(string renderOutputPath)
        {
            if (!StartRenderInServer(m_handle, renderOutputPath))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::StartRender() : Failed to call StartRenderInServer.");

                return false;
            }

            return true;
        }
        /*
         * This sends the command which renders a TIFF file to a set of bitmaps.
         * This is only valid when connected to a server.
         * \param tiffInputPath - The path to the input TIFF file.
         * \param bitmapOutputPath - The path to the base file name that the output will be saved to.
         * \param outputWidth - The width of the output (in pixels). Set this to -1.0 to use the width of the input TIFF file.
         * \param outputHeight - The height of the output (in pixels). Set this to -1.0 to use the width of the input TIFF file.
         * \param p_iPreviewWidth - The maximum width of the preview file (in pixels). Set this to -1 to use the default size.
         * \param p_iPreviewHeight - The maximum height of the preview file (in pixels). Set this to -1 to use the default size.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "renderBitmapWithPreviewWrapper", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool RenderBitmapInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string tiffInputPath, [MarshalAs(UnmanagedType.LPWStr)]string bitmapOutputPath, double outputWidth, double outputHeight, int previewWidth, int previewHeight);
        public bool RenderBitmap(string tiffInputPath, string bitmapOutputPath, double outputWidth = -1.0, double outputHeight = -1.0, int previewWidth = -1, int previewHeight = -1)
        {
            if (!RenderBitmapInServer(m_handle, tiffInputPath, bitmapOutputPath, outputWidth, outputHeight, previewWidth, previewHeight))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::RenderBitmap() : Failed to call RenderBitmapInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which renders the loaded document in a variable way.
	     * This can only be sent after a document has been loaded.
	     * This is only valid when connected to a server.
	     * \param startLabelNumber - The start label number of the render.
	     * \param numberOfLabels - The number of labels that will be rendered.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "renderLabelsWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool RenderLabelsInServer(IntPtr tcp_ipClientObject, int startLabelNumber, int numberOfLabels);
        public bool RenderLabels(int startLabelNumber, int numberOfLabels)
        {
            if (!RenderLabelsInServer(m_handle, startLabelNumber, numberOfLabels))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::RenderLabels() : Failed to call RenderLabelsInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This sends the command which starts the render on demand mode.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "startRenderOnDemandWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool StartRenderOnDemandInServer(IntPtr tcp_ipClientObject);
        public bool StartRenderOnDemand()
        {
            if (!StartRenderOnDemandInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::StartRenderOnDemand() : Failed to call StartRenderOnDemandInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the adds a render on demand item.
	     * This can only be sent while rendering on demand.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "addRenderonDemandItemWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AddRenderonDemandItemInServer(IntPtr tcp_ipClientObject);
        public bool AddRenderonDemandItem()
        {
            if (!AddRenderonDemandItemInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::AddRenderonDemandItem() : Failed to call AddRenderonDemandItemInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which ends the render on demand mode.
	     * This can only be sent while rendering on demand.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "endRenderOnDemandWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool EndRenderOnDemandInServer(IntPtr tcp_ipClientObject);
        public bool EndRenderOnDemand()
        {
            if (!EndRenderOnDemandInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::EndRenderOnDemand() : Failed to call EndRenderOnDemandInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This sends the command which changes the image file being loaded.
	     * This is only valid when connected to a server.
	     * \param imageFilePath - The path to the file to be loaded.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "changeImageFileWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ChangeImageFileInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string imageFilePath);
        public bool ChangeImageFile(string imageFilePath)
        {
            if (!ChangeImageFileInServer(m_handle, imageFilePath))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ChangeImageFile() : Failed to call ChangeImageFileInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which loads the image.
	     * This can only be sent after a file to load has been defined.
	     * This is only valid when connected to a server.
	     * \param numberOfTimesToLoadImage - The number of times to load the image.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "loadImageWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool LoadImageInServer(IntPtr tcp_ipClientObject, int numberOfTimesToLoadImage);
        public bool LoadImage(int numberOfTimesToLoadImage)
        {
            if (!LoadImageInServer(m_handle, numberOfTimesToLoadImage))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::LoadImage() : Failed to call LoadImageInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which starts the load on demand mode.
	     * This can only be sent after a file to load has been defined.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "startLoadOnDemandWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool StartLoadOnDemandInServer(IntPtr tcp_ipClientObject);
        public bool StartLoadOnDemand()
        {
            if (!StartLoadOnDemandInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::StartLoadOnDemand() : Failed to call StartLoadOnDemandInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which adds a load on demand mode image.
	     * This can only be sent while loading on demand.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "addLoadOnDemandImageWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AddLoadOnDemandImageInServer(IntPtr tcp_ipClientObject);
        public bool AddLoadOnDemandImage()
        {
            if (!AddLoadOnDemandImageInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::AddLoadOnDemandImage() : Failed to call AddLoadOnDemandImageInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which ends the load on demand mode.
	     * This can only be sent while loading on demand.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "endLoadOnDemandWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool EndLoadOnDemandInServer(IntPtr tcp_ipClientObject);
        public bool EndLoadOnDemand()
        {
            if (!EndLoadOnDemandInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::EndLoadOnDemand() : Failed to call EndLoadOnDemandInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This sends the command which increments the global counter.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "incrementGlobalCounterWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool IncrementGlobalCounterInServer(IntPtr tcp_ipClientObject);
        public bool IncrementGlobalCounter()
        {
            if (!IncrementGlobalCounterInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::IncrementGlobalCounter() : Failed to call IncrementGlobalCounterInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which dencrements the global counter.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "decrementGlobalCounterWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool DecrementGlobalCounterInServer(IntPtr tcp_ipClientObject);
        public bool DecrementGlobalCounter()
        {
            if (!DecrementGlobalCounterInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::DecrementGlobalCounter() : Failed to call DecrementGlobalCounterInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which sets the global counter to the value specified.
	     * This is only valid when connected to a server.
	     * \param globalCounter - The new value of the global counter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setGlobalCounterWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetGlobalCounterInServer(IntPtr tcp_ipClientObject, int globalCounter);
        public bool SetGlobalCounter(int globalCounter)
        {
            if (!SetGlobalCounterInServer(m_handle, globalCounter))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetGlobalCounter() : Failed to call SetGlobalCounterInServer.");

                return false;
            }

            return true;
        }


        /*
         * This sends the command which copies some RIP files from a location to another folder.
         * This is only valid when connected to a server.
         * \param ripFilesPath - The base path to the RIP files to copy. This is the base path, meaning the colour plane (e.g. _0) should not be included.
         * \param outputFolder - The path to the folder to copy the files into. If this is a drive letter or network name only, this will be combined with the p_pcRIPFilesPath parameter to form the whole path.
         * \param startColourPlane - The lowest colour plane file to copy.
         * \param endColourPlane - The largest colour plane file to copy. Set to -1 to use the largest colour plane file available.
         * \param deleteOriginalFiles - If true, the files that were copied will be deleted once the copy has finished.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "copyRIPFilesWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool CopyRIPFilesInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string ripFilesPath, [MarshalAs(UnmanagedType.LPWStr)]string outputFolder, int startColourPlane, int endColourPlane, bool deleteOriginalFiles);
        public bool CopyRIPFiles(string ripFilesPath, string outputFolder, int startColourPlane = 0, int endColourPlane =- 1, bool deleteOriginalFiles = false)
        {
            if (!CopyRIPFilesInServer(m_handle, ripFilesPath, outputFolder, startColourPlane, endColourPlane, deleteOriginalFiles))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::CopyRIPFiles() : Failed to call CopyRIPFiles.");

                return false;
            }

            return true;
        }
        

	    /*
	     * This sends the command which gets a preview of the loaded document.
	     * This is only valid when connected to a server.
	     * \param previewWidth - The width of the preview (in pixels).
	     * \param previewHeight - The height of the preview (in pixels).
	     * \param previewData - The preview data is returned in this parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getPreviewWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetPreviewInServer(IntPtr tcp_ipClientObject, int previewWidth, int previewHeight, [MarshalAs(UnmanagedType.BStr)]ref string previewData);
        public bool GetPreview(int previewWidth, int previewHeight, ref string previewData)
        {
            if (!GetPreviewInServer(m_handle, previewWidth, previewHeight, ref previewData))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetPreview() : Failed to call GetPreviewInServer.");

                return false;
            }

            return true;
        }


//    }
//    
//    public class PrintControllerClient: Client
//    {
        /////////////////////////////////////
	    // Print Controller
	    /////////////////////////////////////

	    /*
	     * This sends the command which initialises the Print Controller with the configuration file specified.
	     * This is only valid when connected to a server.
	     * \param configurationFile - The configuration file the Print Controller will be initialised with.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "initialisePrintControllerWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool InitialisePrintControllerInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string configurationFile);
        public bool InitialisePrintController(string configurationFile)
        {
            if (!InitialisePrintControllerInServer(m_handle, configurationFile))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::InitialisePrintController() : Failed to call InitialisePrintControllerInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This sends the command which registers* This command for information from the Print Controller.
	     * This is only valid when connected to a server.
	     * \param returnExistingInformation - Whether to return the existing information or not.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "registerForPrintControllerInformationWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool RegisterForPrintControllerInformationInServer(IntPtr tcp_ipClientObject, bool returnExistingInformation);
        private bool m_bRegisteredForPrintControllerInformation = false;
        public bool RegisterForPrintControllerInformation(bool returnExistingInformation)
        {
            if (!m_bRegisteredForPrintControllerInformation)
            {
                m_bRegisteredForPrintControllerInformation = RegisterForPrintControllerInformationInServer(m_handle, returnExistingInformation);
                if (!m_bRegisteredForPrintControllerInformation)
                {
                    Log(GetErrorLogLevel(), "CCSTCP_IPClient::RegisterForPrintControllerInformation() : Failed to call RegisterForPrintControllerInformationInServer.");
                }
            }
            return m_bRegisteredForPrintControllerInformation;
        }


	    /*
	     * This sends the command which applys the print mode with the name specified.
	     * This is only valid when connected to a server.
	     * \param printMode - The name of the print mode to apply.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "applyPrintModeWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ApplyPrintModeInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string printMode);
        public bool ApplyPrintMode(string printMode)
        {
            if (!ApplyPrintModeInServer(m_handle, printMode))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ApplyPrintMode() : Failed to call ApplyPrintModeInServer.");

                return false;
            }

            return true;
        }
        /*
         * This sends the command which gets the name of all print modes.
         * This is only valid when connected to a server.
         * \param printModeNames - The print mode names will be returned in this parameter.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getPrintModeNamesWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetPrintModeNamesInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.BStr)]ref string printModeNames);
        public bool GetPrintModeNames(ref string printModeNames)
        {
            if (!GetPrintModeNamesInServer(m_handle, ref printModeNames))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetPrintModeNames() : Failed to call GetPrintModeNamesInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This sends the command which sets the parameter in the Print Controller with the name specified to the
	     * value specified.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to change.
	     * \param parameterValue - The new value of the parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setPrintControllerParameterValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetPrintControllerParameterValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName, [MarshalAs(UnmanagedType.LPWStr)]string parameterValue);
        public bool SetPrintControllerParameterValue(string parameterName, string parameterValue)
        {
            if (!SetPrintControllerParameterValueInServer(m_handle, parameterName, parameterValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetPrintControllerParameterValue() : Failed to call SetPrintControllerParameterValueInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which sets a number of parameters in the Print Controller at the same time.
	     * This is only valid when connected to a server.
	     * \param parameterNames - A collection of the parameter names to change. The parameters will be changed in the defined order.
	     * \param parameterValues - A collection of the new parameter values. Each entry corresponds to the equalivent entry in parameterNames and so the size of parameterValues must equal the size of parameterNames.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setPrintControllerParameterValuesWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetPrintControllerParameterValuesInServer(IntPtr tcp_ipClientObject, List<string> parameterNames, List<string> parameterValues);
        public bool SetPrintControllerParameterValues(List<string> parameterNames, List<string> parameterValues)
        {
            if (!SetPrintControllerParameterValuesInServer(m_handle, parameterNames, parameterValues))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetPrintControllerParameterValues() : Failed to call SetPrintControllerParameterValuesInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This sends the command which sets the information about the Print Controller node specified.
	     * This is only valid when connected to a server.
	     * \param nodePath - The name of the node to get the informtion for.
	     * \param nodeInformation - The information for the node.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setPrintControllerNodeInformationWrapper", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetPrintControllerNodeInformationInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string nodePath, [MarshalAs(UnmanagedType.LPWStr)]string nodeInformation);
        public bool SetPrintControllerNodeInformation(string nodePath, string nodeInformation)
        {
            if (!SetPrintControllerNodeInformationInServer(m_handle, nodePath, nodeInformation))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetPrintControllerNodeInformation() : Failed to call SetPrintControllerNodeInformation.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which gets the value of the parameter in the Print Controller with the name specified.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to change.
	     * \param parameterValue - The value of the parameter is returned in this parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getPrintControllerParameterValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetPrintControllerParameterValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName, [MarshalAs(UnmanagedType.BStr)]out string parameterValue);
        public bool GetPrintControllerParameterValue(string parameterName, out string parameterValue)
        {
            if (!GetPrintControllerParameterValueInServer(m_handle, parameterName, out parameterValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetPrintControllerParameterValue() : Failed to call GetPrintControllerParameterValueInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This sends the command which gets the information about the Print Controller node specified.
	     * This is only valid when connected to a server.
	     * \param nodePath - The name of the node to get the informtion for.
	     * \param excludedNodeTypes - A comma separated list of node types to be excluded from the information.
	     * \param nodeInformation - The information for the node.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getPrintControllerNodeInformationWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetPrintControllerNodeInformationInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string nodePath, [MarshalAs(UnmanagedType.LPWStr)]string excludedNodeTypes, [MarshalAs(UnmanagedType.BStr)]ref string nodeInformation);
        public bool GetPrintControllerNodeInformation(string nodePath, string excludedNodeTypes, ref string nodeInformation)
        {
            if (!GetPrintControllerNodeInformationInServer(m_handle, nodePath, excludedNodeTypes, ref nodeInformation))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetPrintControllerNodeInformation() : Failed to call GetPrintControllerNodeInformationInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which aborts the Print Controller.
	     * This is only valid when connected to a server.
	     * \param sendInNewClient - If this is true, a new client connection will be made to the Print Server and the abort will be sent using this. This is necessary if a command from This client is already being processed by the Print Server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "abortPrintControllerWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AbortPrintControllerInServer(IntPtr tcp_ipClientObject, bool sendInNewClient);
        public bool AbortPrintController(bool sendInNewClient = false)
        {
            if (!AbortPrintControllerInServer(m_handle, sendInNewClient))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::AbortPrintController() : Failed to call AbortPrintControllerInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which prints an item.
	     * This is only valid when connected to a server.
	     * \param numberOfPrints - The number of times to print.
	     * \param ripFileItemPath - The path to the base file name for the item to print.
	     * \param numberOfCopies - The number of copies of the item in the print.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "printWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool PrintInServer(IntPtr tcp_ipClientObject, int numberOfPrints, [MarshalAs(UnmanagedType.LPWStr)]string ripFileItemPath, int numberOfCopies);
        public bool Print(int numberOfPrints, string ripFileItemPath, int numberOfCopies)
        {
            if (!PrintInServer(m_handle, numberOfPrints, ripFileItemPath, numberOfCopies))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::Print() : Failed to call PrintInServer.");

                return false;
            }

            return true;
        }
        /*
         * This sends the command which prints a number of items.
         * This is only valid when connected to a server.
         * \param numberOfPrints - The number of times to print.
         * \param ripFileItemPaths - A collection of paths to base file names to print. The files will be printed in the defined order.
         * \param numberOfCopies - A collection of the number of copies of base file name that will be printed. Each entry corresponds to the equalivent entry in ripFileItemPaths and so the size of ripFileItemPaths must equal the size of numberOfCopies.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "printWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool PrintInServer(IntPtr tcp_ipClientObject, int numberOfPrints, List<string> ripFileItemPaths, List<int> numberOfCopies);
        public bool Print(int numberOfPrints, List<string> ripFileItemPaths, List<int> numberOfCopies)
        {
            if (!PrintInServer(m_handle, numberOfPrints, ripFileItemPaths, numberOfCopies))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::Print() : Failed to call PrintInServer.");

                return false;
            }

            return true;
        }
        /*
         * This sends the command which initialises a print of an item.
         * This is only valid when connected to a server.
         * \param numberOfPrints - The number of times to print.
         * \param ripFileItemPaths - The path to the base file name for the item to print.
         * \param numberOfCopies - The number of copies of the item in the print.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "initialisePrintWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool InitialisePrintInServer(IntPtr tcp_ipClientObject, int numberOfPrints, [MarshalAs(UnmanagedType.LPWStr)]string ripFileItemPaths, int numberOfCopies);
        public bool InitialisePrint(int numberOfPrints, string ripFileItemPaths, int numberOfCopies)
        {
            if (!InitialisePrintInServer(m_handle, numberOfPrints, ripFileItemPaths, numberOfCopies))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::InitialisePrint() : Failed to call InitialisePrintInServer.");

                return false;
            }

            return true;
        }
        /*
         * This sends the command which initialises a print of a number of items.
         * This is only valid when connected to a server.
         * \param numberOfPrints - The number of times to print.
         * \param ripFileItemPaths - A collection of paths to base file names to print. The files will be printed in the defined order.
         * \param numberOfCopies - A collection of the number of copies of base file name that will be printed. Each entry corresponds to the equalivent entry in ripFileItemPaths and so the size of ripFileItemPaths must equal the size of numberOfCopies.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "initialisePrintWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool InitialisePrintInServer(IntPtr tcp_ipClientObject, int numberOfPrints, List<string> ripFileItemPaths, List<int> numberOfCopies);
        public bool InitialisePrint(int numberOfPrints, List<string> ripFileItemPaths, List<int> numberOfCopies)
        {
            if (!InitialisePrintInServer(m_handle, numberOfPrints, ripFileItemPaths, numberOfCopies))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::InitialisePrint() : Failed to call InitialisePrintInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which starts the initialised print.
	     * This can only be sent once the command to initialise a print has been sent.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "startInitialisedPrintWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool StartInitialisedPrintInServer(IntPtr tcp_ipClientObject);
        public bool StartInitialisedPrint()
        {
            if (!StartInitialisedPrintInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::StartInitialisedPrint() : Failed to call StartInitialisedPrintInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which does not return until the print has fully initialised.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "waitForPrintInitialisedWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool WaitForPrintInitialisedInServer(IntPtr tcp_ipClientObject);
        public bool WaitForPrintInitialised()
        {
            if (!WaitForPrintInitialisedInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::WaitForPrintInitialised() : Failed to call WaitForPrintInitialisedInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which does not return until the print has fully finished.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "waitForPrintFinishedWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool WaitForPrintFinishedInServer(IntPtr tcp_ipClientObject);
        public bool WaitForPrintFinished()
        {
            if (!WaitForPrintFinishedInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::WaitForPrintFinished() : Failed to call WaitForPrintFinishedInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This sends the command which starts the print queue mode.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "startPrintQueueModeWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool StartPrintQueueModeInServer(IntPtr tcp_ipClientObject);
        public bool StartPrintQueueMode()
        {
            if (!StartPrintQueueModeInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::StartPrintQueueMode() : Failed to call StartPrintQueueModeInServer.");

                return false;
            }

            return true;
        }
        /*
         * This sends the command which adds a print queue item.
         * This can only be sent when in print queue mode.
         * This is only valid when connected to a server.
         * \param ripFileItemPath - The path to the base file name for the print queue item to print.
         * \param numberOfCopies - The number of copies of the base file name in the print queue item.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "addPrintQueueItemWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AddPrintQueueItemInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string ripFileItemPath, int numberOfCopies);
        public bool AddPrintQueueItem(string ripFileItemPath, int numberOfCopies)
        {
            if (!AddPrintQueueItemInServer(m_handle, ripFileItemPath, numberOfCopies))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::AddPrintQueueItem() : Failed to call AddPrintQueueItemInServer.");

                return false;
            }

            return true;
        }
        /*
         * This sends the command which adds a print queue item with a number of file items.
         * This can only be sent when in print queue mode.
         * This is only valid when connected to a server.
         * \param ripFileItemPaths - A collection of paths to base file names to print. The files will be printed in the defined order.
         * \param numberOfCopies - A collection of the number of copies of base file name that will be printed. Each entry corresponds to the equalivent entry in ripFileItemPaths and so the size of ripFileItemPaths must equal the size of numberOfCopies.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "addPrintQueueItemWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AddPrintQueueItemInServer(IntPtr tcp_ipClientObject, List<string> ripFileItemPaths, List<int> numberOfCopies);
        public bool AddPrintQueueItem(List<string> ripFileItemPaths, List<int> numberOfCopies)
        {
            if (!AddPrintQueueItemInServer(m_handle, ripFileItemPaths, numberOfCopies))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::AddPrintQueueItem() : Failed to call AddPrintQueueItemInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This sends the command which adds a spit queue item.
	     * This can only be sent when in print queue mode.
	     * This is only valid when connected to a server.
	     * \param frequency - The frequency of the spit in Hertz i.e. the number of times the printheads will fire every second.
	     * \param duration - The duration of the spit (in seconds).
	     * \param greyLevel - The grey level to spit with. -1 will just use the default value in the config.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "addSpitQueueItemWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AddSpitQueueItemInServer(IntPtr tcp_ipClientObject, double frequency, double duration, int greyLevel);
        public bool AddSpitQueueItem(double frequency, double duration, int greyLevel)
        {
            if (!AddSpitQueueItemInServer(m_handle, frequency, duration, greyLevel))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::AddSpitQueueItem() : Failed to call AddSpitQueueItemInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which ends the print queue mode.
	     * This can only be sent when in print queue mode.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "endPrintQueueModeWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool EndPrintQueueModeInServer(IntPtr tcp_ipClientObject);
        public bool EndPrintQueueMode()
        {
            if (!EndPrintQueueModeInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::EndPrintQueueMode() : Failed to call EndPrintQueueModeInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This sends the command which pauses the print.
	     * This can only be sent when printing.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "pausePrintWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool PausePrintInServer(IntPtr tcp_ipClientObject);
        public bool PausePrint()
        {
            if (!PausePrintInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::PausePrint() : Failed to call PausePrintInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which resumes a paused print.
	     * This can only be sent when printing and after the print has been paused.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "resumePrintWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ResumePrintInServer(IntPtr tcp_ipClientObject);
        public bool ResumePrint()
        {
            if (!ResumePrintInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ResumePrint() : Failed to call ResumePrintInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This sends the command which sends a software print go to all connected PMBs.
	     * This can only be sent when printing.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "sendSoftwarePrintGoWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SendSoftwarePrintGoInServer(IntPtr tcp_ipClientObject);
        public bool SendSoftwarePrintGo()
        {
            if (!SendSoftwarePrintGoInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SendSoftwarePrintGo() : Failed to call SendSoftwarePrintGoInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which starts a spit in all ocnnected and enabled printheads.
	     * \param frequency - The frequency od the spit in Hertz i.e. the number of times the printheads will fire every second.
	     * \param duration - The duration of the spit (in seconds).
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "startSpitWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool StartSpitInServer(IntPtr tcp_ipClientObject, double frequency, double duration, int greylevel);
        public bool StartSpit(double frequency, double duration, int greylevel)
        {
            if (!StartSpitInServer(m_handle, frequency, duration, greylevel))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::StartSpit() : Failed to call StartSpitInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which stops the current spit.
	     * This can only be sent when spitting.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "stopSpitWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool StopSpitInServer(IntPtr tcp_ipClientObject);
        public bool StopSpit()
        {
            if (!StopSpitInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::StopSpit() : Failed to call StopSpitInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which waits for the spit to finish.
	     * This can only be sent when spitting.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "waitForSpitFinishedWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool WaitForSpitFinishedInServer(IntPtr tcp_ipClientObject);
        public bool WaitForSpitFinished()
        {
            if (!WaitForSpitFinishedInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::WaitForSpitFinished() : Failed to call WaitForSpitFinishedInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which deletes the print data directories specified.
	     * This is only valid when connected to a server.
	     * \param directories - The paths to the directories to delete. More than one directory can be specified as a comma separated list.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "deletePrintDataDirectoriesWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool DeletePrintDataDirectoriesInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string directories);
        public bool DeletePrintDataDirectories(string directories)
        {
            if (!DeletePrintDataDirectoriesInServer(m_handle, directories))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::DeletePrintDataDirectories() : Failed to call DeletePrintDataDirectoriesInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which deletes the print data files specified.
	     * This is only valid when connected to a server.
	     * \param filePaths - The paths to the files to delete. More than one file can be specified as a comma separated list.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "deletePrintDataFilesWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool DeletePrintDataFilesInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string filePaths);
        public bool DeletePrintDataFiles(string filePaths)
        {
            if (!DeletePrintDataFilesInServer(m_handle, filePaths))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::DeletePrintDataFiles() : Failed to call DeletePrintDataFilesInServer.");

                return false;
            }

            return true;
        }
	

	    /*
	     * This sends the command which jogs the transport mechanism.
	     * This is only valid when connected to a server.
	     * \param direction - The jog direction.
		 *					  0 – Zero X.
		 *					  1 – Limit X.
		 *					  2 – Zero Y.
		 *					  3 – Limit Y.
		 *					  4 - Zero Z.
		 *					  5 - Limit Z.
	     * \param speed - The jog speed.
		 *			      0 - Slow.
		 *				  1 - Fast.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "jogTransportMechanismWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool JogTransportMechanismInServer(IntPtr tcp_ipClientObject, int direction, int speed);
        public bool JogTransportMechanism(int direction, int speed)
        {
            if (!JogTransportMechanismInServer(m_handle, direction, speed))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::JogTransportMechanism() : Failed to call JogTransportMechanismInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which moves the transport mechanism to a pre-set position.
	     * This is only valid when connected to a server.
	     * \param destination - The destination.
		 *					    0 - Home.
		 *					    1 – Start position.
		 *					    2 – Load substrate position.
		 *					    3 – Maintenance position.
		 *					    4 – User position.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "moveTransportMechanismToDestinationWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool MoveTransportMechanismToDestinationInServer(IntPtr tcp_ipClientObject, int destination);
        public bool MoveTransportMechanismToDestination(int destination)
        {
            if (!MoveTransportMechanismToDestinationInServer(m_handle, destination))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::MoveTransportMechanismToDestination() : Failed to call MoveTransportMechanismToDestinationInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which moves the transport mechanism to the defined position.
	     * This is only valid when connected to a server.
	     * \param xPosition - The x position to move to (in mm).
	     * \param yPosition - The y position to move to (in mm).
	     * \param xSpeed - The x speed to move at (in mm/s).
	     * \param ySpeed - The y speed to move at (in mm/s).
	     * \param zPosition - The z position to move to (in mm). Set to -1.0 if there is no z-axis.
	     * \param zSpeed - The z speed to move at (in mm/s). Set to -1.0 if there is no z-axis.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "moveTransportMechanismToPositionWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool MoveTransportMechanismToPositionInServer(IntPtr tcp_ipClientObject, double xPosition, double yPosition, double xSpeed, double ySpeed, double zPosition, double zSpeed);
        public bool MoveTransportMechanismToPosition(double xPosition, double yPosition, double xSpeed, double ySpeed, double zPosition = -1.0, double zSpeed = -1.0)
        {
            if (!MoveTransportMechanismToPositionInServer(m_handle, xPosition, yPosition, xSpeed, ySpeed,  zPosition, zSpeed))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::MoveTransportMechanismToPosition() : Failed to call MoveTransportMechanismToPositionInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which turns purges the ink system.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "purgeInkSystemWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool PurgeInkSystemInServer(IntPtr tcp_ipClientObject);
        public bool PurgeInkSystem()
        {
            if (!PurgeInkSystemInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::PurgeInkSystem() : Failed to call PurgeInkSystemInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which turns the printheads on.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "turnPrintheadsOnWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool TurnPrintheadsOnInServer(IntPtr tcp_ipClientObject);
        public bool TurnPrintheadsOn()
        {
            if (!TurnPrintheadsOnInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::TurnPrintheadsOn() : Failed to call TurnPrintheadsOnInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which turns the printheads off.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "turnPrintheadsOffWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool TurnPrintheadsOffInServer(IntPtr tcp_ipClientObject);
        public bool TurnPrintheadsOff()
        {
            if (!TurnPrintheadsOffInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::TurnPrintheadsOff() : Failed to call TurnPrintheadsOffInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which sets the printhead target temperatures to the value specified.
	     * This is only valid when connected to a server.
	     * \param temperature - The new target temperature (in degrees celcius) of the printheads.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setPrintheadTargetTemperaturesWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetPrintheadTargetTemperaturesInServer(IntPtr tcp_ipClientObject, double temperature);
        public bool SetPrintheadTargetTemperatures(double temperature)
        {
            if (!SetPrintheadTargetTemperaturesInServer(m_handle, temperature))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetPrintheadTargetTemperatures() : Failed to call SetPrintheadTargetTemperaturesInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which cases the printhead status to be read.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "readPrintheadStatusWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ReadPrintheadStatusInServer(IntPtr tcp_ipClientObject);
        public bool ReadPrintheadStatus()
        {
            if (!ReadPrintheadStatusInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ReadPrintheadStatus() : Failed to call ReadPrintheadStatusInServer.");

                return false;
            }

            return true;
        }


        /*
	     * This sends the command which upgrades the head firmware.
	     * This is only valid when connected to a server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "upgradeHeadFirmwareWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool UpgradeHeadFirmwareInServer(IntPtr tcp_ipClientObject);
        public bool UpgradeHeadFirmware()
        {
            if (!UpgradeHeadFirmwareInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::UpgradeHeadFirmware() : Failed to call UpgradeHeadFirmwareInServer.");

                return false;
            }

            return true;
        }


        /*
         * This sends the command which gets the value of the node error state codes in the Print Controller for the
         * nodes with the name specified.
         * This is only valid when connected to a server.
         * \param nodeNames - The names of the nodes.
         * \param errorCodes - The error codes are returned in this parameter.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getPrintControllerNodeErrorStateWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetPrintControllerNodeErrorStateInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string nodeNames, [MarshalAs(UnmanagedType.BStr)]ref string errorCodes);
        public bool GetPrintControllerNodeErrorState(string nodeNames, ref string errorCodes)
        {
            if (!GetPrintControllerNodeErrorStateInServer(m_handle, nodeNames, ref errorCodes))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetPrintControllerNodeErrorState() : Failed to call GetPrintControllerNodeErrorStateInServer.");

                return false;
            }

            return true;
        }

//    }
//
//   public class NetworkControllerClient: Client
//    {
	    /////////////////////////////////////
	    // Network Controller
	    /////////////////////////////////////

	    /*
	     * This sends the command which initialises the Network Controller with the configuration file specified.
	     * This is only valid when connected to a server.
	     * \param configurationFile - The configuration file the Network Controller will be initialised with.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "initialiseNetworkControllerWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool InitialiseNetworkControllerInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string configurationFile);
        public bool InitialiseNetworkController(string configurationFile)
        {
            if (!InitialiseNetworkControllerInServer(m_handle, configurationFile))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::InitialiseNetworkController() : Failed to call InitialiseNetworkControllerInServer.");

                return false;
            }

            return true;
        }

	    /*
	     * This sends the command which registers* This command for information from the Network Controller.
	     * This is only valid when connected to a server.
	     * \param returnExistingInformation - Whether to return the existing information or not.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "registerForNetworkControllerInformationWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool RegisterForNetworkControllerInformationInServer(IntPtr handle, bool returnExistingInformation);
        private bool m_bRegisteredForNetworkControllerInformation = false;
        public bool RegisterForNetworkControllerInformation(bool returnExistingInformation)
        {
            if (!m_bRegisteredForNetworkControllerInformation)
            {
                m_bRegisteredForNetworkControllerInformation = RegisterForNetworkControllerInformationInServer(m_handle, returnExistingInformation);
                if (!m_bRegisteredForNetworkControllerInformation)
                {
                    Log(GetErrorLogLevel(), "CCSTCP_IPClient::RegisterForNetworkControllerInformation() : Failed to call RegisterForNetworkControllerInformationInServer.");
                }
            }
            return m_bRegisteredForNetworkControllerInformation;
        }


        /*
	     * This will not return until all servers have finished initialising.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "waitUntilAllSlaveServersInitialisedWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool WaitUntilAllSlaveServersInitialisedInServer(IntPtr tcp_ipClientObject);
        public bool WaitUntilAllSlaveServersInitialised()
        {
            if (!WaitUntilAllSlaveServersInitialisedInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::WaitUntilAllSlaveServersInitialised() : Failed to call WaitUntilAllSlaveServersInitialisedInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which sets the parameter in the Network Controller with the name specified to the
	     * value specified.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to change.
	     * \param parameterValue - The new value of the parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setNetworkControllerParameterValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetNetworkControllerParameterValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName, [MarshalAs(UnmanagedType.LPWStr)]string parameterValue);
        public bool SetNetworkControllerParameterValue(string parameterName, string parameterValue)
        {
            if (!SetNetworkControllerParameterValueInServer(m_handle, parameterName, parameterValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetNetworkControllerParameterValue() : Failed to call SetNetworkControllerParameterValueInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which sets a number of parameters in the Network Controller at the same time.
	     * This is only valid when connected to a server.
	     * \param parameterNames - A collection of the parameter names to change. The parameters will be changed in the defined order.
	     * \param parameterValues - A collection of the new parameter values. Each entry corresponds to the equalivent entry in parameterNames and so the size of parameterValues must equal the size of parameterNames.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setNetworkControllerParameterValuesWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetNetworkControllerParameterValuesInServer(IntPtr tcp_ipClientObject, List<string> parameterNames, List<string> parameterValues);
        public bool SetNetworkControllerParameterValues(List<string> parameterNames, List<string> parameterValues)
        {
            if (!SetNetworkControllerParameterValuesInServer(m_handle, parameterNames, parameterValues))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetNetworkControllerParameterValues() : Failed to call SetNetworkControllerParameterValuesInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This sends the command which sets the information about the Network Controller node specified.
	     * This is only valid when connected to a server.
	     * \param nodePath - The name of the node to get the informtion for.
	     * \param nodeInformation - The information for the node.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setNetworkControllerNodeInformationWrapper", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetNetworkControllerNodeInformationInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string nodePath, [MarshalAs(UnmanagedType.LPWStr)]string nodeInformation);
        public bool SetNetworkControllerNodeInformation(string nodePath, string nodeInformation)
        {
            if (!SetNetworkControllerNodeInformationInServer(m_handle, nodePath, nodeInformation))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetNetworkControllerNodeInformation() : Failed to call SetNetworkControllerNodeInformation.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which gets the value of the parameter in the Network Controller with the name specified.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to change.
	     * \param parameterValue - The value of the parameter is returned in this parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getNetworkControllerParameterValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetNetworkControllerParameterValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName, [MarshalAs(UnmanagedType.BStr)]ref string parameterValue);
        public bool GetNetworkControllerParameterValue(string parameterName, ref string parameterValue)
        {
            if (!GetNetworkControllerParameterValueInServer(m_handle, parameterName, ref parameterValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetNetworkControllerParameterValue() : Failed to call GetNetworkControllerParameterValueInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This sends the command which gets the information about the Network Controller node specified.
	     * This is only valid when connected to a server.
	     * \param nodePath - The name of the node to get the informtion for.
	     * \param excludedNodeTypes - A comma separated list of node types to be excluded from the information.
	     * \param nodeInformation - The information for the node.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getNetworkControllerNodeInformationWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetNetworkControllerNodeInformationInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string nodePath, [MarshalAs(UnmanagedType.LPWStr)]string excludedNodeTypes, [MarshalAs(UnmanagedType.BStr)]ref string nodeInformation);
        public bool GetNetworkControllerNodeInformation(string nodePath, string excludedNodeTypes, ref string nodeInformation)
        {
            if (!GetNetworkControllerNodeInformationInServer(m_handle, nodePath, excludedNodeTypes, ref nodeInformation))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetNetworkControllerNodeInformation() : Failed to call GetNetworkControllerNodeInformationInServer.");

                return false;
            }

            return true;
        }

                
        /*
         * This sends the command specified to the slave server specified.
         * This is only valid when connected to the slave server.
         * \param slaveServerName - The name of the slave server to send the message to.
         * \param messageToBroadcast - The message to send to the slave server.
         * \param returnValue - If the command returns data, it will all be returned in this parameter.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "broadcastMessageToSlaveServerWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool BroadcastMessageToSlaveServerInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string slaveServerName, [MarshalAs(UnmanagedType.LPWStr)]string messageToBroadcast, [MarshalAs(UnmanagedType.BStr)]ref string returnValue);
        public bool BroadcastMessageToSlaveServer(string slaveServerName, string messageToBroadcast, ref string returnValue)
        {
            if (!BroadcastMessageToSlaveServerInServer(m_handle, slaveServerName, messageToBroadcast, ref returnValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::BroadcastMessageToSlaveServer() : Failed to call BroadcastMessageToSlaveServerInServer.");

                return false;
            }

            return true;
        }
        /*
         * This sends the command specified to all of the slave servers.
         * This is only valid when connected to the slave servers.
         * \param messageToBroadcast - The message to send to the slave server.
         * \param returnValue - If no slave servers fail, the return data is contained in this parameter, otherwise the names of the slave servers which failed to execute the broadcast message are returned in this parameter as a comma separated list.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "broadcastMessageToAllSlaveServersWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool BroadcastMessageToAllSlaveServersInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string messageToBroadcast, [MarshalAs(UnmanagedType.BStr)]ref string returnValue);
        public bool BroadcastMessageToAllSlaveServers(string messageToBroadcast, ref string returnValue)
        {
            if (!BroadcastMessageToAllSlaveServersInServer(m_handle, messageToBroadcast, ref returnValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::BroadcastMessageToAllSlaveServers() : Failed to call BroadcastMessageToAllSlaveServersInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This indicates that the next message sent should actually be sent as a Network Controller broadcast message to a specified slave server.
	     * This is only valid when connected to the slave servers.
	     * \param tcp_ipClientObject - The CTCP_IPClientWrapper object used to call the function.
	     * \param slaveServerName - The name of the slave server to send the message to.
	     * \return Whether the signal was recorded successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "broadcastNextMessageToSlaveServerWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool BroadcastNextMessageToSlaveServerInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string slaveServerName);
        public bool BroadcastNextMessageToSlaveServer(string slaveServerName)
        {
            if (!BroadcastNextMessageToSlaveServerInServer(m_handle, slaveServerName))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::BroadcastNextMessageToSlaveServer() : Failed to call BroadcastNextMessageToSlaveServerInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This indicates that the next message sent should actually be sent as a Network Controller broadcast message to all slave servers.
	     * This is only valid when connected to the slave servers.
	     * \param tcp_ipClientObject - The CTCP_IPClientWrapper object used to call the function.
	     * \return Whether the signal was recorded successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "broadcastNextMessageToAllSlaveServersWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool BroadcastNextMessageToAllSlaveServersInServer(IntPtr tcp_ipClientObject);
        public bool BroadcastNextMessageToAllSlaveServers()
        {
            if (!BroadcastNextMessageToAllSlaveServersInServer(m_handle))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::BroadcastNextMessageToAllSlaveServers() : Failed to call BroadcastNextMessageToAllSlaveServersInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which aborts the Network Controller.
	     * This is only valid when connected to a server.
	     * \param sendInNewClient - If this is true, a new client connection will be made to the Print Server and the abort will be sent using this. This is necessary if a command from This client is already being processed by the Print Server.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "abortNetworkControllerWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AbortNetworkControllerInServer(IntPtr tcp_ipClientObject, bool sendInNewClient);
        public bool AbortNetworkController(bool sendInNewClient = false)
        {
            if (!AbortNetworkControllerInServer(m_handle, sendInNewClient))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::AbortNetworkController() : Failed to call AbortNetworkControllerInServer.");

                return false;
            }

            return true;
        }

//    }
//
//    public  class PrintServerMonitorClient: Client
//    {
	    /////////////////////////////////////
	    // Print Server Monitor
	    /////////////////////////////////////

	    /*
	     * This sends the command which initialises the Print Server Monitor with the configuration file specified.
	     * This is only valid when connected to a server.
	     * \param configurationFile - The configuration file the Render Engine will be initialised with.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "initialisePrintServerMonitorWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool InitialisePrintServerMonitorInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string configurationFile);
        public bool InitialisePrintServerMonitor(string configurationFile)
        {
            if (!InitialisePrintServerMonitorInServer(m_handle, configurationFile))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::InitialisePrintServerMonitor() : Failed to call InitialisePrintServerMonitorInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which sets the parameter in the Print Server Monitor with the name specified to the
	     * value specified.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to change.
	     * \param parameterValue - The new value of the parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setPrintServerMonitorParameterValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetPrintServerMonitorParameterValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName, [MarshalAs(UnmanagedType.LPWStr)]string parameterValue);
        public bool SetPrintServerMonitorParameterValue(string parameterName, string parameterValue)
        {
            if (!SetPrintServerMonitorParameterValueInServer(m_handle, parameterName, parameterValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetPrintServerMonitorParameterValue() : Failed to call SetPrintServerMonitorParameterValueInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which sets a number of parameters in the Print Server Monitor at the same time.
	     * This is only valid when connected to a server.
	     * \param parameterNames - A collection of the parameter names to change. The parameters will be changed in the defined order.
	     * \param parameterValues - A collection of the new parameter values. Each entry corresponds to the equalivent entry in parameterNames and so the size of parameterValues must equal the size of parameterNames.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setPrintServerMonitorParameterValuesWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetPrintServerMonitorParameterValuesInServer(IntPtr tcp_ipClientObject, List<string> parameterNames, List<string> parameterValues);
        public bool SetPrintServerMonitorParameterValues(List<string> parameterNames, List<string> parameterValues)
        {
            if (!SetPrintServerMonitorParameterValuesInServer(m_handle, parameterNames, parameterValues))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetPrintServerMonitorParameterValues() : Failed to call SetPrintServerMonitorParameterValuesInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This sends the command which sets the information about the Print Server Monitor node specified.
	     * This is only valid when connected to a server.
	     * \param nodePath - The name of the node to get the informtion for.
	     * \param nodeInformation - The information for the node.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "setPrintServerMonitorNodeInformationWrapper", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetPrintServerMonitorNodeInformationInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string nodePath, [MarshalAs(UnmanagedType.LPWStr)]string nodeInformation);
        public bool SetPrintServerMonitorNodeInformation(string nodePath, string nodeInformation)
        {
            if (!SetPrintServerMonitorNodeInformationInServer(m_handle, nodePath, nodeInformation))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::SetPrintServerMonitorNodeInformation() : Failed to call SetPrintServerMonitorNodeInformation.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which gets the value of the parameter in the Print Server Monitor with the name specified.
	     * This is only valid when connected to a server.
	     * \param parameterName - The name of the parameter to change.
	     * \param parameterValue - The value of the parameter is returned in this parameter.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getPrintServerMonitorParameterValueWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetPrintServerMonitorParameterValueInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string parameterName, [MarshalAs(UnmanagedType.BStr)]ref string parameterValue);
        public bool GetPrintServerMonitorParameterValue(string parameterName, ref string parameterValue)
        {
            if (!GetPrintServerMonitorParameterValueInServer(m_handle, parameterName, ref parameterValue))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetPrintServerMonitorParameterValue() : Failed to call GetPrintServerMonitorParameterValueInServer.");

                return false;
            }

            return true;
        }
        /*
	     * This sends the command which gets the information about the Print Server Monitor node specified.
	     * This is only valid when connected to a server.
	     * \param nodePath - The name of the node to get the informtion for.
	     * \param excludedNodeTypes - A comma separated list of node types to be excluded from the information.
	     * \param nodeInformation - The information for the node.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "getPrintServerMonitorNodeInformationWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool GetPrintServerMonitorNodeInformationInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string nodePath, [MarshalAs(UnmanagedType.LPWStr)]string excludedNodeTypes, [MarshalAs(UnmanagedType.BStr)]ref string nodeInformation);
        public bool GetPrintServerMonitorNodeInformation(string nodePath, string excludedNodeTypes, ref string nodeInformation)
        {
            if (!GetPrintServerMonitorNodeInformationInServer(m_handle, nodePath, excludedNodeTypes, ref nodeInformation))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::GetPrintServerMonitorNodeInformation() : Failed to call GetPrintServerMonitorNodeInformationInServer.");

                return false;
            }

            return true;
        }


	    /*
	     * This sends the command which aborts the Print Server Monitor.
	     * This is only valid when connected to a server.
	     * \param sendInNewClient - If this is true, a new client connection will be made to the Print Server Monitor and the abort will be sent using this. This is necessary if a command from This client is already being processed by the Print Server Monitor.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "abortPrintServerMonitorWrapper")]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool AbortPrintServerMonitorInServer(IntPtr tcp_ipClientObject, bool sendInNewClient);
        public bool AbortPrintServerMonitor(bool sendInNewClient = false)
        {
            if (!AbortPrintServerMonitorInServer(m_handle, sendInNewClient))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::AbortPrintServerMonitor() : Failed to call AbortPrintServerMonitorInServer.");

                return false;
            }

            return true;
        }


        /*
         * This sends the command which launches a Print Server with the properties specified.
         * This is only valid when connected to a print server monitor.
         * \param printServerName - The name of the server.
         * \param configurationPath - The configuration the server will use.
         * \param port - The TCP/IP port the server will run on.
         * \param startMinimised - Whether the server should start as minimised or not.
         * \param connectedToPMB - Whether this server will communicate with a PMB or not.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "launchPrintServerMonitorPrintServerWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool LaunchPrintServerMonitorPrintServerInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.BStr)]string printServerName, [MarshalAs(UnmanagedType.BStr)]string configurationPath, int port, bool startMinimised, bool connectedToPMB);
        public bool LaunchPrintServerMonitorPrintServer(string printServerName, string configurationPath, ushort port, bool startMinimised, bool connectedToPMB)
        {
            if (!LaunchPrintServerMonitorPrintServerInServer(m_handle, printServerName, configurationPath, port, startMinimised, connectedToPMB))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::LaunchPrintServerMonitorPrintServer() : Failed to call LaunchPrintServerMonitorPrintServerInServer.");

                return false;
            }

            return true;
        }
        /*
         * This sends the command which launches Print Servers with the properties specified.
         * This is only valid when connected to a print server monitor.
         * \param printServerNames - The names of the servers.
         * \param configurationPaths - The configurations the servers will use.
         * \param ports - The TCP/IP ports the servers will run on.
         * \param startMinimised - Whether the servers should start as minimised or not.
         * \param connectedToPMB - Whether this server will communicate with a PMB or not.
         * \return Whether the command executed successfully.
         */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "launchPrintServerMonitorPrintServersWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool LaunchPrintServerMonitorPrintServersInServer(IntPtr tcp_ipClientObject, int numberOfPrintServers, List<string> printServerNames, List<string> configurationPaths, List<int> ports, List<bool> startMinimised, List<bool> connectedToPMB);
        public bool LaunchPrintServerMonitorPrintServers(List<string> printServerNames, List<string> configurationPaths, List<int> ports, List<bool> startMinimised, List<bool> connectedToPMB)
        {
            if (!LaunchPrintServerMonitorPrintServersInServer(m_handle, printServerNames.Count, printServerNames, configurationPaths, ports, startMinimised, connectedToPMB))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::LaunchPrintServerMonitorPrintServers() : Failed to call LaunchPrintServerMonitorPrintServersInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which starts the Print Server with the name specified.
	     * This is only valid when connected to a server.
	     * \param printServerName - The name of the Print Server to start.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "startPrintServerMonitorPrintServerWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool StartPrintServerMonitorPrintServerInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string printServerName);
        public bool StartPrintServerMonitorPrintServer(string printServerName)
        {
            if (!StartPrintServerMonitorPrintServerInServer(m_handle, printServerName))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::StartPrintServerMonitorPrintServer() : Failed to call StartPrintServerMonitorPrintServerInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which shuts down the Print Server with the name specified.
	     * This is only valid when connected to a server.
	     * \param printServerName - The name of the Print Server to shutdown.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "shutdownPrintServerMonitorPrintServerWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ShutdownPrintServerMonitorPrintServerInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string printServerName);
        public bool ShutdownPrintServerMonitorPrintServer(string printServerName)
        {
            if (!ShutdownPrintServerMonitorPrintServerInServer(m_handle,  printServerName))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::ShutdownPrintServerMonitorPrintServer() : Failed to call ShutdownPrintServerMonitorPrintServerInServer.");

                return false;
            }

            return true;
        }
	    /*
	     * This sends the command which restarts the Print Server with the name specified.
	     * This is only valid when connected to a server.
	     * \param printServerName - The name of the Print Server to restart.
	     * \return Whether the command executed successfully.
	     */
        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "restartPrintServerMonitorPrintServerWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool RestartPrintServerMonitorPrintServerInServer(IntPtr tcp_ipClientObject, [MarshalAs(UnmanagedType.LPWStr)]string printServerName);
        public bool RestartPrintServerMonitorPrintServer(string printServerName)
        {
            if (!RestartPrintServerMonitorPrintServerInServer(m_handle, printServerName))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::RestartPrintServerMonitorPrintServer() : Failed to call RestartPrintServerMonitorPrintServerInServer.");

                return false;
            }

            return true;
        }

//    }
//
//    public class JobManagerClient: Client
//    {

        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "jobManagerSubmitJobWrapper", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
        private static extern bool JobManagerSubmitJobInServer(IntPtr tcp_ipClientObject, int iNumberOfCopies, [MarshalAs(UnmanagedType.LPWStr)]string fileName, int iStartLabel, int iNumberOfLabels, [MarshalAs(UnmanagedType.LPWStr)]string systemMode, ref int JobID);
        public bool JobManagerSubmitJob(int numberOfCopies, string fileName, int startLabel, int LabelCount, string systemMode, ref int JobID)
        {
            if (!JobManagerSubmitJobInServer(m_handle, numberOfCopies, fileName, startLabel, LabelCount, systemMode ?? "", ref JobID))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::JobManagerSubmitJob() : Failed to call JobManagerSubmitJobInServer.");

                return false;
            }

            return true;
        }

        [DllImport("GIS Utility - TCP-IP Comms V2.dll", EntryPoint = "jobManagerDeleteJobWrapper", CharSet = CharSet.Unicode)]
        private static extern bool JobManagerDeleteJobInServer(IntPtr tcp_ipClientObject,int JobID);
        public bool JobManagerDeleteJob(int JobID)
        {
            if (!JobManagerDeleteJobInServer(m_handle, JobID))
            {
                Log(GetErrorLogLevel(), "CCSTCP_IPClient::JobManagerSubmitJob() : Failed to call JobManagerDeleteJobInServer.");

                return false;
            }

            return true;
        }




        [DllImport("GIS Utility - TCP-IP Comms.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool sendMessageVBWrapper(IntPtr tcpObject, string message, ref IntPtr returnValue);



        [DllImport("GIS Utility - TCP-IP Comms.dll",
 CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool createVBTCP_IPClient(
     ref IntPtr p_pvTCP_IPClientObject);



        public bool sendMessageToServer(string p_sMessage)
        {
            if (m_pvTCP_IPClientObject == IntPtr.Zero)
            {
                MessageBox.Show("Failed to send message to the server as no TCP/IP comms object created");
                return false;
            }

            try
            {
                IntPtr sReturn = IntPtr.Zero;

                bool bSuccess = sendMessageVBWrapper(m_pvTCP_IPClientObject, p_sMessage, ref sReturn);

                
                if (bSuccess && sReturn != IntPtr.Zero)
                {
                    string p_sReturn = Marshal.PtrToStringBSTR(sReturn);
                    Marshal.FreeBSTR(sReturn);
                }

                if (!bSuccess)
                {
                    MessageBox.Show("Failed to send message to the server");
                    return false;
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            return true;
        }


        public bool sendMessageToServerForStop(string p_sMessage)
        {
            if (m_pvTCP_IPClientObject == IntPtr.Zero)
                return false;

            IntPtr sReturn = IntPtr.Zero;

            try
            {
                bool bSuccess = sendMessageVBWrapper(
                    m_pvTCP_IPClientObject,
                    p_sMessage,
                    ref sReturn);

                return bSuccess;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (sReturn != IntPtr.Zero)
                    Marshal.FreeBSTR(sReturn);
            }
        }




        public bool sendMessageToServerForStart(string p_sMessage)
        {
            if (m_pvTCP_IPClientObject == IntPtr.Zero)
                return false;

            IntPtr sReturn = IntPtr.Zero;
            try
            {
                bool bSuccess = sendMessageVBWrapper(
                    m_pvTCP_IPClientObject,
                    p_sMessage,
                    ref sReturn);
                return bSuccess;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (sReturn != IntPtr.Zero)
                    Marshal.FreeBSTR(sReturn);
            }
        }






        public bool sendMessageToServer(string p_sMessage, ref string p_sReturn)
        {
            if (m_pvTCP_IPClientObject == IntPtr.Zero)
            {
                MessageBox.Show("Failed to send message to the server as no TCP/IP comms object created");
                return false;
            }

            try
            {
                IntPtr sReturn = IntPtr.Zero;

                bool bSuccess = sendMessageVBWrapper(
                                    m_pvTCP_IPClientObject,
                                    p_sMessage,
                                    ref sReturn);

                if (bSuccess && sReturn != IntPtr.Zero)
                {
                    p_sReturn = Marshal.PtrToStringBSTR(sReturn);
                    Marshal.FreeBSTR(sReturn);
                }

                if (!bSuccess)
                {
                    MessageBox.Show("Failed to send message to the server");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            return true;
        }



        [DllImport("GIS Utility - TCP-IP Comms.dll",
    CallingConvention = CallingConvention.StdCall,
    CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool connectToServerVBWrapper(
    IntPtr p_pvTCP_IPClientObject,
    string p_sAddress,
    int p_iPort,
    int p_iConnectionTimeout);


        [DllImport("GIS Utility - TCP-IP Comms.dll",
    CallingConvention = CallingConvention.StdCall,
    CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool registerForPrintServerInformationVBWrapper(
    IntPtr p_pvTCP_IPClientObject,
    [MarshalAs(UnmanagedType.Bool)] bool p_bReturnExistingInformation);


        [DllImport("GIS Utility - TCP-IP Comms.dll",
    CallingConvention = CallingConvention.StdCall,
    CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool sendMessageFullVBWrapper(
    IntPtr p_pvTCP_IPClientObject,
    string p_sMessage,
    [MarshalAs(UnmanagedType.Bool)] bool p_bWaitForComplete,
    ref int p_piMessageID,
    ref bool p_pbCommandSuccess,
    ref IntPtr p_psReturn,
    int p_iResponseTimeout);



        [DllImport("GIS Utility - TCP-IP Comms.dll",
    CallingConvention = CallingConvention.StdCall,
    CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool waitForCompleteVBWrapper(
    IntPtr p_pvTCP_IPClientObject,
    int p_iMessageID,
    ref IntPtr p_psReturn,
    ref bool p_pbCommandSuccess,
    int p_iResponseTimeout);


        [DllImport("GIS Utility - TCP-IP Comms.dll",
    CallingConvention = CallingConvention.StdCall,
    CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool getNumberOfIndividualResponsesVBWrapper(
    IntPtr p_pvTCP_IPClientObject,
    string p_sCombinedResponses,
    ref int p_piNumberOfResponses);




        [DllImport("GIS Utility - TCP-IP Comms.dll",
    CallingConvention = CallingConvention.StdCall,
    CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool getResponseAtIndexVBWrapper(
    IntPtr p_pvTCP_IPClientObject,
    string p_sCombinedResponses,
    int p_iResponseIndex,
    ref IntPtr p_psResponse);






   



        public bool ConnectToServer(string p_sAddress, int p_iPort, int p_iConnectionTimeout)
        {
            // Fail if the TCP_IPClient object has not been created.
            if (m_pvTCP_IPClientObject == IntPtr.Zero)
            {
                MessageBox.Show("Failed to connect to the server as no TCP/IP comms object created");
                return false;
            }

            // Call the connect VB wrapper function in the DLL.
            try
            {
                if (connectToServerVBWrapper(m_pvTCP_IPClientObject, p_sAddress, p_iPort, p_iConnectionTimeout) == false)
                {
                    MessageBox.Show("Failed to connect to the server");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            // Register for Print Server information so that the UI log messages are returned.
            try
            {
                if (registerForPrintServerInformationVBWrapper(m_pvTCP_IPClientObject, true) == false)
                {
                    MessageBox.Show("Failed to connect to the server");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            return true;
        }



        public string testexampleEncoder(string tmpcommand)
        {
            string sMessage1 = "P,V,P," + tmpcommand;

            int iMessage1ID = 0;
            string sMessage1Return = "";
            bool bMessage1CommandSuccess = true;

            IntPtr sReturn = IntPtr.Zero;

            bool bSuccess = sendMessageFullVBWrapper(
                m_pvTCP_IPClientObject,
                sMessage1,
                false,
                ref iMessage1ID,
                ref bMessage1CommandSuccess,
                ref sReturn,
                -1);

            if (!bSuccess)
                return null;

            if (sReturn != IntPtr.Zero)
            {
                Marshal.FreeBSTR(sReturn);
                sReturn = IntPtr.Zero;
            }

            bSuccess = waitForCompleteVBWrapper(
                m_pvTCP_IPClientObject,
                iMessage1ID,
                ref sReturn,
                ref bMessage1CommandSuccess,
                -1);

            if (sReturn != IntPtr.Zero)
            {
                sMessage1Return = Marshal.PtrToStringBSTR(sReturn);
                Marshal.FreeBSTR(sReturn);
                sReturn = IntPtr.Zero;
            }

            if (!bSuccess || !bMessage1CommandSuccess)
                return null;

            int iNumberOfResponsesFromMessage1 = 0;

            if (!getNumberOfIndividualResponsesVBWrapper(
                    m_pvTCP_IPClientObject,
                    sMessage1Return,
                    ref iNumberOfResponsesFromMessage1))
                return null;

            string encoderSpeed = null;

            for (int i = 0; i < iNumberOfResponsesFromMessage1; i++)
            {
                bSuccess = getResponseAtIndexVBWrapper(
                    m_pvTCP_IPClientObject,
                    sMessage1Return,
                    i,
                    ref sReturn);

                if (!bSuccess)
                    return null;

                string sResponse = "";

                if (sReturn != IntPtr.Zero)
                {
                    sResponse = Marshal.PtrToStringBSTR(sReturn);
                    Marshal.FreeBSTR(sReturn);
                    sReturn = IntPtr.Zero;
                }

                if (i == 0)
                {
                    // ✅ No Sleep
                    encoderSpeed = ExtractSpeedValue(sResponse);
                }
            }

            return encoderSpeed;
        }


        private string ExtractSpeedValue(string response)
        {
            if (string.IsNullOrEmpty(response))
                return null;

            // Example response:
            // "0, total responses =1 ,index =0 , value = 125"

            if (response.Contains("value"))
            {
                var parts = response.Split('=');
                if (parts.Length > 1)
                    return parts[parts.Length - 1].Trim();
            }

            return response.Trim();
        }






        public string testexample(string tmpcommand)
        {
            string sMessage1 = "P,V,P," + tmpcommand;

            int iMessage1ID = 0;
            string sMessage1Return = "";
            bool bMessage1CommandSuccess = true;
                
            IntPtr sReturn = IntPtr.Zero;
            string finalValue = "";

            bool bSuccess = sendMessageFullVBWrapper(
                m_pvTCP_IPClientObject,
                sMessage1,
                false,
                ref iMessage1ID,
                ref bMessage1CommandSuccess,   // ✅ FIXED
                ref sReturn,
                -1);

            if (!bSuccess) return "";

            if (sReturn != IntPtr.Zero)
            {
                Marshal.FreeBSTR(sReturn);
                sReturn = IntPtr.Zero;
            }

            bSuccess = waitForCompleteVBWrapper(
                m_pvTCP_IPClientObject,
                iMessage1ID,
                ref sReturn,
                ref bMessage1CommandSuccess,
                -1);

            if (sReturn != IntPtr.Zero)
            {
                sMessage1Return = Marshal.PtrToStringBSTR(sReturn);
                Marshal.FreeBSTR(sReturn);
                sReturn = IntPtr.Zero;
            }

            if (!bSuccess || !bMessage1CommandSuccess)
                return "";

            int iNumberOfResponsesFromMessage1 = 0;

            if (!getNumberOfIndividualResponsesVBWrapper(
                m_pvTCP_IPClientObject,
                sMessage1Return,
                ref iNumberOfResponsesFromMessage1))
                return "";

            for (int i = 0; i < iNumberOfResponsesFromMessage1; i++)
            {
                bSuccess = getResponseAtIndexVBWrapper(
                    m_pvTCP_IPClientObject,
                    sMessage1Return,
                    i,
                    ref sReturn);

                if (!bSuccess) return "";

                if (sReturn != IntPtr.Zero)
                {
                    string sResponse = Marshal.PtrToStringBSTR(sReturn);
                    Marshal.FreeBSTR(sReturn);
                    sReturn = IntPtr.Zero;

                    if (i == 0)
                        finalValue = sResponse;
                }
            }

            return finalValue;
        }


        public List<string> testexampleSysMode(string tmpcommand)
        {
            string sMessage1 = "P,T,N";

            int iMessage1ID = 0;
            string sMessage1Return = "";
            bool bMessage1CommandSuccess = true;

            IntPtr sReturn = IntPtr.Zero;
            List<string> modes = new List<string>();

            bool bSuccess = sendMessageFullVBWrapper(
                m_pvTCP_IPClientObject,
                sMessage1,
                false,
                ref iMessage1ID,
                ref bMessage1CommandSuccess,
                ref sReturn,
                -1);

            if (!bSuccess) return modes;

            if (sReturn != IntPtr.Zero)
            {
                Marshal.FreeBSTR(sReturn);
                sReturn = IntPtr.Zero;
            }

            bSuccess = waitForCompleteVBWrapper(
                m_pvTCP_IPClientObject,
                iMessage1ID,
                ref sReturn,
                ref bMessage1CommandSuccess,
                -1);

            if (sReturn != IntPtr.Zero)
            {
                sMessage1Return = Marshal.PtrToStringBSTR(sReturn);
                Marshal.FreeBSTR(sReturn);
                sReturn = IntPtr.Zero;
            }

            if (!bSuccess || !bMessage1CommandSuccess)
                return modes;

            int iNumberOfResponsesFromMessage1 = 0;

            if (!getNumberOfIndividualResponsesVBWrapper(
                m_pvTCP_IPClientObject,
                sMessage1Return,
                ref iNumberOfResponsesFromMessage1))
                return modes;

            for (int i = 0; i < iNumberOfResponsesFromMessage1; i++)
            {
                bSuccess = getResponseAtIndexVBWrapper(
                    m_pvTCP_IPClientObject,
                    sMessage1Return,
                    i,
                    ref sReturn);

                if (!bSuccess) return modes;

                if (sReturn != IntPtr.Zero)
                {
                    string sResponse = Marshal.PtrToStringBSTR(sReturn);
                    Marshal.FreeBSTR(sReturn);
                    sReturn = IntPtr.Zero;

                    if (i == 0)
                    {
                        string[] strArr = sResponse.Split(';');
                        for (int count = 1; count < strArr.Length; count++)
                        {
                            modes.Add(strArr[count]);
                        }
                    }
                }
            }

            return modes;
        }


        public string testexamplePrintMode(string tmpcommand)
        {
            string sMessage1 = "P,V,P," + tmpcommand;

            int iMessage1ID = 0;
            string sMessage1Return = "";
            bool bMessage1CommandSuccess = true;

            IntPtr sReturn = IntPtr.Zero;
            string currentMode = "";

            bool bSuccess = sendMessageFullVBWrapper(
                m_pvTCP_IPClientObject,
                sMessage1,
                false,
                ref iMessage1ID,
                ref bMessage1CommandSuccess,
                ref sReturn,
                -1);

            if (!bSuccess) return "";

            if (sReturn != IntPtr.Zero)
            {
                Marshal.FreeBSTR(sReturn);
                sReturn = IntPtr.Zero;
            }

            bSuccess = waitForCompleteVBWrapper(
                m_pvTCP_IPClientObject,
                iMessage1ID,
                ref sReturn,
                ref bMessage1CommandSuccess,
                -1);

            if (sReturn != IntPtr.Zero)
            {
                sMessage1Return = Marshal.PtrToStringBSTR(sReturn);
                Marshal.FreeBSTR(sReturn);
                sReturn = IntPtr.Zero;
            }

            if (!bSuccess || !bMessage1CommandSuccess)
                return "";

            int iNumberOfResponsesFromMessage1 = 0;

            if (!getNumberOfIndividualResponsesVBWrapper(
                m_pvTCP_IPClientObject,
                sMessage1Return,
                ref iNumberOfResponsesFromMessage1))
                return "";

            for (int i = 0; i < iNumberOfResponsesFromMessage1; i++)
            {
                bSuccess = getResponseAtIndexVBWrapper(
                    m_pvTCP_IPClientObject,
                    sMessage1Return,
                    i,
                    ref sReturn);

                if (!bSuccess) return "";

                if (sReturn != IntPtr.Zero)
                {
                    string sResponse = Marshal.PtrToStringBSTR(sReturn);
                    Marshal.FreeBSTR(sReturn);
                    sReturn = IntPtr.Zero;

                    if (i == 0)
                        currentMode = sResponse;
                }
            }

            return currentMode;
        }

        

        [DllImport("GIS Utility - TCP-IP Comms.dll",CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool disconnectFromServerVBWrapper(IntPtr p_pvTCP_IPClientObject);


        public bool disconnectFromServer()
        {
            if (m_pvTCP_IPClientObject == IntPtr.Zero)
                return true;

            try
            {
                if (!disconnectFromServerVBWrapper(m_pvTCP_IPClientObject))
                {
                    MessageBox.Show("Failed to disconnect from the server");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            return true;
        }




    }
}
