namespace DomuWave.Services.Clients;

/// <summary>
/// Cifra/decifra a riposo i segreti del download fatture (chiave API del provider).
/// Implementazione basata su ASP.NET Core DataProtection.
/// </summary>
public interface IEInvoiceSecretProtector
{
    /// <summary>Cifra un valore in chiaro. Ritorna null/empty se l'input è null/empty.</summary>
    string Protect(string plaintext);

    /// <summary>Decifra un valore cifrato. Ritorna null/empty se l'input è null/empty.</summary>
    string Unprotect(string ciphertext);
}
