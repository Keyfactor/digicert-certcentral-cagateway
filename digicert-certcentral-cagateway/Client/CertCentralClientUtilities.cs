// Copyright 2022 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

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
		private static ILogger Logger => LogHandler.GetClassLogger<DigiCertCAConnector>();

		/// <summary>
		/// Uses the <see cref="DigiCertCAConfig"/> to build a DigiCert client.
		/// </summary>
		/// <param name="Config"></param>
		/// <returns></returns>
		public static CertCentralClient BuildCertCentralClient(DigiCertCAConfig Config)
		{
			Logger.LogTrace("Entered BuildCertCentralClient");
			try
			{
				Logger.LogTrace("Building CertCentralClient with retrieved configuration information");
				string apiKey = Config.APIKey;
				string region = Config.Region;
				return new CertCentralClient(apiKey, region);
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to build CertCentralClient Client web service client", ex);
			}
		}
	}
}