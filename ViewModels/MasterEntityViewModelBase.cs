using System.ComponentModel.DataAnnotations;

namespace Yash_Gems___Jewelleries.ViewModels
{
    /// <summary>
    /// Abstract base class for all master entity ViewModels.
    /// Provides common properties for identification, status, and concurrency control.
    /// </summary>
    public abstract class MasterEntityViewModelBase
    {
        /// <summary>
        /// Unique identifier for the entity (maps to specific ID property in derived classes)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Indicates whether the entity is currently active
        /// </summary>
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Row version for optimistic concurrency control.
        /// This is sent as a hidden field and validated on update.
        /// </summary>
        public byte[]? RowVersion { get; set; }
    }
}
