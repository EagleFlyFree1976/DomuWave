using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;

namespace DomuWave.Services.Implementations;

public class NotificationTemplateVariableResolver : INotificationTemplateVariableResolver
{
    public string Resolve(string template, NotificationVariableContext ctx)
    {
        if (string.IsNullOrEmpty(template)) return template;

        return template
            .Replace("{{RecipientName}}",      ctx.RecipientName)
            .Replace("{{UnitNumber}}",          ctx.UnitNumber)
            .Replace("{{CondominiumName}}",     ctx.CondominiumName)
            .Replace("{{CommunicationTitle}}", ctx.CommunicationTitle)
            .Replace("{{CommunicationBody}}",  ctx.CommunicationBody)
            .Replace("{{AdministratorName}}",  ctx.AdministratorName)
            .Replace("{{AdministratorEmail}}", ctx.AdministratorEmail)
            .Replace("{{AdministratorPhone}}", ctx.AdministratorPhone)
            .Replace("{{FiscalYearCode}}",     ctx.FiscalYearCode)
            .Replace("{{DueDate}}",            ctx.DueDate)
            .Replace("{{Amount}}",             ctx.Amount)
            .Replace("{{PaymentCode}}",        ctx.PaymentCode)
            .Replace("{{Iban}}",               ctx.Iban);
    }
}
