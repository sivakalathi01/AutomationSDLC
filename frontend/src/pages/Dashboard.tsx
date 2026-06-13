import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Text, PrimaryButton, DefaultButton, Stack } from '@fluentui/react';

export default function Dashboard() {
  const navigate = useNavigate();

  return (
    <div style={{ padding: '2rem' }}>
      <Text variant="xxLarge">Agentic SDLC Platform</Text>
      <Stack tokens={{ childrenGap: 'l' }}>
        <PrimaryButton
          onClick={() => navigate('/runs/new')}
          text="Create New Run"
        />
        <DefaultButton
          onClick={() => navigate('/runs')}
          text="View All Runs"
        />
        <DefaultButton
          onClick={() => navigate('/approvals')}
          text="Approvals Queue"
        />
      </Stack>
    </div>
  );
}
