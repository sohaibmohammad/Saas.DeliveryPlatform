# DeliveryPlatform — Project Context

> Living handoff document. Last updated: 2026-10-22. This reflects discussion, not an audit of the current repository. Verify code before treating an item as implemented.

## Purpose
Solo multi-vendor delivery platform backend built to practice production-oriented .NET backend engineering and present on a CV. Businesses are not limited to restaurants (`BusinessType`). Keep the backend maintainable and layered; avoid a single-file/endpoints-only structure.

## Repository / layout
- Local project root: `D:\all desktop\DeliveryPlatform`
- Backend directory: `DeliveryPlatform.Backend` (previously `DeliveryBackend`)
- Git branch mentioned: `master`
- Frontend planned as a separate folder.
- Architecture: Clean Architecture, with API / Application (Business) / Domain / Infrastructure / Contracts separation. Confirm actual current folder names from repository.

## Stack discussed
.NET 10, ASP.NET Core Web API, EF Core 10, SQL Server, Clean Architecture, MediatR/CQRS, JWT, Google Authentication, Redis, Docker, Docker Compose, repository pattern and Unit of Work. Some are planned or CV wording; do not assume all are fully integrated.

Technical details previously discussed: API `net10.0`, OpenAPI `10.0.11`, EF Core Infrastructure / Tools `10.0.13`. Fluent API preferred over DataAnnotations. Initial migration and `Update-Database` were completed. A Microsoft.Data.SqlClient 6.1.7 dependency chain produced a Pkcs warning; verify whether it remains.

## Domain decisions
- A user may own zero or multiple `Business` records.
- `Business`: `OwnerId`, `Name`, `Description`, `BusinessType`, `IsActive`, `CreatedAt`; has Categories, Products, Branches.
- Category names are unique *within a business*, via `(BusinessId, Name)` alternate key / uniqueness rule.
- `Branch`: `Latitude`, `Longitude`, `IsActive`; a business may have multiple branches.
- `Inventory`: `Quantity`, `ReservedQuantity`, `UpdatedAt`, `BranchId`, `ProductId`. One Product can have inventories across many branches; each Branch has many Inventory rows. Revisit earlier uniqueness on `ProductId` and enforce the intended `(BranchId, ProductId)` rule if appropriate.
- Cart restricted to one Business at a time. Direct purchase from a Business page was considered, not finalized.
- Store delivery/order location on the order rather than a separate persistent user-location record (current preference).
- Order pickup/current location and nearest-captain selection are design topics, not confirmed finished features.

## Authentication — most recent active work
- `JwtTokenService` lives in Infrastructure and has `GenerateToken(User)` using `sub`, `email`, `role` claims and `JwtOptions`.
- `UserRefreshToken` table added and migration applied. Fields: `Id`, `UserId`, `TokenHash` (renamed from `RefreshToken`), `CreatedAt`, `ExpiryDate`, nullable `RevokedAt`, nullable `ReplacedByTokenId`, and `User` navigation.
- `IUserRefreshTokenRepository` extends the generic repository; actual code spelling seen: `IUserRefreshTokenReposiory` (check before renaming).
- `GenericRepository<TEntity>` has `AddAsync`; `UnitOfWork` exists.
- `UserRefreshTokenRepository : GenericRepository<UserRefreshToken>, IUserRefreshTokenReposiory` uses `AppDbContext`; method `GetByTokenHashAsync` discussed.
- `LoginUserHandler` depends on `IUserRepository`, `IPasswordHashService`, `IJwtTokenService`. Work was moving from returning a string to `LoginUserResponse(string AccessToken, string RefreshToken)`.
- Latest unresolved discussion: why refresh-token rotation should add a new token row rather than overwrite the old one; next step is implementing / reviewing refresh-token handler and rotation transaction.

### Proposed refresh-token flow (design recommendation; confirm against implementation)
1. Generate a cryptographically secure opaque refresh token, return plaintext once, persist only its hash.
2. On refresh, hash submitted token and retrieve row; check expiry, revocation, user status, and other relevant security conditions.
3. Rotate: revoke old row, create new row, optionally set `ReplacedByTokenId`, and commit both changes atomically through Unit of Work / transaction.
4. Retaining old row supports audit and replay/reuse detection; overwriting loses that history.
5. Define policy for detected reuse (e.g., revoke token family / active sessions) and concurrency, including simultaneous refresh requests.
6. On blocked user, reject login/refresh even if a JWT is otherwise cryptographically valid; access-token revocation strategy needs an explicit design (e.g., short lifetime and/or server-side revocation/version check).

## Open items / decisions to verify
- Actual current code and migrations versus the discussion above.
- Refresh-token lifetime, rotation, reuse detection, logout, token family, concurrency and transaction implementation.
- Whether Google Auth, Redis, Docker/Compose, repository/UoW and other CV stack items are already complete.
- Branch/Product inventory mapping and indexes.
- Cart/order lifecycle, reservations, branch selection, captain assignment and location model.
- Testing, validation, exception handling, authorization, observability and deployment status.

## Collaboration preferences
- Explain in straightforward colloquial Arabic/Jordanian, step by step, with practical C# examples.
- Don't pretend proposed functionality is implemented. Ask for the relevant latest code when exact integration matters.
- Preserve the current layered structure; prefer one focused step at a time and explain tradeoffs when making design decisions.

## Resume prompt for a new chat
I am continuing my DeliveryPlatform solo .NET backend. Read the attached `PROJECT_CONTEXT.md` first. Treat it as a discussion handoff, not as proof of current code. We were implementing JWT login plus refresh-token rotation. Ask me for the latest relevant files before changing code, and continue from the most recent unresolved step without restarting the architecture discussion.

## Update log template
- Date:
- Implemented and verified:
- Files / migrations changed:
- Design decisions and reasons:
- Bugs / blockers:
- Next exact task:
