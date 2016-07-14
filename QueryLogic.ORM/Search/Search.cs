using System.Collections.Generic;

namespace QueryLogic.ORM
{
    /// <summary>
    /// Search criteria container.
    /// </summary>
    public class Search
    {
        /// <summary>
        /// Default constructor, creates an empty search term container
        /// </summary>
        public Search()
        {
            SearchTerms = new List<SearchTerm>();
        }

        /// <summary>
        /// Consrtucts and adds a search term to the search
        /// </summary>
        /// <param name="searchObject">Name of the search object</param>
        /// <param name="searchOperator">Value of the search operator</param>
        /// <param name="objectValue">Conditional value of the search object</param>
        /// <param name="isOptional">Whether the term is required (and) or optional (or), default value is required</param>
        public void Add(string searchObject, string searchOperator, string objectValue, bool isOptional = false)
        {
            SearchTerms.Add(new SearchTerm(isOptional, searchObject, searchOperator, objectValue));
        }
        
        /// <summary>
        /// Collection of search criteria.
        /// </summary>
        public List<SearchTerm> SearchTerms { get; set; }

        /// <summary>
        /// Name of the property by which the results should be ordered
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// Number of rows to skip when returning results
        /// </summary>
        public int? Offset { get; set; }

        /// <summary>
        /// Number of rows to retrieve when returning results
        /// </summary>
        public int? Rows { get; set; }

        /// <summary>
        /// Sort direction of the results
        /// </summary>
        public string SortOrder { get; set; }
    }
}