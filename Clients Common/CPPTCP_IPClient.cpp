///////////////////////////////////////////////////////////////////////////////////////////////
//
// Copyright (C) 2007-2011 Global Inkjet Systems Ltd.
//
// E-mail: support@globalinkjetsystems.com
//
// Web:	http://www.globalinkjetsystems.com/
//
// CPPTCP_IPClient.cpp
//
// Version: 1.5
// 
///////////////////////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "CPPTCP_IPClient.h"


CCPPTCP_IPClient::CCPPTCP_IPClient(void)
{
	m_xCPPAPIDll = NULL;
	m_pxTCP_IPClient = NULL;

	m_pxLogManager = NULL;
	m_bCreatedLogManager = false;

	m_pxTCP_IPClientListener = NULL;
}
CCPPTCP_IPClient::~CCPPTCP_IPClient(void)
{
	if (m_bCreatedLogManager)
		delete m_pxLogManager;
	m_pxLogManager = NULL;
}


bool CCPPTCP_IPClient::initialise(ISecurityManagerInterface *p_pxSecurityManager, ILogManagerInterface *p_pxLogManager)
{
	if(p_pxLogManager)
	{
		m_pxLogManager = p_pxLogManager;
	}

	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::initialise()"));

	// Load the TCP_IPComms DLL, which must be performed before any functions can be called on it.
	if(!loadTCP_IPCommsDLL())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialise() : Failed to load the TCP_IP comms DLL."));

		return false;
	}

	// If no log manager was passed in, create one.
	if(m_pxLogManager)
	{
		m_bCreatedLogManager = false;
	}
	else
	{
		if(!m_pxTCP_IPClient->createLogManager(&m_pxLogManager))
		{
			logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialise() : Failed to create the log manager."));

			return false;
		}
		if(!m_pxLogManager)
		{
			logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialise() : The created log manager is NULL."));

			return false;
		}
		if(!m_pxLogManager->initialiseLogManager())
		{
			logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialise() : Failed to initialise the log manager."));

			return false;
		}
		if(!m_pxLogManager->setInterfaceLogCallback((void *)this, &wrapperToInterfaceLogCallback))
		{
			logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialise() : Failed to set the interface log callback in the log manager."));

			return false;
		}

		m_bCreatedLogManager = true;

		logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::initialise() : Initialised after creating the Log Manager."));
	}

	// Initialise the TCP_IPClient so it is ready to have functions called on it.
	if(!m_pxTCP_IPClient->initialise(p_pxSecurityManager, m_pxLogManager, false))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialise() : Failed to initialise the TCP_IP client."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::initialise(bool p_bLogToFileEnabled, wstring p_sLogFile, bool p_bLogToInterfaceEnabled)
{
	// Load the TCP_IPComms DLL, which must be performed before any functions can be called on it.
	if(!loadTCP_IPCommsDLL())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialise() : Failed to load the TCP_IP comms DLL."));

		return false;
	}

	// Create the log manager.
	if(!m_pxTCP_IPClient->createLogManager(&m_pxLogManager))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialise() : Failed to create the log manager."));

		return false;
	}
	if(!m_pxLogManager)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialise() : The created log manager is NULL."));

		return false;
	}
	if(!m_pxLogManager->initialiseLogManager(p_bLogToFileEnabled, p_sLogFile.c_str(), p_bLogToInterfaceEnabled))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialise() : Failed to initialise the log manager."));

		return false;
	}
	m_bCreatedLogManager = true;

	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::initialise() : Initialised after creating the Log Manager."));

	// Initialise the TCP_IPClient so it is ready to have functions called on it.
	if(!m_pxTCP_IPClient->initialise(NULL, m_pxLogManager, false))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialise() : Failed to initialise the TCP_IP client."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::abort(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::abort()"));

	bool bReturn = true;

	// Abort the TCP_IPClient so it is no longer doing anything and is therefore ready to be deleted.
	// m_pxTCP_IPClient will not have been created if the DLl could not be loaded.
	if(m_pxTCP_IPClient)
	{
		if(!m_pxTCP_IPClient->abort())
		{
			logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abort() : Failed to abort the TCP_IP client."));

			bReturn = false;
		}
	}

	// Abort and delete the log manager if this class created it.
	if(m_bCreatedLogManager)
	{
		if(!m_pxLogManager->abortLogManager())
		{
			bReturn = false;
		}

		if(!m_pxTCP_IPClient->deleteLogManager(m_pxLogManager))
		{
			logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abort() : Failed to delete the Log Manager."));

			bReturn = false;
		}
		m_pxLogManager = NULL;
		m_bCreatedLogManager = false;
	}

	// Free the TCP_IPComms DLL to make sure all allocated memory and resources are cleaned up.
	if(!freeTCP_IPCommsDLL())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abort() : Failed to free the TCP_IP comms DLL."));

		bReturn = false;
	}
	
	return true;
}
bool CCPPTCP_IPClient::loadTCP_IPCommsDLL(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::loadTCP_IPCommsDLL()"));

	// If thr TCP_IPComms DLL has already been loaded, free it so that no memory or resources leak when the DLL is reloaded.
	if(!freeTCP_IPCommsDLL())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::loadTCP_IPCommsDLL() : Failed to free the TCP_IP comms DLL."));

		return false;
	}

	// Load the library from the DLL file.
	SetLastError(0);
	m_xCPPAPIDll = LoadLibraryEx(_T("GIS Utility - TCP-IP Comms.dll"),NULL,NULL);
	int iErrorCode = (m_xCPPAPIDll == NULL) ? GetLastError() : 0;
	if(!m_xCPPAPIDll || (iErrorCode!=0 && iErrorCode!=183)) // Ignore error code 183, as this means the DLL has already been loaded which may be the case if a client contains more than one CPPTCP_IPClient object.
	{
		wstringstream xLogMessageStringStream;
		xLogMessageStringStream << _T("CCPPTCP_IPClient::loadTCP_IPCommsDLL() : Failed to load the TCP_IP comms DLL. Error code = ") << iErrorCode << _T(".");
		logMessage(ILogManagerInterface::LL_LOW, xLogMessageStringStream.str());

		CString Error = _T("");
		Error.Format(_T("Failed to load GIS Utility - TCP-IP Comms.dll %d"),iErrorCode);
		if (iErrorCode == 126)
		{
			Error.Append(_T(". You may have mismatched versions of GIS DLLs."));
		}

		AfxMessageBox(Error);

		return false;
	}

	// Create the TCP_IPClient object by calling the appropriate function in the DLL.
	getTCP_IPClientInterface_func func = (getTCP_IPClientInterface_func)GetProcAddress(m_xCPPAPIDll, "getTCP_IPClient");
	m_pxTCP_IPClient = func();
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::loadTCP_IPCommsDLL() : Failed to create the TCP_IP client."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::freeTCP_IPCommsDLL(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::freeTCP_IPCommsDLL()"));

	bool bReturn = true;

	// Delete the TCP_IPClient object that was created when the DLL function was called in loadTCP_IPCommsDLL().
	if(m_pxTCP_IPClient)
	{
		delete m_pxTCP_IPClient;
		m_pxTCP_IPClient = NULL;
	}

	// Free the DLL library that was loaded in loadTCP_IPCommsDLL().
	if(m_xCPPAPIDll)
	{
		if(!FreeLibrary(m_xCPPAPIDll))
		{
			logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::freeTCP_IPCommsDLL() : Failed to free the DLL library."));

			bReturn = false;
		}
	}

	return bReturn;
}


bool CCPPTCP_IPClient::connectToServer(wstring p_sIPAddress, int p_iPort, int p_iConnectionTimeout)
{
	wstringstream xLogMessageStringStream;
	xLogMessageStringStream << _T("CCPPTCP_IPClient::connectToServer() : Connecting to server at address \"") << p_sIPAddress.c_str() << _T("\" on port ") << p_iPort << _T(" with timeout ") << p_iConnectionTimeout << _T(".");
	logMessage(ILogManagerInterface::LL_LOW, xLogMessageStringStream.str());

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::connectToServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the connect to server function in the DLL.
	if(!m_pxTCP_IPClient->connectToServer(p_sIPAddress.c_str(), p_iPort, p_iConnectionTimeout))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::connectToServer() : Failed to connect to the server."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::disconnectFromServer(bool p_bFreeTCP /*= false*/)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::disconnectFromServer()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::disconnectFromServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the disconnect from server function in the DLL.
	if(!m_pxTCP_IPClient->disconnectFromServer(true))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::disconnectFromServer() : Failed to disconnect from the server."));

		return false;
	}

	if (p_bFreeTCP)
	{
		if (!freeTCP_IPCommsDLL())
		{
			logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::disconnectFromServer() : Failed to free TCP on Disconnect."));

			return false;
		}
	}
	return true;
}


bool CCPPTCP_IPClient::waitUntilConnectedToServer(void) const
{
	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::waitUntilConnectedToServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	return m_pxTCP_IPClient->waitUntilConnectedToServer();
}
bool CCPPTCP_IPClient::isConnectedToServer(void) const
{
	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::isConnectedToServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	return m_pxTCP_IPClient->isConnectedToServer();
}


bool CCPPTCP_IPClient::sendMessage(wstring p_sMessage, bool p_bWaitForComplete)
{
	wstringstream xLogMessageStringStream;
	xLogMessageStringStream << _T("CCPPTCP_IPClient::sendMessage() : Sending message \"") << p_sMessage.c_str() << _T("\" to the server. Waiting for complete = ") << p_bWaitForComplete << _T(".");
	logMessage(ILogManagerInterface::LL_LOW, xLogMessageStringStream.str());

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::sendMessage() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the send message function in the DLL.
	if(!m_pxTCP_IPClient->sendMessage(p_sMessage.c_str(), NULL, 0, p_bWaitForComplete, false))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::sendMessage() : Failed to send the message to the server."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::sendMessage(wstring p_sMessage, wstring &p_sReturn)
{
	wstringstream xLogMessageStringStream;
	xLogMessageStringStream << _T("CCPPTCP_IPClient::sendMessage() : Sending message \"") << p_sMessage.c_str() << _T("\" to the server.");
	logMessage(ILogManagerInterface::LL_LOW, xLogMessageStringStream.str());

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::sendMessage() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the send message function in the DLL.
	WCHAR pcReturn[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->sendMessage(p_sMessage.c_str(), pcReturn, STANDARD_STRING_SIZE, true, false))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::sendMessage() : Failed to send the message to the server."));

		return false;
	}
	p_sReturn = wstring(pcReturn);

	return true;
}
bool CCPPTCP_IPClient::sendMessageInNewClient(wstring p_sMessage)
{
	wstringstream xLogMessageStringStream;
	xLogMessageStringStream << _T("CCPPTCP_IPClient::sendMessageInNewClient() : Sending message \"") << p_sMessage.c_str() << _T("\" to the server in a new client.");
	logMessage(ILogManagerInterface::LL_LOW, xLogMessageStringStream.str());

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::sendMessageInNewClient() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the send message function in the DLL.
	if(!m_pxTCP_IPClient->sendMessage(p_sMessage.c_str(), NULL, 0, true, false))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::sendMessageInNewClient() : Failed to send the message to the server."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::sendMessageInNewClient(wstring p_sMessage, wstring &p_sReturn)
{
	wstringstream xLogMessageStringStream;
	xLogMessageStringStream << _T("CCPPTCP_IPClient::sendMessageInNewClient() : Sending message \"") << p_sMessage.c_str() << _T("\" to the server in a new client.");
	logMessage(ILogManagerInterface::LL_LOW, xLogMessageStringStream.str());

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::sendMessageInNewClient() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the send message function in the DLL.
	WCHAR pcReturn[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->sendMessage(p_sMessage.c_str(), pcReturn, STANDARD_STRING_SIZE, true, false))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::sendMessageInNewClient() : Failed to send the message to the server."));

		return false;
	}
	p_sReturn = wstring(pcReturn);

	return true;
}
bool CCPPTCP_IPClient::readFromServer(wstring &p_sResponse)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::readFromServer()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::readFromServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the read from server function in the DLL.
	WCHAR pcResponse[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->readFromServer(pcResponse, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::readFromServer() : Failed to read from the server."));

		return false;
	}
	p_sResponse = wstring(pcResponse);

	return true;
}


bool CCPPTCP_IPClient::parsePrintheadInformation(wstring p_sPrintheadStatusInformation, map<wstring, IPrintheadInformationInterface *> &p_mspxPrintheadInformation) const
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::parsePrintheadInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::parsePrintheadInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the parse printhead information function in the DLL.
	WCHAR **ppcPrintheadNames = NULL;
	IPrintheadInformationInterface **ppxPrintheadInformation = NULL;
	int iNumberOfPrintheads = 0;
	if(!m_pxTCP_IPClient->parsePrintheadInformation(p_sPrintheadStatusInformation.c_str(), &ppcPrintheadNames, &ppxPrintheadInformation, iNumberOfPrintheads))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::parsePrintheadInformation() : Failed to parse the printhead information."));

		return false;
	}
	if(iNumberOfPrintheads==0 || !ppcPrintheadNames || !*ppxPrintheadInformation)
	{
		return false;
	}

	for(int iPrintheadIndex=0; iPrintheadIndex<iNumberOfPrintheads; iPrintheadIndex++)
	{
		wstring sPrintheadName(ppcPrintheadNames[iPrintheadIndex]);
		IPrintheadInformationInterface *pxPrintheadInformation = ppxPrintheadInformation[iPrintheadIndex];

		p_mspxPrintheadInformation[sPrintheadName] = pxPrintheadInformation;
	}

	// Clean up the printhead information.
	for(int iPrintheadIndex=0; iPrintheadIndex<iNumberOfPrintheads; iPrintheadIndex++)
	{
		if(ppcPrintheadNames[iPrintheadIndex])
		{
			delete [] ppcPrintheadNames[iPrintheadIndex];
			ppcPrintheadNames[iPrintheadIndex] = NULL;
		}

		// Do not delete the IPrintheadInformationInterface objects.
	}
	delete [] ppcPrintheadNames;
	ppcPrintheadNames = NULL;
	delete [] ppxPrintheadInformation;
	ppxPrintheadInformation = NULL;

	return true;
}

bool CCPPTCP_IPClient::example(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::example()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::example() : The TCP_IP client is NULL."));

		return false;
	}

	// Create a message that returns the values of the parameter which have a unique ID of "UniqueID1" and "UniqueID2"
    wstring sMessage1 = _T("P,V,P,UniqueID1,UniqueID2");
    // Create a message that returns the values of all parameter which have a unique ID of "UniqueID3" and "UniqueID4"
    wstring sMessage2 = _T("P,V,P,UniqueID3,UniqueID4");

    // Create the variables taht will be needed.
    int iMessage1ID = 0;
    wstring sMessage1Return = _T("");
    bool bMessage1CommandSuccess = true;
    int iMessage2ID = 0;
    wstring sMessage2Return = _T("");
    bool bMessage2CommandSuccess = true;

    // Send message 1 to the server.
    if(!m_pxTCP_IPClient->sendMessage(sMessage1.c_str(), false, &iMessage1ID, NULL, NULL, 0, -1))
	{
        logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::example() : Failed to send message 1 to the server."));

		return false;
	}
    // Send message 2 to the server.
    if(!m_pxTCP_IPClient->sendMessage(sMessage2.c_str(), false, &iMessage2ID, NULL, NULL, 0, -1))
	{
        logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::example() : Failed to send message 2 to the server."));

		return false;
	}

    // Wait for message 1 to complete.
	WCHAR pcMessage1Return[STANDARD_STRING_SIZE];
    if(!m_pxTCP_IPClient->waitForComplete(iMessage1ID, pcMessage1Return, STANDARD_STRING_SIZE, &bMessage1CommandSuccess, -1))
	{
        logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::example() : Failed to wait for message 1 to complete."));

		return false;
	}
    if(!bMessage1CommandSuccess)
	{
        logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::example() : Message 1 returned a failure."));

		return false;
	}
    // Wait for message 2 to complete.
	WCHAR pcMessage2Return[STANDARD_STRING_SIZE];
    if(!m_pxTCP_IPClient->waitForComplete(iMessage2ID, pcMessage2Return, STANDARD_STRING_SIZE, &bMessage2CommandSuccess, -1))
	{
        logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::example() : Failed to wait for message 2 to complete."));

		return false;
	}
    if(!bMessage2CommandSuccess)
	{
        logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::example() : Message 2 returned a failure."));

		return false;
	}

	sMessage1Return = wstring(pcMessage1Return);
	sMessage2Return = wstring(pcMessage2Return);

    // Extract all responses from the response to message 1.
	WCHAR **ppcResponses1 = NULL;
	int iNumberOfResponses1 = 0;
	if(!m_pxTCP_IPClient->splitResponses(sMessage1Return.c_str(), &ppcResponses1, iNumberOfResponses1))
	{
        logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::example() : Failed to split the responses from message 1."));

		return false;
	}
	if(iNumberOfResponses1==0 || !ppcResponses1)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::example() : The responses from message 1 are empty."));

		return false;
	}
	for(int iMessage1ResponseIndex=0; iMessage1ResponseIndex<iNumberOfResponses1; iMessage1ResponseIndex++)
	{
        wstring sResponse1(ppcResponses1[iMessage1ResponseIndex]);
        if(iMessage1ResponseIndex == 0)
		{
			wstring sMessage = _T("UniqueID1: ") + sResponse1;
			AfxMessageBox(sMessage.c_str());
		}
		else if(iMessage1ResponseIndex == 1)
		{
			wstring sMessage = _T("UniqueID2: ") + sResponse1;
            AfxMessageBox(sMessage.c_str());
		}
	}
	// Clean up the message 1 responses.
	for(int iMessage1ResponseIndex=0; iMessage1ResponseIndex<iNumberOfResponses1; iMessage1ResponseIndex++)
	{
		delete [] ppcResponses1[iMessage1ResponseIndex];
		ppcResponses1[iMessage1ResponseIndex] = NULL;
	}
	delete [] ppcResponses1;
	ppcResponses1 = NULL;

    // Extract all responses from the response to message 2.
    WCHAR **ppcResponses2 = NULL;
	int iNumberOfResponses2 = 0;
    if(!m_pxTCP_IPClient->splitResponses(sMessage2Return.c_str(), &ppcResponses2, iNumberOfResponses2))
	{
        logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::example() : Failed to split the responses from message 2."));

		return false;
	}
	if(iNumberOfResponses2==0 || !ppcResponses2)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::example() : The responses from message 1 are empty."));

		return false;
	}
    for(int iMessage2ResponseIndex=0; iMessage2ResponseIndex<iNumberOfResponses2; iMessage2ResponseIndex++)
	{
        wstring sResponse2(ppcResponses2[iMessage2ResponseIndex]);
        if(iMessage2ResponseIndex == 0)
		{
			wstring sMessage = _T("UniqueID3: ") + sResponse2;
            AfxMessageBox(sMessage.c_str());
		}
		else if(iMessage2ResponseIndex == 1)
		{
			wstring sMessage = _T("UniqueID4: ") + sResponse2;
            AfxMessageBox(sMessage.c_str());
		}
	}
	// Clean up the message 2 responses.
	for(int iMessage2ResponseIndex=0; iMessage2ResponseIndex<iNumberOfResponses2; iMessage2ResponseIndex++)
	{
		delete [] ppcResponses2[iMessage2ResponseIndex];
		ppcResponses2[iMessage2ResponseIndex] = NULL;
	}
	delete [] ppcResponses2;
	ppcResponses2 = NULL;

	return true;
}


bool CCPPTCP_IPClient::setTCP_IPClientListener(ITCP_IPClientListenerInterface *p_pxTCP_IPClientListenerInterface)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setTCP_IPClientListener()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setTCP_IPClientListener() : The TCP_IP client is NULL."));

		return false;
	}

	m_pxTCP_IPClientListener = p_pxTCP_IPClientListenerInterface;

	// Set the TCP_IPClientListener object in the DLL.
	if(!m_pxTCP_IPClient->setTCP_IPClientListener(m_pxTCP_IPClientListener))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setTCP_IPClientListener() : Failed to set the TCP_IP client listener in the TCP_IP client."));

		return false;
	}
	// Set the interface log callback in the log manager.
	if(m_pxLogManager)
	{
		if(!m_pxLogManager->setInterfaceLogCallback((void *)this, &wrapperToInterfaceLogCallback))
		{
			logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setTCP_IPClientListener() : Failed to set the interface log callback in the log manager."));

			return false;
		}
	}

	return true;
}


ILogManagerInterface * CCPPTCP_IPClient::getLogManager(void) const
{
	return m_pxLogManager;
}


bool CCPPTCP_IPClient::logMessage(ILogManagerInterface::ELOG_LEVEL p_eLogLevel, wstring p_sMessage) const
{
	if(!m_pxLogManager)
	{
		return false;
	}

	// Log the message - first replacing any % characters with %%
	wstring sMsg(_T(""));
	if(p_sMessage.find(_T('%')) != std::string::npos)
	{
		size_t iLen = p_sMessage.length();
		for(size_t i = 0; i < iLen; i++)
		{
			TCHAR tc = p_sMessage[i];
			if(tc == _T('%'))
			{
				sMsg.append(_T("%%"));	
			}
			else
			{
				sMsg.append(1, tc);
			}
		}
		if(!m_pxLogManager->log(p_eLogLevel, sMsg.c_str()))
		{
			return false;
		}
	}
	else
	{
		if(!m_pxLogManager->log(p_eLogLevel, p_sMessage.c_str()))
		{
			return false;
		}
	}

	return true;
}
bool CCPPTCP_IPClient::setLogFilePath(wstring p_sLogFilePath)
{
	if(!m_pxLogManager)
	{
		return false;
	}

	// Set the log file path.
	if(!m_pxLogManager->setLogFilePath(p_sLogFilePath.c_str()))
	{
		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::setLogEnabled(bool p_bLogEnabled, bool p_bLogToInterfaceEnabled)
{
	if(!m_pxLogManager)
	{
		return false;
	}

	// Set the log enabled.
	if(!m_pxLogManager->setLogEnabled(p_bLogEnabled, p_bLogToInterfaceEnabled))
	{
		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::updateLogSettings(bool p_bLogToFileEnabled, wstring p_sLogFilePath, bool p_bLogToInterfaceEnabled, bool p_bExtendedNetworkLogging)
{
	if(!m_pxTCP_IPClient)
	{
		return false;
	}

	// Set the log enabled.
	if(!m_pxTCP_IPClient->updateLogSettings(p_bLogToFileEnabled, p_sLogFilePath.c_str(), p_bLogToInterfaceEnabled, p_bExtendedNetworkLogging))
	{
		return false;
	}

	return true;
}
int CCPPTCP_IPClient::getLowLogLevel(void) const
{
	if(!m_pxTCP_IPClient)
	{
		return 0;
	}

	return m_pxTCP_IPClient->getLowLogLevel();
}
int CCPPTCP_IPClient::getWarningLogLevel(void) const
{
	if(!m_pxTCP_IPClient)
	{
		return 0;
	}

	return m_pxTCP_IPClient->getWarningLogLevel();
}
int CCPPTCP_IPClient::getErrorLogLevel(void) const
{
	if(!m_pxTCP_IPClient)
	{
		return 0;
	}

	return m_pxTCP_IPClient->getErrorLogLevel();
}
int CCPPTCP_IPClient::getUIInfoLogLevel(void) const
{
	if(!m_pxTCP_IPClient)
	{
		return 0;
	}

	return m_pxTCP_IPClient->getUIInfoLogLevel();
}
int CCPPTCP_IPClient::getUIWarningLogLevel(void) const
{
	if(!m_pxTCP_IPClient)
	{
		return 0;
	}

	return m_pxTCP_IPClient->getUIWarningLogLevel();
}
int CCPPTCP_IPClient::getUIErrorLogLevel(void) const
{
	if(!m_pxTCP_IPClient)
	{
		return 0;
	}

	return m_pxTCP_IPClient->getUIErrorLogLevel();
}
bool CCPPTCP_IPClient::wrapperToInterfaceLogCallback(void *p_pvCPPTCP_IPClientObject, ILogManagerInterface::ELOG_LEVEL p_eLogLevel, const WCHAR *p_pcMessage)
{
	if(!p_pvCPPTCP_IPClientObject)
	{
		return false;
	}

	// Cast the void p_pvCPPTCP_IPClientObject to a CCPPTCP_IPClient object.
	CCPPTCP_IPClient *pxCPPTCP_IPClientObject = (CCPPTCP_IPClient *)p_pvCPPTCP_IPClientObject;
	if(!pxCPPTCP_IPClientObject)
	{
		return false;
	}

	// Call the interface log callback.
	if(!pxCPPTCP_IPClientObject->interfaceLogCallback(p_eLogLevel, p_pcMessage))
	{
		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::interfaceLogCallback(ILogManagerInterface::ELOG_LEVEL p_eLogLevel, const WCHAR *p_pcMessage) const
{
	if(!m_pxTCP_IPClientListener)
	{
		return false;
	}

	// Call the interface log callback on the TCP/IP client listener.
	if(!m_pxTCP_IPClientListener->interfaceLogCallback(p_eLogLevel, p_pcMessage))
	{
		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::initialisePrintServer(wstring p_sConfigurationFile)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::initialisePrintServer()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialisePrintServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->initialisePrintServer(p_sConfigurationFile.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialisePrintServer() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::waitUntilPrintServerInitialised(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::waitUntilPrintServerInitialised()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::waitUntilPrintServerInitialised() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->waitUntilPrintServerInitialised())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::waitUntilPrintServerInitialised() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::registerForPrintServerInformation(bool p_bReturnExistingInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::registerForPrintServerInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::registerForPrintServerInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->registerForPrintServerInformation(p_bReturnExistingInformation))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::registerForPrintServerInformation() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::saveConfigurationAs(wstring p_sConfigurationFolder)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::saveConfigurationAs()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::saveConfigurationAs() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->saveConfigurationAs(p_sConfigurationFolder.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::saveConfigurationAs() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::saveConfiguration(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::saveConfiguration()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::saveConfiguration() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->saveConfiguration())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::saveConfiguration() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::applySystemMode(wstring p_sSystemModeName)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::applySystemMode()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::applySystemMode() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->applySystemMode(p_sSystemModeName.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::applySystemMode() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::getSystemModeNames(wstring &p_sSystemModeNames)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getSystemModeNames()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getSystemModeNames() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcSystemModeNames[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->getSystemModeNames(pcSystemModeNames, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getSystemModeNames() : Failed to send the command."));

		return false;
	}
	p_sSystemModeNames = wstring(pcSystemModeNames);

	return true;
}
bool CCPPTCP_IPClient::getSystemModeInformation(wstring p_sSystemModeName, wstring &p_sRenderModeName, wstring &p_sPrintModeName)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getSystemModeInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getSystemModeInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcRenderModeName[STANDARD_STRING_SIZE];
	WCHAR pcPrintModeName[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->getSystemModeInformation(p_sSystemModeName.c_str(), pcRenderModeName, STANDARD_STRING_SIZE, pcPrintModeName, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getSystemModeInformation() : Failed to send the command."));

		return false;
	}
	p_sRenderModeName = wstring(pcRenderModeName);
	p_sPrintModeName = wstring(pcPrintModeName);

	return true;
}


bool CCPPTCP_IPClient::setPrintServerParameterValue(wstring p_sParameterName, wstring p_sParameterValue)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setPrintServerParameterValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerParameterValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setPrintServerParameterValue(p_sParameterName.c_str(), p_sParameterValue.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerParameterValue() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::setPrintServerParameterValues(vector<wstring> p_vsParameterNames, vector<wstring> p_vsParameterValues)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setPrintServerParameterValues()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerParameterValues() : The TCP_IP client is NULL."));

		return false;
	}

	if(p_vsParameterNames.size() != p_vsParameterValues.size())
	{
		CString sLogMessage = _T("");
		sLogMessage.Format(_T("CCPPTCP_IPClient::setPrintServerParameterValues() : The number of parameter names (%d) does no equal the number of parameter values (%d)."), p_vsParameterNames.size(), p_vsParameterValues.size());
		logMessage(ILogManagerInterface::LL_ERROR, sLogMessage.GetBuffer());

		return false;
	}
	int iNumberOfParametersToChange = (int)p_vsParameterNames.size();
	if(iNumberOfParametersToChange == 0)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerParameterValues() : No parameters to change."));

		return false;
	}

	// Create the parameter name and values arrays.
	WCHAR **pcParameterNames = new WCHAR *[iNumberOfParametersToChange];
	WCHAR **pcParameterValues = new WCHAR *[iNumberOfParametersToChange];
	for(int iParameterIndex=0; iParameterIndex<iNumberOfParametersToChange; iParameterIndex++)
	{
		pcParameterNames[iParameterIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(pcParameterNames[iParameterIndex], STANDARD_STRING_SIZE, p_vsParameterNames.at(iParameterIndex).c_str());

		pcParameterValues[iParameterIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(pcParameterValues[iParameterIndex], STANDARD_STRING_SIZE, p_vsParameterValues.at(iParameterIndex).c_str());
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setPrintServerParameterValues(iNumberOfParametersToChange, (const WCHAR **)pcParameterNames, (const WCHAR **)pcParameterValues))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerParameterValues() : Failed to send the command."));

		return false;
	}

	// Clean up the parameter name and values arrays.
	for(int iParameterIndex=0; iParameterIndex<iNumberOfParametersToChange; iParameterIndex++)
	{
		delete [] pcParameterNames[iParameterIndex];
		pcParameterNames[iParameterIndex] = NULL;

		delete [] pcParameterValues[iParameterIndex];
		pcParameterValues[iParameterIndex] = NULL;
	}
	delete [] pcParameterNames;
	pcParameterNames = NULL;
	delete [] pcParameterValues;
	pcParameterValues = NULL;

	return true;
}
bool CCPPTCP_IPClient::setPrintServerNodeInformation(wstring p_sNodePath, wstring &p_sNodeInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setPrintServerNodeInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerNodeInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setPrintServerNodeInformation(p_sNodePath.c_str(), p_sNodeInformation.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerNodeInformation() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::getPrintServerParameterValue(wstring p_sParameterName, wstring &p_sParameterValue)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getPrintServerParameterValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintServerParameterValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcParameterValue[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->getPrintServerParameterValue(p_sParameterName.c_str(), pcParameterValue, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintServerParameterValue() : Failed to send the command."));

		return false;
	}
	p_sParameterValue = wstring(pcParameterValue);

	return true;
}
bool CCPPTCP_IPClient::getPrintServerNodeInformation(wstring p_sNodePath, wstring p_sExcludedNodeTypes, wstring &p_sNodeInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getPrintServerNodeInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintServerNodeInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcNodeInformation[EXTENDED_STRING_SIZE];
	if(!m_pxTCP_IPClient->getPrintServerNodeInformation(p_sNodePath.c_str(), p_sExcludedNodeTypes.c_str(), pcNodeInformation, EXTENDED_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintServerNodeInformation() : Failed to send the command."));

		return false;
	}
	p_sNodeInformation = wstring(pcNodeInformation);

	return true;
}


bool CCPPTCP_IPClient::setParameterStoreValue(wstring p_sParameterName, wstring p_sParameterValue)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setParameterStoreValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setParameterStoreValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setParameterStoreValue(p_sParameterName.c_str(), p_sParameterValue.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setParameterStoreValue() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::getParameterStoreValue(wstring p_sParameterName, wstring &p_sParameterValue)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getParameterStoreValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getParameterStoreValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcParameterValue[EXTENDED_STRING_SIZE];
	if(!m_pxTCP_IPClient->getParameterStoreValue(p_sParameterName.c_str(), pcParameterValue, EXTENDED_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getParameterStoreValue() : Failed to send the command."));

		return false;
	}
	p_sParameterValue = wstring(pcParameterValue);

	return true;
}
bool CCPPTCP_IPClient::deleteParameterStoreValue(wstring p_sParameterName)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::deleteParameterStoreValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::deleteParameterStoreValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->deleteParameterStoreValue(p_sParameterName.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::deleteParameterStoreValue() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::abortOperationPrintServer(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::abortOperationPrintServer()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abortOperationPrintServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->abortOperationPrintServer())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abortOperationPrintServer() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::initialiseOperationPrintServer(wstring p_sConfigurationFolder)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::initialiseOperationPrintServer()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialiseOperationPrintServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->initialiseOperationPrintServer(p_sConfigurationFolder.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialiseOperationPrintServer() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::shutdownOperationPrintServer(wstring p_sPCOperationCode)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::shutdownOperationPrintServer()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::shutdownOperationPrintServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->shutdownOperationPrintServer(p_sPCOperationCode.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::shutdownOperationPrintServer() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::getSystemInformation(wstring &p_sSystemInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getSystemInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getSystemInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcSystemInformation[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->getSystemInformation(pcSystemInformation, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getSystemInformation() : Failed to send the command."));

		return false;
	}
	p_sSystemInformation = wstring(pcSystemInformation);

	return true;
}



bool CCPPTCP_IPClient::initialiseRenderEngine(wstring p_sConfigurationFile)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::initialiseRenderEngine()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialiseRenderEngine() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->initialiseRenderEngine(p_sConfigurationFile.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialiseRenderEngine() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::registerForRenderEngineInformation(bool p_bReturnExistingInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::registerForRenderEngineInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::registerForRenderEngineInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->registerForRenderEngineInformation(p_bReturnExistingInformation))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::registerForRenderEngineInformation() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::applyRenderMode(wstring p_sRenderMode)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::applyRenderMode()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::applyRenderMode() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->applyRenderMode(p_sRenderMode.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::applyRenderMode() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::getRenderModeNames(wstring &p_sRenderModeNames)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getRenderModeNames()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getRenderModeNames() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcRenderModeNames[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->getRenderModeNames(pcRenderModeNames, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getRenderModeNames() : Failed to send the command."));

		return false;
	}
	p_sRenderModeNames = wstring(pcRenderModeNames);

	return true;
}

bool CCPPTCP_IPClient::setRenderEngineParameterValue(wstring p_sParameterName, wstring p_sParameterValue)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setRenderEngineParameterValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setRenderEngineParameterValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setRenderEngineParameterValue(p_sParameterName.c_str(), p_sParameterValue.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setRenderEngineParameterValue() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::setRenderEngineParameterValues(vector<wstring> p_vsParameterNames, vector<wstring> p_vsParameterValues)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setRenderEngineParameterValues()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setRenderEngineParameterValues() : The TCP_IP client is NULL."));

		return false;
	}

	if(p_vsParameterNames.size() != p_vsParameterValues.size())
	{
		CString sLogMessage = _T("");
		sLogMessage.Format(_T("CCPPTCP_IPClient::setRenderEngineParameterValues() : The number of parameter names (%d) does no equal the number of parameter values (%d)."), p_vsParameterNames.size(), p_vsParameterValues.size());
		logMessage(ILogManagerInterface::LL_ERROR, sLogMessage.GetBuffer());

		return false;
	}
	int iNumberOfParametersToChange = (int)p_vsParameterNames.size();
	if(iNumberOfParametersToChange == 0)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setRenderEngineParameterValues() : No parameters to change."));

		return false;
	}

	// Create the parameter name and values arrays.
	WCHAR **pcParameterNames = new WCHAR *[iNumberOfParametersToChange];
	WCHAR **pcParameterValues = new WCHAR *[iNumberOfParametersToChange];
	for(int iParameterIndex=0; iParameterIndex<iNumberOfParametersToChange; iParameterIndex++)
	{
		pcParameterNames[iParameterIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(pcParameterNames[iParameterIndex], STANDARD_STRING_SIZE, p_vsParameterNames.at(iParameterIndex).c_str());

		pcParameterValues[iParameterIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(pcParameterValues[iParameterIndex], STANDARD_STRING_SIZE, p_vsParameterValues.at(iParameterIndex).c_str());
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setRenderEngineParameterValues(iNumberOfParametersToChange, (const WCHAR **)pcParameterNames, (const WCHAR **)pcParameterValues))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setRenderEngineParameterValues() : Failed to send the command."));

		return false;
	}

	// Clean up the parameter name and values arrays.
	for(int iParameterIndex=0; iParameterIndex<iNumberOfParametersToChange; iParameterIndex++)
	{
		delete [] pcParameterNames[iParameterIndex];
		pcParameterNames[iParameterIndex] = NULL;

		delete [] pcParameterValues[iParameterIndex];
		pcParameterValues[iParameterIndex] = NULL;
	}
	delete [] pcParameterNames;
	pcParameterNames = NULL;
	delete [] pcParameterValues;
	pcParameterValues = NULL;

	return true;
}
bool CCPPTCP_IPClient::setRenderEngineNodeInformation(wstring p_sNodePath, wstring &p_sNodeInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setRenderEngineNodeInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setRenderEngineNodeInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setRenderEngineNodeInformation(p_sNodePath.c_str(), p_sNodeInformation.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setRenderEngineNodeInformation() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::getRenderEngineParameterValue(wstring p_sParameterName, wstring &p_sParameterValue)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getRenderEngineParameterValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getRenderEngineParameterValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcParameterValue[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->getRenderEngineParameterValue(p_sParameterName.c_str(), pcParameterValue, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getRenderEngineParameterValue() : Failed to send the command."));

		return false;
	}
	p_sParameterValue = wstring(pcParameterValue);

	return true;
}
bool CCPPTCP_IPClient::getRenderEngineNodeInformation(wstring p_sNodePath, wstring p_sExcludedNodeTypes, wstring &p_sNodeInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getRenderEngineNodeInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getRenderEngineNodeInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcNodeInformation[EXTENDED_STRING_SIZE];
	if(!m_pxTCP_IPClient->getRenderEngineNodeInformation(p_sNodePath.c_str(), p_sExcludedNodeTypes.c_str(), pcNodeInformation, EXTENDED_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getRenderEngineNodeInformation() : Failed to send the command."));

		return false;
	}
	p_sNodeInformation = wstring(pcNodeInformation);

	return true;
}


bool CCPPTCP_IPClient::abortRenderEngine(bool p_bSendInNewClient)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::abortRenderEngine()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abortRenderEngine() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->abortRenderEngine(p_bSendInNewClient))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abortRenderEngine() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::loadDocument(wstring p_sDocumentPath)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::loadDocument()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::loadDocument() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->loadDocument(p_sDocumentPath.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::loadDocument() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::primeForRendering(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::primeForRendering()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::primeForRendering() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->primeForRendering())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::primeForRendering() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::startRender(wstring p_sRenderOutputPath)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::startRender()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startRender() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->startRender(p_sRenderOutputPath.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startRender() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::renderBitmap(wstring p_sTIFFInputPath, wstring p_sBitmapOutputPath, double p_dOutputWidth, double p_dOutputHeight)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::renderBitmap()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::renderBitmap() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->renderBitmap(p_sTIFFInputPath.c_str(), p_sBitmapOutputPath.c_str(), p_dOutputWidth, p_dOutputHeight))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::renderBitmap() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::renderLabels(int p_iStartLabelNumber, int p_iNumberOfLabels)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::renderLabels()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::renderLabels() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->renderLabels(p_iStartLabelNumber, p_iNumberOfLabels))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::renderLabels() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::startRenderOnDemand(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::startRenderOnDemand()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startRenderOnDemand() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->startRenderOnDemand())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startRenderOnDemand() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::addRenderonDemandItem(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::addRenderonDemandItem()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::addRenderonDemandItem() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->addRenderonDemandItem())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::addRenderonDemandItem() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::endRenderOnDemand(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::endRenderOnDemand()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::endRenderOnDemand() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->endRenderOnDemand())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::endRenderOnDemand() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::changeImageFile(wstring p_sImageFilePath)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::changeImageFile()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::changeImageFile() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->changeImageFile(p_sImageFilePath.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::changeImageFile() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::loadImage(int p_iNumberOfTimesToLoadImage)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::loadImage()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::loadImage() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->loadImage(p_iNumberOfTimesToLoadImage))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::loadImage() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::startLoadOnDemand(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::startLoadOnDemand()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startLoadOnDemand() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->startLoadOnDemand())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startLoadOnDemand() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::addLoadOnDemandImage(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::addLoadOnDemandImage()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::addLoadOnDemandImage() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->addLoadOnDemandImage())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::addLoadOnDemandImage() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::endLoadOnDemand(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::endLoadOnDemand()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::endLoadOnDemand() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->endLoadOnDemand())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::endLoadOnDemand() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::incrementGlobalCounter(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::incrementGlobalCounter()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::incrementGlobalCounter() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->incrementGlobalCounter())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::incrementGlobalCounter() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::decrementGlobalCounter(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::decrementGlobalCounter()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::decrementGlobalCounter() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->decrementGlobalCounter())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::decrementGlobalCounter() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::setGlobalCounter(int p_iGlobalCounter)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setGlobalCounter()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setGlobalCounter() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setGlobalCounter(p_iGlobalCounter))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setGlobalCounter() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::copyRIPFiles(wstring p_sRIPFilesPath, wstring p_sOutputFolder, int p_iStartColourPlane, int p_iEndColourPlane, bool p_bDeleteOriginalFiles)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::copyRIPFiles()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::copyRIPFiles() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->copyRIPFiles(p_sRIPFilesPath.c_str(), p_sOutputFolder.c_str(), p_iStartColourPlane, p_iEndColourPlane, p_bDeleteOriginalFiles))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::copyRIPFiles() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::getPreview(int p_iPreviewWidth, int p_iPreviewHeight, wstring &p_sPreviewData)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getPreview()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPreview() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR *pcPreviewData = new WCHAR[EXTENDED_STRING_SIZE];
	if(!pcPreviewData)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPreview() : Failed to create the preview data buffer."));

		return false;
	}
	if(!m_pxTCP_IPClient->getPreview(p_iPreviewWidth, p_iPreviewHeight, pcPreviewData, EXTENDED_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPreview() : Failed to send the command."));

		return false;
	}
	p_sPreviewData = wstring(pcPreviewData);
	delete [] pcPreviewData;
	pcPreviewData = NULL;

	return true;
}



bool CCPPTCP_IPClient::initialisePrintController(wstring p_sConfigurationFile)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::initialisePrintController()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialisePrintController() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->initialisePrintController(p_sConfigurationFile.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialisePrintController() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::registerForPrintControllerInformation(bool p_bReturnExistingInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::registerForPrintControllerInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::registerForPrintControllerInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->registerForPrintControllerInformation(p_bReturnExistingInformation))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::registerForPrintControllerInformation() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::applyPrintMode(wstring p_sPrintMode)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::applyPrintMode()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::applyPrintMode() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->applyPrintMode(p_sPrintMode.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::applyPrintMode() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::getPrintModeNames(wstring &p_sPrintModeNames)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getPrintModeNames()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintModeNames() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcPrintModeNames[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->getPrintModeNames(pcPrintModeNames, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintModeNames() : Failed to send the command."));

		return false;
	}
	p_sPrintModeNames = wstring(pcPrintModeNames);

	return true;
}

bool CCPPTCP_IPClient::setPrintControllerParameterValue(wstring p_sParameterName, wstring p_sParameterValue)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setPrintControllerParameterValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintControllerParameterValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setPrintControllerParameterValue(p_sParameterName.c_str(), p_sParameterValue.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintControllerParameterValue() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::setPrintControllerParameterValues(vector<wstring> p_vsParameterNames, vector<wstring> p_vsParameterValues)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setPrintControllerParameterValues()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintControllerParameterValues() : The TCP_IP client is NULL."));

		return false;
	}

	if(p_vsParameterNames.size() != p_vsParameterValues.size())
	{
		CString sLogMessage = _T("");
		sLogMessage.Format(_T("CCPPTCP_IPClient::setPrintControllerParameterValues() : The number of parameter names (%d) does no equal the number of parameter values (%d)."), p_vsParameterNames.size(), p_vsParameterValues.size());
		logMessage(ILogManagerInterface::LL_ERROR, sLogMessage.GetBuffer());

		return false;
	}
	int iNumberOfParametersToChange = (int)p_vsParameterNames.size();
	if(iNumberOfParametersToChange == 0)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintControllerParameterValues() : No parameters to change."));

		return false;
	}

	// Create the parameter name and values arrays.
	WCHAR **pcParameterNames = new WCHAR *[iNumberOfParametersToChange];
	WCHAR **pcParameterValues = new WCHAR *[iNumberOfParametersToChange];
	for(int iParameterIndex=0; iParameterIndex<iNumberOfParametersToChange; iParameterIndex++)
	{
		pcParameterNames[iParameterIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(pcParameterNames[iParameterIndex], STANDARD_STRING_SIZE, p_vsParameterNames.at(iParameterIndex).c_str());

		pcParameterValues[iParameterIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(pcParameterValues[iParameterIndex], STANDARD_STRING_SIZE, p_vsParameterValues.at(iParameterIndex).c_str());
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setPrintControllerParameterValues(iNumberOfParametersToChange, (const WCHAR **)pcParameterNames, (const WCHAR **)pcParameterValues))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintControllerParameterValues() : Failed to send the command."));

		return false;
	}

	// Clean up the parameter name and values arrays.
	for(int iParameterIndex=0; iParameterIndex<iNumberOfParametersToChange; iParameterIndex++)
	{
		delete [] pcParameterNames[iParameterIndex];
		pcParameterNames[iParameterIndex] = NULL;

		delete [] pcParameterValues[iParameterIndex];
		pcParameterValues[iParameterIndex] = NULL;
	}
	delete [] pcParameterNames;
	pcParameterNames = NULL;
	delete [] pcParameterValues;
	pcParameterValues = NULL;

	return true;
}
bool CCPPTCP_IPClient::setPrintControllerNodeInformation(wstring p_sNodePath, wstring &p_sNodeInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setPrintControllerNodeInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintControllerNodeInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setPrintControllerNodeInformation(p_sNodePath.c_str(), p_sNodeInformation.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintControllerNodeInformation() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::getPrintControllerParameterValue(wstring p_sParameterName, wstring &p_sParameterValue)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getPrintControllerParameterValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintControllerParameterValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcParameterValue[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->getPrintControllerParameterValue(p_sParameterName.c_str(), pcParameterValue, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintControllerParameterValue() : Failed to send the command."));

		return false;
	}
	p_sParameterValue = wstring(pcParameterValue);

	return true;
}
bool CCPPTCP_IPClient::getPrintControllerNodeInformation(wstring p_sNodePath, wstring p_sExcludedNodeTypes, wstring &p_sNodeInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getPrintControllerNodeInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintControllerNodeInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcNodeInformation[EXTENDED_STRING_SIZE];
	if(!m_pxTCP_IPClient->getPrintControllerNodeInformation(p_sNodePath.c_str(), p_sExcludedNodeTypes.c_str(), pcNodeInformation, EXTENDED_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintControllerNodeInformation() : Failed to send the command."));

		return false;
	}
	p_sNodeInformation = wstring(pcNodeInformation);

	return true;
}


bool CCPPTCP_IPClient::abortPrintController(bool p_bSendInNewClient)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::abortPrintController()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abortPrintController() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->abortPrintController(p_bSendInNewClient))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abortPrintController() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::print(int p_iNumberOfPrints, wstring p_sRIPFileItemPath, int p_iNumberOfCopies)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::print()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::print() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->print(p_iNumberOfPrints, p_sRIPFileItemPath.c_str(), p_iNumberOfCopies))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::print() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::print(int p_iNumberOfPrints, vector<wstring> p_vsRIPFileItemPaths, vector<int> p_viNumberOfCopies)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::print()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::print() : The TCP_IP client is NULL."));

		return false;
	}

	// Create the array holding the RIP file item paths and the number of copies.
	int iNumberOfItems = (int)p_vsRIPFileItemPaths.size();
	WCHAR **ppcRIPFileItemPaths = new WCHAR *[iNumberOfItems];
	int *piNumberOfCopies = new int[iNumberOfItems];
	for(int iRIPFileItemPathIndex=0; iRIPFileItemPathIndex<iNumberOfItems; iRIPFileItemPathIndex++)
	{
		ppcRIPFileItemPaths[iRIPFileItemPathIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(ppcRIPFileItemPaths[iRIPFileItemPathIndex], STANDARD_STRING_SIZE, p_vsRIPFileItemPaths.at(iRIPFileItemPathIndex).c_str());

		piNumberOfCopies[iRIPFileItemPathIndex] = p_viNumberOfCopies.at(iRIPFileItemPathIndex);
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->print(p_iNumberOfPrints, iNumberOfItems, ppcRIPFileItemPaths, piNumberOfCopies))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::print() : Failed to send the command."));

		return false;
	}

	// Clean up the RIP file item paths and the number of copies arrays.
	for(int iRIPFileItemPathIndex=0; iRIPFileItemPathIndex<iNumberOfItems; iRIPFileItemPathIndex++)
	{
		delete [] ppcRIPFileItemPaths[iRIPFileItemPathIndex];
		ppcRIPFileItemPaths[iRIPFileItemPathIndex] = NULL;
	}
	delete [] ppcRIPFileItemPaths;
	ppcRIPFileItemPaths = NULL;
	delete [] piNumberOfCopies;
	piNumberOfCopies = NULL;

	return true;
}
bool CCPPTCP_IPClient::initialisePrint(int p_iNumberOfPrints, wstring p_sRIPFileItemPath, int p_iNumberOfCopies)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::initialisePrint()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialisePrint() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->initialisePrint(p_iNumberOfPrints, p_sRIPFileItemPath.c_str(), p_iNumberOfCopies))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialisePrint() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::initialisePrint(int p_iNumberOfPrints, vector<wstring> p_vsRIPFileItemPaths, vector<int> p_viNumberOfCopies)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::initialisePrint()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialisePrint() : The TCP_IP client is NULL."));

		return false;
	}

	// Create the array holding the RIP file item paths and the number of copies.
	int iNumberOfItems = (int)p_vsRIPFileItemPaths.size();
	WCHAR **ppcRIPFileItemPaths = new WCHAR *[iNumberOfItems];
	int *piNumberOfCopies = new int[iNumberOfItems];
	for(int iRIPFileItemPathIndex=0; iRIPFileItemPathIndex<iNumberOfItems; iRIPFileItemPathIndex++)
	{
		ppcRIPFileItemPaths[iRIPFileItemPathIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(ppcRIPFileItemPaths[iRIPFileItemPathIndex], STANDARD_STRING_SIZE, p_vsRIPFileItemPaths.at(iRIPFileItemPathIndex).c_str());

		piNumberOfCopies[iRIPFileItemPathIndex] = p_viNumberOfCopies.at(iRIPFileItemPathIndex);
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->initialisePrint(p_iNumberOfPrints, iNumberOfItems, ppcRIPFileItemPaths, piNumberOfCopies))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialisePrint() : Failed to send the command."));

		return false;
	}

	// Clean up the RIP file item paths and the number of copies arrays.
	for(int iRIPFileItemPathIndex=0; iRIPFileItemPathIndex<iNumberOfItems; iRIPFileItemPathIndex++)
	{
		delete [] ppcRIPFileItemPaths[iRIPFileItemPathIndex];
		ppcRIPFileItemPaths[iRIPFileItemPathIndex] = NULL;
	}
	delete [] ppcRIPFileItemPaths;
	ppcRIPFileItemPaths = NULL;
	delete [] piNumberOfCopies;
	piNumberOfCopies = NULL;

	return true;
}
bool CCPPTCP_IPClient::startInitialisedPrint(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::startInitialisedPrint()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startInitialisedPrint() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->startInitialisedPrint())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startInitialisedPrint() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::waitForPrintInitialised(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::waitForPrintInitialised()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::waitForPrintInitialised() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->waitForPrintInitialised())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::waitForPrintInitialised() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::waitForPrintFinished(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::waitForPrintFinished()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::waitForPrintFinished() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->waitForPrintFinished())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::waitForPrintFinished() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::startPrintQueueMode(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::startPrintQueueMode()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startPrintQueueMode() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->startPrintQueueMode())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startPrintQueueMode() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::addPrintQueueItem(wstring p_sRIPFileItemPath, int p_iNumberOfCopies)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::addPrintQueueItem()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::addPrintQueueItem() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->addPrintQueueItem(p_sRIPFileItemPath.c_str(), p_iNumberOfCopies))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::addPrintQueueItem() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::addPrintQueueItem(vector<wstring> p_vsRIPFileItemPaths, vector<int> p_viNumberOfCopies)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::addPrintQueueItem()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::addPrintQueueItem() : The TCP_IP client is NULL."));

		return false;
	}

	// Create the array holding the RIP file item paths and the number of copies.
	int iNumberOfItems = (int)p_vsRIPFileItemPaths.size();
	WCHAR **ppcRIPFileItemPaths = new WCHAR *[iNumberOfItems];
	int *piNumberOfCopies = new int[iNumberOfItems];
	for(int iRIPFileItemPathIndex=0; iRIPFileItemPathIndex<iNumberOfItems; iRIPFileItemPathIndex++)
	{
		ppcRIPFileItemPaths[iRIPFileItemPathIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(ppcRIPFileItemPaths[iRIPFileItemPathIndex], STANDARD_STRING_SIZE, p_vsRIPFileItemPaths.at(iRIPFileItemPathIndex).c_str());

		piNumberOfCopies[iRIPFileItemPathIndex] = p_viNumberOfCopies.at(iRIPFileItemPathIndex);
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->addPrintQueueItem(iNumberOfItems, ppcRIPFileItemPaths, piNumberOfCopies))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::addPrintQueueItem() : Failed to send the command."));

		return false;
	}

	// Clean up the RIP file item paths and the number of copies arrays.
	for(int iRIPFileItemPathIndex=0; iRIPFileItemPathIndex<iNumberOfItems; iRIPFileItemPathIndex++)
	{
		delete [] ppcRIPFileItemPaths[iRIPFileItemPathIndex];
		ppcRIPFileItemPaths[iRIPFileItemPathIndex] = NULL;
	}
	delete [] ppcRIPFileItemPaths;
	ppcRIPFileItemPaths = NULL;
	delete [] piNumberOfCopies;
	piNumberOfCopies = NULL;

	return true;
}
bool CCPPTCP_IPClient::addSpitQueueItem(double p_dFrequency, double p_dDuration, int p_iGreyLevel)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::addSpitQueueItem()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::addSpitQueueItem() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->addSpitQueueItem(p_dFrequency, p_dDuration, p_iGreyLevel))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::addSpitQueueItem() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::endPrintQueueMode(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::endPrintQueueMode()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::endPrintQueueMode() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->endPrintQueueMode())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::endPrintQueueMode() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::pausePrint(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::pausePrint()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::pausePrint() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->pausePrint())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::pausePrint() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::resumePrint(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::resumePrint()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::resumePrint() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->resumePrint())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::resumePrint() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::sendSoftwarePrintGo(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::sendSoftwarePrintGo()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::sendSoftwarePrintGo() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->sendSoftwarePrintGo())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::sendSoftwarePrintGo() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::startSpit(double p_dFrequency, double p_dDuration)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::startSpit()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startSpit() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->startSpit(p_dFrequency, p_dDuration))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startSpit() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::stopSpit(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::stopSpit()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::stopSpit() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->stopSpit())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::stopSpit() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::waitForSpitFinished(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::waitForSpitFinished()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::waitForSpitFinished() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->waitForSpitFinished())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::waitForSpitFinished() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::deletePrintDataDirectories(wstring p_sDirectories)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::deletePrintDataDirectories()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::deletePrintDataDirectories() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->deletePrintDataDirectories(p_sDirectories.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::deletePrintDataDirectories() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::deletePrintDataFiles(wstring p_sFilePaths)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::deletePrintDataFiles()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::deletePrintDataFiles() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->deletePrintDataFiles(p_sFilePaths.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::deletePrintDataFiles() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::jogTransportMechanism(int p_iDirection, int p_iSpeed)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::jogTransportMechanism()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::jogTransportMechanism() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->jogTransportMechanism(p_iDirection, p_iSpeed))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::jogTransportMechanism() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::moveTransportMechanismToDestination(int p_iDestination)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::moveTransportMechanismToDestination()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::moveTransportMechanismToDestination() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->moveTransportMechanismToDestination(p_iDestination))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::moveTransportMechanismToDestination() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::moveTransportMechanismToPosition(double p_dXPosition, double p_dYPosition, double p_dXSpeed, double p_dYSpeed, double p_dZPosition, double p_dZSpeed)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::moveTransportMechanismToPosition()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::moveTransportMechanismToPosition() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->moveTransportMechanismToPosition(p_dXPosition, p_dYPosition, p_dXSpeed, p_dYSpeed, p_dZPosition, p_dZSpeed))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::moveTransportMechanismToPosition() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::purgeInkSystem(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::purgeInkSystem()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::purgeInkSystem() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->purgeInkSystem())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::purgeInkSystem() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::turnPrintheadsOn(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::turnPrintheadsOn()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::turnPrintheadsOn() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->turnPrintheadsOn())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::turnPrintheadsOn() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::turnPrintheadsOff(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::turnPrintheadsOff()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::turnPrintheadsOff() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->turnPrintheadsOff())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::turnPrintheadsOff() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::setPrintheadTargetTemperatures(double p_dTemperature)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setPrintheadTargetTemperatures()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintheadTargetTemperatures() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setPrintheadTargetTemperatures(p_dTemperature))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintheadTargetTemperatures() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::readPrintheadStatus(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::readPrintheadStatus()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::readPrintheadStatus() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->readPrintheadStatus())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::readPrintheadStatus() : Failed to send the command."));

		return false;
	}

	return true;
}



bool CCPPTCP_IPClient::upgradePMBFirmware(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::upgradePMBFirmware()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::upgradePMBFirmware() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->upgradePMBFirmware())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::upgradePMBFirmware() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::upgradeHeadFirmware(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::upgradeHeadFirmware()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::upgradeHeadFirmware() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->upgradeHeadFirmware())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::upgradeHeadFirmware() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::verifyPMBFirmware(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::verifyPMBFirmware()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::verifyPMBFirmware() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->verifyPMBFirmware())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::verifyPMBFirmware() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::getPrintControllerNodeErrorState(wstring p_sNodeNames, wstring &p_sErrorCodes)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getPrintControllerNodeErrorState()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintControllerNodeErrorState() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcErrorCodes[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->getPrintControllerNodeErrorState(p_sNodeNames.c_str(), pcErrorCodes, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintControllerNodeErrorState() : Failed to send the command."));

		return false;
	}
	p_sErrorCodes = wstring(pcErrorCodes);

	return true;
}


bool CCPPTCP_IPClient::initialiseNetworkController(wstring p_sConfigurationFile)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::initialiseNetworkController()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialiseNetworkController() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->initialiseNetworkController(p_sConfigurationFile.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialiseNetworkController() : Failed to send the command."));

		return false;
	}

	return true;
}

bool CCPPTCP_IPClient::registerForNetworkControllerInformation(bool p_bReturnExistingInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::registerForNetworkControllerInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::registerForNetworkControllerInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->registerForNetworkControllerInformation(p_bReturnExistingInformation))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::registerForNetworkControllerInformation() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::waitUntilAllSlaveServersInitialised(void)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::waitUntilAllSlaveServersInitialised()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::waitUntilAllSlaveServersInitialised() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->waitUntilAllSlaveServersInitialised())
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::waitUntilAllSlaveServersInitialised() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::setNetworkControllerParameterValue(wstring p_sParameterName, wstring p_sParameterValue)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setNetworkControllerParameterValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setNetworkControllerParameterValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setNetworkControllerParameterValue(p_sParameterName.c_str(), p_sParameterValue.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setNetworkControllerParameterValue() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::setNetworkControllerParameterValues(vector<wstring> p_vsParameterNames, vector<wstring> p_vsParameterValues)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setNetworkControllerParameterValues()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setNetworkControllerParameterValues() : The TCP_IP client is NULL."));

		return false;
	}

	if(p_vsParameterNames.size() != p_vsParameterValues.size())
	{
		CString sLogMessage = _T("");
		sLogMessage.Format(_T("CCPPTCP_IPClient::setNetworkControllerParameterValues() : The number of parameter names (%d) does no equal the number of parameter values (%d)."), p_vsParameterNames.size(), p_vsParameterValues.size());
		logMessage(ILogManagerInterface::LL_ERROR, sLogMessage.GetBuffer());

		return false;
	}
	int iNumberOfParametersToChange = (int)p_vsParameterNames.size();
	if(iNumberOfParametersToChange == 0)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setNetworkControllerParameterValues() : No parameters to change."));

		return false;
	}

	// Create the parameter name and values arrays.
	WCHAR **pcParameterNames = new WCHAR *[iNumberOfParametersToChange];
	WCHAR **pcParameterValues = new WCHAR *[iNumberOfParametersToChange];
	for(int iParameterIndex=0; iParameterIndex<iNumberOfParametersToChange; iParameterIndex++)
	{
		pcParameterNames[iParameterIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(pcParameterNames[iParameterIndex], STANDARD_STRING_SIZE, p_vsParameterNames.at(iParameterIndex).c_str());

		pcParameterValues[iParameterIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(pcParameterValues[iParameterIndex], STANDARD_STRING_SIZE, p_vsParameterValues.at(iParameterIndex).c_str());
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setNetworkControllerParameterValues(iNumberOfParametersToChange, (const WCHAR **)pcParameterNames, (const WCHAR **)pcParameterValues))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setNetworkControllerParameterValues() : Failed to send the command."));

		return false;
	}

	// Clean up the parameter name and values arrays.
	for(int iParameterIndex=0; iParameterIndex<iNumberOfParametersToChange; iParameterIndex++)
	{
		delete [] pcParameterNames[iParameterIndex];
		pcParameterNames[iParameterIndex] = NULL;

		delete [] pcParameterValues[iParameterIndex];
		pcParameterValues[iParameterIndex] = NULL;
	}
	delete [] pcParameterNames;
	pcParameterNames = NULL;
	delete [] pcParameterValues;
	pcParameterValues = NULL;

	return true;
}
bool CCPPTCP_IPClient::setNetworkControllerNodeInformation(wstring p_sNodePath, wstring &p_sNodeInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setNetworkControllerNodeInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setNetworkControllerNodeInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setNetworkControllerNodeInformation(p_sNodePath.c_str(), p_sNodeInformation.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setNetworkControllerNodeInformation() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::getNetworkControllerParameterValue(wstring p_sParameterName, wstring &p_sParameterValue)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getNetworkControllerParameterValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getNetworkControllerParameterValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcParameterValue[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->getNetworkControllerParameterValue(p_sParameterName.c_str(), pcParameterValue, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getNetworkControllerParameterValue() : Failed to send the command."));

		return false;
	}
	p_sParameterValue = wstring(pcParameterValue);

	return true;
}
bool CCPPTCP_IPClient::getNetworkControllerNodeInformation(wstring p_sNodePath, wstring p_sExcludedNodeTypes, wstring &p_sNodeInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getNetworkControllerNodeInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getNetworkControllerNodeInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcNodeInformation[EXTENDED_STRING_SIZE];
	if(!m_pxTCP_IPClient->getNetworkControllerNodeInformation(p_sNodePath.c_str(), p_sExcludedNodeTypes.c_str(), pcNodeInformation, EXTENDED_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getNetworkControllerNodeInformation() : Failed to send the command."));

		return false;
	}
	p_sNodeInformation = wstring(pcNodeInformation);

	return true;
}



bool CCPPTCP_IPClient::broadcastMessageToSlaveServer(wstring p_sSlaveServerName, wstring p_sMessageToBroadcast, wstring &p_sReturn)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::broadcastMessageToSlaveServer()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::broadcastMessageToSlaveServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	wchar_t pcReturn[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->broadcastMessageToSlaveServer(p_sSlaveServerName.c_str(), p_sMessageToBroadcast.c_str(), pcReturn, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::broadcastMessageToSlaveServer() : Failed to send the command."));

		return false;
	}
	p_sReturn = wstring(pcReturn);

	return true;
}
bool CCPPTCP_IPClient::broadcastMessageToAllSlaveServers(wstring p_sMessageToBroadcast, wstring &p_sReturn)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::broadcastMessageToAllSlaveServers()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::broadcastMessageToAllSlaveServers() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	wchar_t pcReturn[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->broadcastMessageToAllSlaveServers(p_sMessageToBroadcast.c_str(), pcReturn, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::broadcastMessageToAllSlaveServers() : Failed to send the command."));

		return false;
	}
	p_sReturn = wstring(pcReturn);

	return true;
}

#if 0
bool CCPPTCP_IPClient::abortNetworkController(bool p_bSendInNewClient)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::abortNetworkController()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abortNetworkController() : The TCP_IP client is NULL."));

		return false;
	}


	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->abortNetworkController(p_bSendInNewClient))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abortNetworkController() : Failed to send the command."));

		return false;
	}

	return true;
	logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abortNetworkController() : No longer supported."));

	return false;

}
#endif


bool CCPPTCP_IPClient::initialisePrintServerMonitor(wstring p_sConfigurationFile)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::initialisePrintServerMonitor()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialisePrintServerMonitor() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->initialisePrintServerMonitor(p_sConfigurationFile.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::initialisePrintServerMonitor() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::setPrintServerMonitorParameterValue(wstring p_sParameterName, wstring p_sParameterValue)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setPrintServerMonitorParameterValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerMonitorParameterValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setPrintServerMonitorParameterValue(p_sParameterName.c_str(), p_sParameterValue.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerMonitorParameterValue() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::setPrintServerMonitorParameterValues(vector<wstring> p_vsParameterNames, vector<wstring> p_vsParameterValues)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setPrintServerMonitorParameterValues()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerMonitorParameterValues() : The TCP_IP client is NULL."));

		return false;
	}

	if(p_vsParameterNames.size() != p_vsParameterValues.size())
	{
		CString sLogMessage = _T("");
		sLogMessage.Format(_T("CCPPTCP_IPClient::setPrintServerMonitorParameterValues() : The number of parameter names (%d) does no equal the number of parameter values (%d)."), p_vsParameterNames.size(), p_vsParameterValues.size());
		logMessage(ILogManagerInterface::LL_ERROR, sLogMessage.GetBuffer());

		return false;
	}
	int iNumberOfParametersToChange = (int)p_vsParameterNames.size();
	if(iNumberOfParametersToChange == 0)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerMonitorParameterValues() : No parameters to change."));

		return false;
	}

	// Create the parameter name and values arrays.
	WCHAR **pcParameterNames = new WCHAR *[iNumberOfParametersToChange];
	WCHAR **pcParameterValues = new WCHAR *[iNumberOfParametersToChange];
	for(int iParameterIndex=0; iParameterIndex<iNumberOfParametersToChange; iParameterIndex++)
	{
		pcParameterNames[iParameterIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(pcParameterNames[iParameterIndex], STANDARD_STRING_SIZE, p_vsParameterNames.at(iParameterIndex).c_str());

		pcParameterValues[iParameterIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(pcParameterValues[iParameterIndex], STANDARD_STRING_SIZE, p_vsParameterValues.at(iParameterIndex).c_str());
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setPrintServerMonitorParameterValues(iNumberOfParametersToChange, (const WCHAR **)pcParameterNames, (const WCHAR **)pcParameterValues))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerMonitorParameterValues() : Failed to send the command."));

		return false;
	}

	// Clean up the parameter name and values arrays.
	for(int iParameterIndex=0; iParameterIndex<iNumberOfParametersToChange; iParameterIndex++)
	{
		delete [] pcParameterNames[iParameterIndex];
		pcParameterNames[iParameterIndex] = NULL;

		delete [] pcParameterValues[iParameterIndex];
		pcParameterValues[iParameterIndex] = NULL;
	}
	delete [] pcParameterNames;
	pcParameterNames = NULL;
	delete [] pcParameterValues;
	pcParameterValues = NULL;

	return true;
}
bool CCPPTCP_IPClient::setPrintServerMonitorNodeInformation(wstring p_sNodePath, wstring &p_sNodeInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::setPrintServerMonitorNodeInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerMonitorNodeInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->setPrintServerMonitorNodeInformation(p_sNodePath.c_str(), p_sNodeInformation.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::setPrintServerMonitorNodeInformation() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::getPrintServerMonitorParameterValue(wstring p_sParameterName, wstring &p_sParameterValue)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getPrintServerMonitorParameterValue()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintServerMonitorParameterValue() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcParameterValue[STANDARD_STRING_SIZE];
	if(!m_pxTCP_IPClient->getPrintServerMonitorParameterValue(p_sParameterName.c_str(), pcParameterValue, STANDARD_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintServerMonitorParameterValue() : Failed to send the command."));

		return false;
	}
	p_sParameterValue = wstring(pcParameterValue);

	return true;
}
bool CCPPTCP_IPClient::getPrintServerMonitorNodeInformation(wstring p_sNodePath, wstring p_sExcludedNodeTypes, wstring &p_sNodeInformation)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::getPrintServerMonitorNodeInformation()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintServerMonitorNodeInformation() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	WCHAR pcNodeInformation[EXTENDED_STRING_SIZE];
	if(!m_pxTCP_IPClient->getPrintServerMonitorNodeInformation(p_sNodePath.c_str(), p_sExcludedNodeTypes.c_str(), pcNodeInformation, EXTENDED_STRING_SIZE))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::getPrintServerMonitorNodeInformation() : Failed to send the command."));

		return false;
	}
	p_sNodeInformation = wstring(pcNodeInformation);

	return true;
}


bool CCPPTCP_IPClient::abortPrintServerMonitor(bool p_bSendInNewClient)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::abortPrintServerMonitor()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abortPrintServerMonitor() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->abortPrintServerMonitor(p_bSendInNewClient))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::abortPrintServerMonitor() : Failed to send the command."));

		return false;
	}

	return true;
}


bool CCPPTCP_IPClient::launchPrintServerMonitorPrintServer(wstring p_sPrintServerName, wstring p_sConfigurationPath, int p_iPort, bool p_bStartMinimised, bool p_bConnectedToPMB)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::launchPrintServerMonitorPrintServer()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::launchPrintServerMonitorPrintServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->launchPrintServerMonitorPrintServer(p_sPrintServerName.c_str(), p_sConfigurationPath.c_str(), p_iPort, p_bStartMinimised, p_bConnectedToPMB))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::launchPrintServerMonitorPrintServer() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::launchPrintServerMonitorPrintServers(vector<wstring> p_vsPrintServerNames, vector<wstring> p_vsConfigurationPaths, vector<int> p_viPorts, vector<bool> p_vbStartMinimised, vector<bool> p_vbConnectedToPMB)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::launchPrintServerMonitorPrintServers()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::launchPrintServerMonitorPrintServers() : The TCP_IP client is NULL."));

		return false;
	}

	if(p_vsPrintServerNames.size() != p_vsConfigurationPaths.size())
	{
		CString sLogMessage = _T("");
		sLogMessage.Format(_T("CCPPTCP_IPClient::launchPrintServerMonitorPrintServers() : The number of print servers (%d) does no equal the number of configuration paths (%d)."), p_vsPrintServerNames.size(), p_vsConfigurationPaths.size());
		logMessage(ILogManagerInterface::LL_ERROR, sLogMessage.GetBuffer());

		return false;
	}
	if(p_vsPrintServerNames.size() != p_viPorts.size())
	{
		CString sLogMessage = _T("");
		sLogMessage.Format(_T("CCPPTCP_IPClient::launchPrintServerMonitorPrintServers() : The number of print servers (%d) does no equal the number of ports (%d)."), p_vsPrintServerNames.size(), p_viPorts.size());
		logMessage(ILogManagerInterface::LL_ERROR, sLogMessage.GetBuffer());

		return false;
	}
	if(p_vsPrintServerNames.size() != p_vbStartMinimised.size())
	{
		CString sLogMessage = _T("");
		sLogMessage.Format(_T("CCPPTCP_IPClient::launchPrintServerMonitorPrintServers() : The number of print servers (%d) does no equal the number of start minimised (%d)."), p_vsPrintServerNames.size(), p_vbStartMinimised.size());
		logMessage(ILogManagerInterface::LL_ERROR, sLogMessage.GetBuffer());

		return false;
	}
	if(p_vsPrintServerNames.size() != p_vbConnectedToPMB.size())
	{
		CString sLogMessage = _T("");
		sLogMessage.Format(_T("CCPPTCP_IPClient::launchPrintServerMonitorPrintServers() : The number of print servers (%d) does no equal the number of connected to PMB (%d)."), p_vsPrintServerNames.size(), p_vbConnectedToPMB.size());
		logMessage(ILogManagerInterface::LL_ERROR, sLogMessage.GetBuffer());

		return false;
	}	
	int iNumberOfPrintServersToLaunch = (int)p_vsPrintServerNames.size();
	if(iNumberOfPrintServersToLaunch == 0)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::launchPrintServerMonitorPrintServers() : No print servers."));

		return false;
	}

	// Create the parameter name and values arrays.
	WCHAR **pcPrintServerNames = new WCHAR *[iNumberOfPrintServersToLaunch];
	WCHAR **pcConfigurationPaths = new WCHAR *[iNumberOfPrintServersToLaunch];
	int *piPorts = new int[iNumberOfPrintServersToLaunch];
	bool *pbStartMinimised = new bool[iNumberOfPrintServersToLaunch];
	bool *pbConnectedToPMB = new bool[iNumberOfPrintServersToLaunch];
	for(int iPrintServerIndex=0; iPrintServerIndex<iNumberOfPrintServersToLaunch; iPrintServerIndex++)
	{
		pcPrintServerNames[iPrintServerIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(pcPrintServerNames[iPrintServerIndex], STANDARD_STRING_SIZE, p_vsPrintServerNames.at(iPrintServerIndex).c_str());

		pcConfigurationPaths[iPrintServerIndex] = new WCHAR[STANDARD_STRING_SIZE];
		wcscpy_s(pcConfigurationPaths[iPrintServerIndex], STANDARD_STRING_SIZE, p_vsConfigurationPaths.at(iPrintServerIndex).c_str());

		piPorts[iPrintServerIndex] = p_viPorts.at(iPrintServerIndex);

		pbStartMinimised[iPrintServerIndex] = p_vbStartMinimised.at(iPrintServerIndex);

		pbConnectedToPMB[iPrintServerIndex] = p_vbConnectedToPMB.at(iPrintServerIndex);
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->launchPrintServerMonitorPrintServers(iNumberOfPrintServersToLaunch, (const WCHAR **)pcPrintServerNames, (const WCHAR **)pcConfigurationPaths, piPorts, pbStartMinimised, pbConnectedToPMB))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::launchPrintServerMonitorPrintServers() : Failed to send the command."));

		return false;
	}

	// Clean up the parameter name and values arrays.
	for(int iPrintServerIndex=0; iPrintServerIndex<iNumberOfPrintServersToLaunch; iPrintServerIndex++)
	{
		delete [] pcPrintServerNames[iPrintServerIndex];
		pcPrintServerNames[iPrintServerIndex] = NULL;

		delete [] pcConfigurationPaths[iPrintServerIndex];
		pcConfigurationPaths[iPrintServerIndex] = NULL;
	}
	delete [] pcPrintServerNames;
	pcPrintServerNames = NULL;
	delete [] pcConfigurationPaths;
	pcConfigurationPaths = NULL;
	delete [] piPorts;
	piPorts = NULL;
	delete [] pbStartMinimised;
	pbStartMinimised = NULL;
	delete [] pbConnectedToPMB;
	pbConnectedToPMB = NULL;

	return true;
}
bool CCPPTCP_IPClient::startPrintServerMonitorPrintServer(wstring p_sPrintServerName)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::startPrintServerMonitorPrintServer()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startPrintServerMonitorPrintServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->startPrintServerMonitorPrintServer(p_sPrintServerName.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::startPrintServerMonitorPrintServer() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::shutdownPrintServerMonitorPrintServer(wstring p_sPrintServerName)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::shutdownPrintServerMonitorPrintServer()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::shutdownPrintServerMonitorPrintServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->shutdownPrintServerMonitorPrintServer(p_sPrintServerName.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::shutdownPrintServerMonitorPrintServer() : Failed to send the command."));

		return false;
	}

	return true;
}
bool CCPPTCP_IPClient::restartPrintServerMonitorPrintServer(wstring p_sPrintServerName)
{
	logMessage(ILogManagerInterface::LL_LOW, _T("CCPPTCP_IPClient::restartPrintServerMonitorPrintServer()"));

	// Do not try and talk to the TCP_IPClient object and fail if it has not been created.
	if(!m_pxTCP_IPClient)
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::restartPrintServerMonitorPrintServer() : The TCP_IP client is NULL."));

		return false;
	}

	// Call the function in the DLL.
	if(!m_pxTCP_IPClient->restartPrintServerMonitorPrintServer(p_sPrintServerName.c_str()))
	{
		logMessage(ILogManagerInterface::LL_ERROR, _T("CCPPTCP_IPClient::restartPrintServerMonitorPrintServer() : Failed to send the command."));

		return false;
	}

	return true;
}