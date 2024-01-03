using ErrorOr;
using Shopping.Domain.Core;

namespace Shopping.Services;

public abstract class Mapper<TDomain, TDto, TEvent, TEventDto> 
    where TEvent:IEvent
    where TEventDto: Infrastructure.Interfaces.IEvent
{
    public abstract TDto FromDomain(TDomain aggregate);
    
    public abstract TEventDto FromDomain(TEvent @event);
    
    public abstract ErrorOr<TDomain> ToDomain(TDto dto);
    
    public abstract ErrorOr<TEvent> ToDomain(TEventDto dto);
    
    public ErrorOr<IEnumerable<TDomain>> ToDomain(IEnumerable<TDto> domains)
    {
        var converted = new List<TDomain>();
        foreach (var dto in domains)
        {
            var response = ToDomain(dto);
            if (response.IsError)
            {
                return response.Errors;
            }
            
            converted.Add(response.Value);
        }

        return converted;
    }
    
    public ErrorOr<IEnumerable<TEvent>> ToDomain(IEnumerable<TEventDto> eventDtos)
    {
        var converted = new List<TEvent>();
        foreach (var dto in eventDtos)
        {
            var response = ToDomain(dto);
            if (response.IsError)
            {
                return response.Errors;
            }
            
            converted.Add(response.Value);
        }

        return converted;
    }

    public IEnumerable<TDto> FromDomain(IEnumerable<TDomain> domains)
    {
        return domains
            .Select(FromDomain)
            .ToList();
    }

}