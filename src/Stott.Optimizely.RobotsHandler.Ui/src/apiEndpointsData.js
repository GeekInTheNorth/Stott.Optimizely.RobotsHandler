/**
 * API Endpoints Documentation Data
 * Contains detailed information about all OPAL API endpoints including
 * HTTP methods, URLs, request/response examples, and parameter details.
 */

export const apiEndpoints = {
    discovery: {
        name: 'Discovery API',
        httpMethod: 'GET',
        url: '/stott.robotshandler/opal/discovery/',
        showAuthorization: false,
        requestJson: 'No request body required',
        responseJson: `{
  "functions": [
    {
      "name": "getrobottxtconfigurations",
      "description": "Get a collection of robot.txt configurations optionally filtered by host name.",
      "parameters": [
        {
          "name": "hostName",
          "type": "string",
          "description": "The host name to filter the robot.txt configurations by.",
          "required": false
        }
      ],
      "endpoint": "/tools/get-robot-txt-configurations/",
      "httpMethod": "POST"
    },
    {
      "name": "saverobottxtconfigurations",
      "description": "Saves robots.txt content for a specific Id or Host Name. Where an update is being performed for a host name and a specific configuration does not exist, one will be created.",
      "parameters": [
        {
          "name": "robotsId",
          "type": "Guid",
          "description": "The Id of the robots.txt configuration that is to be updated.",
          "required": false
        },
        {
          "name": "hostName",
          "type": "string",
          "description": "The host name of of the robots.txt configuration that is to be updated.",
          "required": false
        },
        {
          "name": "RobotsTxtContent",
          "type": "string",
          "description": "The robots.txt content to be saved. This should be a valid robots.txt content and must be provided.",
          "required": true
        }
      ],
      "endpoint": "/tools/save-robot-txt-configuration/",
      "httpMethod": "POST"
    },
    {
      "name": "getllmstxtconfigurations",
      "description": "Get a collection of llms.txt configurations optionally filtered by host name.",
      "parameters": [
        {
          "name": "hostName",
          "type": "string",
          "description": "The host name to filter the llms.txt configurations by.",
          "required": false
        }
      ],
      "endpoint": "/tools/get-llms-txt-configurations/",
      "httpMethod": "POST"
    },
    {
      "name": "savellmstxtconfigurations",
      "description": "Saves llms.txt content for a specific Id or Host Name. Where an update is being performed for a host name and a specific configuration does not exist, one will be created.",
      "parameters": [
        {
          "name": "llmsId",
          "type": "Guid",
          "description": "The Id of the llms.txt configuration that is to be updated.",
          "required": false
        },
        {
          "name": "hostName",
          "type": "string",
          "description": "The host name of of the llms.txt configuration that is to be updated.",
          "required": false
        },
        {
          "name": "LlmsTxtContent",
          "type": "string",
          "description": "The llms.txt content to be saved. This should be a valid llms.txt content and must be provided.",
          "required": true
        }
      ],
      "endpoint": "/tools/save-llms-txt-configuration/",
      "httpMethod": "POST"
    }
  ]
}`
    },
    getRobots: {
        name: 'Get robots.txt API',
        httpMethod: 'POST',
        url: '/stott.robotshandler/opal/tools/get-robot-txt-configurations/',
        showAuthorization: true,
        requestJson: `{
  "parameters": {
    "hostName": "example.com" // Optional filter
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
        httpMethod: 'POST',
        url: '/stott.robotshandler/opal/tools/save-robot-txt-configuration/',
        showAuthorization: true,
        requestJson: `{
  "parameters": {
    "robotsId": "guid", // OR use hostName instead
    "hostName": "example.com", // OR use robotsId instead
    "robotsTxtContent": "User-agent: *\\nDisallow: /admin/\\n\\nSitemap: https://example.com/sitemap.xml"
  }
}`,
        responseJson: `{
  "success": true,
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
        httpMethod: 'POST',
        url: '/stott.robotshandler/opal/tools/get-llms-txt-configurations/',
        showAuthorization: true,
        requestJson: `{
  "parameters": {
    "hostName": "example.com" // Optional filter
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
        httpMethod: 'POST',
        url: '/stott.robotshandler/opal/tools/save-llms-txt-configuration/',
        showAuthorization: true,
        requestJson: `{
  "parameters": {
    "llmsId": "guid", // OR use hostName instead
    "hostName": "example.com", // OR use llmsId instead
    "llmsTxtContent": "# LLMS.TXT\\n\\nmodel-name: GPT-4\\nmodel-version: 2024-03\\ntraining-data-cutoff: 2023-04"
  }
}`,
        responseJson: `{
  "success": true,
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
