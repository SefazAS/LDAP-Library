using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
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
    public class LocalhostEnvironment
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
        private readonly LdapUser _testUser = new LdapUser(WriteUserDn, WriteUserCn, "test",
                new Dictionary<string, List<string>> {{"userPassword", new List<string> {WriteUserPwd}}});
        
        
        private readonly LdapUser[] _fakeUsers =
        {
            new LdapUser(WriteUserDn, WriteUserCn, "test",
                new Dictionary<string, List<string>> {{"userPassword", new List<string> {"FakePassword"}}}),
            new LdapUser(WriteUserDn, WriteUserCn, "test",
                new Dictionary<string, List<string>> {{"userPassword", new List<string> {""}}})
        };

        #endregion

        #region Localhost Configuration

        private const AuthType LdapAuthType = AuthType.Basic;
        private const string LdapServer = "127.0.0.1:389";

        private const string LdapAdminUserDn = "cn=Manager,dc=maxcrc,dc=com";
        private const string LdapAdminUserCn = "Manager";
        private const string LdapAdminUserSn = "test";
        private const string LdapAdminUserPassword = "secret";

        private const string LdapSearchBaseDn = "o=ApexNet,ou=People,dc=maxcrc,dc=com";
        private const string LdapUserObjectClass = "person";
        private const string LdapMatchFieldUsername = "cn";
        private const LoggerType EnableLdapLibraryLog = LoggerType.File;
        private const bool SecureSocketLayerFlag = false;
        private const bool TransportSocketLayerFlag = false;
        private const bool ClientCertificationFlag = false;
        private const string ClientCertificatePath = "null";
        private const LDAPAdminMode AdminMode = LDAPAdminMode.Admin;

        private static readonly LdapUser AdminUser = new LdapUser(LdapAdminUserDn,
            LdapAdminUserCn,
            LdapAdminUserSn,
            new Dictionary<string, List<string>> {{"userPassword", new List<string> {LdapAdminUserPassword}}});

        private static readonly string LdapLibraryLogPath = string.Format("{0}", AppDomain.CurrentDomain.BaseDirectory);

        #endregion

        #region LDAP Library Tests - Base

        [TestMethod, TestCategory("LDAPLibrary Test Init")]
        public void TestCompleteInitLibrary()
        {
            _ldapManagerObj = new LdapManager(AdminUser,AdminMode,
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
        public void TestAdminConnect()
        {
            //Init the DLL
            TestCompleteInitLibrary();

            //Connect with admin user
            Assert.IsTrue(_ldapManagerObj.Connect());
        }

        #endregion

        #region LDAP Library Tests - Write Permission Required

        [TestMethod, TestCategory("LDAPLibrary Test Write Permissions")]
        public void TestCreateUser()
        {
            var tempUser = new LdapUser(WriteUserDn, WriteUserCn, "test", null);
            var existingUser = new LdapUser(ReadOnlyUserDn, ReadOnlyUserCn, "test",
                new Dictionary<string, List<string>> {{"userPassword", new List<string> {ReadOnlyUserPwd}}});

            //Init the DLL and connect the admin
            TestAdminConnect();

            //Create existing user
            var result = _ldapManagerObj.CreateUser(existingUser);

            //Assert the correct operations
            Assert.IsFalse(result);
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1), "LDAP CREATE USER ERROR: ");

            //Create user
            result = _ldapManagerObj.CreateUser(tempUser);

            //Assert the correct operations
            Assert.IsTrue(result);
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1),
                "LDAP USER MANIPULATION SUCCESS: ");

            result = _ldapManagerObj.DeleteUser(tempUser);

            Assert.IsTrue(result);
        }

        [TestMethod, TestCategory("LDAPLibrary Test Write Permissions")]
        public void TestDeleteUser()
        {
            //Set the test user
            var testLdapUser = new LdapUser(WriteUserDn, WriteUserCn, "test", null);

            //Init the DLL and connect the admin
            TestAdminConnect();

            //Create LDAPUser to delete.
            var result = _ldapManagerObj.CreateUser(testLdapUser);

            Assert.IsTrue(result);

            //Delete user
            result = _ldapManagerObj.DeleteUser(testLdapUser);

            //Assert the correct operations
            Assert.IsTrue(result);
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1),
                "LDAP USER MANIPULATION SUCCESS: ");

            //Delete user again with error
            result = _ldapManagerObj.DeleteUser(testLdapUser);

            //Assert the correct operations
            Assert.IsFalse(result);
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1), "LDAP DELETE USER ERROR: ");
        }

        [TestMethod, TestCategory("LDAPLibrary Test Write Permissions")]
        public void TestModifyUserAttribute()
        {
            TestAdminConnect();
            var testLdapUser = new LdapUser(WriteUserDn, WriteUserCn, "test",
                new Dictionary<string, List<string>> {{"description", new List<string> {"test"}}});
            var result = _ldapManagerObj.CreateUser(testLdapUser);

            Assert.IsTrue(result);

            IList<ILdapUser> returnUsers;
            const string userAttributeValue = "description Modified";

            result = _ldapManagerObj.ModifyUserAttribute(DirectoryAttributeOperation.Delete, testLdapUser, "ciccio",
                userAttributeValue);

            Assert.IsFalse(result);
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1),
                "LDAP MODIFY USER ATTRIBUTE ERROR: ");

            result = _ldapManagerObj.ModifyUserAttribute(DirectoryAttributeOperation.Replace, testLdapUser,
                "description", userAttributeValue);

            Assert.IsTrue(result);
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1),
                "LDAP USER MANIPULATION SUCCESS: ");

            result = _ldapManagerObj.SearchUsers(
                new List<string> {"description"},
                new[] {WriteUserCn},
                out returnUsers);

            Assert.IsTrue(result);
            Assert.AreEqual(returnUsers[0].GetUserCn(), testLdapUser.GetUserCn());
            Assert.AreEqual(returnUsers[0].GetUserAttribute("description")[0], userAttributeValue);

            result = _ldapManagerObj.DeleteUser(testLdapUser);

            Assert.IsTrue(result);
        }

        [TestMethod, TestCategory("LDAPLibrary Test Write Permissions")]
        public void TestChangeUserPassword()
        {
            TestAdminConnect();
            const string newPassword = "pippo";
            var testUser = new LdapUser(WriteUserDn, WriteUserCn, "test",
                new Dictionary<string, List<string>> {{"userPassword", new List<string> {WriteUserPwd}}});
            //Create the user
            var result = _ldapManagerObj.CreateUser(testUser);

            Assert.IsTrue(result);

            //Perform change of password
            result = _ldapManagerObj.ChangeUserPassword(testUser, newPassword);
            Assert.IsTrue(result);
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1),
                "LDAP USER MANIPULATION SUCCESS: ");

            //Try to connect with the old password
            var testUserCredential = new NetworkCredential(
                testUser.GetUserDn(),
                WriteUserPwd,
                "");

            result = _ldapManagerObj.Connect(testUserCredential);

            Assert.IsFalse(result);
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1), "LDAP CONNECTION ERROR: ");

            //Try to connect with the new password
            testUserCredential = new NetworkCredential(
                testUser.GetUserDn(),
                newPassword,
                "");

            result = _ldapManagerObj.Connect(testUserCredential);

            Assert.IsTrue(result);
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1), "LDAP CONNECTION SUCCESS");

            TestAdminConnect();

            result = _ldapManagerObj.DeleteUser(testUser);

            Assert.IsTrue(result);
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1),
                "LDAP USER MANIPULATION SUCCESS: ");
        }

        [TestMethod, TestCategory("LDAPLibrary Test Write Permissions")]
        public void TestUserConnect()
        {
            TestAdminConnect();


            var result = _ldapManagerObj.CreateUser(_testUser);
            Assert.IsTrue(result);

            var testUserCredential = new NetworkCredential(
                _testUser.GetUserDn(),
                _testUser.GetUserAttribute("userPassword")[0],
                "");
            
            TestConnectFakeUsers();
            

            result = _ldapManagerObj.Connect(testUserCredential);

            Assert.IsTrue(result);
            Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1), "LDAP CONNECTION SUCCESS");

            TestAdminConnect();
            result = _ldapManagerObj.DeleteUser(_testUser);
            Assert.IsTrue(result);
        }

        private void TestConnectFakeUsers()
        {
            _fakeUsers.Select(user => new NetworkCredential(user.GetUserDn(), user.GetUserAttribute("userPassword")[0], ""))
                .ToList()
                .ForEach(
                    nc =>
                    {
                        var result = _ldapManagerObj.Connect(nc);

                        Assert.IsFalse(result);
                        Assert.AreEqual(_ldapManagerObj.GetLdapMessage().Split('-')[1].Substring(1),
                            "LDAP CONNECTION ERROR: ");
                    }
                );
        }

        [TestMethod, TestCategory("LDAPLibrary Test Write Permissions")]
        public void TestSearchUserAndConnect()
        {
            TestAdminConnect();
            var testLdapUser = new LdapUser(WriteUserDn, WriteUserCn, "test",
                new Dictionary<string, List<string>> {{"userPassword", new List<string> {WriteUserPwd}}});

            var result = _ldapManagerObj.CreateUser(testLdapUser);

            Assert.IsTrue(result);

            result = _ldapManagerObj.SearchUserAndConnect(WriteUserCn, WriteUserPwd);

            Assert.IsTrue(result);

            TestAdminConnect();

            result = _ldapManagerObj.DeleteUser(testLdapUser);

            Assert.IsTrue(result);
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

            var result = _ldapManagerObj.SearchUsers(userAttributeToReturnBySearch, fakeuserIdToSearch, out returnUsers);

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
        public void TestSearchNoFieldnameMatch()
        {

            //////////////////////////////////////////////////////////
            // ATTENTION, THIS TEST WILL FAIL IF IN THE DEFAULT LOCALHOST LDAP ISN'T PRESENT:
            // Matteo : objectClass -> person
            // Alessandro : objectClass -> person
            //////////////////////////////////////////////////////////
            TestAdminConnect();

            var userAttributeToReturnBySearch = new List<string>
            {
                "description"
            };

            IList<ILdapUser> returnUsers;

            var result = _ldapManagerObj.SearchUsers(null, out returnUsers);

            Assert.IsTrue(result);
            Assert.AreEqual(returnUsers.Count, 2);
            Assert.AreEqual(returnUsers[0].GetUserCn(), ReadOnlyUserCn);
            Assert.IsTrue(returnUsers[0].GetUserAttributes().Count == 0);

            result = _ldapManagerObj.SearchUsers(userAttributeToReturnBySearch, out returnUsers);

            Assert.IsTrue(result);
            Assert.AreEqual(returnUsers.Count, 2);
            Assert.AreEqual(returnUsers[0].GetUserCn(), ReadOnlyUserCn);
        }

        [TestMethod, TestCategory("LDAPLibrary Test Read Permissions")]
        public void TestSearchAllNodes()
        {

            //////////////////////////////////////////////////////////
            // ATTENTION, THIS TEST WILL FAIL IF IN THE DEFAULT LOCALHOST LDAP ISN'T PRESENT:
            // Matteo : objectClass -> person
            // Alessandro : objectClass -> person
            // Clock : objectClass -> device
            //////////////////////////////////////////////////////////
            TestAdminConnect();

            var userAttributeToReturnBySearch = new List<string>
            {
                "description"
            };

            IList<ILdapUser> returnUsers;

            var result = _ldapManagerObj.SearchAllNodes(null, out returnUsers);

            Assert.IsTrue(result);
            Assert.AreEqual(returnUsers.Count, 4);
            Assert.AreEqual(returnUsers[1].GetUserCn(), ReadOnlyUserCn);
            Assert.IsTrue(returnUsers[1].GetUserAttributes().Count == 0);

            result = _ldapManagerObj.SearchUsers(userAttributeToReturnBySearch, out returnUsers);

            Assert.IsTrue(result);
            Assert.AreEqual(returnUsers.Count, 4);
            Assert.AreEqual(returnUsers[1].GetUserCn(), ReadOnlyUserCn);
            Assert.IsTrue(returnUsers[1].GetUserAttributes().Count == 1);
        }

        [TestMethod, TestCategory("LDAPLibrary Test Read Permissions")]
        public void TestUserConnectWithoutWritePermissions()
        {
            TestAdminConnect();

            var result = _ldapManagerObj.Connect(new NetworkCredential(
                ReadOnlyUserDn, ReadOnlyUserPwd,
                ""));

            Assert.IsTrue(result);

            TestConnectFakeUsers();
        }

        [TestMethod, TestCategory("LDAPLibrary Test Read Permissions")]
        public void TestSearchUserAndConnectWithoutWritePermissions()
        {
            TestAdminConnect();

            var result = _ldapManagerObj.SearchUserAndConnect(ReadOnlyUserCn, ReadOnlyUserPwd);

            Assert.IsTrue(result);
        }

        #endregion
    }
}