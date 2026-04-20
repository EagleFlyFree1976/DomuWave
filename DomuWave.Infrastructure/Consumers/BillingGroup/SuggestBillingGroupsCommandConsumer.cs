using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.BillingGroup;
using DomuWave.Services.Dto.BillingGroup;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class SuggestBillingGroupsCommandConsumer
    : InMemoryConsumerBase<SuggestBillingGroupsCommand, SuggestBillingGroupsResultDto>
{
    private readonly IUserService _userService;

    public SuggestBillingGroupsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<SuggestBillingGroupsResultDto> Consume(
        SuggestBillingGroupsCommand command,
        IMediationContext             mediationContext,
        CancellationToken            cancellationToken)
    {
        await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // 1. Load all active units for the condominium
        var units = await session.Query<RealEstateUnit>()
            .Where(u => u.Condominium.Id == command.CondominiumId && u.IsActive && !u.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!units.Any())
            return new SuggestBillingGroupsResultDto();

        var unitIds = units.Select(u => u.Id).ToList();

        // 2. Load all active owners for these units
        var owners = await session.Query<UnitOwner>()
            .Where(o => unitIds.Contains(o.Unit.Id) && o.IsActive && !o.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // 3. Build owner-signature per unit: sorted set of owner last+first names
        //    Two units are non-ambiguous only if they share the EXACT same set of owners
        var ownersByUnit = owners
            .GroupBy(o => o.Unit.Id)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(o => o.LastName).ThenBy(o => o.FirstName).ToList());

        // Compute a canonical signature for each unit (comma-joined full names)
        string Signature(int unitId)
        {
            if (!ownersByUnit.TryGetValue(unitId, out var list) || list.Count == 0)
                return $"__no_owner_{unitId}__";
            return string.Join("|", list.Select(o => $"{o.LastName},{o.FirstName}".ToUpperInvariant()));
        }

        // 4. Group units by signature
        var grouped = units
            .GroupBy(u => Signature(u.Id))
            .ToList();

        var suggestions  = new List<BillingGroupSuggestionDto>();
        var ambiguous    = new List<BillingGroupUnitDto>();

        // 5. Detect ambiguous owners: owners that appear in multiple signatures
        //    (i.e. same person is co-owner of units with different partner sets)
        var ownerSignatureMap = new Dictionary<string, HashSet<string>>(); // ownerKey → signatures
        foreach (var group in grouped)
        {
            foreach (var unit in group)
            {
                if (!ownersByUnit.TryGetValue(unit.Id, out var unitOwners)) continue;
                foreach (var o in unitOwners)
                {
                    var ownerKey = $"{o.LastName},{o.FirstName}".ToUpperInvariant();
                    if (!ownerSignatureMap.ContainsKey(ownerKey))
                        ownerSignatureMap[ownerKey] = new HashSet<string>();
                    ownerSignatureMap[ownerKey].Add(group.Key);
                }
            }
        }

        // Owners that appear in more than one signature group are "ambiguous"
        var ambiguousOwnerKeys = ownerSignatureMap
            .Where(kv => kv.Value.Count > 1)
            .Select(kv => kv.Key)
            .ToHashSet();

        var ambiguousSignatures = new HashSet<string>();
        foreach (var group in grouped)
        {
            foreach (var unit in group)
            {
                if (!ownersByUnit.TryGetValue(unit.Id, out var unitOwners)) continue;
                if (unitOwners.Any(o => ambiguousOwnerKeys.Contains($"{o.LastName},{o.FirstName}".ToUpperInvariant())))
                {
                    ambiguousSignatures.Add(group.Key);
                    break;
                }
            }
        }

        // 6. Build result
        foreach (var group in grouped)
        {
            var groupUnits = group.ToList();

            if (ambiguousSignatures.Contains(group.Key))
            {
                // Ambiguous: expose all units individually for manual resolution
                ambiguous.AddRange(groupUnits.Select(u => u.ToBillingGroupUnitDto()));
                continue;
            }

            if (groupUnits.Count == 1)
            {
                // Single unit with unique owners → no group needed, individual notice
                continue;
            }

            // Multiple units, same owner set → suggest a group
            ownersByUnit.TryGetValue(groupUnits.First().Id, out var repOwners);
            var suggestedName  = repOwners?.Count > 0
                ? string.Join(" / ", repOwners.Select(o => $"{o.LastName} {o.FirstName}".Trim()))
                : groupUnits.First().DisplayName ?? group.Key;
            var contactEmail   = repOwners?.FirstOrDefault()?.Email ?? string.Empty;

            suggestions.Add(new BillingGroupSuggestionDto
            {
                SuggestedName = suggestedName,
                ContactEmail  = contactEmail,
                Units         = groupUnits.Select(u => u.ToBillingGroupUnitDto()).ToList(),
            });
        }

        return new SuggestBillingGroupsResultDto
        {
            Suggestions    = suggestions,
            AmbiguousUnits = ambiguous,
        };
    }
}
