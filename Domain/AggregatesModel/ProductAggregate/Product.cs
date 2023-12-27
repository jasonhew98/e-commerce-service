using Domain.Model;
using Domain.Seedwork;
using System;
using System.Collections.Generic;

namespace Domain.AggregatesModel.ProductAggregate
{
    public class Product : AuditableEntity, IAggregateRoot
    {
        public string ProductId { get; private set; }
        public string ProductName { get; private set; }
        public double ProductPrice { get; private set; }
        public List<Attachment> ProductImages { get; private set; }

        public Product(
            string productName,
            double productPrice,
            List<Attachment> productImages = null,
            string productId = null,
            string createdBy = null,
            string createdByName = null,
            DateTime? createdUTCDateTime = null,
            string modifiedBy = null,
            string modifiedByName = null,
            DateTime? modifiedUTCDateTime = null)
            : base(
                  createdBy: createdBy,
                  createdByName: createdByName,
                  createdUTCDateTime: createdUTCDateTime,
                  modifiedBy: modifiedBy,
                  modifiedByName: modifiedByName,
                  modifiedUTCDateTime: modifiedUTCDateTime)
        {
            ProductId = productId;
            ProductName = productName;
            ProductPrice = productPrice;
            ProductImages = productImages;
        }

        public void UpdateProductDetails(Product product)
        {
            ProductName = product.ProductName;
            ProductImages = product.ProductImages;
        }
    }
}
