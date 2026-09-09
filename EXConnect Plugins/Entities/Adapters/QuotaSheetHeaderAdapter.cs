using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EXConnect_Plugins.Entities.Adapters
{
	public class QuotaSheetHeaderAdapter : IQuotaSheetHeaderAdapter
	{
		IRepository dbService;
		EXConnect_ServiceContext serviceContext;

		public QuotaSheetHeaderAdapter( IRepository dbService )
		{
			this.dbService = dbService;
			this.serviceContext = dbService.OrgServiceContext as EXConnect_ServiceContext;
		}

		public IQuotaSheetHeaderRecord CreateRecord()
		{
			return new eca_QuotaSheetHeader();
		}

		public IEnumerable<IQuotaSheetHeaderRecord> GetAllRecords()
		{
			return serviceContext.eca_QuotaSheetHeaderSet as IEnumerable<IQuotaSheetHeaderRecord>;
		}

		public IQuotaSheetHeaderRecord GetRecordFromId( Guid id )
		{
			return serviceContext.eca_QuotaSheetHeaderSet.Where(h => h.Id == id && h.IsActive).FirstOrDefault();
		}

		public EntityReference GetEntityReferenceFromId( Guid id )
		{
			return new EntityReference("eca_quotasheetheader", id);
		}

		public IQuotaSheetHeaderRecord GetRecordFromEntity( Entity entity )
		{
			return entity.ToEntity<eca_QuotaSheetHeader>();
		}

		public string GetMatchingGranteeTypeFromString( string strValue )
		{
			var granteeType = string.Empty;

			var upperStrValue = strValue.ToUpper().Replace(" ","");

			foreach (var type in Enum.GetValues(typeof(eca_GranteeType)))
			{
				if (upperStrValue == type.ToString().ToUpper())
				{
					granteeType = type.ToString();
					break;
				}
			}

			return granteeType;
		}

		public string GetMatchingRevisionTypeFromString( string strValue )
		{
			var revisionType = string.Empty;

			var upperStrValue = strValue.ToUpper().Replace(" ","");

			foreach (var type in Enum.GetValues(typeof(eca_quotasheetversions)))
			{
				if (upperStrValue.Contains(type.ToString().ToUpper()))
				{
					revisionType = type.ToString();
					break;
				}
			}

			return revisionType;
		}

        public string GetMatchingRevisionNameFromType(string typeValue)
        {
			eca_quotasheetversions enumValue;
			if (Enum.TryParse<eca_quotasheetversions>(typeValue, out enumValue))
			{
                var field = enumValue.GetType().GetField(typeValue);
                var attributes = field.GetCustomAttributes<OptionSetMetadataAttribute>(false);

                var match = attributes.FirstOrDefault();
				if (match != null)
				{
					return match.Name;
				}
            }
			return typeValue;
        }

		public GranteeType? GetGranteeTypeEnumFromString( string granteeType )
		{
			GranteeType enumValue;
			if (Enum.TryParse<GranteeType>(granteeType, out enumValue))
			{
				return enumValue;
			}
			return null;
		}
	}
}
