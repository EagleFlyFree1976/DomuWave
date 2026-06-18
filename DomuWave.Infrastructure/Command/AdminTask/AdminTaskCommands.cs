using DomuWave.Services.Dto.AdminTask;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.AdminTask;

public class CreateAdminTaskCommand : BaseCommand, IQuery<AdminTaskReadDto>
{
    public Guid               TenantId { get; }
    public CreateAdminTaskDto Dto      { get; }
    public CreateAdminTaskCommand(int currentUserId, Guid tenantId, CreateAdminTaskDto dto) : base(currentUserId)
    {
        TenantId = tenantId;
        Dto      = dto;
    }
}

public class UpdateAdminTaskCommand : BaseCommand, IQuery<AdminTaskReadDto>
{
    public int                Id  { get; }
    public UpdateAdminTaskDto Dto { get; }
    public UpdateAdminTaskCommand(int currentUserId, int id, UpdateAdminTaskDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}

public class DeleteAdminTaskCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; }
    public DeleteAdminTaskCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

public class CompleteAdminTaskCommand : BaseCommand, IQuery<AdminTaskReadDto>
{
    public int Id { get; }
    public CompleteAdminTaskCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

public class GetAdminTaskByIdCommand : BaseCommand, IQuery<AdminTaskReadDto?>
{
    public int Id { get; }
    public GetAdminTaskByIdCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

public class GetAdminTasksByTenantCommand : BaseCommand, IQuery<IList<AdminTaskReadDto>>
{
    public Guid      TenantId         { get; }
    public int?      AssignedToUserId { get; }
    public int?      StatusId         { get; }
    public DateTime? DueBefore        { get; }
    public GetAdminTasksByTenantCommand(int currentUserId, Guid tenantId, int? assignedToUserId, int? statusId, DateTime? dueBefore) : base(currentUserId)
    {
        TenantId         = tenantId;
        AssignedToUserId = assignedToUserId;
        StatusId         = statusId;
        DueBefore        = dueBefore;
    }
}
