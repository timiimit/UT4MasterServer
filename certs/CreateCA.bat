set NAME=UT4MasterCA
set PFX_PASSWORD=pass

makecert.exe -n "CN=%NAME%" -r -pe -a sha512 -len 4096 -cy authority -sv %NAME%.pvk %NAME%.cer
pvk2pfx.exe -pvk %NAME%.pvk -spc %NAME%.cer -pfx %NAME%.pfx -po %PFX_PASSWORD%