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