///////////////////////////////////////////////////////////////////////////////////////////////
//
// Copyright (C) 2007-2011 Global Inkjet Systems Ltd.
//
// E-mail: support@globalinkjetsystems.com
//
// Web:	http://www.globalinkjetsystems.com/
//
// LogManagerInterface.h
//
// Version: 1.5
// 
///////////////////////////////////////////////////////////////////////////////////////////////


#pragma once


#define MAXIMUM_LOG_MESSAGE_SIZE	4096


/*
  This class provides an interface for a log manager. Any class which implements this interface will provide functionality for taking
  log messages and output them to an appropriate place, such as a text file or graphical interface.
 */
class ILogManagerInterface
{
public:
	/*
	  The level of the log.
	  LL_EXTENDED - The message is of very low importance and should only be logged to file if extended logging is on.
	  LL_LOW - The message is of low importance, and should only be logged to file.
	  LL_MEDIUM - The message is of medium importance, and should only be logged to file.
	  LL_HIGH - The message is of high importance, and should only be logged to file.
	  LL_WARNING - The message is a warning, and should only be logged to file.
	  LL_ERROR - The message is an error, and should only be logged to file.
	  LL_UI_INFO - The message is for information, and should be logged to file and an interface.
	  LL_UI_WARNING - The message is a warning, and should be logged to file and an interface.
	  LL_UI_ERROR - The message is an error, and should be logged to file and an interface.
	 */
	enum ELOG_LEVEL {LL_EXTENDED=0, LL_LOW=1, LL_MEDIUM=2, LL_HIGH=3, LL_WARNING=4, LL_ERROR=5, LL_UI_INFO=7, LL_UI_WARNING=8, LL_UI_ERROR=9};


protected:
	void *m_pvInterfaceLogCallbackObject;																						// The object the interface log callback should be called on.
	bool (* m_puInterfaceLogCallback)(void *p_pvInterfaceLogCallbackObject, ELOG_LEVEL p_eLogLevel, const WCHAR *p_pcMessage);	// The pointer to the interface log callback function.


public:
	/*
	  The constructor for the ILogManagerInterface class.
	 */
	ILogManagerInterface(void)
	{
		m_pvInterfaceLogCallbackObject = NULL;
		m_puInterfaceLogCallback = NULL;
	}
	/*
	  The destructor for the ILogManagerInterface class.
	 */
	virtual ~ILogManagerInterface(void)
	{
	}


	/*
	  This initialises the log manager.
	  This must be called only one, before any other functions are called or messages are logged.
	  \return Whether the initialisation was successful or not.
	 */
	virtual bool initialiseLogManager(void) = 0;
	/*
	  This initialises the log manager.
	  This must be called only one, before any other functions are called or messages are logged.
	  \param p_bLogToFileEnabled - Whether the log manager will log to file or not.
	  \param p_pcLogFile - The file to log to.
	  \param p_bLogToInterfaceEnabled - Whether the log manager will log to the interface or not.
	  \return Whether the initialisation was successful or not.
	 */
	virtual bool initialiseLogManager(bool p_bLogToFileEnabled, const WCHAR *p_pcLogFile, bool p_bLogToInterfaceEnabled) = 0;

	/*
	  This aborts the log manager.
	  This must be called only one, and no other functions must be called or messages logged after it is called.
	  \return Whether the abort was successful or not.
	 */
	virtual bool abortLogManager(void) = 0;


	/*
	  This logs the message provided.
	  \param p_eLogLevel - The level of this log message. This determines the formatting that will be applied to the message when it is logged.
	  \param p_pcMessage - The message to log, which has control characters where the remaining parameters are to be inserted.
	  \param ... - A variable (zero or more) number of parameters of any type, which are inserted into p_sMessage based on locations of control characters to form the final message to log. The usage matches the standard sprint() function.
	  \return Whether the lessage was logged successfully or not.
	 */
	virtual bool log(ELOG_LEVEL p_eLogLevel, const WCHAR *p_pcMessage, ...) const = 0;


	/*
	  This sets whether the log is enabled or not.
	  \param p_bLogToFileEnabled - Whether the log manager will log to file or not.
	  \param p_bLogToInterfaceEnabled - Whether the log manager will log to the interface or not.
	 \return Whether the set was successful or not.
	 */
	virtual bool setLogEnabled(bool p_bLogToFileEnabled, bool p_bLogToInterfaceEnabled) = 0;
	/*
	  This sets the path to the file to log to.
	  \param p_pcLogFilPathe - The path to the file to log to.
	  \return Whether the set was successful or not.
	 */
	virtual bool setLogFilePath(const WCHAR *p_pcLogFilePath) = 0;


	/*
	  This sets the interface log callback function and object.
	  \param m_pvInterfaceLogCallbackObject - The object the interface log callback should be called on.
	  \param m_puInterfaceLogCallback - The pointer to the interface log callback function.
	  \return Whether the interface log callback function and object were set successfully or not.
	 */
	bool setInterfaceLogCallback(void *p_pvInterfaceLogCallbackObject, bool (* p_puInterfaceLogCallback)(void *p_pvInterfaceLogCallbackObject, ELOG_LEVEL p_eLogLevel, const WCHAR *p_pcMessage))
	{
		m_pvInterfaceLogCallbackObject = p_pvInterfaceLogCallbackObject;
		m_puInterfaceLogCallback = p_puInterfaceLogCallback;

		return true;
	}
};