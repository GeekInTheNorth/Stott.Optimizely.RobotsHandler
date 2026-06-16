import { request } from '@playwright/test';

export interface TextFileResponse {
  status: number;
  contentType: string;
  body: string;
  /** All response headers, lower-cased keys (e.g. headers['x-robots-tag']). */
  headers: Record<string, string>;
}

/**
 * Fetches a URL with caching disabled — used for /robots.txt and /llms.txt, and for the
 * home page when checking the X-Robots-Tag header / meta robots tag.
 *
 * Both routes return `Cache-Control: public, max-age=300` and the sample app enables
 * ResponseCaching (keyed by host+path, ignoring query), so a `no-cache` request is
 * required to bypass BOTH the browser and the server cache — otherwise re-reading the
 * same URL after an edit can return a stale response.
 *
 * Uses a fresh APIRequestContext (these routes are [AllowAnonymous], so no session is
 * needed) and reads the raw HTTP body, independent of any DOM rendering. Non-200
 * responses (e.g. llms.txt returns 404 when no content exists) are returned, not thrown.
 */
export async function fetchTextFile(url: string): Promise<TextFileResponse> {
  const context = await request.newContext({
    ignoreHTTPSErrors: true,
    extraHTTPHeaders: { 'Cache-Control': 'no-cache', Pragma: 'no-cache' },
  });
  try {
    const res = await context.get(url);
    const headers = res.headers();
    return {
      status: res.status(),
      contentType: headers['content-type'] ?? '',
      body: await res.text(),
      headers,
    };
  } finally {
    await context.dispose();
  }
}
