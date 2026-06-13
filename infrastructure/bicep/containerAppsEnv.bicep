@description('Container Apps Environment name')
param caEnvironmentName string

@description('Location')
param location string = resourceGroup().location

@description('Log Analytics Workspace ID')
param logAnalyticsWorkspaceId string

resource containerAppsEnv 'Microsoft.App/managedEnvironments@2023-04-01-preview' = {
  name: caEnvironmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: split(logAnalyticsWorkspaceId, '/')[8]
      }
    }
  }
}

output containerAppsEnvId string = containerAppsEnv.id
output containerAppsEnvName string = containerAppsEnv.name
