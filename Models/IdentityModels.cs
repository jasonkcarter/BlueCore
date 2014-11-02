using System.Security.Claims;
using System.Threading.Tasks;
using BlueCore.Identity.AzureTableStorage;
using Microsoft.AspNet.Identity;

namespace BlueCore.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : AzureTableUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var identityFactory = manager.ClaimsIdentityFactory;
            return await identityFactory.CreateAsync(manager, this, DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}