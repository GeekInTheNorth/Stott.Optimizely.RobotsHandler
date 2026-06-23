import { expect, test } from '@playwright/test';
import * as path from 'path';

import { RobotsAdminPage } from '../src/RobotsAdminPage';
import { LlmsAdminPage } from '../src/LlmsAdminPage';
import { fetchTextFile } from '../src/textFile';
import { SITE1_LLMS, SITE1_ROBOTS, SITES } from '../src/domains';

// Authenticated session saved by global setup; used by the UI hooks below.
const STATE_PATH = path.resolve(__dirname, '../.auth/state.json');

// Verify both files are served as plain text with UTF-8 encoding. Tests run
// serially; setup/cleanup share the persisted database.
test.describe.configure({ mode: 'serial' });

test.describe('robots.txt and llms.txt Content-Type headers', () => {
  // Both files must return 200 with content for the Content-Type header to be set;
  // llms.txt returns 404 (no header) when no content exists, so seed both on the
  // Site 1 frontend host.
  test.beforeAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const page = await context.newPage();
    await new RobotsAdminPage(page).upsertRobotsConfig(
      SITES.site1.appId,
      SITES.site1.appName,
      SITES.site1.frontendHost,
      SITE1_ROBOTS,
    );
    await new LlmsAdminPage(page).upsertLlmsConfig(
      SITES.site1.appId,
      SITES.site1.appName,
      SITES.site1.frontendHost,
      SITE1_LLMS,
    );
    await context.close();
  });

  test.afterAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const page = await context.newPage();
    await new RobotsAdminPage(page).deleteConfig(SITES.site1.appName, SITES.site1.frontendHost);
    await new LlmsAdminPage(page).deleteConfig(SITES.site1.appName, SITES.site1.frontendHost);
    await context.close();
  });

  for (const file of ['robots.txt', 'llms.txt']) {
    test(`${file} is served as plain text with UTF-8 encoding`, async () => {
      const res = await fetchTextFile(`${SITES.site1.frontendUrl}/${file}`);
      expect(res.status).toBe(200);

      // e.g. "text/plain; charset=utf-8" — assert both parts, case-insensitively.
      const contentType = res.contentType.toLowerCase();
      expect(contentType, `${file} Content-Type was "${res.contentType}"`).toContain('text/plain');
      expect(contentType, `${file} Content-Type was "${res.contentType}"`).toContain('charset=utf-8');
    });
  }
});
