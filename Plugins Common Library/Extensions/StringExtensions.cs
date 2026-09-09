using System.Globalization;

namespace Plugins_CommonLibrary.Extensions
{
    public static class StringExtensions
	{
		public static string Truncate(this string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength ? value : value.Substring(0, maxLength);
		}

		/// <summary>
		/// Returns true if the string begins with a vowel. False otherwise
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool BeginsWithVowel(this string value)
        {
			return value.StartsWith("A", ignoreCase: true, CultureInfo.InvariantCulture)
				  || value.StartsWith("E", ignoreCase: true, CultureInfo.InvariantCulture)
				  || value.StartsWith("I", ignoreCase: true, CultureInfo.InvariantCulture)
				  || value.StartsWith("O", ignoreCase: true, CultureInfo.InvariantCulture)
				  || value.StartsWith("U", ignoreCase: true, CultureInfo.InvariantCulture);

		}

		/// <summary>
		/// A function that takes in the following word and determines if the word to follow begins with a noun.
		/// Returns the appropriate Indefinite Article (A/An) based on the first letter of the following string.
		/// </summary>
		/// <param name="stringValueToFollowThisString">The word to follow the indefinite article, whichever is returned</param>
		/// <param name="beginningOfSentence">Indicates if it's the beginning of sentence. If so, will capitalize the first letter of the indefinite article.</param>
		/// <returns>A or An</returns>
		public static string GetIndefiniteArticle(string stringValueToFollowThisString, bool beginningOfSentence)
        {
            if (stringValueToFollowThisString.BeginsWithVowel())
            {
				return beginningOfSentence ? "An" : "an";
            }
            else
            {
				return beginningOfSentence ? "A" : "a";
            }
        }
	}
}