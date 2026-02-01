using System.Net;
using DomuWave.Services.Command.Book;
using DomuWave.Services.Command.PaymentMethod;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Tag;
using DomuWave.Services.Settings;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using Microsoft.Extensions.Options;
using NHibernate.Linq;
using Remotion.Linq.Parsing;
using SimpleMediator.Core;

namespace DomuWave.Services.Implementations;

public class TagService : BaseService, ITagService
{
    
    private DomuWaveSettings _settings;
    private IMediator _mediator;
    private Dictionary<string, string> cacheKeys = new Dictionary<string, string>();

    public override string CacheRegion
    {
        get { return "Tag"; }
    }

    public TagService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache, IOptions<DomuWaveSettings> settings, IMediator mediator) : base(sessionFactoryProvider, cache)
    {
        _settings = settings.Value;
        _mediator = mediator;
    }

    public async Task<Tag> GetById(long itemId, long currentUserBookId, IUser currentUser, CancellationToken cancellationToken)
    {
        string ckey = $"GetById_{itemId}_{currentUserBookId}";
        Tag returnItem = null;
        if (cacheKeys.ContainsKey(ckey))
        {
            returnItem = _cache.Get<Tag>(ckey);
        }
        if (returnItem == null)
        {
            returnItem = await session.GetAsync<Tag>(itemId, cancellationToken).ConfigureAwait(false);
            if (returnItem == null)
            {
                throw new NotFoundException($"Tag con id {itemId} non trovato");
            }

            if (returnItem != null && returnItem.Book.Id != currentUserBookId )
            {
                throw new NotAllowedOperationException("Non hai accesso alla risorsa richiesta");
            }
            
            CanAccessToBookCommand canAccessToBookCommand = new CanAccessToBookCommand(currentUser.Id, currentUserBookId);
            var canAccess = await _mediator.GetResponse(canAccessToBookCommand, cancellationToken).ConfigureAwait(false);
            
            if (!canAccess)
            {
                throw new NotAllowedOperationException("Non hai accesso alla risorsa richiesta");
            }
            _cache.Set(CacheRegion,ckey, returnItem, _settings.CacheTimeouts.Tag);
        }

        return returnItem;
    }

    public async Task<IList<Tag>> GetAll(IUser currentUser, long? bookId, CancellationToken cancellationToken)
    {
        string ckey = $"GetAll_{bookId}";
        IList<Tag> returnList = null;
        if (cacheKeys.ContainsKey(ckey))
        {
            returnList = _cache.Get<IList<Tag>>(ckey);
        }
        else
        {
         
            returnList = session.Query<Tag>().FilterByBook(bookId.GetValueOrDefault())
                .ToList();

            _cache.Set(CacheRegion, ckey, returnList, _settings.CacheTimeouts.Tag);
        }

        return returnList;
    }

    public Task<Tag> Create(TagCreateUpdateDto dto, long currentUserBookId, IUser currentUser, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Tag> Update(long entityId, TagCreateUpdateDto updateDto, IUser currentUser, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Delete(long tagId, long bookId, IUser currentUser, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Tag> GetByName(string tagName, long bookId, IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.Query<Tag>()
                .FilterByBook(bookId)
                .Where(t => t.Name.ToLower() == tagName.ToLower())
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)

            ;
    }

    public async  Task<Tag> GetOrCreateTag(string tagName, long bookId, IUser currentUser, CancellationToken cancellationToken)
    {
        var tag = await this.GetByName(tagName, bookId, currentUser, cancellationToken).ConfigureAwait(false);
        if (tag != null)
        {
            return tag;
        }
        else
        {
            tag = new Tag()
            {
                Name = tagName,
                Book = await session.GetAsync<Book>(bookId, cancellationToken).ConfigureAwait(false)
            };
            tag.Trace(currentUser);
            await session.SaveAsync(tag, cancellationToken).ConfigureAwait(false);
            await this.ClearCache(cancellationToken).ConfigureAwait(false);
            return tag;
        }
    }

    public async Task<IList<Tag>> FindAll(string q, long bookId, IUser currentUser, CancellationToken cancellationToken)
    {
        var allTags = await GetAll(currentUser, bookId, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(q))
        {
            return allTags;
        }
        else
        {
            return allTags.Where(l=>l.Name.Contains(q)).ToList();
        }
    }

    public async  Task ClearCache(CancellationToken cancellationToken)
    {
       _cache.Clear(CacheRegion);
    }
}