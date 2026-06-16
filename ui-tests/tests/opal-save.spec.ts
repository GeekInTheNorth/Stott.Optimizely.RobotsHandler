import { expect, test } from '@playwright/test';
import * as path from 'path';

import { OpalTokenAdminPage } from '../src/OpalTokenAdminPage';
import { RobotsAdminPage } from '../src/RobotsAdminPage';
import { getRobotConfigurations, parseRobotConfig, saveRobotConfiguration } from '../src/opal';
import { SITES } from '../src/domains';

// Authenticated session saved by global setup; used by the UI hooks below.
const STATE_PATH = path.resolve(__dirname, '../.auth/state.json');

const WRITE_TOKEN_NAME = 'UITEST-OPAL-WRITE-TOKEN';
const READ_TOKEN_NAME = 'UITEST-OPAL-READ-TOKEN';
const HOST = SITES.site1.frontendHost; // localhost:5000

const INITIAL = '# OPAL-SAVE-INITIAL\nUser-agent: *\nDisallow: /initial';
const VIA_HOST = '# OPAL-SAVE-VIA-HOST\nUser-agent: *\nDisallow: /via-host';
const VIA_ID = '# OPAL-SAVE-VIA-ID\nUser-agent: *\nDisallow: /via-id';
const REJECTED = '# OPAL-SAVE-REJECTED\nUser-agent: *\nDisallow: /rejected';

// Read the current robots content for HOST (used to confirm a save did/didn't persist).
async function currentContent(token: string): Promise<string> {
  const config = parseRobotConfig((await getRobotConfigurations({ token, hostName: HOST })).body);
  return config?.content ?? '';
}

test.describe.configure({ mode: 'serial' });

test.describe('Opal save-robot-txt-configuration', () => {
  let writeToken: string;
  let readToken: string;

  test.beforeAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const page = await context.newPage();
    const tokens = new OpalTokenAdminPage(page);
    // Write grants save; Read does not (and Write also satisfies the Read needed for GET).
    writeToken = await tokens.createToken(WRITE_TOKEN_NAME, { robotsScope: 'Write' });
    readToken = await tokens.createToken(READ_TOKEN_NAME, { robotsScope: 'Read' });
    // Seed a known host-specific config so there's a baseline and a real id to target.
    await new RobotsAdminPage(page).upsertRobotsConfig(
      SITES.site1.appId,
      SITES.site1.appName,
      HOST,
      INITIAL,
    );
    await context.close();
  });

  test.afterAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const page = await context.newPage();
    const tokens = new OpalTokenAdminPage(page);
    await tokens.deleteToken(WRITE_TOKEN_NAME);
    await tokens.deleteToken(READ_TOKEN_NAME);
    await new RobotsAdminPage(page).clearAllConfigs(SITES.site1.appName);
    await context.close();
  });

  test('write token + valid data by hostName saves (200) and is reflected in GET', async () => {
    const save = await saveRobotConfiguration({
      token: writeToken,
      hostName: HOST,
      robotsTxtContent: VIA_HOST,
    });
    expect(save.status).toBe(200);
    expect(save.body).toMatch(/"success"\s*:\s*true/i);

    expect(await currentContent(writeToken)).toContain('OPAL-SAVE-VIA-HOST');
  });

  test('write token + valid data by robotsId (resolved from GET) saves (200) and is reflected in GET', async () => {
    const before = parseRobotConfig((await getRobotConfigurations({ token: writeToken, hostName: HOST })).body);
    expect(before, 'a config should exist to read its id from').not.toBeNull();

    const save = await saveRobotConfiguration({
      token: writeToken,
      robotsId: before!.id,
      robotsTxtContent: VIA_ID,
    });
    expect(save.status).toBe(200);
    expect(save.body).toMatch(/"success"\s*:\s*true/i);

    expect(await currentContent(writeToken)).toContain('OPAL-SAVE-VIA-ID');
  });

  test('write token + invalid data (no hostName or robotsId) is rejected and saves nothing', async () => {
    const before = await currentContent(writeToken);

    // Content present but nothing to resolve the target against => success:false (HTTP 200).
    const save = await saveRobotConfiguration({ token: writeToken, robotsTxtContent: REJECTED });
    expect(save.status).toBe(200);
    expect(save.body).toMatch(/"success"\s*:\s*false/i);

    const after = await currentContent(writeToken);
    expect(after).toBe(before);
    expect(after).not.toContain('OPAL-SAVE-REJECTED');
  });

  test('read-only token cannot save (401) and leaves the content unchanged', async () => {
    const before = await currentContent(writeToken);

    const save = await saveRobotConfiguration({
      token: readToken,
      hostName: HOST,
      robotsTxtContent: REJECTED,
    });
    expect(save.status).toBe(401);

    const after = await currentContent(writeToken);
    expect(after).toBe(before);
    expect(after).not.toContain('OPAL-SAVE-REJECTED');
  });
});
