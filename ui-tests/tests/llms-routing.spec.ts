import { expect, test } from '@playwright/test';
import * as path from 'path';

import { LlmsAdminPage } from '../src/LlmsAdminPage';
import { fetchTextFile } from '../src/textFile';
import {
  SITE1_LLMS_MARKER,
  SITE2_LLMS_MARKER,
  SITE1_LLMS,
  SITE2_LLMS,
  SITES,
} from '../src/domains';

// Authenticated session saved by global setup; used by the UI hooks below.
const STATE_PATH = path.resolve(__dirname, '../.auth/state.json');

// Edit llms.txt content via the admin UI, then verify routing across all four sample
// domains. Unlike robots.txt, llms.txt has NO default content — when nothing matches,
// the route returns 404. Tests run serially; setup/cleanup share the persisted database.
test.describe.configure({ mode: 'serial' });

test.describe('llms.txt routing across frontend and editor domains', () => {
  test.beforeAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const admin = new LlmsAdminPage(await context.newPage());
    // Start clean so the editor-host 404 test cannot be broken by a leftover
    // site-wide Default from a crashed prior run.
    await admin.clearAllConfigs(SITES.site1.appName);
    await admin.clearAllConfigs(SITES.site2.appName);
    // Bind content to the FRONTEND host only — editor hosts then have no match.
    await admin.upsertLlmsConfig(
      SITES.site1.appId,
      SITES.site1.appName,
      SITES.site1.frontendHost,
      SITE1_LLMS,
    );
    await admin.upsertLlmsConfig(
      SITES.site2.appId,
      SITES.site2.appName,
      SITES.site2.frontendHost,
      SITE2_LLMS,
    );
    await context.close();
  });

  test.afterAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const admin = new LlmsAdminPage(await context.newPage());
    await admin.deleteConfig(SITES.site1.appName, SITES.site1.frontendHost);
    await admin.deleteConfig(SITES.site2.appName, SITES.site2.frontendHost);
    await context.close();
  });

  // llms.txt is [AllowAnonymous]; fetchTextFile reads it with caching disabled.
  test('site 1 frontend domain serves its configured llms.txt', async () => {
    const res = await fetchTextFile(`${SITES.site1.frontendUrl}/llms.txt`);
    expect(res.status).toBe(200);
    expect(res.contentType).toContain('text/plain');
    expect(res.body).toContain(SITE1_LLMS_MARKER);
    expect(res.body).not.toContain(SITE2_LLMS_MARKER);
  });

  test('site 2 frontend domain serves its configured llms.txt', async () => {
    const res = await fetchTextFile(`${SITES.site2.frontendUrl}/llms.txt`);
    expect(res.status).toBe(200);
    expect(res.body).toContain(SITE2_LLMS_MARKER);
    expect(res.body).not.toContain(SITE1_LLMS_MARKER);
  });

  test('editor domains do not serve frontend llms.txt content (404)', async () => {
    for (const editorUrl of [SITES.site1.editorUrl, SITES.site2.editorUrl]) {
      const res = await fetchTextFile(`${editorUrl}/llms.txt`);
      // No host-specific match and no site-wide default => no llms content => 404.
      expect(res.status, `${editorUrl}/llms.txt status`).toBe(404);
      expect(res.body, `${editorUrl} leaked site1 content`).not.toContain(SITE1_LLMS_MARKER);
      expect(res.body, `${editorUrl} leaked site2 content`).not.toContain(SITE2_LLMS_MARKER);
    }
  });
});
