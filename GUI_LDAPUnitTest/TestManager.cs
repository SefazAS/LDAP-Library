﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LDAPLibrary;
using System.Configuration;
using System.Net;
using System.DirectoryServices.Protocols;

namespace GUI_LDAPUnitTest
{
    public class TestManager
    {

        private LDAPUser testUser;
        private string testUserNewPassword, testUserNewDescription;
        private string[] LDAPMatchSearchField;
        private string[] usersToSearch;
        private ILDAPManager LDAPManagerObj;

        public TestManager(ILDAPManager lm)
        {
            this.LDAPManagerObj = lm;

            if (testUser == null)
            {
                string testUserCN = "defaultTestUserCN";
                string testUserSN = "defaultTestUserSN";
                //Cut the DN of Admin User from his CN and add the default CN of testUser
                string testUserDN = (
                    "cn=" + testUserCN +
                     Config.LDAPLibrary["LDAPAdminUserDN"].Substring(Config.LDAPLibrary["LDAPAdminUserDN"].IndexOf(","))
                    );
                Dictionary<string, string[]> testUserOtherAttribute = new Dictionary<string, string[]>()
				{
					//aggiungere inizializzare così il dizionario
					{	"userPassword", new string[]{"defaultTestUserPassword"}	}
				};

                setupTestUser(testUserDN, testUserCN, testUserSN, testUserOtherAttribute);
            }

            if (string.IsNullOrEmpty(testUserNewPassword))
                setupTestUserNewPassword("defaultNewTestUserPassword");

            if (string.IsNullOrEmpty(testUserNewDescription))
                setupTestUserNewDescription("defaultNewTestUserDescription");

            if (usersToSearch == null)
                setupUsersToSearch(new string[] { "defaultNewTestUserCN" });

            LDAPMatchSearchField = new string[] { Config.LDAPLibrary["LDAPMatchFieldUsername"] };
        }

        #region Method called for configuration

        public void setupUsersToSearch(string[] list)
        {
            usersToSearch = list;
        }

        public void setupTestUserNewPassword(string p)
        {
            testUserNewPassword = p;
        }

        public void setupTestUser(string testUserDN, string testUserCN, string testUserSN, Dictionary<string, string[]> testUserOtherAttribute)
        {
            testUser = new LDAPUser(testUserDN, testUserCN, testUserSN, testUserOtherAttribute);
        }

        public void setupTestUserNewDescription(string p)
        {
            testUserNewDescription = p;
        }


        #endregion

        #region Unit Tests

        #region LDAP Library Tests - Base

        private bool testCompleteInitLibrary()
        {
            try
            {

                Dictionary<string, string[]> tempAttributes = new Dictionary<string, string[]>()
			{
				//aggiungere inizializzare così il dizionario
				{	"userPassword", new string[]{ Config.LDAPLibrary["LDAPAdminUserPassword"]}	}
			};

                LDAPManagerObj = new LDAPManager(Config.LDAPLibrary["LDAPAdminUserDN"],
                                                     Config.LDAPLibrary["LDAPAdminUserCN"],
                                                     Config.LDAPLibrary["LDAPAdminUserSN"],
                                                    tempAttributes,
                                                     Config.LDAPLibrary["LDAPServer"],
                                                     Config.LDAPLibrary["LDAPSearchBaseDN"],
                                                    Convert.ToBoolean(Config.LDAPLibrary["secureSocketLayerFlag"]),
                                                    Convert.ToBoolean(Config.LDAPLibrary["transportSocketLayerFlag"]),
                                                    Convert.ToBoolean(Config.LDAPLibrary["ClientCertificationFlag"]),
                                                     Config.LDAPLibrary["clientCertificatePath"],
                                                    Convert.ToBoolean(Config.LDAPLibrary["enableLDAPLibraryLog"]),
                                                     Config.LDAPLibrary["LDAPLibraryLogPath"],
                                                     Config.LDAPLibrary["LDAPUserObjectClass"],
                                                     Config.LDAPLibrary["LDAPMatchFieldUsername"]
                                                    );

                if (LDAPManagerObj.getLDAPMessage().Equals("LDAP LIBRARY INIT SUCCESS"))
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool testStandardInitLibrary()
        {

            try
            {
                Dictionary<string, string[]> tempAttributes = new Dictionary<string, string[]>()
			{
				//aggiungere inizializzare così il dizionario
				{	"userPassword", new string[]{ Config.LDAPLibrary["LDAPAdminUserPassword"]}	}
			};

                LDAPManagerObj = new LDAPManager(Config.LDAPLibrary["LDAPAdminUserDN"],
                                                     Config.LDAPLibrary["LDAPAdminUserCN"],
                                                     Config.LDAPLibrary["LDAPAdminUserSN"],
                                                    tempAttributes,
                                                     Config.LDAPLibrary["LDAPServer"],
                                                     Config.LDAPLibrary["LDAPSearchBaseDN"]
                                                    );

                if (LDAPManagerObj.getLDAPMessage().Equals("LDAP LIBRARY INIT SUCCESS"))
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool testAdminConnect()
        {

            //Init the DLL
            if (!testCompleteInitLibrary())
                return false;

            //Connect with admin user
            if (!LDAPManagerObj.connect())
                return false;

            //Assert the behavior of DLL
            if (LDAPManagerObj.getLDAPMessage().Equals("LDAP CONNECTION SUCCESS"))
                return true;
            else
                return false;


        }

        #endregion

        #region LDAP Library Tests - Write Permission Required


        private bool testCreateUser()
        {
            if (!testAdminConnect())
                return false;

            //Create user
            bool result = LDAPManagerObj.createUser(testUser);

            //Assert the correct operations
            if (!result &&
                    !LDAPManagerObj.getLDAPMessage().Equals("LDAP USER MANIPULATION SUCCESS: " + "Create User Operation Success")
                )
                return false;



            result = LDAPManagerObj.deleteUser(testUser);

            return result;
        }


        private bool testDeleteUser()
        {
            //Init the DLL and connect the admin
            if (!testAdminConnect())
                return false;

            //Create LDAPuser to delete.
            bool result = LDAPManagerObj.createUser(testUser);

            if (!result)
                return false;

            //Delete user
            result = LDAPManagerObj.deleteUser(testUser);

            if (
                !result &&
                !LDAPManagerObj.getLDAPMessage().Equals("LDAP USER MANIPULATION SUCCESS: " + "Delete User Operation Success"))
                return false;
            else
                return true;
        }


        private bool testModifyUserAttribute()
        {
            if (!testAdminConnect())
                return false;

            if (!LDAPManagerObj.createUser(testUser))
                return false;

            List<LDAPUser> returnUsers = new List<LDAPUser>();


            bool result = LDAPManagerObj.modifyUserAttribute(DirectoryAttributeOperation.Replace, testUser, "description", testUserNewDescription);

            if (!result)
                return result;

            switch (LDAPMatchSearchField[0])
            {
                case "cn": result = LDAPManagerObj.searchUsers(new List<string> { "description" },
                                                               new string[] { testUser.getUserCn() },
                                                               out returnUsers);
                    break;
                case "sn": result = LDAPManagerObj.searchUsers(new List<string> { "description" },
                                                                new string[] { testUser.getUserSn() },
                                                                out returnUsers);
                    break;
                case "dn":
                    result = LDAPManagerObj.searchUsers(new List<string> { "description" },
                                                               new string[] { testUser.getUserDn() },
                                                               out returnUsers);
                    break;
                default:
                    result = LDAPManagerObj.searchUsers(new List<string> { "description" },
                                                               testUser.getUserAttribute(LDAPMatchSearchField[0]),
                                                               out returnUsers);
                    break;
            }

            if (result &&
            returnUsers[0].getUserCn().Equals(testUser.getUserCn()) &&
            returnUsers[0].getUserAttribute("description")[0].Equals(testUserNewDescription))
            {
                result = LDAPManagerObj.deleteUser(testUser);
                if (result)
                    return true;
                return false;
            }
            else
            {
                result = LDAPManagerObj.deleteUser(testUser);
                return false;
            }
        }


        private bool testChangeUserPassword()
        {
            if (!testAdminConnect())
                return false;

            //Create the user
            bool result = LDAPManagerObj.createUser(testUser);

            string oldPassword = testUser.getUserAttribute("userPassword")[0];

            if (!result)
                return false;

            //Perform change of password
            result = LDAPManagerObj.changeUserPassword(testUser, testUserNewPassword);

            if (!result)
                return false;

            //Try to connect with the old password
            NetworkCredential testUserCredential = new NetworkCredential(
                testUser.getUserDn(),
                oldPassword,
                "");

            result = LDAPManagerObj.connect(testUserCredential,
                        Convert.ToBoolean(ConfigurationManager.AppSettings["secureSocketLayerFlag"]),
                        Convert.ToBoolean(ConfigurationManager.AppSettings["transportSocketLayerFlag"]),
                        Convert.ToBoolean(ConfigurationManager.AppSettings["ClientCertificationFlag"]));

            if (result)
                return false;

            //Try to connect with the new password
            testUserCredential = new NetworkCredential(
                testUser.getUserDn(),
                testUserNewPassword,
                "");

            result = LDAPManagerObj.connect(testUserCredential,
                        Convert.ToBoolean(ConfigurationManager.AppSettings["secureSocketLayerFlag"]),
                        Convert.ToBoolean(ConfigurationManager.AppSettings["transportSocketLayerFlag"]),
                        Convert.ToBoolean(ConfigurationManager.AppSettings["ClientCertificationFlag"]));
                       
            testAdminConnect();

            if (result)
            {
                result = LDAPManagerObj.deleteUser(testUser);
                return true;
            }
            else
            {
                result = LDAPManagerObj.deleteUser(testUser);
                return false;
            }
        }


        private bool testUserConnect()
        {

            if (!testAdminConnect())
                return false;

            bool result = LDAPManagerObj.createUser(testUser);

            if (!result)
                return false;

            NetworkCredential testUserCredential = new NetworkCredential(
                testUser.getUserDn(),
                testUser.getUserAttribute("userPassword")[0],
                "");

            result = LDAPManagerObj.connect(testUserCredential,
                        Convert.ToBoolean(Config.LDAPLibrary["secureSocketLayerFlag"]),
                        Convert.ToBoolean(Config.LDAPLibrary["transportSocketLayerFlag"]),
                        Convert.ToBoolean(Config.LDAPLibrary["ClientCertificationFlag"]));

            if (!result)
                return false;

            if (!testAdminConnect())
                return false;

            result = LDAPManagerObj.deleteUser(testUser);

            return result;
        }


        private bool testSearchUserAndConnect()
        {

            if (!testAdminConnect())
                return false;

            bool result = LDAPManagerObj.createUser(testUser);

            if (!result)
                return false;

            switch (LDAPMatchSearchField[0])
            {
                case "cn": result = LDAPManagerObj.searchUserAndConnect(testUser.getUserCn(), testUser.getUserAttribute("userPassword")[0]); break;
                case "sn": result = LDAPManagerObj.searchUserAndConnect(testUser.getUserSn(), testUser.getUserAttribute("userPassword")[0]); break;
                case "dn": result = LDAPManagerObj.searchUserAndConnect(testUser.getUserDn(), testUser.getUserAttribute("userPassword")[0]); break;
                default: result = LDAPManagerObj.searchUserAndConnect(testUser.getUserAttribute(LDAPMatchSearchField[0])[0], testUser.getUserAttribute("userPassword")[0]); break;
            }

            if (!result)
            {
                result = LDAPManagerObj.deleteUser(testUser);
                return false;
            }

            if (!testAdminConnect())
            {
                result = LDAPManagerObj.deleteUser(testUser);
                return false;
            }

            result = LDAPManagerObj.deleteUser(testUser);

            return result;
        }

        #endregion

        #region LDAP Library Tests - Only Read Permission Required


        private bool testSearchUser()
        {
            if (!testAdminConnect())
                return false;

            List<LDAPUser> returnUsers = new List<LDAPUser>();

            bool result = LDAPManagerObj.searchUsers(null, usersToSearch, out returnUsers);

            if (result &&
                returnUsers.Count.Equals(usersToSearch.Length))
                return result;
            else
                return false;
        }


        private bool testUserConnectWithoutWritePermissions()
        {

            if (!testAdminConnect())
                return false;

            NetworkCredential testUserCredential = new NetworkCredential(
                testUser.getUserDn(),
                testUser.getUserAttribute("userPassword")[0],
                "");

            bool result = LDAPManagerObj.connect(testUserCredential,
                        Convert.ToBoolean(Config.LDAPLibrary["secureSocketLayerFlag"]),
                        Convert.ToBoolean(Config.LDAPLibrary["transportSocketLayerFlag"]),
                        Convert.ToBoolean(Config.LDAPLibrary["ClientCertificationFlag"]));

            return result;

        }


        private bool testSearchUserAndConnectWithoutWritePermissions()
        {
            if (!testAdminConnect())
                return false;
            bool result;

            switch (LDAPMatchSearchField[0])
            {
                case "cn": return result = LDAPManagerObj.searchUserAndConnect(testUser.getUserCn(), testUser.getUserAttribute("userPassword")[0]);
                case "sn": return result = LDAPManagerObj.searchUserAndConnect(testUser.getUserSn(), testUser.getUserAttribute("userPassword")[0]);
                case "dn": return result = LDAPManagerObj.searchUserAndConnect(testUser.getUserDn(), testUser.getUserAttribute("userPassword")[0]);
                default: return result = LDAPManagerObj.searchUserAndConnect(testUser.getUserAttribute(LDAPMatchSearchField[0])[0], testUser.getUserAttribute("userPassword")[0]);
            }
        }

        #endregion


        #endregion

        public string getTestUserCN()
        {
            return testUser.getUserCn();
        }
        public string getTestUserDN()
        {
            return testUser.getUserDn();
        }
        public string getTestUserSN()
        {
            return testUser.getUserSn();
        }
        public string[] getTestUserOtherAttributes(string attributeKey)
        {
            return testUser.getUserAttribute(attributeKey);
        }
        public string[] getTestUserOtherAttributesKeys()
        {
            return testUser.getUserAttributeKeys();
        }
        public string[] getUserToSearch()
        {
            return usersToSearch;
        }
        public string getTestUserNewPassword()
        {
            return testUserNewPassword;
        }
        public string getTestUserNewDescription()
        {
            return testUserNewDescription;
        }
        public bool runTest(tests testType, bool writePermission)
        {
            switch (testType)
            {
                case tests.testInitLibrary: return testCompleteInitLibrary();
                case tests.testAdminConnection: return testAdminConnect();
                case tests.testCreateUser: return testCreateUser();
                case tests.testModifyUserDescription: return testModifyUserAttribute();
                case tests.testSearchUsers: return testSearchUser();
                case tests.testConnectUser: if (writePermission) return testUserConnect();
                    else return testUserConnectWithoutWritePermissions();
                case tests.testSearchUserAndConnect: if (writePermission) return testSearchUserAndConnect();
                    else return testSearchUserAndConnectWithoutWritePermissions();
                case tests.testUserChangePassword: return testChangeUserPassword();
                case tests.testDeleteUser: return testDeleteUser();
                default: return false;
            }
        }

    }

    public enum tests
    {
        testInitLibrary,
        testAdminConnection,
        testCreateUser,
        testModifyUserDescription,
        testSearchUsers,
        testConnectUser,
        testSearchUserAndConnect,
        testUserChangePassword,
        testDeleteUser
    }
}