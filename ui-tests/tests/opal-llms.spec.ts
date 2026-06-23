import { expect, test } from '@playwright/test';
import * as path from 'path';

import { LlmsAdminPage } from '../src/LlmsAdminPage';
import { OpalTokenAdminPage } from '../src/OpalTokenAdminPage';
import { getLlmsConfigurations } from '../src/opal';
import { SITES } from '../src/domains';

// Authenticated session saved by global setup; used by the UI hooks below.
const STATE_PATH = path.resolve(__dirname, '../.auth/state.json');

const TOKEN_NAME = 'UITEST-OPAL-LLMS-TOKEN';
const LLMS_MARKER = 'UITEST-OPAL-LLMS-GET';
const LLMS_CONTENT = `# ${LLMS_MARKER}\n\n> Opal llms get test content`;
const HOST = SITES.site1.frontendHost; // localhost:5000

test.describe.configure({ mode: 'serial' });

test.describe('Opal get-llms-txt-configurations', () => {
  let bearerToken: string;

  test.beforeAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const page = await context.newPage();
    // Read access to the llms scope.
    bearerToken = await new OpalTokenAdminPage(page).createToken(TOKEN_NAME, { llmsScope: 'Read' });
    // llms has no synthetic defaults, so seed a config or the GET has nothing to return.
    await new LlmsAdminPage(page).upsertLlmsConfig(
      SITES.site1.appId,
      SITES.site1.appName,
      HOST,
      LLMS_CONTENT,
    );
    await context.close();
  });

  test.afterAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const page = await context.newPage();
    await new OpalTokenAdminPage(page).deleteToken(TOKEN_NAME);
    await new LlmsAdminPage(page).clearAllConfigs(SITES.site1.appName);
    await context.close();
  });

  test('returns data when a valid token is supplied', async () => {
    const res = await getLlmsConfigurations({ token: bearerToken, hostName: HOST });
    expect(res.status).toBe(200);
    expect(res.body).toContain(LLMS_MARKER);
  });

  test('rejects a wrong token with 401 and returns no data', async () => {
    const res = await getLlmsConfigurations({ token: 'this-is-not-a-valid-token', hostName: HOST });
    expect(res.status).toBe(401);
    expect(res.body).not.toContain(LLMS_MARKER);
  });

  test('rejects a missing token with 401 and returns no data', async () => {
    const res = await getLlmsConfigurations({ hostName: HOST });
    expect(res.status).toBe(401);
    expect(res.body).not.toContain(LLMS_MARKER);
  });
});
