using LDAPLibrary;
using LDAPLibrary.Factories;
using LDAPLibrary.Interfarces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LDAP_Library_UnitTest
{
    [TestClass]
    public class LdapConfigRepositoryFactoryUnitTests
    {
        [TestMethod, TestCategory("LdapConfigRepositoryFactory")]
        public void GetLdapConfigRepository()
        {
            ILdapConfigRepository configRepo = LdapConfigRepositoryFactory.GetConfigRepository();

            Assert.IsInstanceOfType(configRepo, typeof (LdapConfigRepository));
        }
    }
}