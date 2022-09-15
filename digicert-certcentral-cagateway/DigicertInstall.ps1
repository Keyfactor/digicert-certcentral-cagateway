####Fill out the following config sections

#Only change this if running the powershell script from a directory different than the one the Digicert DLL was extracted into
$digicertPath = $PSScriptRoot

##Database connection
#FQDN of SQL Server
$sqlServerHostname = "<sql server hostname>"
#Name of database to create/populate
$sqlDatabaseName = "<database name>"
$createNewDatabase = $true
#$false - integrated auth, $true - sql auth
$useSqlAuth = $false
#Username/password for sql auth only
$authUser = "domain\username"
$authPassword = "userpassword"

#DigiCert account information
$apiKey = "<api key>"
#OPTIONAL: Leave blank "" if not using divisions
$divisionId = "<division ID>"

#Configuration string information for gateway
$hostName = "<gateway hostname>"
$logicalName = "<logical name>"

#Publish gateway registration to Active Directory
$publishToAD = $true

#Thumbprint of installed intermediate cert
#This cert should already be installed in the Intermediate Certificate Authorities cert store on the Local Machine
$intermediateCertThumb = "<intermediate cert>"

#If $true, script will offer prompts to fill out the security/template config information, write the completed config out to a JSON file, and call the Set-KeyfactorGatewayConfig cmdlet
#If $false, script will call Get-KeyfactorGatewayConfig cmdlet to provide a blank config template, and pause execution to allow you to manually edit the file, before resuming and calling Set-KeyfactorGatewayConfig
$promptForConfig = $true

###Do not modify anything below this line

function Format-Json {
    Param(
        [Parameter(Mandatory=$true, Position=0, ValueFromPipeline=$true)]
        [string]$Json
    )
    if ($Json -notmatch '\r?\n') {
        $Json = ($Json | ConvertFrom-Json) | ConvertTo-Json -Depth 100
    }

    $indent = 0
    $regex = '(?=([^"]*"[^"]*")*[^"]*$)'

    $result = $Json -split '\r?\n' |
        ForEach-Object {
            if ($_ -match "[}\]]$regex") {
                $indent = [Math]::Max($indent - 4, 0)
            }

            $line = (' ' * $indent) + ($_.TrimStart() -replace ":\s+$regex", ': ')

            if ($_ -match "[\{\[]$regex") {
                $indent += 4
            }

            $line
        }

    return $result -join [Environment]::NewLine
}

function Setup-Service {
    $configFile = $anyGatewayPath + 'CAProxyServer.exe.config'
    ((Get-Content -Path $configFile -Raw) -replace 'CAProxy.AnyGateway.NoOpCAConnector, CAProxy.AnyGateway.Core', 'Keyfactor.Extensions.AnyGateway.DigiCert.DigiCertCAConnector, DigiCertCAProxy') | Set-Content -Path $configFile
}

function Setup-Database {

    if ( $createNewDatabase ) {
        $dbVerb = 'create'
    }
    else {
        $dbVerb = 'populate'
    }
    Write-Verbose ('Performing action ' + $dbVerb + ' on database.')
    if ( $useSQLAuth ) {
        .\DatabaseManagementConsole.exe $dbVerb -s $sqlServerHostname -d $sqlDatabaseName -u $authUser -p $authPassword
    }
    else {
        .\DatabaseManagementConsole.exe $dbVerb -s $sqlServerHostname -d $sqlDatabaseName
    }
    Write-Verbose ('Database ' + $dbVerb + 'd')
    Write-Verbose 'Creating encryption certificate...'
    Set-KeyfactorGatewayEncryptionCert
    Write-Verbose 'Encryption cert created'
    Write-Verbose 'Setting database connection string...'
    if ( $useSQLAuth ) {
        $encryptedPW = ConvertTo-SecureString $authPassword -AsPlainText -Force
        $credential = New-Object System.Management.Automation.PSCredential($authUser, $encryptedPW)
        Set-KeyfactorGatewayDatabaseConnection -Server $sqlServerHostname -Database $sqlDatabaseName -Account $credential
    }
    else {
        Set-KeyfactorGatewayDatabaseConnection -Server $sqlServerHostname -Database $sqlDatabaseName
    }
    Write-Verbose 'Database connection string set'
}

function Build-Config {
    #Build Security Config
    $securityName = Read-Host -Prompt 'Enter the first security principal (use double \\ where needed)'
    $securityList = @()
    do {
        $readAccess = Read-Host -Prompt 'Read? (y/n)'
        $enrollAccess = Read-Host -Prompt 'Enroll? (y/n)'
        $officerAccess = Read-Host -Prompt 'Officer? (y/n)'
        $adminAccess = Read-Host -Prompt 'Admin? (y/n)'
        if ($readAccess -eq 'y') {
            $readStr = "Allow"
        }
        else {
            $readStr = "Deny"
        }
        if ($enrollAccess -eq 'y') {
            $enrollStr = "Allow"
        }
        else {
            $enrollStr = "Deny"
        }
        if ($officerAccess -eq 'y') {
            $officerStr = "Allow"
        }
        else {
            $officerStr = "Deny"
        }
        if ($adminAccess -eq 'y') {
            $adminStr = "Allow"
        }
        else {
            $adminStr = "Deny"
        }

$securityEntry = @"
"${securityName}": {
"READ": "${readStr}",
"ENROLL": "${enrollStr}",
"OFFICER": "${officerStr}",
"ADMINISTRATOR": "${adminStr}"
},
"@
        $securityList += $securityEntry

        $securityName = Read-Host -Prompt 'Enter the next security principal (or Enter when done)'
    } while (-not ([string]::IsNullOrEmpty($securityName)))

$securityConfig = @"
"Security": {
${securityList}
},
"@


    #Build CAConnection Config
$caConnectionConfig = @"
"CAConnection": {
"APIKey": "${apiKey}"
},
"@
    if ($divisionId) {
$caConnectionConfig = @"
"CAConnection": {
"APIKey": "${apiKey}",
"DivisionId": "${divisionId}"
},
"@
    }

    #Build Templates Config
    $templateName = Read-Host -Prompt 'Enter the first template name'
    $templateList = @()
    do {
        $productId = Read-Host -Prompt 'Enter the DigiCert product ID'
        $lifetimeDays = Read-Host -Prompt 'Enter the certificate lifetime (in days) to use for enrollment'

$templateEntry = @"
"${templateName}": {
"ProductID": "${productId}",
"Parameters": {
"LifeTimeDays": "${lifetimeDays}"
}
},
"@
        $templateList += $templateEntry

        $templateName = Read-Host -Prompt 'Enter the next template name (or Enter when done)'
    } while (-not ([string]::IsNullOrEmpty($templateName)))

$templateConfig = @"
"Templates": {
${templateList}
},
"@

    #Build CertificateManagers Config
$certManagerConfig = @"
"CertificateManagers": null,
"@

$gatewayRegistrationConfig = @"
"GatewayRegistration": {
"LogicalName": "${logicalName}",
"GatewayCertificate": {
"StoreName": "CA",
"StoreLocation": "LocalMachine",
"Thumbprint": "${intermediateCertThumb}"
}
},
"@

    #Build ServiceSettings Config
$serviceSettingsConfig = @"
"ServiceSettings": {
"ViewIdleMinutes": 8,
"FullScanPeriodHours": 1,
"PartialScanPeriodMinutes": 10
}
"@

$config = @"
{
$securityConfig
$caConnectionConfig
$templateConfig
$certManagerConfig
$gatewayRegistrationConfig
$serviceSettingsConfig
}
"@
    $gatewayConfigFile = $anyGatewayPath + 'DigiCertConfig.json'
    
    $config | ConvertTo-Json | ConvertFrom-Json | Format-Json | Out-File $gatewayConfigFile

    if ($publishToAD) {
        Set-KeyfactorGatewayConfig -CAHostname $hostName -LogicalName $logicalName -FilePath $gatewayConfigFile -PublishAD
    }
    else {
        Set-KeyfactorGatewayConfig -CAHostname $hostName -LogicalName $logicalName -FilePath $gatewayConfigFile
    }

    Write-Verbose "Gateway successfully configured. Config file saved to $gatewayConfigFile"
}

$ErrorActionPreference = "Stop"
$anyGatewayExePath = (Get-WmiObject win32_service | ?{$_.Name -like '*certsvcproxy*' } | select PathName).PathName
$anyGatewayPath = $anyGatewayExePath.Substring(1, $anyGatewayExePath.Length - 19)
Write-Verbose 'AnyGateway Installation Found'


Write-Verbose 'Stopping Service...'
Stop-Service -Name CertSvcProxy
(Get-Service -Name CertSvcProxy).WaitForStatus('Stopped')
Write-Verbose 'Service Stopped.'
#Path to extracted DigiCert plugin
$sourcePath = $digicertPath + 'DigiCertCAProxy.dll'
Write-Verbose 'Copying Digicert DLLs...'
Copy-Item $sourcePath -Destination $anyGatewayPath
Write-Verbose 'DigiCert DLL Copied Successfully'
Write-Verbose 'Modifying AnyGateway Service Config...'
Setup-Service
Write-Verbose 'Service Config Modified'
Write-Verbose 'Starting Service...'
Start-Service -Name CertSvcProxy
(Get-Service -Name CertSvcProxy).WaitForStatus('Running')
Write-Verbose 'Service Started.'
Set-Location $anyGatewayPath
Import-Module .\ConfigurationCmdlets.dll
Write-Verbose 'Configuring Database...'
Setup-Database
Write-Verbose 'Database configured successfully.'
Start-Sleep -Seconds 5
if ($promptForConfig) {
    Build-Config
}
else {
    $gatewayConfigFile = $anyGatewayPath + 'DigiCertConfig.json'
    Get-KeyfactorGatewayConfig -CAHostname $hostName -FilePath $gatewayConfigFile

    $apiKeyReplace = '"APIKey":"' + $apiKey + '",'
    $divisionReplace = '"DivisionId": "' + $divisionId + '"'
    $logicalNameReplace = '"LogicalName": "' + $logicalName + '",'
    $thumbReplace = '"Thumbprint": "' + $intermediateCertThumb + '"'

    ((Get-Content -Path $gatewayConfigFile -Raw) -replace '"APIKey":"",', $apiKeyReplace) | Set-Content -Path $gatewayConfigFile
    ((Get-Content -Path $gatewayConfigFile -Raw) -replace '"DivisionId": ""', $divisionReplace) | Set-Content -Path $gatewayConfigFile
    ((Get-Content -Path $gatewayConfigFile -Raw) -replace '"LogicalName": "",', $logicalNameReplace) | Set-Content -Path $gatewayConfigFile
    ((Get-Content -Path $gatewayConfigFile -Raw) -replace '"Thumbprint": "1234567890123456789123"', $thumbReplace) | Set-Content -Path $gatewayConfigFile

    Read-Host 'Edit config at ' $gatewayConfigFile ' and then then press Enter to continue. Make sure to uncomment the APIKey line.'
    if ($publishToAD) {
        Set-KeyfactorGatewayConfig -CAHostname $hostName -LogicalName $logicalName -FilePath $gatewayConfigFile -PublishAD
    }
    else {
        Set-KeyfactorGatewayConfig -CAHostname $hostName -LogicalName $logicalName -FilePath $gatewayConfigFile
    }
    Write-Verbose 'Digicert Gateway configuration complete'
}

