@description('Container App name')
param appName string

@description('Container name')
param containerName string

@description('Location')
param location string = resourceGroup().location

@description('Managed environment resource id')
param managedEnvironmentId string

@description('User-assigned managed identity resource id')
param managedIdentityResourceId string

@description('Container registry login server')
param containerRegistryServer string

@description('Image repository name')
param imageRepository string

@description('Container image tag')
param imageTag string = 'latest'

@description('Key Vault URI')
param keyVaultUri string

@description('Deployment environment name')
param environment string

@description('Runtime environment name')
param runtimeEnvironment string = 'Production'

@description('Whether to use Key Vault-backed secrets')
param useKeyVaultSecrets bool = true

@description('Container target port')
param targetPort int = 5000

resource agentApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: appName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityResourceId}': {}
    }
  }
  properties: {
    managedEnvironmentId: managedEnvironmentId
    configuration: {
      ingress: {
        external: false
        targetPort: targetPort
        transport: 'auto'
      }
      registries: [
        {
          server: containerRegistryServer
          identity: managedIdentityResourceId
        }
      ]
      secrets: useKeyVaultSecrets ? [
        {
          name: 'servicebus-connection'
          keyVaultUrl: '${keyVaultUri}secrets/ConnectionStrings--ServiceBus'
          identity: managedIdentityResourceId
        }
        {
          name: 'openai-endpoint'
          keyVaultUrl: '${keyVaultUri}secrets/AzureOpenAI--Endpoint'
          identity: managedIdentityResourceId
        }
        {
          name: 'openai-apikey'
          keyVaultUrl: '${keyVaultUri}secrets/AzureOpenAI--ApiKey'
          identity: managedIdentityResourceId
        }
        {
          name: 'openai-deployment'
          keyVaultUrl: '${keyVaultUri}secrets/AzureOpenAI--DeploymentName'
          identity: managedIdentityResourceId
        }
      ] : []
    }
    template: {
      containers: [
        {
          name: containerName
          image: '${containerRegistryServer}/${imageRepository}:${imageTag}'
          env: useKeyVaultSecrets ? [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: runtimeEnvironment
            }
            {
              name: 'KeyVault__Url'
              value: keyVaultUri
            }
            {
              name: 'ConnectionStrings__ServiceBus'
              secretRef: 'servicebus-connection'
            }
            {
              name: 'AzureOpenAI__Endpoint'
              secretRef: 'openai-endpoint'
            }
            {
              name: 'AzureOpenAI__ApiKey'
              secretRef: 'openai-apikey'
            }
            {
              name: 'AzureOpenAI__DeploymentName'
              secretRef: 'openai-deployment'
            }
          ] : [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: runtimeEnvironment
            }
            {
              name: 'KeyVault__Url'
              value: keyVaultUri
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 2
      }
    }
  }
}

output containerAppId string = agentApp.id
output fqdn string = agentApp.properties.configuration.ingress.fqdn
