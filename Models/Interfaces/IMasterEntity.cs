namespace Yash_Gems___Jewelleries.Models.Interfaces
{
    /// <summary>
    /// Base interface for all master data entities in the system.
    /// Provides common properties for identification, auditing, and concurrency control.
    /// </summary>
    public interface IMasterEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Indicates whether the entity is currently active in the system
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was created
        /// </summary>
        DateTime CreatedDate { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was last modified
        /// </summary>
        DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Row version for optimistic concurrency control
        /// </summary>
        byte[] RowVersion { get; set; }
    }
}
