namespace Our.Umbraco.AzureLogger.Core.Extensions
{
    using System.Linq;
    using System.Collections.Generic;

    public static class IEnumerableExtensions
    {
        /// <summary>
        /// http://stackoverflow.com/questions/13731796/create-batches-in-linq
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="maxItems"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> items, int maxItems)
        {
            return items.Select((item, index) => new { item, index })
                        .GroupBy(x => x.index / maxItems)
                        .Select(x => x.Select(y => y.item));
        }
    }
}