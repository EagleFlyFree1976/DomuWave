using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Communication;
using DomuWave.Services.Dto.Communication;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateCommunicationCommandConsumer : InMemoryConsumerBase<CreateCommunicationCommand, CommunicationReadDto>
{
    private readonly IUserService _userService;

    public CreateCommunicationCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<CommunicationReadDto> Consume(
        CreateCommunicationCommand command,
        IMediationContext           mediationContext,
        CancellationToken          cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Title))
            throw new ValidatorException("Il titolo è obbligatorio.");
        if (string.IsNullOrWhiteSpace(command.Dto.CommunicationType))
            throw new ValidatorException("Il tipo di comunicazione è obbligatorio.");

        var condominium = await session.GetAsync<Models.Condominium>(command.Dto.CondominiumId, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Condominio non trovato.");

        Models.CondominiumInstallment? installment = null;
        if (command.Dto.InstallmentId.HasValue)
        {
            installment = await session.GetAsync<Models.CondominiumInstallment>(command.Dto.InstallmentId.Value, cancellationToken).ConfigureAwait(false)
                          ?? throw new NotFoundException("Rata non trovata.");

            var alreadyExists = await session.Query<Models.Communication>()
                .AnyAsync(c => c.Installment != null && c.Installment.Id == command.Dto.InstallmentId.Value && !c.IsDeleted, cancellationToken)
                .ConfigureAwait(false);
            if (alreadyExists)
                throw new ValidatorException("Esiste già una comunicazione associata a questa rata.");
        }

        Models.Assembly? assembly = null;
        if (command.Dto.CommunicationType == "Meeting")
        {
            if (!command.Dto.AssemblyId.HasValue)
                throw new ValidatorException("Per le comunicazioni di tipo 'Assemblea' è obbligatorio selezionare un'assemblea.");

            assembly = await session.GetAsync<Models.Assembly>(command.Dto.AssemblyId.Value, cancellationToken).ConfigureAwait(false)
                       ?? throw new NotFoundException("Assemblea non trovata.");
        }

        var entity = command.Dto.ToEntity(condominium, installment, assembly);
        entity.Trace(currentUser);
        await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}
