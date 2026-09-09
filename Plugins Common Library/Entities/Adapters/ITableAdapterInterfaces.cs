using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugins_CommonLibrary.Entities.Interfaces;

namespace Plugins_CommonLibrary.Entities.Adapters.Interfaces
{
	public interface ITableAdapter<T>
	{
		T CreateRecord();
		IEnumerable<T> GetAllRecords();
        //int GetHighestDocumentNumberByObligationCodeString(int fiscalYear, string codeString, int countLimit);
        T GetRecordFromEntity(Entity entity);
	}

	public interface IKeyTableAdapter
	{
		string GetCodeFromId(Guid id);
		Guid? GetIdFromCode(string code);
		EntityReference GetEntityReferenceFromCode(string code);
		EntityReference GetEntityReferenceFromId(Guid id);
	}

	public interface IUserTableAdapter
	{
		string GetEmailFromId(Guid id);
		EntityReference GetEntityReferenceFromEmail(string email);
	}

	public interface ISystemUserAdapter : IUserTableAdapter, IKeyTableAdapter
	{
		bool IsUserActive(Guid userId);
		bool IsUserInRole( Guid userId, string roleName );
	}

	public interface ITeamAdapter : ITableAdapter<ITeamRecord>
	{
		ITeamRecord GetRecordFromId( Guid id );
		ITeamRecord GetRecordFromName( string name );
	}

	public interface IEnvironmentVariablesAdapter
	{
		string GetValue( string variableName );
	}

}
