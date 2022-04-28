// Copyright 2022 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Newtonsoft.Json;

using System.ComponentModel;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class User : Contact
	{
		[JsonProperty("id")]
		public int id { get; set; }
	}

	public class Contact
	{
		[JsonProperty("first_name")]
		public string first_name { get; set; }

		[JsonProperty("last_name")]
		public string last_name { get; set; }

		[JsonProperty("email")]
		public string email { get; set; }

		[DefaultValue(null)]
		[JsonProperty("telephone")]
		public string telephone { get; set; }
	}
}