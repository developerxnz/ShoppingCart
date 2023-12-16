namespace Shopping.Core;
using ErrorOr;

public abstract class Transformer<TDomain, TDto>
{
    public abstract TDto FromDomain(TDomain aggregate);
    
    public abstract ErrorOr<TDomain> ToDomain(TDto dto);
    
    public ErrorOr<IEnumerable<TDomain>> ToDomain(IEnumerable<TDto> dtos)
    {
        var converted = new List<TDomain>();
        foreach (var dto in dtos)
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