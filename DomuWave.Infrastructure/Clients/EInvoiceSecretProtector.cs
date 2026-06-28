using Microsoft.AspNetCore.DataProtection;

namespace DomuWave.Services.Clients;

/// <summary>
/// Implementazione di <see cref="IEInvoiceSecretProtector"/> basata su DataProtection.
/// Usa un purpose dedicato così le chiavi fatture non sono decifrabili da altri protector.
/// </summary>
public class EInvoiceSecretProtector : IEInvoiceSecretProtector
{
    private readonly IDataProtector _protector;

    public EInvoiceSecretProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("DomuWave.EInvoice.ApiKey.v1");
    }

    public string Protect(string plaintext)
        => string.IsNullOrEmpty(plaintext) ? plaintext : _protector.Protect(plaintext);

    public string Unprotect(string ciphertext)
        => string.IsNullOrEmpty(ciphertext) ? ciphertext : _protector.Unprotect(ciphertext);
}
