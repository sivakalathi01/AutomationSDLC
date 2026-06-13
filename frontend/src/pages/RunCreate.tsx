import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { Text, PrimaryButton } from '@fluentui/react';
import { apiClient } from '../services/apiClient';

export default function RunCreate() {
  const navigate = useNavigate();
  const [objective, setObjective] = React.useState('Build a .NET microservice with React frontend');
  const [domain, setDomain] = React.useState('engineering-platform');
  const [approvalMode, setApprovalMode] = React.useState('hybrid');

  const createRunMutation = useMutation({
    mutationFn: async () =>
      apiClient.post('/api/runs', {
        objective,
        projectContext: {
          domain,
          targetStack: 'dotnet_azure',
          environment: 'dev',
        },
        governanceProfile: {
          approvalMode,
          complianceLevel: 'medium',
        },
        requestedStages: ['requirements', 'specification', 'story'],
        priorArtifacts: [],
      }),
    onSuccess: (response) => {
      const newRunId = response?.data?.runId;
      if (newRunId) {
        navigate(`/runs/${newRunId}`);
      }
    },
  });

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    createRunMutation.mutate();
  };

  return (
    <div style={{ padding: '2rem' }}>
      <Text variant="xxLarge">Create New Run</Text>
      <form onSubmit={handleSubmit}>
        <div>
          <label>Objective: <input type="text" value={objective} onChange={(e) => setObjective(e.target.value)} /></label>
        </div>
        <div>
          <label>Domain: <input type="text" value={domain} onChange={(e) => setDomain(e.target.value)} /></label>
        </div>
        <div>
          <label>
            Approval Mode:
            <select value={approvalMode} onChange={(e) => setApprovalMode(e.target.value)}>
              <option>hybrid</option>
              <option>manual</option>
              <option>auto</option>
            </select>
          </label>
        </div>
        <PrimaryButton type="submit" text={createRunMutation.isLoading ? 'Creating...' : 'Create Run'} disabled={createRunMutation.isLoading} />
        {createRunMutation.isError && <p>Failed to create run.</p>}
      </form>
    </div>
  );
}
