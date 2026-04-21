import { test, expect } from '@playwright/test';
import { apiBaseUrl } from './env';

interface ApiResult<T> {
  isSuccess: boolean;
  value?: T;
  error?: string;
}

test('create trip, add place via API, share, view as anonymous', async ({ page, browser, request }) => {
  const suffix = Date.now();
  const email = `e2e.trip.${suffix}@example.com`;
  const username = `e2etrip${suffix}`;
  const placeName = `E2E Place ${suffix}`;
  const tripName = `E2E Trip ${suffix}`;
  const api = apiBaseUrl();

  await page.goto('/register');
  await expect(page.getByRole('heading', { name: 'Register' })).toBeVisible();
  await page.getByTestId('register-email').fill(email);
  await page.getByTestId('register-username').fill(username);
  await page.getByTestId('register-display-name').fill('E2E Trip User');
  await page.getByTestId('register-password').fill('E2etest1');
  await page.getByTestId('register-submit').click();
  await expect(page).toHaveURL(/\/trips$/);

  const token = await page.evaluate(() => localStorage.getItem('myplaces.jwt'));
  expect(token, 'JWT should be stored after register').toBeTruthy();

  const placeResp = await request.post(`${api}/api/v1/places`, {
    headers: { Authorization: `Bearer ${token}` },
    data: {
      name: placeName,
      address: '1 Test Street',
      city: 'Testville',
      country: null,
      latitude: null,
      longitude: null,
      note: null,
    },
  });
  expect(placeResp.ok(), await placeResp.text()).toBeTruthy();
  const placeBody = (await placeResp.json()) as ApiResult<{ id: string }>;
  expect(placeBody.isSuccess, placeBody.error).toBeTruthy();
  expect(placeBody.value?.id).toBeTruthy();

  await page.getByTestId('trips-new-link').click();
  await expect(page).toHaveURL(/\/trips\/new$/);
  await page.getByTestId('trip-field-name').fill(tripName);
  await page.getByTestId('trip-field-city').fill('Hanoi');
  await page.getByTestId('trip-save').click();

  await expect(page).toHaveURL(/\/trips\/[0-9a-f-]{36}$/i);
  await expect(page.getByTestId('trip-detail-title')).toHaveText(tripName);

  await page.getByTestId('trip-place-select').selectOption(placeBody.value!.id!);
  await page.getByTestId('trip-add-place-button').click();
  await expect(page.getByText(`${placeName} — Testville`)).toBeVisible();

  await page.getByTestId('trip-share-toggle').click();
  const shareLink = page.getByTestId('shared-trip-open-link');
  await expect(shareLink).toBeVisible();
  const href = await shareLink.getAttribute('href');
  expect(href).toBeTruthy();

  const anon = await browser.newContext();
  const anonPage = await anon.newPage();
  await anonPage.goto(href!);
  await expect(anonPage.getByTestId('shared-trip-heading')).toHaveText(tripName);
  await expect(anonPage.getByTestId('shared-trip-place-list')).toContainText(placeName);
  await anon.close();
});
