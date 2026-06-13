import React from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Text, Spinner } from '@fluentui/react';
import { apiClient } from '../services/apiClient';

export default function RunsList() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['runs'],
    queryFn: () => apiClient.get('/api/runs'),
  });

  if (isLoading) return <Spinner label="Loading runs..." />;
  if (error) return <div>Error loading runs</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <Text variant="xxLarge">All Runs</Text>
      <div>
        {data?.data?.runs?.length ? (
          <ul>
            {data.data.runs.map((run: any) => (
              <li key={run.runId}>
                <Link to={`/runs/${run.runId}`}>{run.runId}</Link> - {run.status}
              </li>
            ))}
          </ul>
        ) : (
          <p>No runs found</p>
        )}
      </div>
    </div>
  );
}
