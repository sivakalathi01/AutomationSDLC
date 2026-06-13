import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Stack, PrimaryButton, DefaultButton } from '@fluentui/react';

const Layout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const navigate = useNavigate();

  const navItems: Array<{ name: string; url: string }> = [
    { name: 'Dashboard', url: '/' },
    { name: 'Runs', url: '/runs' },
    { name: 'New Run', url: '/runs/new' },
    { name: 'Approvals', url: '/approvals' },
  ];

  return (
    <Stack horizontal styles={{ root: { height: '100vh' } }}>
      <div style={{ width: 250, borderRight: '1px solid #e1e1e1', padding: '1rem', display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
        {navItems.map((item, index) =>
          index === 0 ? (
            <PrimaryButton key={item.url} text={item.name} onClick={() => navigate(item.url)} />
          ) : (
            <DefaultButton key={item.url} text={item.name} onClick={() => navigate(item.url)} />
          )
        )}
      </div>
      <div style={{ flex: 1, overflow: 'auto' }}>
        {children}
      </div>
    </Stack>
  );
};

export default Layout;
