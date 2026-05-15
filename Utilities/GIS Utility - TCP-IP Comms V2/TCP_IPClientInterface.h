///////////////////////////////////////////////////////////////////////////////////////////////
//
// Copyright (C) 2007-2011 Global Inkjet Systems Ltd.
//
// E-mail: support@globalinkjetsystems.com
//
// Web:	http://www.globalinkjetsystems.com/
//
// TCP_IPClientInterface.h
//
// Version: 1.6
// 
///////////////////////////////////////////////////////////////////////////////////////////////


#pragma once

#include "LogManagerInterface.h"
#include "PrintheadInformationInterface.h"
#include "SecurityManagerInterface.h"
#include "TCP_IPClientListenerInterface.h"


//#define STANDARD_STRING_SIZE	4096
#define STANDARD_STRING_SIZE	8192
#define EXTENDED_STRING_SIZE	100000
//#define EXTENDED_STRING_SIZE	1048576
#define MASSIVE_STRING_SIZE		1000000

/*
  This is the interface for the TCP/IP client. Any class which implements this interface will provide functionality for a client that can connect to
  a server through TCP/IP.

  This interface is required as it can be provided to applications which are to use the TCP/IP comms utility DLL in order to avoid linking errors.

  For more information about the TCP/IP communication, see the "Print Server Communication User Guide".
 */
class ITCP_IPClientInterface
{
protected:
	ITCP_IPClientListenerInterface *m_pxTCP_IPClientListener;	// This is the listener object which contains the functions which should be called when various events in the TCP/IP comms occur.

	ISecurityManagerInterface *m_pxSecurityManager;	// The manager used to perform security actions.

	ILogManagerInterface *m_pxLogManager;	// The manager used to record log messages.
	bool m_bExtendedNetworkLogging;			// The boolean which determines whether the extended log messages, which are only useful during debugging, should be included.


public:
	/*
	  The constructor for the ITCP_IPClientInterface class, which initialises the variables.
	 */
	ITCP_IPClientInterface(void)
	{
		m_pxTCP_IPClientListener = NULL;
		m_pxSecurityManager = NULL;
		m_pxLogManager = NULL;
		m_bExtendedNetworkLogging = false;
	}
	/*
	  The destructor for the ITCP_IPClientInterface class.
	 */
	virtual ~ITCP_IPClientInterface(void)
	{
	}


	/*
	  This initialises the TCP/IP client.
	  This must be called only one, before any other functions are called.
	  \param p_pxSecurityManager - The security manager is used to perform security actions, such as connect to a locked Print Server. If this is NULL, no security actions will be performed.
	  \param p_pxLogManager - The log manager used to log the TCP/IP comms. If this is NULL, a CTCP_IPCommsLogManager will be created (and deleted automatically).
	  \param p_bExtendedNetworkLogging - Whether to include the addional log messages, which will only be useful during debugging.
	  \return Whether the initialisation was successful or not.
	 */
	virtual bool initialise(ISecurityManagerInterface *p_pxSecurityManager=NULL, ILogManagerInterface *p_pxLogManager=NULL, bool p_bExtendedNetworkLogging=false) = 0;
	/*
	  This initialises the TCP/IP client.
	  This must be called only one, before any other functions are called.
	  \param p_pcSecurityKeyA - The A security key used in the security manager. This is not required if connecting to a Print Server which has TCP/IP enabled.
	  \param p_pcSecurityKeyA - The B security key used in the security manager. This is not required if connecting to a Print Server which has TCP/IP enabled.
	  \param p_bLogToFileEnabled - Whether logging to file is enabled or not.
	  \param p_pcLogFile - The path to thr log file.
	  \param p_bLogToInterfaceEnabled - Whether messages should be logged to the interface or not.
	  \param p_pvInterfaceLogCallbackObject - The object the interface log callback should be called on.
	  \param p_puInterfaceLogCallback - The pointer to the interface log callback function.
	  \param p_bExtendedNetworkLogging - Whether to include the addional log messages, which will only be useful during debugging.
	  \return Whether the initialisation was successful or not.
	 */
	virtual bool initialise(const wchar_t *p_pcSecurityKeyA, const wchar_t *p_pcSecurityKeyB, bool p_bLogToFileEnabled, const wchar_t *p_pcLogFile, bool p_bLogToInterfaceEnabled, void *p_pvInterfaceLogCallbackObject, bool (* p_puInterfaceLogCallback)(void *p_pvInterfaceLogCallbackObject, ILogManagerInterface::ELOG_LEVEL p_eLogLevel, const wchar_t *p_pcMessage), bool p_bExtendedNetworkLogging=false) = 0;
	/*
	  This aborts the TCP/IP client.
	  This must be called only one, and no other functions must be called after it is called.
	  \return Whether the abort was successful or not.
	 */
	virtual bool abort(void) = 0;


	/*
	  This connects to the server with the properties specified.
	  A successful connection must be made before any function which communicates with the server is called.
	  This is only valid when not connected to a server.
	  \param p_pcIPAddress - The IP address of the server to connect to. Use "localhost" if the server is on the same computer.
	  \param p_iPort - The network port of the server to connect to.
	  \param p_iConnectionTimeout - The maximum time in milliseconds the function will wait until the connection has been established. This parameter should be set to -1 to have no timeout i.e. the function will wait until the connection is established or it is stopped. This parameter should be set to 0 to have no wait i.e. the functon will fail if a connection cannot be made immediately.
	  \param p_bWaitUntilConnectionComplete - If this is true, the function will not return until the connection process is complete.
	  \param p_bContinuouslyMonitorServer - If this is true, the client will continuously monitor the server to see if there is anything to read from it and to check if it has disconnected. If this is false, an attempt to read or check the server will only be made when the appropriate function is called.
	  \param p_bAutomaticallyReconnectToServer - If this is true, the client will automatically try to reconnect to the server if it detects that the server has disconnected.
	  \return Whether the connection was successful or not.
	 */
	virtual bool connectToServer(const wchar_t *p_pcIPAddress, int p_iPort, int p_iConnectionTimeout=0, bool p_bWaitUntilConnectionComplete=true, bool p_bContinuouslyMonitorServer=true, bool p_bAutomaticallyReconnectToServer=false) = 0;
	/*
	  This launches the server executable at the patch specified and then connects to the server with the properties specified.
	  The server must be located on the same computer as this client, so the IP address can only be "localhost".
	  A successful connection must be made before any function which communicates with the server is called.
	  This is only valid when not connected to a server.
	  \param p_pcExecutablePath - The path to the executable to launch. Leave blank to launch the "GIS Print Server 2.exe" executable contain within the current working folder.
	  \param p_pcServerID - The ID of the server.
	  \param p_pcConfigurationPath - The configuration the server will load after it starts.
	  \param p_iPort - The network port of the server to connect to.
	  \param p_bStartMinimised - If true, the server will be minimised when it starts.
	  \param p_iConnectionTimeout - The maximum time in milliseconds the function will wait until the connection has been established. This parameter should be set to -1 to have no timeout i.e. the function will wait until the connection is established or it is stopped. This parameter should be set to 0 to have no wait i.e. the functon will fail if a connection cannot be made immediately.
	  \param p_bWaitUntilConnectionComplete - If this is true, the function will not return until the connection process is complete.
	  \param p_bContinuouslyMonitorServer - If this is true, the client will continuously monitor the server to see if there is anything to read from it and to check if it has disconnected. If this is false, an attempt to read or check the server will only be made when the appropriate function is called.
	  \param p_bAutomaticallyReconnectToServer - If this is true, the client will automatically try to reconnect to the server if it detects that the server has disconnected.
	  \return Whether the connection was successful or not.
	 */
	virtual bool launchAndConnectToServer(const wchar_t *p_pcExecutablePath, const wchar_t *p_pcServerID, const wchar_t *p_pcConfigurationPath, int p_iPort, bool p_bStartMinimised, int p_iConnectionTimeout=0, bool p_bWaitUntilConnectionComplete=true, bool p_bContinuouslyMonitorServer=true, bool p_bAutomaticallyReconnectToServer=false) = 0;
	/*
	  This connects to a print server monitor, launches the server with the properties specified and then connects to it.
	  A successful connection must be made before any function which communicates with the server is called.
	  This is only valid when not connected to a server or print server monitor.
	  \param p_pcIPAddress - The IP address of the print server monitor and print server server to connect to. Use "localhost" if the server is on the same computer.
	  \param p_iMonitorPort - The network port of the monitor to connect to.
	  \param p_iServerPort - The network port of the server to connect to.
	  \param p_pcServerID - The ID of the server.
	  \param p_pcConfigurationPath - The configuration the server will load after it starts. 
	  \param p_bStartMinimised - If true, the server will be minimised when it starts.
	  \param p_bConnectedToPMB - Whether this server will communicate with a PMB or not.
	  \param p_iConnectionTimeout - The maximum time in milliseconds the function will wait until the connection has been established. This parameter should be set to -1 to have no timeout i.e. the function will wait until the connection is established or it is stopped. This parameter should be set to 0 to have no wait i.e. the functon will fail if a connection cannot be made immediately.
	  \param p_bWaitUntilConnectionComplete - If this is true, the function will not return until the connection process is complete.
	  \param p_bContinuouslyMonitorServer - If this is true, the client will continuously monitor the server to see if there is anything to read from it and to check if it has disconnected. If this is false, an attempt to read or check the server will only be made when the appropriate function is called.
	  \param p_bAutomaticallyReconnectToServer - If this is true, the client will automatically try to reconnect to the server if it detects that the server has disconnected.
	  \return Whether the connection was successful or not.
	 */
	virtual bool launchAndConnectToServerThroughMonitor(const wchar_t *p_pcIPAddress, int p_iMonitorPort, int p_iServerPort, const wchar_t *p_pcServerID, const wchar_t *p_pcConfigurationPath, bool p_bStartMinimised, bool p_bConnectedToPMB, int p_iConnectionTimeout=0, bool p_bWaitUntilConnectionComplete=true, bool p_bContinuouslyMonitorServer=true, bool p_bAutomaticallyReconnectToServer=false) = 0;
	/*
	  This disconnects from the server the client is currently connected to and then connects to the server with the properties specified.
	  This is only valid when connected to a server.
	  \param p_pcIPAddress - The IP address of the server to connect to. Use "localhost" if the server is on the same computer.
	  \param p_iPort - The network port of the server to connect to.
	  \param p_iConnectionTimeout - The maximum time in milliseconds the function will wait until the connection has been established. This parameter should be set to -1 to have no timeout i.e. the function will wait until the connection is established or it is stopped. This parameter should be set to 0 to have no wait i.e. the functon will fail if a connection cannot be made immediately.
	  \param p_bWaitUntilConnectionComplete - If this is true, the function will not return until the connection process is complete.
	  \param p_bContinuouslyMonitorServer - If this is true, the client will continuously monitor the server to see if there is anything to read from it and to check if it has disconnected. If this is false, an attempt to read or check the server will only be made when the appropriate function is called.
	  \param p_bAutomaticallyReconnectToServer - If this is true, the client will automatically try to reconnect to the server if it detects that the server has disconnected.
	  \return Whether the connection was successful or not.
	 */
	virtual bool reconnectToServer(const wchar_t *p_pcIPAddress, int p_iPort, int p_iConnectionTimeout=0, bool p_bWaitUntilConnectionComplete=true, bool p_bContinuouslyMonitorServer=true, bool p_bAutomaticallyReconnectToServer=false) = 0;
	/*
	  This stops the current connection to the server.
	  This is only valid when the connection was not told to wait until the connection is complete.
	  \return Whether the stop was successful or not.
	 */
	virtual bool stopConnectionToServer(void) = 0;
	/*
	  This disconnects from the server the client is currently connected to.
	  \param p_bShutdownPrintServer - Whether to shutdown the server after disconnecting from it. This is only valid when connected to a server and when this client launched the server.
	  \return Whether the disconnection was successful or not.
	 */
	virtual bool disconnectFromServer(bool p_bShutdownPrintServer=true) = 0;
	/*
	  This blocks until the connection has fully completed.
	  \return Whether the wait until connection executed successfully or not.
	 */
	virtual bool waitUntilConnectedToServer(void) const = 0;
	/*
	  This returns whether the client is currently connected to a server or not.
	  \return Whether the client is currently connected to a server or not.
	 */
	virtual bool isConnectedToServer(void) const = 0;
	/*
	  This returns the server executable process information.
	  \return The server executable process information.
	 */
	virtual PROCESS_INFORMATION getServerProcessInformation(void) = 0;


	/*
	  This sends a message to the server the client is connected to.
	  This is only valid when connected to a server.
	  \param p_pcMessage - The message to send.
	  \param p_psReturn - If the command returns data, it will all be returned in this parameter. This parameter should be NULL if nothing is to be returned or if the return values are not to be recorded.
	  \param p_iReturnLength - The length of the p_pcReturn buffer.
	  \param p_bWaitForComplete - If this is true the command will not return until the complete command associated with the command being sent is returned.
	  \param p_bFailOnCommandError - If this is true, if the complete command with an error code associated with the command being sent has a non-successful error code, this function will return a failure. This is only relavent if p_bWaitForComplete is true. 
	  \return Whether the send was successful or not.
	 */
	virtual bool sendMessage(const wchar_t *p_pcMessage, wchar_t *p_pcReturn=NULL, int p_iReturnLength=0, bool p_bWaitForComplete=true, bool p_bFailOnCommandError=true) = 0;
	/*
	  This sends a message to the server the client is connected to.
	  This is only valid when connected to a server.
	  \param p_pcMessage - The message to send.
	  \param p_bWaitForComplete - If this is true the command will not return until the complete command associated with the command being sent is returned.
	  \param p_piMessageID - The ID of the command that was sent will be returned in this parameter. This parameter should be NULL if the ID is not to be recorded. This is only relavent if p_bWaitForComplete is false.
	  \param p_pbCommandSuccess - Whether the complete command associated with the command that was sent is returned in this parameter. This parameter should be NULL if this parameter is not to be recorded. this function will always return true if the command being sent has a non-successful error code.
	  \param p_psReturn - If the command returns data, it will all be returned in this parameter. This parameter should be NULL if nothing is to be returned or if the return values are not to be recorded.
	  \param p_iReturnLength - The length of the p_pcReturn buffer.
	  \param p_iResponseTimeout - The amount of time (in milliseconds) that the function will wait for a complete command to be returned. This parameter should be set to -1 to have no timeout. This is only relavent if p_bWaitForComplete is true. 
	  \return Whether the send was successful or not.
	 */
	virtual bool sendMessage(const wchar_t *p_pcMessage, bool p_bWaitForComplete, int *p_piMessageID, bool *p_pbCommandSuccess, wchar_t *p_pcReturn=NULL, int p_iReturnLength=0, int p_iResponseTimeout=-1) = 0;
	/*
	  This signals that the next message sent should not wait for a complete.
	  This is only valid when connected to a server.
	  \return Whether the signal was set was successful or not.
	 */
	virtual bool doNotWaitForCompleteFromNextMessage(void) = 0;
	/*
	  This returns the information about the last message sent.
	  This is only valid when connected to a server and when the last message sent did not wait for a complete
	  \param p_piMessageID - The ID of the command that was sent will be returned in this parameter. This parameter should be NULL if the ID is not to be recorded. This is only relavent if p_bWaitForComplete is false.
	  \return Whether the information was returned successfully or not.
	 */
	virtual bool getLastMessageInformation(int *p_piMessageID) = 0;
	/*
	  This sends a message to the server, but by creating a new client (using the same settings as the current client). This may be needed as the Print Server will only process once command from
	  a client at a time, but some commands (such as aborts) may need to be sent in parallel.
	  This is only valid when connected to a server.
	  \param p_pcMessage - The message to send.
	  \param p_psReturn - If the command returns data, it will all be returned in this parameter. This parameter should be NULL if nothing is to be returned or if the return values are not to be recorded.
	  \param p_iReturnLength - The length of the p_pcReturn buffer.
	  \param p_bWaitForComplete - If this is true the command will not return until the complete command associated with the command being sent is returned.
	  \param p_bFailOnCommandError - If this is true, if the complete command with an error code associated with the command being sent has a non-successful error code, this function will return a failure. This is only relavent if p_bWaitForComplete is true. 
	  \return Whether the send was successful or not.
	 */
	virtual bool sendMessageInNewClient(const wchar_t *p_pcMessage, wchar_t *p_pcReturn=NULL, int p_iReturnLength=0, bool p_bFailOnCommandError=true) = 0;
	/*
	  This signals that the next message should be sent in a new client.
	  This is only valid when connected to a server.
	  \return Whether the signal was set was successful or not.
	 */
	virtual bool sendNextMessageInNewClient(void) = 0;
	/*
	  This sends some bytes to the server the client is connected to.
	  This is only valid when connected to a server.
	  \param p_pyBuffer - The bytes to send.
	  \param p_iBytesToSend - The number of bytes to send.
	  \return Whether the send was successful or not.
	 */
	virtual bool sendBytes(BYTE *p_pyBuffer, int p_iBytesToSend) = 0;

	/*
	  This reads a response from the server the client is connected to.
	  This is only valid when connected to a server.
	  \param p_pcResponse - The data read will be returned in this parameter. If this is blank then the server disconnected from the client before anything could be read.
	  \param p_iResponseLength - The length of the p_pcResponse buffer.
	  \param p_iReadTimeout - The amount of time (in milliseconds) that the function will wait for a response to be returned. This parameter should be set to -1 to have no timeout.
	  \return Whether the read was successful or not.
	 */
	virtual bool readFromServer(wchar_t *p_pcResponse, int p_iResponseLength, int p_iReadTimeout=-1) = 0;
	/*
	  This reads a response from the server the client is connected to.
	  This is only valid when connected to a server.
	  \param p_pyBuffer - The data read will be returned in this parameter. If this is blank then the server disconnected from the client before anything could be read.
	  \param p_iBytesToRead - The number of bytes to read from the server.
	  \param p_iBytesRead - The number of bytes that were read from the server will be returned in this parameter.
	  \param p_iReadTimeout - The amount of time (in milliseconds) that the function will wait for a response to be returned. This parameter should be set to -1 to have no timeout.
	  \return Whether the read was successful or not.
	 */
	virtual bool readBytes(BYTE *p_pyBuffer, int p_iBytesToRead, int &p_iBytesRead, int p_iReadTimeout=-1) = 0;

	/*
	  This waits for an acknowledgement message associated with the command specified to be read from the server.
	  This is only valid when connected to a server.
	  \param p_pcMessage - The message that the acknowledgement command should be associated with.
	  \param p_piMessageID - The ID of the acknowledgement command will be returned in this parameter. This parameter should be NULL if the ID is not to be recorded.
	  \param p_iResponseTimeout - The amount of time (in milliseconds) that the function will wait for an acknowledgement to be returned. This parameter should be set to -1 to have no timeout. This parameter should be set to 0 to have no wait i.e. if function only checks if an acknowledgement command has already been returned.
	  \return Whether the wait was successful or not.
	 */
	virtual bool waitForAcknowledge(const wchar_t *p_pcMessage, int *p_piMessageID, int p_iResponseTimeout=-1) = 0;
	/*
	  This waits for an information message associated with the command ID specified to be read from the server.
	  This is only valid when connected to a server.
	  \param p_piMessageID - The ID of the message that the information command should be associated with.
	  \param p_pcInformationCode - The code of the information command will be returned in this parameter. This parameter should be NULL if the code is not to be recorded.
	  \param p_iInformationCodeLength - The length of the p_pcInformationCode buffer.
	  \param p_pcInformationData - The data associated with the information command will be returned in this parameter. This parameter should be NULL if the data is not to be recorded.
	  \param p_iInformationDataLength - The length of the p_psInformationData buffer.
	  \param p_iResponseTimeout - The amount of time (in milliseconds) that the function will wait for an information command to be returned. This parameter should be set to -1 to have no timeout. This parameter should be set to 0 to have no wait i.e. if function only checks if an information command has already been returned.
	  \return Whether the wait was successful or not.
	 */
	virtual bool waitForInformation(int p_iMessageID, wchar_t *p_pcInformationCode, int p_iInformationCodeLength, wchar_t *p_pcInformationData, int p_iInformationDataLength, int p_iResponseTimeout=-1) = 0;
	/*
	  This waits for a complete associated with the command specified to be read from the server.
	  This is only valid when connected to a server.
	  \param p_pcMessage - The message that the complete command should be associated with.
	  \param p_psReturn - The return information in the complete command will be returned in this parameter. This parameter should be NULL if no return data is expected or of the data is not to be recorded.
	  \param p_iReturnLength - The length of the p_pcReturn buffer.
	  \param p_pbCommandSuccess - Whether the complete code indicated success or not. This parameter should be NULL if the success is not to be recorded.
	  \param p_iResponseTimeout - The amount of time (in milliseconds) that the function will wait for a complete to be returned. This parameter should be set to -1 to have no timeout. This parameter should be set to 0 to have no wait i.e. if function only checks if a complete command has already been returned.
	  \return Whether the wait was successful or not.
	 */
	virtual bool waitForComplete(int p_iMessageID, wchar_t *p_pcReturn, int p_iReturnLength, bool *p_pbCommandSuccess, int p_iResponseTimeout=-1) = 0;


	/*
	  This takes the data from a complete command (which will be a number of values separated by commas) and splits them into the individual values.
	  This array and all strings inside it will have to be deleted by the caling function.
	  \param p_pcCombinedResponses - The combined responses.
	  \param p_pppcResponses - This parameter will be populated with the individual values taken from the combined responses. The value returned is an array of responses. The array will be of length p_iNumberOfResponses, and each response will be a wchar_t * of length STANDARD_STRING_SIZE.
	  \param p_iNumberOfResponses - The number of responses returned in the p_ppcResponses parameter will be returned in this parameter. 
	  \return Whether the split was successful or not.
	 */
	virtual bool splitResponses(const wchar_t *p_pcCombinedResponses, wchar_t ***p_pppcResponses, int &p_iNumberOfResponses) const = 0;

	/*
	  This takes the data from a printhead status information message and returns a map which links the printhead name to the information.
	  \param p_pcPrintheadStatusInformation - The printhead status information.
	  \param p_mspxPrintheadInformation - The printheads and associated information. The map is of the form <Printhead Name, Information>.
	  \param p_pppcPrintheadNames - This parameter will be populated with the names of the printheads. The value returned is an array of names. The array will be of length p_iNumberOfPrintheads, and each name will be a wchar_t * of length STANDARD_STRING_SIZE.
	  \param p_pppxPrintheadInformation - This parameter will be populated with the printheads informatiob. The value returned is an array of names. The array will be of length p_iNumberOfPrintheads.
	  \param p_iNumberOfPrintheads - The number of printheads returned in the p_ppcPrintheadNames and p_ppxPrintheadInformation parameters will be returned in this parameter.
	  \return Whether the parse was successful or not.
	 */
	virtual bool parsePrintheadInformation(const wchar_t *p_pcPrintheadStatusInformation, wchar_t ***p_pppcPrintheadNames, IPrintheadInformationInterface ***p_pppxPrintheadInformation, int &p_iNumberOfPrintheads) const = 0;
	
	
	/*
	  This updates the log settings.
	  \param p_bLogToFileEnabled - Whether logging to file is enabled or not.
	  \param p_pcLogFile - The path to thr log file.
	  \param p_bLogToInterfaceEnabled - Whether messages should be logged to the interface or not.
	  \param p_bExtendedNetworkLogging - Whether to include the addional log messages, which will only be useful during debugging.
	  \return Whether the log settings were updated successfully or not.
	 */
	virtual bool updateLogSettings(bool p_bLogToFileEnabled, const wchar_t *p_pcLogFile, bool p_bLogToInterfaceEnabled, bool p_bExtendedNetworkLogging=false) = 0;
	/*
	  This logs a message.
	  \param p_iLogLevel - The log level.
	  \param p_pcLogMessage - The message to log.
	  \return Whether the log was successful or not.
	*/
	virtual bool log(int p_iLogLevel, const wchar_t *p_pcLogMessage) const = 0;
	/*
	  This returns the low log level.
	  \return The low log level.
	*/
	virtual int getLowLogLevel(void) const = 0;
	/*
	  This returns the warning log level.
	  \return The warning log level.
	*/
	virtual int getWarningLogLevel(void) const = 0;
	/*
	  This returns the error log level.
	  \return The error log level.
	*/
	virtual int getErrorLogLevel(void) const = 0;
	/*
	  This returns the UI info log level.
	  \return The UI info log level.
	*/
	virtual int getUIInfoLogLevel(void) const = 0;
	/*
	  This returns the UI warning log level.
	  \return The UI warning log level.
	*/
	virtual int getUIWarningLogLevel(void) const = 0;
	/*
	  This returns the UI error log level.
	  \return The UI error log level.
	*/
	virtual int getUIErrorLogLevel(void) const = 0;
	/*
	  This returns a Log Manager object.
	  The log manager will not be initialised.
	  The calling function takes ownership of the log manager and must delete it.
	  \param p_ppxLogManager - The Log Manager is returned in this parameter.
	  \return Whether the Log Manager was created successfully or not.
	 */
	virtual bool createLogManager(ILogManagerInterface **p_ppxLogManager) const = 0;
	/*
	  This deletes a Log Manager object.
	  \param p_pxLogManager - The Log Manager to delete.
	  \return Whether the Log Manager was deleted successfully or not.
	 */
	virtual bool deleteLogManager(ILogManagerInterface *p_pxLogManager) const = 0;

	
	/*
	  This sets the client listener object.
	  This class will not automatically delete this object.
	  \param p_pxTCP_IPClientListener - The new client listener object.
	  \return Whether the set was successful or not.
	 */
	bool setTCP_IPClientListener(ITCP_IPClientListenerInterface *p_pxTCP_IPClientListener)
	{
		m_pxTCP_IPClientListener = p_pxTCP_IPClientListener;

		return true;
	}
	/*
	  This deletes the client listener object.
	  This is only necessary if the object which created the listener object will not delete it.
	  \return Whether the delete was successful or not.
	 */
	bool deleteTCP_IPClientListener(void)
	{
		if(m_pxTCP_IPClientListener)
		{
			delete m_pxTCP_IPClientListener;
			m_pxTCP_IPClientListener = NULL;
		}

		return true;
	}



	/////////////////////////////////////
	// Print Server
	/////////////////////////////////////

	/*
	  This sends the command which initialises the Print Server with the configuration file specified.
	  This is only valid when connected to a server.
	  \param p_pcConfigurationFile - The configuration file the Prin Server will be initialised with.
	  \return Whether the command executed successfully.
	*/
	virtual bool initialisePrintServer(const wchar_t *p_pcConfigurationFile) = 0;
	/*
	  This sends the command which waits for the Print Server to finish initialising.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool waitUntilPrintServerInitialised(void) = 0;
	
	/*
	  This sends the command which registers this command for information from the Print Server.
	  This is only valid when connected to a server.
	  \param p_bReturnExistingInformation - Whether to return the existing information or not.
	  \return Whether the command executed successfully.
	*/
	virtual bool registerForPrintServerInformation(bool p_bReturnExistingInformation) = 0;


	/*
	  This sends the command which saves the Print Server configuration to the folder specified.
	  This is only valid when connected to a server.
	  \param p_pcConfigurationFolder - The folder where the print server configuration will be saved.
	  \return Whether the command executed successfully.
	*/
	virtual bool saveConfigurationAs(const wchar_t *p_pcConfigurationFolder) = 0;
	/*
	  This sends the command which saves the Print Server configuration to the current folder.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool saveConfiguration(void) = 0;


	/*
	  This sends the command which applys the system mode with the name specified.
	  This is only valid when connected to a server.
	  \param p_pcSystemModeName - The name of the system mode to apply.
	  \param p_bApplySubModes - Whether to aply the sub modes or not.
	  \return Whether the command executed successfully.
	*/
	virtual bool applySystemMode(const wchar_t *p_pcSystemModeName, bool p_bApplySubModes) = 0;
	/*
	  This sends the command which gets the name of all system modes.
	  This is only valid when connected to a server.
	  \param p_pcSystemModeNames - The system mode names will be returned in this parameter.
	  \param p_iSystemModeNamesLength - The length of the p_pcSystemModeNames buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getSystemModeNames(wchar_t *p_pcSystemModeNames, int p_iSystemModeNamesLength) = 0;
	/*
	  This sends the command which gets the information for the system mode specified.
	  This is only valid when connected to a server.
	  \param p_pcSystemModeName - The name of the system mode to retrieve information for.
	  \param p_pcRenderModeName - The name of the render mode for the system mode specified is returned in this parameter.
	  \param p_iRenderModeNameLength - The length of the p_pcRenderModeName buffer.
	  \param p_pcPrintModeName - The name of the print mode for the system mode specified is returned in this parameter.
	  \param p_iPrintModeNameLength - The length of the p_pcPrintModeName buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getSystemModeInformation(const wchar_t *p_pcSystemModeName, wchar_t *p_pcRenderModeName, int p_iRenderModeNameLength, wchar_t *p_pcPrintModeName, int p_iPrintModeNameLength) = 0;


	/*
	  This sends the command which sets the parameter in the Print Server with the name specified to the
	  value specified.
	  This is only valid when connected to a server.
	  \param p_pcParameterName - The name of the parameter to change.
	  \param p_pcParameterValue - The new value of the parameter.
	  \return Whether the command executed successfully.
	*/
	virtual bool setPrintServerParameterValue(const wchar_t *p_pcParameterName, const wchar_t *p_pcParameterValue) = 0;
	/*
	  This sends the command which sets a number of parameters in the Print Server at the same time.
	  This is only valid when connected to a server.
	  \param p_iNumberOfParametersToChange - The number of entries in the p_ppcParameterNames and p_pcParameterValues arrays.
	  \param p_ppcParameterNames - An array of of the parameter names to change. The parameters will be changed in the defined order.
	  \param p_pcParameterValues - A array of of the new parameter values. Each entry corresponds to the equalivent entry in p_vsParameterNames and so the size of p_vsParameterValues must equal the size of p_vsParameterNames.
	  \return Whether the command executed successfully.
	*/
	virtual bool setPrintServerParameterValues(int p_iNumberOfParametersToChange, const wchar_t **p_ppcParameterNames, const wchar_t **p_pcParameterValue) = 0;
	/*
	  This sets the Print Server parameter values based on the node information provided.
	  This is only valid when connected to a server.
	  \param p_pcNodePath - The path of the node to get the informtion for.
	  \param p_pcNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	virtual bool setPrintServerNodeInformation(const WCHAR *p_psNodePath, const WCHAR *p_pcNodeInformation) = 0;
	/*
	  This sends the command which gets the value of the parameter in the Print Server with the name specified.
	  This is only valid when connected to a server.
	  \param p_pcParameterName - The name of the parameter to change.
	  \param p_pcParameterValue - The value of the parameter is returned in this parameter.
	  \param p_iParameterValueLength - The length of the p_pcParameterValue buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getPrintServerParameterValue(const wchar_t *p_pcParameterName, wchar_t *p_pcParameterValue, int p_iParameterValueLength) = 0;
	/*
	  This sends the command which gets the information about the Print Server node specified.
	  This is only valid when connected to a server.
	  \param p_pcNodePath - The path of the node to get the informtion for.
	  \param p_pcExcludedNodeTypes - A comma separated list of node types to be excluded from the information.
	  \param p_ppcNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	virtual bool getPrintServerNodeInformation(const wchar_t *p_pcNodePath, const wchar_t *p_pcExcludedNodeTypes, wchar_t **p_ppcNodeInformation) = 0;


	/*
	  This sets the parameter in the paramter store with the name specified to the value given.
	  This is only valid when connected to a server.
	  \param p_pcParameterName - The name of the parameter to set.
	  \param p_pcParameterValue - The new value of the parameter.
	  \return Whether the command executed successfully.
	*/
	virtual bool setParameterStoreValue(const wchar_t *p_pcParameterName, const wchar_t *p_pcParameterValue) = 0;
	/*
	  This gets the value of the parameter in the paramter store.
	  This is only valid when connected to a server.
	  \param p_pcParameterName - The name of the parameter to get.
	  \param p_pcParameterValue - The value of the parameter is returned in this parameter.
	  \param p_iParameterValueLength - The length of the p_pcParameterValue buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getParameterStoreValue(const wchar_t *p_pcParameterName, wchar_t *p_pcParameterValue, int p_iParameterValueLength) = 0;
	/*
	  This deletes the parameter in the paramter store with the name specified.
	  This is only valid when connected to a server.
	  \param p_pcParameterName - The name of the parameter to delete.
	  \return Whether the command executed successfully.
	*/
	virtual bool deleteParameterStoreValue(const wchar_t *p_pcParameterName) = 0;


	/*
	  This sends the command which aborts the Print Server.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool abortOperationPrintServer(void) = 0;
	/*
	  This sends the command which initialises the Print Server.
	  This is only valid when connected to a server.
	  \param p_pcConfigurationFolder - The configuration folder used in the initialisation.
	  \return Whether the command executed successfully.
	*/
	virtual bool initialiseOperationPrintServer(const wchar_t *p_pcConfigurationFolder) = 0;
	/*
	  This sends the command which ahuts down the Print Server.
	  This is only valid when connected to a server.
	  \param p_pcPCOperationCode - The code of the operation to perform on the PC after the PC has finished shutting down.
	  \return Whether the command executed successfully.
	*/
	virtual bool shutdownOperationPrintServer(const wchar_t *p_pcPCOperationCode) = 0;


	/*
	  This sends the command which gets information about the Print Server.
	  This is only valid when connected to a server.
	  \param p_pcSystemInformation - The system information is returned in this parameter.
	  \param p_iSystemInformationLength - The length of the p_pcSystemInformation buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getSystemInformation(wchar_t *p_pcSystemInformation, int p_iSystemInformationLength) = 0;



	/////////////////////////////////////
	// Render Engine
	/////////////////////////////////////

	/*
	  This sends the command which initialises the Render Engine with the configuration file specified.
	  This is only valid when connected to a server.
	  \param p_pcConfigurationFile - The configuration file the Render Engine will be initialised with.
	  \return Whether the command executed successfully.
	*/
	virtual bool initialiseRenderEngine(const wchar_t *p_pcConfigurationFile) = 0;

	/*
	  This sends the command which registers this command for information from the Render Engine.
	  This is only valid when connected to a server.
	  \param p_bReturnExistingInformation - Whether to return the existing information or not.
	  \return Whether the command executed successfully.
	*/
	virtual bool registerForRenderEngineInformation(bool p_bReturnExistingInformation) = 0;


	/*
	  This sends the command which applys the render mode with the name specified.
	  This is only valid when connected to a server.
	  \param p_pcRenderMode - The name of the render mode to apply.
	  \return Whether the command executed successfully.
	*/
	virtual bool applyRenderMode(const wchar_t *p_pcRenderMode) = 0;
	/*
	  This sends the command which gets the names of all render modes.
	  This is only valid when connected to a server.
	  \param p_pcRenderModeNames - The render mode names will be returned in this parameter.
	  \param p_iRenderModeNamesLength - The length of the p_pcRenderModeNames buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getRenderModeNames(wchar_t *p_pcRenderModeNames, int p_iRenderModeNamesLength) = 0;

	/*
	  This sends the command which sets the parameter in the Render Engine with the name specified to the
	  value specified.
	  This is only valid when connected to a server.
	  \param p_pcParameterName - The name of the parameter to change.
	  \param p_pcParameterValue - The new value of the parameter.
	  \return Whether the command executed successfully.
	*/
	virtual bool setRenderEngineParameterValue(const wchar_t *p_pcParameterName, const wchar_t *p_pcParameterValue) = 0;
	/*
	  This sends the command which sets a number of parameters in the Render Engine at the same time.
	  This is only valid when connected to a server.
	  \param p_iNumberOfParametersToChange - The number of entries in the p_ppcParameterNames and p_pcParameterValues arrays.
	  \param p_ppcParameterNames - An array of of the parameter names to change. The parameters will be changed in the defined order.
	  \param p_pcParameterValues - A array of of the new parameter values. Each entry corresponds to the equalivent entry in p_vsParameterNames and so the size of p_vsParameterValues must equal the size of p_vsParameterNames.
	  \return Whether the command executed successfully.
	*/
	virtual bool setRenderEngineParameterValues(int p_iNumberOfParametersToChange, const wchar_t **p_ppcParameterNames, const wchar_t **p_pcParameterValues) = 0;
	/*
	  This sets the Render Engine parameter values based on the node information provided.
	  This is only valid when connected to a server.
	  \param p_pcNodePath - The path of the node to get the informtion for.
	  \param p_pcNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	virtual bool setRenderEngineNodeInformation(const WCHAR *p_psNodePath, const WCHAR *p_pcNodeInformation) = 0;
	/*
	  This sends the command which gets the value of the parameter in the Render Engine with the name specified.
	  This is only valid when connected to a server.
	  \param p_pcParameterName - The name of the parameter to change.
	  \param p_pcParameterValue - The value of the parameter is returned in this parameter.
	  \param p_iParameterValueLength - The length of the p_pcParameterValue buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getRenderEngineParameterValue(const wchar_t *p_pcParameterName, wchar_t *p_pcParameterValue, int p_iParameterValueLength) = 0;
	/*
	  This sends the command which gets the information about the Render Engine node specified.
	  This is only valid when connected to a server.
	  \param p_pcNodePath - The path of the node to get the informtion for.
	  \param p_pcExcludedNodeTypes - A comma separated list of node types to be excluded from the information.
	  \param p_ppcNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	virtual bool getRenderEngineNodeInformation(const wchar_t *p_pcNodePath, const wchar_t *p_pcExcludedNodeTypes, wchar_t **p_ppcNodeInformation) = 0;


	/*
	  This sends the command which aborts the Render Engine.
	  This is only valid when connected to a server.
	  \param p_bSendInNewClient - If this is true, a new client connection will be made to the Print Server and the abort will be sent using this. This is necessary if a command from this client is already being processed by the Print Server.
	  \return Whether the command executed successfully.
	*/
	virtual bool abortRenderEngine(bool p_bSendInNewClient=false) = 0;


	/*
	  This sends the command which loads the document at the path specified.
	  This is only valid when connected to a server.
	  \param p_pcDocumentPath - The path to the document to load.
	  \return Whether the command executed successfully.
	*/
	virtual bool loadDocument(const wchar_t *p_pcDocumentPath) = 0;

	/*
	  This primes the currently loaded document for rendering.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool primeForRendering(void) = 0;

	/*
	  This sends the command which renders the loaded document to a set of bitmaps.
	  This can only be sent after a document has been loaded.
	  This is only valid when connected to a server.
	  \param p_pcRenderOutputPath - The path to the base file name that the output will be saved to.
	  \return Whether the command executed successfully.
	*/
	virtual bool startRender(const wchar_t *p_pcRenderOutputPath) = 0;
	/*
	  This sends the command which renders an input file to a set of bitmaps.
	  This is only valid when connected to a server.
	  \param p_pcInputFilePath - The path to the input file.
	  \param p_pcBitmapOutputPath - The path to the base file name that the output will be saved to.
	  \param p_dOutputWidth - The width of the output (in millimetres). Set this to -1.0 to use the width specified by the input file.
	  \param p_dOutputHeight - The height of the output (in millimetres). Set this to -1.0 to use the width specified by the input file.
	  \param p_iPageNo - Selects the page to be rendered from a multi-page input file.
	  \return Whether the command executed successfully.
	*/
	virtual bool renderBitmap(const wchar_t *p_pcInputFilePath, const wchar_t *p_pcBitmapOutputPath, double p_dOutputWidth=-1.0, double p_dOutputHeight=-1.0, int p_iPageNo=1) = 0;
	/*
	  This sends the command which renders an input file to a set of bitmaps.
	  This is only valid when connected to a server.
	  \param p_pcInputFilePath - The path to the input file.
	  \param p_pcBitmapOutputPath - The path to the base file name that the output will be saved to.
	  \param p_dOutputWidth - The width of the output (in pixels). Set this to -1.0 to use the width specified by the input file.
	  \param p_dOutputHeight - The height of the output (in pixels). Set this to -1.0 to use the height specified the input file.
	  \param p_iPreviewWidth - The maximum width of the preview file (in pixels). Set this to -1 to use the default size.
	  \param p_iPreviewHeight - The maximum height of the preview file (in pixels). Set this to -1 to use the default size.
	  \return Whether the command executed successfully.
	*/
	virtual bool renderBitmap(const wchar_t *p_pcTIFFInputPath, const wchar_t *p_pcBitmapOutputPath, double p_dOutputWidth, double p_dOutputHeight, int p_iPreviewWidth, int p_iPreviewHeight) = 0;
	/*
	  This sends the command which renders the loaded document in a variable way.
	  This can only be sent after a document has been loaded.
	  This is only valid when connected to a server.
	  \param p_iStartLabelNumber - The start label number of the render.
	  \param p_iNumberOfLabels - The number of labels that will be rendered.
	  \return Whether the command executed successfully.
	*/
	virtual bool renderLabels(int p_iStartLabelNumber, int p_iNumberOfLabels) = 0;

	/*
	  This sends the command which starts the render on demand mode.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool startRenderOnDemand(void) = 0;
	/*
	  This sends the adds a render on demand item.
	  This can only be sent while rendering on demand.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool addRenderonDemandItem(void) = 0;
	/*
	  This sends the command which ends the render on demand mode.
	  This can only be sent while rendering on demand.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool endRenderOnDemand(void) = 0;

	/*
	  This sends the command which changes the image file being loaded.
	  This is only valid when connected to a server.
	  \param p_pcImageFilePath - The path to the file to be loaded.
	  \return Whether the command executed successfully.
	*/
	virtual bool changeImageFile(const wchar_t *p_pcImageFilePath) = 0;
	/*
	  This sends the command which loads the image.
	  This can only be sent after a file to load has been defined.
	  This is only valid when connected to a server.
	  \param p_iNumberOfTimesToLoadImage - The number of times to load the image.
	  \return Whether the command executed successfully.
	*/
	virtual bool loadImage(int p_iNumberOfTimesToLoadImage) = 0;
	/*
	  This sends the command which starts the load on demand mode.
	  This can only be sent after a file to load has been defined.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool startLoadOnDemand(void) = 0;
	/*
	  This sends the command which adds a load on demand mode image.
	  This can only be sent while loading on demand.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool addLoadOnDemandImage(void) = 0;
	/*
	  This sends the command which ends the load on demand mode.
	  This can only be sent while loading on demand.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool endLoadOnDemand(void) = 0;

	/*
	  This sends the command which increments the global counter.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool incrementGlobalCounter(void) = 0;
	/*
	  This sends the command which dencrements the global counter.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool decrementGlobalCounter(void) = 0;
	/*
	  This sends the command which sets the global counter to the value specified.
	  This is only valid when connected to a server.
	  \param p_iGlobalCounter - The new value of the global counter.
	  \return Whether the command executed successfully.
	*/
	virtual bool setGlobalCounter(int p_iGlobalCounter) = 0;


	/*
	  This sends the command which copies some RIP files from a location to another folder.
	  This is only valid when connected to a server.
	  \param p_pcRIPFilesPath - The base path to the RIP files to copy. This is the base path, meaning the colour plane (e.g. _0) should not be included.
	  \param p_pcOutputFolder - The path to the folder to copy the files into. If this is a drive letter or network name only, this will be combined with the p_pcRIPFilesPath parameter to form the whole path.
	  \param p_iStartColourPlane - The lowest colour plane file to copy.
	  \param p_iEndColourPlane - The largest colour plane file to copy. Set to -1 to use the largest colour plane file available.
	  \param p_bDeleteOriginalFiles - If true, the files that were copied will be deleted once the copy has finished.
	  \return Whether the command executed successfully.
	*/
	virtual bool copyRIPFiles(const wchar_t *p_pcRIPFilesPath, const wchar_t *p_pcOutputFolder, int p_iStartColourPlane=0, int p_iEndColourPlane=-1, bool p_bDeleteOriginalFiles=false) = 0;


	/*
	  This sends the command which gets a preview of the loaded document.
	  This is only valid when connected to a server.
	  \param p_iPreviewWidth - The width of the preview (in pixels).
	  \param p_iPreviewHeight - The height of the preview (in pixels).
	  \param p_pcPreviewData - The preview data is returned in this parameter.
	  \param p_iPreviewDataLength - The length of the p_pcPreviewData buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getPreview(int p_iPreviewWidth, int p_iPreviewHeight, wchar_t *p_pcPreviewData, int p_iPreviewDataLength) = 0;
	/*
	  This returns the decimal value of a specified pixel in the preview data passed.
	  \param p_pcPreviewData - The preview data.
	  \param p_iPreviewDataLength - The length of the preview data defined in p_pcPreviewData.
	  \param p_iPixelIndex - The index of the pixel to return.
	  \param p_iPixelValue - The decimal value of the pixel is returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	virtual bool getPreviewDataPixelValue(const wchar_t *p_pcPreviewData, int p_iPreviewDataLength, int p_iPixelIndex, int &p_iPixelValue) = 0;



	/////////////////////////////////////
	// Print Controller
	/////////////////////////////////////

	/*
	  This sends the command which initialises the Print Controller with the configuration file specified.
	  This is only valid when connected to a server.
	  \param p_pcConfigurationFile - The configuration file the Print Controller will be initialised with.
	  \return Whether the command executed successfully.
	*/
	virtual bool initialisePrintController(const wchar_t *p_pcConfigurationFile) = 0;

	/*
	  This sends the command which registers this command for information from the Print Controller.
	  This is only valid when connected to a server.
	  \param p_bReturnExistingInformation - Whether to return the existing information or not.
	  \return Whether the command executed successfully.
	*/
	virtual bool registerForPrintControllerInformation(bool p_bReturnExistingInformation) = 0;


	/*
	  This sends the command which waits for the Print Server to finish USB initialising.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool waitUntilServerUSBInitialised(bool p_bCheckHeadConnections = false) = 0;


	/*
	  This sends the command which applys the print mode with the name specified.
	  This is only valid when connected to a server.
	  \param p_pcPrintMode - The name of the print mode to apply.
	  \return Whether the command executed successfully.
	*/
	virtual bool applyPrintMode(const wchar_t *p_pcPrintMode) = 0;
	/*
	  This sends the command which gets the names of all print modes.
	  This is only valid when connected to a server.
	  \param p_pcPrintModeNames - The print mode names will be returned in this parameter.
	  \param p_iPrintModeNamesLength - The length of the p_pcRenderModeNames buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getPrintModeNames(wchar_t *p_pcPrintModeNames, int p_iPrintModeNamesLength) = 0;

	/*
	  This sends the command which sets the parameter in the Print Controller with the name specified to the
	  value specified.
	  This is only valid when connected to a server.
	  \param p_pcParameterName - The name of the parameter to change.
	  \param p_pcParameterValue - The new value of the parameter.
	  \return Whether the command executed successfully.
	*/
	virtual bool setPrintControllerParameterValue(const wchar_t *p_pcParameterName, const wchar_t *p_pcParameterValue) = 0;
	/*
	  This sends the command which sets a number of parameters in the Print Controller at the same time.
	  This is only valid when connected to a server.
	  \param p_iNumberOfParametersToChange - The number of entries in the p_ppcParameterNames and p_pcParameterValues arrays.
	  \param p_ppcParameterNames - An array of of the parameter names to change. The parameters will be changed in the defined order.
	  \param p_pcParameterValues - A array of of the new parameter values. Each entry corresponds to the equalivent entry in p_vsParameterNames and so the size of p_vsParameterValues must equal the size of p_vsParameterNames.
	  \return Whether the command executed successfully.
	*/
	virtual bool setPrintControllerParameterValues(int p_iNumberOfParametersToChange, const wchar_t **p_ppcParameterNames, const wchar_t **p_pcParameterValues) = 0;
	/*
	  This sets the Print Controller parameter values based on the node information provided.
	  This is only valid when connected to a server.
	  \param p_pcNodePath - The path of the node to get the informtion for.
	  \param p_pcNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	virtual bool setPrintControllerNodeInformation(const WCHAR *p_psNodePath, const WCHAR *p_pcNodeInformation) = 0;
	/*
	  This sends the command which gets the value of the parameter in the Print Controller with the name specified.
	  This is only valid when connected to a server.
	  \param p_pcParameterName - The name of the parameter to change.
	  \param p_pcParameterValue - The value of the parameter is returned in this parameter.
	  \param p_iParameterValueLength - The length of the p_pcParameterValue buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getPrintControllerParameterValue(const wchar_t *p_pcParameterName, wchar_t *p_pcParameterValue, int p_iParameterValueLength) = 0;
	/*
	  This sends the command which gets the information about the Print Controller node specified.
	  This is only valid when connected to a server.
	  \param p_pcNodePath - The path of the node to get the informtion for.
	  \param p_pcExcludedNodeTypes - A comma separated list of node types to be excluded from the information.
	  \param p_ppcNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	virtual bool getPrintControllerNodeInformation(const wchar_t *p_pcNodePath, const wchar_t *p_pcExcludedNodeTypes, wchar_t **p_ppcNodeInformation) = 0;


	/*
	  This sends the command which aborts the Print Controller.
	  This is only valid when connected to a server.
	  \param p_bSendInNewClient - If this is true, a new client connection will be made to the Print Server and the abort will be sent using this. This is necessary if a command from this client is already being processed by the Print Server.
	  \return Whether the command executed successfully.
	*/
	virtual bool abortPrintController(bool p_bSendInNewClient=false) = 0;


	/*
	  This sends the command which prints an item.
	  This is only valid when connected to a server.
	  \param p_iNumberOfPrints - The number of times to print.
	  \param p_pcRIPFileItemPath - The path to the base file name for the item to print.
	  \param p_iNumberOfCopies - The number of copies of the item in the print.
	  \return Whether the command executed successfully.
	 */
	virtual bool print(int p_iNumberOfPrints, const wchar_t *p_pcRIPFileItemPath, int p_iNumberOfCopies) = 0;
	/*
	  This sends the command which prints a number of items.
	  This is only valid when connected to a server.
	  \param p_iNumberOfPrints - The number of times to print.
	  \param p_iNumberOfItems - The number of items in the p_ppcRIPFileItemPaths and p_piNumberOfCopies arrays.
	  \param p_ppcRIPFileItemPaths - An array of paths to base file names to print. The files will be printed in the defined order.
	  \param p_piNumberOfCopies - An array of the number of copies of base file name that will be printed. Each entry corresponds to the equalivent entry in p_vsRIPFileItemPaths and so the size of p_vsRIPFileItemPaths must equal the size of p_viNumberOfCopies.
	  \return Whether the command executed successfully.
	 */
	virtual bool print(int p_iNumberOfPrints, int p_iNumberOfItems, wchar_t **p_ppcRIPFileItemPaths, int *p_piNumberOfCopies) = 0;
	/*
	  This sends the command which initialises a print of an item.
	  This is only valid when connected to a server.
	  \param p_iNumberOfPrints - The number of times to print.
	   \param p_pcRIPFileItemPath - The path to the base file name for the item to print.
	  \param p_iNumberOfCopies - The number of copies of the item in the print.
	  \return Whether the command executed successfully.
	 */
	virtual bool initialisePrint(int p_iNumberOfPrints, const wchar_t *p_pcRIPFileItemPath, int p_iNumberOfCopies) = 0;
	/*
	  This sends the command which initialises a print of a number of items.
	  This is only valid when connected to a server.
	  \param p_iNumberOfPrints - The number of times to print.
	  \param p_iNumberOfItems - The number of items in the p_ppcRIPFileItemPaths and p_piNumberOfCopies arrays.
	  \param p_ppcRIPFileItemPaths - An array of paths to base file names to print. The files will be printed in the defined order.
	  \param p_piNumberOfCopies - An array of the number of copies of base file name that will be printed. Each entry corresponds to the equalivent entry in p_vsRIPFileItemPaths and so the size of p_vsRIPFileItemPaths must equal the size of p_viNumberOfCopies.
	  \return Whether the command executed successfully.
	 */
	virtual bool initialisePrint(int p_iNumberOfPrints, int p_iNumberOfItems, wchar_t **p_ppcRIPFileItemPaths, int *p_piNumberOfCopies) = 0;
	/*
	  This sends the command which starts the initialised print.
	  This can only be sent once the command to initialise a print has been sent.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	virtual bool startInitialisedPrint(void) = 0;
	/*
	  This sends the command which does not return until the print has fully initialised.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	virtual bool waitForPrintInitialised(void) = 0;
	/*
	  This sends the command which does not return until the print has fully finished.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	virtual bool waitForPrintFinished(void) = 0;

	/*
	  This sends the command which starts the print queue mode.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	virtual bool startPrintQueueMode(void) = 0;
	/*
	  This sends the command which adds a print queue item.
	  This can only be sent when in print queue mode.
	  This is only valid when connected to a server.
	  \param p_pcRIPFileItemPath - The path to the base file name for the print queue item to print.
	  \param p_iNumberOfCopies - The number of copies of the base file name in the print queue item.
	  \return Whether the command executed successfully.
	 */
	virtual bool addPrintQueueItem(const wchar_t *p_pcRIPFileItemPath, int p_iNumberOfCopies) = 0;
	/*
	  This sends the command which adds a print queue item with a number of file items.
	  This can only be sent when in print queue mode.
	  This is only valid when connected to a server.
	  \param p_iNumberOfItems - The number of items in the p_ppcRIPFileItemPaths and p_piNumberOfCopies arrays.
	  \param p_ppcRIPFileItemPaths - An array of paths to base file names to print. The files will be printed in the defined order.
	  \param p_piNumberOfCopies - An array of the number of copies of base file name that will be printed. Each entry corresponds to the equalivent entry in p_vsRIPFileItemPaths and so the size of p_vsRIPFileItemPaths must equal the size of p_viNumberOfCopies.
	  \return Whether the command executed successfully.
	 */
	virtual bool addPrintQueueItem(int p_iNumberOfItems, wchar_t **p_ppcRIPFileItemPaths, int *p_piNumberOfCopies) = 0;
	/*
	  This sends the command which adds a spit queue item.
	  This can only be sent when in print queue mode.
	  This is only valid when connected to a server.
	  \param p_dFrequency - The frequency od the spit in Hertz i.e. the number of times the printheads will fire every second.
	  \param p_dDuration - The duration of the spit (in seconds).
	  \param p_iGreyLevel - The grey level to spit with. -1 will just use the default value in the config.
	  \return Whether the command executed successfully.
	 */
	virtual bool addSpitQueueItem(double p_dFrequency, double p_dDuration, int p_iGreyLevel=-1) = 0;
	/*
	  This sends the command which ends the print queue mode.
	  This can only be sent when in print queue mode.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	virtual bool endPrintQueueMode(void) = 0;

	/*
	  This sends the command which pauses the print.
	  This can only be sent when printing.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	virtual bool pausePrint(void) = 0;
	/*
	  This sends the command which resumes a paused print.
	  This can only be sent when printing and after the print has been paused.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	virtual bool resumePrint(void) = 0;

	/*
	  This sends the command which sends a software print go to all connected PMBs.
	  This can only be sent when printing.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	virtual bool sendSoftwarePrintGo(void) = 0;


	/*
	  This sends the command which starts a spit in all ocnnected and enabled printheads.
	  \param p_dFrequency - The frequency od the spit in Hertz i.e. the number of times the printheads will fire every second.
	  \param p_dDuration - The duration of the spit (in seconds).
	  \param p_iGreyLevel - The Grey Level for the spit (-1 results in the default from the config being used).
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	virtual bool startSpit(double p_dFrequency, double p_dDuration, int p_iGreyLevel = -1) = 0;
	/*
	  This sends the command which stops the current spit.
	  This can only be sent when spitting.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	virtual bool stopSpit(void) = 0;
	/*
	  This sends the command which waits for the spit to finish.
	  This can only be sent when spitting.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	virtual bool waitForSpitFinished(void) = 0;


	/*
	  This sends the command which deletes the print data directories specified.
	  This is only valid when connected to a server.
	  \param p_pcDirectories - The paths to the directories to delete. More than one directory can be specified as a comma separated list.
	  \return Whether the command executed successfully.
	*/
	virtual bool deletePrintDataDirectories(const wchar_t *p_pcDirectories) = 0;
	/*
	  This sends the command which deletes the print data files specified.
	  This is only valid when connected to a server.
	  \param p_pcFilePaths - The paths to the files to delete. More than one file can be specified as a comma separated list.
	  \return Whether the command executed successfully.
	*/
	virtual bool deletePrintDataFiles(const wchar_t *p_pcFilePaths) = 0;
	

	/*
	  This sends the command which jogs the transport mechanism.
	  This is only valid when connected to a server.
	  \param p_iDirection - The jog direction.
							0 – Zero X.
							1 – Limit X.
							2 – Zero Y.
							3 – Limit Y.
							4 - Zero Z.
							5 - Limit Z.
	  \param p_iSpeed - The jog speed.
						0 - Slow.
						1 - Fast.
	  \return Whether the command executed successfully.
	*/
	virtual bool jogTransportMechanism(int p_iDirection, int p_iSpeed) = 0;
	/*
	  This sends the command which moves the transport mechanism to a pre-set position.
	  This is only valid when connected to a server.
	  \param p_iDestination - The destination.
							  0 - Home.
							  1 – Start position.
							  2 – Load substrate position.
							  3 – Maintenance position.
							  4 – User position.
	  \return Whether the command executed successfully.
	*/
	virtual bool moveTransportMechanismToDestination(int p_iDestination) = 0;
	/*
	  This sends the command which moves the transport mechanism to the defined position.
	  This is only valid when connected to a server.
	  \param p_dXPosition - The x position to move to (in mm).
	  \param p_dYPosition - The y position to move to (in mm).
	  \param p_dXSpeed - The x speed to move at (in mm/s).
	  \param p_dYSpeed - The y speed to move at (in mm/s).
	  \param p_dZPosition - The z position to move to (in mm). Set to -1.0 if there is no z-axis.
	  \param p_dZSpeed - The z speed to move at (in mm/s). Set to -1.0 if there is no z-axis.
	  \return Whether the command executed successfully.
	*/
	virtual bool moveTransportMechanismToPosition(double p_dXPosition, double p_dYPosition, double p_dXSpeed, double p_dYSpeed, double p_dZPosition=-1.0, double p_dZSpeed=-1.0) = 0;


	/*
	  This sends the command which turns purges the ink system.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool purgeInkSystem(void) = 0;


	/*
	  This sends the command which turns the printheads on.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool turnPrintheadsOn(void) = 0;
	/*
	  This sends the command which turns the printheads off.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool turnPrintheadsOff(void) = 0;
	/*
	  This sends the command which sets the printhead target temperatures to the value specified.
	  This is only valid when connected to a server.
	  \param p_dTemperature - The new target temperature (in degrees celcius) of the printheads.
	  \return Whether the command executed successfully.
	*/
	virtual bool setPrintheadTargetTemperatures(double p_dTemperature) = 0;
	/*
	  This sends the command which cases the printhead status to be read.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool readPrintheadStatus(void) = 0;


	/*
	  This sends the command which upgrades the head firmware.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	virtual bool upgradeHeadFirmware(void) = 0;


	/*
	  This sends the command which gets the value of the node error state codes in the Print Controller for the
	  nodes with the name specified.
	  This is only valid when connected to a server.
	  \param p_pcNodeNames - The names of the nodes.
	  \param p_pcErrorCodes - The error codes are returned in this parameter.
	  \param p_iErrorCodesLength  - The length of the p_pcErrorCodesbuffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getPrintControllerNodeErrorState(const wchar_t *p_pcNodeNames, wchar_t *p_pcErrorCodes, int p_iErrorCodesLength) = 0;



	/////////////////////////////////////
	// Network Controller
	/////////////////////////////////////

	/*
	  This sends the command which initialises the Network Controller with the configuration file specified.
	  This is only valid when connected to a server.
	  \param p_pcConfigurationFile - The configuration file the Network Controller will be initialised with.
	  \return Whether the command executed successfully.
	*/
	virtual bool initialiseNetworkController(const wchar_t *p_pcConfigurationFile) = 0;

	/*
	  This sends the command which registers this command for information from the Network Controller.
	  This is only valid when connected to a server.
	  \param p_bReturnExistingInformation - Whether to return the existing information or not.
	  \return Whether the command executed successfully.
	*/
	virtual bool registerForNetworkControllerInformation(bool p_bReturnExistingInformation) = 0;


	/*
	  This will not return until all servers have finished initialising.
	  \return Whether the command executed successfully.
	 */
	virtual bool waitUntilAllSlaveServersInitialised(void) = 0;


	/*
	  This sends the command which sets the parameter in the Network Controller with the name specified to the
	  value specified.
	  This is only valid when connected to a server.
	  \param p_pcParameterName - The name of the parameter to change.
	  \param p_pcParameterValue - The new value of the parameter.
	  \return Whether the command executed successfully.
	*/
	virtual bool setNetworkControllerParameterValue(const wchar_t *p_pcParameterName, const wchar_t *p_pcParameterValue) = 0;
	/*
	  This sends the command which sets a number of parameters in the Network Controller at the same time.
	  This is only valid when connected to a server.
	  \param p_iNumberOfParametersToChange - The number of entries in the p_ppcParameterNames and p_pcParameterValues arrays.
	  \param p_ppcParameterNames - An array of of the parameter names to change. The parameters will be changed in the defined order.
	  \param p_pcParameterValues - A array of of the new parameter values. Each entry corresponds to the equalivent entry in p_vsParameterNames and so the size of p_vsParameterValues must equal the size of p_ppcParameterNames.
	  \return Whether the command executed successfully.
	*/
	virtual bool setNetworkControllerParameterValues(int p_iNumberOfParametersToChange, const wchar_t **p_ppcParameterNames, const wchar_t **p_pcParameterValues) = 0;
	/*
	  This sets the Network Controller parameter values based on the node information provided.
	  This is only valid when connected to a server.
	  \param p_pcNodePath - The path of the node to get the informtion for.
	  \param p_pcNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	virtual bool setNetworkControllerNodeInformation(const WCHAR *p_psNodePath, const WCHAR *p_pcNodeInformation) = 0;
	/*
	  This sends the command which gets the value of the parameter in the Network Controller with the name specified.
	  This is only valid when connected to a server.
	  \param p_pcParameterName - The name of the parameter to change.
	  \param p_pcParameterValue - The value of the parameter is returned in this parameter.
	  \param p_iParameterValueLength - The length of the p_pcParameterValue buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getNetworkControllerParameterValue(const wchar_t *p_pcParameterName, wchar_t *p_pcParameterValue, int p_iParameterValueLength) = 0;
	/*
	  This sends the command which gets the information about the Network Controller node specified.
	  This is only valid when connected to a server.
	  \param p_pcNodePath - The path of the node to get the informtion for.
	  \param p_pcExcludedNodeTypes - A comma separated list of node types to be excluded from the information.
	  \param p_ppcNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	virtual bool getNetworkControllerNodeInformation(const wchar_t *p_pcNodePath, const wchar_t *p_pcExcludedNodeTypes, wchar_t **p_ppcNodeInformation) = 0;


	/*
	  This sends the command specified to the slave server specified.
	  This is only valid when connected to the slave server.
	  \param p_pcSlaveServerName - The name of the slave server to send the message to.
	  \param p_pcMessageToBroadcast - The message to send to the slave server.
	  \param p_pcReturn - If the command returns data, it will all be returned in this parameter. This parameter should be NULL if nothing is to be returned or if the return values are not to be recorded.
	  \param p_iReturnLength - The length of the p_pcReturn buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool broadcastMessageToSlaveServer(const wchar_t *p_pcSlaveServerName, const wchar_t *p_pcMessageToBroadcast, wchar_t *p_pcReturn=NULL, int p_iReturnLength=0) = 0;
	/*
	  This sends the command specified to all of the slave servers.
	  This is only valid when connected to the slave servers.
	  \param p_pcMessageToBroadcast - The message to send to the slave server.
	  \param p_pcReturn - If no slave servers fail, the return data is contained in this parameter, otherwise the names of the slave servers which failed to execute the broadcast message are returned in this parameter as a comma separated list. This parameter should be NULL if nothing is to be returned or if the return values are not to be recorded.
	  \param p_iReturnLength - The length of the p_pcReturn buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool broadcastMessageToAllSlaveServers(const wchar_t *p_pcMessageToBroadcast, wchar_t *p_pcReturn=NULL, int p_iReturnLength=0) = 0;
	/*
	  This indicates that the next message sent should actually be sent as a Network Controller broadcast message to a specified slave server.
	  This is only valid when connected to the slave servers.
	  \param p_pcSlaveServerName - The name of the slave server to send the message to.
	  \return Whether the signal was recorded successfully.
	 */
	virtual bool broadcastNextMessageToSlaveServer(const wchar_t *p_pcSlaveServerName) = 0;
	/*
	  This indicates that the next message sent should actually be sent as a Network Controller broadcast message to all slave servers.
	  This is only valid when connected to the slave servers.
	  \return Whether the signal was recorded successfully.
	 */
	virtual bool broadcastNextMessageToAllSlaveServers(void) = 0;



	/*
	  This indicates that this server will now be a slave server.
	  \param p_pcSlaveServerName - The slave name the server will have.
	  \param p_pcMasterAddress - The address of this server (i.e. the master server).
	  \param p_iMasterPort - The port of this server (i.e. the master server).
	  \return Whether the command executed successfully.
     */
	virtual bool setAsSlaveServer(const wchar_t *p_pcSlaveServerName, const wchar_t *p_pcMasterAddress, int p_iMasterPort) = 0;
	/*
	  This indicates that this server will now no longer be be a slave server.
	  This is only valid if the receiving server is a slave.
	  \return Whether the command executed successfully.
     */
	virtual bool setAsNotSlaveServer(void) = 0;

	/*
	  This indicates that this server (i.e. the master server) has initialised.
	  This is only valid if the receiving server is a slave.
	  \return Whether the command executed successfully.
     */
	virtual bool masterInitialised(void) = 0;
	/*
	  This indicates that this server (i.e. the master server) has started a print.
	  This is only valid if the receiving server is a slave.
	  \return Whether the command executed successfully.
     */
	virtual bool masterPrintStarted(void) = 0;
	/*
	  This indicates that this server (i.e. the master server) is ready to print the slave boards.
	  This is only valid if the receiving server is a slave.
	  \param p_iTotalNumberOfSwathes - The total number of swathes to print.
	  \return Whether the command executed successfully.
     */
	virtual bool masterReadyToPrintSlaveBoards(int p_iTotalNumberOfSwathes) = 0;
	/*
	  This indicates that this server (i.e. the master server) is ready to print the master boards.
	  This is only valid if the receiving server is a slave.
	  \param p_iTotalNumberOfSwathes - The total number of swathes to print.
	  \return Whether the command executed successfully.
     */
	virtual bool masterReadyToPrintMasterBoard(int p_iTotalNumberOfSwathes) = 0;
	/*
	  This indicates that this server (i.e. the master server) has finished printing.
	  This is only valid if the receiving server is a slave.
	  \return Whether the command executed successfully.
     */
	virtual bool masterPrintFinished(void) = 0;

	/*
	  This indicates that this server (i.e. the slave server) has initialised.
	  This is only valid if the current server is a slave.
	  \param p_pcSlaveServerName - The slave name of this server.
	  \param p_bInitialisationSuccess - Whether the initialisation was successful or not.
	  \return Whether the command executed successfully.
     */
	virtual bool slaveInitialised(const wchar_t *p_pcSlaveServerName, bool p_bInitialisationSuccess) = 0;
	/*
	  This indicates that this server (i.e. the slave server) has started a print.
	  This is only valid if the current server is a slave.
	  \param p_pcSlaveServerName - The slave name of this server.
	  \return Whether the command executed successfully.
     */
	virtual bool slavePrintStarted(const wchar_t *p_pcSlaveServerName) = 0;
	/*
	  This indicates that this server (i.e. the slave server) is ready to print.
	  This is only valid if the current server is a slave.
	  \param p_pcSlaveServerName - The slave name of this server.
	  \param p_iNumberOfSwathesToPrint - The number of swathes to print.
	  \return Whether the command executed successfully.
     */
	virtual bool slaveReadyToPrint(const wchar_t *p_pcSlaveServerName, int p_iNumberOfSwathesToPrint) = 0;
	/*
	  This indicates that this server (i.e. the slave server) has processed a swathe.
	  This is only valid if the current server is a slave.
	  \param p_pcSlaveServerName - The slave name of this server.
	  \param p_iSwatheProcessed - The swathe processed.
	  \return Whether the command executed successfully.
     */
	virtual bool slaveSwatheProcessed(const wchar_t *p_pcSlaveServerName, int p_iSwatheProcessed) = 0;
	/*
	  This indicates that this server (i.e. the slave server) has changed the swathe number.
	  This is only valid if the current server is a slave.
	  \param p_pcSlaveServerName - The slave name of this server.
	  \param p_iSwatheNumber - The swathe number.
	  \return Whether the command executed successfully.
     */
	virtual bool slaveSwatheNumber(const wchar_t *p_pcSlaveServerName, int p_iSwatheNumber) = 0;
	/*
	  This indicates that this server (i.e. the slave server) has finished a print.
	  This is only valid if the current server is a slave.
	  \param p_pcSlaveServerName - The slave name of this server.
	  \param p_bPrintSuccess - Whether the print was successful or not.
	  \return Whether the command executed successfully.
     */
	virtual bool slavePrintFinished(const wchar_t *p_pcSlaveServerName, bool p_bPrintSuccess) = 0;
	/*
	  This indicates that this server (i.e. the slave server) has logged a message.
	  This is only valid if the current server is a slave.
	  \param p_pcSlaveServerName - The slave name of this server.
	  \param p_iLevel - The log level.
	  \param p_pcMessage - The message to log.
	  \return Whether the command executed successfully.
     */
	virtual bool slaveLog(const wchar_t *p_pcSlaveServerName, int p_iLevel, const wchar_t *p_pcMessage) = 0;


#if 0
	/*
	  This sends the command which aborts the Network Controller.
	  This is only valid when connected to a server.
	  \param p_bSendInNewClient - If this is true, a new client connection will be made to the Print Server and the abort will be sent using this. This is necessary if a command from this client is already being processed by the Print Server.
	  \return Whether the command executed successfully.
	*/
	virtual bool abortNetworkController(bool p_bSendInNewClient=false) = 0;
#endif



	/////////////////////////////////////
	// Print Server Monitor
	/////////////////////////////////////

	/*
	  This sends the command which initialises the Print Server Monitor with the configuration file specified.
	  This is only valid when connected to a print server monitor.
	  \param p_pcConfigurationFile - The configuration file the Render Engine will be initialised with.
	  \return Whether the command executed successfully.
	*/
	virtual bool initialisePrintServerMonitor(const wchar_t *p_pcConfigurationFile) = 0;


	/*
	  This sends the command which sets the parameter in the Print Server Monitor with the name specified to the
	  value specified.
	  This is only valid when connected to a print server monitor.
	  \param p_pcParameterName - The name of the parameter to change.
	  \param p_pcParameterValue - The new value of the parameter.
	  \return Whether the command executed successfully.
	*/
	virtual bool setPrintServerMonitorParameterValue(const wchar_t *p_pcParameterName, const wchar_t *p_pcParameterValue) = 0;
	/*
	  This sends the command which sets a number of parameters in the Print Server Monitor at the same time.
	  This is only valid when connected to a print server monitor.
	  \param p_iNumberOfParametersToChange - The number of entries in the p_ppcParameterNames and p_pcParameterValues arrays.
	  \param p_ppcParameterNames - An array of of the parameter names to change. The parameters will be changed in the defined order.
	  \param p_pcParameterValues - A array of of the new parameter values. Each entry corresponds to the equalivent entry in p_vsParameterNames and so the size of p_vsParameterValues must equal the size of p_vsParameterNames.
	  \return Whether the command executed successfully.
	*/
	virtual bool setPrintServerMonitorParameterValues(int p_iNumberOfParametersToChange, const wchar_t **p_ppcParameterNames, const wchar_t **p_pcParameterValues) = 0;
	/*
	  This sets the Print Server Monitor parameter values based on the node information provided.
	  This is only valid when connected to a server.
	  \param p_pcNodePath - The path of the node to get the informtion for.
	  \param p_pcNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	virtual bool setPrintServerMonitorNodeInformation(const WCHAR *p_psNodePath, const WCHAR *p_pcNodeInformation) = 0;
	/*
	  This sends the command which gets the value of the parameter in the Print Server Monitor with the name specified.
	  This is only valid when connected to a print server monitor.
	  \param p_pcParameterName - The name of the parameter to change.
	  \param p_pcParameterValue - The value of the parameter is returned in this parameter.
	  \param p_iParameterValueLength - The length of the p_pcParameterValue buffer.
	  \return Whether the command executed successfully.
	*/
	virtual bool getPrintServerMonitorParameterValue(const wchar_t *p_pcParameterName, wchar_t *p_pcParameterValue, int p_iParameterValueLength) = 0;
	/*
	  This sends the command which gets the information about the Print Server Monitor node specified.
	  This is only valid when connected to a server.
	  \param p_pcNodePath - The path of the node to get the informtion for.
	  \param p_pcExcludedNodeTypes - A comma separated list of node types to be excluded from the information.
	  \param p_ppcNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	virtual bool getPrintServerMonitorNodeInformation(const wchar_t *p_pcNodePath, const wchar_t *p_pcExcludedNodeTypes, wchar_t **p_ppcNodeInformation) = 0;


	/*
	  This sends the command which aborts the Print Server Monitor.
	  This is only valid when connected to a print server monitor.
	  \param p_bSendInNewClient - If this is true, a new client connection will be made to the Print Server Monitor and the abort will be sent using this. This is necessary if a command from this client is already being processed by the Print Server Monitor.
	  \return Whether the command executed successfully.
	*/
	virtual bool abortPrintServerMonitor(bool p_bSendInNewClient=false) = 0;


	/*
	  This sends the command which ahuts down the Print Server Monitor.
	  This is only valid when connected to a print server monitor.
	  \param p_pcPCOperationCode - The code of the operation to perform on the PC after the PC has finished shutting down.
	  \return Whether the command executed successfully.
	*/
	virtual bool shutdownOperationPrintServerMonitor(const wchar_t *p_pcPCOperationCode) = 0;


	/*
	  This sends the command which starts launches a Print Server with the properties specified.
	  This is only valid when connected to a print server monitor.
	  \param p_pcPrintServerName - The name of the server.
	  \param p_pcConfigurationPath - The configuration the server will use.
	  \param p_iPort - The TCP/IP port the server will run on.
	  \param p_bStartMinimised - Whether the server should start as minimised or not.
	  \param p_bConnectedToPMB - Whether this server will communicate with a PMB or not.
	  \return Whether the command executed successfully.
	*/
	virtual bool launchPrintServerMonitorPrintServer(const wchar_t *p_pcPrintServerName, const wchar_t *p_pcConfigurationPath, int p_iPort, bool p_bStartMinimised, bool p_bConnectedToPMB) = 0;
	/*
	  This sends the command which starts launches Print Servers with the properties specified.
	  This is only valid when connected to a print server monitor.
	  \param p_iNumberOfPrintServersToLaunch - The number of entries in the p_ppcPrintServerName, p_ppcConfigurationPath, p_piPort and p_pbStartMinimised arrays.
	  \param p_ppcPrintServerNames - The names of the servers.
	  \param p_ppcConfigurationPaths - The configurations the servers will use.
	  \param p_piPorts - The TCP/IP ports the servers will run on.
	  \param p_pbStartMinimised - Whether the servers should start as minimised or not.
	  \param p_pbConnectedToPMB - Whether this server will communicate with a PMB or not.
	  \return Whether the command executed successfully.
	*/
	virtual bool launchPrintServerMonitorPrintServers(int p_iNumberOfPrintServersToLaunch, const wchar_t **p_ppcPrintServerNames, const wchar_t **p_ppcConfigurationPaths, int *p_piPorts, bool *p_pbStartMinimised, bool *p_pbConnectedToPMB) = 0;
	/*
	  This sends the command which starts the Print Server with the name specified.
	  This is only valid when connected to a print server monitor.
	  \param p_pcPrintServerName - The name of the Print Server to start.
	  \return Whether the command executed successfully.
	*/
	virtual bool startPrintServerMonitorPrintServer(const wchar_t *p_pcPrintServerName) = 0;
	/*
	  This sends the command which shuts down the Print Server with the name specified.
	  This is only valid when connected to a print server monitor.
	  \param p_pcPrintServerName - The name of the Print Server to shutdown.
	  \return Whether the command executed successfully.
	*/
	virtual bool shutdownPrintServerMonitorPrintServer(const wchar_t *p_pcPrintServerName) = 0;
	/*
	  This sends the command which restarts the Print Server with the name specified.
	  This is only valid when connected to a print server monitor.
	  \param p_pcPrintServerName - The name of the Print Server to restart.
	  \return Whether the command executed successfully.
	*/
	virtual bool restartPrintServerMonitorPrintServer(const wchar_t *p_pcPrintServerName) = 0;


	virtual bool jobManagerSubmitJob(int p_iNumberOfCopies, const wchar_t *p_pcFileName, const int p_iStartLabel, const int p_iNumberOfLabels, const wchar_t *p_pcSystemMode, int *p_piJobID) = 0;

	virtual bool jobManagerDeleteJob(int p_piJobID) = 0;

};


typedef ITCP_IPClientInterface* (*getTCP_IPClientInterface_func)();