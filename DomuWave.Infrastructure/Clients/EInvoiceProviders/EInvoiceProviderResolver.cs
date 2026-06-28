using System.Collections.Generic;
using System.Linq;
using CPQ.Core.Exceptions;

namespace DomuWave.Services.Clients.EInvoiceProviders;

/// <summary>
/// Resolver basato su DI: riceve tutte le <see cref="IEInvoiceProvider"/> registrate
/// e le indicizza per <see cref="IEInvoiceProvider.ProviderId"/>.
/// </summary>
public class EInvoiceProviderResolver : IEInvoiceProviderResolver
{
    private readonly IReadOnlyDictionary<int, IEInvoiceProvider> _providers;

    public EInvoiceProviderResolver(IEnumerable<IEInvoiceProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.ProviderId);
    }

    public IEInvoiceProvider Resolve(int providerId)
    {
        if (_providers.TryGetValue(providerId, out var provider))
            return provider;

        throw new ValidatorException("Provider fatturazione elettronica non supportato o non configurato.");
    }
}
