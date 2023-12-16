using Bogus;
using Shopping.Cart;
using Shopping.Cart.Core;
using Shopping.Core;

namespace ShoppingUnitTests;

public static class CartBuilder
{
    public static CartAggregate BasicCart()
    {
        return
            new Faker<CartAggregate>()
                .StrictMode(true)
                .RuleFor(x => x.CustomerId, new CustomerId(Guid.NewGuid()))
                .RuleFor(x => x.CreatedOnUtc, DateTime.UtcNow)
                .RuleFor(x => x.Etag, Guid.NewGuid().ToString())
                .RuleFor(x => x.Id, new CartId(Guid.NewGuid()));
    }
    
    /*
     * Categories Read Model
     * - Contains All Categories
     * - Contains All Products
     * Compare RU Cost, Read Model vs Individual Category Queries
     * Caching option
     * - When to delete
     *  - On Change Feed during Read Model creation?
     *
     * 
     * Product Read Model
     * - Product Details
     * - Activity Details
     * Compare RU Cost, Read Model vs Individual Product, Activity Queries
     * Caching option
     * - When to delete
     *  - On Change Feed during Read Model creation?
     *
     * Steps Read Model
     * Step Details
     * Compare RU Cost, Read Model vs Individual Step Queries
     * Caching option
     * - When to delete
     *  - On Change Feed during Read Model creation?
     */
}