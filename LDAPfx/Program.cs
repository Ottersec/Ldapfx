using System;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.DirectoryServices.Protocols;
using System.IO;
namespace LDAPfx
{
    class LDAPfx
    {
        public LdapConnection ldapConnection = null;
        public X509Certificate2 userCertificate = null;
        public string computerFQDN = "";

        public bool getLdapBaseDN()
        {
            SearchRequest searchRequest = new SearchRequest(
            "",
            "objectClass=*",
            SearchScope.Base,
            "defaultNamingContext"
            );
            var searchResponse = (SearchResponse)this.ldapConnection.SendRequest(searchRequest);
            string defaultNamingContext = "";
            foreach (SearchResultEntry entry in searchResponse.Entries)
            {
                defaultNamingContext = entry.Attributes["defaultNamingContext"][0].ToString();
                if(defaultNamingContext != "")
                {
                    Console.WriteLine("[*] Default naming context : " + defaultNamingContext);
                    return true;

                }
                else
                {
                    Console.WriteLine("[*] Error getting default naming context");
                    return false;
                }
            }
            // Not very clean but in any case if we iterate to this point we've failed
            return false;
        }

        public bool getLdapUserName()
        {
            ExtendedRequest getCurrentUserOID = new ExtendedRequest("1.3.6.1.4.1.4203.1.11.3");
            Console.WriteLine("[*] Requesting LDAPS using the certificate");
            try
            {
                var responseExtended = (ExtendedResponse)this.ldapConnection.SendRequest(getCurrentUserOID);
                Console.WriteLine("[*] Current LDAP user : " + Encoding.UTF8.GetString(responseExtended.ResponseValue));
                return true;
            }
            catch
            {
                Console.WriteLine("[*] Error getting current LDAP user");
                return false;
            }
            
        }

        public void modifyDnsHostname(string newDnsHostnameValue)
        {
            ModifyRequest modifyRequest = new ModifyRequest(this.computerFQDN, DirectoryAttributeOperation.Replace, "dnshostname", newDnsHostnameValue);
            ModifyResponse modResponse = (ModifyResponse)this.ldapConnection.SendRequest(modifyRequest);
            Console.WriteLine("[*] Did modifying the attribute succeed ? : " + modResponse.ResultCode);
            return;

        }
        public bool getDnsHostname()
        {
            SearchRequest searchRequest2 = new SearchRequest(this.computerFQDN, "(objectcategory=*)", SearchScope.Base);
            var dnsHostnameValue = "";
            var searchResponse = (SearchResponse)this.ldapConnection.SendRequest(searchRequest2);
            foreach (SearchResultEntry entry in searchResponse.Entries)
            {
                dnsHostnameValue = entry.Attributes["dnshostname"][0].ToString();
                if(dnsHostnameValue != "")
                {
                    Console.WriteLine("[*] Current dnshostname attribute : " + dnsHostnameValue);
                    return true;
                }
                else
                {
                    Console.WriteLine("[*] Error getting current dnshostname attribute");
                    return false;
                }   
            }
            // Not very clean but in any case if we iterate to this point we've failed
            return false;

        }
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("[*] Not enough args");
                Console.WriteLine("[*] Ex : ldapfx.exe <COMPUTERNAME:636> <full path to .pfx> <Object FQDN> <New dnshostname>");
                return ;
            }
                
            LDAPfx ldapfr = new LDAPfx();
            ldapfr.ldapConnection = new LdapConnection(args[0]);
            Console.WriteLine("[*] Adding certificate to the LDAPConnection");
            ldapfr.userCertificate = new X509Certificate2(File.ReadAllBytes(args[1]));
            ldapfr.computerFQDN = args[2];
            ldapfr.getLdapUserName();
            ldapfr.getLdapBaseDN();
            ldapfr.getDnsHostname();
            ldapfr.modifyDnsHostname(args[3]);
            ldapfr.getDnsHostname();


        }

    }
}
