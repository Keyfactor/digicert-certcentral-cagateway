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
	public abstract class CertCentralBaseResponse
	{
		public enum StatusType
		{
			SUCCESS,
			ERROR,
			WARNING
		}

		public enum ContentTypes
		{
			XML,
			JSON,
			TEXT
		}

		public CertCentralBaseResponse()
		{
			this.Errors = new List<Error>();
			this.Status = StatusType.SUCCESS;
			this.ContentType = ContentTypes.JSON;
		}

		[JsonIgnore]
		public ContentTypes ContentType { get; internal set; }

		[JsonIgnore]
		public StatusType Status { get; set; }

		[JsonIgnore]
		public List<Error> Errors { get; set; }
	}

	public abstract class CertCentralBaseRequest
	{
		[JsonIgnore]
		public string Resource { get; internal set; }

		[JsonIgnore]
		public string Method { get; internal set; }

		[JsonIgnore]
		public string targetURI { get; set; }

		public string BuildParameters()
		{
			return "";
		}
	}
}