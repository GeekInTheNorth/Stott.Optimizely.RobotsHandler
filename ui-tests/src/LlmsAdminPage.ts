import { expect, type Locator, type Page } from '@playwright/test';

import { ADMIN_URL } from './domains';

// Page object for the Robots Handler admin SPA "LLMS.txt Files" view.
// Selectors mirror LlmsConfigurationList.jsx / AddSiteLlms.jsx / EditSiteLlms.jsx.
//
// Key difference from robots: there is NO synthetic "Default" row — the llms list is
// empty until a configuration exists (DefaultLlmsContentService.GetAll maps only real
// records), so setting the site-wide Default content is an Add, not an Edit.
export class LlmsAdminPage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    await this.page.goto(`${ADMIN_URL}#llms-files`, { waitUntil: 'domcontentloaded' });
    await expect(this.page.locator('table.table-striped')).toBeVisible({ timeout: 30_000 });
  }

  private rows(): Locator {
    return this.page.locator('table.table-striped tbody tr');
  }

  private dialog(): Locator {
    return this.page.getByRole('dialog');
  }

  // List row for an application + sanitised host (e.g. "localhost:5000").
  // A site-wide row shows "Default" in the host column instead.
  private configRow(appName: string, host: string): Locator {
    return this.rows().filter({ hasText: appName }).filter({ hasText: host });
  }

  /**
   * Idempotently set llms.txt content for an application + host-specific host.
   * Edits the existing row if present, otherwise creates it via "Add Configuration".
   */
  async upsertLlmsConfig(
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
      await modal.locator("select[name='SpecificHost']").selectOption(host);
      await this.fillAndSave(modal, content);
    } else {
      await this.addLlmsConfig(appId, appName, host, content);
    }
  }

  /**
   * Create a configuration via the "Add Configuration" flow. Pass host = '' to
   * create the site-wide Default configuration.
   */
  async addLlmsConfig(
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
   * Set the site-wide "Default" content (host = ''). Because llms has no synthetic
   * Default row, this Adds the configuration the first time and Edits it thereafter
   * (so the suite stays re-runnable without hitting the 409 conflict).
   */
  async setDefaultLlms(appId: string, appName: string, content: string): Promise<void> {
    await this.goto();
    const defaultRow = this.rows().filter({ hasText: appName }).filter({ hasText: 'Default' });
    if ((await defaultRow.count()) > 0) {
      await defaultRow.first().getByRole('button', { name: 'Edit', exact: true }).click();
      const modal = this.dialog();
      await expect(modal).toBeVisible();
      await modal.locator("select[name='SpecificHost']").selectOption('');
      await this.fillAndSave(modal, content);
    } else {
      await this.addLlmsConfig(appId, appName, '', content);
    }
  }

  private async fillAndSave(modal: Locator, content: string): Promise<void> {
    // The Add modal pre-fills a markdown template; fill() replaces it entirely.
    await modal.locator("textarea[name='LlmsContent']").fill(content);
    await modal.getByRole('button', { name: 'Save Changes' }).click();
    await expect(this.page.locator('.toast-body')).toContainText('successfully applied', {
      timeout: 15_000,
    });
  }

  /**
   * Remove every saved configuration for an application (host-specific and the
   * site-wide Default). For llms every row is a real, deletable record.
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
