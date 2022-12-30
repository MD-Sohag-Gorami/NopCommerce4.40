using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.NopStation.Core.Models
{
    public partial record LicenseModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.NopStation.Core.License.LicenseString")]
        public string LicenseString { get; set; }
    }
}
