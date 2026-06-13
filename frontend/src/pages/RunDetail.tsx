import React from 'react';
import { useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Text, Spinner, PrimaryButton } from '@fluentui/react';
import { apiClient } from '../services/apiClient';

export default function RunDetail() {
  const { runId } = useParams<{ runId: string }>();
  const queryClient = useQueryClient();
  const { data, isLoading, error } = useQuery({
    queryKey: ['run', runId],
    queryFn: () => apiClient.get(`/api/runs/${runId}`),
    enabled: !!runId,
  });

  const executePhase1Mutation = useMutation({
    mutationFn: async () => apiClient.post(`/api/runs/${runId}/execute-phase1`),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['run', runId] });
      await queryClient.invalidateQueries({ queryKey: ['runs'] });
    },
  });

  if (isLoading) return <Spinner label="Loading run details..." />;
  if (error) return <div>Error loading run</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <Text variant="xxLarge">Run {runId}</Text>
      <div>
        <p>Status: {data?.data?.status}</p>
        <PrimaryButton
          text={executePhase1Mutation.isLoading ? 'Executing...' : 'Execute Phase 1'}
          disabled={executePhase1Mutation.isLoading}
          onClick={() => executePhase1Mutation.mutate()}
        />
        <pre>{JSON.stringify(data?.data, null, 2)}</pre>
      </div>
    </div>
  );
}
