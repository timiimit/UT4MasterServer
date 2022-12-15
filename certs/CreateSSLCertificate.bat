set CA_NAME=UT4MasterCA
set SSL_NAME=UT4MasterServerAccounts
set PFX_PASSWORD=pass

makecert.exe -n "CN=account-public-service-prod03.ol.epicgames.com" -iv %CA_NAME%.pvk -ic %CA_NAME%.cer -pe -a sha512 -len 4096 -sky exchange -eku 1.3.6.1.5.5.7.3.1 -sv %SSL_NAME%.pvk %SSL_NAME%.cer
pvk2pfx.exe -pvk %SSL_NAME%.pvk -spc %SSL_NAME%.cer -pfx %SSL_NAME%.pfx -po %PFX_PASSWORD%