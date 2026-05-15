///////////////////////////////////////////////////////////////////////////////////////////////
//
// Copyright (C) 2007-2011 Global Inkjet Systems Ltd.
//
// E-mail: support@globalinkjetsystems.com
//
// Web:	http://www.globalinkjetsystems.com/
//
// CPPTCP_IPClient.h
//
// Version: 1.5
// 
///////////////////////////////////////////////////////////////////////////////////////////////


#pragma once

#include "TCP_IPClientInterface.h"
#include "Afxmt.h"
#include "afxwin.h"
#include "shlwapi.h"
#include <map>
#include <process.h>
#include <sstream>
#include <string>
#include <vector>

using namespace std;


/*
  This class provides a simple C++ client interface by wrapping the C++ comms DLL.
 */
class CCPPTCP_IPClient
{
protected:
	HINSTANCE m_xCPPAPIDll;								// The handle to the TCP/IP comms DLL.
	ITCP_IPClientInterface *m_pxTCP_IPClient;			// The pointer to the TCP_IPClientInterface, which provides the entry point to the TCP/IP comms DLL.


	ITCP_IPClientListenerInterface *m_pxTCP_IPClientListener;	// This is the listener object which contains the functions which should be called when various events in the TCP/IP comms occur.


	ILogManagerInterface *m_pxLogManager;		// The log manager which is used to log the client.
	bool m_bCreatedLogManager;					// Whether the log manager was created (and must therefore be destroyed) by this class or not.


protected:
	/*
	  This loads the TCP/IP comms DLL.
	  This will free any previously loaded TCP/IP comms DLL.
	  \return Whether the load was successful or not.
	 */
	bool loadTCP_IPCommsDLL(void);
	/*
	  This frees the loaded TCP/IP comms DLL.
	  This is only relavent if the TCP/IP comms DLL has been loaded.
	  \return Whether the free was successful or not.
	 */
	bool freeTCP_IPCommsDLL(void);


	/*
	  This wraps the interfaceLogCallback() function in a CCPPTCP_IPClient object so that it can be used in a callback function.
	  \param p_pvCPPTCP_IPClientObject - The pointer to the CCPPTCP_IPClient object that the interfaceLogCallback function will be called on.
	  \param p_eLogLevel - The log level (as defined by ELOG_LEVEL in ILogManagerInterface).
	  \param p_pcMessage - The message to log.
	  \return Whether the message was logged successfully or not.
	 */
	static bool wrapperToInterfaceLogCallback(void *p_pvCPPTCP_IPClientObject, ILogManagerInterface::ELOG_LEVEL p_eLogLevel, const WCHAR *p_sMessage);
	/*
	  This writes a message to the interface log.
	  \param p_eLogLevel - The log level (as defined by ELOG_LEVEL in ILogManagerInterface).
	  \param p_pcMessage - The message to log.
	  \return Whether the message was logged successfully or not.
	 */
	bool interfaceLogCallback(ILogManagerInterface::ELOG_LEVEL p_eLogLevel, const WCHAR *p_pcMessage) const;



public:
	/*
	  The constructor for the CCPPTCP_IPClient class, which initialises the variables.
	 */
	CCPPTCP_IPClient(void);
	/*
	  The destructor for the CCPPTCP_IPClient class.
	 */
	~CCPPTCP_IPClient(void);


	/*
	  This initialises the TCP/IP client using an existing log manager (which can be NULL).
	  This must be called only one, before any other functions are called.
	  \param p_pxSecurityManager - The security manager is used to perform security actions, such as connect to a locked Print Server.
	  \param p_pxLogManager - The log manager which is used to log the client.
	  \return Whether the initialisation was successful or not.
	 */
	bool initialise(ISecurityManagerInterface *p_pxSecurityManager=NULL, ILogManagerInterface *p_pxLogManager=NULL);
	/*
	  This initialises the TCP/IP client, and automatically creates a log manager (which must be deleted).
	  This must be called only one, before any other functions are called.
	  \param p_bLogToFileEnabled - Whether logging to file is enabled or not.
	  \param p_sLogFile - The path to thr log file.
	  \param p_bLogToInterfaceEnabled - Whether messages should be logged to the interface or not.
	  \return Whether the initialisation was successful or not.
	 */
	bool initialise(bool p_bLogToFileEnabled, wstring p_sLogFile, bool p_bLogToInterfaceEnabled);
	/*
	  This aborts the TCP/IP client.
	  This must be called only one, and no other functions must be called after it is called.
	  \return Whether the abort was successful or not.
	 */
	bool abort(void);


	/*
	  This connects to the server with the properties specified.
	  A successful connection must be made before any function which communicates with the server is called.
	  This is only valid when not connected to a server.
	  \param p_sIPAddress - The IP address of the server to connect to. Use "localhost" if the server is on the same computer.
	  \param p_iPort - The network port of the server to connect to.
	  \param p_iConnectionTimeout - The maximum time in milliseconds the function will wait until the connection has been established. This parameter should be set to -1 to have no timeout i.e. the function will wait until the connection is established or it is stopped. This parameter should be set to 0 to have no wait i.e. the functon will fail if a connection cannot be made immediately.
	  \return Whether the connection was successful or not.
	 */
	bool connectToServer(wstring p_sIPAddress, int p_iPort, int p_iConnectionTimeout=0);
	/*
	  This disconnects from the server the client is currently connected to.
	  This is only valid when connected to a server.
	  \return Whether the disconnection was successful or not.
	 */
	bool disconnectFromServer(bool bFreeTCP = false);
	/*
	  This sets whether the
	  \param p_bShutdownServerOnDisconnect - Whether the server should be shutdown on disconnect.
	  \return Whether the signal was set correctly or not.
	 */
	bool setShutdownServerOnDisconnect(bool p_bShutdownServerOnDisconnect);
	/*
	  This blocks until the connection has fully completed.
	  \return Whether the wait until connection executed successfully or not.
	 */
	bool waitUntilConnectedToServer(void) const;
	/*
	  This returns whether the client is currently connected to a server or not.
	  \return Whether the client is currently connected to a server or not.
	 */
	bool isConnectedToServer(void) const;


	/*
	  This sends a message to the server the client is connected to.
	  This is only valid when connected to a server.
	  \param p_sMessage - The message to send.
	  \param p_bWaitForComplete - If this is true the command will not return until the complete command associated with the command being sent is returned.
	  \return Whether the send was successful or not.
	 */
	bool sendMessage(wstring p_sMessage, bool p_bWaitForComplete=true);
	/*
	  This sends a message to the server the client is connected to.
	  As this function records the returned data, it will always wait until the complete message for the command sent is returned from the server.
	  This is only valid when connected to a server.
	  \param p_sMessage - The message to send.
	  \param p_sReturn - If the command returns data, it will all be returned in this parameter.
	  \return Whether the send was successful or not.
	 */
	bool sendMessage(wstring p_sMessage, wstring &p_sReturn);
	/*
	  This sends a message to the server, but by creating a new client (using the same settings as the current client). This may be needed as the Print Server will only process once command from
	  a client at a time, but some commands (such as aborts) may need to be sent in parallel.
	  This is only valid when connected to a server.
	  \param p_sMessage - The message to send.
	  \return Whether the send was successful or not.
	 */
	bool sendMessageInNewClient(wstring p_sMessage);
	/*
	  This sends a message to the server, but by creating a new client (using the same settings as the current client). This may be needed as the Print Server will only process once command from
	  a client at a time, but some commands (such as aborts) may need to be sent in parallel.
	  This is only valid when connected to a server.
	  \param p_sMessage - The message to send.
	  \param p_sReturn - If the command returns data, it will all be returned in this parameter.
	  \return Whether the send was successful or not.
	 */
	bool sendMessageInNewClient(wstring p_sMessage, wstring &p_sReturn);
	/*
	  This reads a response from the server the client is connected to.
	  This is only valid when connected to a server.
	  \param p_sResponse - The data read will be returned in this parameter.
	  \return Whether the read was successful or not.
	 */
	bool readFromServer(wstring &p_sResponse);


	/*
	  This takes the data from a printhead status information message and returns a map which links the printhead name to the information.
	  The IPrintheadInformationInterface objects returned must be deleted by the calling function.
	  \param p_sPrintheadStatusInformation - The printhead status information.
	  \param p_mspxPrintheadInformation - The printheads and associated information. The map is of the form <Printhead Name, Information>.
	  \return Whether the parse was successful or not.
	 */
	bool parsePrintheadInformation(wstring p_sPrintheadStatusInformation, map<wstring, IPrintheadInformationInterface *> &p_mspxPrintheadInformation) const;

	
	/*
	  This function provides an example of how to send two get print controller parameter commands in parallel, wait
      for them to complete and then separate out the responses.
      This example can only be run when currently connected to a server.
	  This example requires that at least one parameter in the Print Server has the unique ID "UniqueID1", "UniqueID2", "UniqueID3" and "UniqueID4".
	  \return Whether the example executed successfully or not.
	*/
	bool example(void);


	/*
	  This sets the client listener object.
	  This class will not automatically delete this object and no delete funtion is required as C++ in not managed so the function which
	  created the listener object should be responsible for deleting it.
	  \param p_pxTCP_IPClientListener - The new client listener object.
	  \return Whether the set was successful or not.
	 */
	bool setTCP_IPClientListener(ITCP_IPClientListenerInterface *p_pxTCP_IPClientListenerInterface);


	/*
	  This returns a Log Manager object.
	  \return A Log Manager object.
	 */
	ILogManagerInterface * getLogManager(void) const;
	/*
	  This writes a message to the log.
	  \param p_eLogLevel - The log level (as defined by ELOG_LEVEL in ILogManagerInterface).
	  \param p_sMessage - The message to log.
	  \return Whether the message was logged successfully or not.
	 */
	bool logMessage(ILogManagerInterface::ELOG_LEVEL p_eLogLevel, wstring p_sMessage) const;
	/*
	  This sets the log file path to the path specified.
	  \param p_sLogFilePath - The new log file path.
	  \return Whether the log file path was set successfully or not.
	 */
	bool setLogFilePath(wstring p_sLogFilePath);
	/*
	  This sets whether the log is enabled path to the value specified.
	  \param p_bLogEnabled - Whether the log is enabled or not.
	  \param p_bLogToInterfaceEnabled - Whether the log to interface is enabled or not.
	  \return Whether the log enabled was set successfully or not.
	 */
	bool setLogEnabled(bool p_bLogEnabled, bool p_bLogToInterfaceEnabled);
	/*
	  This updates the log settings.
	  \param p_bLogToFileEnabled - Whether logging to file is enabled or not.
	  \param p_sLogFilePath - The path to thr log file.
	  \param p_bLogToInterfaceEnabled - Whether messages should be logged to the interface or not.
	  \param p_bExtendedNetworkLogging - Whether to include the addional log messages, which will only be useful during debugging.
	  \return Whether the log settings were updated successfully or not.
	 */
	bool updateLogSettings(bool p_bLogToFileEnabled, wstring p_sLogFilePath, bool p_bLogToInterfaceEnabled, bool p_bExtendedNetworkLogging=false);
	/*
	  This returns the low log level.
	  \return The low log level.
	*/
	int getLowLogLevel(void) const;
	/*
	  This returns the warning log level.
	  \return The warning log level.
	*/
	int getWarningLogLevel(void) const;
	/*
	  This returns the error log level.
	  \return The error log level.
	*/
	int getErrorLogLevel(void) const;
	/*
	  This returns the UI info log level.
	  \return The UI info log level.
	*/
	int getUIInfoLogLevel(void) const;
	/*
	  This returns the UI warning log level.
	  \return The UI warning log level.
	*/
	int getUIWarningLogLevel(void) const;
	/*
	  This returns the UI error log level.
	  \return The UI error log level.
	*/
	int getUIErrorLogLevel(void) const;



	/////////////////////////////////////
	// Print Server
	/////////////////////////////////////

	/*
	  This sends the command which initialises the Print Server with the configuration file specified.
	  This is only valid when connected to a server.
	  \param p_sConfigurationFile - The configuration file the Prin Server will be initialised with.
	  \return Whether the command executed successfully.
	*/
	bool initialisePrintServer(wstring p_sConfigurationFile);
	/*
	  This sends the command which waits for the Print Server to finish initialising.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool waitUntilPrintServerInitialised(void);

	/*
	  This sends the command which registers this command for information from the Print Server.
	  This is only valid when connected to a server.
	  \param p_bReturnExistingInformation - Whether to return the existing information or not.
	  \return Whether the command executed successfully.
	*/
	bool registerForPrintServerInformation(bool p_bReturnExistingInformation);


	/*
	  This sends the command which saves the Print Server configuration to the folder specified.
	  This is only valid when connected to a server.
	  \param p_sConfigurationFolder - The folder where the print server configuration will be saved.
	  \return Whether the command executed successfully.
	*/
	bool saveConfigurationAs(wstring p_sConfigurationFolder);
	/*
	  This sends the command which saves the Print Server configuration to the current folder.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool saveConfiguration(void);


	/*
	  This sends the command which applys the system mode with the name specified.
	  This is only valid when connected to a server.
	  \param p_sSystemModeName - The name of the system mode to apply.
	  \return Whether the command executed successfully.
	*/
	bool applySystemMode(wstring p_sSystemModeName);
	/*
	  This sends the command which gets the name of all system modes.
	  This is only valid when connected to a server.
	  \param p_sSystemModeNames - The system mode names will be returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getSystemModeNames(wstring &p_sSystemModeNames);
	/*
	  This sends the command which gets the information for the system mode specified.
	  This is only valid when connected to a server.
	  \param p_sSystemModeName - The name of the system mode to retrieve information for.
	  \param p_sRenderModeName - The name of the render mode for the system mode specified is returned in this parameter.
	  \param p_sPrintModeName - The name of the print mode for the system mode specified is returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getSystemModeInformation(wstring p_sSystemModeName, wstring &p_sRenderModeName, wstring &p_sPrintModeName);


	/*
	  This sends the command which sets the parameter in the Print Server with the name specified to the
	  value specified.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to change.
	  \param p_sParameterValue - The new value of the parameter.
	  \return Whether the command executed successfully.
	*/
	bool setPrintServerParameterValue(wstring p_sParameterName, wstring p_sParameterValue);
	/*
	  This sends the command which sets a number of parameters in the Print Server at the same time.
	  This is only valid when connected to a server.
	  \param p_vsParameterNames - A collection of the parameter names to change. The parameters will be changed in the defined order.
	  \param p_vsParameterValues - A collection of the new parameter values. Each entry corresponds to the equalivent entry in p_vsParameterNames and so the size of p_vsParameterValues must equal the size of p_vsParameterNames.
	  \return Whether the command executed successfully.
	*/
	bool setPrintServerParameterValues(vector<wstring> p_vsParameterNames, vector<wstring> p_vsParameterValues);
	/*
	  This sets the Print Server parameter values based on the node information provided.
	  This is only valid when connected to a server.
	  \param p_sNodePath - The path of the node to get the informtion for.
	  \param p_sNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	bool setPrintServerNodeInformation(wstring p_sNodePath, wstring &p_sNodeInformation);
	/*
	  This sends the command which gets the value of the parameter in the Print Server with the name specified.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to change.
	  \param p_sParameterValue - The value of the parameter is returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getPrintServerParameterValue(wstring p_sParameterName, wstring &p_sParameterValue);
	/*
	  This sends the command which gets the information about the Print Server node specified.
	  This is only valid when connected to a server.
	  \param p_sNodePath - The name of the node to ge the informtion for.
	  \param p_sExcludedNodeTypes - A comma separated list of node types to be excluded from the information.
	  \param p_sNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	bool getPrintServerNodeInformation(wstring p_sNodePath, wstring p_sExcludedNodeTypes, wstring &p_sNodeInformation);


	/*
	  This sets the parameter in the paramter store with the name specified to the value given.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to set.
	  \param p_sParameterValue - The new value of the parameter.
	  \return Whether the command executed successfully.
	*/
	bool setParameterStoreValue(wstring p_sParameterName, wstring p_sParameterValue);
	/*
	  This gets the value of the parameter in the paramter store.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to get.
	  \param p_sParameterValue - The value of the parameter is returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getParameterStoreValue(wstring p_sParameterName, wstring &p_sParameterValue);
	/*
	  This deletes the parameter in the paramter store with the name specified.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to set.
	  \return Whether the command executed successfully.
	*/
	bool deleteParameterStoreValue(wstring p_sParameterName);


	/*
	  This sends the command which aborts the Print Server.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool abortOperationPrintServer(void);
	/*
	  This sends the command which initialises the Print Server.
	  This is only valid when connected to a server.
	  \param p_sConfigurationFolder - The configuration folder used in the initialisation.
	  \return Whether the command executed successfully.
	*/
	bool initialiseOperationPrintServer(wstring p_sConfigurationFolder);
	/*
	  This sends the command which shuts down the Print Server.
	  This is only valid when connected to a server.
	  \param p_sPCOperationCode - The code of the operation to perform on the PC after the PC has finished shutting down.
	  \return Whether the command executed successfully.
	*/
	bool shutdownOperationPrintServer(wstring p_sPCOperationCode);


	/*
	  This sends the command which gets information about the Print Server.
	  This is only valid when connected to a server.
	  \param p_sSystemInformation - The system information is returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getSystemInformation(wstring &p_sSystemInformation);



	/////////////////////////////////////
	// Render Engine
	/////////////////////////////////////

	/*
	  This sends the command which initialises the Render Engine with the configuration file specified.
	  This is only valid when connected to a server.
	  \param p_sConfigurationFile - The configuration file the Render Engine will be initialised with.
	  \return Whether the command executed successfully.
	*/
	bool initialiseRenderEngine(wstring p_sConfigurationFile);

	/*
	  This sends the command which registers this command for information from the Render Engine.
	  This is only valid when connected to a server.
	  \param p_bReturnExistingInformation - Whether to return the existing information or not.
	  \return Whether the command executed successfully.
	*/
	bool registerForRenderEngineInformation(bool p_bReturnExistingInformation);


	/*
	  This sends the command which applys the render mode with the name specified.
	  This is only valid when connected to a server.
	  \param p_sRenderMode - The name of the render mode to apply.
	  \return Whether the command executed successfully.
	*/
	bool applyRenderMode(wstring p_sRenderMode);
	/*
	  This sends the command which gets the names of all render modes.
	  This is only valid when connected to a server.
	  \param p_sRenderModeNames - The render mode names will be returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getRenderModeNames(wstring &p_sRenderModeNames);

	/*
	  This sends the command which sets the parameter in the Render Engine with the name specified to the
	  value specified.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to change.
	  \param p_sParameterValue - The new value of the parameter.
	  \return Whether the command executed successfully.
	*/
	bool setRenderEngineParameterValue(wstring p_sParameterName, wstring p_sParameterValue);
	/*
	  This sends the command which sets a number of parameters in the Render Engine at the same time.
	  This is only valid when connected to a server.
	  \param p_vsParameterNames - A collection of the parameter names to change. The parameters will be changed in the defined order.
	  \param p_vsParameterValues - A collection of the new parameter values. Each entry corresponds to the equalivent entry in p_vsParameterNames and so the size of p_vsParameterValues must equal the size of p_vsParameterNames.
	  \return Whether the command executed successfully.
	*/
	bool setRenderEngineParameterValues(vector<wstring> p_vsParameterNames, vector<wstring> p_vsParameterValues);
	/*
	  This sets the Render Engine parameter values based on the node information provided.
	  This is only valid when connected to a server.
	  \param p_sNodePath - The path of the node to get the informtion for.
	  \param p_sNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	bool setRenderEngineNodeInformation(wstring p_sNodePath, wstring &p_sNodeInformation);
	/*
	  This sends the command which gets the value of the parameter in the Render Engine with the name specified.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to change.
	  \param p_sParameterValue - The value of the parameter is returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getRenderEngineParameterValue(wstring p_sParameterName, wstring &p_sParameterValue);
	/*
	  This sends the command which gets the information about the Render Engine node specified.
	  This is only valid when connected to a server.
	  \param p_sNodePath - The name of the node to ge the informtion for.
	  \param p_sExcludedNodeTypes - A comma separated list of node types to be excluded from the information.
	  \param p_sNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	bool getRenderEngineNodeInformation(wstring p_sNodeName, wstring p_sExcludedNodeTypes, wstring &p_sNodeInformation);


	/*
	  This sends the command which aborts the Render Engine.
	  This is only valid when connected to a server.
	  \param p_bSendInNewClient - If this is true, a new client connection will be made to the Print Server and the abort will be sent using this. This is necessary if a command from this client is already being processed by the Print Server.
	  \return Whether the command executed successfully.
	*/
	bool abortRenderEngine(bool p_bSendInNewClient=false);


	/*
	  This sends the command which loads the document at the path specified.
	  This is only valid when connected to a server.
	  \param p_sDocumentPath - The path to the document to load.
	  \return Whether the command executed successfully.
	*/
	bool loadDocument(wstring p_sDocumentPath);

	/*
	  This primes the currently loaded document for rendering.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool primeForRendering(void);

	/*
	  This sends the command which renders the loaded document to a set of bitmaps.
	  This can only be sent after a document has been loaded.
	  This is only valid when connected to a server.
	  \param p_sRenderOutputPath - The path to the base file name that the output will be saved to.
	  \return Whether the command executed successfully.
	*/
	bool startRender(wstring p_sRenderOutputPath);
	/*
	  This sends the command which renders an input file to a set of bitmaps.
	  This is only valid when connected to a server.
	  \param p_sInputFilePath - The path to the input file.
	  \param p_sBitmapOutputPath - The path to the base file name that the output will be saved to.
	  \param p_dOutputWidth - The width of the output (in millimetres). Set this to -1.0 to use the width specified by the input file.
	  \param p_dOutputHeight - The height of the output (in millimetres). Set this to -1.0 to use the width specified by the input file.
	  \return Whether the command executed successfully.
	*/
	bool renderBitmap(wstring p_sInputFilePath, wstring p_sBitmapOutputPath, double p_dOutputWidth=-1.0, double p_dOutputHeight=-1.0);
	/*
	  This sends the command which renders the loaded document in a variable way.
	  This can only be sent after a document has been loaded.
	  This is only valid when connected to a server.
	  \param p_iStartLabelNumber - The start label number of the render.
	  \param p_iNumberOfLabels - The number of labels that will be rendered.
	  \return Whether the command executed successfully.
	*/
	bool renderLabels(int p_iStartLabelNumber, int p_iNumberOfLabels);

	/*
	  This sends the command which starts the render on demand mode.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool startRenderOnDemand(void);
	/*
	  This sends the adds a render on demand item.
	  This can only be sent while rendering on demand.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool addRenderonDemandItem(void);
	/*
	  This sends the command which ends the render on demand mode.
	  This can only be sent while rendering on demand.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool endRenderOnDemand(void);

	/*
	  This sends the command which changes the image file being loaded.
	  This is only valid when connected to a server.
	  \param p_sImageFilePath - The path to the file to be loaded.
	  \return Whether the command executed successfully.
	*/
	bool changeImageFile(wstring p_sImageFilePath);
	/*
	  This sends the command which loads the image.
	  This can only be sent after a file to load has been defined.
	  This is only valid when connected to a server.
	  \param p_iNumberOfTimesToLoadImage - The number of times to load the image.
	  \return Whether the command executed successfully.
	*/
	bool loadImage(int p_iNumberOfTimesToLoadImage);
	/*
	  This sends the command which starts the load on demand mode.
	  This can only be sent after a file to load has been defined.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool startLoadOnDemand(void);
	/*
	  This sends the command which adds a load on demand mode image.
	  This can only be sent while loading on demand.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool addLoadOnDemandImage(void);
	/*
	  This sends the command which ends the load on demand mode.
	  This can only be sent while loading on demand.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool endLoadOnDemand(void);

	/*
	  This sends the command which increments the global counter.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool incrementGlobalCounter(void);
	/*
	  This sends the command which dencrements the global counter.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool decrementGlobalCounter(void);
	/*
	  This sends the command which sets the global counter to the value specified.
	  This is only valid when connected to a server.
	  \param p_iGlobalCounter - The new value of the global counter.
	  \return Whether the command executed successfully.
	*/
	bool setGlobalCounter(int p_iGlobalCounter);


	/*
	  This sends the command which copies some RIP files from a location to another folder.
	  This is only valid when connected to a server.
	  \param p_sRIPFilesPath - The base path to the RIP files to copy. This is the base path, meaning the colour plane (e.g. _0) should not be included.
	  \param p_sOutputFolder - The path to the folder to copy the files into. If this is a drive letter or network name only, this will be combined with the p_pcRIPFilesPath parameter to form the whole path.
	  \param p_iStartColourPlane - The lowest colour plane file to copy.
	  \param p_iEndColourPlane - The largest colour plane file to copy. Set to -1 to use the largest colour plane file available.
	  \param p_bDeleteOriginalFiles - If true, the files that were copied will be deleted once the copy has finished.
	  \return Whether the command executed successfully.
	*/
	bool copyRIPFiles(wstring p_sRIPFilesPath, wstring p_sOutputFolder, int p_iStartColourPlane=0, int p_iEndColourPlane=-1, bool p_bDeleteOriginalFiles=false);


	/*
	  This sends the command which gets a preview of the loaded document.
	  This is only valid when connected to a server.
	  \param p_iPreviewWidth - The width of the preview (in pixels).
	  \param p_iPreviewHeight - The height of the preview (in pixels).
	  \param p_sPreviewData - The preview data is returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getPreview(int p_iPreviewWidth, int p_iPreviewHeight, wstring &p_sPreviewData);



	/////////////////////////////////////
	// Print Controller
	/////////////////////////////////////

	/*
	  This sends the command which initialises the Print Controller with the configuration file specified.
	  This is only valid when connected to a server.
	  \param p_sConfigurationFile - The configuration file the Print Controller will be initialised with.
	  \return Whether the command executed successfully.
	*/
	bool initialisePrintController(wstring p_sConfigurationFile);

	/*
	  This sends the command which registers this command for information from the Print Controller.
	  This is only valid when connected to a server.
	  \param p_bReturnExistingInformation - Whether to return the existing information or not.
	  \return Whether the command executed successfully.
	*/
	bool registerForPrintControllerInformation(bool p_bReturnExistingInformation);


	/*
	  This sends the command which applys the print mode with the name specified.
	  This is only valid when connected to a server.
	  \param p_sPrintMode - The name of the print mode to apply.
	  \return Whether the command executed successfully.
	*/
	bool applyPrintMode(wstring p_sPrintMode);
	/*
	  This sends the command which gets the names of all print modes.
	  This is only valid when connected to a server.
	  \param p_sPrintModeNames - The print mode names will be returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getPrintModeNames(wstring &p_sPrintModeNames);

	/*
	  This sends the command which sets the parameter in the Print Controller with the name specified to the
	  value specified.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to change.
	  \param p_sParameterValue - The new value of the parameter.
	  \return Whether the command executed successfully.
	*/
	bool setPrintControllerParameterValue(wstring p_sParameterName, wstring p_sParameterValue);
	/*
	  This sends the command which sets a number of parameters in the Print Controller at the same time.
	  This is only valid when connected to a server.
	  \param p_vsParameterNames - A collection of the parameter names to change. The parameters will be changed in the defined order.
	  \param p_vsParameterValues - A collection of the new parameter values. Each entry corresponds to the equalivent entry in p_vsParameterNames and so the size of p_vsParameterValues must equal the size of p_vsParameterNames.
	  \return Whether the command executed successfully.
	*/
	bool setPrintControllerParameterValues(vector<wstring> p_vsParameterNames, vector<wstring> p_vsParameterValues);
	/*
	  This sets the Print Controller parameter values based on the node information provided.
	  This is only valid when connected to a server.
	  \param p_sNodePath - The path of the node to get the informtion for.
	  \param p_sNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	bool setPrintControllerNodeInformation(wstring p_sNodePath, wstring &p_sNodeInformation);
	/*
	  This sends the command which gets the value of the parameter in the Print Controller with the name specified.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to change.
	  \param p_sParameterValue - The value of the parameter is returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getPrintControllerParameterValue(wstring p_sParameterName, wstring &p_sParameterValue);
	/*
	  This sends the command which gets the information about the Print Controller node specified.
	  This is only valid when connected to a server.
	  \param p_sNodePath - The name of the node to ge the informtion for.
	  \param p_sExcludedNodeTypes - A comma separated list of node types to be excluded from the information.
	  \param p_sNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	bool getPrintControllerNodeInformation(wstring p_sNodeName, wstring p_sExcludedNodeTypes, wstring &p_sNodeInformation);


	/*
	  This sends the command which aborts the Print Controller.
	  This is only valid when connected to a server.
	  \param p_bSendInNewClient - If this is true, a new client connection will be made to the Print Server and the abort will be sent using this. This is necessary if a command from this client is already being processed by the Print Server.
	  \return Whether the command executed successfully.
	*/
	bool abortPrintController(bool p_bSendInNewClient=false);


	/*
	  This sends the command which prints an item.
	  This is only valid when connected to a server.
	  \param p_iNumberOfPrints - The number of times to print.
	  \param p_sRIPFileItemPath - The path to the base file name for the item to print.
	  \param p_iNumberOfCopies - The number of copies of the item in the print.
	  \return Whether the command executed successfully.
	 */
	bool print(int p_iNumberOfPrints, wstring p_sRIPFileItemPath, int p_iNumberOfCopies);
	/*
	  This sends the command which prints a number of items.
	  This is only valid when connected to a server.
	  \param p_iNumberOfPrints - The number of times to print.
	  \param p_vsRIPFileItemPaths - A collection of paths to base file names to print. The files will be printed in the defined order.
	  \param p_viNumberOfCopies - A collection of the number of copies of base file name that will be printed. Each entry corresponds to the equalivent entry in p_vsRIPFileItemPaths and so the size of p_vsRIPFileItemPaths must equal the size of p_viNumberOfCopies.
	  \return Whether the command executed successfully.
	 */
	bool print(int p_iNumberOfPrints, vector<wstring> p_vsRIPFileItemPaths, vector<int> p_viNumberOfCopies);
	/*
	  This sends the command which initialises a print of an item.
	  This is only valid when connected to a server.
	  \param p_iNumberOfPrints - The number of times to print.
	  \param p_sRIPFileItemPath - The path to the base file name for the item to print.
	  \param p_iNumberOfCopies - The number of copies of the item in the print.
	  \return Whether the command executed successfully.
	 */
	bool initialisePrint(int p_iNumberOfPrints, wstring p_sRIPFileItemPath, int p_iNumberOfCopies);
	/*
	  This sends the command which initialises a print of a number of items.
	  This is only valid when connected to a server.
	  \param p_iNumberOfPrints - The number of times to print.
	  \param p_vsRIPFileItemPaths - A collection of paths to base file names to print. The files will be printed in the defined order.
	  \param p_viNumberOfCopies - A collection of the number of copies of base file name that will be printed. Each entry corresponds to the equalivent entry in p_vsRIPFileItemPaths and so the size of p_vsRIPFileItemPaths must equal the size of p_viNumberOfCopies.
	  \return Whether the command executed successfully.
	 */
	bool initialisePrint(int p_iNumberOfPrints, vector<wstring> p_vsRIPFileItemPaths, vector<int> p_viNumberOfCopies);
	/*
	  This sends the command which starts the initialised print.
	  This can only be sent once the command to initialise a print has been sent.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	bool startInitialisedPrint(void);
	/*
	  This sends the command which does not return until the print has fully initialised.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	bool waitForPrintInitialised(void);
	/*
	  This sends the command which does not return until the print has fully finished.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	bool waitForPrintFinished(void);

	/*
	  This sends the command which starts the print queue mode.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	bool startPrintQueueMode(void);
	/*
	  This sends the command which adds a print queue item.
	  This can only be sent when in print queue mode.
	  This is only valid when connected to a server.
	  \param p_sRIPFileItemPath - The path to the base file name for the print queue item to print.
	  \param p_iNumberOfCopies - The number of copies of the base file name in the print queue item.
	  \return Whether the command executed successfully.
	 */
	bool addPrintQueueItem(wstring p_sRIPFileItemPath, int p_iNumberOfCopies);
	/*
	  This sends the command which adds a print queue item with a number of file items.
	  This can only be sent when in print queue mode.
	  This is only valid when connected to a server.
	  \param p_vsRIPFileItemPaths - A collection of paths to base file names to print. The files will be printed in the defined order.
	  \param p_viNumberOfCopies - A collection of the number of copies of base file name that will be printed. Each entry corresponds to the equalivent entry in p_vsRIPFileItemPaths and so the size of p_vsRIPFileItemPaths must equal the size of p_viNumberOfCopies.
	  \return Whether the command executed successfully.
	 */
	bool addPrintQueueItem(vector<wstring> p_vsRIPFileItemPaths, vector<int> p_viNumberOfCopies);
	/*
	  This sends the command which adds a spit queue item.
	  This can only be sent when in print queue mode.
	  This is only valid when connected to a server.
	  \param p_dFrequency - The frequency od the spit in Hertz i.e. the number of times the printheads will fire every second.
	  \param p_dDuration - The duration of the spit (in seconds).
	  \param p_iGreyLevel - The grey level to spit with. -1 will just use the default value in the config.
	  \return Whether the command executed successfully.
	 */
	bool addSpitQueueItem(double p_dFrequency, double p_dDuration, int p_iGreyLevel=-1);
	/*
	  This sends the command which ends the print queue mode.
	  This can only be sent when in print queue mode.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	bool endPrintQueueMode(void);

	/*
	  This sends the command which pauses the print.
	  This can only be sent when printing.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	bool pausePrint(void);
	/*
	  This sends the command which resumes a paused print.
	  This can only be sent when printing and after the print has been paused.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	bool resumePrint(void);

	/*
	  This sends the command which sends a software print go to all connected PMBs.
	  This can only be sent when printing.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	bool sendSoftwarePrintGo(void);


	/*
	  This sends the command which starts a spit in all ocnnected and enabled printheads.
	  \param p_dFrequency - The frequency od the spit in Hertz i.e. the number of times the printheads will fire every second.
	  \param p_dDuration - The duration of the spit (in seconds).
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	bool startSpit(double p_dFrequency, double p_dDuration);
	/*
	  This sends the command which stops the current spit.
	  This can only be sent when spitting.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	bool stopSpit(void);
	/*
	  This sends the command which waits for the spit to finish.
	  This can only be sent when spitting.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	 */
	bool waitForSpitFinished(void);


	/*
	  This sends the command which deletes the print data directories specified.
	  This is only valid when connected to a server.
	  \param p_sDirectories - The paths to the directories to delete. More than one directory can be specified as a comma separated list.
	  \return Whether the command executed successfully.
	*/
	bool deletePrintDataDirectories(wstring p_sDirectories);
	/*
	  This sends the command which deletes the print data files specified.
	  This is only valid when connected to a server.
	  \param p_sFilePaths - The paths to the files to delete. More than one file can be specified as a comma separated list.
	  \return Whether the command executed successfully.
	*/
	bool deletePrintDataFiles(wstring p_sFilePaths);
	

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
	bool jogTransportMechanism(int p_iDirection, int p_iSpeed);
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
	bool moveTransportMechanismToDestination(int p_iDestination);
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
	bool moveTransportMechanismToPosition(double p_dXPosition, double p_dYPosition, double p_dXSpeed, double p_dYSpeed, double p_dZPosition=-1.0, double p_dZSpeed=-1.0);


	/*
	  This sends the command which turns purges the ink system.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool purgeInkSystem(void);


	/*
	  This sends the command which turns the printheads on.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool turnPrintheadsOn(void);
	/*
	  This sends the command which turns the printheads off.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool turnPrintheadsOff(void);
	/*
	  This sends the command which sets the printhead target temperatures to the value specified.
	  This is only valid when connected to a server.
	  \param p_dTemperature - The new target temperature (in degrees celcius) of the printheads.
	  \return Whether the command executed successfully.
	*/
	bool setPrintheadTargetTemperatures(double p_dTemperature);
	/*
	  This sends the command which cases the printhead status to be read.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool readPrintheadStatus(void);


	/*
	  This sends the command which upgrades the head firmware.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool upgradeHeadFirmware(void);

	/*
	  This sends the command which upgrades the PMB firmware.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool upgradePMBFirmware(void);

	/*
	  This sends the command which verifies the PMB firmware.
	  This is only valid when connected to a server.
	  \return Whether the command executed successfully.
	*/
	bool verifyPMBFirmware(void);

	/*
	  This sends the command which gets the value of the node error state codes in the Print Controller for the
	  nodes with the name specified.
	  This is only valid when connected to a server.
	  \param p_sNodeNames - The names of the nodes.
	  \param p_sErrorCodes - The error codes are returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getPrintControllerNodeErrorState(wstring p_sNodeNames, wstring &p_sErrorCodes);



	/////////////////////////////////////
	// Network Controller
	/////////////////////////////////////

	/*
	  This sends the command which initialises the Network Controller with the configuration file specified.
	  This is only valid when connected to a server.
	  \param p_sConfigurationFile - The configuration file the Network Controller will be initialised with.
	  \return Whether the command executed successfully.
	*/
	bool initialiseNetworkController(wstring p_sConfigurationFile);

	/*
	  This sends the command which registers this command for information from the Network Controller.
	  This is only valid when connected to a server.
	  \param p_bReturnExistingInformation - Whether to return the existing information or not.
	  \return Whether the command executed successfully.
	*/
	bool registerForNetworkControllerInformation(bool p_bReturnExistingInformation);


	/*
	  This will not return until all servers have finished initialising.
	  \return Whether the command executed successfully.
	 */
	bool waitUntilAllSlaveServersInitialised(void);


	/*
	  This sends the command which sets the parameter in the Network Controller with the name specified to the
	  value specified.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to change.
	  \param p_sParameterValue - The new value of the parameter.
	  \return Whether the command executed successfully.
	*/
	bool setNetworkControllerParameterValue(wstring p_sParameterName, wstring p_sParameterValue);
	/*
	  This sends the command which sets a number of parameters in the Network Controller at the same time.
	  This is only valid when connected to a server.
	  \param p_vsParameterNames - A collection of the parameter names to change. The parameters will be changed in the defined order.
	  \param p_vsParameterValues - A collection of the new parameter values. Each entry corresponds to the equalivent entry in p_vsParameterNames and so the size of p_vsParameterValues must equal the size of p_vsParameterNames.
	  \return Whether the command executed successfully.
	*/
	bool setNetworkControllerParameterValues(vector<wstring> p_vsParameterNames, vector<wstring> p_vsParameterValues);
	/*
	  This sets the Network Controller parameter values based on the node information provided.
	  This is only valid when connected to a server.
	  \param p_sNodePath - The path of the node to get the informtion for.
	  \param p_sNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	bool setNetworkControllerNodeInformation(wstring p_sNodePath, wstring &p_sNodeInformation);
	/*
	  This sends the command which gets the value of the parameter in the Network Controller with the name specified.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to change.
	  \param p_sParameterValue - The value of the parameter is returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getNetworkControllerParameterValue(wstring p_sParameterName, wstring &p_sParameterValue);
	/*
	  This sends the command which gets the information about the Network Controller node specified.
	  This is only valid when connected to a server.
	  \param p_sNodePath - The name of the node to ge the informtion for.
	  \param p_sExcludedNodeTypes - A comma separated list of node types to be excluded from the information.
	  \param p_sNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	bool getNetworkControllerNodeInformation(wstring p_sNodeName, wstring p_sExcludedNodeTypes, wstring &p_sNodeInformation);


	/*
	  This sends the command specified to the slave server specified.
	  This is only valid when connected to the slave server.
	  \param p_sSlaveServerName - The name of the slave server to send the message to.
	  \param p_sMessageToBroadcast - The message to send to the slave server.
	  \param p_sReturn - If the command returns data, it will all be returned in this parameter. This parameter should be NULL if nothing is to be returned or if the return values are not to be recorded.
	  \return Whether the command executed successfully.
	*/
	bool broadcastMessageToSlaveServer(wstring p_sSlaveServerName, wstring p_sMessageToBroadcast, wstring &p_sReturn);
	/*
	  This sends the command specified to all of the slave servers.
	  This is only valid when connected to the slave servers.
	  \param p_sMessageToBroadcast - The message to send to the slave server.
	  \param p_sFailedSlaveServers - The names of the slave servers which failed to execute the broadcast message are returned in this parameter as a comma separated list. If no slave servers fail, the return data is contained in this parameter.
	  \param p_sReturn - If no slave servers fail, the return data is contained in this parameter, otherwise the names of the slave servers which failed to execute the broadcast message are returned in this parameter as a comma separated list. This parameter should be NULL if nothing is to be returned or if the return values are not to be recorded.
	  \return Whether the command executed successfully.
	*/
	bool broadcastMessageToAllSlaveServers(wstring p_sMessageToBroadcast, wstring &p_sReturn);


#if 0
	/*
	  This sends the command which aborts the Network Controller.
	  This is only valid when connected to a server.
	  \param p_bSendInNewClient - If this is true, a new client connection will be made to the Print Server and the abort will be sent using this. This is necessary if a command from this client is already being processed by the Print Server.
	  \return Whether the command executed successfully.
	*/
	bool abortNetworkController(bool p_bSendInNewClient=false);
#endif



	/////////////////////////////////////
	// Print Server Monitor
	/////////////////////////////////////

	/*
	  This sends the command which initialises the Print Server Monitor with the configuration file specified.
	  This is only valid when connected to a server.
	  \param p_sConfigurationFile - The configuration file the Render Engine will be initialised with.
	  \return Whether the command executed successfully.
	*/
	bool initialisePrintServerMonitor(wstring p_sConfigurationFile);


	/*
	  This sends the command which sets the parameter in the Print Server Monitor with the name specified to the
	  value specified.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to change.
	  \param p_sParameterValue - The new value of the parameter.
	  \return Whether the command executed successfully.
	*/
	bool setPrintServerMonitorParameterValue(wstring p_sParameterName, wstring p_sParameterValue);
	/*
	  This sends the command which sets a number of parameters in the Print Server Monitor at the same time.
	  This is only valid when connected to a server.
	  \param p_vsParameterNames - A collection of the parameter names to change. The parameters will be changed in the defined order.
	  \param p_vsParameterValues - A collection of the new parameter values. Each entry corresponds to the equalivent entry in p_vsParameterNames and so the size of p_vsParameterValues must equal the size of p_vsParameterNames.
	  \return Whether the command executed successfully.
	*/
	bool setPrintServerMonitorParameterValues(vector<wstring> p_vsParameterNames, vector<wstring> p_vsParameterValues);
	/*
	  This sets the Print Server Monitor parameter values based on the node information provided.
	  This is only valid when connected to a server.
	  \param p_sNodePath - The path of the node to get the informtion for.
	  \param p_sNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	bool setPrintServerMonitorNodeInformation(wstring p_sNodePath, wstring &p_sNodeInformation);
	/*
	  This sends the command which gets the value of the parameter in the Print Server Monitor with the name specified.
	  This is only valid when connected to a server.
	  \param p_sParameterName - The name of the parameter to change.
	  \param p_sParameterValue - The value of the parameter is returned in this parameter.
	  \return Whether the command executed successfully.
	*/
	bool getPrintServerMonitorParameterValue(wstring p_sParameterName, wstring &p_sParameterValue);
	/*
	  This sends the command which gets the information about the Print Server Monitor node specified.
	  This is only valid when connected to a server.
	  \param p_sNodePath - The name of the node to ge the informtion for.
	  \param p_sExcludedNodeTypes - A comma separated list of node types to be excluded from the information.
	  \param p_sNodeInformation - The information for the node.
	  \return Whether the command executed successfully.
	*/
	bool getPrintServerMonitorNodeInformation(wstring p_sNodeName, wstring p_sExcludedNodeTypes, wstring &p_sNodeInformation);


	/*
	  This sends the command which aborts the Print Server Monitor.
	  This is only valid when connected to a server.
	  \param p_bSendInNewClient - If this is true, a new client connection will be made to the Print Server Monitor and the abort will be sent using this. This is necessary if a command from this client is already being processed by the Print Server Monitor.
	  \return Whether the command executed successfully.
	*/
	bool abortPrintServerMonitor(bool p_bSendInNewClient=false);


	/*
	  This sends the command which launches a Print Server with the properties specified.
	  This is only valid when connected to a print server monitor.
	  \param p_sPrintServerName - The name of the server.
	  \param p_sConfigurationPath - The configuration the server will use.
	  \param p_iPort - The TCP/IP port the server will run on.
	  \param p_bStartMinimised - Whether the server should start as minimised or not.
	  \param p_bConnectedToPMB - Whether this server will communicate with a PMB or not.
	  \return Whether the command executed successfully.
	*/
	bool launchPrintServerMonitorPrintServer(wstring p_sPrintServerName, wstring p_sConfigurationPath, int p_iPort, bool p_bStartMinimised, bool p_bConnectedToPMB);
	/*
	  This sends the command which launches Print Servers with the properties specified.
	  This is only valid when connected to a print server monitor.
	  \param p_vsPrintServerNames - The names of the servers.
	  \param p_vsConfigurationPaths - The configurations the servers will use.
	  \param p_viPorts - The TCP/IP ports the servers will run on.
	  \param p_vbStartMinimised - Whether the servers should start as minimised or not.
	  \param p_vbConnectedToPMB - Whether this server will communicate with a PMB or not.
	  \return Whether the command executed successfully.
	*/
	bool launchPrintServerMonitorPrintServers(vector<wstring> p_vsPrintServerNames, vector<wstring> p_vsConfigurationPaths, vector<int> p_viPorts, vector<bool> p_vbStartMinimised, vector<bool> p_vbConnectedToPMB);
	/*
	  This sends the command which starts the Print Server with the name specified.
	  This is only valid when connected to a server.
	  \param p_sPrintServerName - The name of the Print Server to start.
	  \return Whether the command executed successfully.
	*/
	bool startPrintServerMonitorPrintServer(wstring p_sPrintServerName);
	/*
	  This sends the command which shuts down the Print Server with the name specified.
	  This is only valid when connected to a server.
	  \param p_sPrintServerName - The name of the Print Server to shutdown.
	  \return Whether the command executed successfully.
	*/
	bool shutdownPrintServerMonitorPrintServer(wstring p_sPrintServerName);
	/*
	  This sends the command which restarts the Print Server with the name specified.
	  This is only valid when connected to a server.
	  \param p_sPrintServerName - The name of the Print Server to restart.
	  \return Whether the command executed successfully.
	*/
	bool restartPrintServerMonitorPrintServer(wstring p_sPrintServerName);
};