﻿using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace LDAPLibrary
{
    public class LdapManager : ILdapManager
    {
        #region Class Variables

        private readonly ILdapConfigRepository _configRepository;
        
        private readonly LdapModeChecker _modeChecker;
        private LdapConnection _ldapConnection;
        private LdapState _ldapCurrentState;
        private LdapUserManipulator _manageLdapUser;
        private readonly ILogger _logger;

        #endregion

        /// <summary>
        ///     LDAP library constructior where all the class variables are initialized
        ///     The variables not specified in definition will be set at default values.
        /// </summary>
        /// <param name="adminUser"></param>
        /// <param name="ldapServer">LDAP Server with port</param>
        /// <param name="ldapSearchBaseDn">Base DN where start the search.</param>
        /// <param name="authType"></param>
        public LdapManager(ILdapUser adminUser,
            string ldapServer,
            string ldapSearchBaseDn,
            AuthType authType
            )
        {
            _configRepository = LdapConfigRepositoryFactory.GetConfigRepository();
            try
            {
                _configRepository.BasicLdapConfig(adminUser, ldapServer, ldapSearchBaseDn, authType);
            }
            catch (ArgumentNullException)
            {
                _ldapCurrentState = LdapState.LdapLibraryInitError;
                throw;
            }
           
            _modeChecker = new LdapModeChecker(_configRepository);
            _ldapCurrentState = LdapState.LdapLibraryInitSuccess;
        }

        /// <summary>
        ///     More detailed contructor that user the default constructor and the addictionalLDAPInformation method
        /// </summary>
        public LdapManager(ILdapUser adminUser,
            string ldapServer,
            string ldapSearchBaseDn,
            AuthType authType,
            bool secureSocketLayerFlag,
            bool transportSocketLayerFlag,
            bool clientCertificateFlag,
            string clientCertificatePath,
            bool writeLogFlag,
            string logPath,
            string userObjectClass,
            string matchFieldUsername
            )
            : this(adminUser,
                ldapServer,
                ldapSearchBaseDn,
                authType)
        {
            try
            {
                _logger = LoggerFactory.GetLogger(writeLogFlag,logPath);
                _configRepository.AdditionalLdapConfig(secureSocketLayerFlag, transportSocketLayerFlag,
                    clientCertificateFlag, clientCertificatePath, writeLogFlag, logPath, userObjectClass,
                    matchFieldUsername);
            }
            catch (ArgumentNullException e)
            {
                _ldapCurrentState = LdapState.LdapLibraryInitError;
                _logger.Write(_logger.BuildLogMessage(e.Message, _ldapCurrentState));
                throw;
            }
            _ldapCurrentState = LdapState.LdapLibraryInitSuccess;
            _logger.Write(_logger.BuildLogMessage("", _ldapCurrentState));
        }

        #region Methods from LDAPUserManipulator Class

        /// <summary>
        ///     Create a new LDAP User
        /// </summary>
        /// <param name="newUser"> The LDAPUser object that contain all the details of the new user to create</param>
        /// <returns>Boolean that comunicate the result of creation</returns>
        public bool CreateUser(ILdapUser newUser)
        {
            bool operationResult = _manageLdapUser.CreateUser(newUser, out _ldapCurrentState,
                _configRepository.GetUserObjectClass());
            _logger.Write(_logger.BuildLogMessage(_manageLdapUser.GetLdapUserManipulationMessage(), _ldapCurrentState));
            return operationResult;
        }

        /// <summary>
        ///     delete the specified  LdapUser
        /// </summary>
        /// <param name="user">LDAPUser to delete</param>
        /// <returns>the result of operation</returns>
        public bool DeleteUser(ILdapUser user)
        {
            bool operationResult = _manageLdapUser.DeleteUser(user, out _ldapCurrentState);
            _logger.Write(_logger.BuildLogMessage(_manageLdapUser.GetLdapUserManipulationMessage(), _ldapCurrentState));
            return operationResult;
        }

        /// <summary>
        ///     Modify an LDAPUser Attribute
        /// </summary>
        /// <param name="operationType">Choose the operation to do, it's an enum</param>
        /// <param name="user">The User to Modify the attribute</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <param name="attributeValue">Value of the attribute</param>
        /// <returns></returns>
        public bool ModifyUserAttribute(DirectoryAttributeOperation operationType, ILdapUser user, string attributeName,
            string attributeValue)
        {
            bool operationResult = _manageLdapUser.ModifyUserAttribute(operationType, user, attributeName,
                attributeValue, out _ldapCurrentState);
            _logger.Write(_logger.BuildLogMessage(_manageLdapUser.GetLdapUserManipulationMessage(), _ldapCurrentState));
            return operationResult;
        }

        /// <summary>
        ///     Change the user Password
        /// </summary>
        /// <param name="user">LDAPUser to change the password</param>
        /// <param name="newPwd"></param>
        /// <returns></returns>
        public bool ChangeUserPassword(ILdapUser user, string newPwd)
        {
            bool operationResult = _manageLdapUser.ChangeUserPassword(user, newPwd, out _ldapCurrentState);
            _logger.Write(_logger.BuildLogMessage(_manageLdapUser.GetLdapUserManipulationMessage(), _ldapCurrentState));
            return operationResult;
        }

        /// <summary>
        ///     Search Users in the LDAP system
        /// </summary>
        /// <param name="otherReturnedAttributes">Addictional attributes added to the results LDAPUsers objects</param>
        /// <param name="searchedUsers">Credential for the search</param>
        /// <param name="searchResult">LDAPUsers object returned in the search</param>
        /// <returns>Boolean that comunicate the result of search</returns>
        public bool SearchUsers(List<string> otherReturnedAttributes, string[] searchedUsers,
            out List<ILdapUser> searchResult)
        {
            bool operationResult = _manageLdapUser.SearchUsers(_configRepository.GetSearchBaseDn(),
                _configRepository.GetUserObjectClass(), _configRepository.GetMatchFieldName(),
                otherReturnedAttributes, searchedUsers, out searchResult, out _ldapCurrentState);
            _logger.Write(_logger.BuildLogMessage(_manageLdapUser.GetLdapUserManipulationMessage(), _ldapCurrentState));
            return operationResult;
        }

        #endregion

        /// <summary>
        ///     Return the Error Message of an occurred LDAP Exception
        /// </summary>
        /// <returns>Message</returns>
        public string GetLdapMessage()
        {
            return _logger.BuildLogMessage("", _ldapCurrentState);
        }

        /// <summary>
        ///     Instance the Ldap connection with admin config credential
        /// </summary>
        /// <returns>Success or Failed</returns>
        public bool Connect()
        {
            try
            {
                if (_modeChecker.IsCompleteMode())
                {
                    return Connect(
                        new NetworkCredential(_configRepository.GetAdminUser().GetUserDn(),
                            _configRepository.GetAdminUser().GetUserAttribute("userPassword")[0]),
                        _configRepository.GetSecureSocketLayerFlag(),
                        _configRepository.GetTransportSocketLayerFlag(),
                        _configRepository.GetClientCertificateFlag());
                }
                return false;
            }
            catch (Exception)
            {
                const string error = "LDAP CONNECTION WITH ADMIN WS-CONFIG CREDENTIAL DENIED: unable to connect with administrator credential, see the config file";
                _ldapCurrentState = LdapState.LdapConnectionError;
                _logger.Write(_logger.BuildLogMessage(error, _ldapCurrentState));
                throw new Exception(error);
            }
        }

        /// <summary>
        ///     Connect to LDAP with the specified credential
        /// </summary>
        /// <param name="credential">user Credential</param>
        /// <param name="secureSocketLayer">Flag that specify if we want to use SSL for connection.</param>
        /// <param name="transportSocketLayer"></param>
        /// <param name="clientCertificate"></param>
        /// <returns>Success or Failed</returns>
        public bool Connect(NetworkCredential credential, bool secureSocketLayer, bool transportSocketLayer,
            bool clientCertificate)
        {
            try
            {
                _ldapConnection = new LdapConnection(_configRepository.GetServer())
                {
                    AuthType = _configRepository.GetAuthType()
                };
                _ldapConnection.SessionOptions.ProtocolVersion = 3;

                #region secure Layer Options

                if (secureSocketLayer)
                    _ldapConnection.SessionOptions.SecureSocketLayer = true;

                if (transportSocketLayer)
                {
                    LdapSessionOptions options = _ldapConnection.SessionOptions;
                    options.StartTransportLayerSecurity(null);
                }

                if (clientCertificate)
                {
                    var clientCertificateFile = new X509Certificate();
                    clientCertificateFile.Import(_configRepository.GetClientCertificatePath());
                    _ldapConnection.ClientCertificates.Add(clientCertificateFile);
                }

                #endregion

                _ldapConnection.Bind(credential);
                //ldapConnection.SendRequest(new SearchRequest(LDAPServer, "(objectClass=*)", SearchScope.Subtree, null));
                _manageLdapUser = new LdapUserManipulator(_ldapConnection);
            }
            catch (Exception e)
            {
                var errorConnectionMessage = String.Format("{0}\n User: {1}\n Pwd: {2}{3}{4}{5}",
                    e.Message,
                    credential.UserName,
                    credential.Password,
                    (secureSocketLayer ? "\n With SSL " : ""),
                    (transportSocketLayer ? "\n With TLS " : ""),
                    (clientCertificate ? "\n With Client Certificate" : ""));
                _ldapCurrentState = LdapState.LdapConnectionError;
                _logger.Write(_logger.BuildLogMessage(errorConnectionMessage, _ldapCurrentState));
                return false;
            }
            var successConnectionMessage = String.Format("Connection success\n User: {0}\n Pwd: {1}{2}{3}{4}",
                credential.UserName,
                credential.Password,
                (secureSocketLayer ? "\n With SSL " : ""),
                (transportSocketLayer ? "\n With TLS " : ""),
                (clientCertificate ? "\n With Client Certificate" : ""));
            if (_modeChecker.IsBasicMode())
                _ldapConnection.Dispose();
            _ldapCurrentState = LdapState.LdapConnectionSuccess;
            _logger.Write(_logger.BuildLogMessage(successConnectionMessage, _ldapCurrentState));
            return true;
        }

        /// <summary>
        ///     Search the user and try to connect to LDAP
        /// </summary>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        /// <returns>
        ///     TRUE: connected
        ///     FALSE: not connected
        /// </returns>
        public bool SearchUserAndConnect(string user, string password)
        {
            List<ILdapUser> searchReturn;

            //Do the search and check the result 
            bool searchResult = SearchUsers(null, new[] {user}, out searchReturn);

            //if the previous search goes try to connect all the users
            return searchResult &&
                   searchReturn.Select(
                       searchedUser =>
                           Connect(new NetworkCredential(searchedUser.GetUserDn(), password),
                               _configRepository.GetSecureSocketLayerFlag(),
                               _configRepository.GetTransportSocketLayerFlag(),
                               _configRepository.GetClientCertificateFlag()))
                       .Any(connectResult => connectResult);
        }
    }
}