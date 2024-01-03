using Microsoft.Azure.Cosmos.Linq;
using Shopping.Domain.Cart;
using Shopping.Domain.Cart.Core;
using Shopping.Domain.Cart.Events;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;
using Shopping.Services.Cart;
using Shopping.Services.Interfaces;
using CartAggregate = Shopping.Domain.Cart.CartAggregate;
using CartEvent = Shopping.Infrastructure.Persistence.Cart.CartEvent;

namespace ShoppingUnitTests;

public class CartMapperTests
{
    private readonly IMapper<Shopping.Domain.Cart.CartAggregate, Shopping.Infrastructure.Persistence.Cart.CartAggregate, ICartEvent, CartEvent> _mapper;

    public CartMapperTests()
    {
        _mapper = new CartMapper();
    }

    [Fact]
    public void FromDomain_Should_Return_Expected_Dto()
    {
        DateTime createdOnUtc = DateTime.UtcNow;
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        Sku firstSku = new(Guid.NewGuid().ToString());
        var firstQuantity = new CartQuantity(10);
        
        Sku lastSku = new(Guid.NewGuid().ToString());
        var lastQuantity = new CartQuantity(10);
        
        IEnumerable<CartItem> items = new []
        {
            new CartItem(firstSku, firstQuantity),
            new CartItem(lastSku, lastQuantity)
        };
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
        {
            Items = items
        };

        var cartDto = _mapper.FromDomain(aggregate);
        
        Assert.Equal(customerId.Value.ToString(), cartDto.CustomerId);
        Assert.Equal(createdOnUtc, cartDto.CreatedOnUtc);
        Assert.Equal(aggregate.MetaData.StreamId.Value.ToString(), cartDto.Metadata.StreamId);
        Assert.Equal(aggregate.MetaData.Version.Value, cartDto.Metadata.Version);
        Assert.Equal(aggregate.MetaData.TimeStamp, cartDto.Metadata.Timestamp);
        Assert.Equal(2, aggregate.Items.Count());
        Assert.Equal(firstSku.Value, cartDto.Items.First().Sku);
        Assert.Equal(firstQuantity.Value, cartDto.Items.First().Quantity);
        Assert.Equal(lastSku.Value, cartDto.Items.Last().Sku);
        Assert.Equal(lastQuantity.Value, cartDto.Items.Last().Quantity);
    }
    
    [Fact]
    public void Enumerable_FromDomain_Should_Return_Expected_Dto()
    {
        DateTime createdOnUtc = DateTime.UtcNow;
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        Sku firstSku = new(Guid.NewGuid().ToString());
        var firstQuantity = new CartQuantity(10);
        
        Sku lastSku = new(Guid.NewGuid().ToString());
        var lastQuantity = new CartQuantity(10);
        
        IEnumerable<CartItem> items = new []
        {
            new CartItem(firstSku, firstQuantity),
            new CartItem(lastSku, lastQuantity)
        };
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
        {
            Items = items
        };

        var cartDtos = _mapper.FromDomain(new [] { aggregate });

        foreach (var cartDto in cartDtos)
        {
            Assert.Equal(customerId.Value.ToString(), cartDto.CustomerId);
            Assert.Equal(createdOnUtc, cartDto.CreatedOnUtc);
            Assert.Equal(aggregate.MetaData.StreamId.Value.ToString(), cartDto.Metadata.StreamId);
            Assert.Equal(aggregate.MetaData.Version.Value, cartDto.Metadata.Version);
            Assert.Equal(aggregate.MetaData.TimeStamp, cartDto.Metadata.Timestamp);
            Assert.Equal(2, aggregate.Items.Count());
            Assert.Equal(firstSku.Value, cartDto.Items.First().Sku);
            Assert.Equal(firstQuantity.Value, cartDto.Items.First().Quantity);
            Assert.Equal(lastSku.Value, cartDto.Items.Last().Sku);
            Assert.Equal(lastQuantity.Value, cartDto.Items.Last().Quantity);
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
        Shopping.Domain.Core.Persistence.Metadata metaData = 
            new(
                streamId.Value.ToString(),
                version,
                timestamp
            );
        
        Sku firstSku = new(Guid.NewGuid().ToString());
        var firstQuantity = new CartQuantity(10);
        
        Sku lastSku = new(Guid.NewGuid().ToString());
        var lastQuantity = new CartQuantity(10);
        
        IEnumerable<Shopping.Infrastructure.Persistence.Cart.CartItem> items = new []
        {
            new Shopping.Infrastructure.Persistence.Cart.CartItem(firstSku.Value, firstQuantity.Value),
            new Shopping.Infrastructure.Persistence.Cart.CartItem(lastSku.Value, lastQuantity.Value)
        };
        Shopping.Infrastructure.Persistence.Cart.CartAggregate dto = new()
        {
            CustomerId = customerId.Value.ToString(),
            CreatedOnUtc = createdOnUtc,
            Items = items,
            Etag = eTag,
            Id = cartId.Value.ToString(),
            Metadata = metaData
        };

        var domain = _mapper.ToDomain(dto);
        
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
        Shopping.Domain.Core.Persistence.Metadata metaData = 
            new(
                streamId.Value.ToString(),
                version,
                timestamp
            );
        
        Sku firstSku = new(Guid.NewGuid().ToString());
        var firstQuantity = new CartQuantity(10);
        
        Sku lastSku = new(Guid.NewGuid().ToString());
        var lastQuantity = new CartQuantity(10);
        
        IEnumerable<Shopping.Infrastructure.Persistence.Cart.CartItem> items = new []
        {
            new Shopping.Infrastructure.Persistence.Cart.CartItem(firstSku.Value, firstQuantity.Value),
            new Shopping.Infrastructure.Persistence.Cart.CartItem(lastSku.Value, lastQuantity.Value)
        };
        Shopping.Infrastructure.Persistence.Cart.CartAggregate dto = new()
        {
            CustomerId = customerId.Value.ToString(),
            CreatedOnUtc = createdOnUtc,
            Items = items,
            Etag = eTag,
            Id = cartId.Value.ToString(),
            Metadata = metaData
        };

        var domains = _mapper.ToDomain(new [] { dto });
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