import { expect, type Locator, type Page } from '@playwright/test';

import { ADMIN_URL } from './domains';

export interface EnvironmentOverrides {
  noFollow?: boolean;
  noIndex?: boolean;
  noImageIndex?: boolean;
  noArchive?: boolean;
}

// Switch label text (from EnvironmentConfiguration.jsx) keyed by override flag.
// "No Index" is not a substring of "No Image Index", so these are unambiguous.
const SWITCHES: { key: keyof EnvironmentOverrides; label: string }[] = [
  { key: 'noFollow', label: 'No Follow' },
  { key: 'noIndex', label: 'No Index' },
  { key: 'noImageIndex', label: 'No Image Index' },
  { key: 'noArchive', label: 'No Archive' },
];

// Page object for the Robots Handler admin SPA "Environment Robots" view.
// Selectors mirror EnvironmentRobotsSettings.jsx / EnvironmentConfiguration.jsx.
export class EnvironmentAdminPage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    await this.page.goto(`${ADMIN_URL}#environment-robots`, { waitUntil: 'domcontentloaded' });
    await expect(this.page.locator('.card').first()).toBeVisible({ timeout: 30_000 });
  }

  private card(envName: string): Locator {
    // Match by the card header, anchored to the start, so e.g. 'Production' does not
    // also match the 'Preproduction' card (substring) and trip strict mode.
    const escaped = envName.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    return this.page.locator('.card').filter({
      has: this.page.locator('.card-header', { hasText: new RegExp(`^${escaped}\\b`) }),
    });
  }

  private toggle(envName: string, label: string): Locator {
    return this.card(envName)
      .locator('.form-check')
      .filter({ hasText: label })
      .locator('input[type="checkbox"]');
  }

  /**
   * Set the four override switches for an environment to the desired state and save
   * only if something changed. Missing flags default to false (disabled). Idempotent.
   */
  async setOverrides(envName: string, flags: EnvironmentOverrides): Promise<void> {
    await this.goto();
    const card = this.card(envName);
    await expect(card).toBeVisible();

    let changed = false;
    for (const { key, label } of SWITCHES) {
      const input = this.toggle(envName, label);
      const desired = flags[key] ?? false;
      if ((await input.isChecked()) !== desired) {
        await input.setChecked(desired);
        changed = true;
      }
    }

    // The Save button only enables after a change; persist when one occurred.
    if (changed) {
      const save = card.getByRole('button', { name: 'Save Changes' });
      await expect(save).toBeEnabled({ timeout: 5_000 });
      await save.click();
      await expect(this.page.locator('.toast-body')).toContainText('successfully applied', {
        timeout: 15_000,
      });
    }
  }

  /** Disable all overrides for an environment (used for setup/cleanup). */
  async disableAll(envName: string): Promise<void> {
    await this.setOverrides(envName, {});
  }
}
