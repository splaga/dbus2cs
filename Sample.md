# Input #
http://code.google.com/p/dbus2cs/source/browse/trunk/dbus2cs/testdocs/otapi/org.freesmartphone.GSM.SIM.xml.in

# Output #
```
/*This document was generated with a tool.
dbus2cs version 0.1.0.0*/

using System;
using System.Collections.Generic;
using NDesk.DBus;

namespace org.freesmartphone.GSM.SIM
{
	/// <summary>
	/// The SIM object is used to access the Subscriber Identification Module (SIM)
	/// plugged as a card into the GSM device. Use it to authenticate yourself
	/// against the GSM network and read/store data from/to the embedded SIM card memory.
	/// Limited support for accessing the on-card phonebook and messagebook is provided.
	/// </summary>
	[Interface("org.freesmartphone.GSM.SIM")]
	public interface SIM
	{
		/// <summary>
		/// Retrieve the authentication status for the SIM card.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.07 command +CPIN=?, see 3GPP TS 07.07 Chapter 8.3.
		/// </remarks>
		/// <returns>
		/// The authentication status for the SIM card. Values to expect:
		/// [ul]
		/// [li]"READY" = not waiting for any PIN or PUK,[/li]
		/// [li]"SIM PIN" = waiting for SIM PIN to be given,[/li]
		/// [li]"SIM PUK" = waiting for SIM PUK to be given,[/li]
		/// [li]"SIM PIN2" = waiting for SIM PIN 2 to be given,[/li]
		/// [li]"SIM PUK2" = waiting for SIM PUK 2 to be given.[/li]
		/// [/ul]
		/// </returns>
		string GetAuthStatus();
		/// <summary>
		/// Sent, when the authentication status for the SIM card changes.
		/// </summary>
		event AuthStatusHandler AuthStatus;
		/// <summary>
		/// Send authentication code (PIN) for the SIM card.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.07 command +CPIN="(pin)", see 3GPP TS 07.07 Chapter 8.3.
		/// </remarks>
		/// <param name="pin">
		/// The authentication code.
		/// </param>
		void SendAuthCode(string pin);
		/// <summary>
		/// Send unlock code (PUK) and new authentication code (PIN).
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.07 command +CPIN="(puk)","(new_pin)", see 3GPP TS 07.07 Chapter 8.3.
		/// </remarks>
		/// <param name="puk">
		/// The unlock code.
		/// </param>
		/// <param name="new_pin">
		/// The new authentication code.
		/// </param>
		void Unlock(string puk, string new_pin);
		/// <summary>
		/// Change the authentication code.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.07 command +CPWD="SC","(old_pin)","(new_pin)", see 3GPP TS 07.07 Chapter 7.5.
		/// </remarks>
		/// <param name="old_pin">
		/// The old authentication code.
		/// </param>
		/// <param name="new_pin">
		/// The new authentication code.
		/// </param>
		void ChangeAuthCode(string old_pin, string new_pin);
		/// <summary>
		/// Enable or disable checking for the SIM card authentification on startup.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.07 command +CLCK,"SC",(check),"(pin)", see 3GPP TS 07.07 Chapter 7.4.
		/// </remarks>
		/// <param name="check">
		/// True, to turn SIM card authentification on. False, to turn it off.
		/// </param>
		/// <param name="pin">
		/// A valid authentication code.
		/// </param>
		void SetAuthCodeRequired(bool check, string pin);
		/// <summary>
		/// Retrieve whether the SIM card checks for authentification on startup.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.07 command +CLCK,"SC",2, see 3GPP TS 07.07 Chapter 7.4.
		/// </remarks>
		/// <returns>
		/// True, if SIM card authentification is turned on. False, otherwise.
		/// </returns>
		bool GetAuthCodeRequired();
		/// <summary>
		/// Get information about the IMSI, the subscriber numbers,
		/// and the country code of the SIM card.
		/// </summary>
		/// <remarks>
		/// This can map to the following GSM 07.07 commands:
		/// [ul]
		/// [li]+CIMI (see 3GPP TS 07.07 Chapter 5.6),[/li]
		/// [li]+CNUM (see 3GPP TS 07.07 Chapter 7.1).[/li]
		/// [/ul]
		/// The dial code is computed by cross-referencing ITU E.212 (Land Mobile Numbering Plan).
		/// </remarks>
		/// <returns>
		/// Information about the SIM card. Required tuples are:
		/// [ul]
		/// [li]("imei", string)[/li]
		/// [/ul]
		/// Optional tuples are:
		/// [ul]
		/// [li]("subscriber_numbers", array),[/li]
		/// [li]("dial_prefix", string),[/li]
		/// [li]("country", string).[/li]
		/// [/ul]
		/// The subscriber_numbers array contains two-tuples formatted as (string:line-name, string:line-number).
		/// </returns>
		Dictionary<string, object> GetSimInfo();
		/// <summary>
		/// Send a generic SIM command to the SIM card.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.07 command +CSIM=(command), see 3GPP TS 07.07 Chapter 8.17.
		/// </remarks>
		/// <param name="command">
		/// The command to send, encoded as described in GSM 11.11.
		/// </param>
		/// <returns>
		/// The result of the command, encoded as described in GSM 11.11.
		/// </returns>
		string SendGenericSimCommand(string command);
		/// <summary>
		/// Send a restricted SIM command to the SIM card.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.07 command +CRSM=(command),(fileid),(p1),(p2),(p3),(data), see 3GPP TS 07.07 Chapter 8.18.
		/// </remarks>
		/// <param name="command">
		/// The command to send. Valid values are:
		///           [ul]
		///             [li]176 = READ BINARY,[/li]
		///             [li]178 = READ RECORD,[/li]
		///             [li]192 = GET RESPONSE,[/li]
		///             [li]214 = UPDATE BINARY,[/li]
		///             [li]220 = UPDATE RECORD,[/li]
		///             [li]242 = STATUS.[/li]
		///           [/ul]
		/// </param>
		/// <param name="fileid">
		/// The identifier of an elementary datafile on SIM. Mandatory for every command except STATUS.
		/// </param>
		/// <param name="p1">
		/// Parameter 1 passed to the SIM. Mandatory for every command except STATUS and GET RESPONSE.
		/// </param>
		/// <param name="p2">
		/// Parameter 1 passed to the SIM. Mandatory for every command except STATUS and GET RESPONSE.
		/// </param>
		/// <param name="p3">
		/// Parameter 1 passed to the SIM. Mandatory for every command except STATUS and GET RESPONSE.
		/// </param>
		/// <param name="data">
		/// The command data to the SIM, encoded as described in GSM 11.11.
		/// </param>
		/// <returns>
		/// The result of the command, encoded as described in GSM 11.11.
		/// </returns>
		string SendRestrictedSimCommand(int command, int fileid, int p1, int p2, int p3, string data);
		/// <summary>
		/// Retrieve the homezones coordinates, if stored on SIM.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.07 command +CRSM=176,28512,0,0,123, see 3GPP TS 07.07 Chapter 8.17.
		/// </remarks>
		/// <returns>
		/// An array containing up to four homezones in Gauss-Krueger coordinates. The array contains of four-tuples with the following format:
		/// [ol]
		/// [li](name:string), the name of the zone,[/li]
		/// [li](x:int), the X value of the zone center in Gauss-Krueger format,[/li]
		/// [li](y:int), the Y value of the zone center in Gauss-Krueger format,[/li]
		/// [li](r:radious), the R*R value of the zone dimension in meters.[/li]
		/// [/ol]
		/// </returns>
		Homezone[] GetHomeZones();
		/// <summary>
		/// Request information about the SIM phonebook.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.07 command +CPBR=?, see 3GPP TS 07.07 Chapter 8.12.
		/// </remarks>
		/// <returns>
		/// Information about the SIM phonebook storage. Tuples to expect:
		/// [ul]
		/// [li]("slots", int:value) = maximum of entries stored on the SIM,[/li]
		/// [li]("used", int:value) = actual number of entries stored on the SIM,[/li]
		/// [li]("number_length", int:value) = maximum length for the number,[/li]
		/// [li]("name_length", int:value) = maximum length for the name.[/li]
		/// [/ul]
		/// </returns>
		Dictionary<string, object> GetPhonebookInfo();
		/// <summary>
		/// Retrieve all entries from the SIM phonebook.
		/// </summary>
		/// <remarks>
		/// This vaguely maps to the GSM 07.05 command +CPBR=(index1),(index2), see 3GPP TS 07.07 Chapter 8.12.
		/// </remarks>
		/// <returns>
		/// The phonebook entries. This is an array of three-tuples. Every entry has the following structure:
		/// [ul]
		/// [li](int:index) = storage index,[/li]
		/// [li](string:name) = name,[/li]
		/// [li](string:number) = number.[/li]
		/// [/ul]
		/// </returns>
		Phonebook[] RetrievePhonebook();
		/// <summary>
		/// Delete an entry in the SIM phonebook.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.07 command +CPBW=, see 3GPP TS 07.07 Chapter 8.14.
		/// </remarks>
		/// <param name="index">
		/// Index of entry to delete.
		/// </param>
		void DeleteEntry(int index);
		/// <summary>
		/// Store an entry in the SIM phonebook.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.07 command +CPBW=(index),"(name)",(number),(ntype), see 3GPP TS 07.07 Chapter 8.14.
		/// </remarks>
		/// <param name="index">
		/// Index of entry to store.
		/// </param>
		/// <param name="name">
		/// Name corresponding to the number.
		/// </param>
		/// <param name="number">
		/// Number corresponding to the name.
		/// </param>
		void StoreEntry(int index, string name, string number);
		/// <param name="index">
		/// Index of entry to retrieve.
		/// </param>
		/// <returns>
		/// Name corresponding to the number.
		/// Number corresponding to the name.
		/// </returns>
		string RetrieveEntry(int index);
		/// <summary>
		/// Request information about the SIM messagebook.
		/// </summary>
		/// <remarks>
		/// This vaguely maps to the GSM 07.05 command +CMGL=?, see 3GPP TS 07.07 Chapter 3.4.2.
		/// </remarks>
		/// <returns>
		/// Information about the SIM messagebook storage. Tuples to expect:
		/// [ul]
		/// [li]("slots", int:value) = maximum of messages stored on the SIM,[/li]
		/// [li]("used", int:value) = actual number of messages stored on the SIM,[/li]
		/// [li]("contents_length", int:value) = maximum length for a message.[/li]
		/// [/ul]
		/// </returns>
		Dictionary<string, object> GetMessagebookInfo();
		/// <summary>
		/// Retrieve all messages from the SIM messagebook that match a given category.
		/// </summary>
		/// <remarks>
		/// This vaguely maps to the GSM 07.05 command +CMGL="(category), see 3GPP TS 07.07 Chapter 3.4.2.
		/// </remarks>
		/// <param name="category">
		/// The category of messages you want to receive. Valid categories are:
		///           [ul]
		///              [li]"all" = all messages,[/li]
		///              [li]"read" = message you have received and read,[/li]
		///              [li]"unread" = new messages you have received,[/li]
		///              [li]"sent" = messages you have sent,[/li]
		///              [li]"unsent" = message you have not sent yet.[/li]
		///            [/ul]
		/// </param>
		/// <returns>
		/// Messages matching the given category. This is an array of four-tuples. Every entry has the following structure:
		/// [ul]
		/// [li](int:index) = storage index,[/li]
		/// [li](string:status) = status of message, one of ("read", "unread", "sent", "unsent"),[/li]
		/// [li](string:number) = sender or receiver number.[/li]
		/// [li](string:content) = contents of the message.[/li]
		/// [/ul]
		/// </returns>
		Messagebook[] RetrieveMessagebook(string category);
		/// <summary>
		/// Retrieve phone number of SMS Center Number.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.05 command +CSCA?, see 3GPP TS 07.05 Chapter 3.3.1.
		/// </remarks>
		/// <returns>
		/// The SMS Center Number.
		/// </returns>
		string GetServiceCenterNumber();
		/// <summary>
		/// Set phone number of SMS Center Number.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.05 command +CSCA=(number),(type), see 3GPP TS 07.05 Chapter 3.3.1.
		/// </remarks>
		/// <param name="number">
		/// The SMS Center Number.
		/// </param>
		void SetServiceCenterNumber(string number);
		/// <summary>
		/// Sent, when a new message has been received and stored on the SIM card.
		/// </summary>
		event IncomingMessageHandler IncomingMessage;
		/// <summary>
		/// Delete a message in the SIM messagebook.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.05 command +CMGD=(index), see 3GPP TS 07.05 Chapter 3.5.4.
		/// </remarks>
		/// <param name="index">
		/// Index of message to delete.
		/// </param>
		void DeleteMessage(int index);
		/// <summary>
		/// Store a message in the SIM messagebook.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.05 command +CMGW=..., see 3GPP TS 07.05 Chapter 3.5.3.
		/// </remarks>
		/// <param name="recipient_number">
		/// Number of recipient.
		/// </param>
		/// <param name="contents">
		/// Contents of message.
		/// </param>
		/// <returns>
		/// Index of new message.
		/// </returns>
		int StoreMessage(string recipient_number, string contents);
		/// <summary>
		/// Sends a message stored in the SIM messagebook.
		/// </summary>
		/// <remarks>
		/// This maps to the GSM 07.05 command +CMGS=(index)..., see 3GPP TS 07.05 Chapter 3.5.x.
		/// </remarks>
		/// <param name="index">
		/// Index of message.
		/// </param>
		/// <returns>
		/// Transaction index.
		/// </returns>
		int SendStoredMessage(int index);
		/// <summary>
		/// Retrieve a message from the SIM messagebook.
		/// </summary>
		/// <remarks>
		/// This can map to the GSM 07.05 command +CMGR=(index), see 3GPP TS 07.05 Chapter 3.4.3.
		/// It might also map to the GSM 07.05 command +CMGL=(storage_spec), see 3GPP TS 07.05 Chapter 3.4.2.
		/// </remarks>
		/// <param name="index">
		/// Index of message to retrieve.
		/// </param>
		/// <returns>
		/// Number of sender.
		/// Contents of message.
		/// </returns>
		string RetrieveMessage(int index);
	}

	public delegate void AuthStatusHandler(string status);

	public struct Homezone
	{
		public string Name;
		public int X;
		public int Y;
		public int Radius;
	}

	public struct Phonebook
	{
		public int Index;
		public string Name;
		public string Number;
	}

	public struct Messagebook
	{
		public int Index;
		public string Status;
		public string Number;
		public string Content;
	}

	public delegate void IncomingMessageHandler(int index);

}
```