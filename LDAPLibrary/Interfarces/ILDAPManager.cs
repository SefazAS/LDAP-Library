using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;

namespace LDAPLibrary.Interfarces
{
    public interface ILdapManager : IDisposable
    {
        /// <summary>
        /// Create a new LDAP User
        /// </summary>
        /// <param name="newUser"> The LDAPUser object that contain all the details of the new user to create</param>
        /// <returns>Boolean that comunicate the result of creation</returns>
        bool CreateUser(ILdapUser newUser);

        /// <summary>
        /// delete the specified  LDAPUser
        /// </summary>
        /// <param name="user">LDAPUser to delete</param>
        /// <returns>the result of operation</returns>
        bool DeleteUser(ILdapUser user);

        /// <summary>
        /// Modify an LDAPUser Attribute
        /// </summary>
        /// <param name="operationType">Choose the operation to do, it's an enum</param>
        /// <param name="user">The User to Modify the attribute</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <param name="attributeValue">Value of the attribute</param>
        /// <returns></returns>
        bool ModifyUserAttribute(DirectoryAttributeOperation operationType, ILdapUser user, string attributeName,
            string attributeValue);

        /// <summary>
        /// Change the user Password
        /// </summary>
        /// <param name="user">LDAPUser to change the password</param>
        /// <param name="newPwd"></param>
        /// <returns></returns>
        bool ChangeUserPassword(ILdapUser user, string newPwd);

        /// <summary>
        /// Search Users in the LDAP system
        /// </summary>
        /// <param name="otherReturnedAttributes">Addictional attributes added to the results LDAPUsers objects</param>
        /// <param name="searchedUsers">Credential for the search</param>
        /// <param name="searchResult">LDAPUsers object returned in the search</param>
        /// <returns>Boolean that comunicate the result of search</returns>
        bool SearchUsers(IList<string> otherReturnedAttributes, string[] searchedUsers, out IList<ILdapUser> searchResult);

        /// <summary>
        /// Search all the users in the base tree.
        /// Does not apply the match fieldname to search
        /// </summary>
        /// <param name="otherReturnedAttributes">Attribute To return in the search</param>
        /// <param name="searchResult">Result of the search</param>
        /// <returns>Boolean that comunicate the result of search</returns>
        bool SearchUsers(IList<string> otherReturnedAttributes, out IList<ILdapUser> searchResult);

        /// <summary>
        /// Search for every node in the LDAP Tree
        /// </summary>
        /// <param name="otherReturnedAttributes">Attribute To return in the search</param>
        /// <param name="searchResult">Result of the search</param>
        /// <returns>Boolean that comunicate the result of search</returns>
        bool SearchAllNodes(IList<string> otherReturnedAttributes, out IList<ILdapUser> searchResult);

        /// <summary>
        /// Return the Error Message of an occurred LDAP Exception
        /// </summary>
        /// <returns></returns>
        string GetLdapMessage();

        /// <summary>
        /// Instance the Ldap connection
        /// </summary>
        /// <returns></returns>
        bool Connect();

        bool Connect(NetworkCredential credential);

        /// <summary>
        /// BuildLogMessage that wrap SearchUsers and Connect in one operation
        /// </summary>
        /// <param name="user">The Username to search and Connect</param>
        /// <param name="password">The passwords of the User</param>
        /// <returns>the operation result</returns>
        bool SearchUserAndConnect(string user, string password);
    }
}