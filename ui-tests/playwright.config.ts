import { defineConfig, devices } from '@playwright/test';
import * as dotenv from 'dotenv';
import * as path from 'path';

dotenv.config({ path: path.resolve(__dirname, '.env') });

// The CMS editor domain is the canonical host for the admin UI.
const ADMIN_HOST = 'https://localhost:5001';

export default defineConfig({
  testDir: './tests',
  // Authentication runs once in global setup (logs in, saves the session) so it is
  // true setup — not a listed test. The single project below keeps the Test Explorer
  // showing only the real specs (robots-routing, admin-smoke).
  globalSetup: require.resolve('./global-setup'),
  // Tests share a single persisted database, so run them serially to avoid races.
  fullyParallel: false,
  workers: 1,
  retries: 0,
  timeout: 60_000,
  expect: { timeout: 10_000 },
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: ADMIN_HOST,
    ignoreHTTPSErrors: true,
    // Reuse the session saved by global-setup.ts.
    storageState: path.resolve(__dirname, '.auth/state.json'),
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: {
    // --no-launch-profile avoids opening a browser and forces our explicit env below.
    command:
      'dotnet run --project ../Sample/OptimizelyTwelveTest/OptimizelyTwelveTest.csproj --no-launch-profile',
    // The Development environment enables the 4-port Kestrel block in Program.cs.
    env: { ASPNETCORE_ENVIRONMENT: 'Development' },
    url: 'https://localhost:5000',
    ignoreHTTPSErrors: true,
    reuseExistingServer: true,
    timeout: 180_000,
    stdout: 'pipe',
    stderr: 'pipe',
  },
});
