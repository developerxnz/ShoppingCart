using Bogus;
using Shopping.Cart;
using Shopping.Core;

namespace ShoppingUnitTests;

public static class CartBuilder
{
    public static CartAggregate CreateNew()
    {
        return
            new Faker<CartAggregate>()
                //Ensure all properties have rules. By default, StrictMode is false
                //Set a global policy by using Faker.DefaultStrictMode
                .StrictMode(true)
                .CustomInstantiator(f => new CartAggregate(DateTime.UtcNow, CustomerId.Create()));
    }
}