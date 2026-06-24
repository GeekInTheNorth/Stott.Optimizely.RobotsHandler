import { expect, test } from '@playwright/test';
import * as path from 'path';

import { EnvironmentAdminPage, type EnvironmentOverrides } from '../src/EnvironmentAdminPage';
import { fetchTextFile } from '../src/textFile';
import { ENVIRONMENT_NAME, SITES } from '../src/domains';

// Authenticated session saved by global setup; used by the UI hooks below.
const STATE_PATH = path.resolve(__dirname, '../.auth/state.json');

// The override applies site-wide; the Site 1 frontend home page is a convenient target.
const HOME_URL = `${SITES.site1.frontendUrl}/`;

// Mirror EnvironmentRobotsModel.ToMetaContent(): order is noindex, nofollow,
// noimageindex, noarchive, comma-joined with no spaces.
function expectedDirectives(flags: EnvironmentOverrides): string {
  const parts: string[] = [];
  if (flags.noIndex) parts.push('noindex');
  if (flags.noFollow) parts.push('nofollow');
  if (flags.noImageIndex) parts.push('noimageindex');
  if (flags.noArchive) parts.push('noarchive');
  return parts.join(',');
}

// Content of every <meta name="robots"> tag in the HTML (the home page renders the
// tag; MetaRobotsTagHelper removes it when empty/disabled or rewrites its content).
function robotsMetaContents(html: string): string[] {
  const metas = html.match(/<meta\b[^>]*>/gi) ?? [];
  return metas
    .filter((tag) => /name\s*=\s*["']robots["']/i.test(tag))
    .map((tag) => {
      const match = tag.match(/content\s*=\s*["']([^"']*)["']/i);
      return match ? match[1] : '';
    });
}

async function newEnvAdmin(browser: import('@playwright/test').Browser): Promise<{
  env: EnvironmentAdminPage;
  close: () => Promise<void>;
}> {
  const context = await browser.newContext({ storageState: STATE_PATH, ignoreHTTPSErrors: true });
  return { env: new EnvironmentAdminPage(await context.newPage()), close: () => context.close() };
}

test.describe.configure({ mode: 'serial' });

test.describe(`environment robots overrides for "${ENVIRONMENT_NAME}"`, () => {
  // Leave the environment with no overrides so other specs / re-runs are unaffected.
  test.afterAll(async ({ browser }) => {
    const { env, close } = await newEnvAdmin(browser);
    await env.disableAll(ENVIRONMENT_NAME);
    await close();
  });

  test('no overrides: home page has no meta robots tag and no X-Robots-Tag header', async ({
    browser,
  }) => {
    const { env, close } = await newEnvAdmin(browser);
    await env.disableAll(ENVIRONMENT_NAME);
    await close();

    const res = await fetchTextFile(HOME_URL);
    expect(res.status).toBe(200);
    expect(res.headers['x-robots-tag'], 'X-Robots-Tag header should be absent').toBeUndefined();
    expect(robotsMetaContents(res.body), 'meta robots tag should be absent').toHaveLength(0);
  });

  const combinations: { name: string; flags: EnvironmentOverrides }[] = [
    { name: 'No Follow', flags: { noFollow: true } },
    { name: 'No Index', flags: { noIndex: true } },
    { name: 'No Image Index', flags: { noImageIndex: true } },
    { name: 'No Archive', flags: { noArchive: true } },
    {
      name: 'all four',
      flags: { noFollow: true, noIndex: true, noImageIndex: true, noArchive: true },
    },
  ];

  for (const { name, flags } of combinations) {
    test(`overrides enabled (${name}): meta robots tag and X-Robots-Tag header present with directives`, async ({
      browser,
    }) => {
      const { env, close } = await newEnvAdmin(browser);
      await env.setOverrides(ENVIRONMENT_NAME, flags);
      await close();

      const expected = expectedDirectives(flags);
      const res = await fetchTextFile(HOME_URL);
      expect(res.status).toBe(200);

      // X-Robots-Tag header present with the expected directives.
      expect(res.headers['x-robots-tag']).toBe(expected);

      // meta robots tag present; every robots meta's content equals the directives.
      const metas = robotsMetaContents(res.body);
      expect(metas.length, 'expected at least one meta robots tag').toBeGreaterThan(0);
      for (const content of metas) {
        expect(content).toBe(expected);
      }
    });
  }
});
