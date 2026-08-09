# Diffing API

A small HTTP API that accepts two base64-encoded binary payloads per ID and reports whether
they're equal, differ in size, or differ in content (and if so, where).

## Project structure
```
DiffingApi.sln
DiffingApi.Core/ Pure diffing logic. No dependencies beyond the BCL.
DiffingApi.Api/ Minimal API host: HTTP endpoints, JSON contracts, in-memory storage.
DiffingApi.Tests.Unit/ Unit tests for DiffingApi.Core (the "internal logic").
DiffingApi.Tests.Integration/ End-to-end HTTP tests for DiffingApi.Api (the "functionality").
```

`Core` and `Api` are split into separate projects specifically so the diffing algorithm can be
unit tested in complete isolation from any HTTP/hosting concerns, mirroring the assignment's own
split between "internal logic" (unit tests) and "functionality" (integration tests).

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Running the API

```bash
cd DiffingApi.Api
dotnet run
```

By default this listens on the URL(s) printed to the console on startup (typically
`http://localhost:5xxx`, see the console output — Kestrel picks a port from
`Properties/launchSettings.json` or an available one if run standalone).

## Running the tests

```bash
dotnet test
```

This runs both the unit tests (`DiffingApi.Tests.Unit`) and the integration tests
(`DiffingApi.Tests.Integration`, which spin up the real API in-memory via
`WebApplicationFactory<Program>` and exercise it over HTTP).

## CI

`.github/workflows/ci.yml` runs on every push and pull request to `main`: restores,
builds, and runs the full test suite (`dotnet restore` / `build` / `test`) on .NET 10. No
project/solution path is passed explicitly — both commands auto-discover the single `.sln` at
the repo root.

## API

| Method | Path                     | Description                                      |
|--------|--------------------------|--------------------------------------------------|
| PUT    | `/v1/diff/{id}/left`     | Upload/overwrite the left payload for `id`.      |
| PUT    | `/v1/diff/{id}/right`    | Upload/overwrite the right payload for `id`.     |
| GET    | `/v1/diff/{id}`          | Get the diff result for `id`.                    |
| GET    | `/health`                | Liveness check.                                  |

**PUT** body:

```json
{ "data": "AAAAAA==" }
```

`data` must be a valid base64 string. Returns `201 Created` on success (both first upload and
overwrite), `400 Bad Request` if `data` is missing, `null`, or not valid base64.

**GET** response, once both sides have been uploaded:

```json
{ "diffResultType": "Equals" }
```
```json
{ "diffResultType": "SizeDoNotMatch" }
```
```json
{
  "diffResultType": "ContentDoNotMatch",
  "diffs": [
    { "offset": 0, "length": 1 },
    { "offset": 2, "length": 2 }
  ]
}
```

Returns `404 Not Found` if `id` doesn't exist yet, or if only one of the two sides has been
uploaded so far.

`GET /health` returns `200 OK` with body `Healthy` if the service is up. No dependency checks are registered 
beyond that, since this service has no external dependencies (database, downstream API, etc.) to check.

## Assumptions made (and why)

The assignment explicitly asks for assumptions to be called out, so here they are, roughly in
order of how much they matter:

- **GET is 404 until *both* sides are set, not just until the ID exists at all.** The sample
  in the PDF shows GET returning 404 right after only `left` has been PUT (step 3), so an
  entry existing with one side missing is treated the same as not existing yet.
- **Every successful PUT returns 201, including overwrites.** The sample's step 6 overwrites
  an already-set `right` and still gets `201 Created` back, not `200 OK`. That's a bit
  unconventional for a REST update, but the code follows the spec as given rather than
  guessing at an unstated convention.
- **Invalid base64 is treated as a 400**, same as `null` data. The spec only shows the `null`
  case explicitly, but the same reasoning clearly extends to "data that isn't decodable at all."
- **No LCS/edit-distance diffing.** The spec explicitly says an ideal diff algorithm isn't
  required, and since inputs of different lengths are already rejected up front
  (`SizeDoNotMatch`), a same-length positional byte comparison is sufficient — no need to
  detect insertions/deletions. This keeps the algorithm O(n) with no extra dependencies.
- **Consecutive differing bytes are merged into one segment**, not reported one byte at a
  time — this is what makes the PDF's own sample come out as two segments (`{0,1}` and
  `{2,2}`) instead of four.
- **No persistence.** Data lives in memory for the lifetime of the process
  (`ConcurrentDictionary`, safe under concurrent requests at the entry level). The assignment
  doesn't ask for durability, and adding a database would be scope creep for this exercise.
- **`DiffResult` (from `Core`) is serialized directly** as the API response body rather than
  mapped to a separate response DTO — its shape already matches the required JSON exactly
  once camelCased. A larger API would warrant a dedicated DTO to avoid coupling the wire
  format to the domain model; at this scale it would be pure boilerplate.
- **IDs are treated as opaque strings**, not parsed as integers, since the spec never
  constrains their format (the sample just happens to use `"1"`).
- **xUnit v3**, using the classic VSTest-based test runner (via `xunit.runner.visualstudio`)
  rather than the newer Microsoft Testing Platform, for compatibility with a plain
  `dotnet test` invocation without extra `global.json`/MTP configuration.
- **Classic `.sln` format**, not the `.slnx` format the .NET 10 SDK now defaults to, for
  broadest compatibility with tooling/CI that may not yet support it.

## Possible improvements (deliberately out of scope here)

This implementation deliberately stays minimal and focused on what the assignment asks for.
The list below is what I'd prioritize next for a production deployment, not a to-do list I
ran out of time for.

- **Structured logging** (`ILogger<T>`, business-level events like uploads/rejections/computed
  diffs) — no extra package needed, ships in the ASP.NET Core shared framework.
- **OpenAPI/Swagger UI** for interactive exploration (skipped to keep the dependency surface
  minimal — trivial to add via `Microsoft.AspNetCore.OpenApi`).
- **Dockerfile** for containerized, reproducible deployment — not required for a take-home
    exercise where `dotnet run` is sufficient, but would matter for actual deployment
    consistency across environments.
- **Persistence** (e.g. SQLite/Postgres) instead of the in-memory store — needed both to
  survive a restart and for correctness under horizontal scaling: right now, PUTting `left`
  to one instance and `right` to another would never produce a complete pair, since each
  instance only sees its own in-memory dictionary.
- **Entry expiration/cleanup.** The in-memory store has no eviction policy, so a long-running
  instance accumulates entries indefinitely. A TTL per entry (or an explicit DELETE endpoint)
  would bound memory growth.
- **Rate limiting, auth** — not asked for and would be scope creep for a take-home exercise.