using Shopping.Cart;
using Shopping.Cart.Core;
using Shopping.Core;
using Shopping.Product;

namespace ShoppingUnitTests;

public class CartTransformerTests
{
    private readonly ITransformer<CartAggregate, Shopping.Cart.Persistence.Cart> _transformer;

    public CartTransformerTests()
    {
        _transformer = new CartTransformer(new CartItemTransformer());
    }

    [Fact]
    public void FromDomain_Should_Return_Expected_Dto()
    {
        DateTime createdOnUtc = DateTime.UtcNow;
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        Sku firstSku = new(Guid.NewGuid());
        uint firstQuantity = 10;
        
        Sku lastSku = new(Guid.NewGuid());
        uint lastQuantity = 10;
        
        IEnumerable<CartItem> items = new []
        {
            new CartItem(firstSku, firstQuantity),
            new CartItem(lastSku, lastQuantity)
        };
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
        {
            Items = items
        };

        var cartDto = _transformer.FromDomain(aggregate);
        
        Assert.Equal(customerId.Value.ToString(), cartDto.CustomerId);
        Assert.Equal(createdOnUtc, cartDto.CreatedOnUtc);
        Assert.Equal(aggregate.MetaData.StreamId.Value.ToString(), cartDto.MetaData.StreamId);
        Assert.Equal(aggregate.MetaData.Version.Value, cartDto.MetaData.Version);
        Assert.Equal(aggregate.MetaData.TimeStamp, cartDto.MetaData.TimeStamp);
        Assert.Equal(2, aggregate.Items.Count());
        Assert.Equal(firstSku.Value.ToString(), cartDto.Items.First().Sku);
        Assert.Equal(firstQuantity, cartDto.Items.First().Quantity);
        Assert.Equal(lastSku.Value.ToString(), cartDto.Items.Last().Sku);
        Assert.Equal(lastQuantity, cartDto.Items.Last().Quantity);
    }
    
    [Fact]
    public void Enumerable_FromDomain_Should_Return_Expected_Dto()
    {
        DateTime createdOnUtc = DateTime.UtcNow;
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        Sku firstSku = new(Guid.NewGuid());
        uint firstQuantity = 10;
        
        Sku lastSku = new(Guid.NewGuid());
        uint lastQuantity = 10;
        
        IEnumerable<CartItem> items = new []
        {
            new CartItem(firstSku, firstQuantity),
            new CartItem(lastSku, lastQuantity)
        };
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
        {
            Items = items
        };

        var cartDtos = _transformer.FromDomain(new [] { aggregate });

        foreach (var cartDto in cartDtos)
        {
            Assert.Equal(customerId.Value.ToString(), cartDto.CustomerId);
            Assert.Equal(createdOnUtc, cartDto.CreatedOnUtc);
            Assert.Equal(aggregate.MetaData.StreamId.Value.ToString(), cartDto.MetaData.StreamId);
            Assert.Equal(aggregate.MetaData.Version.Value, cartDto.MetaData.Version);
            Assert.Equal(aggregate.MetaData.TimeStamp, cartDto.MetaData.TimeStamp);
            Assert.Equal(2, aggregate.Items.Count());
            Assert.Equal(firstSku.Value.ToString(), cartDto.Items.First().Sku);
            Assert.Equal(firstQuantity, cartDto.Items.First().Quantity);
            Assert.Equal(lastSku.Value.ToString(), cartDto.Items.Last().Sku);
            Assert.Equal(lastQuantity, cartDto.Items.Last().Quantity);
        }
    }
    
    [Fact]
    public void ToDomain_Should_Return_Expected_Domain()
    {
        CartId cartId = new(Guid.NewGuid());
        DateTime createdOnUtc = DateTime.UtcNow;
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        string eTag = Guid.NewGuid().ToString();
        StreamId streamId = new StreamId(Guid.NewGuid());
        DateTime timestamp = DateTime.UtcNow;
        uint version = 25;
        Shopping.Core.Persistence.MetaData metaData = 
            new(
                streamId.Value.ToString(),
                version,
                timestamp
            );
        
        Sku firstSku = new(Guid.NewGuid());
        uint firstQuantity = 10;
        
        Sku lastSku = new(Guid.NewGuid());
        uint lastQuantity = 10;
        
        IEnumerable<Shopping.Cart.Persistence.CartItem> items = new []
        {
            new Shopping.Cart.Persistence.CartItem(firstSku.Value.ToString(), firstQuantity),
            new Shopping.Cart.Persistence.CartItem(lastSku.Value.ToString(), lastQuantity)
        };
        Shopping.Cart.Persistence.Cart dto = new()
        {
            CustomerId = customerId.Value.ToString(),
            CreatedOnUtc = createdOnUtc,
            Items = items,
            ETag = eTag,
            Id = cartId.Value.ToString(),
            MetaData = metaData
        };

        var domain = _transformer.ToDomain(dto);
        
        Assert.Equal(customerId.Value, domain.Value.CustomerId.Value);
        Assert.Equal(createdOnUtc, domain.Value.CreatedOnUtc);
        Assert.Equal(cartId.Value.ToString(), domain.Value.MetaData.StreamId.Value.ToString());
        Assert.Equal(version, domain.Value.MetaData.Version.Value);
        Assert.Equal(timestamp, domain.Value.MetaData.TimeStamp);
        Assert.Equal(2, dto.Items.Count());
        Assert.Equal(firstSku, domain.Value.Items.First().Sku);
        Assert.Equal(firstQuantity, domain.Value.Items.First().Quantity);
        Assert.Equal(lastSku, domain.Value.Items.Last().Sku);
        Assert.Equal(lastQuantity, domain.Value.Items.Last().Quantity);
    }
    
        [Fact]
    public void Enumerable_ToDomain_Should_Return_Expected_Domain()
    {
        CartId cartId = new(Guid.NewGuid());
        DateTime createdOnUtc = DateTime.UtcNow;
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        string eTag = Guid.NewGuid().ToString();
        StreamId streamId = new StreamId(Guid.NewGuid());
        DateTime timestamp = DateTime.UtcNow;
        uint version = 25;
        Shopping.Core.Persistence.MetaData metaData = 
            new(
                streamId.Value.ToString(),
                version,
                timestamp
            );
        
        Sku firstSku = new(Guid.NewGuid());
        uint firstQuantity = 10;
        
        Sku lastSku = new(Guid.NewGuid());
        uint lastQuantity = 10;
        
        IEnumerable<Shopping.Cart.Persistence.CartItem> items = new []
        {
            new Shopping.Cart.Persistence.CartItem(firstSku.Value.ToString(), firstQuantity),
            new Shopping.Cart.Persistence.CartItem(lastSku.Value.ToString(), lastQuantity)
        };
        Shopping.Cart.Persistence.Cart dto = new()
        {
            CustomerId = customerId.Value.ToString(),
            CreatedOnUtc = createdOnUtc,
            Items = items,
            ETag = eTag,
            Id = cartId.Value.ToString(),
            MetaData = metaData
        };

        var domains = _transformer.ToDomain(new [] { dto });
        foreach (var domain in domains.Value)
        {

            Assert.Equal(customerId.Value, domain.CustomerId.Value);
            Assert.Equal(createdOnUtc, domain.CreatedOnUtc);
            Assert.Equal(cartId.Value.ToString(), domain.MetaData.StreamId.Value.ToString());
            Assert.Equal(version, domain.MetaData.Version.Value);
            Assert.Equal(timestamp, domain.MetaData.TimeStamp);
            Assert.Equal(2, dto.Items.Count());
            Assert.Equal(firstSku, domain.Items.First().Sku);
            Assert.Equal(firstQuantity, domain.Items.First().Quantity);
            Assert.Equal(lastSku, domain.Items.Last().Sku);
            Assert.Equal(lastQuantity, domain.Items.Last().Quantity);
        }
    }
    
}