using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using Remotion.Linq.Parsing;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateCondominiumCommandConsumer : InMemoryConsumerBase<CreateCondominiumCommand, CondominiumReadDto>
{
    private readonly ICondominiumService     _condominiumService;
    private readonly ITenantService          _tenantService;
    private readonly IUserService            _userService;
    private readonly IMillesimalTableService _millesimalTableService;
    private readonly IChartOfAccountsService _accountService;

    public CreateCondominiumCommandConsumer(
        ISessionFactoryProvider  sessionFactoryProvider,
        ICondominiumService      condominiumService,
        ITenantService           tenantService,
        IUserService             userService,
        IMillesimalTableService  millesimalTableService,
        IChartOfAccountsService  accountService) : base(sessionFactoryProvider)
    {
        _condominiumService     = condominiumService;
        _tenantService          = tenantService;
        _userService            = userService;
        _millesimalTableService = millesimalTableService;
        _accountService         = accountService;
    }

    protected override async Task<CondominiumReadDto> Consume(
        CreateCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var tenant = await _tenantService
            .GetByIdAsync(command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (tenant == null)
            throw new NotFoundException($"Dati non validi");
        
        var entity = command.Dto.ToEntity(tenant);
        
        /* validazioni */
        // verifico l'univocit� del codice condominio

        if (string.IsNullOrEmpty(command.Dto.Code.Trim()))
        {
            throw new ValidatorException($"Specificare il codice condominio");
        }

        var existsCode = await session.Query<Models.Condominium>()
            .Where(k => k.Code == command.Dto.Code.Trim() && k.Tenant.Id == command.TenantId && !k.IsDeleted).AnyAsync(cancellationToken).ConfigureAwait(false);

        if (existsCode)
        {
            throw new ValidatorException($"Esiste gi� un condominio con il codice {command.Dto.Code}");
        }


            var created = await _condominiumService
            .CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        var defTable = new MillesimalTable
        {
            Tenant          = tenant,
            Condominium     = created,
            Code            = "DEF",
            Name            = "Default",
            TotalMillesimal = 1000,
            IsActive        = false,
            IsDraft         = true,
            IsEnabled       = true,
            IsDefault       = true,
        };
        await _millesimalTableService
            .CreateAsync(defTable, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Crea il piano dei conti dal template di default del tenant
        var defaultTemplate = await session.Query<ChartOfAccountsTemplate>()
            .FirstOrDefaultAsync(t => t.Tenant.Id == tenant.Id && t.IsDefault && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (defaultTemplate != null)
        {
            var templateItems = await session.Query<ChartOfAccountsTemplateItem>()
                .Where(i => i.Template.Id == defaultTemplate.Id && !i.IsDeleted && i.IsActive)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Code)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var idMap = new Dictionary<int, ChartOfAccounts>();
            foreach (var item in templateItems)
            {
                ChartOfAccounts? parent = null;
                if (item.ParentItem != null)
                    idMap.TryGetValue(item.ParentItem.Id, out parent);

                var account = new ChartOfAccounts
                {
                    Condominium   = created,
                    Tenant        = tenant,
                    ParentAccount = parent,
                    Code          = item.Code,
                    Name          = item.Name,
                    Type          = item.Type,
                    Level         = parent != null ? parent.Level + 1 : 1,
                    IsActive      = true,
                    IsDeleted     = false,
                };
                account.Trace(currentUser);

                var saved = await _accountService
                    .CreateAsync(account, currentUser, cancellationToken)
                    .ConfigureAwait(false);

                idMap[item.Id] = saved;
            }
        }

        // Crea i template di notifica standard
        await SeedNotificationTemplatesAsync(created, tenant, currentUser, cancellationToken);

        return created.ToReadDto();
    }

    private async Task SeedNotificationTemplatesAsync(
        Models.Condominium condominium,
        Models.Tenant             tenant,
        IUser              currentUser,
        CancellationToken  cancellationToken)
    {
        var defaults = new[]
        {
            new
            {
                Type    = "FeeNotice",
                Name    = "Avviso di pagamento quote",
                Subject = "Avviso di pagamento quote condominiali — {{CondominiumName}}",
                Body    =
                    "Gentile {{RecipientName}},\n\n" +
                    "Le ricordiamo che risultano le seguenti quote condominiali a Suo carico relative all'unità {{UnitNumber}}:\n\n" +
                    "{{FeeTable}}\n\n" +
                    "Totale da versare: {{TotalAmount}}\n\n" +
                    "Il pagamento dovrà essere effettuato tramite bonifico bancario al seguente IBAN:\n" +
                    "{{Iban}}\n" +
                    "causale: Quote condominiali — {{CondominiumName}} — {{FiscalYearCode}}\n\n" +
                    "Per qualsiasi chiarimento non esiti a contattarci.\n\n" +
                    "Cordiali saluti,\n" +
                    "{{AdministratorName}}\n" +
                    "{{AdministratorEmail}}\n" +
                    "{{AdministratorPhone}}",
            },
            new
            {
                Type    = "Notice",
                Name    = "Avviso generico",
                Subject = "{{CommunicationTitle}} — {{CondominiumName}}",
                Body    =
                    "Gentile {{RecipientName}},\n\n" +
                    "{{CommunicationBody}}\n\n" +
                    "Cordiali saluti,\n" +
                    "{{AdministratorName}}\n" +
                    "{{AdministratorEmail}}\n" +
                    "{{AdministratorPhone}}",
            },
            new
            {
                Type    = "Meeting",
                Name    = "Convocazione assemblea",
                Subject = "Convocazione assemblea condominiale — {{CondominiumName}}",
                Body    =
                    "Gentile {{RecipientName}},\n\n" +
                    "La informiamo che è stata convocata un'assemblea condominiale.\n\n" +
                    "{{CommunicationBody}}\n\n" +
                    "La Sua presenza è gradita.\n\n" +
                    "Cordiali saluti,\n" +
                    "{{AdministratorName}}\n" +
                    "{{AdministratorEmail}}\n" +
                    "{{AdministratorPhone}}",
            },
            new
            {
                Type    = "Maintenance",
                Name    = "Comunicazione lavori",
                Subject = "Comunicazione lavori — {{CondominiumName}}",
                Body    =
                    "Gentile {{RecipientName}},\n\n" +
                    "La informiamo che sono previsti i seguenti interventi di manutenzione:\n\n" +
                    "{{CommunicationBody}}\n\n" +
                    "Ci scusiamo per gli eventuali disagi.\n\n" +
                    "Cordiali saluti,\n" +
                    "{{AdministratorName}}\n" +
                    "{{AdministratorEmail}}\n" +
                    "{{AdministratorPhone}}",
            },
            new
            {
                Type    = "Emergency",
                Name    = "Comunicazione urgente",
                Subject = "URGENTE — {{CommunicationTitle}} — {{CondominiumName}}",
                Body    =
                    "Gentile {{RecipientName}},\n\n" +
                    "COMUNICAZIONE URGENTE\n\n" +
                    "{{CommunicationBody}}\n\n" +
                    "La preghiamo di prenderne immediata visione.\n\n" +
                    "{{AdministratorName}}\n" +
                    "{{AdministratorEmail}}\n" +
                    "{{AdministratorPhone}}",
            },
            new
            {
                Type    = "Info",
                Name    = "Comunicazione informativa",
                Subject = "{{CommunicationTitle}} — {{CondominiumName}}",
                Body    =
                    "Gentile {{RecipientName}},\n\n" +
                    "La informiamo che:\n\n" +
                    "{{CommunicationBody}}\n\n" +
                    "Cordiali saluti,\n" +
                    "{{AdministratorName}}\n" +
                    "{{AdministratorEmail}}\n" +
                    "{{AdministratorPhone}}",
            },
        };

        foreach (var d in defaults)
        {
            var tmpl = new NotificationTemplate
            {
                Condominium       = condominium,
                Tenant            = tenant,
                Name              = d.Name,
                CommunicationType = d.Type,
                SubjectTemplate   = d.Subject,
                BodyTemplate      = d.Body,
                IsDefault         = true,
            };
            tmpl.Trace(currentUser);
            await session.SaveAsync(tmpl, cancellationToken).ConfigureAwait(false);
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
