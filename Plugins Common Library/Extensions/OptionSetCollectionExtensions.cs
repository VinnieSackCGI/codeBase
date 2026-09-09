using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Plugins_CommonLibrary.Extensions
{
    public static class OptionSetCollectionExtensions
    {
        public static IEnumerable<T> ParseOptionSetCollection<T>(this object objectSetCollectionToParse) where T : Enum
        {
            OptionSetValueCollection optionSetValueCollection = (OptionSetValueCollection)objectSetCollectionToParse;
            ICollection<T> optionSetValues = new List<T>();
            if (optionSetValueCollection != null)
            {
                foreach (var optionSetKeyValuePair in optionSetValueCollection)
                {
                    optionSetValues.Add((T)Enum.Parse(typeof(T), Enum.GetName(typeof(T), optionSetKeyValuePair.Value)));
                }
            }
            return optionSetValues;
        }
    }
}
