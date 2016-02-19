using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Text;
using LDAPLibrary.Interfarces;

namespace LDAPLibrary
{
	using System.Linq;
	using Enums;
	using StaticClasses;

	public class LdapSearcher : ILdapSearcher
	{
		private ILogger _logger;
		private LdapConnection _ldapConnection;
		private ILdapConfigRepository _configRepository;
	    private readonly IEnumerable<string> _baseAttributes;


		public LdapSearcher(ILdapConfigRepository configRepository, ILogger logger)
		{
			_logger = logger;
		    _configRepository = configRepository;
		    _baseAttributes = new List<string> {"cn", "sn"};
		}

        private LdapState BaseLdapSearch(IList<string> otherReturnedAttributes, out IList<ILdapUser> searchResult, string searchFilter)
        {
            searchResult = new List<ILdapUser>();
            otherReturnedAttributes = otherReturnedAttributes == null ? _baseAttributes.ToList() : _baseAttributes.Union(otherReturnedAttributes).ToList();
            try
            {
                searchResult = LdapUserUtils.ConvertToLdapUsers((SearchResponse)_ldapConnection.SendRequest(
                    LdapRequestBuilder.GetSearchUserRequest(_configRepository.GetSearchBaseDn(),
                        searchFilter, otherReturnedAttributes
                        )));
            }
            catch (Exception e)
            {
                _logger.Write(_logger.BuildLogMessage(e.Message, LdapState.LdapSearchUserError));
                return LdapState.LdapSearchUserError;
            }
            if (searchResult.Count == 0)
            {
                _logger.Write(_logger.BuildLogMessage("Search Operation with NO RESULTS", LdapState.LdapSearchUserError));
                return LdapState.LdapSearchUserError;
            }
            _logger.Write(_logger.BuildLogMessage("Search Operation Success", LdapState.LdapUserManipulatorSuccess));
            return LdapState.LdapUserManipulatorSuccess;
        }

        public void SetLdapConnection(LdapConnection ldapConnection)
        {
            _ldapConnection = ldapConnection;
        }

		public LdapState SearchUsers(IList<string> otherReturnedAttributes, string[] searchedUsers, out IList<ILdapUser> searchResult)
		{
			searchResult = new List<ILdapUser>();
            otherReturnedAttributes = otherReturnedAttributes == null ? _baseAttributes.ToList() : _baseAttributes.Union(otherReturnedAttributes).ToList();
			try
			{
			    //Foreach all the credential,for everyone do the search and add user results to the out parameter
				searchResult = searchedUsers.Select(
					users =>
						(SearchResponse)_ldapConnection.SendRequest(
							LdapRequestBuilder.GetSearchUserRequest(_configRepository.GetSearchBaseDn(),
								LdapFilterBuilder.GetSearchFilter(_configRepository.GetUserObjectClass(),
									_configRepository.GetMatchFieldName(), users), otherReturnedAttributes)
							))
					.Aggregate(searchResult,
						(current, searchReturn) =>
							current.Concat(LdapUserUtils.ConvertToLdapUsers(searchReturn)).ToList());
			}
			catch (Exception e)
			{
				_logger.Write(_logger.BuildLogMessage(e.Message, LdapState.LdapSearchUserError));
				return LdapState.LdapSearchUserError;
			}

			if (searchResult.Count == 0)
			{
				_logger.Write(_logger.BuildLogMessage("Search Operation with NO RESULTS", LdapState.LdapSearchUserError));
				return LdapState.LdapSearchUserError;
			}
			_logger.Write(_logger.BuildLogMessage("Search Operation Success", LdapState.LdapUserManipulatorSuccess));
			return LdapState.LdapUserManipulatorSuccess;
		}

		public LdapState SearchUsers(IList<string> otherReturnedAttributes, out IList<ILdapUser> searchResult)
		{
            return BaseLdapSearch(otherReturnedAttributes, out searchResult, LdapFilterBuilder.GetSearchFilter(_configRepository.GetUserObjectClass()));
		}
        
	    public LdapState SearchAllNodes(IList<string> otherReturnedAttributes, out IList<ILdapUser> searchResult)
		{
            return BaseLdapSearch(otherReturnedAttributes, out searchResult, LdapFilterBuilder.GetSearchFilter());
		}

		public void Dispose()
		{
			_configRepository = null;
			_logger = null;
			_ldapConnection.Dispose();
		}
	}
}
