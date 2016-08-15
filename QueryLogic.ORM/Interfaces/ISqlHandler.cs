using System.Collections.Generic;

namespace QueryLogic.ORM
{
    /// <summary>
    /// Specifies ORM methods
    /// </summary>
    public interface ISqlHandler
    {
        /// <summary>
        /// Find objects in a database meeting specified criteria
        /// </summary>
        /// <typeparam name="T">Type of requested object</typeparam>
        /// <param name="search">Search criteria</param>
        /// <returns>Collection of requested objects</returns>
        SearchResults<T> Find<T>(Search search);
        
        /// <summary>
        /// Returns all objects of the requested type from a database
        /// </summary>
        /// <typeparam name="T">Type of requested object</typeparam>
        /// <returns>Collection of requested objects</returns>
        IEnumerable<T> List<T>();

        /// <summary>
        /// Returns all objects of the requested type from a database
        /// in subsets intended for a paginated grid or list of data
        /// </summary>
        /// <typeparam name="T">Type of requested object</typeparam>
        /// <returns>Collection of requested objects</returns>
        /// <param name="offset">current page in zero-based format</param>
        /// <param name="rows">number of rows to fetch for the list</param>
        IEnumerable<T> List<T>(int offset, int rows);

        /// <summary>
        /// Retrieves an object from a database based on the provided id
        /// </summary>
        /// <typeparam name="T">Type of requested object</typeparam>
        /// <param name="id">Id of the requested object</param>
        /// <returns>Requested object</returns>
        T Get<T>(object id);
        
        /// <summary>
        /// Retrieves objects from a database based on the provided ids
        /// </summary>
        /// <typeparam name="T">Type of requested object</typeparam>
        /// <param name="ids">Ids of the requested objects</param>
        /// <returns>Collection of requested objects</returns>
        IEnumerable<T> Get<T>(List<object> ids);
        
        /// <summary>
        /// Saves an instance of an object in a specified database
        /// </summary>
        /// <param name="entity">Object instance</param>
        /// <returns>Populated object</returns>
        object Create(object entity);

        /// <summary>
        /// Saves instances of objects in a specified database as a batch transaction
        /// </summary>
        /// <param name="entities"></param>
        /// <returns>Populated objects</returns>
        IEnumerable<object> Create(IEnumerable<object> entities);

        /// <summary>
        /// Updates an instance of an object in a specified database
        /// </summary>
        /// <param name="entity">Object instances</param>
        void Update(object entity);

        /// <summary>
        /// Updates instances of objects in a specified database as a batch transaction
        /// </summary>
        /// <param name="entities">Object instances</param>
        void Update(IEnumerable<object> entities);

        /// <summary>
        /// Deletes an instance of an object in a specified database
        /// </summary>
        /// <param name="entity">Object instance</param>
        void Delete(object entity);
    }
}