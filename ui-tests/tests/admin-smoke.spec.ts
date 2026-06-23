import { expect, test } from '@playwright/test';

import { ADMIN_URL, SITES } from '../src/domains';

// Sanity check: the authenticated admin SPA loads and lists both sample sites.
test('admin SPA loads and lists both sample sites', async ({ page }) => {
  await page.goto(`${ADMIN_URL}#robots-files`, { waitUntil: 'domcontentloaded' });

  await expect(page.getByText('Stott Robots Handler', { exact: false })).toBeVisible();

  const table = page.locator('table.table-striped');
  await expect(table).toBeVisible();
  await expect(table).toContainText(SITES.site1.appName);
  await expect(table).toContainText(SITES.site2.appName);
});
