# DigiCert CertCentral CA AnyGateway

This integration allows for the Synchronization, Enrollment, and Revocation of certificates from DigiCert CertCentral.

#### Integration status: Production - Ready for use in production environments.

## About the Keyfactor AnyGateway CA Connector

This repository contains an AnyGateway CA Connector, which is a plugin to the Keyfactor AnyGateway. AnyGateway CA Connectors allow Keyfactor Command to be used for inventory, issuance, and revocation of certificates from a third-party certificate authority.

---





---

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

 ```json
  "Templates": {
	"WebServer": {
      "ProductID": "ssl_plus",
      "Parameters": {
		"LifetimeDays":"365",
        "CACertId":"123456789ABCDEF"
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
* ```DivisionId```
OPTIONAL: If your CertCentral account has multiple divisions AND uses any custom per-division product settings, provide a DivisionId for the gateway to use for enrollment calls. Otherwise, omit this configuration field.
NOTE: The division ID is currently only used for product lookups, this will not impact any other gateway functionality currently.

```json
  "CAConnection": {
	"APIKey" : "DigiCert API Key",
    "DivisionId": "12345"
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

