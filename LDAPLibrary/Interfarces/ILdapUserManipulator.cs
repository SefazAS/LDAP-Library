using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using LDAPLibrary.Enums;

namespace LDAPLibrary.Interfarces
{
    public interface ILdapUserManipulator : ILdapConnectionObserver
    {
        /// <summary>
        /// Create a new LDAPUser
        /// </summary>
        /// <param name="newUser">User to create</param>
        /// <returns> Success or Failed</returns>
        LdapState CreateUser(ILdapUser newUser);

        /// <summary>
        /// Delete an LDAPUser
        /// </summary>
        /// <param name="user">User to delete</param>
        /// <returns>Success or Failed</returns>
        LdapState DeleteUser(ILdapUser user);

        /// <summary>
        /// Modify an LDAPUser Attribute
        /// </summary>
        /// <param name="operationType">Operation to execute on the attribute</param>
        /// <param name="user">LDAPUser's Attribute</param>
        /// <param name="attributeName">Attribute name</param>
        /// <param name="attributeValue">Attribute Value</param>
        /// <returns>Success or Failed</returns>
        LdapState ModifyUserAttribute(DirectoryAttributeOperation operationType, ILdapUser user,string attributeName,string attributeValue);

        /// <summary>
        /// Change an LDAPUser's Password
        /// </summary>
        /// <param name="user">LDAPUser to change the password</param>
        /// <param name="newPwd">New Passowrd</param>
        /// <returns>Success or Failed</returns>
        LdapState ChangeUserPassword(ILdapUser user, string newPwd);

    }
}