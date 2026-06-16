import { expect, type Locator, type Page } from '@playwright/test';

import { ADMIN_URL } from './domains';

export type OpalScope = 'None' | 'Read' | 'Write';

// Page object for the Robots Handler admin SPA "API Tokens" view.
// Selectors mirror TokenManagement.jsx.
export class OpalTokenAdminPage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    await this.page.goto(`${ADMIN_URL}#api-tokens`, { waitUntil: 'domcontentloaded' });
    await expect(this.page.getByRole('button', { name: 'Add Token' })).toBeVisible({
      timeout: 30_000,
    });
  }

  private dialog(): Locator {
    return this.page.getByRole('dialog');
  }

  /**
   * Create a token and return its plaintext value (generated client-side and only
   * available at creation — the server stores a hash). The value is read from the
   * read-only Token field before submitting.
   */
  async createToken(
    name: string,
    scopes: { robotsScope?: OpalScope; llmsScope?: OpalScope } = {},
  ): Promise<string> {
    await this.goto();
    await this.page.getByRole('button', { name: 'Add Token' }).click();

    const modal = this.dialog();
    await expect(modal).toBeVisible();
    await modal.getByPlaceholder('Enter a descriptive name for this token').fill(name);

    // Two selects: first = Robots Scope, second = LLMS Scope.
    const selects = modal.locator('select');
    await selects.first().selectOption(scopes.robotsScope ?? 'None');
    await selects.nth(1).selectOption(scopes.llmsScope ?? 'None');

    // The generated token is the only read-only input in the modal.
    const token = await modal.locator('input[readonly]').inputValue();
    expect(token, 'a token value should have been generated').not.toEqual('');

    await modal.getByRole('button', { name: 'Create Token' }).click();
    await expect(this.page.locator('.toast-body')).toContainText('Token created successfully', {
      timeout: 15_000,
    });

    return token;
  }

  /**
   * Delete every token matching a name (used for cleanup). Removes all matches so a
   * crashed prior run can't leave orphan rows accumulating in the persisted store.
   */
  async deleteToken(name: string): Promise<void> {
    await this.goto();

    const rows = () => this.page.locator('table.table-striped tbody tr').filter({ hasText: name });
    let remaining = await rows().count();
    while (remaining > 0) {
      await rows().first().getByRole('button', { name: 'Delete', exact: true }).click();
      const modal = this.dialog();
      await expect(modal).toBeVisible();
      await modal.getByRole('button', { name: 'Delete', exact: true }).click();
      await expect(this.page.locator('.toast-body')).toContainText('Token deleted successfully', {
        timeout: 15_000,
      });
      // The list reloads after each delete; wait for the row count to drop.
      await expect(rows()).toHaveCount(remaining - 1, { timeout: 15_000 });
      remaining -= 1;
    }
  }
}
