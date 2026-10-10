using System.Linq.Expressions;
using Yash_Gems___Jewelleries.Models.Interfaces;

namespace Yash_Gems___Jewelleries.Repositories
{
    /// <summary>
    /// Generic repository interface for master data entities.
    /// Provides type-safe CRUD operations with concurrency control and validation.
    /// </summary>
    /// <typeparam name="TEntity">The master entity type that implements IMasterEntity</typeparam>
    public interface IMasterDataRepository<TEntity> where TEntity : class, IMasterEntity
    {
        /// <summary>
        /// Retrieves all entities with optional active status filtering
        /// </summary>
        /// <param name="isActive">Optional filter for active/inactive entities. Null returns all.</param>
        /// <param name="includes">Navigation properties to include in the query</param>
        /// <returns>Collection of entities matching the filter criteria</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(bool? isActive = null, params Expression<Func<TEntity, object>>[] includes);

        /// <summary>
        /// Retrieves a single entity by its unique identifier
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<TEntity?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new entity in the database
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>True if creation was successful, false otherwise</returns>
        Task<bool> CreateAsync(TEntity entity);

        /// <summary>
        /// Updates an existing entity with optimistic concurrency control
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <returns>True if update was successful, false if concurrency conflict occurred</returns>
        /// <exception cref="DbUpdateConcurrencyException">Thrown when a concurrency conflict is detected</exception>
        Task<bool> UpdateAsync(TEntity entity);

        /// <summary>
        /// Soft deletes an entity by setting IsActive to false
        /// </summary>
        /// <param name="id">The ID of the entity to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Checks if an entity with the specified ID exists
        /// </summary>
        /// <param name="id">The entity ID to check</param>
        /// <returns>True if entity exists, false otherwise</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Validates that a field value is unique across entities
        /// </summary>
        /// <param name="fieldName">The name of the field to check</param>
        /// <param name="value">The value to validate</param>
        /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
        /// <returns>True if the value already exists, false if it's unique</returns>
        Task<bool> UniqueFieldExistsAsync(string fieldName, string value, int? excludeId = null);
    }
}
