using System.Collections.Generic;

namespace QueryLogic.ORM
{
    /// <summary>
    /// Results of a sucessful search
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SearchResults<T>
    {
        /// <summary>
        /// Total results
        /// </summary>
        public int Matches { get; set; }

        /// <summary>
        /// Retrieved results (for paginated search)
        /// </summary>
        public IEnumerable<T> Results { get; set; }
    }
}