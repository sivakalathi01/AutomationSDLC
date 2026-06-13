@description('Service Bus Namespace name')
param sbNamespace string

@description('Location for resources')
param location string = resourceGroup().location

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2021-11-01' = {
  name: sbNamespace
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    zoneRedundant: false
  }
}

resource agentTasksTopic 'Microsoft.ServiceBus/namespaces/topics@2021-11-01' = {
  parent: serviceBusNamespace
  name: 'agent-tasks'
  properties: {
    defaultMessageTimeToLive: 'P1D'
    maxSizeInMegabytes: 1024
  }
}

resource qualityGateTopic 'Microsoft.ServiceBus/namespaces/topics@2021-11-01' = {
  parent: serviceBusNamespace
  name: 'quality-gates'
  properties: {
    defaultMessageTimeToLive: 'P1D'
  }
}

output sbNamespaceId string = serviceBusNamespace.id
