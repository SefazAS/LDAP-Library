using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LDAP Library UnitTest")]

namespace LDAPLibrary.StaticClasses
{
    /// <summary>
    /// Class that build the search filter for the search methods
    /// </summary>
    internal static class LdapFilterBuilder
    {
        private const string SearchFilterTemplateObjectClassAndFieldMatch = "(&(objectClass={0})({1}={2}))";
        private const string SearchFilterTemplateObjectClassMatch = "(objectClass={0})";
        private const string SearchFilterTemplateAllMatch = "(objectClass=*)";

        /// <summary>
        /// Build a filter that fetch only the nodes with:
        /// The specified object class
        /// The match between the second parameter and the third.
        /// </summary>
        /// <param name="objectClass">Object class value to match</param>
        /// <param name="fieldUsername">Fieldname to match</param>
        /// <param name="user">field value to match</param>
        /// <returns></returns>
        public static string GetSearchFilter(string objectClass, string fieldUsername, string user)
        {
            return String.Format(SearchFilterTemplateObjectClassAndFieldMatch, objectClass, fieldUsername, user);
        }

        /// <summary>
        /// Build a filter that fetch only the nodes with:
        /// The specified object class
        /// </summary>
        /// <param name="objectClass">Object class value to match</param>
        /// <returns></returns>
        public static string GetSearchFilter(string objectClass)
        {
            return String.Format(SearchFilterTemplateObjectClassMatch, objectClass);
        }

        /// <summary>
        /// Fetch all the nodes in the base one. NO FILTER.
        /// </summary>
        /// <returns></returns>
        public static string GetSearchFilter()
        {
            return String.Format(SearchFilterTemplateAllMatch);
        }
    }
}