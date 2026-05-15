#pragma once

#include <afxmt.h>

class ISecurityManagerInterface
{
public:
	ISecurityManagerInterface(void)
	{
		
	}
	virtual ~ISecurityManagerInterface(void)
	{
		
	}


	virtual CString encryptValue(CString p_sValue, CString p_sKey) const = 0;
	virtual CString decryptValue(CString p_sValue, CString p_sKey) const = 0;
	virtual CString backupEncryptValue(CString p_sValue) const = 0;
	virtual CString backupDecryptValue(CString p_sValue) const = 0;


	virtual CString getSignature(CString p_sValue, CString p_sPrivateKey) const = 0;
	virtual bool verifySignature(CString p_sValue, CString p_sSignature, CString p_sPublicKey) const = 0;


	virtual CString getNonce(int p_iLength=-1) const = 0;

	virtual CString getGISClientAuthenticationKeyA(void) const = 0;
	virtual CString getGISClientAuthenticationKeyB(void) const = 0;
	virtual CString getLogKey(void) const = 0;
	virtual CString getSecurityFilePublicKey(void) const = 0;
	virtual CString getSecurityFileEncryptionKey(void) const = 0;

	virtual bool debugLoggingAllowed(void) const = 0;
};