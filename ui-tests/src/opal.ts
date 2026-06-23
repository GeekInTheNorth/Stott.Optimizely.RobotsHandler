import { request } from '@playwright/test';

import { SITES } from './domains';

// Opal endpoints are global routes; any host serves them. Use the Site 1 frontend
// (matching the sample's Opal/*.http files).
const OPAL_BASE = SITES.site1.frontendUrl;
const DISCOVERY_URL = `${OPAL_BASE}/stott.robotshandler/opal/discovery/`;
const GET_ROBOTS_URL = `${OPAL_BASE}/stott.robotshandler/opal/tools/get-robot-txt-configurations/`;
const SAVE_ROBOTS_URL = `${OPAL_BASE}/stott.robotshandler/opal/tools/save-robot-txt-configuration/`;
const GET_LLMS_URL = `${OPAL_BASE}/stott.robotshandler/opal/tools/get-llms-txt-configurations/`;
const SAVE_LLMS_URL = `${OPAL_BASE}/stott.robotshandler/opal/tools/save-llms-txt-configuration/`;

export interface OpalResponse {
  status: number;
  body: string;
}

/** A single config as returned by the get-*-configurations tools (camelCase). */
export interface OpalConfig {
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

// POST an Opal tool with optional bearer token and a parameters object.
async function postOpalTool(
  url: string,
  token: string | undefined,
  parameters: Record<string, string>,
): Promise<OpalResponse> {
  const context = await request.newContext({ ignoreHTTPSErrors: true });
  try {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    };
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }
    const res = await context.post(url, { headers, data: { parameters } });
    return { status: res.status(), body: await res.text() };
  } finally {
    await context.dispose();
  }
}

// --- robots.txt tools ---

/** POST get-robot-txt-configurations. Omit/wrong token exercises the 401 path. */
export function getRobotConfigurations(opts: { token?: string; hostName?: string }): Promise<OpalResponse> {
  return postOpalTool(GET_ROBOTS_URL, opts.token, opts.hostName ? { hostName: opts.hostName } : {});
}

/** POST save-robot-txt-configuration (Write scope required). */
export function saveRobotConfiguration(opts: {
  token?: string;
  hostName?: string;
  robotsId?: string;
  robotsTxtContent?: string;
}): Promise<OpalResponse> {
  const parameters: Record<string, string> = {};
  if (opts.hostName !== undefined) parameters.hostName = opts.hostName;
  if (opts.robotsId !== undefined) parameters.robotsId = opts.robotsId;
  if (opts.robotsTxtContent !== undefined) parameters.robotsTxtContent = opts.robotsTxtContent;
  return postOpalTool(SAVE_ROBOTS_URL, opts.token, parameters);
}

// --- llms.txt tools ---

/** POST get-llms-txt-configurations. Omit/wrong token exercises the 401 path. */
export function getLlmsConfigurations(opts: { token?: string; hostName?: string }): Promise<OpalResponse> {
  return postOpalTool(GET_LLMS_URL, opts.token, opts.hostName ? { hostName: opts.hostName } : {});
}

/** POST save-llms-txt-configuration (Write scope required). */
export function saveLlmsConfiguration(opts: {
  token?: string;
  hostName?: string;
  llmsId?: string;
  llmsTxtContent?: string;
}): Promise<OpalResponse> {
  const parameters: Record<string, string> = {};
  if (opts.hostName !== undefined) parameters.hostName = opts.hostName;
  if (opts.llmsId !== undefined) parameters.llmsId = opts.llmsId;
  if (opts.llmsTxtContent !== undefined) parameters.llmsTxtContent = opts.llmsTxtContent;
  return postOpalTool(SAVE_LLMS_URL, opts.token, parameters);
}

/**
 * Parse a single get-*-configurations response (the shape returned when a hostName
 * filter matches one config). Returns null if the body isn't a config object (e.g. a
 * { success: false } message). Field names are camelCase (CreateSafeJsonResult).
 */
export function parseOpalConfig(body: string): OpalConfig | null {
  try {
    const obj = JSON.parse(body) as Partial<OpalConfig>;
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
