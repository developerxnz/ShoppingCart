using ErrorOr;

namespace Shopping.Services.Interfaces;

public interface ITransformer<TDomain, TDto>
{
    TDto FromDomain(TDomain aggregate);

    ErrorOr<TDomain> ToDomain(TDto dto);

    ErrorOr<IEnumerable<TDomain>> ToDomain(IEnumerable<TDto> carts);

    IEnumerable<TDto> FromDomain(IEnumerable<TDomain> domains);
}