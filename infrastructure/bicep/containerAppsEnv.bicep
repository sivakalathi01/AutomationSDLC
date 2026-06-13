@description('Container Apps Environment name')
param caEnvironmentName string

@description('Location')
param location string = resourceGroup().location

@description('Log Analytics Workspace ID')
param logAnalyticsWorkspaceId string

@description('Log Analytics Workspace customer ID (GUID)')
param logAnalyticsWorkspaceCustomerId string

@description('Log Analytics Workspace shared key')
@secure()
param logAnalyticsWorkspaceSharedKey string

resource containerAppsEnv 'Microsoft.App/managedEnvironments@2023-04-01-preview' = {
  name: caEnvironmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspaceCustomerId
        sharedKey: logAnalyticsWorkspaceSharedKey
      }
    }
  }
}

output containerAppsEnvId string = containerAppsEnv.id
output containerAppsEnvName string = containerAppsEnv.name
