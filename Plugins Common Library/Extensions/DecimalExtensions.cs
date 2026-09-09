namespace Plugins_CommonLibrary.Extensions
{
    public static class DecimalExtensions
    {
        /// <summary>
        /// Returns decimal in US currency format.
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The currency formatted value</returns>
        public static string DecimalAsCurrencyString(this decimal value)
        {
            return value.ToString("C");
        }
    }
}
