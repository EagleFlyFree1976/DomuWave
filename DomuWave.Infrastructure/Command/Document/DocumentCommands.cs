using DomuWave.Services.Dto.Document;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Document;

public class GetDocumentsByCondominiumCommand : BaseCommand, IQuery<IList<DocumentReadDto>>
{
    public int CondominiumId { get; set; }
    public GetDocumentsByCondominiumCommand() { }
    public GetDocumentsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}

public class GetDocumentByIdCommand : BaseCommand, IQuery<DocumentReadDto?>
{
    public int Id { get; set; }
    public GetDocumentByIdCommand() { }
    public GetDocumentByIdCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}

public class DownloadDocumentCommand : BaseCommand, IQuery<(byte[] Content, string FileName, string MimeType)>
{
    public int Id { get; set; }
    public DownloadDocumentCommand() { }
    public DownloadDocumentCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}

public class UploadDocumentCommand : BaseCommand, IQuery<DocumentReadDto>
{
    public UploadDocumentDto Dto { get; set; } = null!;
    public UploadDocumentCommand() { }
    public UploadDocumentCommand(int currentUserId, UploadDocumentDto dto) : base(currentUserId)
        => Dto = dto;
}

public class UpdateDocumentCommand : BaseCommand, IQuery<DocumentReadDto?>
{
    public int               Id  { get; set; }
    public UpdateDocumentDto Dto { get; set; } = null!;
    public UpdateDocumentCommand() { }
    public UpdateDocumentCommand(int currentUserId, int id, UpdateDocumentDto dto) : base(currentUserId)
    { Id = id; Dto = dto; }
}

public class DeleteDocumentCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }
    public DeleteDocumentCommand() { }
    public DeleteDocumentCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
