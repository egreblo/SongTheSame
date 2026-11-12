# SongTheSame

A daily music guessing game. Each day: five songs.

1. **Phase 1:** name the song from ever-longer clips (1 s, 2 s, 4 s, 7 s, 11 s, 16 s).
2. **Phase 2:** the song is revealed together with a six-second segment of it. Name *another* song that contains
   the part most similar to that segment (tempo and key don't matter). The server scores the similarity 1-100.
   If you can't find one, you can skip it for 0 points.

Up to 500 points a day. After each round you can listen to the best twins and to your own pick, each starting at
the part that sounds most like the segment. Results can be compared with other players, shared as text or an image,
and flagged when a score looks wrong. Signed-in players can join leagues with friends and build custom games.
Game sound effects are synthesised in the browser and can be muted.

The working plan (in Croatian) lives in [PLAN.md](PLAN.md). The codebase itself is in English.

## How the similarity score works

Audio is never analysed while someone plays. The Ingest tool downloads each song's 30 s Deezer preview once and
stores a compact **chroma** sequence (12 pitch classes, 10 frames a second, about 3.6 KB per song). A guess is
scored by aligning the target segment against every position of the guessed song with **subsequence DTW**
(tolerates tempo changes) and over all 12 circular **transpositions** (tolerates a different key). The alignment
also tells *where* in the guessed song the match is, which is what the "listen" buttons start from. The raw cost is
converted to 1-100 with a curve calibrated on random song pairs, then rescaled so the best twin available in the
catalog for that round is worth 100. Details and measurements are in [PLAN.md](PLAN.md).

## Structure

```
src/
  SongTheSame.Domain/          Entities, value objects, game rules. No dependencies.
  SongTheSame.Application/     Use cases (IHandler<,>), DTOs, ports. One feature per folder.
  SongTheSame.Infrastructure/  PostgreSQL (EF Core), Deezer client, mp3 decoding, adapters for the ports.
  SongTheSame.Audio/           Chroma extraction, subsequence DTW, scoring (pure library).
  SongTheSame.Api/             ASP.NET Core minimal API: endpoints, Supabase JWT auth, admin policy, rate limiting.
  SongTheSame.Ingest/          CLI: migrate, seed catalog, calibrate, pre-generate and refresh puzzles.
tests/                         xUnit + Shouldly, one project per layer.
tools/SongTheSame.Spike/       Throwaway analyzer evaluation tool (downloads Deezer previews, needs ffmpeg).
web/                           React + TypeScript + Vite + Tailwind v4 + shadcn/ui, generated API client.
scripts/                       File-size check, local PostgreSQL helper.
```

Dependencies point inwards: Api → Infrastructure → Application → Domain.

## Prerequisites

.NET SDK 10, Node 24, PostgreSQL 15+ (local, or a Supabase project), ffmpeg (only for the spike tool).

## Run locally

```bash
# 1. Database. Either your own PostgreSQL, or the private dev cluster (Windows):
powershell -File scripts/local-db.ps1

# 2. Connection string (stored outside the repo; shared by the API and the Ingest tool)
dotnet user-secrets set "ConnectionStrings:Database" "Host=localhost;Port=54329;Database=songthesame_dev;Username=postgres" --project src/SongTheSame.Ingest

# 3. Fill the database (about 25 minutes per 1,200 songs of the seed playlists)
dotnet run --project src/SongTheSame.Ingest -- migrate
dotnet run --project src/SongTheSame.Ingest -- seed
dotnet run --project src/SongTheSame.Ingest -- calibrate
dotnet run --project src/SongTheSame.Ingest -- puzzles 14

# 4. API (http://localhost:5111, OpenAPI at /openapi/v1.json) and web (http://localhost:5173)
dotnet run --project src/SongTheSame.Api --launch-profile http
cd web && npm install && npm run api:generate && npm run dev
```

`web/openapi/openapi.json` is rewritten by every full build of `SongTheSame.Api`
(`dotnet build src/SongTheSame.Api --no-incremental` forces it). The generated client in
`web/src/shared/api/generated` is git-ignored: run `npm run api:generate` after API changes.

Environment variables override user-secrets in both the API and the Ingest tool, so
`ConnectionStrings__Database=... dotnet run --project src/SongTheSame.Ingest -- migrate` targets any database
without touching your stored secret.

## Ingest tool

| Command | What it does |
|---|---|
| `migrate` | Applies database migrations (creates the `pg_trgm` extension too). |
| `seed [path]` | Ingests every playlist of `seed/playlists.json` (international and regional, tagged). Re-running is safe: songs are keyed by Deezer id. |
| `playlist <id> [--origin international\|regional] [--limit N]` | Ingests one Deezer playlist. |
| `calibrate [samples]` | Samples random pairs to calibrate the 1-100 score. **Run again after the catalog changes noticeably, then run `refresh-matches`.** |
| `puzzles [days]` | Pre-generates daily puzzles starting today (about 20-40 s each). |
| `refresh-matches [dayOffset]` | Recomputes the stored best twins (and where they match) of puzzles from today onward, keeping their songs, segments and players' games. Run after `calibrate` or after the catalog grew. |

Create a new migration after changing the model:

```bash
dotnet ef migrations add <Name> --project src/SongTheSame.Infrastructure --startup-project src/SongTheSame.Ingest
```

## Accounts (Supabase)

Anonymous play needs nothing. Accounts, leagues and the game editor need Supabase Auth.

1. Create a Supabase project. Authentication → Providers: enable Google (and any others you want).
2. Authentication → URL configuration: add your site URL and `http://localhost:5173` to the redirect URLs.
3. API settings (`appsettings.json`, user-secrets or environment variables):
   `Supabase__Url=https://<ref>.supabase.co`. Tokens are verified with the project's public keys (JWKS). Only if
   your project still uses the legacy shared secret, also set `Supabase__JwtSecret`.
4. Web settings (`web/.env.local`, see `web/.env.example`): `VITE_SUPABASE_URL`, `VITE_SUPABASE_ANON_KEY`,
   optionally `VITE_AUTH_PROVIDERS=google,apple`.

After signing in, games played anonymously in that browser are moved to the account automatically.

## Admin

Players can flag a phase 2 score they think is wrong. Admins review these at `/admin` (the link appears in the
navigation for admins): each report shows the segment, the player's pick and the best twins with play buttons that
start at the matching part, a note field and "Mark as reviewed". Admins are configured on the API by the e-mail they
sign in with and/or their Supabase user id:

```
Admin__Emails__0=you@example.com
Admin__UserIds__0=<supabase user id>
```

## Configuration reference

| Setting | Where | Notes |
|---|---|---|
| `ConnectionStrings__Database` | API, Ingest | Npgsql format. Supabase: session pooler string plus `SSL Mode=Require;Trust Server Certificate=true`. |
| `Supabase__Url`, `Supabase__JwtSecret` | API | See above. Empty `Url` disables sign-in. |
| `Admin__Emails__N`, `Admin__UserIds__N` | API | Who may use `/admin`. |
| `Cors__AllowedOrigins__0` ... | API | Origin(s) of the web app, for example `https://songthesame.example`. |
| `VITE_API_BASE_URL` | web | Public URL of the API. |
| `VITE_SUPABASE_URL`, `VITE_SUPABASE_ANON_KEY`, `VITE_AUTH_PROVIDERS` | web | Public values; empty disables sign-in. |
| `VITE_BASE_PATH` | web build | Defaults to `/<repo>/` on GitHub project pages; `/` for a custom domain. |

## Test

```bash
dotnet test
cd web && npm run lint && npm run typecheck && npm test
bash scripts/check-file-size.sh 250   # keeps source files short
# Database tests are skipped unless this points at a server where a throwaway database may be created:
SONGTHESAME_TEST_DB="Host=localhost;Port=54329;Username=postgres" dotnet test
```

## Deployment

- **Web:** `.github/workflows/deploy-web.yml` builds and publishes to GitHub Pages (set the repository variables
  `VITE_API_BASE_URL`, `VITE_SUPABASE_URL`, `VITE_SUPABASE_ANON_KEY`).
- **API:** the `Dockerfile` in the root builds a container, for example on Google Cloud Run
  (`gcloud run deploy songthesame-api --source . --region europe-west9 --allow-unauthenticated` plus the settings above).
- **Daily puzzles:** `.github/workflows/daily-puzzles.yml` runs `puzzles 7` every evening (repository secret
  `DATABASE_CONNECTION_STRING`). Without pre-generated puzzles the first player of a day waits about 20 s.
- **Catalog, calibration and migrations:** run `migrate` before deploying a new API version that changes the model.

## Conventions

- Everything in the codebase is English; Croatian lives only in `web/src/shared/i18n/locales/hr.json`.
- New backend feature: `Application/Features/<Area>/<UseCase>/` with a request, a response and a handler
  (one type per file), then an endpoint class in `Api/Endpoints/`. Handlers are registered automatically.
- Source files stay under 250 lines (enforced in CI).
- No MediatR and no FluentAssertions (both commercial); use the built-in handler interface and Shouldly.
- Audio is never stored. Deezer previews are streamed from Deezer to the player; check Deezer's terms of use
  before a public launch.
- `web/src/pages/PrivacyPage` and its texts are a plain-language starting point, not legal advice; have them
  reviewed before a public launch.
