# Introduction
This AnyGateway plug-in enables issuance, revocation, and synchronization of certificates from DigiCert's CertCentral offering.  

# Important DigiCert Notes

## Multi-Year Orders
Due to industry changes, certificates can no longer have a total validity of greater than around 13 months.
DigiCert does offer multi-year orders, where you will be issued a certificate that's good for one year, after which you can reissue it for another year.
Currently, the DigiCert CertCentral Gateway does not support new enrollments for multi-year orders. It does, however, support reissuing existing multi-year orders.
If a renewal request is received by the Gateway, and there are 90 days or more remaining on the validity period of the underlying order, the Gateway will convert the renewal request into a reissue request, allowing it to consume the additional order validity without triggering a new purchase.

The validity period of the resulting certificate will be the SHORTER of:
1. The remaining validity period on the underlying order
2. The maximum standard certificate validity (currently defined as 397 days)

## Revocation
By default, revocation through DigiCert revokes the entire order, not just the individual certificate. The RevokeCertificateOnly property (see below) can be used to change that behavior.
However, if setting RevokeCertificateOnly to true, see the [notes from DigiCert here](https://dev.digicert.com/en/certcentral-apis/services-api/certificates/revoke-certificate.html#what-happens-if-i-revoke-a-certificate-on-an-order-with-only-a-single-certificate-) on how that will change the behavior of your account. 


# Prerequisites for Installation

## AnyGateway Platform Minimum Version
The DigiCert CertCentral AnyGateway requires the Keyfactor AnyGateway v21.5.1 or newer

## Migrating to the DigiCert AnyGateway plugin from a previous version of the standalone DigiCert Gateway.
In the installed programs list on the gateway machine, if the install is listed as Keyfactor AnyGateway, you do NOT need to follow these instructions. If the install is listed as Keyfactor Gateway for DigiCert CertCentral, follow these instructions to migrate to Keyfactor AnyGateway.

## Migration From 20.1.x or Earlier
If you are upgrading from an older version of the DigiCert gateway that still used the GUI configuration wizard (20.1.x or earlier), you first have to do an upgrade to DigiCert version 21.x to migrate your database to SQL.
IMPORTANT NOTE: This database migration is REQUIRED if you are using Keyfactor Command as well, otherwise the database IDs will not match up and your gateway will not sync properly.
After doing that upgrade, follow the below steps to migrate from 21.x to the current version.

## Migration from 21.3.2 or Earlier

* Before doing any upgrade, run the following PowerShell command:
    reg export "HKLM\Software\Keyfactor\Keyfactor CA Gateway" C:\DigiCertGatewayBackup.reg
* After backing up the registry key, completely uninstall the old version of the DigiCert CA Gateway
* Follow the instructions to install the AnyGateway product and update the CAProxyServer.config file, but do not do any further configuration yet
* Run the following PowerShell command:
    reg import C:\DigiCertGatewayBackup.reg
* Continue with the gateway configuration as described in the AnyGateway documentation, but do NOT run the Set-KeyfactorGatewayEncryptionCert or the Set-KeyfactorGatewayDatabaseConnection cmdlets, as those values were the ones persisted in the registry backup.

This is a one-time process as the DigiCert gateway moves fully to the Keyfactor AnyGateway model. Future upgrades will not require this process.

## Certificate Chain

In order to enroll for certificates the Keyfactor Command server must trust the trust chain. Once you create your Root and/or Subordinate CA, make sure to import the certificate chain into the AnyGateway and Command Server certificate store


# Install
* Download latest successful build from [GitHub Releases](../../releases/latest)

* Copy DigiCertCAProxy.dll to the Program Files\Keyfactor\Keyfactor AnyGateway directory

* Update the CAProxyServer.config file
  * Update the CAConnection section to point at the DigiCertCAProxy class
  ```xml
  <alias alias="CAConnector" type="Keyfactor.Extensions.AnyGateway.DigiCert.DigiCertCAConnector, DigiCertCAProxy"/>
  ```

# Configuration
The following sections will breakdown the required configurations for the AnyGatewayConfig.json file that will be imported to configure the AnyGateway.

## Templates
The Template section will map the CA's products to an AD template.
* ```ProductID```
This is the ID of the DigiCert product to map to the specified template. If you don't know the available product IDs in your DigiCert account, put a placeholder value here and run the Set-KeyfactorGatewayConfig cmdlet according to the AnyGateway documentation. The list of available product IDs will be returned.
* ```LifetimeDays```
OPTIONAL: The number of days of validity to use when requesting certs. If not provided, default is 365

**NOTE FOR RENEWALS**
If the LifetimeDays value is evenly divisible by 365, when a certificate is renewed, the new certificate's expiration date will be the same month and day as the original certificate (assuming you are renewing close enough to expiration that the new expiration date fits within the maximum validity)
* ```CACertId```
OPTIONAL: If your DigiCert account has multiple issuing CAs, you can specify which one to use by supplying its ID here. If not provided, no CA ID will be passed in to the DigiCert API, and the default for your account will be used.
* ```Organization-Name
OPTIONAL: If you wish to provide your organization name here, rather than in the Subject of the certificate requests (for example, ACME requests that have no subject), you can use this field.

NOTE: If this field is provided, even if the value is empty, it will override subject-supplied organization values. Therefore, delete this field from your config if not using.

 ```json
  "Templates": {
	"WebServer": {
      "ProductID": "ssl_plus",
      "Parameters": {
		"LifetimeDays":"365",
        "CACertId":"123456789ABCDEF",
		"Organization-Name":"Org Name"
      }
   }
}
 ```
 
## Security
The security section does not change specifically for the DigiCert CA Gateway.  Refer to the AnyGateway Documentation for more detail.
```json
  /*Grant permissions on the CA to users or groups in the local domain.
	READ: Enumerate and read contents of certificates.
	ENROLL: Request certificates from the CA.
	OFFICER: Perform certificate functions such as issuance and revocation. This is equivalent to "Issue and Manage" permission on the Microsoft CA.
	ADMINISTRATOR: Configure/reconfigure the gateway.
	Valid permission settings are "Allow", "None", and "Deny".*/
    "Security": {
		"Keyfactor\\Administrator": {
			"READ": "Allow",
			"ENROLL": "Allow",
			"OFFICER": "Allow",
			"ADMINISTRATOR": "Allow"
		},
		"Keyfactor\\gateway_test": {
			"READ": "Allow",
			"ENROLL": "Allow",
			"OFFICER": "Allow",
			"ADMINISTRATOR": "Allow"
		},		
		"Keyfactor\\SVC_TimerService": {
			"READ": "Allow",
			"ENROLL": "Allow",
			"OFFICER": "Allow",
			"ADMINISTRATOR": "None"
		},
		"Keyfactor\\SVC_AppPool": {
			"READ": "Allow",
			"ENROLL": "Allow",
			"OFFICER": "Allow",
			"ADMINISTRATOR": "Allow"
		}
    }
```
## CerificateManagers
The Certificate Managers section is optional.
	If configured, all users or groups granted OFFICER permissions under the Security section
	must be configured for at least one Template and one Requester. 
	Uses "<All>" to specify all templates. Uses "Everyone" to specify all requesters.
	Valid permission values are "Allow" and "Deny".
```json
	"CertificateManagers":{
		"DOMAIN\\Username":{
			"Templates":{
				"MyTemplateShortName":{
					"Requesters":{
						"Everyone":"Allow",
						"DOMAIN\\Groupname":"Deny"
					}
				},
				"<All>":{
					"Requesters":{
						"Everyone":"Allow"
					}
				}
			}
		}
	}
```
## CAConnection
The CA Connection section will determine the API endpoint and configuration data used to connect to DigiCert CA Gateway. 
* ```APIKey```
This is the API key to use to connect to the DigiCert API.  
* ```Region```
OPTIONAL: Specify the geographic region of your DigiCert account. Valid options are US and EU. If not provided, defaults to US.
* ```DivisionId```
OPTIONAL: If your CertCentral account has multiple divisions AND uses any custom per-division product settings, provide a DivisionId for the gateway to use for enrollment calls. Otherwise, omit this configuration field.
NOTE: The division ID is currently only used for product lookups, this will not impact any other gateway functionality currently.
* ```RevokeCertificateOnly```
OPTIONAL: By default, when you revoke a certificate through DigiCert, it revokes it by order number, so orders with multiple certificates all get revoked. If you wish to only revoke single certificates, set this property to true.
* ```SyncCAFilter```
OPTIONAL: If you list one or more CA IDs here, the sync process will only sync records from those CAs. If you want to sync all CA IDs, do not include this config option.
* ```FilterExpiredOrders```
OPTIONAL: If set to 'true', syncing will apply a filter to only return orders whose expiration date is later than the current day. Default if not specified is 'false'
* ```SyncExpirationDays```
OPTIONAL: If FilterExpiredOrders is set to true and SyncExpirationDays is provided, it specifies the number of days in the past to sync expired certs. Example: a value of 30 means all certs that expired within the past 30 days will still be sync. If FilterExpiredOrders is true and this is not provided, the default will filter out certs that expired before the current day. This value is ignored if FilterExpiredOrders is false or not provided.
* ```PaymentMethod```
OPTIONAL: Specify which payment method to use when enrolling for certs.  Valid options are "balance" and "card". Balance uses account balance on your DigiCert account, whereas Card tells DigiCert to use the account's default credit card (which you must set up in your DigiCert account)  
If this field is not specified, the DigiCert API default will be used, which is "balance"

```json
	"CAConnection": {
		"APIKey" : "DigiCert API Key",
		"Region" : "US",
		"DivisionId": "12345",
		"RevokeCertificateOnly": false,
		"SyncCAFilter": ["ABC12345", "DEF67890"],
		"FilterExpiredorders": false,
		"SyncExpirationDays": 30
		"PaymentMethod": "balance"
	},
```
## GatewayRegistration
There are no specific Changes for the GatewayRegistration section. Refer to the AnyGateway Documentation for more detail.
```json
	"GatewayRegistration": {
		"LogicalName": "DigiCertCASandbox",
		"GatewayCertificate": {
			"StoreName": "CA",
			"StoreLocation": "LocalMachine",
			"Thumbprint": "0123456789abcdef"
		}
	}
```

## ServiceSettings
There are no specific Changes for the ServiceSettings section. Refer to the AnyGateway Documentation for more detail.
```json
	"ServiceSettings": {
		"ViewIdleMinutes": 8,
		"FullScanPeriodHours": 24,
		"PartialScanPeriodMinutes": 240 
	}
```
