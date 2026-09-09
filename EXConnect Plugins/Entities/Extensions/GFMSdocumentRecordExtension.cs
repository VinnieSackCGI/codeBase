using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Entities
{
	partial class eca_GFMSDocument : IGFMSdocumentRecord
	{
		#region Interrogation Properties

		public bool IsActive
		{
			get => StateCode == eca_GFMSDocumentState.Active;
		}

		public bool IsApproved
		{
			get => StatusCode == eca_GFMSDocument_StatusCode.Approved;
		}

		public bool IsCommitment
		{
			get => eca_ECATransactionType == eca_ECATransactionTypes.Commitment;
		}

		public bool IsObligation
		{
			get => eca_ECATransactionType == eca_ECATransactionTypes.Obligation;
		}

		#endregion

		#region Data Properties

		public string Name
		{
			get => eca_DocumentTitle;

			set
			{
				eca_DocumentTitle = value;
			}
		}

		public string DocumentNumber
		{
			get => eca_DocumentNumber;
			set => eca_DocumentNumber = value;
		}

		public EntityReference ObligationType
		{
			get => eca_ObligationType;
			set => eca_ObligationType = value;
		}

		public string TransactionType
		{
			get => this.eca_ECATransactionType.ToString();
			set
			{
				eca_ECATransactionTypes type;

				if (Enum.TryParse<eca_ECATransactionTypes>(value, out type))
				{
					eca_ECATransactionType = type;
				}
				else
				{
					eca_ECATransactionType = null;
				}
			}
		}

		public string DocumentType
		{
			get => eca_GFMSDocumentType.ToString();
			set
			{
				rcade_transcode type;

				if (Enum.TryParse<rcade_transcode>(value, out type))
				{
					eca_GFMSDocumentType = type;
				}
				else
				{
					eca_GFMSDocumentType = null;
				}
			}
		}


		public bool NeedsValidation
		{
			get => eca_NeedsValidation.GetValueOrDefault(false);
			set => eca_NeedsValidation = value;
		}

		public string Status
		{
			get => StatusCode.ToString();
		
			set
			{
				eca_GFMSDocument_StatusCode status;
				if (Enum.TryParse<eca_GFMSDocument_StatusCode>(value, out status))
				{
					StatusCode = status;
				}
				else
				{
					StatusCode = null;
				}
			}
		}


		public Entity Entity
		{
			get
			{
				return this;
			}
		}

		#endregion

		#region Public Methods
		#endregion

	}
}
