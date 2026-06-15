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

The suite edits robots.txt content bound to each **frontend** host, then asserts:

- each frontend domain serves its own configured content (`text/plain`);
- content is isolated between sites (site 1's content never appears on site 2 and vice-versa);
- the **editor** domains (5001 / 5003) never expose the frontend content — they fall back to
  the add-on default (`User-agent: *` / `Disallow: /`).

This proves that robots.txt content is only accessible via the frontend domains.

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
   ```

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

- `playwright.config.ts` — auto-starts the sample and defines two projects: a `setup`
  project that logs in once, and a `chromium` project that depends on it and reuses the
  saved session. Also sets `ignoreHTTPSErrors` for the self-signed dev cert.
- `tests/auth.setup.ts` — the `setup` project: logs in once at `/util/Login` (fields
  `#UserName` / `#Password`) and saves the session to `.auth/state.json`. Because it runs as a
  real test it is **visible under `--headed` / `--ui`**, and login runs exactly once per run.
- `src/domains.ts` — the four domains, site/host constants, and the unique content markers.
- `src/RobotsAdminPage.ts` — page object that drives the admin SPA (idempotent add/edit + delete).
- `tests/robots-routing.spec.ts` — the routing assertions (edit via UI, then navigate to
  `/robots.txt` on each domain and assert on the response).
- `tests/admin-smoke.spec.ts` — light check that the admin SPA loads and lists both sites.

The routing tests navigate the browser to `/robots.txt` (so the run is watchable headed) and
assert on the raw HTTP response. `/robots.txt` is `[AllowAnonymous]` and routing is host-based,
so the reused admin session does not affect the served content.

To watch the run: `npm run test:headed` (drives every step in a visible browser), or
`npm run test:ui` to step through interactively. `npx playwright test --project=setup --headed`
shows just the login.

## Viewing tests in VS Code

The Playwright VS Code extension enables only the **first** project by default, so `chromium`
is listed first in `playwright.config.ts` to keep the real tests (`robots-routing`,
`admin-smoke`) visible in the Test Explorer. The `authenticate` login runs automatically as a
`setup` dependency. If you ever want to see/run the login on its own, tick the `setup` project
in the extension's project selector (Test Explorer → Playwright settings).

## Notes

- Tests run serially against a single shared database; setup creates the configs and
  `afterAll` deletes them, so the suite is re-runnable and leaves the database clean.
- Scope is `robots.txt` only. `llms.txt` and environment-robots are not covered; the page
  object is structured so a parallel `LlmsAdminPage` could be added later.
- No add-on or UI source is modified — tests target the existing rendered DOM.
