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
	public class StatusChangesRequest : CertCentralBaseRequest
	{
		public StatusChangesRequest(string lastSync, string todayUTC)
		{
			this.Resource = $"services/v2/order/certificate/status-changes?filters[status_last_updated]={lastSync}...{todayUTC}";
			this.Method = "GET";
		}
	}

	public class StatusOrder
	{
		[JsonProperty("order_id")]
		public int order_id { get; set; }

		[JsonProperty("certificate_id")]
		public int certificate_id { get; set; }

		[JsonProperty("status")]
		public string status { get; set; }
	}

	public class StatusChangesResponse : CertCentralBaseResponse
	{
		[JsonProperty("orders")]
		public List<StatusOrder> orders { get; set; }
	}
}