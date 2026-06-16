# Robots Handler UI Tests

Playwright + TypeScript end-to-end tests that drive the **Robots Handler admin UI** in the
sample site (`Sample/OptimizelyTwelveTest`), edit `robots.txt` content, and verify that
routing is correct across the sample's four domains.

## What is verified

The sample hosts two sites across four HTTPS ports:

| Site | Frontend (Primary) | CMS editor (Edit) |
| --- | --- | --- |
| Test Website 1 | `https://localhost:5000` | `https://localhost:5001` |
| Test Website 2 | `https://localhost:5002` | `https://localhost:5003` |

The suite edits content via the admin UI and asserts, for both `robots.txt` and `llms.txt`:

- each frontend domain serves its own configured content (`text/plain; charset=utf-8`);
- content is isolated between sites (site 1's content never appears on site 2 and vice-versa);
- the **editor** domains (5001 / 5003) never expose the frontend content — robots.txt falls
  back to the add-on default (`Disallow: /`), llms.txt returns `404` (it has no default);
- a host-specific config overrides the site-wide Default for the same host.

It also covers **environment-level overrides** for the `Development` environment: with no
overrides the home page has no `X-Robots-Tag` header and no `<meta name="robots">` tag; with
overrides enabled, both are present and contain the selected directives (no follow / no index /
no image index / no archive, and all combined).

This proves robots/llms content is only accessible via the frontend domains, and that the
environment overrides are emitted correctly.

## Prerequisites

1. **.NET SDK 10** (the sample targets `net10.0`).
2. **SQL Server LocalDB** available — the sample uses `(localdb)\mssqllocaldb`, database
   `StottRobotsCms13Db` (created automatically on first run).
3. **Trusted HTTPS dev certificate** so the sample can serve HTTPS:
   ```
   dotnet dev-certs https --trust
   ```
4. **Node.js 18+** and npm.

## One-time setup

The sample uses interactive first-run admin registration (`RegisterAdminUserBehaviors.Enabled`),
so there is no seeded admin. Create one once:

1. Start the sample:
   ```
   dotnet run --project ../Sample/OptimizelyTwelveTest/OptimizelyTwelveTest.csproj --no-launch-profile
   ```
   (set `ASPNETCORE_ENVIRONMENT=Development` if your shell doesn't already).
2. Open `https://localhost:5001` and complete the administrator registration form.
3. Copy `.env.example` to `.env` and fill in the credentials you just created:
   ```
   ADMIN_USERNAME=admin@example.com
   ADMIN_PASSWORD=your-password
   ENVIRONMENT_NAME=Development
   ```
   `ENVIRONMENT_NAME` is the hosting environment the environment-override tests target; the
   sample runs as `Development`.

Because LocalDB persists, this is only needed once per machine/database.

## Install

```
cd ui-tests
npm install
npm run install-browsers   # downloads the Chromium build Playwright uses
```

## Run

```
npm test                   # auto-starts the sample, runs the suite, tears down
npm run test:headed        # watch it drive the browser
npm run report             # open the HTML report after a run
```

The Playwright `webServer` config starts the sample with `dotnet run` and waits up to
180 s for `https://localhost:5000`. If the sample is already running, it is reused
(`reuseExistingServer: true`).

## How it works

- `playwright.config.ts` — auto-starts the sample, runs `global-setup.ts` once, and defines a
  single `chromium` project. Sets `ignoreHTTPSErrors` for the self-signed dev cert and reuses
  the saved session via `storageState`.
- `global-setup.ts` — logs in once at `/util/Login` (fields `#UserName` / `#Password`) and saves
  the session to `.auth/state.json`. This is **global setup**, not a listed test, so the Test
  Explorer shows only the real specs.
- `src/domains.ts` — the four domains, site/host constants, and the unique content markers.
- `src/RobotsAdminPage.ts` / `src/LlmsAdminPage.ts` — page objects that drive the admin SPA
  (add / edit / delete / clear-all / set-default). The browser visibly drives the UI under
  `--headed`.
- `src/textFile.ts` — `fetchTextFile(url)`: reads `/robots.txt` or `/llms.txt` with caching
  disabled.
- `tests/robots-routing.spec.ts` / `tests/llms-routing.spec.ts` — per-domain routing assertions
  (edit via UI, then read the file and assert content + isolation).
- `tests/robots-priority.spec.ts` / `tests/llms-priority.spec.ts` — host precedence: a
  host-specific config overrides the site-wide Default for the same host.
- `tests/content-type.spec.ts` — asserts both files are served as `text/plain; charset=utf-8`.
- `src/EnvironmentAdminPage.ts` + `tests/environment-robots.spec.ts` — environment overrides:
  toggles the Development switches and asserts the home page's `X-Robots-Tag` header and
  `<meta name="robots">` tag (absent when disabled; present with directives when enabled).
- `src/OpalTokenAdminPage.ts` + `src/opal.ts` + `tests/opal.spec.ts` — Opal AI endpoints:
  the discovery manifest (anonymous), and `get-robot-txt-configurations` guarded by a bearer
  token created via the API Tokens UI — returns data with a valid Read-scoped token, and `401`
  with a wrong or missing token.
- `tests/admin-smoke.spec.ts` — light check that the admin SPA loads and lists both sites.

Both files are `[AllowAnonymous]` and routing is host-based, so reads don't need the session.
Reads go through `fetchTextFile`, which sends `Cache-Control: no-cache` — the routes return
`Cache-Control: public, max-age=300` and the sample enables ResponseCaching, so this is required
to observe freshly-saved content (and to stop one test's cached response affecting another).

**robots.txt vs llms.txt** — robots.txt always returns `200`, falling back to the add-on default
(`Disallow: /`) when nothing matches; llms.txt has no default, so it returns **`404`** when no
configuration matches the requesting host. The llms list is also empty until configurations
exist (no synthetic Default row), so setting the site-wide Default is an Add rather than an Edit.

To watch the run: `npm run test:headed` (the browser visibly drives the admin UI for every edit),
or `npm run test:ui` to step through interactively.

## Notes

- Tests run serially against a single shared database; each spec creates the configs it needs
  and cleans up in `afterAll`, so the suite is re-runnable and leaves the database clean.
- Scope is `robots.txt` only. `llms.txt` and environment-robots are not covered; the page
  object is structured so a parallel `LlmsAdminPage` could be added later.
- No add-on or UI source is modified — tests target the existing rendered DOM.
