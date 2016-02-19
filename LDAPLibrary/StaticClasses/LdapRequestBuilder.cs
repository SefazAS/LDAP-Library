using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Runtime.CompilerServices;
using LDAPLibrary.Interfarces;

[assembly: InternalsVisibleTo("LDAP Library UnitTest")]

namespace LDAPLibrary.StaticClasses
{
    using System.Linq;

    internal static class LdapRequestBuilder
    {
        public static AddRequest GetAddRequest(ILdapUser user, string objectClass)
        {
            var addReq = new AddRequest {DistinguishedName = user.GetUserDn()};
            addReq.Attributes.Add(new DirectoryAttribute("objectClass", objectClass));
            addReq.Attributes.Add(new DirectoryAttribute("cn", user.GetUserCn()));
            addReq.Attributes.Add(new DirectoryAttribute("sn", user.GetUserSn()));

            foreach (string attributeName in user.GetUserAttributeKeys())
            {
                foreach (string attributeValue in user.GetUserAttribute(attributeName))
                {
                    addReq.Attributes.Add(new DirectoryAttribute(attributeName, attributeValue));
                }
            }

            return addReq;
        }

        public static DeleteRequest GetDeleteRequest(ILdapUser user)
        {
            return new DeleteRequest(user.GetUserDn());
        }

        public static ModifyRequest GetModifyRequest(ILdapUser user, DirectoryAttributeOperation attributeOperation,string attributeName, string attributeValue)
        {
            return new ModifyRequest(user.GetUserDn(), attributeOperation, attributeName, attributeValue);
        }

        public static ModifyRequest GetModifyPasswordRequest(ILdapUser user, string newPassword)
        {
            var modifyUserPassword = new DirectoryAttributeModification
            {
                Operation = DirectoryAttributeOperation.Replace,
                Name = "userPassword"
            };
            modifyUserPassword.Add(newPassword);

            return new ModifyRequest(user.GetUserDn(), modifyUserPassword);
        }

        public static SearchRequest GetSearchUserRequest(string baseDn, string searchFilter,IEnumerable<string> searchAttributes)
        {
            return new SearchRequest(baseDn, searchFilter, SearchScope.Subtree, searchAttributes.ToArray());
        }
    }
}