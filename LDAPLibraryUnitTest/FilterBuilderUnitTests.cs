using System;
using LDAPLibrary.StaticClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LDAP_Library_UnitTest
{
    [TestClass]
    public class FilterBuilderUnitTests
    {
        private const string ObjectClass = "person";
        private const string FieldUsername = "cn";
        private const string UserCn = "Manager";

        [TestMethod, TestCategory("FilterBuilder Search")]
        public void SearchFilterBuilderObjectClassAndFieldName()
        {
            string searchFilterExpected = String.Format("(&(objectClass={0})({1}={2}))", ObjectClass, FieldUsername,
                UserCn);
            string searchFilter = LdapFilterBuilder.GetSearchFilter(ObjectClass, FieldUsername, UserCn);

            Assert.AreEqual(searchFilterExpected, searchFilter);
        }

        [TestMethod, TestCategory("FilterBuilder Search")]
        public void SearchFilterBuilderOnlyObjectClass()
        {
            string searchFilterExpected = String.Format("(objectClass={0})", ObjectClass);
            string searchFilter = LdapFilterBuilder.GetSearchFilter(ObjectClass);

            Assert.AreEqual(searchFilterExpected, searchFilter);
        }

        [TestMethod, TestCategory("FilterBuilder Search")]
        public void SearchFilterBuilderAll()
        {
            string searchFilterExpected = "(objectClass=*)";
            string searchFilter = LdapFilterBuilder.GetSearchFilter();

            Assert.AreEqual(searchFilterExpected, searchFilter);
        }
    }
}