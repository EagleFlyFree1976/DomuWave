using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Assembly;
using DomuWave.Services.Dto.Assembly;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class PlanAssemblyCommandConsumer : InMemoryConsumerBase<PlanAssemblyCommand, AssemblyReadDto>
{
    private readonly IAssemblyService                       _assemblyService;
    private readonly IUserService                           _userService;
    private readonly INotificationTemplateVariableResolver  _resolver;

    public PlanAssemblyCommandConsumer(
        ISessionFactoryProvider                 sessionFactoryProvider,
        IAssemblyService                        assemblyService,
        IUserService                            userService,
        INotificationTemplateVariableResolver   resolver) : base(sessionFactoryProvider)
    {
        _assemblyService = assemblyService;
        _userService     = userService;
        _resolver        = resolver;
    }

    protected override async Task<AssemblyReadDto> Consume(
        PlanAssemblyCommand command,
        IMediationContext    mediationContext,
        CancellationToken   cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var entity      = await _assemblyService.GetByIdAsync(command.AssemblyId, currentUser, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Assemblea non trovata.");

        if (entity.Status?.Id != AssemblyStatusLookup.Bozza)
            throw new ValidatorException("Solo le assemblee in stato 'Bozza' possono essere spostate in 'Pianificata'.");

        // Validazioni pre-pianificazione
        if (entity.ScheduledDate <= DateTime.UtcNow)
            throw new ValidatorException("La data di convocazione deve essere nel futuro.");

        var agendaCount = await session.Query<AssemblyAgendaItem>()
            .CountAsync(a => a.Assembly.Id == entity.Id && !a.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (agendaCount == 0)
            throw new ValidatorException("È necessario inserire almeno un punto all'ordine del giorno prima di pianificare l'assemblea.");

        var status = await session.GetAsync<AssemblyStatusLookup>(AssemblyStatusLookup.Pianificata, cancellationToken).ConfigureAwait(false)!;
        entity.Status = status;
        entity.Trace(currentUser);

        // Crea automaticamente la comunicazione di convocazione
        var communication = new Communication
        {
            Condominium       = entity.Condominium,
            Tenant            = entity.Tenant,
            Assembly          = entity,
            Name              = $"Convocazione: {entity.Name}",
            Description       = entity.Notes ?? string.Empty,
            CommunicationType = "Meeting",
            Priority          = "Normal",
            PublicationDate   = DateTime.UtcNow,
            ExpirationDate    = entity.ScheduledDate,
            SendEmail         = true,
            IsVisible         = false,
            IsArchived        = false,
        };
        communication.Trace(currentUser);
        await session.SaveAsync(communication, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        // Genera automaticamente le notifiche per tutti i proprietari del condominio
        var condominium = entity.Condominium;

        var template = await session.Query<NotificationTemplate>()
            .Where(t => t.Condominium.Id == condominium.Id
                     && t.CommunicationType == "Meeting"
                     && t.IsDefault && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        var owners = await session.Query<UnitOwner>()
            .Where(o => o.Unit.Condominium.Id == condominium.Id && o.IsActive && !o.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var addressParts = condominium.Address != null
            ? $"{condominium.Address.Street} {condominium.Address.StreetNumber}, {condominium.Address.PostalCode} {condominium.Address.City}"
            : string.Empty;

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
                AssemblyDate       = entity.ScheduledDate.ToString("dd/MM/yyyy HH:mm"),
                AssemblyLocation   = entity.Location ?? string.Empty,
                AssemblyType       = entity.AssemblyType?.Name ?? string.Empty,
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
                DeliveryMethod    = 0, // Email
                Status            = 0, // Draft
                EmailAddress      = owner.Email,
                PostalAddress     = addressParts,
                SubjectResolved   = subject,
                BodyResolved      = body,
            };
            notification.Trace(currentUser);
            await session.SaveAsync(notification, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
