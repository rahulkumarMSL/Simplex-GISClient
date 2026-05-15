///////////////////////////////////////////////////////////////////////////////////////////////
//
// Copyright (C) 2007-2011 Global Inkjet Systems Ltd.
//
// E-mail: support@globalinkjetsystems.com
//
// Web:	http://www.globalinkjetsystems.com/
//
// PrintheadInformationInterface.h
//
// Version: 1.6
// 
///////////////////////////////////////////////////////////////////////////////////////////////


#pragma once



/*
  This is the interface for the printhead information. Any class which implements this interface will provide information about
  a printhead, which is returned in a printhead status information message.
 */
class IPrintheadInformationInterface
{
protected:


public:
	/*
	  The constructor for the CPrintheadInformation class, which is responsible for initialising variables.
	 */
	IPrintheadInformationInterface(void)
	{
	}
	/*
	  The destructor for the CPrintheadInformation class.
	 */
	virtual ~IPrintheadInformationInterface(void)
	{
	}

	
	/*
	  This returns the name of the Print Manager node which holds this Printhead node.
	  \param The name of the Print Manager node which holds this Printhead node.
	 */
	virtual const WCHAR * getPrintManagerName(void) const = 0;
	/*
	  This sets the name of the Print Manager node which holds this Printhead node to the name given.
	  \param p_pcPrintManagerName - The new name of the Print Manager node which holds this Printhead node.
	  \return Whether the name was set correctly or not.
	 */
	virtual bool setPrintManagerName(const WCHAR *p_pcPrintManagerName) = 0;
	/*
	  This returns the name of the Print Line Manager node which holds this Printhead node.
	  \param The name of the Print Line Manager node which holds this Printhead node.
	 */
	virtual const WCHAR * getPrintLineManagerName(void) const = 0;
	/*
	  This sets the name of the Print Line Manager node which holds this Printhead node to the name given.
	  \param p_pcPrintLineManagerName - The new name of the Print Line Manager node which holds this Printhead node.
	  \return Whether the name was set correctly or not.
	 */
	virtual bool setPrintLineManagerName(const WCHAR *p_pcPrintLineManagerName) = 0;
	/*
	  This returns the name of the Print Manager Board node which holds this Printhead node.
	  \param The name of the Print Manager Board node which holds this Printhead node.
	 */
	virtual const WCHAR * getPrintManagerBoardName(void) const = 0;
	/*
	  This sets the name of the Print Manager Board node which holds this Printhead node to the name given.
	  \param p_pcPrintManagerBoardName - The new name of the Print Manager Board node which holds this Printhead node.
	  \return Whether the name was set correctly or not.
	 */
	virtual bool setPrintManagerBoardName(const WCHAR *p_pcPrintManagerBoardName) = 0;
	/*
	  This returns name of this Printhead node.
	  \param The name of this Printhead node.
	 */
	virtual const WCHAR * getPrintheadName(void) const = 0;
	/*
	  This sets the name of this Printhead node to the name given.
	  \param p_pcPrintheadName - The new name of this Printhead node.
	  \return Whether the name was set correctly or not.
	 */
	virtual bool setPrintheadName(const WCHAR *p_pcPrintheadName) = 0;
	
	/*
	  This returns whether this printhead is enabled or not.
	  \param Whether this printhead is enabled or not.
	 */
	virtual bool getHeadEnabled(void) const = 0;
	/*
	  This sets whether this printhead is enabled or not to the value given.
	  \param p_bHeadEnabled - The new value for whether this printhead is enabled or not.
	  \return Whether the set was successful or not.
	 */
	virtual bool setHeadEnabled(bool p_bHeadEnabled) = 0;
	/*
	  This returns whether this printhead is enabled for spitting or not.
	  \param Whether this printhead is enabled for spitting or not.
	 */
	virtual bool getSpitEnabled(void) const = 0;
	/*
	  This sets whether this printhead is enabled for spitting or not to the value given.
	  \param p_bSpitEnabled - The new value for whether this printhead is enabled for spitting or not.
	  \return Whether the set was successful or not.
	 */
	virtual bool setSpitEnabled(bool p_bSpitEnabled) = 0;

	/*
	  This returns whether this printhead heater is enabled or not.
	  \param Whether this printhead heater is enabled or not.
	 */
	virtual bool getHeaterEnabled(void) const = 0;
	/*
	  This sets whether this printhead heater is enabled or not to the value given.
	  \param p_bHeaterEnabled - The new value for whether this printhead heater is enabled or not.
	  \return Whether the set was successful or not.
	 */
	virtual bool setHeaterEnabled(bool p_bHeaterEnabled) = 0;
	/*
	  This returns the current temperature of the printhead (in degrees Celcius).
	  \param The current temperature of the printhead (in degrees Celcius).
	 */
	virtual double getCurrentTemperature(void) const = 0;
	/*
	  This sets the current temperature of the printhead (in degrees Celcius) to the value given.
	  \param p_dCurrentTemperature - The new value for the current temperature of the printhead (in degrees Celcius).
	  \return Whether the current temperature was set correctly or not.
	 */
	virtual double setCurrentTemperature(double p_dCurrentTemperature) = 0;
	/*
	  This returns the target temperature of the printhead (in degrees Celcius).
	  \param The target temperature of the printhead (in degrees Celcius).
	 */
	virtual double getTargetTemperature(void) const = 0;
	/*
	  This sets the target temperature of the printhead (in degrees Celcius) to the value given.
	  \param p_dTargetTemperature - The new value for the target temperature of the printhead (in degrees Celcius).
	  \return Whether the target temperature was set correctly or not.
	 */
	virtual double setTargetTemperature(double p_dTargetTemperature) = 0;
};