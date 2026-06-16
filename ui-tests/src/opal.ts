import { request } from '@playwright/test';

import { SITES } from './domains';

// Opal endpoints are global routes; any host serves them. Use the Site 1 frontend
// (matching the sample's Opal/*.http files).
const OPAL_BASE = SITES.site1.frontendUrl;
const DISCOVERY_URL = `${OPAL_BASE}/stott.robotshandler/opal/discovery/`;
const GET_ROBOTS_URL = `${OPAL_BASE}/stott.robotshandler/opal/tools/get-robot-txt-configurations/`;

export interface OpalResponse {
  status: number;
  body: string;
}

/** GET the Opal discovery document (AllowAnonymous — no token required). */
export async function getDiscovery(): Promise<OpalResponse> {
  const context = await request.newContext({ ignoreHTTPSErrors: true });
  try {
    const res = await context.get(DISCOVERY_URL, { headers: { Accept: 'application/json' } });
    return { status: res.status(), body: await res.text() };
  } finally {
    await context.dispose();
  }
}

/**
 * POST the get-robot-txt-configurations tool. Pass a bearer token to authorise; omit
 * it (or pass a wrong value) to exercise the 401 path. Optionally filter by host.
 */
export async function getRobotConfigurations(opts: {
  token?: string;
  hostName?: string;
}): Promise<OpalResponse> {
  const context = await request.newContext({ ignoreHTTPSErrors: true });
  try {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    };
    if (opts.token) {
      headers.Authorization = `Bearer ${opts.token}`;
    }

    const res = await context.post(GET_ROBOTS_URL, {
      headers,
      data: { parameters: opts.hostName ? { hostName: opts.hostName } : {} },
    });
    return { status: res.status(), body: await res.text() };
  } finally {
    await context.dispose();
  }
}
