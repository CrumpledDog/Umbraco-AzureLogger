namespace Our.Umbraco.AzureLogger.Core.Extensions
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// http://stackoverflow.com/questions/18549555/multiple-filter-conditions-azure-table-storage
    /// </summary>
    public static class TableQueryExtensions
    {
        public static TableQuery<TElement> AndWhere<TElement>(this TableQuery<TElement> tableQuery, string filterString)
        {
            // safety check - is there a filter to add ?
            if (!string.IsNullOrWhiteSpace(filterString))
            {
                // safety check - is there an existing filter to add too
                if (string.IsNullOrWhiteSpace(tableQuery.FilterString))
                {
                    // no exisiting filter found, so set without add
                    tableQuery.FilterString = filterString;
                }
                else
                {
                    // add to existing filter
                    tableQuery.FilterString = TableQuery.CombineFilters(tableQuery.FilterString, TableOperators.And, filterString);
                }
            }

            return tableQuery;
        }

        //public static TableQuery<TElement> OrWhere<TElement>(this TableQuery<TElement> @this, string filter)
        //{
        //    @this.FilterString = TableQuery.CombineFilters(@this.FilterString, TableOperators.Or, filter);
        //    return @this;
        //}

        //public static TableQuery<TElement> NotWhere<TElement>(this TableQuery<TElement> @this, string filter)
        //{
        //    @this.FilterString = TableQuery.CombineFilters(@this.FilterString, TableOperators.Not, filter);
        //    return @this;
        //}
    }
}
