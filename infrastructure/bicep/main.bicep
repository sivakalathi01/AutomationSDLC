targetScope = 'subscription'

@description('Resource Group name')
param resourceGroupName string

@description('Location')
param location string = 'eastus'

@description('Static Web App location (limited set of supported regions)')
param staticWebAppLocation string = 'eastus2'

@description('Environment')
param environment string = 'dev'

@description('SQL administrator login username')
param sqlAdminLogin string = 'sqladminagentic'

@description('SQL administrator login password')
@secure()
param sqlAdminPassword string

// Create resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
}

// Create Log Analytics Workspace
module logAnalytics 'logAnalytics.bicep' = {
  scope: rg
  name: 'logAnalytics'
  params: {
    workspaceName: 'la-${environment}-agentic'
    location: location
  }
}

// Create Container Registry
module acr 'containerRegistry.bicep' = {
  scope: rg
  name: 'containerRegistry'
  params: {
    acrName: 'acr${environment}agentic'
    location: location
  }
}

// Create Service Bus
module serviceBus 'serviceBus.bicep' = {
  scope: rg
  name: 'serviceBus'
  params: {
    sbNamespace: 'sb-${environment}-agentic'
    location: location
  }
}

// Create Cosmos DB
module cosmosDb 'cosmosDb.bicep' = {
  scope: rg
  name: 'cosmosDb'
  params: {
    cosmosAccountName: 'cosmos-${environment}-agentic'
    location: location
  }
}

// Create Azure SQL Database (Phase 2 read model and reporting store)
module sqlDatabase 'sqlDatabase.bicep' = {
  scope: rg
  name: 'sqlDatabase'
  params: {
    sqlServerName: 'sql-${environment}-agentic'
    sqlDatabaseName: 'agentic-readmodel'
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
    location: location
  }
}

// Create Key Vault for secrets and configuration
module keyVault 'keyVault.bicep' = {
  scope: rg
  name: 'keyVault'
  params: {
    keyVaultName: 'kv-${environment}-agentic'
    location: location
  }
}

// Create user-assigned managed identity for platform services
module managedIdentity 'managedIdentity.bicep' = {
  scope: rg
  name: 'managedIdentity'
  params: {
    identityName: 'mi-${environment}-agentic-platform'
    location: location
  }
}

// Grant the platform identity permission to pull images from ACR
module acrPullRoleAssignment 'acrPullRoleAssignment.bicep' = {
  scope: rg
  name: 'acrPullRoleAssignment'
  params: {
    acrName: 'acr${environment}agentic'
    principalId: managedIdentity.outputs.principalId
  }
}

// Grant the platform identity access to Key Vault secrets
module keyVaultRoleAssignment 'keyVaultRoleAssignment.bicep' = {
  scope: rg
  name: 'keyVaultRoleAssignment'
  params: {
    keyVaultName: 'kv-${environment}-agentic'
    principalId: managedIdentity.outputs.principalId
  }
}

// Create Container Apps Environment
module containerAppsEnv 'containerAppsEnv.bicep' = {
  scope: rg
  name: 'containerAppsEnv'
  params: {
    caEnvironmentName: 'cae-${environment}-agentic'
    location: location
    logAnalyticsWorkspaceId: logAnalytics.outputs.workspaceId
    logAnalyticsWorkspaceSharedKey: logAnalytics.outputs.workspaceSharedKey
  }
}

// Create Azure Static Web Apps for Frontend
module staticWebApp 'staticWebApp.bicep' = {
  scope: rg
  name: 'staticWebApp'
  params: {
    swaName: 'swa-${environment}-agentic'
    location: staticWebAppLocation
  }
}

module requirementsAgentContainerApp './agentContainerApp.bicep' = {
  scope: rg
  name: 'requirementsAgentContainerApp'
  params: {
    appName: 'requirements-${environment}-agentic'
    containerName: 'requirements-agent'
    location: location
    managedEnvironmentId: containerAppsEnv.outputs.containerAppsEnvId
    managedIdentityResourceId: managedIdentity.outputs.identityId
    containerRegistryServer: acr.outputs.registryUrl
    imageRepository: 'agentrequirements'
    keyVaultUri: keyVault.outputs.keyVaultUri
    environment: environment
  }
}

module specificationAgentContainerApp './agentContainerApp.bicep' = {
  scope: rg
  name: 'specificationAgentContainerApp'
  params: {
    appName: 'specification-${environment}-agentic'
    containerName: 'specification-agent'
    location: location
    managedEnvironmentId: containerAppsEnv.outputs.containerAppsEnvId
    managedIdentityResourceId: managedIdentity.outputs.identityId
    containerRegistryServer: acr.outputs.registryUrl
    imageRepository: 'agentspecification'
    keyVaultUri: keyVault.outputs.keyVaultUri
    environment: environment
  }
}

module storyAgentContainerApp './agentContainerApp.bicep' = {
  scope: rg
  name: 'storyAgentContainerApp'
  params: {
    appName: 'story-${environment}-agentic'
    containerName: 'story-agent'
    location: location
    managedEnvironmentId: containerAppsEnv.outputs.containerAppsEnvId
    managedIdentityResourceId: managedIdentity.outputs.identityId
    containerRegistryServer: acr.outputs.registryUrl
    imageRepository: 'agentstory'
    keyVaultUri: keyVault.outputs.keyVaultUri
    environment: environment
  }
}

module architectureAgentContainerApp './agentContainerApp.bicep' = {
  scope: rg
  name: 'architectureAgentContainerApp'
  params: {
    appName: 'architecture-${environment}-agentic'
    containerName: 'architecture-agent'
    location: location
    managedEnvironmentId: containerAppsEnv.outputs.containerAppsEnvId
    managedIdentityResourceId: managedIdentity.outputs.identityId
    containerRegistryServer: acr.outputs.registryUrl
    imageRepository: 'agentarchitecture'
    keyVaultUri: keyVault.outputs.keyVaultUri
    environment: environment
  }
}

module taskPlanningAgentContainerApp './agentContainerApp.bicep' = {
  scope: rg
  name: 'taskPlanningAgentContainerApp'
  params: {
    appName: 'task-planning-${environment}-agentic'
    containerName: 'task-planning-agent'
    location: location
    managedEnvironmentId: containerAppsEnv.outputs.containerAppsEnvId
    managedIdentityResourceId: managedIdentity.outputs.identityId
    containerRegistryServer: acr.outputs.registryUrl
    imageRepository: 'agenttaskplanning'
    keyVaultUri: keyVault.outputs.keyVaultUri
    environment: environment
  }
}

module testDesignAgentContainerApp './agentContainerApp.bicep' = {
  scope: rg
  name: 'testDesignAgentContainerApp'
  params: {
    appName: 'test-design-${environment}-agentic'
    containerName: 'test-design-agent'
    location: location
    managedEnvironmentId: containerAppsEnv.outputs.containerAppsEnvId
    managedIdentityResourceId: managedIdentity.outputs.identityId
    containerRegistryServer: acr.outputs.registryUrl
    imageRepository: 'agenttestdesign'
    keyVaultUri: keyVault.outputs.keyVaultUri
    environment: environment
  }
}

module codeGenerationAgentContainerApp './agentContainerApp.bicep' = {
  scope: rg
  name: 'codeGenerationAgentContainerApp'
  params: {
    appName: 'code-generation-${environment}-agentic'
    containerName: 'code-generation-agent'
    location: location
    managedEnvironmentId: containerAppsEnv.outputs.containerAppsEnvId
    managedIdentityResourceId: managedIdentity.outputs.identityId
    containerRegistryServer: acr.outputs.registryUrl
    imageRepository: 'agentcodegeneration'
    keyVaultUri: keyVault.outputs.keyVaultUri
    environment: environment
  }
}

module iacCicdAgentContainerApp './agentContainerApp.bicep' = {
  scope: rg
  name: 'iacCicdAgentContainerApp'
  params: {
    appName: 'iac-cicd-${environment}-agentic'
    containerName: 'iac-cicd-agent'
    location: location
    managedEnvironmentId: containerAppsEnv.outputs.containerAppsEnvId
    managedIdentityResourceId: managedIdentity.outputs.identityId
    containerRegistryServer: acr.outputs.registryUrl
    imageRepository: 'agentiaccicd'
    keyVaultUri: keyVault.outputs.keyVaultUri
    environment: environment
  }
}

module qualityGateAgentContainerApp './agentContainerApp.bicep' = {
  scope: rg
  name: 'qualityGateAgentContainerApp'
  params: {
    appName: 'quality-gate-${environment}-agentic'
    containerName: 'quality-gate-agent'
    location: location
    managedEnvironmentId: containerAppsEnv.outputs.containerAppsEnvId
    managedIdentityResourceId: managedIdentity.outputs.identityId
    containerRegistryServer: acr.outputs.registryUrl
    imageRepository: 'qualitygateservice'
    keyVaultUri: keyVault.outputs.keyVaultUri
    environment: environment
  }
}

// Deploy the orchestrator workload using managed identity, Key Vault-backed secret references,
// and internal service discovery endpoints for the agent fleet.
module orchestratorContainerApp 'orchestratorContainerApp.bicep' = {
  scope: rg
  name: 'orchestratorContainerApp'
  params: {
    appName: 'orchestrator-${environment}-agentic'
    location: location
    managedEnvironmentId: containerAppsEnv.outputs.containerAppsEnvId
    managedIdentityResourceId: managedIdentity.outputs.identityId
    containerRegistryServer: acr.outputs.registryUrl
    keyVaultUri: keyVault.outputs.keyVaultUri
    environment: environment
    requirementsAgentUrl: 'https://${requirementsAgentContainerApp.outputs.fqdn}'
    specificationAgentUrl: 'https://${specificationAgentContainerApp.outputs.fqdn}'
    storyAgentUrl: 'https://${storyAgentContainerApp.outputs.fqdn}'
    architectureAgentUrl: 'https://${architectureAgentContainerApp.outputs.fqdn}'
    taskPlanningAgentUrl: 'https://${taskPlanningAgentContainerApp.outputs.fqdn}'
    testDesignAgentUrl: 'https://${testDesignAgentContainerApp.outputs.fqdn}'
    codeGenerationAgentUrl: 'https://${codeGenerationAgentContainerApp.outputs.fqdn}'
    iacCicdAgentUrl: 'https://${iacCicdAgentContainerApp.outputs.fqdn}'
    qualityGateAgentUrl: 'https://${qualityGateAgentContainerApp.outputs.fqdn}'
  }
}

output resourceGroupId string = rg.id
output acrUrl string = acr.outputs.registryUrl
output serviceBusId string = serviceBus.outputs.sbNamespaceId
output cosmosDbEndpoint string = cosmosDb.outputs.cosmosEndpoint
output sqlServerFqdn string = sqlDatabase.outputs.sqlServerFqdn
output sqlDatabaseName string = sqlDatabase.outputs.sqlDatabaseName
output keyVaultUri string = keyVault.outputs.keyVaultUri
output platformManagedIdentityClientId string = managedIdentity.outputs.clientId
output orchestratorUrl string = orchestratorContainerApp.outputs.fqdn
output requirementsAgentUrl string = requirementsAgentContainerApp.outputs.fqdn
output specificationAgentUrl string = specificationAgentContainerApp.outputs.fqdn
output storyAgentUrl string = storyAgentContainerApp.outputs.fqdn
output architectureAgentUrl string = architectureAgentContainerApp.outputs.fqdn
output taskPlanningAgentUrl string = taskPlanningAgentContainerApp.outputs.fqdn
output testDesignAgentUrl string = testDesignAgentContainerApp.outputs.fqdn
output codeGenerationAgentUrl string = codeGenerationAgentContainerApp.outputs.fqdn
output iacCicdAgentUrl string = iacCicdAgentContainerApp.outputs.fqdn
output qualityGateAgentUrl string = qualityGateAgentContainerApp.outputs.fqdn
