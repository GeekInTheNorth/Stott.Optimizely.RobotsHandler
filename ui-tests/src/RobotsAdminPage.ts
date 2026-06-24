import { expect, type Locator, type Page } from '@playwright/test';

import { ADMIN_URL } from './domains';

// Page object for the Robots Handler admin SPA "Robots.txt Files" view.
// Selectors mirror ConfigurationList.jsx / AddSiteRobots.jsx / EditSiteRobots.jsx.
export class RobotsAdminPage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    await this.page.goto(`${ADMIN_URL}#robots-files`, { waitUntil: 'domcontentloaded' });
    await expect(this.page.locator('table.table-striped')).toBeVisible({ timeout: 30_000 });
  }

  private rows(): Locator {
    return this.page.locator('table.table-striped tbody tr');
  }

  private dialog(): Locator {
    return this.page.getByRole('dialog');
  }

  // List row for an application + sanitised host (e.g. "localhost:5000").
  // The default fallback row shows "Default" in the host column instead.
  private configRow(appName: string, host: string): Locator {
    return this.rows().filter({ hasText: appName }).filter({ hasText: host });
  }

  /**
   * Idempotently set robots.txt content for an application + host.
   * Edits the existing row if present, otherwise creates it via "Add Configuration".
   * This keeps the suite re-runnable against the persisted sample database.
   */
  async upsertRobotsConfig(
    appId: string,
    appName: string,
    host: string,
    content: string,
  ): Promise<void> {
    await this.goto();

    const existing = this.configRow(appName, host);
    if ((await existing.count()) > 0) {
      await existing.first().getByRole('button', { name: 'Edit', exact: true }).click();
      const modal = this.dialog();
      await expect(modal).toBeVisible();
      // Edit modal has a single host <select>; re-select to stay deterministic.
      await modal.locator("select[name='SpecificHost']").selectOption(host);
      await this.fillAndSave(modal, content);
    } else {
      await this.page.getByRole('button', { name: 'Add Configuration' }).click();
      const modal = this.dialog();
      await expect(modal).toBeVisible();
      // Add modal has two <select name="SpecificHost">: first = Application, second = Host.
      const selects = modal.locator("select[name='SpecificHost']");
      await selects.first().selectOption(appId);
      await selects.nth(1).selectOption(host);
      await this.fillAndSave(modal, content);
    }
  }

  private async fillAndSave(modal: Locator, content: string): Promise<void> {
    await modal.locator("textarea[name='RobotsContent']").fill(content);
    await modal.getByRole('button', { name: 'Save Changes' }).click();
    await expect(this.page.locator('.toast-body')).toContainText('successfully applied', {
      timeout: 15_000,
    });
  }

  /**
   * Create a host-specific configuration via the "Add Configuration" flow.
   * (The user wants an explicit ADD, distinct from the upsert above.)
   */
  async addRobotsConfig(
    appId: string,
    appName: string,
    host: string,
    content: string,
  ): Promise<void> {
    await this.goto();
    await this.page.getByRole('button', { name: 'Add Configuration' }).click();
    const modal = this.dialog();
    await expect(modal).toBeVisible();
    // Add modal has two <select name="SpecificHost">: first = Application, second = Host.
    const selects = modal.locator("select[name='SpecificHost']");
    await selects.first().selectOption(appId);
    await selects.nth(1).selectOption(host);
    await this.fillAndSave(modal, content);
  }

  /**
   * Set the site-wide "Default" content for an application by editing its Default
   * row (host = '' / "Default"). This content is served for any host of the site
   * that has no host-specific configuration.
   */
  async setDefaultRobots(appName: string, content: string): Promise<void> {
    await this.goto();
    const defaultRow = this.rows().filter({ hasText: appName }).filter({ hasText: 'Default' });
    await defaultRow.first().getByRole('button', { name: 'Edit', exact: true }).click();
    const modal = this.dialog();
    await expect(modal).toBeVisible();
    // Keep the host on "Default" (empty value) and save the site-wide content.
    await modal.locator("select[name='SpecificHost']").selectOption('');
    await this.fillAndSave(modal, content);
  }

  /**
   * Remove every saved configuration for an application (host-specific and any
   * saved site-wide default), leaving only the synthetic Default fallback row.
   * The synthetic Default row's Delete button is disabled, so it is skipped.
   */
  async clearAllConfigs(appName: string): Promise<void> {
    await this.goto();

    const deletable = () =>
      this.rows().filter({ hasText: appName }).locator('button:enabled').filter({ hasText: 'Delete' });

    let remaining = await deletable().count();
    while (remaining > 0) {
      await deletable().first().click();
      const modal = this.dialog();
      await expect(modal).toBeVisible();
      await modal.getByRole('button', { name: 'Delete', exact: true }).click();
      await expect(this.page.locator('.toast-body')).toContainText('successfully deleted', {
        timeout: 15_000,
      });
      // The list reloads after each delete; wait for the row count to drop.
      await expect(deletable()).toHaveCount(remaining - 1, { timeout: 15_000 });
      remaining -= 1;
    }
  }

  /** Remove a host-specific configuration if present (used for cleanup). */
  async deleteConfig(appName: string, host: string): Promise<void> {
    await this.goto();

    const row = this.configRow(appName, host);
    if ((await row.count()) === 0) {
      return;
    }

    await row.first().getByRole('button', { name: 'Delete', exact: true }).click();
    const modal = this.dialog();
    await expect(modal).toBeVisible();
    await modal.getByRole('button', { name: 'Delete', exact: true }).click();
    await expect(this.page.locator('.toast-body')).toContainText('successfully deleted', {
      timeout: 15_000,
    });
  }
}
