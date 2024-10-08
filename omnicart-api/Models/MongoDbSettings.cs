// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : MongoDB settings Class (Connection String, Database Name, Collection Name).
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

namespace omnicart_api.Models
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string UsersCollectionName { get; set; } = null!;

        public string CategoriesCollectionName { get; set; } = null!;

        public string ProductsCollectionName { get; set; } = null!;

        public string OrdersCollectionName { get; set; } = null!;

        public string ReviewsCollectionName { get; set; } = null!;

        public string NotificationsCollectionName { get; set; } = null!;
    }
}