''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
''
'' Copyright (C) 2007-2011 Global Inkjet Systems Ltd.
''
'' E-mail: support@globalinkjetsystems.com
''
'' Web:	http://www.globalinkjetsystems.com/
''
'' TCP_IPClient.vb
''
'' Version: 1.5
'' 
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''


Imports System.Runtime.InteropServices


' This class provides the basic functionality of a TCP/IP client by wrapping the C++ TCP_IPComms DLL.
Public Class TCP_IPClient
    Dim m_pvTCP_IPClientObject As IntPtr   ' The memory address of TCP_IPClientObject which must be passed to all calls to the DLL.


    Dim m_xConnectedCallbackDelegate As connectedCallbackDelegate               ' The delegate representing the connected callback function.
    Dim m_xDisconnectedCallbackDelegate As disconnectedCallbackDelegate         ' The delegate representing the disconnected callback function.
    Dim m_xMessageSentCallbackDelegate As messageSentCallbackDelegate           ' The delegate representing the message sent callback function.
    Dim m_xMessageReceivedCallbackDelegate As messageReceivedCallbackDelegate   ' The delegate representing the message received callback function.
    Dim m_xStatusCallbackDelegate As statusCallbackDelegate                     ' The delegate representing the status update callback function.
    Dim m_xTrafficLightCallbackDelegate As trafficLightCallbackDelegate         ' The delegate representing the traffic light update callback function.
    Dim m_xSwatheProcessedCallbackDelegate As swatheProcessedCallbackDelegate   ' The delegate representing the swathe processed update callback function.
    Dim m_xLabelNumberCallbackDelegate As labelNumberCallbackDelegate           ' The delegate representing the label number update callback function.
    Dim m_xPositionCallbackDelegate As positionCallbackDelegate                 ' The delegate representing the position update callback function.
    Dim m_xInkSystemParameterValueCallbackDelegate As inkSystemParameterValueCallbackDelegate   ' The delegate representing the ink system parameter value update callback function.
    Dim m_xPrintheadStatusCallbackDelegate As printheadStatusCallbackDelegate   ' The delegate representing the printhead status update callback function.
    Dim m_xPrintStartedCallbackDelegate As printStartedCallbackDelegate         ' The delegate representing the print started callback function.
    Dim m_xReadyToPrintCallbackDelegate As readyToPrintCallbackDelegate         ' The delegate representing the ready to print callback function.
    Dim m_xEndOfPrintingCallbackDelegate As endOfPrintingCallbackDelegate       ' The delegate representing the end of printing callback function.
    Dim m_xPrintServerReporterCallbackDelegate As printServerReporterCallbackDelegate           ' The delegate representing the print server reporter callback function.
    Dim m_xInterfaceLogCallbackDelegate As interfaceLogCallbackDelegate         ' The delegate representing the interface log callback function.


    ' Declare the functions from the DLL that can be used.
    Private Declare Function createVBTCP_IPClient Lib "GIS Utility - TCP-IP Comms.dll" (ByRef p_pvTCP_IPClientObject As IntPtr) As Boolean

    Private Declare Function destroyVBTCP_IPClient Lib "GIS Utility - TCP-IP Comms.dll" (ByVal p_pvTCP_IPClientObject As IntPtr) As Boolean

    Private Declare Function initialiseVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByVal p_bLogToFileEnabled As Boolean, _
        ByRef p_sLogFileName As String, _
        ByVal p_bLogToInterfaceEnabled As Boolean, _
        ByVal p_bExtendedNetworkLogging As Boolean, _
        ByVal p_xConnectedCallbackDelegate As connectedCallbackDelegate, _
        ByVal p_xDisconnectedCallbackDelegate As disconnectedCallbackDelegate, _
        ByVal p_xMessageSentCallbackDelegate As messageSentCallbackDelegate, _
        ByVal p_xMessageReceivedCallbackDelegate As messageReceivedCallbackDelegate, _
        ByVal p_xStatusCallbackDelegate As statusCallbackDelegate, _
        ByVal p_xTrafficLightCallbackDelegate As trafficLightCallbackDelegate, _
        ByVal p_xSwatheProcessedCallbackDelegate As swatheProcessedCallbackDelegate, _
        ByVal p_xLabelNumberCallbackDelegate As labelNumberCallbackDelegate, _
        ByVal p_xPositionCallbackDelegate As positionCallbackDelegate, _
        ByVal p_xInkSystemParameterValueCallbackDelegate As inkSystemParameterValueCallbackDelegate, _
        ByVal p_xPrintheadStatusCallbackDelegate As printheadStatusCallbackDelegate, _
        ByVal p_xPrintStartedCallbackDelegate As printStartedCallbackDelegate, _
        ByVal p_xReadyToPrintCallbackDelegate As readyToPrintCallbackDelegate, _
        ByVal p_xEndOfPrintingCallbackDelegate As endOfPrintingCallbackDelegate, _
        ByVal p_xPrintServerReporterCallbackDelegate As printServerReporterCallbackDelegate, _
        ByVal p_xInterfaceLogCallbackDelegate As interfaceLogCallbackDelegate) As Boolean

    Private Declare Function abortVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" (ByVal p_pvTCP_IPClientObject As IntPtr) As Boolean

    Private Declare Function connectToServerVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByVal p_sAddress As String, _
        ByVal p_iPort As Integer, _
        ByVal p_iConnectionTimeout As Integer) As Boolean

    Private Declare Function disconnectFromServerVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" (ByVal p_pvTCP_IPClientObject As IntPtr) As Boolean

    Private Declare Function sendMessageVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByVal p_sMessage As String, _
        ByRef p_psReturn As IntPtr) As Boolean

    Private Declare Function sendMessageFullVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByVal p_sMessage As String, _
        ByVal p_bWaitForComplete As Boolean, _
        ByRef p_piMessageID As Integer, _
        ByRef p_pbCommandSuccess As Boolean, _
        ByRef p_psReturn As IntPtr, _
        ByVal p_iResponseTimeout As Integer) As Boolean

    Private Declare Function waitForCompleteVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByVal p_iMessageID As Integer, _
        ByRef p_psReturn As IntPtr, _
        ByRef p_pbCommandSuccess As Boolean, _
        ByVal p_iResponseTimeout As Integer) As Boolean

    Private Declare Function getNumberOfIndividualResponsesVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByVal p_sCombinedResponses As String, _
        ByRef p_piNumberOfResponses As Integer) As Boolean

    Private Declare Function getResponseAtIndexVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByVal p_sCombinedResponses As String, _
        ByVal p_iResponseIndex As Integer, _
        ByRef p_psResponse As IntPtr) As Boolean

    Private Declare Function loadDocumentVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByVal p_sDocumentPath As String) As Boolean

    Private Declare Function getPreviewVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByVal p_iPreviewWidth As Integer, _
        ByVal p_iPreviewHeight As Integer, _
        ByRef p_sPreviewData As IntPtr) As Boolean

    Private Declare Function getPreviewDataPixelValueVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByRef p_sPreviewData As String, _
        ByVal p_iPreviewDataLength As Integer, _
        ByVal p_iPixelIndex As Integer, _
        ByRef p_iPixelValue As Integer) As Boolean

    Private Declare Function getNumberOfPrintheadInformationItemsVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByVal p_sPrintheadStatusInformation As String, _
        ByRef p_piNumberOfPrintheads As Integer) As Boolean

    Private Declare Function getPrintheadItemAtIndexInformationVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByVal p_sPrintheadStatusInformation As String, _
        ByVal p_iPrintheadIndex As Integer, _
        ByRef p_psPrintheadName As IntPtr, _
        ByRef p_pdCurrentTemperature As Double, _
        ByRef p_pdTargetTemperature As Double) As Boolean

    Private Declare Function registerForPrintServerInformationVBWrapper Lib "GIS Utility - TCP-IP Comms.dll" ( _
        ByVal p_pvTCP_IPClientObject As IntPtr, _
        ByVal p_bReturnExistingInformation As Boolean) As Boolean


    ' This function initialises the TCP/IP client.
    ' This must be called once only before any other functions are called.
    ' \param p_xConnectedCallbackDelegate - The delegate representing the connected callback function.
    ' \param p_xDisconnectedCallbackDelegate - The delegate representing the disconnected callback function.
    ' \param p_xMessageSentCallbackDelegate - The delegate representing the message sent callback function.
    ' \param p_xMessageReceivedCallbackDelegate - The delegate representing the message received callback function.
    ' \param p_xStatusCallbackDelegate - The delegate representing the status update callback function.
    ' \param p_xTrafficLightCallbackDelegate - The delegate representing the traffic light update callback function.
    ' \param p_xSwatheProcessedCallbackDelegate - The delegate representing the swathe processed update callback function.
    ' \param p_xLabelNumberCallbackDelegate - The delegate representing the label number update callback function.
    ' \param p_xPositionCallbackDelegate - The delegate representing the position update callback function.
    ' \param p_xInkSystemParameterValueCallbackDelegate - The delegate representing the ink system parameter value update callback function.
    ' \param p_xPrintheadStatusCallbackDelegate - The delegate representing the printhead status update callback function.
    ' \param p_xPrintStartedCallbackDelegate - The delegate representing the print started callback function.
    ' \param p_xReadyToPrintCallbackDelegate - The delegate representing the ready to print callback function.
    ' \param p_xEndOfPrintingCallbackDelegate - The delegate representing the end of printing callback function.
    ' \param p_xPrintServerReporterCallbackDelegate - The delegate representing the print server reporter callback function.
    ' \param p_xInterfaceLogCallbackDelegate - The delegate representing the interface log callback function.
    ' \return Whether the initialisation was successful or not.
    Public Function initialise( _
        ByVal p_xConnectedCallbackDelegate As connectedCallbackDelegate, _
        ByVal p_xDisconnectedCallbackDelegate As disconnectedCallbackDelegate, _
        ByVal p_xMessageSentCallbackDelegate As messageSentCallbackDelegate, _
        ByVal p_xMessageReceivedCallbackDelegate As messageReceivedCallbackDelegate, _
        ByVal p_xStatusCallbackDelegate As statusCallbackDelegate, _
        ByVal p_xTrafficLightCallbackDelegate As trafficLightCallbackDelegate, _
        ByVal p_xSwatheProcessedCallbackDelegate As swatheProcessedCallbackDelegate, _
        ByVal p_xLabelNumberCallbackDelegate As labelNumberCallbackDelegate, _
        ByVal p_xPositionCallbackDelegate As positionCallbackDelegate, _
        ByVal p_xInkSystemParameterValueCallbackDelegate As inkSystemParameterValueCallbackDelegate, _
        ByVal p_xPrintheadStatusCallbackDelegate As printheadStatusCallbackDelegate, _
        ByVal p_xPrintStartedCallbackDelegate As printStartedCallbackDelegate, _
        ByVal p_xReadyToPrintCallbackDelegate As readyToPrintCallbackDelegate, _
        ByVal p_xEndOfPrintingCallbackDelegate As endOfPrintingCallbackDelegate, _
        ByVal p_xPrintServerReporterCallbackDelegate As printServerReporterCallbackDelegate, _
        ByVal p_xInterfaceLogCallbackDelegate As interfaceLogCallbackDelegate _
    ) As Boolean
        ' Store the parameters.
        m_xConnectedCallbackDelegate = p_xConnectedCallbackDelegate
        m_xDisconnectedCallbackDelegate = p_xDisconnectedCallbackDelegate
        m_xMessageSentCallbackDelegate = p_xMessageSentCallbackDelegate
        m_xMessageReceivedCallbackDelegate = p_xMessageReceivedCallbackDelegate
        m_xStatusCallbackDelegate = p_xStatusCallbackDelegate
        m_xTrafficLightCallbackDelegate = p_xTrafficLightCallbackDelegate
        m_xSwatheProcessedCallbackDelegate = p_xSwatheProcessedCallbackDelegate
        m_xLabelNumberCallbackDelegate = p_xLabelNumberCallbackDelegate
        m_xPositionCallbackDelegate = p_xPositionCallbackDelegate
        m_xInkSystemParameterValueCallbackDelegate = p_xInkSystemParameterValueCallbackDelegate
        m_xPrintheadStatusCallbackDelegate = p_xPrintheadStatusCallbackDelegate
        m_xPrintStartedCallbackDelegate = p_xPrintStartedCallbackDelegate
        m_xReadyToPrintCallbackDelegate = p_xReadyToPrintCallbackDelegate
        m_xEndOfPrintingCallbackDelegate = p_xEndOfPrintingCallbackDelegate
        m_xPrintServerReporterCallbackDelegate = p_xPrintServerReporterCallbackDelegate
        m_xInterfaceLogCallbackDelegate = p_xInterfaceLogCallbackDelegate

        ' Create the TCP_IPClient object first, as it is needed in all other calls to the DLL.
        Try
            If createVBTCP_IPClient(m_pvTCP_IPClientObject) = False Then
                MsgBox("Failed to create the TCP/IP comms")
                Return False
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try

        ' Initialise the VB TCP/IP client wrapper as this must be performed before any other functions can be called on it,
        Try
            If initialiseVBWrapper(m_pvTCP_IPClientObject, False, "", False, False, m_xConnectedCallbackDelegate, m_xDisconnectedCallbackDelegate, m_xMessageSentCallbackDelegate, m_xMessageReceivedCallbackDelegate, m_xStatusCallbackDelegate, m_xTrafficLightCallbackDelegate, m_xSwatheProcessedCallbackDelegate, m_xLabelNumberCallbackDelegate, m_xPositionCallbackDelegate, m_xInkSystemParameterValueCallbackDelegate, m_xPrintheadStatusCallbackDelegate, m_xPrintStartedCallbackDelegate, m_xReadyToPrintCallbackDelegate, m_xEndOfPrintingCallbackDelegate, m_xPrintServerReporterCallbackDelegate, m_xInterfaceLogCallbackDelegate) = False Then
                MsgBox("Failed to initialise the TCP/IP comms")
                Return False
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try

        Return True
    End Function


    ' This function closes the TCP/IP client and performs any necessary clean up.
    ' This must be called once only and no other functions can be called after it.
    Public Function close() As Boolean
        ' If the TCP_IPClient object has been created then it must be destroyed, as it was created in the DLL and so is not managed and will
        ' therefore not be deleted automatically.
        If m_pvTCP_IPClientObject <> IntPtr.Zero Then
            Try
                If abortVBWrapper(m_pvTCP_IPClientObject) = False Then
                    MsgBox("Failed to abort the TCP/IP comms")
                    Return False
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
                Return False
            End Try

            Try
                If destroyVBTCP_IPClient(m_pvTCP_IPClientObject) = False Then
                    MsgBox("Failed to destroy the TCP/IP comms")
                    Return False
                End If
                m_pvTCP_IPClientObject = IntPtr.Zero
            Catch ex As Exception
                MsgBox(ex.Message)
                Return False
            End Try
        End If

        Return True
    End Function


    ' This connects to the server with the properties specified.
    ' This is only valid when the TCP_IPClient has been created and initialised.
    ' This is only valid when not connected to a server.
    ' \param p_sAddress - The IP address of the server to connect to.
    ' \param p_iPort - The computer port of the server to connect to.
    ' \param p_iConnectionTimeout - The timeout for connection in milliseconds. Use -1 for no timeout. Use 0 to have no wait and fail if a connection cannot be made immediately.
    ' \return Whether the connection was successful or not.
    Public Function connectToServer(ByVal p_sAddress As String, ByVal p_iPort As Integer, ByVal p_iConnectionTimeout As Integer) As Boolean
        ' Fail if the TCP_IPClient object has not been created.
        If m_pvTCP_IPClientObject = IntPtr.Zero Then
            MsgBox("Failed to connect to the server as no TCP/IP comms object created")
            Return False
        End If

        ' Call the connect VB wrapper function in the DLL.
        Try
            If connectToServerVBWrapper(m_pvTCP_IPClientObject, p_sAddress, p_iPort, p_iConnectionTimeout) = False Then
                MsgBox("Failed to connect to the server")
                Return False
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try

        ' Register for Print Server information so that the UI log messages are returned.
        Try
            If registerForPrintServerInformationVBWrapper(m_pvTCP_IPClientObject, True) = False Then
                MsgBox("Failed to connect to the server")
                Return False
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try

        Return True
    End Function

    ' This disconnects from the server currently connected to.
    ' This is only valid when the TCP_IPClient has been created and initialised. 
    ' This is only valid when connected to a server.
    ' \return Whether the disconnection was successful or not.
    Public Function disconnectFromServer() As Boolean
        ' Fail if the TCP_IPClient object has not been created.
        If m_pvTCP_IPClientObject = IntPtr.Zero Then
            Return True
        End If

        ' Call the disconnect VB wrapper function in the DLL.
        Try
            If disconnectFromServerVBWrapper(m_pvTCP_IPClientObject) = False Then
                MsgBox("Failed to disconnect from the server")
                Return False
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try

        Return True
    End Function


    ' This sends a message to the server currently connected to and then waits for it to complete.
    ' This is only valid when the TCP_IPClient has been created and initialised. 
    ' This is only valid when connected to a server.
    ' \param p_sMessage - The message to send.
    ' \return Whether the message was sent successful or not.
    Public Function sendMessageToServer(ByVal p_sMessage As String) As Boolean
        ' Fail if the TCP_IPClient object has not been created.
        If m_pvTCP_IPClientObject = IntPtr.Zero Then
            MsgBox("Failed to send message to the server as no TCP/IP comms object created")
            Return False
        End If

        ' Call the send message VB wrapper function in the DLL.
        Try
            Dim sReturn As IntPtr = IntPtr.Zero
            Dim bSuccess As Boolean = sendMessageVBWrapper(m_pvTCP_IPClientObject, p_sMessage, sReturn)

            If bSuccess = True And sReturn <> IntPtr.Zero Then
                Dim p_sReturn As String = Marshal.PtrToStringBSTR(sReturn)
                Marshal.FreeBSTR(sReturn)
            End If

            If Not bSuccess Then
                MsgBox("Failed to send message to the server")
                Return False
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try

        Return True
    End Function
    ' This sends a message to the server currently connected to and then waits for it to complete.
    ' This is only valid when the TCP_IPClient has been created and initialised. 
    ' This is only valid when connected to a server.
    ' \param p_sMessage - The message to send.
    ' \param p_sReturn - All of the additional information contained in the complete command will be returned in this parameter.
    ' \return Whether the message was sent successful or not.
    Public Function sendMessageToServer(ByVal p_sMessage As String, ByRef p_sReturn As String) As Boolean
        ' Fail if the TCP_IPClient object has not been created.
        If m_pvTCP_IPClientObject = IntPtr.Zero Then
            MsgBox("Failed to send message to the server as no TCP/IP comms object created")
            Return False
        End If

       ' Call the send message VB wrapper function in the DLL.
        Try
            Dim sReturn As IntPtr = IntPtr.Zero
            Dim bSuccess As Boolean = sendMessageVBWrapper(m_pvTCP_IPClientObject, p_sMessage, sReturn)

            If bSuccess = True And sReturn <> IntPtr.Zero Then
                p_sReturn = Marshal.PtrToStringBSTR(sReturn)
                Marshal.FreeBSTR(sReturn)
            End If

            If Not bSuccess Then
                MsgBox("Failed to send message to the server")
                Return False
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try

        Return True
    End Function


    ' This takes the data from a printhead status information message and returns the number of printheads are defined.
    ' \param p_pvTCP_IPVBClientObject - The CTCP_IPVBClient object used to call the function.
    ' \param p_sPrintheadStatusInformation - The printhead status information message.
    ' \param p_piNumberOfPrintheads - The number of printheads in the information.
    ' \return Whether the get was successful or not.
    Public Function getNumberOfPrintheadInformationItems(ByVal p_sPrintheadStatusInformation As String, ByRef p_piNumberOfPrintheads As Integer) As Boolean
        ' Fail if the TCP_IPClient object has not been created.
        If m_pvTCP_IPClientObject = IntPtr.Zero Then
            MsgBox("Failed to get the printhead item at index as no TCP/IP comms object created")
            Return False
        End If

        ' Call the get printhead item at index VB wrapper function in the DLL.
        Try
            If getNumberOfPrintheadInformationItemsVBWrapper(m_pvTCP_IPClientObject, p_sPrintheadStatusInformation, p_piNumberOfPrintheads) = False Then
                MsgBox("Failed to get the number of printhead information items")
                Return False
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try

        Return True
    End Function
    ' This takes the data from a printhead status information message and returns the information about it.
    ' If there is no response for the index specified, the function will fail.
    ' \param p_pvTCP_IPVBClientObject - The CTCP_IPVBClient object used to call the function.
    ' \param p_sPrintheadStatusInformation - The printhead status information message.
    ' \param p_iPrintheadIndex - The index of the printhead to return.
    ' \param p_sPrintheadName - The name of the printhead.
    ' \param p_pdCurrentTemperature - The current temperature of the printhead in degrees Celcius.
    ' \param p_pdTargetTemperature - The target temperature of the printhead in degrees Celcius.
    ' \return Whether the get was successful or not.
    Public Function getPrintheadItemAtIndexInformation(ByVal p_sPrintheadStatusInformation As String, ByVal p_iPrintheadIndex As Integer, ByRef p_psPrintheadName As String, ByRef p_pdCurrentTemperature As Double, ByRef p_pdTargetTemperature As Double) As Boolean
        ' Fail if the TCP_IPClient object has not been created.
        If m_pvTCP_IPClientObject = IntPtr.Zero Then
            MsgBox("Failed to get the printhead item information as no TCP/IP comms object created")
            Return False
        End If

        ' Call the printhead item at index information VB wrapper function in the DLL.
        Try
            Dim sReturn As IntPtr = IntPtr.Zero
            Dim bSuccess As Boolean = getPrintheadItemAtIndexInformationVBWrapper( _
                m_pvTCP_IPClientObject, _
                p_sPrintheadStatusInformation, _
                p_iPrintheadIndex, _
                sReturn, _
                p_pdCurrentTemperature, _
                p_pdTargetTemperature)

            If bSuccess = True And sReturn <> IntPtr.Zero Then
                p_psPrintheadName = Marshal.PtrToStringBSTR(sReturn)
                Marshal.FreeBSTR(sReturn)
            End If

            If Not bSuccess Then
                MsgBox("Failed to get the printhead item information")
                Return False
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try

        Return True
    End Function


    ' This function takes a list of strings (which combine to create a single block of preview data) and creates a bitmap based on it.
    ' \param p_iWidth - The width of the preview data (in pixels).
    ' \param p_iHeight - The height of the preview data (in pixels).
    ' \param p_sPreviewData - The list of blocks of preview data.
    Private Function extractPreviewData(ByRef p_sPreviewData As String, ByRef p_xBitmap As Bitmap) As Boolean
        Dim iPreviewDataHeaderCommaPosition As Integer = p_sPreviewData.IndexOf(",")
        p_sPreviewData = p_sPreviewData.Substring(iPreviewDataHeaderCommaPosition + 1, p_sPreviewData.Length - iPreviewDataHeaderCommaPosition - 1)
        Dim iPreviewDataWidthCommaPosition As Integer = p_sPreviewData.IndexOf(",")
        Dim sWidth As String = p_sPreviewData.Substring(0, iPreviewDataWidthCommaPosition)
        Dim iWidth As Integer = CInt(sWidth)
        p_sPreviewData = p_sPreviewData.Substring(iPreviewDataWidthCommaPosition + 1, p_sPreviewData.Length - iPreviewDataWidthCommaPosition - 1)
        Dim iPreviewDataHeightCommaPosition As Integer = p_sPreviewData.IndexOf(",")
        Dim sHeight As String = p_sPreviewData.Substring(0, iPreviewDataHeightCommaPosition)
        Dim iHeight As Integer = CInt(sHeight)
        p_sPreviewData = p_sPreviewData.Substring(iPreviewDataHeightCommaPosition + 1, p_sPreviewData.Length - iPreviewDataHeightCommaPosition - 1)

        Dim x As Integer = 0
        Dim y As Integer = 0

        Dim iPreviewDataLength As Integer = p_sPreviewData.Length

        p_xBitmap = New Bitmap(iWidth, iHeight, Imaging.PixelFormat.Format24bppRgb)

        Try

            For y = 0 To iHeight - 1
                For x = 0 To iWidth - 1
                    Dim iRed As Integer = 0
                    Dim bSuccess As Boolean = getPreviewDataPixelValueVBWrapper( _
                        m_pvTCP_IPClientObject, _
                        p_sPreviewData, _
                        iPreviewDataLength, _
                        (x * 3 + y * iWidth * 3) + 0, _
                        iRed)
                    If Not bSuccess Then
                        MsgBox("Failed to get the red byte")
                        Return False
                    End If
                    Dim iGreen As Integer = 0
                    bSuccess = getPreviewDataPixelValueVBWrapper( _
                        m_pvTCP_IPClientObject, _
                        p_sPreviewData, _
                        iPreviewDataLength, _
                        (x * 3 + y * iWidth * 3) + 1, _
                        iGreen)
                    If Not bSuccess Then
                        MsgBox("Failed to get the green byte")
                        Return False
                    End If
                    Dim iBlue As Integer = 0
                    bSuccess = getPreviewDataPixelValueVBWrapper( _
                        m_pvTCP_IPClientObject, _
                        p_sPreviewData, _
                        iPreviewDataLength, _
                        (x * 3 + y * iWidth * 3) + 2, _
                        iBlue)
                    If Not bSuccess Then
                        MsgBox("Failed to get the blue byte")
                        Return False
                    End If

                    p_xBitmap.SetPixel(x, (p_xBitmap.Height - 1) - y, System.Drawing.Color.FromArgb(255, iRed, iGreen, iBlue))
                Next x
            Next y
        Catch
            MsgBox("Failed to extranct the preview data")
            Return False
        End Try

        Return True
    End Function


    ' This function provides an example of how to send two get print controller parameter commands in parallel, wait
    ' for them to complete and then separate out the responses.
    ' This example can only be run when currently connected to a server.
    ' This example requires that at least one parameter in the Print Server has the unique ID "UniqueID1", "UniqueID2", "UniqueID3" and "UniqueID4".
    Public Function example() As Boolean
        ' Create a message that returns the values of the parameter which have a unique ID of "UniqueID1" and "UniqueID2"
        Dim sMessage1 As String = "P,V,P,UniqueID1,UniqueID2"
        ' Create a message that returns the values of all parameter which have a unique ID of "UniqueID3" and "UniqueID4"
        Dim sMessage2 As String = "P,V,P,UniqueID3,UniqueID4"

        ' Create the variables taht will be needed.
        Dim iMessage1ID As Integer = 0
        Dim sMessage1Return As String = ""
        Dim bMessage1CommandSuccess As Boolean = True
        Dim iMessage2ID As Integer = 0
        Dim sMessage2Return As String = ""
        Dim bMessage2CommandSuccess As Boolean = True

        Dim sReturn As IntPtr = IntPtr.Zero
        ' Send message 1 to the server.
        Dim bSuccess As Boolean = sendMessageFullVBWrapper( _
            m_pvTCP_IPClientObject, _
            sMessage1, _
            False, _
            iMessage1ID, _
            False, _
            sReturn, _
            -1)
        If Not bSuccess Then
            Return False
        End If
        If sReturn <> IntPtr.Zero Then
            Marshal.FreeBSTR(sReturn)
            sReturn = IntPtr.Zero
        End If

        ' Send message 2 to the server.
        bSuccess = sendMessageFullVBWrapper( _
            m_pvTCP_IPClientObject, _
            sMessage2, _
            False, _
            iMessage2ID, _
            False, _
            sReturn, _
            -1)
        If Not bSuccess Then
            Return False
        End If
        If sReturn <> IntPtr.Zero Then
            Marshal.FreeBSTR(sReturn)
            sReturn = IntPtr.Zero
        End If

        ' Wait for message 1 to complete.
        bSuccess = waitForCompleteVBWrapper( _
            m_pvTCP_IPClientObject, _
            iMessage1ID, _
            sReturn, _
            bMessage1CommandSuccess, _
            -1)
        If sReturn <> IntPtr.Zero Then
            sMessage1Return = Marshal.PtrToStringBSTR(sReturn)
            Marshal.FreeBSTR(sReturn)
            sReturn = IntPtr.Zero
        End If
        If Not bSuccess Then
            Return False
        End If
        If bMessage1CommandSuccess = False Then
            Return False
        End If

        ' Wait for message 2 to complete.
        bSuccess = waitForCompleteVBWrapper( _
            m_pvTCP_IPClientObject, _
            iMessage2ID, _
            sReturn, _
            bMessage2CommandSuccess, _
            -1)
        If sReturn <> IntPtr.Zero Then
            sMessage2Return = Marshal.PtrToStringBSTR(sReturn)
            Marshal.FreeBSTR(sReturn)
            sReturn = IntPtr.Zero
        End If
        If Not bSuccess Then
            Return False
        End If
        If bMessage2CommandSuccess = False Then
            Return False
        End If

        ' Extract all responses from the response to message 1.
        Dim iNumberOfResponsesFromMessage1 As Integer = 0
        If getNumberOfIndividualResponsesVBWrapper(m_pvTCP_IPClientObject, sMessage1Return, iNumberOfResponsesFromMessage1) = False Then
            Return False
        End If
        Dim iMessage1ResponseIndex As Integer = 0
        For iMessage1ResponseIndex = 0 To iNumberOfResponsesFromMessage1 - 1
            bSuccess = getResponseAtIndexVBWrapper( _
                m_pvTCP_IPClientObject, _
                sMessage1Return, _
                iMessage1ResponseIndex, _
                sReturn)
            If Not bSuccess Then
                Return False
            End If
            Dim sResponse As String = ""
            If sReturn <> IntPtr.Zero Then
                sResponse = Marshal.PtrToStringBSTR(sReturn)
                Marshal.FreeBSTR(sReturn)
                sReturn = IntPtr.Zero
            End If
            If iMessage1ResponseIndex = 0 Then
                MsgBox("UniqueID1: " + sResponse)
            ElseIf iMessage1ResponseIndex = 1 Then
                MsgBox("UniqueID2: " + sResponse)
            End If
        Next
        ' Extract all responses from the response to message 2.
        Dim iNumberOfResponsesFromMessage2 As Integer = 0
        If getNumberOfIndividualResponsesVBWrapper(m_pvTCP_IPClientObject, sMessage2Return, iNumberOfResponsesFromMessage2) = False Then
            Return False
        End If
        Dim iMessage2ResponseIndex As Integer = 0
        For iMessage2ResponseIndex = 0 To iNumberOfResponsesFromMessage2 - 1
            bSuccess = getResponseAtIndexVBWrapper( _
                m_pvTCP_IPClientObject, _
                sMessage2Return, _
                iMessage2ResponseIndex, _
                sReturn)
            If Not bSuccess Then
                Return False
            End If
            Dim sResponse As String = ""
            If sReturn <> IntPtr.Zero Then
                sResponse = Marshal.PtrToStringBSTR(sReturn)
                Marshal.FreeBSTR(sReturn)
                sReturn = IntPtr.Zero
            End If
            If iMessage2ResponseIndex = 0 Then
                MsgBox("UniqueID3: " + sResponse)
            ElseIf iMessage2ResponseIndex = 1 Then
                MsgBox("UniqueID4: " + sResponse)
            End If
        Next

        Return True
    End Function

    ' This function provides an example of how to load a document in the Print Server and get a preview.
    ' It will save the preview of size 100x100 pixels to "C:\VPIPreview.bmp"
    ' This example can only be run when currently connected to a server.
    ' This example requires that a VPI file exists at "C:\ExampleVPI.vpi".
    Public Function getPreviewExample() As Boolean
        Dim sVPIFilepath As String = "C:\ExampleVPI.vpi"
        If loadDocumentVBWrapper(m_pvTCP_IPClientObject, sVPIFilepath) = False Then
            MsgBox("Failed to load the document")
            Return False
        End If

        Dim iWidth As Integer = 10
        Dim iHeight As Integer = 10
        Dim sReturn As IntPtr = IntPtr.Zero
        If getPreviewVBWrapper(m_pvTCP_IPClientObject, iWidth, iHeight, sReturn) = False Then
            MsgBox("Failed to get the preview")
            Return False
        End If
        Dim sPreviewData As String = ""
        If sReturn <> IntPtr.Zero Then
            sPreviewData = Marshal.PtrToStringBSTR(sReturn)
            Marshal.FreeBSTR(sReturn)
        End If

        Dim xBitmap As Bitmap = Nothing
        If extractPreviewData(sPreviewData, xBitmap) = False Then
            MsgBox("Failed to extract the preview data")
            Return False
        End If
        xBitmap.Save("C:\VPIPreview.bmp")

        Return True
    End Function


    ' Declare the delegates which represent the callback functions.
    Public Delegate Function connectedCallbackDelegate() As Boolean
    Public Delegate Function disconnectedCallbackDelegate() As Boolean
    Public Delegate Function messageSentCallbackDelegate(ByVal p_sMessage As String) As Boolean
    Public Delegate Function messageReceivedCallbackDelegate(ByVal p_sMessage As String) As Boolean
    Public Delegate Function statusCallbackDelegate(ByVal p_sEngineCode As String, ByVal p_sStatus As String, ByVal p_iStatusCode As Integer) As Boolean
    Public Delegate Function trafficLightCallbackDelegate(ByVal p_iState As Integer) As Boolean
    Public Delegate Function swatheProcessedCallbackDelegate(ByVal p_iSwatheNumber As Integer) As Boolean
    Public Delegate Function labelNumberCallbackDelegate(ByVal p_iLabelNumber As Integer) As Boolean
    Public Delegate Function positionCallbackDelegate(ByVal p_dXPosition As Double, ByVal p_dYPosition As Double, ByVal p_dZPosition As Double) As Boolean
    Public Delegate Function inkSystemParameterValueCallbackDelegate(ByVal p_sParameterName As String, ByVal p_sParameterValue As String) As Boolean
    Public Delegate Function printheadStatusCallbackDelegate(ByVal p_sPrintheadStatusInformation As String) As Boolean
    Public Delegate Function printStartedCallbackDelegate() As Boolean
    Public Delegate Function readyToPrintCallbackDelegate(ByVal p_iNumberOfSwathes As Integer) As Boolean
    Public Delegate Function endOfPrintingCallbackDelegate() As Boolean
    Public Delegate Function printServerReporterCallbackDelegate(ByVal p_sMessage As String) As Boolean
    Public Delegate Function interfaceLogCallbackDelegate(ByVal p_iLevel As Integer, ByVal p_sMessage As String) As Boolean
End Class
