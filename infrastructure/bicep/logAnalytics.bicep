@description('Log Analytics Workspace')
param workspaceName string

@description('Location')
param location string = resourceGroup().location

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: workspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

output workspaceId string = logAnalyticsWorkspace.id
output workspaceCustomerId string = logAnalyticsWorkspace.properties.customerId
output workspaceSharedKey string = listKeys(logAnalyticsWorkspace.id, '2020-08-01').primarySharedKey
