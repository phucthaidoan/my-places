/** API origin (no trailing slash). Must match client `ApiBaseUrl` in Development. */
export function apiBaseUrl(): string {
  const raw = process.env.E2E_API_BASE_URL ?? 'http://localhost:5160';
  return raw.replace(/\/$/, '');
}
