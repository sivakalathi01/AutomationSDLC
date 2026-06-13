@description('Azure Container Registry name')
param acrName string

@description('Location for resources')
param location string = resourceGroup().location

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-06-01-preview' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

output registryUrl string = containerRegistry.properties.loginServer
output registryId string = containerRegistry.id
