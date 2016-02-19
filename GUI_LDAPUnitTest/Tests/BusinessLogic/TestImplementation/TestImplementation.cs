using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using LDAPLibrary;
using LDAPLibrary.Enums;
using LDAPLibrary.Interfarces;
using LDAPLibrary.Logger;

namespace GUI_LDAPUnitTest.Tests.BusinessLogic.TestImplementation
{
    internal class TestImplementation
    {
        private readonly string[] _ldapMatchSearchField = {Config.LdapLibrary["LDAPMatchFieldUsername"]};
        private readonly TestUserRepository _userRepository;
        private ILdapManager _ldapManagerObj;

        delegate bool Test();

        public TestImplementation(TestUserRepository userRepository, ILdapManager ldapManagerObj)
        {
            _userRepository = userRepository;
            _ldapManagerObj = ldapManagerObj;
        }

        #region Unit Tests

        #region LDAP Library TestType - Base

        public bool TestCompleteInitLibrary()
        {
            try
            {
                var adminUser = new LdapUser(Config.LdapLibrary["LDAPAdminUserDN"],
                    Config.LdapLibrary["LDAPAdminUserCN"],
                    Config.LdapLibrary["LDAPAdminUserSN"],
                    null);
                adminUser.CreateUserAttribute("userPassword", Config.LdapLibrary["LDAPAdminUserPassword"]);

                var authType = (AuthType) Enum.Parse(typeof (AuthType),
                    Config.LdapLibrary["LDAPAuthType"]);

                _ldapManagerObj = new LdapManager(adminUser,
                    (LDAPAdminMode) Enum.Parse(typeof (LDAPAdminMode),Config.LdapLibrary["LDAPAdminMode"]),
                    Config.LdapLibrary["LDAPServer"],
                    Config.LdapLibrary["LDAPSearchBaseDN"],
                    authType,
                    Convert.ToBoolean(Config.LdapLibrary["secureSocketLayerFlag"]),
                    Convert.ToBoolean(Config.LdapLibrary["transportSocketLayerFlag"]),
                    Convert.ToBoolean(Config.LdapLibrary["ClientCertificationFlag"]),
                    Config.LdapLibrary["clientCertificatePath"],
                    (LoggerType) Enum.Parse(typeof (LoggerType), Config.LdapLibrary["enableLDAPLibraryLog"]),
                    Config.LdapLibrary["LDAPLibraryLogPath"],
                    Config.LdapLibrary["LDAPUserObjectClass"],
                    Config.LdapLibrary["LDAPMatchFieldUsername"]
                    );

                if (!_ldapManagerObj.Equals(null))
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TestAdminConnect()
        {
            var adminMode = (LDAPAdminMode) Enum.Parse(typeof (LDAPAdminMode), Config.LdapLibrary["LDAPAdminMode"]);

            Test initLibrary;

            if (adminMode.Equals(LDAPAdminMode.Admin))
                initLibrary = TestCompleteInitLibrary;
            else
                initLibrary = TestStardardInitLibraryNoAdmin;
            //Init the DLL
            
            if (!initLibrary())
                return false;
                

            //Connect with admin user
            if (!_ldapManagerObj.Connect())
                return false;

            //Assert the behavior of DLL
            if (!_ldapManagerObj.Equals(null))
                return true;
            return false;
        }

        public bool TestCompleteInitLibraryNoAdmin()
        {
            try
            {
                var authType = (AuthType) Enum.Parse(typeof (AuthType),
                    Config.LdapLibrary["LDAPAuthType"]);

                _ldapManagerObj = new LdapManager(null,
                    (LDAPAdminMode)Enum.Parse(typeof(LDAPAdminMode), Config.LdapLibrary["LDAPAdminMode"]),
                    Config.LdapLibrary["LDAPServer"],
                    Config.LdapLibrary["LDAPSearchBaseDN"],
                    authType,
                    Convert.ToBoolean(Config.LdapLibrary["secureSocketLayerFlag"]),
                    Convert.ToBoolean(Config.LdapLibrary["transportSocketLayerFlag"]),
                    Convert.ToBoolean(Config.LdapLibrary["ClientCertificationFlag"]),
                    Config.LdapLibrary["clientCertificatePath"],
                    (LoggerType) Enum.Parse(typeof (LoggerType), Config.LdapLibrary["enableLDAPLibraryLog"]),
                    Config.LdapLibrary["LDAPLibraryLogPath"],
                    Config.LdapLibrary["LDAPUserObjectClass"],
                    Config.LdapLibrary["LDAPMatchFieldUsername"]
                    );

                if (!_ldapManagerObj.Equals(null))
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TestStardardInitLibraryNoAdmin()
        {
            try
            {
                var authType = (AuthType) Enum.Parse(typeof (AuthType),
                    Config.LdapLibrary["LDAPAuthType"]);

                _ldapManagerObj = new LdapManager(null,
                    (LDAPAdminMode)Enum.Parse(typeof(LDAPAdminMode), Config.LdapLibrary["LDAPAdminMode"]),
                    Config.LdapLibrary["LDAPServer"],
                    Config.LdapLibrary["LDAPSearchBaseDN"],
                    authType, (LoggerType) Enum.Parse(typeof (LoggerType), Config.LdapLibrary["enableLDAPLibraryLog"]),
                    Config.LdapLibrary["LDAPLibraryLogPath"]
                    );

                if (!_ldapManagerObj.Equals(null))
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region LDAP Library TestType - Write Permission Required

        public bool TestCreateUser()
        {
            if (!TestAdminConnect())
                return false;

            //Create user
            bool result = _ldapManagerObj.CreateUser(_userRepository.TestUser);

            //Assert the correct operations
            if (!result &&
                !_ldapManagerObj.GetLdapMessage()
                    .Contains("LDAP USER MANIPULATION SUCCESS: " + "Create User Operation Success")
                )
                return false;


            result = _ldapManagerObj.DeleteUser(_userRepository.TestUser);

            return result;
        }

        public bool TestDeleteUser()
        {
            //Init the DLL and connect the admin
            if (!TestAdminConnect())
                return false;

            //Create LDAPUser to delete.
            bool result = _ldapManagerObj.CreateUser(_userRepository.TestUser);

            if (!result)
                return false;

            //Delete user
            result = _ldapManagerObj.DeleteUser(_userRepository.TestUser);

            if (
                !result &&
                !_ldapManagerObj.GetLdapMessage()
                    .Contains("LDAP USER MANIPULATION SUCCESS: " + "Delete User Operation Success"))
                return false;
            return true;
        }

        public bool TestModifyUserAttribute()
        {
            string oldDescription;

            if (!TestAdminConnect())
                return false;

            if (!_ldapManagerObj.CreateUser(_userRepository.TestUser))
                return false;

            IList<ILdapUser> returnUsers;

            try
            {
                oldDescription = _userRepository.TestUser.GetUserAttribute("description")[0];
            }
            catch (Exception)
            {
                oldDescription = "";
            }
            bool result = _ldapManagerObj.ModifyUserAttribute(DirectoryAttributeOperation.Replace,
                _userRepository.TestUser,
                "description", _userRepository.TestUserNewDescription);

            if (!result)
            {
                _ldapManagerObj.DeleteUser(_userRepository.TestUser);
                _userRepository.TestUser.OverwriteUserAttribute("description", oldDescription);
                return false;
            }
            switch (_ldapMatchSearchField[0])
            {
                case "cn":
                    result = _ldapManagerObj.SearchUsers(new List<string> {"description"},
                        new[] {_userRepository.TestUser.GetUserCn()},
                        out returnUsers);
                    break;
                case "sn":
                    result = _ldapManagerObj.SearchUsers(new List<string> {"description"},
                        new[] {_userRepository.TestUser.GetUserSn()},
                        out returnUsers);
                    break;
                case "dn":
                    result = _ldapManagerObj.SearchUsers(new List<string> {"description"},
                        new[] {_userRepository.TestUser.GetUserDn()},
                        out returnUsers);
                    break;
                default:
                    result = _ldapManagerObj.SearchUsers(new List<string> {"description"},
                        _userRepository.TestUser.GetUserAttribute(_ldapMatchSearchField[0]).ToArray(),
                        out returnUsers);
                    break;
            }

            if (result &&
                returnUsers[0].GetUserCn().Equals(_userRepository.TestUser.GetUserCn()) &&
                returnUsers[0].GetUserAttribute("description")[0].Equals(_userRepository.TestUserNewDescription))
            {
                result = _ldapManagerObj.DeleteUser(_userRepository.TestUser);
                if (result)
                    return true;
                return false;
            }
            _ldapManagerObj.DeleteUser(_userRepository.TestUser);
            _userRepository.TestUser.OverwriteUserAttribute("description",
                returnUsers[0].GetUserAttribute("description"));
            return false;
        }

        public bool TestChangeUserPassword()
        {
            if (!TestAdminConnect())
                return false;

            //Create the user
            bool result = _ldapManagerObj.CreateUser(_userRepository.TestUser);

            string oldPassword = _userRepository.TestUser.GetUserAttribute("userPassword")[0];

            if (!result)
            {
                return false;
            }

            //Perform change of password
            result = _ldapManagerObj.ChangeUserPassword(_userRepository.TestUser, _userRepository.TestUserNewPassword);

            if (!result)
            {
                _ldapManagerObj.DeleteUser(_userRepository.TestUser);
                _userRepository.TestUser.OverwriteUserAttribute("userPassword", oldPassword);
                return false;
            }

            //Try to connect with the old password
            var testUserCredential = new NetworkCredential(
                _userRepository.TestUser.GetUserDn(),
                oldPassword,
                "");

            result = _ldapManagerObj.Connect(testUserCredential);

            if (result)
            {
                _ldapManagerObj.DeleteUser(_userRepository.TestUser);
                _userRepository.TestUser.OverwriteUserAttribute("userPassword", oldPassword);
                return false;
            }

            //Try to connect with the new password
            testUserCredential = new NetworkCredential(
                _userRepository.TestUser.GetUserDn(),
                _userRepository.TestUserNewPassword,
                "");

            result = _ldapManagerObj.Connect(testUserCredential);

            TestAdminConnect();

            if (result)
            {
                result = _ldapManagerObj.DeleteUser(_userRepository.TestUser);
                _userRepository.TestUser.OverwriteUserAttribute("userPassword", oldPassword);
                return result;
            }
            _ldapManagerObj.DeleteUser(_userRepository.TestUser);
            _userRepository.TestUser.OverwriteUserAttribute("userPassword", oldPassword);
            return false;
        }

        public bool TestUserConnect()
        {
            bool result;

            if (!string.IsNullOrEmpty(Config.LdapLibrary["LDAPAdminUserDN"]))
            {
                if (!TestAdminConnect())
                    return false;

                result = _ldapManagerObj.CreateUser(_userRepository.TestUser);

                if (!result)
                    return false;
            }

            var testUserCredential = new NetworkCredential(
                _userRepository.TestUser.GetUserDn(),
                _userRepository.TestUser.GetUserAttribute("userPassword")[0],
                "");

            result = _ldapManagerObj.Connect(testUserCredential);

            if (!result)
            {
                _ldapManagerObj.DeleteUser(_userRepository.TestUser);
                return false;
            }

            if (!string.IsNullOrEmpty(Config.LdapLibrary["LDAPAdminUserDN"]))
            {
                if (!TestAdminConnect())
                    return false;
            }
            result = _ldapManagerObj.DeleteUser(_userRepository.TestUser);
            return result;
        }

        public bool TestSearchUserAndConnect()
        {
            if (!TestAdminConnect())
                return false;

            bool result = _ldapManagerObj.CreateUser(_userRepository.TestUser);

            if (!result)
                return false;

            switch (_ldapMatchSearchField[0])
            {
                case "cn":
                    result = _ldapManagerObj.SearchUserAndConnect(_userRepository.TestUser.GetUserCn(),
                        _userRepository.TestUser.GetUserAttribute("userPassword")[0]);
                    break;
                case "sn":
                    result = _ldapManagerObj.SearchUserAndConnect(_userRepository.TestUser.GetUserSn(),
                        _userRepository.TestUser.GetUserAttribute("userPassword")[0]);
                    break;
                case "dn":
                    result = _ldapManagerObj.SearchUserAndConnect(_userRepository.TestUser.GetUserDn(),
                        _userRepository.TestUser.GetUserAttribute("userPassword")[0]);
                    break;
                default:
                    result = _ldapManagerObj.SearchUserAndConnect(
                        _userRepository.TestUser.GetUserAttribute(_ldapMatchSearchField[0])[0],
                        _userRepository.TestUser.GetUserAttribute("userPassword")[0]);
                    break;
            }

            if (!result)
            {
                _ldapManagerObj.DeleteUser(_userRepository.TestUser);
                return false;
            }

            if (!TestAdminConnect())
            {
                _ldapManagerObj.DeleteUser(_userRepository.TestUser);
                return false;
            }

            result = _ldapManagerObj.DeleteUser(_userRepository.TestUser);

            return result;
        }

        #endregion

        #region LDAP Library TestType - Only Read Permission Required

        public bool TestSearchUser()
        {
            if (!TestAdminConnect())
                return false;

            IList<ILdapUser> returnUsers;

            bool result = _ldapManagerObj.SearchUsers(null, _userRepository.GetUserToSearch(), out returnUsers);

            if (result &&
                returnUsers.Count.Equals(_userRepository.GetUserToSearch().Length))
                return true;
            return false;
        }

        public bool TestUserConnectWithoutWritePermissions()
        {
            if (!string.IsNullOrEmpty(Config.LdapLibrary["LDAPAdminUserDN"]))
                if (!TestAdminConnect())
                    return false;

            var testUserCredential = new NetworkCredential(
                _userRepository.TestUser.GetUserDn(),
                _userRepository.TestUser.GetUserAttribute("userPassword")[0],
                "");

            bool result = _ldapManagerObj.Connect(testUserCredential);

            return result;
        }

        public bool TestSearchUserAndConnectWithoutWritePermissions()
        {
            if (!TestAdminConnect() || _ldapManagerObj == null)
                return false;

            switch (_ldapMatchSearchField[0])
            {
                case "cn":
                    return
                        _ldapManagerObj.SearchUserAndConnect(_userRepository.TestUser.GetUserCn(),
                            _userRepository.TestUser.GetUserAttribute("userPassword")[0]);
                case "sn":
                    return
                        _ldapManagerObj.SearchUserAndConnect(_userRepository.TestUser.GetUserSn(),
                            _userRepository.TestUser.GetUserAttribute("userPassword")[0]);
                case "dn":
                    return
                        _ldapManagerObj.SearchUserAndConnect(_userRepository.TestUser.GetUserDn(),
                            _userRepository.TestUser.GetUserAttribute("userPassword")[0]);
                default:
                    return
                        _ldapManagerObj.SearchUserAndConnect(
                            _userRepository.TestUser.GetUserAttribute(_ldapMatchSearchField[0])[0],
                            _userRepository.TestUser.GetUserAttribute("userPassword")[0]);
            }
        }

        #endregion

        #endregion
    }
}