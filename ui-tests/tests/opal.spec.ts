import { expect, test } from '@playwright/test';
import * as path from 'path';

import { OpalTokenAdminPage } from '../src/OpalTokenAdminPage';
import { RobotsAdminPage } from '../src/RobotsAdminPage';
import { getDiscovery, getRobotConfigurations } from '../src/opal';
import { SITES } from '../src/domains';

// Authenticated session saved by global setup; used by the UI hooks below.
const STATE_PATH = path.resolve(__dirname, '../.auth/state.json');

const TOKEN_NAME = 'UITEST-OPAL-TOKEN';
const OPAL_ROBOTS_MARKER = 'UITEST-OPAL-ROBOTS';
const OPAL_ROBOTS = `# ${OPAL_ROBOTS_MARKER}\nUser-agent: *\nDisallow: /opal-only`;

test.describe.configure({ mode: 'serial' });

test.describe('Opal discovery endpoint', () => {
  test('returns the Opal function manifest (no token required)', async () => {
    const res = await getDiscovery();
    expect(res.status).toBe(200);
    // Function names are values, so this is independent of JSON property casing.
    expect(res.body).toContain('getrobottxtconfigurations');
    expect(res.body).toContain('saverobottxtconfigurations');
    expect(res.body).toContain('getllmstxtconfigurations');
    expect(res.body).toContain('savellmstxtconfigurations');
  });
});

test.describe('Opal get-robot-txt-configurations', () => {
  let bearerToken: string;

  test.beforeAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const page = await context.newPage();
    // Create a token that grants Read access to the robots scope, capturing its value.
    bearerToken = await new OpalTokenAdminPage(page).createToken(TOKEN_NAME, { robotsScope: 'Read' });
    // Seed a robots config so the API has identifiable data to return.
    await new RobotsAdminPage(page).upsertRobotsConfig(
      SITES.site1.appId,
      SITES.site1.appName,
      SITES.site1.frontendHost,
      OPAL_ROBOTS,
    );
    await context.close();
  });

  test.afterAll(async ({ browser }) => {
    const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
    const page = await context.newPage();
    await new OpalTokenAdminPage(page).deleteToken(TOKEN_NAME);
    await new RobotsAdminPage(page).deleteConfig(SITES.site1.appName, SITES.site1.frontendHost);
    await context.close();
  });

  test('returns data when a valid token is supplied', async () => {
    const res = await getRobotConfigurations({
      token: bearerToken,
      hostName: SITES.site1.frontendHost,
    });
    expect(res.status).toBe(200);
    expect(res.body).toContain(OPAL_ROBOTS_MARKER);
  });

  test('rejects a wrong token with 401 and returns no data', async () => {
    const res = await getRobotConfigurations({
      token: 'this-is-not-a-valid-token',
      hostName: SITES.site1.frontendHost,
    });
    expect(res.status).toBe(401);
    expect(res.body).not.toContain(OPAL_ROBOTS_MARKER);
  });

  test('rejects a missing token with 401 and returns no data', async () => {
    const res = await getRobotConfigurations({ hostName: SITES.site1.frontendHost });
    expect(res.status).toBe(401);
    expect(res.body).not.toContain(OPAL_ROBOTS_MARKER);
  });
});
