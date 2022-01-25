using Keyfactor.Logging;

using Microsoft.Extensions.Logging;

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.Client
{
	public class Conversions
	{
		private static ILogger Logger = LogHandler.GetClassLogger<Conversions>();

		public static string RevokeReasonToString(UInt32 revokeType)
		{
			switch (revokeType)
			{
				case 1:
					return "Key compromise";

				case 2:
					return "CA compromise";

				case 3:
					return "Affiliation changed";

				case 4:
					return "Superseded";

				case 5:
					return "Cessation of operation";

				case 6:
					return "Certificate hold";

				case 8:
					return "Remove from CRL";

				default:
					return "Unspecified";
			}
		}

		public static byte[] PemToDer(string pem)
		{
			if (pem == null) { return null; }

			string noHeaders = Regex.Replace(pem, @"-----[^-]+-----", "").Trim();
			return Convert.FromBase64String(noHeaders);
		}

		public static int HResultForErrorCode(string errorCode)
		{
			try
			{
				int subCodeInt = int.Parse(errorCode.Replace("0x", ""), NumberStyles.HexNumber);
				return unchecked((int)0xA0030000 | subCodeInt);
			}
			catch (Exception ex)
			{
				Logger.LogWarning($"Unable to convert error code '{errorCode}' to HResult: {LogHandler.FlattenException(ex)}");
				return unchecked((int)0x80004005); // E_FAIL
			}
		}

		//this is off and on errors for microsoft event viewer on box
		public static int EventIDForErrorCode(int EventCat, string errorHexCode)
		{
			try
			{
				errorHexCode = errorHexCode.Replace("0x", "");
				decimal errorDecCode = long.Parse(errorHexCode, System.Globalization.NumberStyles.HexNumber);

				StringBuilder sbNumber = new StringBuilder();
				sbNumber.Append(EventCat).Append(errorDecCode);

				return Convert.ToInt32(sbNumber.ToString());
			}
			catch (Exception ex)
			{
				Logger.LogWarning($"Unable to convert error code '{errorHexCode}' to Event ID: {LogHandler.FlattenException(ex)}");
				return 0;
			}
		}

		public static string ConvertStringtoHex(string convertstring)
		{
			byte[] ba = Encoding.Default.GetBytes(convertstring);
			var hexString = BitConverter.ToString(ba);
			return hexString = hexString.Replace("-", "");
		}
	}
}