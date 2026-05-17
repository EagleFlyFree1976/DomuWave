using CPQ.Core.Extensions;
using DomuWave.Services.Dto.PrivateThread;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class PrivateThreadMappingExtensions
{
    public static PrivateThreadReadDto ToReadDto(this PrivateThread entity, int messageCount = 0, int unreadCount = 0, string? lastMessageBody = null, DateTime? lastMessageDate = null)
    {
        if (entity == null) return null!;
        var dto = new PrivateThreadReadDto
        {
            CondominiumId          = entity.Condominium?.Id   ?? 0,
            CondominiumName        = entity.Condominium?.Name ?? string.Empty,
            CondominioUserId       = entity.CondominioUserId,
            CondominioUserFullName = entity.Name,
            MessageCount           = messageCount,
            UnreadCount            = unreadCount,
            LastMessageBody        = lastMessageBody,
            LastMessageDate        = lastMessageDate,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static PrivateThread ToEntity(this CreatePrivateThreadDto dto, Condominium condominium, Tenant tenant, long condominioUserId, string condominioUserFullName)
    {
        if (dto == null) return null!;
        return new PrivateThread
        {
            Condominium            = condominium,
            Tenant                 = tenant,
            CondominioUserId = condominioUserId,
            Name             = condominioUserFullName,
        };
    }

    public static PrivateMessageReadDto ToReadDto(this PrivateMessage entity)
    {
        if (entity == null) return null!;
        var dto = new PrivateMessageReadDto
        {
            ThreadId          = entity.Thread?.Id ?? 0,
            SenderUserId      = entity.SenderUserId,
            SenderFullName    = entity.SenderFullName,
            Body              = entity.Name,
            IsReadByRecipient = entity.IsReadByRecipient,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static PrivateMessage ToEntity(this CreatePrivateMessageDto dto, PrivateThread thread, Tenant tenant, long senderUserId, string senderFullName)
    {
        if (dto == null) return null!;
        return new PrivateMessage
        {
            Thread            = thread,
            Tenant            = tenant,
            SenderUserId      = senderUserId,
            SenderFullName    = senderFullName,
            Name              = dto.Body,
            IsReadByRecipient = false,
        };
    }
}
