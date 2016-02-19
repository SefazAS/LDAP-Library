namespace LDAPLibrary.Factories
{
    using Interfarces;

    internal static class LdapSearcherFactory
    {
        public static ILdapSearcher GetSearcher(ILdapConnector connector, ILogger logger, ILdapConfigRepository configRepository)
        {
            var searcher = new LdapSearcher(configRepository, logger);
            connector.LdapConnectionSubscribe(searcher);
            return searcher;
        }
    }
}