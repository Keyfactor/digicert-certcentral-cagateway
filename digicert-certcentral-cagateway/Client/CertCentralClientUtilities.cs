using CAProxy.AnyGateway.Interfaces;

using Keyfactor.Logging;

using Microsoft.Extensions.Logging;

using System;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.Client
{
	/// <summary>
	/// Static class containing some utility methods for the cert central client.
	/// </summary>
	public static class CertCentralClientUtilities
	{
		/// <summary>
		/// Private instance of the logger.
		/// </summary>
		private static ILogger Logger => LogHandler.GetClassLogger<Conversions>();

		/// <summary>
		/// Uses the <see cref="ICAConnectorConfigProvider"/> to build a DigiCert client.
		/// </summary>
		/// <param name="ConfigProvider"></param>
		/// <returns></returns>
		public static CertCentralClient BuildCertCentralClient(ICAConnectorConfigProvider ConfigProvider)
		{
			Logger.LogTrace("Entered BuildCertCentralClient");
			try
			{
				Logger.LogTrace("Building CertCentralClient with retrieved configuration information");
				string apiKey = (string)ConfigProvider.CAConnectionData["APIKey"];
				return new CertCentralClient(apiKey);
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to build CertCentralClient Client web service client", ex);
			}
		}
	}
}