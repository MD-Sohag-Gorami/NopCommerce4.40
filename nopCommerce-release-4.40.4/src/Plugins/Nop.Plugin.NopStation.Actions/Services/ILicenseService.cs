using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Nop.Plugin.NopStation.Core.Domains;
using Nop.Plugin.NopStation.Core.Infrastructure;

namespace Nop.Plugin.NopStation.Core.Services
{
    public interface ILicenseService
    {
        Task<bool> IsLicensedAsync(Assembly assembly);

        KeyVerificationResult VerifyProductKey(string key, bool checkFileName = false, string fileName = "");

        Task<IList<License>> GetLicensesAsync();

        Task DeleteLicenseAsync(License license);

        Task UpdateLicenseAsync(License license);

        Task InsertLicenseAsync(License license);
    }
}
