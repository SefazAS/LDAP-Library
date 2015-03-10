﻿using System;
using System.DirectoryServices.Protocols;
using LDAPLibrary.Enums;
using LDAPLibrary.Interfarces;
using LDAPLibrary.Logger;
using LDAPLibrary.StaticClasses;

namespace LDAPLibrary
{
    public class LdapConfigRepository : ILdapConfigRepository
    {
        private const string BasicConfigNullParametersErrorMessage =
            "Server or SearchBaseDn parameter cannot be null or empty and the file log path cannot be null if the logType is 'File' ";

        private const string CompleteConfigNullParametersErrorMessage =
            "One param are null or empty:Admin User: {0},clientCertificatePath: {1}, userObjectClass: {2},matchFieldUsername: {3}";

        #region Configuration Parameters

        private ILdapUser _adminUser;
        private AuthType _authType;
        private bool _clientCertificateFlag;
        private string _clientCertificatePath;
        private string _logPath;
        private LoggerType _loggerType;
        private string _matchFieldUsername;
        private string _searchBaseDn;
        private bool _secureSocketLayerFlag;
        private string _server;
        private bool _transportSocketLayerFlag;
        private string _userObjectClass;
        private LDAPAdminMode _adminMode;

        #endregion

        #region Configuration Parameters Getters

        public ILdapUser GetAdminUser()
        {
            return _adminUser ?? new FakeLdapUser();
        }

        public string GetServer()
        {
            return _server;
        }

        public string GetSearchBaseDn()
        {
            return _searchBaseDn;
        }

        public AuthType GetAuthType()
        {
            return _authType;
        }

        public bool GetSecureSocketLayerFlag()
        {
            return _secureSocketLayerFlag;
        }

        public bool GetTransportSocketLayerFlag()
        {
            return _transportSocketLayerFlag;
        }

        public bool GetClientCertificateFlag()
        {
            return _clientCertificateFlag;
        }

        public string GetClientCertificatePath()
        {
            return _clientCertificatePath;
        }

        public LoggerType GetWriteLogFlag()
        {
            return _loggerType;
        }

        public string GetLogPath()
        {
            return _logPath;
        }

        public string GetUserObjectClass()
        {
            return _userObjectClass;
        }

        public string GetMatchFieldName()
        {
            return _matchFieldUsername;
        }

        public LDAPAdminMode GetAdminMode()
        {
            return _adminMode;
        }

        #endregion
        /// <summary>
        /// Check the validity of parameters
        /// Set the basic Ldap Configuration from parameters
        /// Set the others to the standard values
        /// </summary>
        /// <param name="adminUser">Admin User</param>
        /// <param name="adminMode">Library Admin Mode</param>
        /// <param name="server">Server URL</param>
        /// <param name="searchBaseDn">Root Search Node</param>
        /// <param name="authType"></param>
        /// <param name="loggerType"></param>
        /// <param name="logPath">Path of the log file</param>
        public void BasicLdapConfig(ILdapUser adminUser, LDAPAdminMode adminMode, string server, string searchBaseDn, AuthType authType, LoggerType loggerType, string logPath)
        {
            BasicLdapConfigValidator(server, loggerType, logPath,searchBaseDn,adminUser,adminMode);

            _authType = authType;
            _searchBaseDn = searchBaseDn;
            _server = server;
            _adminUser = adminUser;
            _loggerType = loggerType;
            _logPath = logPath;
            _adminMode = adminMode;

            StandardLdapInformation();
        }

        /// <summary>
        /// Check the validity of the parameters
        /// Set addiitonal configuration form the parameters
        /// </summary>
        /// <param name="secureSocketLayerFlag"></param>
        /// <param name="transportSocketLayerFlag"></param>
        /// <param name="clientCertificateFlag"></param>
        /// <param name="clientCertificatePath">Path of the certificate file</param>
        /// <param name="userObjectClass">Object class that rapresent an user</param>
        /// <param name="matchFieldUsername">Attribute that rapresent the username</param>
        public void AdditionalLdapConfig(
            bool secureSocketLayerFlag, bool transportSocketLayerFlag, bool clientCertificateFlag,
            string clientCertificatePath, string userObjectClass,
            string matchFieldUsername)
        {
            AddictionalLdapConfigValidator(clientCertificatePath, userObjectClass, matchFieldUsername);

            _matchFieldUsername = matchFieldUsername;
            _userObjectClass = userObjectClass;
            _clientCertificatePath = clientCertificatePath;
            _clientCertificateFlag = clientCertificateFlag;
            _transportSocketLayerFlag = transportSocketLayerFlag;
            _secureSocketLayerFlag = secureSocketLayerFlag;
        }

        /// <summary>
        /// Set LDAP Information To standard values.
        /// </summary>
        private void StandardLdapInformation()
        {
            //Default class variables information
            _secureSocketLayerFlag = false;
            _transportSocketLayerFlag = false;
            _clientCertificateFlag = false;
            _clientCertificatePath = "";
            _userObjectClass = "person";
            _matchFieldUsername = "cn";
        }

        /// <summary>
        /// Check the validity of parameters
        /// 
        /// * Server and searchBaseDn cannot be null or empty
        /// * logPath cannot be null or empty if the loggerType is set to File
        /// * Admin cannot be null if there's admin mode
        /// 
        /// Used in the BasicLdapConfig method
        /// 
        /// Can throw an ArgumentNullException
        /// </summary>
        /// <param name="server">Server URL</param>
        /// <param name="loggerType"></param>
        /// <param name="logPath">Path of the log file</param>
        /// <param name="searchBaseDn">Search Root Node</param>
        /// <param name="admin">Admin User</param>
        /// <param name="adminMode">Library Admin Mode</param>
        private static void BasicLdapConfigValidator(string server, LoggerType loggerType, string logPath, string searchBaseDn, ILdapUser admin, LDAPAdminMode adminMode)
        {
            if (LdapParameterChecker.ParametersIsNullOrEmpty(new[] { server, searchBaseDn }) ||
                (LdapParameterChecker.ParametersIsNullOrEmpty(new[] { logPath })) && loggerType == LoggerType.File ||
                (adminMode == LDAPAdminMode.Admin && admin == null))
                throw new ArgumentNullException(String.Format(BasicConfigNullParametersErrorMessage));
        }

        /// <summary>
        /// Check the validity of parameters
        /// 
        /// * Check if clientCertificatePath, userObjectClass, matchFieldUsername are null or empty
        /// 
        /// Used in AdditionalLdapConfig
        /// 
        /// Can throw an ArgumentNullException
        /// </summary>
        /// <param name="clientCertificatePath"></param>
        /// <param name="userObjectClass"></param>
        /// <param name="matchFieldUsername"></param>
        private void AddictionalLdapConfigValidator(string clientCertificatePath, string userObjectClass,
            string matchFieldUsername)
        {
            if (LdapParameterChecker.ParametersIsNullOrEmpty(new[] { clientCertificatePath, userObjectClass, matchFieldUsername }))
                throw new ArgumentNullException(String.Format(CompleteConfigNullParametersErrorMessage,
                    _adminUser, clientCertificatePath, userObjectClass, matchFieldUsername));
        }
    }
}