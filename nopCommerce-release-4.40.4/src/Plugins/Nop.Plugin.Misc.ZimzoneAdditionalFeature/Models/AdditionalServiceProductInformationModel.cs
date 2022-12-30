using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models
{
    public record AdditionalServiceProductInformationModel : BaseNopModel
    {
        public AdditionalServiceProductInformationModel()
        {
            Errors = new List<string>();
        }
        public bool IsServiceProduct { get; set; }
        public string SubmitButtonText { get; set; }
        public List<string> Errors { get; set; }
    }
}
