using System;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Query
{
    public class Question : BaseEntity, ISoftDeletedEntity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public string Message { get; set; }

        public string AdditionalLink { get; set; }

        public Guid DownloadGuid { get; set; }

        public int CustomerId { get; set; }

        public string AdminComment { get; set; }

        public bool MarkedAsRead { get; set; }

        public int MarkedByUserId { get; set; }

        public string AdditionalField { get; set; }

        public bool Deleted { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
