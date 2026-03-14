# DomuWave — Project Guide for Claude

## Project Overview

DomuWave is a condominium management SaaS platform. Multi-tenant architecture.
- **Backend**: ASP.NET Core + NHibernate + SimpleMediator (CQRS)
- **Frontend**: Vue 3 Composition API + Vite + Pinia
- **Database**: SQL Server (T-SQL always — never generic SQL)

---

## Project Structure

```
DomuWave.Infrastructure/    ← Domain layer: Models, Commands, Consumers, DTOs, Services
DomuWave.Application/       ← ASP.NET Core: Controllers, Filters, DI config
DomuWave.Web/               ← Frontend host + Vue app (ClientApp/src/)
docs/migrations/            ← SQL Server migration scripts
```

---

## Backend Patterns

### CQRS — Commands

**Location**: `DomuWave.Infrastructure/Command/<Domain>/`

```csharp
public class CreateFooCommand : BaseCommand, IQuery<FooReadDto>
{
    public CreateFooDto Dto { get; set; }

    public CreateFooCommand() { }
    public CreateFooCommand(int currentUserId, CreateFooDto dto) : base(currentUserId)
        => Dto = dto;
}

public class UpdateFooCommand : BaseCommand, IQuery<FooReadDto>
{
    public int Id { get; set; }
    public UpdateFooDto Dto { get; set; }

    public UpdateFooCommand() { }
    public UpdateFooCommand(int currentUserId, int id, UpdateFooDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
```

Rules:
- Always inherit `BaseCommand` + implement `IQuery<TResult>`
- `CurrentUserId` always passed via constructor
- Commands are data containers only — no logic

---

### CQRS — Consumers

**Location**: `DomuWave.Infrastructure/Consumers/<Domain>/`

```csharp
public class CreateFooCommandConsumer : InMemoryConsumerBase<CreateFooCommand, FooReadDto>
{
    private readonly IFooService    _fooService;
    private readonly IUserService   _userService;

    public CreateFooCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IFooService fooService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _fooService  = fooService;
        _userService = userService;
    }

    protected override async Task<FooReadDto> Consume(
        CreateFooCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        // 1. Always fetch currentUser first
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // 2. Validate (throw NotFoundException / ValidatorException)
        if (string.IsNullOrWhiteSpace(command.Dto.Name))
            throw new ValidatorException("Il nome è obbligatorio.");

        // 3. Use session for direct NHibernate queries
        var duplicate = await session.Query<Foo>()
            .AnyAsync(x => x.Name == command.Dto.Name && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        if (duplicate) throw new ValidatorException("Esiste già un elemento con questo nome.");

        // 4. Create entity, trace, save
        var entity = command.Dto.ToEntity(parent, currentUser.Tenant);
        entity.Trace(currentUser);
        var created = await _fooService.CreateAsync(entity, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // 5. Return DTO
        return created.ToReadDto();
    }
}
```

Rules:
- Inject `ISessionFactoryProvider` (required by base) + domain services + `IUserService`
- `session` property available from `InMemoryConsumerBase` for direct NHibernate queries
- Always `.ConfigureAwait(false)` on awaited calls
- `entity.Trace(currentUser)` on new entities; `entity.TraceUpdate(currentUser)` on updates (usually called inside service)
- Flush manually when needed: `await session.FlushAsync(cancellationToken)`
- Return DTOs, never raw entities

---

### DTOs

**Location**: `DomuWave.Infrastructure/Dto/<Domain>/`

Three files per domain:

```csharp
// FooReadDto.cs — extends TraceEntityDTO<int>
public class FooReadDto : TraceEntityDTO<int>
{
    public int    ParentId   { get; set; }   // FK id
    public string ParentName { get; set; }   // FK display
    public string Name       { get; set; }
    public int    StatusId   { get; set; }   // lookup FK id
    public string StatusName { get; set; }   // lookup display
}

// CreateFooDto.cs — only fields settable on creation
public class CreateFooDto
{
    public int    ParentId { get; set; }
    public string Name     { get; set; }
}

// UpdateFooDto.cs — only fields editable after creation
public class UpdateFooDto
{
    public string? Name  { get; set; }
    public string? Notes { get; set; }
}
```

Rules:
- Read DTOs always extend `TraceEntityDTO<int>` (from `CPQ.Core.DTO`)
- Include both FK id AND display name in Read DTOs (`CondominiumId` + `CondominiumName`)
- Status/lookup fields: `StatusId` (int) + `StatusName` (string) in Read DTOs
- Nullable types for optional fields (`string?`, `decimal?`, `DateTime?`)

---

### Mapping Extensions

**Location**: `DomuWave.Infrastructure/Interfaces/Extensions/<Domain>MappingExtensions.cs`

```csharp
public static class FooMappingExtensions
{
    public static FooReadDto ToReadDto(this Foo entity)
    {
        if (entity == null) return null;
        var dto = new FooReadDto
        {
            ParentId   = entity.Parent?.Id   ?? 0,
            ParentName = entity.Parent?.Name,
            Name       = entity.Name,
            StatusId   = entity.Status?.Id   ?? 0,
            StatusName = entity.Status?.Name ?? string.Empty,
        };
        dto.SetTraceInfo(entity);   // ALWAYS call this
        return dto;
    }

    public static Foo ToEntity(this CreateFooDto dto, Parent parent, Tenant tenant)
    {
        if (dto == null) return null;
        return new Foo
        {
            Parent = parent,
            Tenant = tenant,
            Name   = dto.Name,
        };
    }

    public static void ApplyUpdate(this Foo entity, UpdateFooDto dto)
    {
        entity.Name  = dto.Name;
        entity.Notes = dto.Notes;
    }
}
```

Rules:
- Three methods: `ToReadDto()`, `ToEntity(dto, parent, tenant)`, `ApplyUpdate(entity, dto)`
- `ToReadDto` always calls `dto.SetTraceInfo(entity)` at the end
- `ApplyUpdate` is void — modifies entity in place
- Null-coalesce FK navigation: `entity.Parent?.Id ?? 0`

---

### Models

**Location**: `DomuWave.Infrastructure/Models/`

```csharp
public class Foo : TenantEntity<int>
{
    public virtual Parent     Parent  { get; set; }
    public virtual FooStatus  Status  { get; set; }
    public virtual string     Name    { get; set; }
    public virtual decimal    Amount  { get; set; }
    public virtual bool       IsActive { get; set; }

    public virtual IList<Bar> Bars { get; set; } = new List<Bar>();

    public override int GetHashCode() => Id.GetHashCode();
}
```

Rules:
- Inherit `TenantEntity<int>` (provides `Tenant`, `Name`, `Description`, and all trace fields)
- All properties and navigations are `virtual` (required by NHibernate proxies)
- Initialize collections inline: `= new List<Bar>()`
- Override `GetHashCode()`

---

### NHibernate Mappings (.hbm.xml)

**Location**: `DomuWave.Infrastructure/Models/Mappings/`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<hibernate-mapping xmlns="urn:nhibernate-mapping-2.2"
                   assembly="DomuWave.Services"
                   namespace="DomuWave.Services.Models">

  <class name="DomuWave.Services.Models.Foo, DomuWave.Services" table="Foo" lazy="true">

    <id name="Id" column="Id" type="Int32">
      <generator class="hilo">
        <param name="table">hibernate_unique_key</param>
        <param name="column">next_hi</param>
        <param name="max_lo">10</param>
        <param name="where">entity_type='Foo'</param>
      </generator>
    </id>

    <!-- FKs — regular navigation: lazy (default) -->
    <many-to-one name="Tenant"  class="DomuWave.Services.Models.Tenant, DomuWave.Services"  column="TenantId"  not-null="true" />
    <many-to-one name="Parent"  class="DomuWave.Services.Models.Parent, DomuWave.Services"  column="ParentId"  not-null="true" />

    <!-- Lookup FK — always fetch="join" (eager) -->
    <many-to-one name="Status"  class="DomuWave.Services.Models.FooStatus, DomuWave.Services" column="StatusId" not-null="true" lazy="false" fetch="join" />

    <!-- Simple properties -->
    <property name="Name"     column="Name"     type="String"  length="200" not-null="true" />
    <property name="Amount"   column="Amount"   type="Decimal" not-null="true" />
    <property name="IsActive" column="IsActive" type="Boolean" not-null="true" />

    <!-- Read-only alias (same column, different C# property name) -->
    <property name="AliasName" column="Name" type="String" length="200" insert="false" update="false" />

    <!-- Enum stored as int -->
    <property name="FooType" column="FooType" type="Int32" not-null="true" />

    <!-- Trace fields — always at the end, always in this order -->
    <property name="CreatedBy"             column="CreatedById"           type="Int32"     not-null="true" />
    <property name="CreatedByFullName"     column="CreatedByFullName"     type="String"    length="200" />
    <property name="LastUpdatedBy"         column="LastUpdatedById"       type="Int32" />
    <property name="LastUpdatedByFullName" column="LastUpdatedByFullName" type="String"    length="200" />
    <property name="IsDeleted"             column="IsDeleted"             type="Boolean"   not-null="true" />
    <property name="IsEnabled"             column="IsDeleted"             type="Boolean"   insert="false" update="false" />
    <property name="CreationDate"          column="CreationDate"          type="datetime2" not-null="true" />
    <property name="LastUpdateDate"        column="LastUpdateDate"        type="datetime2" />

    <!-- Collections -->
    <bag name="Bars" inverse="true" cascade="all-delete-orphan" lazy="true">
      <key column="FooId" />
      <one-to-many class="DomuWave.Services.Models.Bar, DomuWave.Services" />
    </bag>

  </class>

</hibernate-mapping>
```

Rules:
-- Embedded resource: the build action of file *hbm.xml is ever Embedded resource
- `entity_type` in hilo `where` clause must exactly match the C# class name
- Lookup/status FKs: always `lazy="false" fetch="join"`
- Dates: always `type="datetime2"` (not `datetime`)
- Enums: `type="Int32"` (no C# entity needed for the lookup — that's a separate SQL table)
- Read-only duplicate mapping: `insert="false" update="false"`
- Trace fields always in this exact order at the end
- Collections: owned children use `cascade="all-delete-orphan"`; references use no cascade

---

### Services

**Location**:
- Interfaces: `DomuWave.Infrastructure/Interfaces/I<Domain>Service.cs`
- Implementations: `DomuWave.Infrastructure/Implementations/<Domain>Service.cs`

```csharp
public interface IFooService : IBaseService<Foo, int>
{
    Task<IList<Foo>> GetByParentAsync(int parentId, IUser currentUser, CancellationToken ct);
}

public class FooService : BaseService, IFooService
{
    public FooService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "Foos";

    public async Task<IList<Foo>> GetByParentAsync(int parentId, IUser currentUser, CancellationToken ct)
        => await session.Query<Foo>()
            .Where(x => x.Parent.Id == parentId && !x.IsDeleted)
            .ToListAsync(ct);
}
```

Rules:
- All service methods accept `IUser currentUser` and `CancellationToken cancellationToken`
- Soft delete filter: `!x.IsDeleted` on every query
- Services inherit from `BaseService` and must define `CacheRegion`
- Services return entities — DTOs are the consumer's responsibility

---

### Controllers

**Location**: `DomuWave.Application/Controllers/`

```csharp
using SimpleMediator.Core;  // REQUIRED — always include this using

[Route("api/foos")]
[Produces("application/json")]
public class FoosController(
    ILogger<FoosController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    // TenantGuid helper (when needed)
    private Guid TenantGuid => Guid.Parse(HttpContext.Items["TenantId"]?.ToString() ?? Guid.Empty.ToString());

    [HttpGet("by-parent/{parentId:int}")]
    [ProducesResponseType(typeof(IList<FooReadDto>), 200)]
    public async Task<IActionResult> GetByParent(int parentId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetFoosByParentCommand(CurrentUser.Id, parentId), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FooReadDto), 200)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetFooByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FooReadDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateFooDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateFooCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(FooReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFooDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateFooCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteFooCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
```

Rules:
- **Always** `using SimpleMediator.Core;` in controllers that use `IMediator`
- Primary constructor syntax (C# 12)
- `CurrentUser.Id` (int) from `PrivateControllerBase`
- `TenantGuid` from `HttpContext.Items["TenantId"]` — used when command needs tenant scope
- `CancellationToken ct` on every action
- `[ProducesResponseType]` on every action
- Mediator call: `await _mediator.GetResponse(command, ct)`

---

### Error Handling

```csharp
using CPQ.Core.Exceptions;

// Validation errors → HTTP 400
throw new ValidatorException("Messaggio di errore in italiano.");

// Not found → HTTP 404
throw new NotFoundException("Elemento non trovato.");
```

---

## Database Conventions

### General Rules
- **Always T-SQL** — never generic SQL
- Booleans: `BIT NOT NULL DEFAULT 0`
- Dates: `DATETIME2`
- Money: `DECIMAL(18,4)`
- Text: `NVARCHAR(n)`
- TenantId column type: `uniqueidentifier NOT NULL`
- Auto-increment IDs: via NHibernate hilo — NOT `IDENTITY`

### Lookup Tables — ALWAYS required for FK integer columns

**Never** add a bare `INT` column without the corresponding lookup table. Pattern:

```sql
-- 1. Create lookup table
CREATE TABLE FooTypeLookup (
    Id   INT          NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

-- 2. Seed values
INSERT INTO FooTypeLookup (Id, Name) VALUES
    (0, 'TypeA'),
    (1, 'TypeB'),
    (2, 'TypeC');

-- 3. Add column + FK
ALTER TABLE Foo ADD FooTypeId INT NOT NULL DEFAULT 0;
ALTER TABLE Foo ADD CONSTRAINT FK_Foo_FooTypeLookup
    FOREIGN KEY (FooTypeId) REFERENCES FooTypeLookup(Id);
```

### Naming Conventions
- Tables: PascalCase (`RealEstateUnit`, `BudgetStatus`)
- Lookup tables: `<Entity>Lookup` (`BudgetStatusLookup`, `ExpenseTypeLookup`)
- Primary key: `Id INT NOT NULL PRIMARY KEY`
- Foreign keys: `<Entity>Id` (`CondominiumId`, `FiscalYearId`, `TenantId`)
- FK constraints: `FK_<Table>_<ReferencedTable>`
- Unique constraints: `UQ_<Table>_<Columns>`
- Booleans: `Is...` or `Has...` prefix (`IsDeleted`, `IsActive`, `HasElevator`)

### Trace / Audit Columns (every table)
```sql
CreatedById          INT            NOT NULL,
CreatedByFullName    NVARCHAR(200)  NULL,
LastUpdatedById      INT            NULL,
LastUpdatedByFullName NVARCHAR(200) NULL,
IsDeleted            BIT            NOT NULL DEFAULT 0,
CreationDate         DATETIME2      NOT NULL,
LastUpdateDate       DATETIME2      NULL
```

### Hilo Sequence Entry (every new table)
```sql
INSERT INTO hibernate_unique_key (entity_type, next_hi)
SELECT 'Foo', 1
WHERE NOT EXISTS (
    SELECT 1 FROM hibernate_unique_key WHERE entity_type = 'Foo'
);
```

---

## Frontend Patterns

### api.js

**Location**: `DomuWave.Web/ClientApp/src/services/api.js`

```javascript
export const fooApi = {
  getAll:           ()          => api.get('/foos'),
  getById:          (id)        => api.get(`/foos/${id}`),
  getByParent:      (parentId)  => api.get(`/foos/by-parent/${parentId}`),
  create:           (data)      => api.post('/foos', data),
  update:           (id, data)  => api.put(`/foos/${id}`, data),
  delete:           (id)        => api.delete(`/foos/${id}`),
  // custom actions
  approve:          (id, opts)  => api.post(`/foos/${id}/approve`, opts ?? {}),
}
```

Rules:
- Group by domain — one export per domain
- Base URL: `import.meta.env.VITE_API_DOMUAPP_URL + "/api"` (configured in axios instance)
- Auth: `Bearer {token}` from `localStorage('domuwave_token')` + `X-Tenant-Id` header (injected by interceptor)
- Global error handler dispatches `api:error` CustomEvent — components only handle `!err?.response` (network errors)
- Error message extraction: `data?.Errors[]` → `data.message` → `data.title` → `data.detail`

### Vue Component Structure

```vue
<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { usePermissions } from '@/composables/usePermissions'
import { fooApi, parentApi } from '@/services/api'

const store  = useAppStore()
const route  = useRoute()
const { canCreate, canEdit, canDelete } = usePermissions()

// ── State ──────────────────────────────────────────────────────────────────
const loading   = ref(false)
const saving    = ref(false)
const items     = ref([])
const showModal = ref(false)
const editing   = ref(null)   // null = create, id = edit
const errors    = ref({})

const defaultForm = () => ({
  name:  '',
  notes: '',
})
const form = ref(defaultForm())

// ── Computed ───────────────────────────────────────────────────────────────
const filtered = computed(() => items.value.filter(i => i.isActive))

// ── Data loading ───────────────────────────────────────────────────────────
async function loadData() {
  loading.value = true
  try {
    const { data } = await fooApi.getByParent(store.selectedCondominioId)
    items.value = data ?? []
  } catch {
    // Global error handler via api:error CustomEvent
  } finally {
    loading.value = false
  }
}

// ── Modal ──────────────────────────────────────────────────────────────────
function openModal(item = null) {
  editing.value = item?.id ?? null
  form.value    = item ? { ...item } : defaultForm()
  errors.value  = {}
  showModal.value = true
}

// ── Validation ─────────────────────────────────────────────────────────────
function clearError(field) { delete errors.value[field] }

function validate() {
  const e = {}
  if (!form.value.name?.trim()) e.name = 'Il nome è obbligatorio'
  errors.value = e
  return Object.keys(e).length === 0
}

// ── Save ───────────────────────────────────────────────────────────────────
async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (editing.value) {
      await fooApi.update(editing.value, form.value)
      store.toast('Elemento aggiornato', 'success')
    } else {
      await fooApi.create(form.value)
      store.toast('Elemento creato', 'success')
    }
    showModal.value = false
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  } finally {
    saving.value = false
  }
}

// ── Delete ─────────────────────────────────────────────────────────────────
async function deleteItem(id) {
  if (!confirm('Eliminare questo elemento?')) return
  try {
    await fooApi.delete(id)
    store.toast('Elemento eliminato', 'success')
    await loadData()
  } catch (err) {
    if (!err?.response) store.toast('Impossibile raggiungere il server', 'error')
  }
}

// ── Lifecycle ──────────────────────────────────────────────────────────────
onMounted(loadData)
watch(() => store.selectedCondominioId, loadData)
</script>
```

### Form Validation Template

```vue
<div class="form-group" :class="{ 'has-error': errors.name }">
  <label class="form-label">Nome *</label>
  <input class="form-input" v-model="form.name" @input="clearError('name')" />
  <span v-if="errors.name" class="field-error">{{ errors.name }}</span>
</div>
```

### Modal Template

```vue
<div class="modal-overlay" v-if="showModal" @click.self="showModal=false">
  <div class="modal">
    <div class="modal-header">
      <h2>{{ editing ? 'Modifica' : 'Nuovo' }} elemento</h2>
      <button class="btn-icon" @click="showModal=false">✕</button>
    </div>
    <div class="modal-body">
      <!-- form content -->
    </div>
    <div class="modal-footer">
      <button class="btn btn-ghost" @click="showModal=false">Annulla</button>
      <button class="btn btn-primary" @click="save" :disabled="saving">
        <span v-if="saving" class="spinner" style="width:14px;height:14px"></span>
        {{ editing ? 'Salva' : 'Crea' }}
      </button>
    </div>
  </div>
</div>
```

### CSS Variables

```css
/* Always use these — never hardcode colors */
--accent             /* Primary color: #6366f1 */
--accent-green       /* Success: #22c55e */
--accent-red         /* Error/danger: #ef4444 */
--accent-glow        /* Accent tint background */
--border             /* Border color */
--border-active      /* Active/focused border */
--text               /* Primary text */
--text-secondary     /* Secondary/muted text: #6b7280 */
--text-muted         /* Dimmer text: #9ca3af */
--bg-surface         /* Card/surface background */
--bg-base            /* Page base background */
```

### Common CSS Classes

```
Layout:   .card  .table-wrap  .toolbar  .row-actions  .form-grid  .form-fieldset
Forms:    .form-group  .form-label  .form-input  .form-select  .form-textarea
          .has-error  .field-error  .form-group--full  .form-fieldset-legend
Buttons:  .btn  .btn-primary  .btn-ghost  .btn-sm  .btn-icon
State:    .badge  .badge-green  .badge-muted  .spinner  .loading-state  .empty-state
Text:     .text-secondary  .text-muted  .mono  .text-right
```

### App Store Usage

```javascript
const store = useAppStore()

store.selectedCondominioId   // Current condominium ID (int)
store.selectedCondominio     // Current condominium object
store.toast('message', 'success' | 'error' | 'info')
```

---

## Domain Reference

### Completed Domains

| Domain | Controller Route | Key DTOs |
|--------|-----------------|----------|
| Condominium | `api/condominiums` | CondominiumReadDto |
| RealEstateUnit | `api/real-estate-units` | RealEstateUnitReadDto |
| UnitOwner | `api/unit-owners` | UnitOwnerReadDto |
| UnitTenant | `api/unit-tenants` | UnitTenantReadDto |
| Budget | `api/budgets` | BudgetReadDto |
| BudgetItem | `api/budget-items` | BudgetItemReadDto |
| Expense | `api/expenses` | ExpenseReadDto |
| Supplier | `api/suppliers` | SupplierReadDto |
| FiscalYear | `api/fiscal-years` | FiscalYearReadDto |
| MillesimalTable | `api/millesimal-tables` | MillesimalTableReadDto |
| ChartOfAccounts | `api/chart-of-accounts` | ChartOfAccountsReadDto |
| UnitOpeningBalance | `api/real-estate-units/{id}/opening-balance` | UnitOpeningBalanceReadDto |

### Important Business Rules

- **Budget workflow**: Draft → Approve → Close
- **Budget types**: `Preventivo = 1`, `Consuntivo = 2`
- **Budget status**: `Draft = 1`, `Approved = 2`, `Closed = 3`
- **UnitOpeningBalance**: editable only when no `Consuntivo` with `Status = Approved` exists for same condominium + fiscal year
- **ChargeabilityType** (on ChartOfAccounts + Expense): `Owner = 0`, `Tenant = 1`, `Auto = 2`
- **FiscalYear status**: `Open`, `Closing`, `Closed`, `Locked`
- **Condomino** role (profile == 3): can only see their own condominium(s)
- **SuperAdmin** role (profile == 1): full access across tenants

### Namespaces

| Layer | Namespace |
|-------|-----------|
| Models | `DomuWave.Services.Models` |
| DTOs | `DomuWave.Services.Dto.<Domain>` |
| Commands | `DomuWave.Services.Command.<Domain>` |
| Consumers | `DomuWave.Services.Consumers` |
| Service interfaces | `DomuWave.Services.Interfaces` |
| Mapping extensions | `DomuWave.Services.Interfaces.Extensions` |
| Controllers | `DomuWave.Microservice.Controllers` |

---

## Known Issues

- Server response encoding: `"giÃ "` instead of `"già"` — UTF-8/Latin-1 mismatch in backend exception serialization (not fixed)
- `ValidatorException` / `NotFoundException` from `CPQ.Core.Exceptions`
