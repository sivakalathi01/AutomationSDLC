@description('Container App name')
param appName string

@description('Location')
param location string = resourceGroup().location

@description('Managed environment resource id')
param managedEnvironmentId string

@description('User-assigned managed identity resource id')
param managedIdentityResourceId string

@description('Container registry login server')
param containerRegistryServer string

@description('Container image tag')
param imageTag string = 'latest'

@description('Key Vault URI')
param keyVaultUri string

@description('Deployment environment name')
param environment string

@description('Runtime environment for the orchestrator')
param appRuntimeEnvironment string = 'Development'

@description('Whether to use Key Vault-backed secrets')
param useKeyVaultSecrets bool = false

@description('Internal endpoint for requirements agent')
param requirementsAgentUrl string = ''

@description('Internal endpoint for specification agent')
param specificationAgentUrl string = ''

@description('Internal endpoint for story agent')
param storyAgentUrl string = ''

@description('Internal endpoint for architecture agent')
param architectureAgentUrl string = ''

@description('Internal endpoint for task planning agent')
param taskPlanningAgentUrl string = ''

@description('Internal endpoint for test design agent')
param testDesignAgentUrl string = ''

@description('Internal endpoint for code generation agent')
param codeGenerationAgentUrl string = ''

@description('Internal endpoint for IaC/CI-CD agent')
param iacCicdAgentUrl string = ''

@description('Internal endpoint for quality gate agent')
param qualityGateAgentUrl string = ''

resource orchestratorApp 'Microsoft.App/containerApps@2023-05-01' = {
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
        external: true
        targetPort: 5000
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
          name: 'cosmos-connection'
          keyVaultUrl: '${keyVaultUri}secrets/ConnectionStrings--CosmosDb'
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
        {
          name: 'jwt-authority'
          keyVaultUrl: '${keyVaultUri}secrets/Security--Jwt--Authority'
          identity: managedIdentityResourceId
        }
        {
          name: 'jwt-audience'
          keyVaultUrl: '${keyVaultUri}secrets/Security--Jwt--Audience'
          identity: managedIdentityResourceId
        }
      ] : []
    }
    template: {
      containers: [
        {
          name: 'orchestrator'
          image: '${containerRegistryServer}/orchestrator:${imageTag}'
          env: concat([
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: appRuntimeEnvironment
            }
          ], useKeyVaultSecrets ? [
            {
              name: 'KeyVault__Url'
              value: keyVaultUri
            }
            {
              name: 'ConnectionStrings__ServiceBus'
              secretRef: 'servicebus-connection'
            }
            {
              name: 'ConnectionStrings__CosmosDb'
              secretRef: 'cosmos-connection'
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
            {
              name: 'Security__EnableAuthentication'
              value: 'true'
            }
            {
              name: 'Security__Jwt__Authority'
              secretRef: 'jwt-authority'
            }
            {
              name: 'Security__Jwt__Audience'
              secretRef: 'jwt-audience'
            }
            {
              name: 'Security__RequireHttpsMetadata'
              value: 'true'
            }
          ] : [
            {
              name: 'Security__EnableAuthentication'
              value: 'false'
            }
          ], [
            {
              name: 'Services__Requirements'
              value: requirementsAgentUrl
            }
            {
              name: 'Services__Specification'
              value: specificationAgentUrl
            }
            {
              name: 'Services__Story'
              value: storyAgentUrl
            }
            {
              name: 'Services__Architecture'
              value: architectureAgentUrl
            }
            {
              name: 'Services__TaskPlanning'
              value: taskPlanningAgentUrl
            }
            {
              name: 'Services__TestDesign'
              value: testDesignAgentUrl
            }
            {
              name: 'Services__CodeGeneration'
              value: codeGenerationAgentUrl
            }
            {
              name: 'Services__IaCCicd'
              value: iacCicdAgentUrl
            }
            {
              name: 'Services__QualityGate'
              value: qualityGateAgentUrl
            }
          ])
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

output containerAppId string = orchestratorApp.id
output latestRevisionName string = orchestratorApp.properties.latestRevisionName
output fqdn string = orchestratorApp.properties.configuration.ingress.fqdn
