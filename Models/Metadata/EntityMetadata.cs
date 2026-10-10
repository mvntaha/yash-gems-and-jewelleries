using System;
using System.Collections.Generic;
using System.Linq;
using Yash_Gems___Jewelleries.Models.Masters;

namespace Yash_Gems___Jewelleries.Models.Metadata
{
    public interface IEntityMetadataProvider
    {
        EntityMetadata? GetMetadata(string entityType);
        IEnumerable<EntityMetadata> GetAllMetadata();
    }

    public class EntityMetadata
    {
        public string EntityType { get; set; } = string.Empty;
        public Type EntityClass { get; set; } = typeof(object);
        public string DisplayName { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string TableTitle { get; set; } = string.Empty;
        public string FormPartial { get; set; } = string.Empty;
        public string TableView { get; set; } = string.Empty;
        public string TableRowsPartial { get; set; } = string.Empty;
    }

    public class EntityMetadataProvider : IEntityMetadataProvider
    {
        private readonly Dictionary<string, EntityMetadata> _metadata;

        public EntityMetadataProvider()
        {
            _metadata = new Dictionary<string, EntityMetadata>(StringComparer.OrdinalIgnoreCase)
            {
                { "brand", new EntityMetadata { 
                    EntityType = "brand", 
                    EntityClass = typeof(Brand),
                    DisplayName = "Brand", 
                    IconClass = "fas fa-tag", 
                    TableTitle = "Brand Management",
                    FormPartial = "_BrandFormPartial",
                    TableView = "Brands",
                    TableRowsPartial = "_BrandTableRowsPartial"
                } },
                { "category", new EntityMetadata { 
                    EntityType = "category", 
                    EntityClass = typeof(Category),
                    DisplayName = "Category", 
                    IconClass = "fas fa-list", 
                    TableTitle = "Category Management",
                    FormPartial = "_CategoryFormPartial",
                    TableView = "Categories",
                    TableRowsPartial = "_CategoryTableRowsPartial"
                } },
                { "certificate", new EntityMetadata { 
                    EntityType = "certificate", 
                    EntityClass = typeof(Certificate),
                    DisplayName = "Certificate", 
                    IconClass = "fas fa-certificate", 
                    TableTitle = "Certificate Management",
                    FormPartial = "_CertificateFormPartial",
                    TableView = "Certifications",
                    TableRowsPartial = "_CertificateTableRowsPartial"
                } },
                { "producttype", new EntityMetadata { 
                    EntityType = "producttype", 
                    EntityClass = typeof(ProductType),
                    DisplayName = "Product Type", 
                    IconClass = "fas fa-gem", 
                    TableTitle = "Product Type Management",
                    FormPartial = "_ProductTypeFormPartial",
                    TableView = "ProductTypes",
                    TableRowsPartial = "_ProductTypeTableRowsPartial"
                } },
                { "goldkarat", new EntityMetadata { 
                    EntityType = "goldkarat", 
                    EntityClass = typeof(GoldKarat),
                    DisplayName = "Gold Karat", 
                    IconClass = "fas fa-coins", 
                    TableTitle = "Gold Karat Management",
                    FormPartial = "_GoldKaratFormPartial",
                    TableView = "GoldKarats",
                    TableRowsPartial = "_GoldKaratTableRowsPartial"
                } },
                { "diamondquality", new EntityMetadata { 
                    EntityType = "diamondquality", 
                    EntityClass = typeof(DiamondQuality),
                    DisplayName = "Diamond Quality", 
                    IconClass = "fas fa-gem", 
                    TableTitle = "Diamond Quality Management",
                    FormPartial = "_DiamondQualityFormPartial",
                    TableView = "DiamondQualities",
                    TableRowsPartial = "_DiamondQualityTableRowsPartial"
                } },
                { "stonequality", new EntityMetadata { 
                    EntityType = "stonequality", 
                    EntityClass = typeof(StoneQuality),
                    DisplayName = "Stone Quality", 
                    IconClass = "fas fa-gem", 
                    TableTitle = "Stone Quality Management",
                    FormPartial = "_StoneQualityFormPartial",
                    TableView = "StoneQualities",
                    TableRowsPartial = "_StoneQualityTableRowsPartial"
                } }
            };
        }

        public EntityMetadata? GetMetadata(string entityType)
        {
            return _metadata.TryGetValue(entityType, out var metadata) ? metadata : null;
        }

        public IEnumerable<EntityMetadata> GetAllMetadata()
        {
            return _metadata.Values;
        }
    }
}
