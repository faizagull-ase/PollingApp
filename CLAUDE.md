# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

PulsePoll is a real-time live-polling API built with ASP.NET Core (.NET 10), EF Core (SQL Server), and SignalR. Operators create polls from templates; participants join via a short poll code and answer questions in real time, with live vote tallies broadcast to everyone in the poll.

Project root: `PulsePoll_Api/PulsePoll/` (`PulsePoll.csproj`). No test project exists yet.

## Build, run, and database commands

Run from `PulsePoll_Api/PulsePoll/`:

- Build: `dotnet build`
- Run (dev, with Swagger UI at `/swagger`): `dotnet run`
- Restore packages: `dotnet restore`
- Add a migration: `dotnet ef migrations add <Name>`
- Apply migrations to the database: `dotnet ef database update`

No lint or test commands are configured yet — there is no test project in the solution.

## Architecture

- **Controllers** (`Controllers/`) — REST endpoints:
  - `PollsController`: `POST /api/polls` creates a poll from a template, returns a join code.
  - `TemplatesController`: `POST /api/templates` creates a reusable question template (validated by `TemplateValidator`).
- **Hub** (`Hubs/PollHub.cs`) — SignalR hub at `/hubs/poll` driving real-time flow: `JoinPoll`, `SubmitAnswer`, `NextQuestion`, `ClosePoll`, `GetPollSnapshot`. Clients are grouped by poll code; results are broadcast as typed payloads (`PollJoined`, `AnswerTally`, `QuestionChanged`, `PollClosed`, etc. in `Models/Hub/`).
  - Note: there is currently no operator authentication — anyone who knows the poll code can drive the poll (see inline comment in `PollHub.cs`).
- **Services** (`Services/`):
  - `Services/Polls/PollService` (`IPollService`) — core poll lifecycle logic (join, submit answer, advance question, close, snapshot), returns typed result objects (`JoinPollResult`, `SubmitAnswerResult`, `NextQuestionResult`, `ClosePollResult`).
  - `Services/Templates/TemplateService` (`ITemplateService`) — template creation logic.
  - `Services/Caching/MemoryPollCacheStore` (`IPollCacheStore`) — in-memory store of live poll state (`Models/Cache/PollState.cs`) for fast access during an active session.
  - `Services/PollCodes/PollCodeGenerator` — generates short unique poll join codes.
- **Data** (`Data/`) — `PulsePollDbContext` (EF Core, SQL Server) and `Data/Repositories/TemplateRepository` (`ITemplateRepository`) for template persistence. Migrations live in `Migrations/`.
- **Models** (`Models/`) — `Entities/` (EF entities: `Template`, `Question`), `Dtos/` (request/response payloads for `Polls` and `Templates`), `Hub/` (SignalR payload contracts), `Cache/` (in-memory poll state).
- **Validation** (`Validation/TemplateValidator.cs`) — validates template creation requests before persistence.
- **Middleware** (`Middleware/ExceptionHandlingMiddleware.cs`) + `Exceptions/ApiExceptions.cs` — centralized error handling, standardizes API error responses.
- **Program.cs** — DI wiring, CORS policy (`SignalRClient`, currently scoped to `http://127.0.0.1:5500`), SignalR/memory cache/EF Core registration. A TODO notes rate limiting for `JoinPoll`, `SubmitAnswer`, and template upload is not yet implemented.

## Known gaps (from code comments/state)

- No operator authentication on hub actions (`NextQuestion`, `ClosePoll`).
- No rate limiting yet (planned per spec section 5, see `Program.cs` TODO).
- No test project in the repo.
