using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;
using Plugins_CommonLibrary.Extensions;

namespace EXConnect_Plugins.Common
{
	public static class TransactionValidator
	{
		/// <summary>
		/// Verify the required keys of the transaction record are present
		/// </summary>
		/// <param name="transaction"></param>
		/// <param name="tracer"></param>
		/// <returns name="error" type="string"></returns>
		public static string KeyValidation( ICARTtransactionRecord transaction, IRepository dbService, ITracingService tracer )
		{
			// Make sure all the required keys are present
			var error = string.Empty;
			if (transaction.Appropriation == null)
			{
				error += "Appropriation";
			}
			if (transaction.CUFFaccount == null)
			{
				if (!string.IsNullOrEmpty(error))
				{
					error += ", ";
				}
				error += "CUFF Account";
			}

			if ((transaction.IsProgramTransfer || transaction.IsAOA) && transaction.FromCUFFaccount == null)
			{
				// If an AOA, set the From CUFF Account as the parent of the given CUFF Account, if any
				bool gotFromAccount = false;
				if ( transaction.IsAOA && transaction.CUFFaccount != null)
				{
					var parentAccount = dbService.CUFFaccount.GetRecordFromId(transaction.CUFFaccount.Id)?.ParentAccount;
					if (parentAccount != null)
					{
						transaction.FromCUFFaccount = parentAccount;
						gotFromAccount = true;
					}
				}
				
				if (!gotFromAccount)
				{
					if (!string.IsNullOrEmpty(error))
					{
						error += ", ";
					}
					error += "From CUFF Account";
				}
			}

			if (transaction.FundingType == null)
			{
				if (!string.IsNullOrEmpty(error))
				{
					error += ", ";
				}
				error += "Funding Type";
			}
			if (transaction.IsObligation && transaction.ObligationType == null)
			{
				if (!string.IsNullOrEmpty(error))
				{
					error += ", ";
				}
				error += "Obligation Type";
			}
			if (transaction.IsAOA)
			{
				if (string.IsNullOrEmpty(transaction.IBIS_RequestCode))
				{
					if (!string.IsNullOrEmpty(error))
					{
						error += ", ";
					}
					error += "IBIS Request Code";
				}
				if (string.IsNullOrEmpty(transaction.Description))
				{
					if (!string.IsNullOrEmpty(error))
					{
						error += ", ";
					}
					error += "Description";
				}
			}


			if (transaction.FiscalYear == null)
			{
				transaction.FiscalYear = DateTime.Now.FiscalYear();
				tracer.Trace($"The transaction Fiscal Year is missing. Set to {transaction.FiscalYear}.");
			}

			// Construct the error message to return
			var errMsg = string.Empty;

			if (!string.IsNullOrEmpty(error))
			{
				errMsg = $"The following field(s) need to be supplied: {error}.";
			}

			// Verify the Funding Source, Funding Type, and Appropriation combination is correct
			if (transaction.Appropriation != null && transaction.FundingSource != null && transaction.FundingType != null)
			{
				// Search the Funding Source table 
				// Compare the Funding Source, Type, and Appropriation against the Funding Source table settings
				// to get the proper reference
				var fundingSrcList = dbService.FundingSource.GetAppropriationFundingDataList();

				var fundRec = fundingSrcList.Where(f => f.FundingSrcId == transaction.FundingSource?.Id
													&& f.AppropriationId == transaction.Appropriation?.Id
													&& f.FundingTypeId == transaction.FundingType?.Id)
											.FirstOrDefault();
				if (fundRec == null)
				{
					if (!string.IsNullOrEmpty(errMsg))
					{
						errMsg += " \n";
					}
					errMsg += "The Funding Source, Funding Type, and Appropriation combination is invalid.";
				}
			}

			return errMsg;
		}

		/// <summary>
		/// Verify that the given transaction has been approved, or set up as approved if
		/// the current user can auto-approve it.
		/// </summary>
		/// <param name="transaction"></param>
		public static bool CanProceedWithValidation( ICARTtransactionRecord transaction, IRepository dbService, ITracingService tracer )
		{
			// Check that the transaction type is a Spending transaction

			if (transaction.IsApproved)
			{
				return true;
			}

			// Verify the user can auto-approve the transaction
			if (dbService.Transaction.CanBeAutoApproved(transaction))
			{
				transaction.InitialStatus = "Approved";
				return true;
			}

			tracer.Trace($"The current user with id = \"{dbService.UserId}\" cannot approve this transaction.");
			transaction.InitialStatus = "Pending";
			return false;
		}

		/// <summary>
		/// Verify the required keys of the transaction record are present
		/// </summary>
		/// <param name="transaction"></param>
		/// <param name="tracer"></param>
		/// <returns name="error" type="string"></returns>
		public static string TransactionKeyValidation( ICARTtransactionRecord transaction, IRepository dbService, ITracingService tracer )
		{
			// Make sure all the required keys are present
			var error = string.Empty;
			if (transaction.Appropriation == null)
			{
				error += "Appropriation";
			}
			if (transaction.CUFFaccount == null)
			{
				if (!string.IsNullOrEmpty(error))
				{
					error += ", ";
				}
				error += "CUFF Account";
			}

			if ((transaction.IsProgramTransfer || transaction.IsAOA) && transaction.FromCUFFaccount == null)
			{
				// If an AOA, set the From CUFF Account as the parent of the given CUFF Account, if any
				bool gotFromAccount = false;
				if (transaction.IsAOA && transaction.CUFFaccount != null)
				{
					var parentAccount = dbService.CUFFaccount.GetRecordFromId(transaction.CUFFaccount.Id)?.ParentAccount;
					if (parentAccount != null)
					{
						transaction.FromCUFFaccount = parentAccount;
						gotFromAccount = true;
					}
				}

				if (!gotFromAccount)
				{
					if (!string.IsNullOrEmpty(error))
					{
						error += ", ";
					}
					error += "From CUFF Account";
				}
			}

			if (transaction.FundingType == null)
			{
				if (!string.IsNullOrEmpty(error))
				{
					error += ", ";
				}
				error += "Funding Type";
			}
			if (transaction.IsObligation && transaction.ObligationType == null)
			{
				if (!string.IsNullOrEmpty(error))
				{
					error += ", ";
				}
				error += "Obligation Type";
			}
			if (transaction.IsAOA)
			{
				if (string.IsNullOrEmpty(transaction.IBIS_RequestCode))
				{
					if (!string.IsNullOrEmpty(error))
					{
						error += ", ";
					}
					error += "IBIS Request Code";
				}
				if (string.IsNullOrEmpty(transaction.Description))
				{
					if (!string.IsNullOrEmpty(error))
					{
						error += ", ";
					}
					error += "Description";
				}
			}


			if (transaction.FiscalYear == null)
			{
				transaction.FiscalYear = DateTime.Now.FiscalYear();
				tracer.Trace($"The transaction Fiscal Year is missing. Set to {transaction.FiscalYear}.");
			}

			// Construct the error message to return
			var errMsg = string.Empty;

			if (!string.IsNullOrEmpty(error))
			{
				errMsg = $"The following field(s) need to be supplied: {error}.";
			}

			// Verify the Funding Source, Funding Type, and Appropriation combination is correct
			if (transaction.Appropriation != null && transaction.FundingSource != null && transaction.FundingType != null)
			{
				// Search the Funding Source table 
				// Compare the Funding Source, Type, and Appropriation against the Funding Source table settings
				// to get the proper reference
				var fundingSrcList = dbService.FundingSource.GetAppropriationFundingDataList();

				var fundRec = fundingSrcList.Where(f => f.FundingSrcId == transaction.FundingSource?.Id
													&& f.AppropriationId == transaction.Appropriation?.Id
													&& f.FundingTypeId == transaction.FundingType?.Id)
											.FirstOrDefault();
				if (fundRec == null)
				{
					if (!string.IsNullOrEmpty(errMsg))
					{
						errMsg += " \n";
					}
					errMsg += "The Funding Source, Funding Type, and Appropriation combination is invalid.";
				}
			}

			return errMsg;
		}

	}

	public static class SpendPlanValidator
	{
		/// <summary>
		/// Verify that the given Spend Plan Request has the necessary information
		/// </summary>
		/// <param name="spendPlanRequest"></param>
		public static string KeyValidation( ISpendPlanRequestRecord spendRequest, IRepository dbService, ITracingService tracer )
		{
			// Make sure all the required keys are present
			var error = string.Empty;
			if (spendRequest.Appropriation == null)
			{
				error += "Appropriation";
			}
			if (spendRequest.CUFFaccount == null)
			{
				if (!string.IsNullOrEmpty(error))
				{
					error += ", ";
				}
				error += "CUFF Account";
			}

			if (spendRequest.FundingType == null)
			{
				if (!string.IsNullOrEmpty(error))
				{
					error += ", ";
				}
				error += "Funding Type";
			}

			if (string.IsNullOrEmpty(spendRequest.Justification))
			{
				if (!string.IsNullOrEmpty(error))
				{
					error += ", ";
				}
				error += "Justification";
			}

			if (string.IsNullOrEmpty(spendRequest.FiscalYear))
			{
				spendRequest.FiscalYear = DateTime.Now.FiscalYear().ToString();
				tracer.Trace($"The record's Fiscal Year is missing. Set to {spendRequest.FiscalYear}.");
			}

			// Construct the error message to return
			var errMsg = string.Empty;

			if (!string.IsNullOrEmpty(error))
			{
				errMsg = $"The following field(s) need to be supplied: {error}.";
			}

			return errMsg;
		}

	}
}
