using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXConnect_Plugins.Entities.Interfaces;
using Microsoft.Xrm.Sdk;

namespace EXConnect_Plugins.Plugins.Common
{
    public class DocumentNumberGeneration
    {
        private IRepository dbService;
        private ITracingService tracer;
        private int GetPayPeriod(DateTime transactionDate)
        {
            //PP01 start date - only this needs updating each fiscal year
            var pp01Start = new DateTime(2026, 1, 12);
            var daysSincePP01 = (transactionDate.Date - pp01Start).Days;

            // Each pay period is 14 days
            var payPeriod = (daysSincePP01 / 14) + 1;

            // If before PP01, calculate backwards into previous cycle
            if (daysSincePP01 < 0)
            {
                payPeriod = 26 + (daysSincePP01 / 14);
            }

            // Keep within 1-26 range
            payPeriod = ((payPeriod - 1) % 26) + 1;

            return payPeriod;
        }

        public DocumentNumberGeneration(IRepository _dbService, ITracingService _tracer)
        {
            dbService = _dbService;
            tracer = _tracer;
        }

        public string GenerateObligationDocumentNumber(
            EntityReference allotmentRef,
            EntityReference obligationTypeRef,
            EntityReference postRef,
            string documentType,
            int fiscalYear,
            bool hasParentDocument)
        {
            if (allotmentRef == null)
            {
                tracer.Trace("Missing Allotment reference for this obligation. Document Number will not be generated.");
                return null;
            }

            var allotment = dbService.Allotment.GetCodeFromId(allotmentRef.Id);
            if (string.IsNullOrEmpty(allotment))
            {
                tracer.Trace("Allotment code is empty. Document Number will not be generated.");
                return null;
            }

            // Ensure obligation type name is hydrated
            if (obligationTypeRef != null && string.IsNullOrEmpty(obligationTypeRef.Name))
            {
                var ext = dbService.ObligationType.GetExtendedEntityReference(obligationTypeRef);
                if (ext != null && !string.IsNullOrEmpty(ext.Name))
                    obligationTypeRef = ext;
            }

            var obligationTypeName = obligationTypeRef?.Name ?? string.Empty;

            int countLimit = 9999;
            string numFormat = "D4";
            string codeString = string.Empty;

            // 1) M9 International Travel special case
            if (string.Equals(documentType, "M9", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(obligationTypeName, "International Travel", StringComparison.OrdinalIgnoreCase))
            {
                var postRecord = dbService.Post.GetRecordFromId(postRef.Id);
                if (postRecord == null) return null;

                codeString = postRecord.Region.Substring(0, 2);
                if (postRecord.Region == "SCA") codeString = "SA";

                countLimit = 99;
                numFormat = "D2";
            }
            // 2) EP salary mapping (GFACS1 etc.)
            else if (string.Equals(documentType, "EP", StringComparison.OrdinalIgnoreCase))
            {
                // REMOVE the " - EP" stripping block entirely

                var salaryMappings = new Dictionary<string, (string Code, int Limit, string Format)>(StringComparer.OrdinalIgnoreCase)
    {
                { "WCF - Salary and Benefits - GFACS1 - EP", ("GFACS1", 99, "D2") },
                { "WCF - Salary and Benefits - GFACS1",      ("GFACS1", 99, "D2") },
                { "WCF - Salary and Benefits - GFACS - EP",  ("GFACS",  99, "D2") },
                { "WCF - Salary and Benefits - GFACS",       ("GFACS",  99, "D2") },
                { "WCF - Salary and Benefits - FEE - EP",    ("FEE",    99, "D2") },
                { "WCF - Salary and Benefits - FEE",         ("FEE",    99, "D2") },
                { "WCF - Salary and Benefits - DHS - EP",    ("DHS",    99, "D2") },
                { "WCF - Salary and Benefits - DHS",         ("DHS",    99, "D2") },
                { "WCF - Salary and Benefits - ECE - EP",    ("SB",     99, "D2") },
                { "WCF - Salary and Benefits - ECE",         ("SB",     99, "D2") },
    };

                if (salaryMappings.TryGetValue(obligationTypeName, out var cfg))
                {
                    codeString = cfg.Code;
                    countLimit = cfg.Limit;
                    numFormat = cfg.Format;
                }
                else
                {
                    codeString = GetCharacterFromDocumentTypeCode(documentType);
                    numFormat = GetNumberFormat(codeString);
                    countLimit = (numFormat == "D3" ? 999 : 9999);
                }
            }
            // 3) default behavior
            else
            {
                codeString = GetCharacterFromDocumentTypeCode(documentType);
                numFormat = GetNumberFormat(codeString);
                countLimit = (numFormat == "D3" ? 999 : 9999);
            }

            // Find next number
            int nextNumber;
            if (hasParentDocument)
                nextNumber = dbService.GFMSdocument.GetHighestDocumentNumberByObligationCodeString(fiscalYear, codeString, countLimit);
            else
                nextNumber = dbService.Transaction.GetHighestDocumentNumberByObligationCodeString(fiscalYear, codeString, countLimit);

            // FORMAT: Allotment + FY(2 digits) + Increment + GFACS1 
            // Example: 107226 + 26 + GFACS1 + 01 => 1072262601GFACS1

            var fy2 = fiscalYear.ToString().Substring(2, 2);

            // Some environments store allotment already including FY (107226).
            // Only append FY if it's not already present to prevent "10722626"
            var prefix = allotment.EndsWith(fy2, StringComparison.OrdinalIgnoreCase)
                ? allotment
                : (allotment + fy2);

            // FINAL FORMAT: 1072 + 26 + 01 + GFACS1   => 10722601GFACS1
            // Salary types use pay period, all others use incrementing number
            var salaryCodes = new HashSet<string> { "GFACS1", "GFACS", "FEE", "DHS", "SB" };

            string documentNumber;
            if (salaryCodes.Contains(codeString))
            {
                var payPeriod = GetPayPeriod(DateTime.UtcNow);
                documentNumber =
                    prefix
                    + payPeriod.ToString("D2")
                    + codeString;
            }
            else
            {
                // Original format: allotment + FY + codeString + number
                documentNumber =
                    prefix
                    + codeString
                    + nextNumber.ToString(numFormat);
            }
            return documentNumber;
        }

        public string GenerateCommitmentDocumentNumber(EntityReference allotmentRef, EntityReference appropriationRef, string documentType,
        int fiscalYear, bool hasParentDocument)
        {
            // Get the allotment for this transaction
            if (allotmentRef == null)
            {
                tracer.Trace("Missing Allotment reference for this Commitment. Document Number will not be generated.");
                return null;
            }

            var allotment = dbService.Allotment.GetCodeFromId(allotmentRef.Id);

            var startCode = string.Empty;
            var endCode = string.Empty;
            var digits = 3;

            if (documentType == "AQ")
            {
                // Select the appropriate Document Number code based on the Appropriation
                var appropCode = dbService.Appropriation.GetCodeFromId(appropriationRef.Id);

                if (appropCode.Contains("X0209000"))
                {
                    startCode = "AQ";
                    endCode = "A";
                    if (appropCode.EndsWith("R"))
                    {
                        endCode = "R";
                    }
                }
            }
            else if (documentType == "GIQ")
            {
                startCode = "Q";
                endCode = string.Empty;
                digits = 4;
            }
            else
            {
                return null;
            }

            // Get the highest number from the table for the associated code
            var countLimit = (int)Math.Pow(10, digits) - 1;

            var nextNumber = 0;
            if (hasParentDocument)
            {
                nextNumber = dbService.GFMSdocument.GetHighestDocumentNumberByCommitmentCodeString(fiscalYear, startCode, countLimit);
            }
            else
            {
                nextNumber = dbService.Transaction.GetHighestDocumentNumberByCommitmentCodeString(fiscalYear, startCode, countLimit);
            }

            var documentNumber = allotment
            + fiscalYear.ToString().Substring(2, 2)
            + startCode
            + nextNumber.ToString($"D{digits}")
            + endCode;

            return documentNumber;
        }

        #region Private Methods

        private string GetCharacterFromDocumentTypeCode(string transCode)
        {
            switch (transCode)
            {
                case "TO":
                    return "T";
                case "M9":
                    return "M";
                case "D9":
                    return "D";
                case "F9":
                    return "F";
                case "GIO/GIQ":
                    return "G/Q";
                case "GIO":
                    return "G";
                case "GIQ":
                case "76B":
                    return "Q";
                default:
                    return "A";


            }
        }

        private string GetNumberFormat(string codeString)
        {
            switch (codeString)
            {
                case "D":
                case "F":
                    return "D3";
                default:
                    return "D4";
            }
        }

        #endregion

    }
}
