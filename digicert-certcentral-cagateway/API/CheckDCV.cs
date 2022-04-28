// Copyright 2022 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Newtonsoft.Json;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class DVCheckDCVRequest : CertCentralBaseRequest
	{
		public DVCheckDCVRequest(int orderID)
		{
			this.OrderID = orderID;
			this.Resource = "services/v2/order/certificate/" + orderID.ToString() + "/check-dcv";
			this.Method = "PUT";
		}

		[JsonIgnore]
		public int OrderID { get; set; }
	}

	public class DVCheckDCVResponse : CertCentralBaseResponse
	{
		[JsonProperty("dcv_status")]
		public string dcv_status { get; set; }

		[JsonProperty("order_status")]
		public string order_status { get; set; }
	}
}