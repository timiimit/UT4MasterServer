:: this is thumbprint of SSLCertificate
set THUMBPRINT=8cae560af44ee1879c545536ca221cfe963f2cb6

:: this is guid located in source code in file AssemblyInfo.cs
set APP_GUID=2a3a3f35-1640-4f7b-b076-60dcacb2014e

netsh http add sslcert ipport=0.0.0.0:443 certhash=%THUMBPRINT% appid={%APP_GUID%}