import { request } from '@playwright/test';

export interface TextFileResponse {
  status: number;
  contentType: string;
  body: string;
}

/**
 * Fetches a public text file (e.g. /robots.txt or /llms.txt) with caching disabled.
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
    return {
      status: res.status(),
      contentType: res.headers()['content-type'] ?? '',
      body: await res.text(),
    };
  } finally {
    await context.dispose();
  }
}
