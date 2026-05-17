using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DomuWave.Services.Consumers;

public class DeleteCondominiumCommandConsumer : InMemoryConsumerBase<DeleteCondominiumCommand, bool>
{
    private readonly ICondominiumService _condominiumService;
    private readonly IUserService        _userService;

    public DeleteCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        ICondominiumService condominiumService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _condominiumService = condominiumService;
        _userService        = userService;
    }

    protected override async Task<bool> Consume(
        DeleteCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var cid = command.CondominiumId;

        // ── Leaf children of RealEstateUnit ───────────────────────────
        var unitIds = await session.Query<RealEstateUnit>()
            .Where(x => x.Condominium.Id == cid && !x.IsDeleted)
            .Select(x => x.Id).ToListAsync(cancellationToken).ConfigureAwait(false);

        if (unitIds.Any())
        {
            await SoftDeleteInt<UnitOwner>(
                session.Query<UnitOwner>().Where(x => unitIds.Contains(x.Unit.Id) && !x.IsDeleted),
                currentUser, cancellationToken);

            await SoftDeleteInt<UnitTenant>(
                session.Query<UnitTenant>().Where(x => unitIds.Contains(x.Unit.Id) && !x.IsDeleted),
                currentUser, cancellationToken);

            await SoftDeleteInt<UnitMillesimal>(
                session.Query<UnitMillesimal>().Where(x => unitIds.Contains(x.Unit.Id) && !x.IsDeleted),
                currentUser, cancellationToken);

            await SoftDeleteLong<ExpenseAllocation>(
                session.Query<ExpenseAllocation>().Where(x => unitIds.Contains(x.Unit.Id) && !x.IsDeleted),
                currentUser, cancellationToken);
        }

        // ── Leaf children of CondominiumInstallment ───────────────────
        var installmentIds = await session.Query<CondominiumInstallment>()
            .Where(x => x.Condominium.Id == cid && !x.IsDeleted)
            .Select(x => x.Id).ToListAsync(cancellationToken).ConfigureAwait(false);

        if (installmentIds.Any())
        {
            await SoftDeleteLong<CondominiumFee>(
                session.Query<CondominiumFee>().Where(x => installmentIds.Contains(x.Installment.Id) && !x.IsDeleted),
                currentUser, cancellationToken);
        }

        // ── Direct condominium children ───────────────────────────────
        await SoftDeleteInt<RealEstateUnit>(
            session.Query<RealEstateUnit>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteInt<CondominiumInstallment>(
            session.Query<CondominiumInstallment>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteInt<FiscalYear>(
            session.Query<FiscalYear>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteInt<Budget>(
            session.Query<Budget>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteLong<Expense>(
            session.Query<Expense>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteInt<MillesimalTable>(
            session.Query<MillesimalTable>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteInt<Models.ExtraordinaryWork>(
            session.Query<Models.ExtraordinaryWork>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteInt<Models.Maintenance>(
            session.Query<Models.Maintenance>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteLong<Receipt>(
            session.Query<Receipt>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteLong<Payment>(
            session.Query<Payment>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteInt<Models.Document>(
            session.Query<Models.Document>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteInt<Communication>(
            session.Query<Communication>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteLong<Message>(
            session.Query<Message>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteInt<CondominiumAddress>(
            session.Query<CondominiumAddress>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteInt<CondominiumCadastralData>(
            session.Query<CondominiumCadastralData>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteInt<ChartOfAccounts>(
            session.Query<ChartOfAccounts>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        await SoftDeleteInt<Models.SupplierContract>(
            session.Query<Models.SupplierContract>().Where(x => x.Condominium.Id == cid && !x.IsDeleted),
            currentUser, cancellationToken);

        // ── Condominium itself ────────────────────────────────────────
        return await _condominiumService
            .DeleteAsync(cid, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task SoftDeleteInt<T>(IQueryable<T> query, IUser currentUser, CancellationToken ct)
        where T : TenantEntity<int>
    {
        var items = await query.ToListAsync(ct).ConfigureAwait(false);
        foreach (var item in items)
        {
            item.Trace(currentUser);
            item.IsDeleted = true;
            await session.SaveOrUpdateAsync(item, ct).ConfigureAwait(false);
        }
        if (items.Count > 0)
            await session.FlushAsync(ct).ConfigureAwait(false);
    }

    private async Task SoftDeleteLong<T>(IQueryable<T> query, IUser currentUser, CancellationToken ct)
        where T : TenantEntity<long>
    {
        var items = await query.ToListAsync(ct).ConfigureAwait(false);
        foreach (var item in items)
        {
            item.Trace(currentUser);
            item.IsDeleted = true;
            await session.SaveOrUpdateAsync(item, ct).ConfigureAwait(false);
        }
        if (items.Count > 0)
            await session.FlushAsync(ct).ConfigureAwait(false);
    }
}
