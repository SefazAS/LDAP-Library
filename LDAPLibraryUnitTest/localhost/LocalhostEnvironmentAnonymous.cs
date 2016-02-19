using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using LDAPLibrary;
using LDAPLibrary.Enums;
using LDAPLibrary.Interfarces;
using LDAPLibrary.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/*
 * 
 * 
 * TEST TRAMITE LDAP LOCALE ( tramite ldap for windows vedi oneNote)
 * IMPORTANTE:
 * - AVERE UN A STRUTTURA COME QUESTA: o=ApexNet,ou=People,dc=maxcrc,dc=com
 * - AVERE ALL'INTERNO UN UTENTE DI NOME "MATTEO", VEDI COSTANTI DI CLASSE (ReadOnlyUserCn....)
 *   L'UTENTE WRITE INVECE SI CREA E SI CANCELLA DA SOLO A MENO DI FALLIMENTI NEI TEST.
 * 
 * 
 */

namespace LDAP_Library_UnitTest.localhost
{
    [TestClass]
    public class LocalhostEnvironmentAnonymous
    {
        //Class fields for the test
        private ILdapManager _ldapManagerObj; //LDAPLibrary

        #region Users

        //READ ONLY USER
        private const string ReadOnlyUserCn = "Matteo";
        private const string ReadOnlyUserPwd = "1";
        private const string ReadOnlyUserDn = "cn=" + ReadOnlyUserCn + ",o=ApexNet,ou=People,dc=maxcrc,dc=com";
        //WRITE USER THIS MUST NOT EXIST INITIALLY
        private const string WriteUserCn = "Fabio";
        private const string WriteUserPwd = "1";
        private const string WriteUserDn = "cn=" + WriteUserCn + ",o=ApexNet,ou=People,dc=maxcrc,dc=com";

        #endregion

        #region Localhost Configuration

        private const AuthType LdapAuthType = AuthType.Basic;
        private const string LdapServer = "127.0.0.1:389";

        private const string LdapSearchBaseDn = "o=ApexNet,ou=People,dc=maxcrc,dc=com";
        private const string LdapUserObjectClass = "person";
        private const string LdapMatchFieldUsername = "cn";
        private const LoggerType EnableLdapLibraryLog = LoggerType.File;
        private const bool SecureSocketLayerFlag = false;
        private const bool TransportSocketLayerFlag = false;
        private const bool ClientCertificationFlag = false;
        private const string ClientCertificatePath = "null";
        private const LDAPAdminMode AdminMode = LDAPAdminMode.Anonymous;

        private static readonly string LdapLibraryLogPath = string.Format("{0}", AppDomain.CurrentDomain.BaseDirectory);

        #endregion

        #region LDAP Library Tests - Base

        [TestMethod, TestCategory("LDAPLibrary Test Init")]
        public void TestCompleteInitLibrary()
        {
            _ldapManagerObj = new LdapManager(null,AdminMode,
                LdapServer,
                LdapSearchBaseDn,
                LdapAuthType,
                SecureSocketLayerFlag,
                TransportSocketLayerFlag,
                ClientCertificationFlag,
                ClientCertificatePath,
                EnableLdapLibraryLog,
                LdapLibraryLogPath,
                LdapUserObjectClass,
                LdapMatchFieldUsername
                );

            Assert.IsFalse(_ldapManagerObj.Equals(null));
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1), "LDAP LIBRARY INIT SUCCESS");
        }

        [TestMethod, TestCategory("LDAPLibrary Test Init")]
        public void TestStandardInitLibrary()
        {
            _ldapManagerObj = new LdapManager(null, AdminMode,
                LdapServer,
                LdapSearchBaseDn,
                LdapAuthType, EnableLdapLibraryLog, LdapLibraryLogPath
                );

            Assert.IsFalse(_ldapManagerObj.Equals(null));
        }

        [TestMethod, TestCategory("LDAPLibrary Test Init")]
        public void TestAdminConnect()
        {
            //Init the DLL
            TestCompleteInitLibrary();

            //Connect with admin user
            Assert.IsTrue(_ldapManagerObj.Connect());
        }

        #endregion
        
        #region LDAP Library Tests - Only Read Permission Required

        [TestMethod, TestCategory("LDAPLibrary Test Read Permissions")]
        public void TestSearchUser()
        {
            TestAdminConnect();

            string[] userIdToSearch =
            {
                ReadOnlyUserCn
            };
            string[] fakeuserIdToSearch =
            {
                WriteUserCn
            };
            var userAttributeToReturnBySearch = new List<string>
            {
                "description"
            };

            IList<ILdapUser> returnUsers;

            bool result = _ldapManagerObj.SearchUsers(userAttributeToReturnBySearch, fakeuserIdToSearch, out returnUsers);

            Assert.IsFalse(result);
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1), "LDAP SEARCH USER ERROR: ");

            result = _ldapManagerObj.SearchUsers(null, userIdToSearch, out returnUsers);

            Assert.IsTrue(result);
            Assert.AreEqual(returnUsers.Count, userIdToSearch.Length);
            Assert.AreEqual(returnUsers[0].GetUserCn(), ReadOnlyUserCn);
            Assert.IsTrue(returnUsers[0].GetUserAttributes().Count == 0);

            result = _ldapManagerObj.SearchUsers(userAttributeToReturnBySearch, userIdToSearch, out returnUsers);

            Assert.IsTrue(result);
            Assert.AreEqual(returnUsers.Count, userIdToSearch.Length);
            Assert.AreEqual(returnUsers[0].GetUserCn(), ReadOnlyUserCn);
        }

        [TestMethod, TestCategory("LDAPLibrary Test Read Permissions")]
        public void TestUserConnectWithoutWritePermissions()
        {
            TestAdminConnect();

            bool result = _ldapManagerObj.Connect(new NetworkCredential(
                ReadOnlyUserDn, ReadOnlyUserPwd,
                ""));

            Assert.IsTrue(result);
        }

        [TestMethod, TestCategory("LDAPLibrary Test Read Permissions")]
        public void TestSearchUserAndConnectWithoutWritePermissions()
        {
            TestAdminConnect();

            bool result = _ldapManagerObj.SearchUserAndConnect(ReadOnlyUserCn, ReadOnlyUserPwd);

            Assert.IsTrue(result);
        }

        #endregion
    }
}