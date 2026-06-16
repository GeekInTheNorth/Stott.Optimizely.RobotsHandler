import { request } from '@playwright/test';

import { SITES } from './domains';

// Opal endpoints are global routes; any host serves them. Use the Site 1 frontend
// (matching the sample's Opal/*.http files).
const OPAL_BASE = SITES.site1.frontendUrl;
const DISCOVERY_URL = `${OPAL_BASE}/stott.robotshandler/opal/discovery/`;
const GET_ROBOTS_URL = `${OPAL_BASE}/stott.robotshandler/opal/tools/get-robot-txt-configurations/`;
const SAVE_ROBOTS_URL = `${OPAL_BASE}/stott.robotshandler/opal/tools/save-robot-txt-configuration/`;

export interface OpalResponse {
  status: number;
  body: string;
}

/** A single robots config as returned by get-robot-txt-configurations (camelCase). */
export interface OpalRobotConfig {
  id: string;
  siteName: string;
  specificHost: string;
  content: string;
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

/**
 * POST the save-robot-txt-configuration tool. The backend resolves the target from
 * robotsId and/or hostName. Pass a bearer token to authorise (Write scope required);
 * omit or use an insufficient token to exercise the 401 path.
 */
export async function saveRobotConfiguration(opts: {
  token?: string;
  hostName?: string;
  robotsId?: string;
  robotsTxtContent?: string;
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

    const parameters: Record<string, string> = {};
    if (opts.hostName !== undefined) parameters.hostName = opts.hostName;
    if (opts.robotsId !== undefined) parameters.robotsId = opts.robotsId;
    if (opts.robotsTxtContent !== undefined) parameters.robotsTxtContent = opts.robotsTxtContent;

    const res = await context.post(SAVE_ROBOTS_URL, { headers, data: { parameters } });
    return { status: res.status(), body: await res.text() };
  } finally {
    await context.dispose();
  }
}

/**
 * Parse a single get-robot-txt-configurations response (the shape returned when a
 * hostName filter matches one config). Returns null if the body isn't a config object
 * (e.g. a { success: false } message). Field names are camelCase (CreateSafeJsonResult).
 */
export function parseRobotConfig(body: string): OpalRobotConfig | null {
  try {
    const obj = JSON.parse(body) as Partial<OpalRobotConfig>;
    if (obj && typeof obj === 'object' && typeof obj.content === 'string') {
      return {
        id: obj.id ?? '',
        siteName: obj.siteName ?? '',
        specificHost: obj.specificHost ?? '',
        content: obj.content,
      };
    }
    return null;
  } catch {
    return null;
  }
}
