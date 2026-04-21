import { test, expect } from '@playwright/test';

test('login page loads', async ({ page }) => {
  const res = await page.goto('/login');
  expect(res?.ok()).toBeTruthy();
  await expect(page.getByRole('heading', { name: 'Login' })).toBeVisible();
  await expect(page.getByTestId('login-email')).toBeVisible();
});
