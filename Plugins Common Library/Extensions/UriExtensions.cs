using System;
using System.Linq;

namespace Plugins_CommonLibrary.Extensions
{
	public static class UriExtensions
	{
		public static Uri Append(this Uri uri, params string[] segments)
		{
			return new Uri(segments.Aggregate(uri.IsAbsoluteUri ? uri.AbsoluteUri : uri.ToString(), (current, path) => $"{current.TrimEnd('/')}/{path.TrimStart('/')}"), uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
		}

		public static Uri TrimLastSegment(this Uri uri)
		{
			Uri trimmedLastSegment = new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped), uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);

			trimmedLastSegment.Append(uri.Segments.Take(uri.Segments.Length - 1).ToArray());

			return trimmedLastSegment;
		}
	}
}