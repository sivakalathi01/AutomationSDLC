import React from 'react';
import { RouteObject } from 'react-router-dom';
import Dashboard from './pages/Dashboard';
import RunsList from './pages/RunsList';
import RunDetail from './pages/RunDetail';
import RunCreate from './pages/RunCreate';
import ApprovalsQueue from './pages/ApprovalsQueue';
import QualityResults from './pages/QualityResults';
import NotFound from './pages/NotFound';

export const routes: RouteObject[] = [
  {
    path: '/',
    element: <Dashboard />,
  },
  {
    path: '/runs',
    element: <RunsList />,
  },
  {
    path: '/runs/new',
    element: <RunCreate />,
  },
  {
    path: '/runs/:runId',
    element: <RunDetail />,
  },
  {
    path: '/approvals',
    element: <ApprovalsQueue />,
  },
  {
    path: '/quality/:runId',
    element: <QualityResults />,
  },
  {
    path: '*',
    element: <NotFound />,
  },
];
