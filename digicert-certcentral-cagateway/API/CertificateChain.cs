// Copyright 2022 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Newtonsoft.Json;

using System.Collections.Generic;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class CertificateChainRequest : CertCentralBaseRequest
	{
		public CertificateChainRequest(string certificate_id)
		{
			this.Resource = $"services/v2/certificate/{certificate_id}/chain";
			this.Method = "GET";
		}
	}

	public class CertificateChainResponse : CertCentralBaseResponse
	{
		[JsonProperty("Intermediates")]
		public List<CertificateChainElement> Intermediates { get; set; }
	}
}