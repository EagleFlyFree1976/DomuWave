using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface INotificationTemplateVariableResolver
{
    /// <summary>
    /// Sostituisce le variabili {{NomeVariabile}} nel template con i valori reali.
    /// </summary>
    string Resolve(string template, NotificationVariableContext ctx);
}

public class NotificationVariableContext
{
    public string RecipientName      { get; set; } = string.Empty;
    public string UnitNumber         { get; set; } = string.Empty;
    public string CondominiumName    { get; set; } = string.Empty;
    public string CommunicationTitle { get; set; } = string.Empty;
    public string CommunicationBody  { get; set; } = string.Empty;
    public string AdministratorName  { get; set; } = string.Empty;
    public string AdministratorEmail { get; set; } = string.Empty;
    public string AdministratorPhone { get; set; } = string.Empty;
    public string FiscalYearCode     { get; set; } = string.Empty;
    public string DueDate            { get; set; } = string.Empty;
    public string Amount             { get; set; } = string.Empty;
    public string PaymentCode        { get; set; } = string.Empty;
    public string Iban               { get; set; } = string.Empty;
}
