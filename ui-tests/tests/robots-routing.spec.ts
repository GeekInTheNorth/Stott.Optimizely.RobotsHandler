import { expect, test } from '@playwright/test';
import * as path from 'path';

import { RobotsAdminPage } from '../src/RobotsAdminPage';
import { SITE1_MARKER, SITE2_MARKER, SITE1_ROBOTS, SITE2_ROBOTS, SITES } from '../src/domains';

// Authenticated session saved by the 'setup' project; used by the UI hooks below.
const STATE_PATH = path.resolve(__dirname, '../.auth/state.json');

// Edit robots.txt content via the admin UI, then verify routing across all four
// sample domains. Tests run serially; setup/cleanup share the persisted database.
test.describe.configure({ mode: 'serial' });

test.describe('robots.txt routing across frontend and editor domains', () => {
  test.beforeAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const admin = new RobotsAdminPage(await context.newPage());
    // Bind content to the FRONTEND host only — this is what makes the
    // "frontend domains only" behaviour observable on the editor domains.
    await admin.upsertRobotsConfig(
      SITES.site1.appId,
      SITES.site1.appName,
      SITES.site1.frontendHost,
      SITE1_ROBOTS,
    );
    await admin.upsertRobotsConfig(
      SITES.site2.appId,
      SITES.site2.appName,
      SITES.site2.frontendHost,
      SITE2_ROBOTS,
    );
    await context.close();
  });

  test.afterAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const admin = new RobotsAdminPage(await context.newPage());
    await admin.deleteConfig(SITES.site1.appName, SITES.site1.frontendHost);
    await admin.deleteConfig(SITES.site2.appName, SITES.site2.frontendHost);
    await context.close();
  });

  // Each test navigates the browser to /robots.txt (visible under --headed) and
  // asserts on the raw HTTP response. robots.txt is [AllowAnonymous] and routing
  // is host-based, so the authenticated session does not affect the output.
  test('site 1 frontend domain serves its configured robots.txt', async ({ page }) => {
    const res = await page.goto(`${SITES.site1.frontendUrl}/robots.txt`);
    expect(res).not.toBeNull();
    expect(res!.status()).toBe(200);
    expect(res!.headers()['content-type']).toContain('text/plain');

    const body = await res!.text();
    expect(body).toContain(SITE1_MARKER);
    expect(body).toContain('/site1-only');
    expect(body).not.toContain(SITE2_MARKER);
  });

  test('site 2 frontend domain serves its configured robots.txt', async ({ page }) => {
    const res = await page.goto(`${SITES.site2.frontendUrl}/robots.txt`);
    expect(res).not.toBeNull();
    expect(res!.status()).toBe(200);

    const body = await res!.text();
    expect(body).toContain(SITE2_MARKER);
    expect(body).toContain('/site2-only');
    expect(body).not.toContain(SITE1_MARKER);
  });

  test('editor domains do not serve frontend robots.txt content', async ({ page }) => {
    for (const editorUrl of [SITES.site1.editorUrl, SITES.site2.editorUrl]) {
      const res = await page.goto(`${editorUrl}/robots.txt`);
      expect(res, `${editorUrl}/robots.txt response`).not.toBeNull();
      expect(res!.status(), `${editorUrl}/robots.txt status`).toBe(200);

      const body = await res!.text();
      // None of the frontend-specific content leaks to the editor domains.
      expect(body, `${editorUrl} leaked site1 content`).not.toContain(SITE1_MARKER);
      expect(body, `${editorUrl} leaked site2 content`).not.toContain(SITE2_MARKER);
      expect(body).not.toContain('/site1-only');
      expect(body).not.toContain('/site2-only');
      // Falls back to the add-on default (disallow everything).
      expect(body, `${editorUrl} did not fall back to default`).toMatch(/Disallow:\s*\/\s*$/m);
    }
  });
});
