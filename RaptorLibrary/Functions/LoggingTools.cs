using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;

namespace RaptorLibrary
{
	public static class LoggingTools
	{
		/// <summary>
		/// Provides a notification on screen, doesn't log to disk. Useful for in-game messages regarding function testing, without having to alt-tab to see progress.
		/// If the text is defined as an error, it will tell the player the class and method provided.
		/// </summary>
		/// <param name="modName"></param>
		/// <param name="pageName"></param>
		/// <param name="methodName"></param>
		/// <param name="modMessage"></param>
		/// <param name="isError"></param>
		public static void TextPlayerMessage(string modName, string pageName, string methodName, string modMessage, bool isError = false, string textSender = "RaptorLibrary")
		{
			string subject = modName + " " + (isError ? "Error" : "Message");
			string message = modMessage;
			if (isError) message = $"Error in function: {pageName}.{methodName}\n{modMessage}";
			GTA.UI.Notification.Show(GTA.UI.NotificationIcon.Bugstars, textSender, subject, message, blinking: true);
		}
		/// <summary>
		/// Logs the error to the disk, also sends an informative message to the screen or (Pause -> Brief -> Notifications) if it's not a game-breaking exception.
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="modName"></param>
		/// <param name="pageName"></param>
		/// <param name="methodName"></param>
		/// <param name="friendlyError"></param>
		public static void RecordError(Exception ex, string modName, string pageName, string methodName, string friendlyError)
		{
			LogError(1, modName, pageName, methodName, friendlyError, ex.Message);
			string bugstarsMessage = $"There was an error in the {modName} mod, {pageName}.{methodName}(), \"{friendlyError}\".";
			TextPlayerMessage(modName, pageName, methodName, bugstarsMessage, true);
		}
		/// <summary>
		/// Safely logs an error to the disk, logging a LoggingTools error if it fails to log the input error.
		/// </summary>
		/// <param name="category">Success = 0, Error = 1, UNKNOWN = 2</param>
		/// <param name="modName"></param>
		/// <param name="pageName"></param>
		/// <param name="pageMethod"></param>
		/// <param name="technicalError"></param>
		public static void LogError(int category, string modName, string pageName, string pageMethod, string friendlyError, string technicalError)
		{
			string fncName = "LogError";
			try
			{
				string errorType;
				switch (category)
				{
					case (int)Enums.ErrorType.Success: errorType = "Success"; break;
					case (int)Enums.ErrorType.Error: errorType = "Error"; break;
					default:
					case (int)Enums.ErrorType.UNKNOWN: errorType = "Unknown"; break;
				}
				string message = $"Page: {pageName}\tMethod: {pageMethod}\tError: {technicalError}\t\t({errorType.ToUpper()})\t\"{friendlyError}\"";
				WriteMessageToDisk(modName, message);
			}
			catch (Exception ex)
			{
				// In an exception here we'll assume it's pretty drastic and the game might crash so just attempt write it to the disk.
				WriteLogToDisk(true, ex.Message, "RaptorLibrary", "LoggingTools", fncName, UtilityFunctions.GetExceptionLineNo(ex), $"There was an error logging the error from mod \"{modName}\" on page \"{pageName}\" in method \"{pageMethod}\".", ex.Message);
			}
		}
		/// <summary>
		/// Logs a message to the disk, simply with the date/time of the message log, and a message. Useful for logging non-error messages.
		/// </summary>
		/// <param name="modName"></param>
		/// <param name="message"></param>
		public static void WriteMessageToDisk(string modName, string message)
		{
			try
			{
				char[] forbiddenChars = new char[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };
				foreach (var character in forbiddenChars)
				{
					modName = modName.Replace(character, '_');
				}

				DateTime now = DateTime.Now;
				string year = now.Year.ToString();
				string month = now.Month.ToString().PadLeft(2, '0');
				string day = now.Day.ToString().PadLeft(2, '0');
				string fileName = $"{year}-{month}-{day}.log";
				string filePath = Environment.CurrentDirectory + "/scripts/RaptorLibraryLogs";

				if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
				if (!Directory.Exists($"{filePath}\\{modName}")) Directory.CreateDirectory($"{filePath}\\{modName}");

				using (StreamWriter file = File.AppendText($"{filePath}\\{modName}\\{fileName}"))
				{
					file.WriteLine($"{now.ToLongTimeString()}.{now.Millisecond}\t\t{message}");
				}
			}
			catch (IOException ex)
			{
				// multithreaded calls can lock this up, so just wait until the file isn't in use and try again.
				Script.Wait(100);
				WriteMessageToDisk(modName, message);
			}
		}
		/// <summary>
		/// More in depth logger than <see cref="LogError(int, string, string, string, string)">LogError()</see>. Structured log showing where the message came from.
		/// </summary>
		/// <param name="ModName"></param>
		/// <param name="PageName"></param>
		/// <param name="PageMethod"></param>
		/// <param name="UserFriendlyMessage"></param>
		public static void WriteLogToDisk(string ModName, string PageName, string PageMethod, string UserFriendlyMessage)
		{
			WriteLogToDisk(false, string.Empty, ModName, PageName, PageMethod, string.Empty, UserFriendlyMessage, string.Empty);
		}
		/// <summary>
		/// More in depth logger than <see cref="LogError(int, string, string, string, string)">LogError()</see>. Structured log showing where the message came from, including exception messages.
		/// </summary>
		/// <param name="isError"></param>
		/// <param name="exceptionMessage"></param>
		/// <param name="ModName"></param>
		/// <param name="PageName"></param>
		/// <param name="PageMethod"></param>
		/// <param name="ErrorLineNumber"></param>
		/// <param name="UserFriendlyMessage"></param>
		/// <param name="ErrorMessage"></param>
		public static void WriteLogToDisk(bool isError, string exceptionMessage, string ModName, string PageName, string PageMethod, string ErrorLineNumber, string UserFriendlyMessage, string ErrorMessage)
		{
			try
			{
				string MessageType = isError ? "  ERROR" : "Message";
				string pageName = string.IsNullOrEmpty(PageName) ? "" : "\tPage: " + PageName;
				string pageMethod = string.IsNullOrEmpty(PageMethod) ? "" : "\tMethod: " + PageMethod;
				string errorLineNumber = string.IsNullOrEmpty(ErrorLineNumber) ? "" : "\tError Line: " + ErrorLineNumber;
				string userFriendlyMessage = string.IsNullOrEmpty(UserFriendlyMessage) ? "" : "\t\"" + UserFriendlyMessage + "\"";
				string errorMessage = string.IsNullOrEmpty(ErrorMessage) ? "" : "\tError Message: \"" + ErrorMessage + "\"";

				string logMessage = pageName + pageMethod + errorLineNumber + userFriendlyMessage + errorMessage;

				WriteMessageToDisk(ModName, MessageType + " : " + exceptionMessage + logMessage);
			}
			catch (Exception)
			{
				// Well we're buggered here, but at least we might survive the catch.
			}
		}
	}
}
