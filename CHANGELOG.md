1.0.0
Inital Release.  Support for Enroll, Sync, and Revocation. 

1.0.2
Added ability to provide a division ID for product type lookups, when the DigiCert account has different product settings per division.

1.1.0
Added support for CSRs with empty subjects, first DNS SAN will be used as the Common Name instead

1.1.2
Added support for specifying the issuing CA cert ID as part of the product/template definition

1.2.0
Added support for renewing multi-year orders.

1.2.1
Fixed an issue where OU was not properly being passed in to the DigiCert API

1.2.2
Fixed an issue where renewals were not properly getting tagged with the previous order ID in DigiCert

1.3.0
Added a config option to allow for revoking a single cert, instead of the entire order

1.3.2
Fixed an issue where status of some certs was being returned as Unknown