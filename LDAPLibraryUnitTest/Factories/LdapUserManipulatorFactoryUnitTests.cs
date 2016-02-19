using LDAPLibrary;
using LDAPLibrary.Factories;
using LDAPLibrary.Interfarces;
using LDAPLibrary.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LDAP_Library_UnitTest
{
    [TestClass]
    public class LdapUserManipulatorFactoryUnitTests
    {
        [TestMethod, TestCategory("LdapUserManipulatorFactory")]
        public void GetLdapUserManipulator()
        {
            var configRepo = new LdapConfigRepository();
            var logger = new FakeLogger();
            var ldapConnector = LdapConnectorFactory.GetLdapConnector(new LdapAdminModeChecker(configRepo), configRepo, logger);
            ILdapUserManipulator ldapUserManipulator = LdapUserManipulatorFactory.GetUserManipulator(ldapConnector,
                logger, configRepo);

            Assert.IsInstanceOfType(ldapUserManipulator, typeof (ILdapUserManipulator));
        }
    }
}