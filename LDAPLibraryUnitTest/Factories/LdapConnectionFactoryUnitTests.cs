using System.DirectoryServices.Protocols;
using System.Net;
using LDAPLibrary;
using LDAPLibrary.Connectors;
using LDAPLibrary.Enums;
using LDAPLibrary.Factories;
using LDAPLibrary.Interfarces;
using LDAPLibrary.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LDAP_Library_UnitTest
{
    [TestClass]
    public class LdapConnectionFactoryUnitTests
    {
        private readonly NetworkCredential _credential = new NetworkCredential("testUsername","testPassword");
        private bool secureSocketLayer = true;
        private bool transportSocketLayer = false;
        private bool clientCertificate = true;

        [TestMethod, TestCategory("LdapConnectionFactory")]
        public void GetLdapConnector()
        {
            var configRepo = new LdapConfigRepository();
            ILdapConnector connector = LdapConnectorFactory.GetLdapConnector(new LdapAdminModeChecker(configRepo), configRepo,
                new FakeLogger());

            Assert.IsInstanceOfType(connector, typeof (ALdapConnector));
        }

        [TestMethod, TestCategory("LdapConnectionFactory")]
        public void GetLdapConnection()
        {
            var ldapConfigRepository = new LdapConfigRepository();
            ldapConfigRepository.BasicLdapConfig(null,LDAPAdminMode.Anonymous, "127.0.0.1:636","test",AuthType.Basic,LoggerType.None,"");
            ldapConfigRepository.AdditionalLdapConfig(secureSocketLayer, transportSocketLayer, clientCertificate, @"LDAPCert/Terena-chain.pem", "test", "test");
            var ldapConnection = LdapConnectionFactory.GetLdapConnection(ldapConfigRepository);

            Assert.IsInstanceOfType(ldapConnection, typeof(LdapConnection));         
            Assert.IsTrue(ldapConnection.ClientCertificates.Count == 1);
        }
    }
}