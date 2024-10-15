// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : AuthSettings Model.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio (based on)
// ***********************************************************************

namespace omnicart_api.Models
{
    public class AuthSettings
    {
        public string AdminToken { get; set; } = null!;
    }
}
