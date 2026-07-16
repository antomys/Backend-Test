# Evaluation Prep Notes

Prep for the interview discussion described in `README.md`: "you will be asked to
explain and demo your solution, and we will review it together." Structured to walk
through the reasoning in commit order, then close with open trade-offs and likely
questions.

---

## 1. The starting point (what was actually wrong)

The original single-project app (`ffac893`, initial commit) had:

- One static in-memory `Data` class shared via `static Data data = new()` fields on
  each controller — no persistence, no thread safety.
- `ObjPerson`/`ObjProduct`/`ObjPurchase` "god objects" in `Objects.cs`: public mutable
  fields, business rules (age validation) thrown from property setters as raw
  `System.Exception`, and a `double Id`.
- Controllers doing data access, validation, and error handling inline —
  `exceptions.ItemNotExists()` didn't actually return or throw, so the code
  immediately after it (`return null`) ran unconditionally. A real logic bug, not
  just style.
- `GetPurchaseReportById` was a stub: `throw new NotImplementedException("Please
  implement me!")`.
- No tests, no CI, .NET 5, no DI beyond controller `new()`.

Stating this plainly up front frames every later decision as a response to a
specific problem, not refactoring for its own sake.

---

## 2. Architectural restructuring (`613915e`, `f1b0e2d`)

Moved to a layered solution:

- **`BackendTest.Host`** — controllers, requests/responses (DTOs), validators,
  mappers between application models and DTOs, cross-cutting HTTP concerns.
- **`BackendTest.Application`** — application models, `IEntityService<T>`, a generic
  `EntityService<TPersistence, TApplication>` base class, per-entity mappers to
  persistence models.
- **`BackendTest.Infrastructure`** — EF Core `DbContext`, entity type configurations,
  persistence models, migrations (SQLite).

**Talking point:** the generic `EntityService<TPersistence, TApplication>`
(`src/BackendTest.Application/Services/EntityService.cs`) implements
`GetAll`/`Get`/`Insert`/`Update`/`Delete` once via delegate-injected mapping
functions and an optional `includeRelated` query hook. Each entity's concrete
service (`PersonService`, `ProductService`, `PurchaseService`) supplies its own
mapping + relations. `PurchaseService` overrides `Insert`/`Update` because purchases
need to attach *existing* `Product` entities by id rather than re-inserting them —
a good example of "generic base, override where the domain actually differs"
instead of forcing every entity through identical logic.

Also added: FluentValidation validators per request type, API versioning
(`Asp.Versioning`), Scalar for OpenAPI docs, health checks, Docker +
`docker-compose.yaml`, and a GitHub Actions CI pipeline (build → unit tests →
integration tests).

---

## 3. Domain-by-domain narrative (mirrors commit order)

- **`bc52375`** — removed a speculative `IEntityMapper` abstraction, made
  `PersonMapper` a static utility instead. A clean example of "simplicity first":
  added an abstraction, it didn't earn its keep, removed it.
- **`6f0214b`** — removed dead code (`IPersonService`, `CommonExceptions`,
  `HelperUtils` legacy leftovers), added the first EF Core migrations + seed data,
  reworked `ProductsController` to be async and validated.
- **`d139a77`** — built out the full Purchases domain (model, mapper, service,
  controller, validator) and updated CI.
- **`439136b`** — nested-object JSON serialization; made service methods `virtual`
  specifically so `PurchaseService` could override `Insert`/`Update`.
- **`3cf4cee`** — the big one: **implemented `GetPurchaseReportById`**, added
  integration tests for all three controllers, added
  `BackendTestWebApplicationFactory` for isolated SQLite-per-test-class testing,
  removed the now-obsolete `Data` class entirely.
- **`1fb8a36`** — split `Add` vs. `Update` request DTOs and fixed a real bug in the
  process (see §5).

---

## 4. The required "Get Report" endpoint

`PurchasesController.GetPurchaseReportById`
(`src/BackendTest.Host/Controllers/PurchasesController.cs`) loads the purchase via
`IEntityService<PurchaseApplicationModel>`, returns 404 via `Problem(...)` if
missing, then calls `PurchaseMapper.ToCsvReport(this PurchaseApplicationModel)`.

`ToCsvReport` (`src/BackendTest.Host/Mappers/PurchaseMapper.cs`):

- Groups purchased products by id, computing count/name/price per group — buying
  the same product twice collapses into one report line, matching the sample
  output in the README.
- Writes the `CustomerName:;First Last` header line manually, then uses
  **Sylvan.Data.Csv** (`CsvDataWriter` with `;` delimiter) to write the tabular rows
  via `AsDataReader()` — a fast, allocation-conscious writer instead of hand-rolled
  string escaping.
- Formats price with a comma decimal separator
  (`NumberFormatInfo { NumberDecimalSeparator = "," }`) to match the README's
  `19,99` / `4,99` example exactly.
- Returns bytes via `File(report, "text/csv", "purchase-{id}-report.csv")`.

---

## 5. Cross-cutting concerns worth highlighting

- **Error handling.** `GlobalExceptionHandler` (`IExceptionHandler`) +
  `CustomProblemDetailsFactory` funnel both unhandled exceptions and controller-level
  `Problem()`/`ValidationProblem()` calls through one `CustomizeProblemDetails`
  enrichment (adds `requestId`, `traceId`, `method`, `instance`) — one consistent
  error shape everywhere, versus the original's exception-that-doesn't-return bug.
- **Validation.** FluentValidation validators per request, `PersonValidationOptions`
  bound from config with `ValidateOnStart()` — misconfiguration fails fast at boot,
  not on first request.
- **Add/Update request split fixed a real bug (`1fb8a36`).** Before this commit,
  every `Update` action (Person/Product/Purchase) checked `id != request.Id` and
  then called `entityService.Update(...)` **without ever running the request
  validator** — only `Add` was validated. A malformed update (empty name, underage
  `yearOfBirth`) would have gone straight to the database. The fix does two things
  at once: adds the missing `requestValidator.ValidateAsync(...)` call to every
  `Update` action, and introduces dedicated `*AddRequest` types (no `Id`) separate
  from the existing `*Request` types (which carry `Id`), each with its own
  validator. Splitting the type is what makes the validation rule difference
  expressible: `PersonRequestValidator` adds `RuleFor(request => request.Id
  ).NotNull()` where `PersonAddRequestValidator` has no `Id` rule at all — nothing
  to check on create. Good concrete example of "found and fixed a real bug while
  refactoring," worth stating as exactly that rather than downplaying it as a
  cosmetic DTO split.
- **File/class naming cleanup.** The `PersonRequest`/`PersonRequestValidator`
  classes used to live in files literally named `BookingRequest.cs` /
  `BookingRequestValidator.cs` — a leftover from an earlier rename that never got
  finished. Renamed via `git mv` to `PersonRequest.cs` /
  `PersonRequestValidator.cs` (no content changes; class names were already
  correct, only the filenames were wrong). Small, but worth mentioning as the kind
  of drift a fresh pair of eyes catches.
- **Testing.** 37 integration test methods (`[Fact]`) across
  Person/Products/Purchases/Environment controllers, each run against a real SQLite
  file DB spun up per test class via `BackendTestWebApplicationFactory` (temp file,
  deleted on dispose, WAL/SHM files cleaned up too) — real EF Core behavior under
  test, not mocked repositories.

---

## 6. Given more time (README explicitly invites this — have 2–3 ready)

- `EntityService.Get`/`GetAll` swallow exceptions and log + return null/empty
  rather than surfacing a distinguishable "not found" vs. "DB error." Fine for a
  take-home; in production you'd want a typed result (e.g. `Result<T>`) instead.
- No auth/authorization anywhere — reasonable for a test exercise, worth naming
  explicitly so it doesn't look like an oversight.
- The CSV report has no automated test asserting exact byte-for-byte output against
  the README's sample — worth adding a snapshot-style test if time allows before
  the call.

---

## 7. Likely interview questions to rehearse

### "Why a generic `EntityService<TPersistence, TApplication>` instead of a repository per entity?"

Person, Product, and Purchase all need the exact same CRUD shape: streamed
`GetAll`, `Get` by id, `Insert`, `Update`, `Delete` — open a context, query/mutate,
catch `DbUpdateException`, log, dispose. Writing that three times as three
near-identical repositories would be pure duplication with zero behavioral
difference for two of the three entities. The generic base takes the mapping
(`Func<TPersistenceModel?, TApplicationModel?>` both ways) and an optional
`includeRelated` hook as constructor parameters, so each concrete service just
supplies its own mapper + relations.

Where behavior genuinely diverges — `PurchaseService` needs to attach *existing*
`Product` rows instead of inserting new ones — the base methods are `virtual`, so
`PurchaseService` overrides `Insert`/`Update` outright (it doesn't call `base`; the
override fully replaces the logic via `AttachProduct`'s `ChangeTracker` lookup).

**If pushed on the trade-off:** generics + virtual overrides read as more
indirection than three flat services would, and that cost is only worth paying
while divergence stays the exception. If a fourth entity showed up needing its own
custom `Insert`/`Update` too, I'd reconsider whether the shared base is still
pulling its weight versus just writing that one service by hand.

### "Why SQLite for tests instead of mocking `DbContext`?"

The bugs worth catching in this kind of app live at the EF Core / database
boundary — FK violations, unique-constraint conflicts, query translation — and a
mocked `DbContext`/`DbSet` never exercises any of that; it just returns whatever
you configured it to return. Concrete example already in the code:
`PurchaseService.Insert` attaches a stub `ProductPersistenceModel{ Id = productId }`
and relies on the *real* FK constraint to reject a bogus id, caught via
`catch (DbUpdateException ex)`. A mock never throws that exception unless you
hand-script it to — at which point you're testing your mock setup, not your code.

SQLite specifically (rather than spinning up SQL Server/Postgres in CI) needs no
external service, runs as a single file, and its EF Core provider supports enough
of the same relational surface (constraints, indexes, real migrations) to exercise
that layer honestly. `BackendTestWebApplicationFactory` gives each test class its
own temp-file DB (deleted with its WAL/SHM companions on dispose) while booting the
*real* `WebApplicationFactory<Program>` pipeline — actual DI container, actual
middleware — so it's a true integration test, not a unit test wearing that label.

**Honest caveat:** SQLite isn't a perfect stand-in for whatever runs in
production — dialect quirks and real concurrency/locking behavior under contention
won't be faithfully reproduced. It buys you the EF Core query/constraint layer, not
a production-fidelity load test.

### "Why keep `Get`/`GetAll` swallowing exceptions rather than throwing?"

`EntityService.Get` catches any `Exception`, logs it, and returns `null`, which the
controller then maps to a 404. The reasoning: from a GET caller's point of view,
"doesn't exist" and "the read failed unexpectedly" both mean "you can't have this
resource right now" — collapsing them avoids leaking internal failure detail to the
client, and the `ILogger` call means it's not silent server-side.

**Where I'd push back on myself if the interviewer does:** this genuinely hides
operational problems. A client hitting a real outage (connection pool exhausted,
timeout) sees the identical 404 a truly-missing id would produce, which makes
client-side retry/alerting logic harder to write correctly — you shouldn't retry a
404, but you should retry a transient failure, and this collapses that distinction.
The fix I'd want for production is a typed outcome (e.g. a `Result<T>`/discriminated
NotFound-vs-Failure) so the controller can choose 404 vs. 500 deliberately, while
still logging both paths. Already flagged as a "given more time" item (§6) — good to
frame it that way rather than defend it as obviously correct.

Worth being precise about if asked directly: this swallow-and-log pattern is only in
`Get`, not wrapped around the per-item loop in `GetAll` — a genuine asymmetry, not
something I'd claim was fully consistent by design.

### "Walk me through the Update validation bug."

Found it while doing the Add/Update DTO split (`1fb8a36`), not flagged in advance.
Wiring the new `*AddRequestValidator` into each `Add` action meant looking closely
at the matching `Update` action to give it the equivalent treatment — and `Update`
wasn't calling `ValidateAsync` at all. It only checked `id != request.Id`; the
`IValidator<PersonRequest>` was injected into the constructor and genuinely never
invoked, identically across Person, Product, and Purchase. So a malformed update
payload (empty name, underage `yearOfBirth`) would sail past every rule enforced on
create and go straight to the database.

Fix landed in the same commit as the DTO split: added
`var validation = await requestValidator.ValidateAsync(request, token); if (!validation.IsValid) return this.ToValidationProblem(validation);`
to every `Update` action.

**If asked why the fix wasn't a separate commit from the refactor:** they were
discovered and fixed as one coherent unit of work while already touching those
exact lines — splitting it after the fact would've been artificial. The framing
worth landing on: this is refactoring doing its job — forcing a line-by-line read
of `Update` instead of assuming it mirrored `Add` is exactly how the gap surfaced.

---

## 8. Design-choice deep dives: FluentValidation & pooled `IDbContextFactory`

### Why FluentValidation over Data Annotations

- The killer reason, grounded in the code: `PersonAddRequestValidator`/
  `PersonRequestValidator` take an injected `IOptionsMonitor<PersonValidationOptions>`
  in their constructor to enforce a *configurable* minimum age
  (`Validation:Person:MinimumAge` from config). Data Annotations attributes
  (`[Range]`, custom `ValidationAttribute`) are static metadata evaluated by the
  model binder — they can't have a constructor, so they can't receive a
  DI-injected `IOptionsMonitor<T>`. FluentValidation validators are ordinary
  DI-registered classes, so config-driven rules fall out naturally.
- Auto-discovery: `builder.Services.AddValidatorsFromAssemblyContaining<Program>()`
  (`Program.cs`) picks up every `AbstractValidator<T>` in the assembly with zero
  registration boilerplate — relevant now that there are 6 validators (Add/Update ×
  Person/Product/Purchase) after the last split.
- Consistent error shape: `ValidationResultExtensions.ToValidationProblem` folds
  FluentValidation's `ValidationResult` into `ModelState` and calls
  `controller.ValidationProblem()`, so validation failures flow through the same
  `CustomProblemDetailsFactory` enrichment as every other error response.
- Decouples validation rules from the DTO shape entirely — `PersonRequest`/
  `PersonAddRequest` stay plain data holders; rules live in a separate,
  independently unit-testable class instead of scattered attributes on properties.

### Why `AddPooledDbContextFactory<BackendTestDbContext>` over a scoped `DbContext`

- The concrete, undeniable reason: `DependencyInjectionExtensions.MigrateDatabase(
  this IServiceProvider)` runs in `Program.cs` **before `app.Run()`** — outside any
  HTTP request, so there is no request scope to hand out a scoped `DbContext` from.
  `IDbContextFactory` is the documented answer to exactly this scenario: creating a
  context on demand outside the request pipeline.
- It also fits how `EntityService.GetAll` is written: an `async IAsyncEnumerable<T>`
  using `yield return`, so nothing runs until the caller starts enumerating
  (`await foreach` in `PurchasesController.GetAll`). Creating the context *inside*
  the method body and disposing it in a `finally` ties its lifetime exactly to that
  one streaming enumeration.
- `PurchaseService.Insert`/`Update` each call `factory.CreateDbContextAsync(token)`
  independently to get a fresh context they can freely mutate the `ChangeTracker`
  on (see `AttachProduct`) before one `SaveChangesAsync` — "one factory call = one
  self-contained unit of work," awkward to express with a single shared scoped
  context since `DbContext` isn't thread-safe and only supports one in-flight
  operation at a time.
- Pooling on top (`AddPooledDbContextFactory` vs. plain `AddDbContextFactory`) is
  the perf argument: since this design creates a new context per DB operation
  rather than per request, pooling recycles instances instead of re-paying
  constructor/model-resolution cost on every single `Get`/`Insert`/`Update`/`Delete`
  call.
- Same abstraction, no special-casing in tests: `BackendTestWebApplicationFactory`
  just overrides the `ConnectionStrings:Sqlite` config value per test class, and the
  factory registration picks it up identically to production.

