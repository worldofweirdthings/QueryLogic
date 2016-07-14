using System.Runtime.Serialization;

namespace QueryLogic.ORM
{
    /// <summary>
    /// Search term container
    /// </summary>
    public class SearchTerm
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SearchTerm() { }

        /// <summary>
        /// Parameterized constructor for syntactic brevity.
        /// </summary>
        /// <param name="isOptional">Whether the term is required (and) or optional (or)</param>
        /// <param name="searchObject">Name of the search object</param>
        /// <param name="searchOperator">Value of the search operator</param>
        /// <param name="objectValue">Conditional value of the search object</param>
        public SearchTerm(bool isOptional, string searchObject, string searchOperator, string objectValue)
        {
            SearchObject = searchObject;
            SearchOperator = searchOperator;
            ObjectValue = objectValue;
            Condition = isOptional ? "or" : "and";
        }

        /// <summary>
        /// Whether the term is required (and) or optional (or)
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// Name of the property used in the search clause
        /// </summary>
        public string SearchObject { get; set; }
        
        /// <summary>
        /// Search operator for building the where clause
        /// </summary>
        public string SearchOperator { get; set; }
        
        /// <summary>
        /// Search term value
        /// </summary>
        public string ObjectValue { get; set; }
    }
}