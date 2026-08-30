# ParkingApp

> A multi-tenant parking-management platform, built with **.NET 10**, **Blazor WebAssembly**, and **JWT-secured ASP.NET Core**.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-SQL%20Server-512BD4?logo=dotnet&logoColor=white)
![Auth](https://img.shields.io/badge/Auth-JWT%20%2B%20Identity-2ea44f)
![License](https://img.shields.io/badge/License-MIT-blue)

> ⚠️ **Work in progress.** This is a learning-driven project built one vertical slice at a time. The authentication backend is complete; the rest of the domain is being added incrementally.

---

## 📋 What is this?

ParkingApp is a **multi-tenant SaaS** for managing parking facilities. Each customer is a
**company** (a tenant) that manages its own world in complete isolation from every other tenant:
its parking branches, the parking spaces within them, its employees, and eventually the
day-to-day vehicle traffic and statistics.

The system is built around **two roles**:

- **Owner** — sets up and oversees. Creates parking branches, defines their structure
  (floors, spots, spot sizes), adds employees, and views statistics.
- **Employee** — operates. Signs in, opens a work shift, and records vehicles entering and leaving.

Every piece of data belongs to exactly one company. This tenant isolation is part of the
model from day one, not an afterthought.

---

## 🎯 Vision (where this is heading)

**Owner's world — setup & oversight**
- Log in (owner accounts are provisioned by seed, not public registration)
- Create one or more parking branches
- Define each branch's structure: floors, spots per floor, and spot size (motorcycle / car / large)
- Create employee accounts (username + password), bound to the owner's company only
- View statistics (not day-to-day entries)

**Employee's world — operation**
- Log in with credentials created by the owner
- Open and manage a work shift
- Record vehicle entries and exits

**Cross-cutting**
- Strict multi-tenancy: a company only ever sees its own data, enforced via the `companyId` carried in the auth token.

---

## ✅ Current status

| Area | Status |
| --- | --- |
| Auth backend (Identity + JWT, multi-tenant claims) | **Done** |
| Login endpoint returning a signed JWT | **Done** |
| Seed (roles + demo owner + demo company) | **Done** |
| Blazor client auth (login page, protected route, logout, token persistence) | **Done** |
| Company profile endpoint (`/api/companies/me`) — show company name | Next |
| Parking branches, floors, spots | Planned |
| Employees & shifts | Planned |
| Vehicle entries & statistics | Planned |

**Feature Slice 1 (authentication) is complete end-to-end** — a seeded owner can log in from
the Blazor UI, receives a JWT, and lands on a protected page that reads their identity, role,
and company id from the token. Logging out clears the token and blocks protected pages again.

> **Note on `ParkingEntry`:** an early prototype (a flat "vehicle entry log") exists in the
> codebase from the project's first iteration. It is currently **dormant** and will be
> reworked when the operational slice (shifts + entries) is built. It is not part of the
> current auth work.

---

## 🗓️ Changelog

> Dates are approximate (month-level). See `git log` for exact commit dates.

**June 2026 — Initial prototype**
- Solution scaffold: API + Blazor WASM client + shared contracts library
- `ParkingEntry` entity with full CRUD REST API and a read-only Blazor list page

**August 2026 — Multi-tenant SaaS direction & authentication (Feature Slice 1)**
- Reframed the project as a multi-tenant SaaS around Company / Owner / Employee
- `Company` (tenant) and `ApplicationUser` (extends Identity user with `CompanyId`, `FullName`)
- `ParkingDbContext` converted to `IdentityDbContext`, with the Company↔Users relationship
- `TokenService` issuing JWTs with `role` and `companyId` claims
- Database seed for roles (`Owner` / `Employee`) and a demo owner + company
- `AuthController` login endpoint returning a uniform `ApiResponse<LoginResponse>`
- Identity + JWT bearer wired into the request pipeline
- EF Core migration for the Identity and Company tables
- **Blazor client authentication:** token persistence in `localStorage`, a custom
  `AuthenticationStateProvider` that reads claims from the JWT, an auth service/consumer pair,
  a login page, a protected home page, and logout — completing Feature Slice 1 end-to-end

---

## 🛠️ Tech stack

| Area            | Technology                                              |
| --------------- | ------------------------------------------------------- |
| Framework       | .NET 10                                                 |
| Backend         | ASP.NET Core Web API                                    |
| Frontend        | Blazor WebAssembly                                      |
| Data access     | Entity Framework Core (Code-First migrations)           |
| Database        | SQL Server                                              |
| Authentication  | ASP.NET Core Identity + JWT Bearer (API); JWT claims + `AuthorizeRouteView` (client) |
| Client storage  | Blazored.LocalStorage (token persistence)              |
| API docs        | Swagger / OpenAPI (Development only)                    |
| Language        | C#                                                       |

---

## 🧱 Architecture & conventions

**Project layout** — three projects communicating over HTTP, with a shared contracts library:

```
ParkingApp.Api      →  ASP.NET Core Web API (controllers, services, EF Core, Identity)
ParkingApp.Client   →  Blazor WebAssembly front-end
ParkingApp.Shared   →  DTOs / request-response contracts (ApiResponse<T>), referenced by both
```

`Api` and `Client` never reference each other; they agree on the same `Shared` contracts so
their HTTP conversation is type-safe.

**Conventions adopted for this project:**

- **Folder organisation:** services grouped by area inside the API (`Services/Auth`,
  `Services/Parking`) — pragmatic feature folders rather than full Clean Architecture layering.
- **Client-side layering:** a **Consumer** owns pure HTTP communication with the API
  (e.g. `AuthConsumer`), while a **Service** owns orchestration and state
  (e.g. `AuthService`: token storage + auth-state notification). Each layer has one responsibility.
- **Uniform responses:** every endpoint returns an `ApiResponse<T>` envelope
  (`Success` / `Message` / `Errors` / `Value`) via `.Ok(...)` / `.Fail(...)` factory methods.
- **Typed actions:** controllers return `ActionResult<T>` so the success payload type is explicit
  (better Swagger docs, self-documenting signatures).
- **Identifiers:** the tenant key (`Company.Id`) is a `Guid` generated with
  `NEWSEQUENTIALID()` — non-guessable when exposed in tokens, without the index fragmentation
  of random client-side GUIDs.
- **Auth model:** owner accounts are created by **seed only** (no public registration);
  employees are created by their owner and bound to that company. Multi-tenancy is carried by
  a `companyId` claim inside the JWT.
- **Documentation:** English-only XML doc comments (`///`) on public members.
- **Commits:** [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `docs:`, `refactor:` …).
- **Secrets:** never committed — kept in .NET User Secrets locally (see below).

---

## 🚀 Getting started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express or LocalDB is fine)
- An IDE: Visual Studio, JetBrains Rider, or VS Code

### 1. Clone
```bash
git clone https://github.com/XaplanterisNikos/ParkingApp.git
cd ParkingApp
```

### 2. Configure secrets (do NOT put these in appsettings.json)

This project reads its **connection string** and **JWT signing key** from
.NET User Secrets, so they never end up in source control. Set your own:

```bash
cd ParkingApp.Api

# initialise user secrets (skip if already initialised)
dotnet user-secrets init

# your local database connection string
dotnet user-secrets set "ConnectionStrings:ParkingDb" "Server=YOUR_SERVER\SQLEXPRESS;Database=ParkingAppDb;Trusted_Connection=true;TrustServerCertificate=true"

# a long random JWT signing key (min 32 chars)
dotnet user-secrets set "Jwt:Key" "PASTE_A_LONG_RANDOM_SECRET_HERE"
```

Need a key? Generate one:

```powershell
# PowerShell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Max 256 }))
```

The non-secret JWT settings (`Issuer`, `Audience`, `ExpiryMinutes`) already live in
`appsettings.json` and need no change.

### 3. Create the database

Apply the EF Core migrations to build the schema (Identity tables + Company):

```bash
# from the repo root
dotnet ef database update --project ParkingApp.Api
```

> In Visual Studio you can instead run `Update-Database` in the Package Manager Console
> (with `ParkingApp.Api` as the default project).

### 4. Run

```bash
dotnet run --project ParkingApp.Api
```

On startup the app **seeds** its baseline data automatically (see below) and, in Development,
exposes Swagger UI where you can try the login endpoint.

### 5. Log in (demo credentials)

The seed creates one demo owner you can use immediately:

| Field    | Value               |
| -------- | ------------------- |
| Username | `owner@athens.test` |
| Password | *(see `Data/DbSeeder.cs`)* |

Call `POST /api/auth/login` with these credentials and you'll receive a signed JWT.

---

## 🌱 About the seed (and how startup works)

Because owner registration is intentionally **not public**, the application would start with
an empty database that nobody could log into. To solve this, a seeder runs **once on every
startup** and creates the baseline data the app needs:

- the two roles: `Owner` and `Employee`
- a demo company
- that company's owner account (with a properly hashed password)

The seeder is **idempotent** — it checks for existence before creating anything, so running it
on every startup never produces duplicates. This "seed instead of register" approach stands in
for a future super-admin who would otherwise provision owners; it keeps the current phase simple
while staying close to the intended architecture.

---

## 🔐 Security notes

- Secrets (connection string, JWT key) are kept out of source control via User Secrets.
- Passwords are never stored in plain text — ASP.NET Core Identity hashes them.
- Login returns the **same** generic message whether the username or the password is wrong,
  to avoid user enumeration.
- The JWT is **signed, not encrypted**: its claims are readable by anyone holding the token,
  so no secrets are ever placed inside it — only id, role, and company id.

---

## ⚖️ Known limitations & deliberate scope

Some production-grade concerns are intentionally out of scope for this phase. They are
documented here as conscious decisions, not oversights:

- **JWT stored in `localStorage`.** The access token is kept in the browser's `localStorage`,
  which is readable by JavaScript and therefore exposed to XSS attacks if the app ever had an
  XSS vulnerability. This is the most common approach for learning/portfolio Blazor apps and is
  used here for simplicity. A hardened, production-grade setup would instead use `httpOnly`
  cookies (inaccessible to JavaScript), typically combined with anti-CSRF measures and short-lived
  access tokens plus refresh tokens. That is understood but not implemented at this stage.
- **No token refresh / sliding expiration yet.** Tokens simply expire after their lifetime;
  the user logs in again. Refresh-token rotation is planned for a later phase.
- **No account lockout / brute-force protection on login.** `lockoutOnFailure` is currently off;
  it can be enabled once a lockout policy is defined.

---

## 📄 License

Distributed under the [MIT](LICENSE) license.
