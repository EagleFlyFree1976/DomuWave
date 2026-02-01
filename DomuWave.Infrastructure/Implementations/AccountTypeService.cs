using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class AccountTypeService : BaseService, IAccountTypeService
{
    public override string CacheRegion
    {
        get { return "AccountType"; }
    }
    public AccountTypeService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
    {
    }

    public async Task<AccountType> GetById(int itemId, IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.GetAsync<AccountType>(itemId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IList<AccountType>> GetAll(IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.Query<AccountType>().GetQueryable<AccountType, int>().ToListAsync(cancellationToken);
    }

    

    public Task Delete(int entityId, IUser currentUser, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task Validate(int? entityId, string code, string description, IUser currentUser,
        CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ValidatorException("Specificare il codice");
        }
        IQueryable<AccountType> accounts = session.Query<AccountType>().GetQueryable<AccountType, int>();


        if (entityId.HasValue)
        {
            accounts = accounts.Where(j => j.Id != entityId);
        }
        var existsByName = await accounts.Where(k => k.Description == code).AnyAsync(cancellationToken);
        if (existsByName)
        {
            throw new ValidatorException($"Esiste già una tipologia conto con questo nome");
        }

        if (string.IsNullOrEmpty(code))
        {
            throw new ValidatorException("Specificare il codice");
        }
        var existsByCode = await accounts.Where(j => j.Code == code).AnyAsync(cancellationToken);
        if (existsByCode)
        {
            throw new ValidatorException($"Esiste già una tipologia conto con questo codice");
        }


    }


    public async Task<AccountType> Create(AccountTypeCreateUpdateDto createDto, IUser currentUser, CancellationToken cancellationToken)
    {
        await Validate(null, createDto.Code, createDto.Description,  currentUser, cancellationToken).ConfigureAwait(false);



        AccountType newAccountType = new AccountType()
        {
            Description= createDto.Description,
            Code = createDto.Code
            
        };

        newAccountType.Trace(currentUser);

        await session.SaveAsync(newAccountType, cancellationToken).ConfigureAwait(false);

        return newAccountType;
    }

    public async Task<AccountType> Update(int entityId, AccountTypeCreateUpdateDto updateDto, IUser currentUser, CancellationToken cancellationToken)
    {
        AccountType editAccountType = await session.GetAsync<AccountType>(entityId, cancellationToken).ConfigureAwait(false);

        if (editAccountType == null)
        {
            throw new NotFoundException("Elemento non trovato");
        }
        await Validate(entityId, updateDto.Code, updateDto.Description,  currentUser, cancellationToken).ConfigureAwait(false);





        
        editAccountType.Code = updateDto.Code;
        editAccountType.Description = updateDto.Description;
        
        editAccountType.Trace(currentUser);

        await session.SaveAsync(editAccountType, cancellationToken).ConfigureAwait(false);

        return editAccountType;
    }
    
    
}