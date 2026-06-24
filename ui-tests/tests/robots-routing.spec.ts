import { expect, test } from '@playwright/test';
import * as path from 'path';

import { RobotsAdminPage } from '../src/RobotsAdminPage';
import { fetchTextFile } from '../src/textFile';
import { SITE1_MARKER, SITE2_MARKER, SITE1_ROBOTS, SITE2_ROBOTS, SITES } from '../src/domains';

// Authenticated session saved by global setup; used by the UI hooks below.
const STATE_PATH = path.resolve(__dirname, '../.auth/state.json');

// Edit robots.txt content via the admin UI, then verify routing across all four
// sample domains. Tests run serially; setup/cleanup share the persisted database.
test.describe.configure({ mode: 'serial' });

test.describe('robots.txt routing across frontend and editor domains', () => {
  test.beforeAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const admin = new RobotsAdminPage(await context.newPage());
    // Start clean so the editor-host default-fallback test cannot be broken by a
    // leftover site-wide config from a crashed prior run.
    await admin.clearAllConfigs(SITES.site1.appName);
    await admin.clearAllConfigs(SITES.site2.appName);
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

  // robots.txt is [AllowAnonymous]; fetchTextFile reads it with caching disabled
  // so the freshly-saved content is what gets asserted.
  test('site 1 frontend domain serves its configured robots.txt', async () => {
    const res = await fetchTextFile(`${SITES.site1.frontendUrl}/robots.txt`);
    expect(res.status).toBe(200);
    expect(res.contentType).toContain('text/plain');
    expect(res.body).toContain(SITE1_MARKER);
    expect(res.body).toContain('/site1-only');
    expect(res.body).not.toContain(SITE2_MARKER);
  });

  test('site 2 frontend domain serves its configured robots.txt', async () => {
    const res = await fetchTextFile(`${SITES.site2.frontendUrl}/robots.txt`);
    expect(res.status).toBe(200);
    expect(res.body).toContain(SITE2_MARKER);
    expect(res.body).toContain('/site2-only');
    expect(res.body).not.toContain(SITE1_MARKER);
  });

  test('editor domains do not serve frontend robots.txt content', async () => {
    for (const editorUrl of [SITES.site1.editorUrl, SITES.site2.editorUrl]) {
      const res = await fetchTextFile(`${editorUrl}/robots.txt`);
      expect(res.status, `${editorUrl}/robots.txt status`).toBe(200);
      // None of the frontend-specific content leaks to the editor domains.
      expect(res.body, `${editorUrl} leaked site1 content`).not.toContain(SITE1_MARKER);
      expect(res.body, `${editorUrl} leaked site2 content`).not.toContain(SITE2_MARKER);
      expect(res.body).not.toContain('/site1-only');
      expect(res.body).not.toContain('/site2-only');
      // Falls back to the add-on default (disallow everything).
      expect(res.body, `${editorUrl} did not fall back to default`).toMatch(/Disallow:\s*\/\s*$/m);
    }
  });
});
