using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.CommunicationNotification;
using DomuWave.Services.Dto.CommunicationNotification;
using DomuWave.Services.Dto.PaymentNotice;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using DomuWave.Services.Pdf;
using NHibernate.Linq;
using QuestPDF.Fluent;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

// ── GET BY COMMUNICATION ─────────────────────────────────────────────────────

public class GetNotificationsByCommunicationCommandConsumer
    : InMemoryConsumerBase<GetNotificationsByCommunicationCommand, IList<CommunicationNotificationReadDto>>
{
    private readonly IUserService _userService;

    public GetNotificationsByCommunicationCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<IList<CommunicationNotificationReadDto>> Consume(
        GetNotificationsByCommunicationCommand command,
        IMediationContext                       mediationContext,
        CancellationToken                      cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var list = await session.Query<CommunicationNotification>()
            .Where(n => n.Communication.Id == command.CommunicationId && !n.IsDeleted)
            .OrderBy(n => n.RecipientFullName)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return list.Select(n => n.ToReadDto()).ToList();
    }
}

// ── GENERATE ─────────────────────────────────────────────────────────────────

public class GenerateCommunicationNotificationsCommandConsumer
    : InMemoryConsumerBase<GenerateCommunicationNotificationsCommand, IList<CommunicationNotificationReadDto>>
{
    private readonly IUserService                       _userService;
    private readonly INotificationTemplateVariableResolver _resolver;

    public GenerateCommunicationNotificationsCommandConsumer(
        ISessionFactoryProvider                  sessionFactoryProvider,
        IUserService                             userService,
        INotificationTemplateVariableResolver    resolver) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _resolver    = resolver;
    }

    protected override async Task<IList<CommunicationNotificationReadDto>> Consume(
        GenerateCommunicationNotificationsCommand command,
        IMediationContext                          mediationContext,
        CancellationToken                         cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var communication = await session.Query<Communication>()
            .Where(c => c.Id == command.CommunicationId && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Comunicazione non trovata.");

        var condominium = communication.Condominium;

        // Load template
        NotificationTemplate? template = null;
        if (command.Dto.NotificationTemplateId.HasValue)
        {
            template = await session.GetAsync<NotificationTemplate>(command.Dto.NotificationTemplateId.Value, cancellationToken).ConfigureAwait(false);
        }
        template ??= await session.Query<NotificationTemplate>()
            .Where(t => t.Condominium.Id == condominium.Id
                     && t.CommunicationType == communication.CommunicationType
                     && t.IsDefault && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        // Load all active unit owners for the condominium
        var owners = await session.Query<UnitOwner>()
            .Where(o => o.Unit.Condominium.Id == condominium.Id && o.IsActive && !o.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var addressParts = condominium.Address != null
            ? $"{condominium.Address.Street} {condominium.Address.StreetNumber}, {condominium.Address.PostalCode} {condominium.Address.City}"
            : string.Empty;

        // Delete previously generated Draft notifications for this communication
        var existing = await session.Query<CommunicationNotification>()
            .Where(n => n.Communication.Id == command.CommunicationId && n.Status == 0 && !n.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var old in existing)
        {
            old.IsDeleted = true;
            await session.UpdateAsync(old, cancellationToken).ConfigureAwait(false);
        }

        var created = new List<CommunicationNotification>();

        foreach (var owner in owners)
        {
            var recipientName = $"{owner.FirstName} {owner.LastName}".Trim();
            var ctx = new NotificationVariableContext
            {
                RecipientName      = recipientName,
                UnitNumber         = owner.Unit?.InternalNumber ?? string.Empty,
                CondominiumName    = condominium.Name,
                CommunicationTitle = communication.Title,
                CommunicationBody  = communication.Content,
                AdministratorName  = condominium.AdministratorName ?? string.Empty,
                AdministratorEmail = condominium.AdministratorEmail ?? string.Empty,
                AdministratorPhone = condominium.AdministratorPhone ?? string.Empty,
                Iban               = condominium.Iban ?? string.Empty,
            };

            var subject = template != null ? _resolver.Resolve(template.SubjectTemplate, ctx) : communication.Title;
            var body    = template != null ? _resolver.Resolve(template.BodyTemplate,    ctx) : communication.Content;

            var notification = new CommunicationNotification
            {
                Tenant            = condominium.Tenant,
                Communication     = communication,
                Name              = $"{recipientName} — {communication.Title}",
                RecipientUserId   = owner.UserId,
                RecipientFullName = recipientName,
                Unit              = owner.Unit,
                DeliveryMethod    = command.Dto.DeliveryMethod,
                Status            = 0, // Draft
                ScheduledAt       = command.Dto.ScheduledAt,
                EmailAddress      = owner.Email,
                PostalAddress     = addressParts,
                SubjectResolved   = subject,
                BodyResolved      = body,
            };
            notification.Trace(currentUser);

            await session.SaveAsync(notification, cancellationToken).ConfigureAwait(false);
            created.Add(notification);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return created.Select(n => n.ToReadDto()).ToList();
    }
}

// ── SEND EMAIL ────────────────────────────────────────────────────────────────

public class SendEmailNotificationsCommandConsumer
    : InMemoryConsumerBase<SendEmailNotificationsCommand, SendNotificationsResultDto>
{
    private readonly IUserService  _userService;
    private readonly IEmailService _emailService;

    public SendEmailNotificationsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService,
        IEmailService           emailService) : base(sessionFactoryProvider)
    {
        _userService  = userService;
        _emailService = emailService;
    }

    protected override async Task<SendNotificationsResultDto> Consume(
        SendEmailNotificationsCommand command,
        IMediationContext              mediationContext,
        CancellationToken             cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var communication = await session.Query<Communication>()
            .Where(c => c.Id == command.CommunicationId && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Comunicazione non trovata.");

        // Load SMTP settings for the tenant
        var smtpSettings = await session.Query<TenantSmtpSettings>()
            .Where(s => s.Tenant.Id == communication.Condominium.Tenant.Id && !s.IsDeleted && s.IsEnabled)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new ValidatorException("Configurazione SMTP non trovata. Configura il server email nelle impostazioni.");

        var password = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(smtpSettings.PasswordEncrypted));
        var config   = new SmtpConfig(smtpSettings.Host, smtpSettings.Port, smtpSettings.UseSsl,
                                      smtpSettings.Username, password,
                                      smtpSettings.FromEmail, smtpSettings.FromName);

        // Load pending email notifications
        var notifications = await session.Query<CommunicationNotification>()
            .Where(n => n.Communication.Id == command.CommunicationId
                     && n.DeliveryMethod == 0   // Email
                     && n.Status <= 1            // Draft or Scheduled
                     && !n.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var result = new SendNotificationsResultDto();

        foreach (var notification in notifications)
        {
            if (string.IsNullOrWhiteSpace(notification.EmailAddress))
            {
                notification.Status       = 4; // Failed
                notification.ErrorMessage = "Indirizzo email non disponibile.";
                result.Failed++;
                result.Errors.Add($"{notification.RecipientFullName}: indirizzo email mancante");
                await session.UpdateAsync(notification, cancellationToken).ConfigureAwait(false);
                continue;
            }

            try
            {
                var msg = new EmailMessage(
                    To:       notification.EmailAddress,
                    ToName:   notification.RecipientFullName ?? string.Empty,
                    Subject:  notification.SubjectResolved   ?? communication.Title,
                    BodyHtml: notification.BodyResolved      ?? communication.Content);

                await _emailService.SendAsync(msg, config, cancellationToken).ConfigureAwait(false);

                notification.Status = 2; // Sent
                notification.SentAt = DateTime.UtcNow;
                notification.Trace(currentUser);
                result.Sent++;
            }
            catch (Exception ex)
            {
                notification.Status       = 4; // Failed
                notification.ErrorMessage = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
                notification.Trace(currentUser);
                result.Failed++;
                result.Errors.Add($"{notification.RecipientFullName}: {notification.ErrorMessage}");
            }

            await session.UpdateAsync(notification, cancellationToken).ConfigureAwait(false);
        }

        if (notifications.Count > 0)
        {
            communication.EmailSentAt = DateTime.UtcNow;
            await session.UpdateAsync(communication, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }
}

// ── MARK PRINTED ─────────────────────────────────────────────────────────────

public class MarkNotificationPrintedCommandConsumer
    : InMemoryConsumerBase<MarkNotificationPrintedCommand, CommunicationNotificationReadDto>
{
    private readonly IUserService _userService;

    public MarkNotificationPrintedCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<CommunicationNotificationReadDto> Consume(
        MarkNotificationPrintedCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.GetAsync<CommunicationNotification>(command.NotificationId, cancellationToken).ConfigureAwait(false)
                     ?? throw new NotFoundException("Notifica non trovata.");

        entity.Status    = 5; // Printed
        entity.PrintedAt = DateTime.UtcNow;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

// ── MARK SENT ────────────────────────────────────────────────────────────────

public class MarkNotificationSentCommandConsumer
    : InMemoryConsumerBase<MarkNotificationSentCommand, CommunicationNotificationReadDto>
{
    private readonly IUserService _userService;

    public MarkNotificationSentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<CommunicationNotificationReadDto> Consume(
        MarkNotificationSentCommand command,
        IMediationContext            mediationContext,
        CancellationToken           cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.GetAsync<CommunicationNotification>(command.NotificationId, cancellationToken).ConfigureAwait(false)
                     ?? throw new NotFoundException("Notifica non trovata.");

        entity.Status         = 2; // Sent
        entity.SentAt         = DateTime.UtcNow;
        entity.TrackingNumber = command.Dto.TrackingNumber ?? entity.TrackingNumber;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

// ── MARK DELIVERED ───────────────────────────────────────────────────────────

public class MarkNotificationDeliveredCommandConsumer
    : InMemoryConsumerBase<MarkNotificationDeliveredCommand, CommunicationNotificationReadDto>
{
    private readonly IUserService _userService;

    public MarkNotificationDeliveredCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<CommunicationNotificationReadDto> Consume(
        MarkNotificationDeliveredCommand command,
        IMediationContext                 mediationContext,
        CancellationToken                cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.GetAsync<CommunicationNotification>(command.NotificationId, cancellationToken).ConfigureAwait(false)
                     ?? throw new NotFoundException("Notifica non trovata.");

        entity.Status         = 3; // Delivered
        entity.DeliveredAt    = DateTime.UtcNow;
        entity.TrackingNumber = command.Dto.TrackingNumber ?? entity.TrackingNumber;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

// ── PDF BATCH RACCOMANDATE ────────────────────────────────────────────────────

public class GetNotificationBatchPdfCommandConsumer
    : InMemoryConsumerBase<GetNotificationBatchPdfCommand, byte[]>
{
    private readonly IUserService _userService;

    public GetNotificationBatchPdfCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<byte[]> Consume(
        GetNotificationBatchPdfCommand command,
        IMediationContext               mediationContext,
        CancellationToken              cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var communication = await session.Query<Communication>()
            .Where(c => c.Id == command.CommunicationId && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("Comunicazione non trovata.");

        var notifications = await session.Query<CommunicationNotification>()
            .Where(n => n.Communication.Id == command.CommunicationId
                     && n.DeliveryMethod == 1   // Raccomandata
                     && !n.IsDeleted)
            .OrderBy(n => n.RecipientFullName)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        if (notifications.Count == 0)
            throw new ValidatorException("Nessuna notifica via raccomandata trovata per questa comunicazione.");

        var notices = notifications.Select(n => new PaymentNoticeData
        {
            CondominiumName    = communication.Condominium?.Name ?? string.Empty,
            CondominiumAddress = string.Empty,
            AdministratorName  = communication.Condominium?.AdministratorName ?? string.Empty,
            AdministratorEmail = communication.Condominium?.AdministratorEmail ?? string.Empty,
            OwnerFullName      = n.RecipientFullName ?? string.Empty,
            OwnerEmail         = n.EmailAddress ?? string.Empty,
            UnitInternalNumber = n.Unit?.InternalNumber ?? string.Empty,
            UnitDisplayName    = n.Unit?.DisplayName ?? string.Empty,
            FiscalYearCode     = string.Empty,
            CustomSubject      = n.SubjectResolved ?? communication.Title,
            CustomBody         = n.BodyResolved    ?? communication.Content,
            Rows               = [],
        }).ToList();

        // Mark all as printed
        foreach (var n in notifications)
        {
            if (n.Status < 5) // Not yet printed
            {
                n.Status    = 5; // Printed
                n.PrintedAt = DateTime.UtcNow;
                n.Trace(currentUser);
                await session.UpdateAsync(n, cancellationToken).ConfigureAwait(false);
            }
        }
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        var document = new CommunicationLetterDocument(notices);
        return document.GeneratePdf();
    }
}

// ── GENERATE FROM FEES ───────────────────────────────────────────────────────

public class GenerateNotificationsFromFeesCommandConsumer
    : InMemoryConsumerBase<GenerateNotificationsFromFeesCommand, GenerateFromFeesResultDto>
{
    private readonly IUserService                          _userService;
    private readonly INotificationTemplateVariableResolver _resolver;

    public GenerateNotificationsFromFeesCommandConsumer(
        ISessionFactoryProvider                sessionFactoryProvider,
        IUserService                           userService,
        INotificationTemplateVariableResolver  resolver) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _resolver    = resolver;
    }

    protected override async Task<GenerateFromFeesResultDto> Consume(
        GenerateNotificationsFromFeesCommand command,
        IMediationContext                     mediationContext,
        CancellationToken                    cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var dto         = command.Dto;

        // Load fees for selected installments, filtered by unit if specified
        var feesQuery = session.Query<CondominiumFee>()
            .Where(f => dto.InstallmentIds.Contains(f.Installment.Id) && !f.IsDeleted);
        if (dto.UnitIds != null && dto.UnitIds.Count > 0)
            feesQuery = feesQuery.Where(f => dto.UnitIds.Contains(f.Unit.Id));

        var fees = await feesQuery
            .OrderBy(f => f.Unit.InternalNumber)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        if (fees.Count == 0)
            throw new ValidatorException("Nessuna quota trovata per le rate e le unità selezionate.");

        // All fees belong to the same condominium — take it from the first
        var firstInstallment = await session.GetAsync<CondominiumInstallment>(dto.InstallmentIds[0], cancellationToken).ConfigureAwait(false)
                               ?? throw new NotFoundException("Rata non trovata.");
        var condominium = firstInstallment.Condominium;

        // Resolve or create the Communication
        Communication communication;
        if (dto.CommunicationId.HasValue)
        {
            communication = await session.Query<Communication>()
                .Where(c => c.Id == dto.CommunicationId.Value && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException("Comunicazione non trovata.");
        }
        else
        {
            var installmentNumbers = fees.Select(f => f.Installment.InstallmentNumber).Distinct().OrderBy(x => x);
            var title = $"Avviso rate {string.Join(", ", installmentNumbers.Select(n => $"#{n}"))} — {condominium.Name}";
            communication = new Communication
            {
                Condominium       = condominium,
                Tenant            = condominium.Tenant,
                Name              = title,
                Description       = title,
                CommunicationType = "FeeNotice",
                Priority          = "Normal",
                PublicationDate   = DateTime.Now,
                IsVisible         = true,
                SendEmail         = dto.DeliveryMethod == 0,
            };
            communication.Trace(currentUser);
            await session.SaveAsync(communication, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        // Load template
        NotificationTemplate? template = null;
        if (dto.NotificationTemplateId.HasValue)
            template = await session.GetAsync<NotificationTemplate>(dto.NotificationTemplateId.Value, cancellationToken).ConfigureAwait(false);
        template ??= await session.Query<NotificationTemplate>()
            .Where(t => t.Condominium.Id == condominium.Id
                     && t.CommunicationType == "FeeNotice"
                     && t.IsDefault && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        // Delete existing Draft notifications for this communication on the same fees
        var feeUnitIds = fees.Select(f => f.Unit.Id).Distinct().ToList();
        var existing = await session.Query<CommunicationNotification>()
            .Where(n => n.Communication.Id == communication.Id && n.Status == 0 && !n.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var existingForUnits = existing.Where(n => n.Unit != null && feeUnitIds.Contains(n.Unit.Id)).ToList();
        foreach (var old in existingForUnits)
        {
            old.IsDeleted = true;
            await session.UpdateAsync(old, cancellationToken).ConfigureAwait(false);
        }

        var addressParts = condominium.Address != null
            ? $"{condominium.Address.Street} {condominium.Address.StreetNumber}, {condominium.Address.PostalCode} {condominium.Address.City}"
            : string.Empty;

        // Load all owners for affected units in one query
        var unitIds = fees.Select(f => f.Unit.Id).Distinct().ToList();
        var allOwners = await session.Query<UnitOwner>()
            .Where(o => unitIds.Contains(o.Unit.Id) && o.IsActive && !o.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var ownerByUnit = allOwners.GroupBy(o => o.Unit.Id).ToDictionary(g => g.Key, g => g.First());

        // Group fees by owner identity: email if available, otherwise FirstName+LastName, otherwise unit
        var feesByOwner = fees
            .GroupBy(f =>
            {
                if (!ownerByUnit.TryGetValue(f.Unit.Id, out var o)) return $"unit:{f.Unit.Id}";
                if (!string.IsNullOrWhiteSpace(o.Email))             return $"email:{o.Email.Trim().ToLowerInvariant()}";
                var name = $"{o.FirstName?.Trim()} {o.LastName?.Trim()}".Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(name))                     return $"name:{name}";
                return $"unit:{f.Unit.Id}";
            })
            .ToList();

        var culture = System.Globalization.CultureInfo.GetCultureInfo("it-IT");
        var created = new List<CommunicationNotification>();

        foreach (var group in feesByOwner)
        {
            var groupFees = group.ToList();
            var firstFee  = groupFees[0];
            ownerByUnit.TryGetValue(firstFee.Unit.Id, out var owner);

            var recipientName = owner != null
                ? $"{owner.FirstName} {owner.LastName}".Trim()
                : firstFee.Unit.DisplayName ?? firstFee.Unit.InternalNumber;

            // Build fee table: grouped by installment, detail per unit, subtotal per installment
            var feesByInstallment = groupFees
                .GroupBy(f => f.Installment.Id)
                .OrderBy(g => g.First().Installment.InstallmentNumber);

            var tableLines = new List<string>();
            foreach (var instGroup in feesByInstallment)
            {
                var inst         = instGroup.First().Installment;
                var instTotal    = instGroup.Sum(f => f.AmountDue);
                tableLines.Add($"Rata {inst.InstallmentNumber} — scad. {inst.DueDate:dd/MM/yyyy} — Totale: {instTotal.ToString("C", culture)}");
                foreach (var f in instGroup.OrderBy(f => f.Unit.InternalNumber))
                {
                    var line = $"  {f.Unit.InternalNumber}: {f.AmountDue.ToString("C", culture)}";
                    if (!string.IsNullOrEmpty(f.PaymentCode)) line += $" — cod. {f.PaymentCode}";
                    tableLines.Add(line);
                }
            }
            tableLines.Add(new string('─', 40));
            var totalAmount = groupFees.Sum(f => f.AmountDue);
            tableLines.Add($"Totale complessivo: {totalAmount.ToString("C", culture)}");
            var feeTable       = string.Join("\n", tableLines);
            var totalAmountStr = totalAmount.ToString("C", culture);

            // Use first fee's installment for single-value variables (fallback)
            var firstInst = firstFee.Installment;
            var ctx = new NotificationVariableContext
            {
                RecipientName      = recipientName,
                UnitNumber         = firstFee.Unit?.InternalNumber ?? string.Empty,
                CondominiumName    = condominium.Name,
                CommunicationTitle = communication.Title,
                CommunicationBody  = communication.Content,
                AdministratorName  = condominium.AdministratorName ?? string.Empty,
                AdministratorEmail = condominium.AdministratorEmail ?? string.Empty,
                AdministratorPhone = condominium.AdministratorPhone ?? string.Empty,
                FiscalYearCode     = firstInst?.FiscalYear?.Name ?? string.Empty,
                DueDate            = firstInst?.DueDate.ToString("dd/MM/yyyy") ?? string.Empty,
                Amount             = firstFee.AmountDue.ToString("C", culture),
                PaymentCode        = firstFee.PaymentCode ?? string.Empty,
                Iban               = condominium.Iban ?? string.Empty,
                FeeTable           = feeTable,
                TotalAmount        = totalAmountStr,
            };

            var subject = template != null ? _resolver.Resolve(template.SubjectTemplate, ctx) : communication.Title;
            var body    = template != null ? _resolver.Resolve(template.BodyTemplate,    ctx) : communication.Content;

            // Codici unità del gruppo (distinti, ordinati)
            var unitCodes = groupFees
                .Select(f => f.Unit?.InternalNumber ?? f.Unit?.DisplayName ?? string.Empty)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .OrderBy(s => s)
                .ToList();
            var unitsDisplay = string.Join(", ", unitCodes);

            var notification = new CommunicationNotification
            {
                Tenant            = condominium.Tenant,
                Communication     = communication,
                Name              = $"{recipientName} — {communication.Title}",
                RecipientUserId   = owner?.UserId ?? 0,
                RecipientFullName = recipientName,
                Unit              = firstFee.Unit,
                UnitsDisplay      = unitsDisplay,
                DeliveryMethod    = dto.DeliveryMethod,
                Status            = 0, // Draft
                ScheduledAt       = dto.ScheduledAt,
                EmailAddress      = owner?.Email,
                PostalAddress     = addressParts,
                SubjectResolved   = subject,
                BodyResolved      = body,
            };
            notification.Trace(currentUser);
            await session.SaveAsync(notification, cancellationToken).ConfigureAwait(false);
            created.Add(notification);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return new GenerateFromFeesResultDto
        {
            CommunicationId = communication.Id,
            Notifications   = created.Select(n => n.ToReadDto()).ToList(),
        };
    }
}

// ── UPDATE TEXT ───────────────────────────────────────────────────────────────

public class UpdateNotificationTextCommandConsumer
    : InMemoryConsumerBase<UpdateNotificationTextCommand, CommunicationNotificationReadDto>
{
    private readonly IUserService _userService;

    public UpdateNotificationTextCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<CommunicationNotificationReadDto> Consume(
        UpdateNotificationTextCommand command,
        IMediationContext              mediationContext,
        CancellationToken             cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.GetAsync<CommunicationNotification>(command.NotificationId, cancellationToken).ConfigureAwait(false)
                     ?? throw new NotFoundException("Notifica non trovata.");

        if (entity.Status > 1)
            throw new ValidatorException("Non è possibile modificare una notifica già inviata.");

        entity.SubjectResolved = command.Dto.SubjectResolved ?? entity.SubjectResolved;
        entity.BodyResolved    = command.Dto.BodyResolved;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

// ── DELETE ────────────────────────────────────────────────────────────────────

public class DeleteCommunicationNotificationCommandConsumer
    : InMemoryConsumerBase<DeleteCommunicationNotificationCommand, bool>
{
    private readonly IUserService _userService;

    public DeleteCommunicationNotificationCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<bool> Consume(
        DeleteCommunicationNotificationCommand command,
        IMediationContext                       mediationContext,
        CancellationToken                      cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var entity = await session.GetAsync<CommunicationNotification>(command.NotificationId, cancellationToken).ConfigureAwait(false);
        if (entity == null) return false;

        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
