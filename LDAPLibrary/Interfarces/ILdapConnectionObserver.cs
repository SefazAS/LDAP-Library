using System.DirectoryServices.Protocols;

namespace LDAPLibrary.Interfarces
{
    using System;

    public interface ILdapConnectionObserver : IDisposable
    {
        void SetLdapConnection(LdapConnection ldapConnection);
    }
}