using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yash_Gems___Jewelleries.Data;
using Yash_Gems___Jewelleries.Models.Interfaces;

namespace Yash_Gems___Jewelleries.Repositories
{
    /// <summary>
    /// Generic repository implementation for master data entities.
    /// Provides optimized, thread-safe CRUD operations with comprehensive error handling.
    /// </summary>
    /// <typeparam name="TEntity">The master entity type that implements IMasterEntity</typeparam>
    public class MasterDataRepository<TEntity> : IMasterDataRepository<TEntity> where TEntity : class, IMasterEntity
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private readonly ILogger<MasterDataRepository<TEntity>> _logger;

        public MasterDataRepository(
            ApplicationDbContext context,
            ILogger<MasterDataRepository<TEntity>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbSet = _context.Set<TEntity>();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TEntity>> GetAllAsync(bool? isActive = null, params Expression<Func<TEntity, object>>[] includes)
        {
            try
            {
                IQueryable<TEntity> query = _dbSet.AsNoTracking();

                if (isActive.HasValue)
                {
                    query = query.Where(e => e.IsActive == isActive.Value);
                }

                if (includes != null)
                {
                    foreach (var include in includes)
                    {
                        query = query.Include(include);
                    }
                }

                var entities = await query.ToListAsync();
                
                _logger.LogInformation(
                    "Retrieved {Count} {EntityType} entities with isActive filter: {IsActive}",
                    entities.Count,
                    typeof(TEntity).Name,
                    isActive?.ToString() ?? "null");

                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving {EntityType} entities", typeof(TEntity).Name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TEntity?> GetByIdAsync(int id)
        {
            try
            {
                var keyName = _context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties.Select(x => x.Name).Single();
                if (keyName == null) throw new InvalidOperationException("Primary key not found for entity.");

                var entity = await _dbSet.AsNoTracking()
                    .FirstOrDefaultAsync(e => EF.Property<int>(e, keyName) == id);

                if (entity == null)
                {
                    _logger.LogWarning(
                        "{EntityType} with ID {Id} not found using key {KeyName}",
                        typeof(TEntity).Name,
                        id,
                        keyName);
                }

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving {EntityType} with ID {Id}", typeof(TEntity).Name, id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CreateAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                entity.CreatedDate = DateTime.UtcNow;
                entity.ModifiedDate = null;

                await _dbSet.AddAsync(entity);
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Created new {EntityType} with ID {Id}",
                    typeof(TEntity).Name,
                    entity.Id);

                return result > 0;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating {EntityType}", typeof(TEntity).Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                entity.ModifiedDate = DateTime.UtcNow;

                _dbSet.Update(entity);
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated {EntityType} with ID {Id}",
                    typeof(TEntity).Name,
                    entity.Id);

                return result > 0;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Concurrency conflict updating {EntityType} with ID {Id}",
                    typeof(TEntity).Name,
                    entity.Id);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var keyName = _context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties.Select(x => x.Name).Single();
                if (keyName == null) throw new InvalidOperationException("Primary key not found for entity.");

                var entity = await _dbSet.FirstOrDefaultAsync(e => EF.Property<int>(e, keyName) == id);
                
                if (entity == null)
                {
                    _logger.LogWarning(
                        "Cannot delete {EntityType} with ID {Id} - not found",
                        typeof(TEntity).Name,
                        id);
                    return false;
                }

                _dbSet.Remove(entity);
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Hard deleted {EntityType} with ID {Id}",
                    typeof(TEntity).Name,
                    id);

                return result > 0;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting {EntityType} with ID {Id}. This is often due to foreign key constraints.", typeof(TEntity).Name, id);
                throw new InvalidOperationException($"Cannot delete this {typeof(TEntity).Name} because it is referenced by other items in the system.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityType} with ID {Id}", typeof(TEntity).Name, id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var keyName = _context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties.Select(x => x.Name).Single();
                if (keyName == null) throw new InvalidOperationException("Primary key not found for entity.");

                return await _dbSet.AnyAsync(e => EF.Property<int>(e, keyName) == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of {EntityType} with ID {Id}", typeof(TEntity).Name, id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UniqueFieldExistsAsync(string fieldName, string value, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));

            if (string.IsNullOrWhiteSpace(value))
                return false;

            try
            {
                // Build dynamic expression for field comparison
                var parameter = Expression.Parameter(typeof(TEntity), "e");
                var property = Expression.Property(parameter, fieldName);
                var constant = Expression.Constant(value, typeof(string));
                var equalsExpression = Expression.Equal(property, constant);

                var predicate = Expression.Lambda<Func<TEntity, bool>>(equalsExpression, parameter);

                var keyName = _context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties.Select(x => x.Name).Single();
                if (keyName == null) throw new InvalidOperationException("Primary key not found for entity.");

                var query = _dbSet.AsNoTracking().Where(predicate);

                if (excludeId.HasValue)
                {
                    query = query.Where(e => EF.Property<int>(e, keyName) != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking uniqueness of field {FieldName} with value {Value} for {EntityType}",
                    fieldName,
                    value,
                    typeof(TEntity).Name);
                throw;
            }
        }
    }
}
