using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using LDAPLibrary.Enums;
using LDAPLibrary.Interfarces;
using LDAPLibrary.StaticClasses;

namespace LDAPLibrary
{
    /// <summary>
    /// Class used to perform all the LDAP CUD operations
    /// </summary>
    public class LdapUserManipulator : ILdapUserManipulator
    {
        private ILdapConfigRepository _configRepository;
        private ILogger _logger;
        private LdapConnection _ldapConnection;

        public LdapUserManipulator(ILogger logger, ILdapConfigRepository configRepository)
        {
            _logger = logger;
            _configRepository = configRepository;
        }

        public void SetLdapConnection(LdapConnection ldapConnection)
        {
            _ldapConnection = ldapConnection;
        }

        /// <summary>
        ///     Create a new LDAPUser
        /// </summary>
        /// <param name="newUser">User to create</param>
        /// <returns> Success or Failed</returns>
        public LdapState CreateUser(ILdapUser newUser)
        {
            try
            {
                _ldapConnection.SendRequest(LdapRequestBuilder.GetAddRequest(newUser,
                    _configRepository.GetUserObjectClass()));
            }
            catch (Exception e)
            {
                _logger.Write(_logger.BuildLogMessage(e.Message, LdapState.LdapCreateUserError));
                return LdapState.LdapCreateUserError;
            }
            _logger.Write(_logger.BuildLogMessage("Create User Operation Success", LdapState.LdapUserManipulatorSuccess));
            return LdapState.LdapUserManipulatorSuccess;
        }

        /// <summary>
        ///     Delete an LDAPUser
        /// </summary>
        /// <param name="user">User to delete</param>
        /// <returns>Success or Failed</returns>
        public LdapState DeleteUser(ILdapUser user)
        {
            try
            {
                _ldapConnection.SendRequest(LdapRequestBuilder.GetDeleteRequest(user));
            }
            catch (Exception e)
            {
                _logger.Write(_logger.BuildLogMessage(e.Message, LdapState.LdapDeleteUserError));
                return LdapState.LdapDeleteUserError;
            }
            _logger.Write(_logger.BuildLogMessage("Delete User Operation Success", LdapState.LdapUserManipulatorSuccess));
            return LdapState.LdapUserManipulatorSuccess;
        }

        /// <summary>
        ///     Modify an LDAPUser Attribute
        /// </summary>
        /// <param name="operationType">Operation to execute on the attribute</param>
        /// <param name="user">LDAPUser's Attribute</param>
        /// <param name="attributeName">Attribute name</param>
        /// <param name="attributeValue">Attribute Value</param>
        /// <returns>Success or Failed</returns>
        public LdapState ModifyUserAttribute(DirectoryAttributeOperation operationType, ILdapUser user,
            string attributeName,
            string attributeValue)
        {
            try
            {
                _ldapConnection.SendRequest(LdapRequestBuilder.GetModifyRequest(user, operationType, attributeName,
                    attributeValue));
            }
            catch (Exception e)
            {
                _logger.Write(_logger.BuildLogMessage(e.Message, LdapState.LdapModifyUserAttributeError));
                return LdapState.LdapModifyUserAttributeError;
            }

            user.GetUserOperation(operationType, attributeName, attributeValue)();

            _logger.Write(_logger.BuildLogMessage("Modify User Attribute Operation Success",
                LdapState.LdapUserManipulatorSuccess));
            return LdapState.LdapUserManipulatorSuccess;
        }

        /// <summary>
        ///     Change an LDAPUser's Password
        /// </summary>
        /// <param name="user">LDAPUser to change the password</param>
        /// <param name="newPwd">New Passowrd</param>
        /// <returns>Success or Failed</returns>
        public LdapState ChangeUserPassword(ILdapUser user, string newPwd)
        {
            try
            {
                _ldapConnection.SendRequest(LdapRequestBuilder.GetModifyPasswordRequest(user, newPwd));
            }
            catch (Exception e)
            {
                _logger.Write(_logger.BuildLogMessage(e.Message, LdapState.LdapChangeUserPasswordError));
                return LdapState.LdapChangeUserPasswordError;
            }

            user.OverwriteUserAttribute("userPassword", newPwd);

            _logger.Write(_logger.BuildLogMessage("Change Password Operation Success",
                LdapState.LdapUserManipulatorSuccess));
            return LdapState.LdapUserManipulatorSuccess;
        }
        
        public void Dispose()
        {
            _configRepository = null;
            _logger = null;
            _ldapConnection.Dispose();
        }
    }
}