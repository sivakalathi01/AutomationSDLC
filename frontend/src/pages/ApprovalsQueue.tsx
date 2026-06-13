import React from 'react';
import { Text } from '@fluentui/react';

export default function ApprovalsQueue() {
  return (
    <div style={{ padding: '2rem' }}>
      <Text variant="xxLarge">Pending Approvals</Text>
      <p>No pending approvals</p>
    </div>
  );
}
