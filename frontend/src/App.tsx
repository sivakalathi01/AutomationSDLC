import React from 'react';
import { useRoutes } from 'react-router-dom';
import Layout from './components/Layout';
import { routes } from './routes';
import './App.css';

function App() {
  const routeElements = useRoutes(routes);

  return (
    <Layout>
      {routeElements}
    </Layout>
  );
}

export default App;
