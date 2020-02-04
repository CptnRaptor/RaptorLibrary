using System;

using GTA;
using GTA.Math;

namespace RaptorLibrary
{
	public static class UtilityFunctions
	{
        /// <summary>
        /// Finds the exception line number of an input exception, returning the line number as a string or "UNKNOWN".
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
		public static string GetExceptionLineNo(Exception exception)
		{
            string[] lineParts = exception.StackTrace.Split(new char[] { '\\' });
            string exLineNumber = lineParts[lineParts.Length - 1];
            if (exLineNumber.Contains("line"))
            {
                return exLineNumber.Split(new char[] { ' ' })[1];
            }
            else
            {
                return "UNKNOWN ";
            }
        }
	}
}
