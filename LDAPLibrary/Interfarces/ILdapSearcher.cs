using System;
using System.Collections.Generic;
using System.Text;
using LDAPLibrary.Enums;
using LDAPLibrary.Interfarces;

namespace LDAPLibrary
{
	public interface ILdapSearcher : ILdapConnectionObserver
	{
	    /// <summary>
	    /// Search Users in the LDAP system
	    /// </summary>
	    /// <param name="otherReturnedAttributes">Addictional attributes added to the results LDAPUsers objects</param>
	    /// <param name="searchedUsers">Credential for the search</param>
	    /// <param name="searchResult">LDAPUsers object returned in the search</param>
	    /// <returns>Boolean that comunicate the result of search</returns>
	    LdapState SearchUsers(IList<string> otherReturnedAttributes, string[] searchedUsers, out IList<ILdapUser> searchResult);

		/// <summary>
		/// Search all the users in the base tree.
		/// Does not apply the match fieldname to search
		/// </summary>
		/// <param name="otherReturnedAttributes">Attribute To return in the search</param>
		/// <param name="searchResult">Result of the search</param>
		/// <returns>Boolean that comunicate the result of search</returns>
		LdapState SearchUsers(IList<string> otherReturnedAttributes, out IList<ILdapUser> searchResult);
		
		/// <summary>
		/// Search for every node in the LDAP Tree
		/// </summary>
		/// <param name="otherReturnedAttributes">Attribute To return in the search</param>
		/// <param name="searchResult">Result of the search</param>
		/// <returns>Boolean that comunicate the result of search</returns>
		LdapState SearchAllNodes(IList<string> otherReturnedAttributes, out IList<ILdapUser> searchResult);
	}
}
