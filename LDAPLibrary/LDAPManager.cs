using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using LDAPLibrary.Enums;
using LDAPLibrary.Factories;
using LDAPLibrary.Interfarces;
using LDAPLibrary.Logger;
using LDAPLibrary.StaticClasses;

namespace LDAPLibrary
{
    /// <summary>
    /// Entry class of the Library
    /// </summary>
    public class LdapManager : ILdapManager
    {
        #region Class Variables

        private ILdapConfigRepository _configRepository;
        private ILdapConnector _connector;
        private ILogger _logger;
        private ILdapUserManipulator _manageLdapUser;
        private ILdapSearcher _searcher;

        private ILdapAdminModeChecker _adminModeChecker;
        private LdapState _ldapCurrentState;

        #endregion

        /// <summary>
        /// LDAP library constructior where all the class variables are initialized
        /// The variables not specified in definition will be set at default values.
        /// </summary>
        /// <param name="adminUser">Admin user</param>
        /// <param name="adminMode">Admin User</param>
        /// <param name="ldapServer">LDAP Server with port</param>
        /// <param name="ldapSearchBaseDn">Base DN where start the search.</param>
        /// <param name="authType"></param>
        /// <param name="loggerType">Mode to log</param>
        /// <param name="logPath">Path of the logger File</param>
        public LdapManager(ILdapUser adminUser, LDAPAdminMode adminMode,
            string ldapServer,
            string ldapSearchBaseDn,
            AuthType authType,
            LoggerType loggerType,
            string logPath
            )
        {
            _configRepository = LdapConfigRepositoryFactory.GetConfigRepository();
            try
            {
                _configRepository.BasicLdapConfig(adminUser,adminMode, ldapServer, ldapSearchBaseDn, authType, loggerType, logPath);
                _logger = LoggerFactory.GetLogger(_configRepository.GetWriteLogFlag(), _configRepository.GetLogPath());
            }
            catch (ArgumentNullException)
            {
                _ldapCurrentState = LdapState.LdapLibraryInitError;
                throw;
            }

            _adminModeChecker = new LdapAdminModeChecker(_configRepository);

            CommonInitOperations();
            _ldapCurrentState = LdapState.LdapLibraryInitSuccess;
        }

        private void CommonInitOperations()
        {
            _connector = LdapConnectorFactory.GetLdapConnector(_adminModeChecker, _configRepository, _logger);
            _manageLdapUser = LdapUserManipulatorFactory.GetUserManipulator(_connector, _logger, _configRepository);
            _searcher = LdapSearcherFactory.GetSearcher(_connector, _logger, _configRepository);
        }

        /// <summary>
        /// More detailed contructor that user the default constructor and the addictionalLDAPInformation method
        /// </summary>
        public LdapManager(ILdapUser adminUser, LDAPAdminMode adminMode,string ldapServer,string ldapSearchBaseDn,AuthType authType,bool secureSocketLayerFlag,bool transportSocketLayerFlag,bool clientCertificateFlag,string clientCertificatePath,LoggerType loggerType,string logPath,string userObjectClass,string matchFieldUsername)
            : this(adminUser, adminMode,
                ldapServer,
                ldapSearchBaseDn,
                authType,
                loggerType, logPath)
        {
            try
            {
                _configRepository.AdditionalLdapConfig(secureSocketLayerFlag, transportSocketLayerFlag,
                    clientCertificateFlag, clientCertificatePath, userObjectClass,
                    matchFieldUsername);
            }
            catch (ArgumentNullException e)
            {
                _ldapCurrentState = LdapState.LdapLibraryInitError;
                _logger.Write(_logger.BuildLogMessage(e.Message, _ldapCurrentState));
                throw;
            }

            CommonInitOperations();
            _ldapCurrentState = LdapState.LdapLibraryInitSuccess;
            _logger.Write(_logger.BuildLogMessage("", _ldapCurrentState));
        }

        #region Methods from LDAPUserManipulator Class

        public bool CreateUser(ILdapUser newUser)
        {
            _ldapCurrentState = _manageLdapUser.CreateUser(newUser);
            return LdapStateUtils.ToBoolean(_ldapCurrentState);
        }

        public bool DeleteUser(ILdapUser user)
        {
            _ldapCurrentState = _manageLdapUser.DeleteUser(user);
            return LdapStateUtils.ToBoolean(_ldapCurrentState);
        }


        public bool ModifyUserAttribute(DirectoryAttributeOperation operationType, ILdapUser user, string attributeName,
            string attributeValue)
        {
            _ldapCurrentState = _manageLdapUser.ModifyUserAttribute(operationType, user, attributeName, attributeValue);
            return LdapStateUtils.ToBoolean(_ldapCurrentState);
        }

        public bool ChangeUserPassword(ILdapUser user, string newPwd)
        {
            _ldapCurrentState = _manageLdapUser.ChangeUserPassword(user, newPwd);
            return LdapStateUtils.ToBoolean(_ldapCurrentState);
        }


        public bool SearchUsers(IList<string> otherReturnedAttributes, string[] searchedUsers,
            out IList<ILdapUser> searchResult)
        {
            _ldapCurrentState = _searcher.SearchUsers(otherReturnedAttributes, searchedUsers, out searchResult);
            return LdapStateUtils.ToBoolean(_ldapCurrentState);
        }

        public bool SearchUsers(IList<string> otherReturnedAttributes, out IList<ILdapUser> searchResult)
        {
            _ldapCurrentState = _searcher.SearchUsers(otherReturnedAttributes, out searchResult);
            return LdapStateUtils.ToBoolean(_ldapCurrentState);
        }

        public bool SearchAllNodes(IList<string> otherReturnedAttributes, out IList<ILdapUser> searchResult)
        {
            _ldapCurrentState = _searcher.SearchAllNodes(otherReturnedAttributes, out searchResult);
            return LdapStateUtils.ToBoolean(_ldapCurrentState);
        }

        #endregion

        public string GetLdapMessage()
        {
            return _logger.BuildLogMessage("", _ldapCurrentState);
        }

        public bool Connect()
        {
            _ldapCurrentState = _connector.Connect();
            return LdapStateUtils.ToBoolean(_ldapCurrentState);
        }

        public bool Connect(NetworkCredential credential)
        {
            _ldapCurrentState = _connector.Connect(credential);
            return LdapStateUtils.ToBoolean(_ldapCurrentState);
        }


        public bool SearchUserAndConnect(string user, string password)
        {
            IList<ILdapUser> searchReturn;

            //Do the search and check the result 
            bool searchResult = SearchUsers(null, new[] {user}, out searchReturn);

            //if the previous search goes try to connect all the users
            return searchResult &&
                   searchReturn.Select(
                       searchedUser =>
                           Connect(new NetworkCredential(searchedUser.GetUserDn(), password)))
                       .Any(connectResult => connectResult);
        }

        /// <summary>
        /// Call the dispose method of his fields and set to null the reference fo the others
        /// </summary>
        public void Dispose()
        {
            _manageLdapUser.Dispose();
            _connector.Dispose();
            _configRepository = null;
            _logger = null;
            _adminModeChecker = null;
        }
    }
}