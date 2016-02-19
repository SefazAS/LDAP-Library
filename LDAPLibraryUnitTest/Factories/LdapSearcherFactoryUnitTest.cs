using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LDAP_Library_UnitTest.Factories
{
    using LDAPLibrary;
    using LDAPLibrary.Factories;
    using LDAPLibrary.Logger;

    [TestClass]
    public class LdapSearcherFactoryUnitTest
    {
        [TestMethod, TestCategory("LdapSearcherFactory")]
        public void GetSearcher()
        {
            var configRepo = new LdapConfigRepository();
            var logger = new FakeLogger();
            var ldapConnector = LdapConnectorFactory.GetLdapConnector(new LdapAdminModeChecker(configRepo), configRepo, logger);

            var ldapSearcher = LdapSearcherFactory.GetSearcher(ldapConnector, logger, configRepo);

            Assert.IsInstanceOfType(ldapSearcher, typeof(ILdapSearcher));

        }
    }
}
