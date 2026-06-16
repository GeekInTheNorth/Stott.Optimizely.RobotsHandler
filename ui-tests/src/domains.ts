// Domain map for the sample site (Sample/OptimizelyTwelveTest).
// See SetupMigrationStep.cs and Program.cs: two sites, each with a frontend
// (Primary) host and a CMS editor (Edit) host. Sanitised host values match
// what RobotsContentService stores/compares (host + non-default port).

export const SITES = {
  site1: {
    appId: 'TestWebsite1',
    appName: 'Test Website 1',
    frontendHost: 'localhost:5000',
    frontendUrl: 'https://localhost:5000',
    editorHost: 'localhost:5001',
    editorUrl: 'https://localhost:5001',
  },
  site2: {
    appId: 'TestWebsite2',
    appName: 'Test Website 2',
    frontendHost: 'localhost:5002',
    frontendUrl: 'https://localhost:5002',
    editorHost: 'localhost:5003',
    editorUrl: 'https://localhost:5003',
  },
} as const;

export const ADMIN_HOST = 'https://localhost:5001';
export const ADMIN_URL = `${ADMIN_HOST}/stott.robotshandler/administration/`;

// Unique, identifiable robots.txt content bound to each frontend host only.
export const SITE1_MARKER = 'UITEST-SITE1-FRONTEND';
export const SITE2_MARKER = 'UITEST-SITE2-FRONTEND';
export const SITE1_ROBOTS = `# ${SITE1_MARKER}\nUser-agent: *\nDisallow: /site1-only`;
export const SITE2_ROBOTS = `# ${SITE2_MARKER}\nUser-agent: *\nDisallow: /site2-only`;

// Unique, identifiable llms.txt content (markdown) bound to each frontend host only.
export const SITE1_LLMS_MARKER = 'UITEST-SITE1-LLMS';
export const SITE2_LLMS_MARKER = 'UITEST-SITE2-LLMS';
export const SITE1_LLMS = `# ${SITE1_LLMS_MARKER}\n\n> Site 1 frontend llms content`;
export const SITE2_LLMS = `# ${SITE2_LLMS_MARKER}\n\n> Site 2 frontend llms content`;
