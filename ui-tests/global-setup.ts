import { chromium, expect, type FullConfig } from '@playwright/test';
import * as dotenv from 'dotenv';
import * as fs from 'fs';
import * as path from 'path';

import { ADMIN_HOST, ADMIN_URL } from './src/domains';

// Logs into the CMS once before the suite and saves the authenticated session to
// .auth/state.json, which every test reuses via `use.storageState`. This runs as
// global setup, so it never appears as a test in the Test Explorer / UI mode.
const authFile = path.resolve(__dirname, '.auth/state.json');

export default async function globalSetup(_config: FullConfig): Promise<void> {
  dotenv.config({ path: path.resolve(__dirname, '.env') });

  const username = process.env.ADMIN_USERNAME;
  const password = process.env.ADMIN_PASSWORD;
  if (!username || !password) {
    throw new Error(
      'ADMIN_USERNAME / ADMIN_PASSWORD must be set in ui-tests/.env (copy from .env.example). See README.md.',
    );
  }

  fs.mkdirSync(path.dirname(authFile), { recursive: true });

  const browser = await chromium.launch();
  const context = await browser.newContext({ ignoreHTTPSErrors: true, baseURL: ADMIN_HOST });
  const page = await context.newPage();

  try {
    // Navigate straight to the CMS login form (baseURL resolves it to the editor host).
    await page.goto('/util/Login', { waitUntil: 'domcontentloaded' });

    // Guard: the first-run admin-registration screen means no admin exists yet
    // (we use supplied credentials, not auto-registration).
    if (/register/i.test(page.url())) {
      throw new Error(
        'The CMS is showing the first-run administrator registration screen, so no admin user exists yet.\n' +
          'Complete the one-time registration in a browser at https://localhost:5001 using the credentials in\n' +
          'ui-tests/.env, then re-run the tests. See README.md ("One-time setup").',
      );
    }

    // Field ids are #UserName / #Password (EPiServer.Cms.UI.AspNetIdentity login view).
    await page.locator('#UserName').fill(username);
    await page.locator('#Password').fill(password);

    // Submit by control type (robust to localised button text); Enter-key fallback.
    const submit = page.locator('form button[type="submit"], form input[type="submit"]').first();
    if (await submit.count()) {
      await submit.click();
    } else {
      await page.locator('#Password').press('Enter');
    }

    // A successful POST redirects away from /util/Login. Surface the server-side
    // validation message instead of a bare timeout if it doesn't.
    await page
      .waitForURL((url) => !/\/util\/login/i.test(url.pathname), { timeout: 30_000 })
      .catch(async () => {
        const serverError = await page
          .locator('.validation-summary-errors, [role="alert"]')
          .first()
          .textContent()
          .catch(() => null);
        throw new Error(
          `Login failed — still on /util/Login. Server message: ${serverError?.trim() || 'none (check credentials / account lockout)'}`,
        );
      });

    // Confirm we are genuinely authorised by loading the Robots admin SPA.
    await page.goto(`${ADMIN_URL}#robots-files`, { waitUntil: 'domcontentloaded' });
    await expect(
      page.getByText('Stott Robots Handler', { exact: false }),
      'Login did not reach the Robots Handler admin screen — check ADMIN_USERNAME / ADMIN_PASSWORD.',
    ).toBeVisible({ timeout: 30_000 });

    await context.storageState({ path: authFile });
  } finally {
    await browser.close();
  }
}
