using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class DeleteQuoteDocumentCommand : BaseCommand, IQuery<bool>
{
    public int DocumentId { get; }
    public DeleteQuoteDocumentCommand(int currentUserId, int documentId) : base(currentUserId)
        => DocumentId = documentId;
}
