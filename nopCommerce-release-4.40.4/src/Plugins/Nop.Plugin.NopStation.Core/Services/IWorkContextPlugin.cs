using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Services.Plugins;

namespace Nop.Plugin.NopStation.Core.Services
{
    public interface IWorkContextPlugin : IPlugin
    {
        bool CheckGuestCustomer { get; }

        Task<Customer> GetAuthenticatedCustomerAsync();

        Task<Customer> GetGuestCustomerAsync();
    }
}
