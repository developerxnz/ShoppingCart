using ErrorOr;

namespace Shopping.Core;

public interface ITransformer<TDomain, TDto>
{
    TDto FromDomain(TDomain domain);

    ErrorOr<TDomain> ToDomain(TDto dto);

    ErrorOr<IEnumerable<TDomain>> ToDomain(IEnumerable<TDto> carts);

    IEnumerable<TDto> FromDomain(IEnumerable<TDomain> domains);
}