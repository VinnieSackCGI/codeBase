using Microsoft.Xrm.Sdk.Query;

namespace Plugins_CommonLibrary.Extensions
{
	public static class QueryExpressionExtensions
	{
		public static QueryExpression First(this QueryExpression query)
		{
			query.TopCount = 1;
			return query;
		}

		public static QueryExpression NoLock(this QueryExpression query)
		{
			query.NoLock = true;
			return query;
		}

		public static QueryExpression ColumnSet(this QueryExpression query, ColumnSet columnSet)
		{
			query.ColumnSet = columnSet;
			return query;
		}

		public static QueryExpression Criteria(this QueryExpression query, FilterExpression filterExpression)
		{
			query.Criteria = filterExpression;
			return query;
		}
	}
}