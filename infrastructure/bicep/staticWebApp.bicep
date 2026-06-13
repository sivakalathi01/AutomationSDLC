@description('Static Web App name')
param swaName string

@description('Location')
param location string = 'eastus'

resource staticWebApp 'Microsoft.Web/staticSites@2022-03-01' = {
  name: swaName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    repositoryUrl: 'https://github.com/virtusa/agentic-platform'
    branch: 'main'
    buildProperties: {
      appLocation: '/frontend'
      outputLocation: 'dist'
      apiLocation: 'api'
    }
  }
}

output staticWebAppUrl string = staticWebApp.properties.defaultHostname
