---
name: Backend .NET version
description: Why the backend targets net9.0 and which NuGet package versions to use
---

## Target framework
`net9.0` — was originally `net10.0` but the Replit dotnet-9.0 module is the latest available.

**How to apply:** When adding new NuGet packages that have major-version alignment with the framework (EF Core, ASP.NET Core extensions), pin to `9.0.x`, not `10.0.x`.

## Pinned versions (as of last update)
- `Microsoft.EntityFrameworkCore.*` → `9.0.7`
- `Microsoft.AspNetCore.OpenApi` → `9.0.7`
- `Microsoft.AspNetCore.Authentication.JwtBearer` → `9.0.7`
- `Scalar.AspNetCore` → `2.3.1` (not the 10.x-era releases)
- `BCrypt.Net-Next` → `4.0.3`
- `Microsoft.IdentityModel.Tokens` + `System.IdentityModel.Tokens.Jwt` → `8.12.1`

## Asset model quirk
`Asset` model does NOT have a `DeletedAt` property (only `IsDeleted`). Soft-delete pattern for assets omits the timestamp.
