import { expect, test } from '@playwright/test';
import * as path from 'path';

import { LlmsAdminPage } from '../src/LlmsAdminPage';
import { fetchTextFile } from '../src/textFile';
import { SITES } from '../src/domains';

// Authenticated session saved by global setup; used by the UI hooks below.
const STATE_PATH = path.resolve(__dirname, '../.auth/state.json');

const DEFAULT_CONTENT = '# llms-default-content';
const DIRECT_CONTENT = '# llms-direct-content';

test.describe.configure({ mode: 'serial' });

test.describe('site 1 llms.txt host precedence', () => {
  // Leave Site 1 clean so the routing spec (and re-runs) start from a known state.
  test.afterAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const admin = new LlmsAdminPage(await context.newPage());
    await admin.clearAllConfigs(SITES.site1.appName);
    await context.close();
  });

  test('host-specific config overrides the site-wide default for localhost:5000', async ({
    browser,
  }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const admin = new LlmsAdminPage(await context.newPage());

    // 1. Clear all existing llms.txt configurations for Site 1.
    await admin.clearAllConfigs(SITES.site1.appName);

    // 2. Set up the Site 1 Default host content (an Add — llms has no default row).
    await admin.setDefaultLlms(SITES.site1.appId, SITES.site1.appName, DEFAULT_CONTENT);

    // 3. With only the Default set, the frontend host serves the default content.
    const beforeDirect = await fetchTextFile(`${SITES.site1.frontendUrl}/llms.txt`);
    expect(beforeDirect.status).toBe(200);
    expect(beforeDirect.body).toContain(DEFAULT_CONTENT);

    // 4. ADD a host-specific configuration for localhost:5000.
    await admin.addLlmsConfig(
      SITES.site1.appId,
      SITES.site1.appName,
      SITES.site1.frontendHost,
      DIRECT_CONTENT,
    );

    // 5. The host-specific content now wins for localhost:5000; the default is gone.
    const afterDirect = await fetchTextFile(`${SITES.site1.frontendUrl}/llms.txt`);
    expect(afterDirect.status).toBe(200);
    expect(afterDirect.body).toContain(DIRECT_CONTENT);
    expect(afterDirect.body).not.toContain(DEFAULT_CONTENT);

    await context.close();
  });
});
