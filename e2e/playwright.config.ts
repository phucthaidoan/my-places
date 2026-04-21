import { defineConfig } from '@playwright/test';

const baseURL = process.env.E2E_BASE_URL ?? 'http://localhost:5178';

export default defineConfig({
  testDir: './specs',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  trace: 'on-first-retry',
  timeout: 90_000,
  expect: { timeout: 20_000 },
  use: {
    baseURL,
    navigationTimeout: 45_000,
    actionTimeout: 15_000,
  },
});
