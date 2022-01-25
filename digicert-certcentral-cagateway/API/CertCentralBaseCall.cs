using System.Collections.Generic;

using Newtonsoft.Json;

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