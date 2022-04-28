// Copyright 2022 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class DownloadCertificateByFormatRequest : CertCentralBaseRequest
	{
		public DownloadCertificateByFormatRequest(int Certificate_id)
		{
			this.format_type = "pem_nointermediate";
			this.certificate_id = Certificate_id;
			this.Resource = "services/v2/certificate/" + this.certificate_id.ToString() + "/download/format/" + format_type;
			this.Method = "GET";
		}

		public DownloadCertificateByFormatRequest(int Certificate_id, string Format_type)
		{
			this.format_type = Format_type;
			this.certificate_id = Certificate_id;
			this.Resource = "services/v2/certificate/" + this.certificate_id.ToString() + "/download/format/" + format_type;
			this.Method = "GET";
		}

		public int certificate_id { get; set; }

		public string format_type { get; set; }
	}

	public class DownloadCertificateByFormatResponse : CertCentralBaseResponse
	{
		public string certificate { get; set; }
	}
}