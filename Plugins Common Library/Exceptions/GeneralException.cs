using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Plugins_CommonLibrary.Exceptions
{
	[Serializable]
	[ExcludeFromCodeCoverage]
	public sealed class GeneralException : Exception
	{
		public GeneralException( string message) : base(message) { }

		public GeneralException( string message, Exception ex) : base(message, ex) { }

		private GeneralException( SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}