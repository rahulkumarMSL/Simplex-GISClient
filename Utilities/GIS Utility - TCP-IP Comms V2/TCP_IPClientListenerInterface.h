///////////////////////////////////////////////////////////////////////////////////////////////
//
// Copyright (C) 2007-2011 Global Inkjet Systems Ltd.
//
// E-mail: support@globalinkjetsystems.com
//
// Web:	http://www.globalinkjetsystems.com/
//
// TCP_IPClientListenerInterface.h
//
// Version: 1.6
// 
///////////////////////////////////////////////////////////////////////////////////////////////


#pragma once


/*
  The ITCP_IPClientListenerInterface class is the interface for a TCP/IP client listener. Any class which implements this interface
  will provide the functionality that should be called when various events in the TCP/IP client occur.

  The functionality is implementd as callback functions, which are called when a specific event in the TCP/IP connection occurs.
  Each of these callback functions should perform simple actions such as updating interfaces or signaling for other actions to happen.
  They do not need to perform any actions with the TCP/IP connection as these will be performed automatically if necessary and the
  must not block.

  For more information about the TCP/IP communication, see the "Print Server Communication User Guide".
 */
class ITCP_IPClientListenerInterface
{
public:
	/*
	  The constructor for the CTCP_IPClientListener class.
	 */
	ITCP_IPClientListenerInterface(void)
	{
	}
	/*
	  The destructor for the CTCP_IPClientListener class.
	 */
	virtual ~ITCP_IPClientListenerInterface(void)
	{
	}


	/*
	  This is called when the TCP/IP client successfully connects to the server.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool connectedCallback(void) = 0;
	/*
	  This is called when the TCP/IP client successfully disconnects from the server, or when the
	  client detects that the server has disconnected.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool disconnectedCallback(void) = 0;
	/*
	  This is called just before the TCP/IP client sends a message to the server.
	  \param p_pcMessage - The message being sent.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool messageSentCallback(const WCHAR *p_pcMessage) = 0;
	/*
	  This is called as soon as the TCP/IP client receives a message from the server.
	  \param p_pcMessage - The message which was received.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool messageReceivedCallback(const WCHAR *p_pcMessage) = 0;
	/*
	  This is called when the TCP/IP client receives a status update from the server.
	  \param p_pcEngineCode - The code indicating what engine the status relates to. S = Print Server, R = Render Engine, P = Print Controller, K = Network Controller.
	  \param p_pcStatus - The text representation of the status.
	  \param p_iStatusCode - The code of the status.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool statusCallback(const WCHAR *p_pcEngineCode, const WCHAR *p_pcStatus, int p_iStatusCode) = 0;
	/*
	  This is called when the TCP/IP client receives a traffic light update from the server.
	  \param p_iState - The traffic light state.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool trafficLightCallback(int p_iState) = 0;
	/*
	  This is called when the TCP/IP client receives a swathe processed update from the server.
	  \param p_iSwatheProcessed - The swathe number processed.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool swatheProcessedCallback(int p_iSwatheProcessed) = 0;
	/*
	  This is called when the TCP/IP client receives a label number update from the server.
	  \param p_iLabelNumber - The current label number.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool labelNumberCallback(int p_iLabelNumber) = 0;
	/*
	  This is called when the TCP/IP client receives a position update from the server.
	  \param p_dXPosition - The x position received from the server.
	  \param p_dYPosition - The y position received from the server.
	  \param p_dZPosition - The z position received from the server.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool positionCallback(double p_dXPosition, double p_dYPosition, double p_dZPosition) = 0;
	/*
	  This is called when the TCP/IP client receives an ink system parameter value update from the server.
	  \param p_pcParameterName - The name of the parameter.
	  \param p_pcParameterValue - The value associated with the parameter.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool inkSystemParameterValueCallback(const WCHAR *p_pcParameterName, const WCHAR *p_pcParameterValue) = 0;
	/*
	  This is called when the TCP/IP client receives a printhead status update from the server.
	  \param p_pcPrintheadStatusInformation - The compressed printhead status information.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool printheadStatusCallback(const WCHAR *p_pcPrintheadStatusInformation) = 0;
	/*
	  This is called when the TCP/IP client receives a print started update from the server.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool printStartedCallback(void) = 0;
	/*
	  This is called when the TCP/IP client receives a ready to print update from the server.
	  \param p_iNumberOfSwathes - The number of swathes to print.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool readyToPrintCallback(int p_iNumberOfSwathes) = 0;
	/*
	  This is called when the TCP/IP client receives an end of printing update from the server.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool endOfPrintingCallback(void) = 0;
	/*
	  This is called when the print server is reporting something.
	  \param p_pcMessage - The message being reported.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool printServerReporterCallback(const WCHAR *p_pcMessage) = 0;
	/*
	  This is called when the client has a message to log to the interface.
	  \param p_iLevel - The log level.
						ILogManagerInterface::LL_UI_INFO = Information.
						ILogManagerInterface::LL_UI_WARNING = Warning.
						ILogManagerInterface::LL_UI_ERROR = Error.
	  \param p_pcMessage - The message to log.
	  \return Whether the callback action completed successfully or not.
	 */
	virtual bool interfaceLogCallback(int p_iLevel, const WCHAR *p_pcMessage) = 0;
};