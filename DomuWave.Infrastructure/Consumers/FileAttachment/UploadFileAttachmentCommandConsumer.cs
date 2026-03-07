using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.FileAttachment;
using DomuWave.Services.Dto.FileAttachment;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.FileAttachment;

public class UploadFileAttachmentCommandConsumer
    : InMemoryConsumerBase<UploadFileAttachmentCommand, FileAttachmentReadDto>
{
    private readonly ITenantService _tenantService;
    private readonly IUserService   _userService;

    public UploadFileAttachmentCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ITenantService tenantService,
        IUserService   userService) : base(sessionFactoryProvider)
    {
        _tenantService = tenantService;
        _userService   = userService;
    }

    protected override async Task<FileAttachmentReadDto> Consume(
        UploadFileAttachmentCommand command,
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
            throw new NotFoundException("Tenant non trovato");

        var dto = command.Dto;

        if (string.IsNullOrWhiteSpace(dto.EntityKey))
            throw new ValidatorException("EntityKey e' obbligatorio");
        if (dto.EntityId <= 0)
            throw new ValidatorException("EntityId non valido");
        if (string.IsNullOrWhiteSpace(dto.FileName))
            throw new ValidatorException("Il nome del file e' obbligatorio");
        if (string.IsNullOrWhiteSpace(dto.Base64Data))
            throw new ValidatorException("Il contenuto del file e' obbligatorio");

        var entity = dto.ToEntity(tenant);
        entity.Trace(currentUser);
        entity.Content.Trace(currentUser);

        await session.SaveAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.SaveAsync(entity.Content, cancellationToken).ConfigureAwait(false);

        return entity.ToReadDto();
    }
}
