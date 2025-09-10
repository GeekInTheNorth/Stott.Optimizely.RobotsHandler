/**
 * API Endpoints Documentation Data
 * Contains detailed information about all OPAL API endpoints including
 * HTTP methods, URLs, request/response examples, and parameter details.
 */

export const apiEndpoints = {
    discovery: {
        name: 'Discovery API',
        description: 'Fetches a list of available API functions, their parameters, and endpoints.',
        endpoint: 'GET: /stott.robotshandler/opal/discovery/',
        requestJson: 'No request body required',
        responseJson: `{
  "functions": [
    {
      "name": "string",
      "description": "string",
      "parameters": [
        {
          "name": "string",
          "type": "string",
          "description": "string",
          "required": boolean
        }
      ],
      "endpoint": "/relative/url/",
      "http_method": "POST"
    }
  ]
}`
    },
    getRobots: {
        name: 'Get robots.txt API',
        description: 'Fetches a list of robots.txt configurations filtered by an optional hostName.  This API requires a bearer token with at least "Read" permission for robots.txt content.',
        endpoint: `POST: /stott.robotshandler/opal/tools/get-robot-txt-configurations/
Authorization: bearer token-value`,
        requestJson: `{
  "parameters": {
    "hostName": "example.com"
  }
}`,
        responseJson: `[
  {
    "id": "guid",
    "siteName": "string",
    "isDefaultForSite": boolean,
    "specificHost": "string",
    "robotsContent": "string"
  }
]`
    },
    saveRobots: {
        name: 'Save robots.txt API',
        description: 'Saves a single robots.txt configuration based on robotsId or hostName.  This API requires a bearer token with "Write" permission for robots.txt content',
        endpoint: `POST: /stott.robotshandler/opal/tools/save-robot-txt-configuration/
Authorization: bearer token-value`,
        requestJson: `{
  "parameters": {
    "robotsId": "guid",
    "hostName": "example.com",
    "robotsTxtContent": "User-agent: *\\nDisallow: /admin/\\n\\nSitemap: https://example.com/sitemap.xml"
  }
}`,
        responseJson: `{
  "success": boolean,
  "message": "string",
  "data": {
    "id": "guid",
    "siteId": "guid",
    "siteName": "string",
    "specificHost": "string",
    "robotsContent": "string"
  }
}`
    },
    getLlms: {
        name: 'Get llms.txt API',
        description: 'Fetches a list of llms.txt configurations filtered by an optional hostName.  This API requires a bearer token with at least "Read" permission for llms.txt content.',
        endpoint: `POST: /stott.robotshandler/opal/tools/get-llms-txt-configurations/
Authorization: bearer token-value`,
        requestJson: `{
  "parameters": {
    "hostName": "example.com"
  }
}`,
        responseJson: `[
  {
    "id": "guid",
    "siteName": "string",
    "isDefaultForSite": boolean,
    "specificHost": "string",
    "llmsContent": "string"
  }
]`
    },
    saveLlms: {
        name: 'Save llms.txt API',
        description: 'Saves a single llms.txt configuration based on llmsId or hostName.  This API requires a bearer token with "Write" permission for llms.txt content',
        endpoint: `POST: /stott.robotshandler/opal/tools/save-llms-txt-configuration/
Authorization: bearer token-value`,
        requestJson: `{
  "parameters": {
    "llmsId": "guid",
    "hostName": "example.com",
    "llmsTxtContent": "# LLMS.TXT\\n\\nmodel-name: GPT-4\\nmodel-version: 2024-03\\ntraining-data-cutoff: 2023-04"
  }
}`,
        responseJson: `{
  "success": boolean,
  "message": "string",
  "data": {
    "id": "guid",
    "siteId": "guid",
    "siteName": "string",
    "specificHost": "string",
    "llmsContent": "string"
  }
}`
    }
};
