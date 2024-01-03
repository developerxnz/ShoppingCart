using ErrorOr;
using Shopping.Domain.Core;

namespace Shopping.Services.Interfaces;

public interface IMapper<TDomain, TDto, in TEvent, TEventDto> 
    where TEvent:IEvent 
    where TEventDto: Infrastructure.Interfaces.IEvent
{
    TDto FromDomain(TDomain aggregate);
    
    TEventDto FromDomain(TEvent @event);
    
    (TDto, IEnumerable<TEventDto>) FromDomain(TDomain aggregate, IEnumerable<TEvent> events);

    ErrorOr<TDomain> ToDomain(TDto dto);

    ErrorOr<IEnumerable<TDomain>> ToDomain(IEnumerable<TDto> carts);

    IEnumerable<TDto> FromDomain(IEnumerable<TDomain> domains);
}