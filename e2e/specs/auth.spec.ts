import { test, expect } from '@playwright/test';

test('register then land on trips', async ({ page }) => {
  const suffix = Date.now();
  const email = `e2e.${suffix}@example.com`;
  const username = `e2euser${suffix}`;

  await page.goto('/register');
  await expect(page.getByRole('heading', { name: 'Register' })).toBeVisible();

  await page.getByTestId('register-email').fill(email);
  await page.getByTestId('register-username').fill(username);
  await page.getByTestId('register-display-name').fill('E2E User');
  await page.getByTestId('register-password').fill('E2etest1');
  await page.getByTestId('register-submit').click();

  await expect(page).toHaveURL(/\/trips$/);
  await expect(page.getByTestId('trips-heading')).toBeVisible();
});
