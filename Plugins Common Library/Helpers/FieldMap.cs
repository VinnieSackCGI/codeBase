namespace Plugins_CommonLibrary.Helpers
{
	public class FieldMap
	{
		public FieldMap(string sourceField, string targetField)
		{
			Guard.AgainstNull(sourceField, nameof(sourceField));
			Guard.AgainstEmptyOrWhitespace(sourceField, nameof(sourceField));

			Guard.AgainstNull(targetField, nameof(targetField));
			Guard.AgainstEmptyOrWhitespace(targetField, nameof(targetField));

			this.SourceField = sourceField;
			this.TargetField = targetField;
		}

		public string SourceField { get; }

		public string TargetField { get; }
	}
}