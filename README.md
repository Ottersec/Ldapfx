# Ldapfx

Small tool created due to some edge cases when trying to exploit the relay using PetitPotam to AD CS. 

When you successfuly retrieve a .pfx certificate if the **SAN within the X509** differs from the **dnshostname** attribute of your target you won't be able to retrieve a TGT and have the following error :
```
KDC_ERR_CLIENT_NAME_MISMATCH (ERR 75)
```
Here is an example of this edge case :
- FQDN according to the domain : **wks2.test.local**
- FQDN which is stored within the dnshostname attribute: **wks2.modifyme.test.local**

This is **NOT related to a child domain** but just a custom **dnshostname** value. It has to be noted that a **computer account can modify the dnshostname** attribute as long as the modification stick to the following value :
- \<computer name>.\<domain name>

Once you've modified the **dnshostname** attribute you'll have to request a **new certificate** and only then you'll be able to ask for a TGT.
# Usage 
```
LDAPfx.exe DC1:636 wks2_cert.pfx "CN=WKS2,CN=Computers,DC=test,DC=local" "wks2.test.local"
[*] Adding certificate to the LDAPConnection
[*] Requesting LDAPS using the certificate
[*] Current LDAP user : u:TEST\Administrator
[*] Default naming context : DC=test,DC=local
[*] Current dnshostname attribute : wks2.modifyme.test.local
[*] Did modifying the attribute succeed ? : Success
[*] Current dnshostname attribute : wks2.test.local
```

Author : @Ottersec & @dav1dstr33t
