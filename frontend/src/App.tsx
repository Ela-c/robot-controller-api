import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthContext'
import { ProtectedRoute } from './components/common/ProtectedRoute'
import { RoleGate } from './components/common/RoleGate'
import { AppShell } from './components/layout/AppShell'
import { DashboardPage } from './pages/DashboardPage'
import { LoginPage } from './pages/LoginPage'
import { MapsPage } from './pages/MapsPage'
import { MapDetailsPage } from './pages/MapDetailsPage'
import { CommandsPage } from './pages/CommandsPage'
import { CommandDetailsPage } from './pages/CommandDetailsPage'
import { SequencesPage } from './pages/SequencesPage'
import { SequenceDetailsPage } from './pages/SequenceDetailsPage'
import { UsersPage } from './pages/UsersPage'
import { ProfilePage } from './pages/ProfilePage'
import { SystemPage } from './pages/SystemPage'
import { RealtimeProvider } from './realtime/RealtimeContext'

const queryClient = new QueryClient()

const router = createBrowserRouter([
  { path: '/', element: <Navigate to="/app" replace /> },
  { path: '/login', element: <LoginPage /> },
  {
    path: '/app',
    element: <ProtectedRoute />,
    children: [
      {
        element: <AppShell />,
        children: [
          { index: true, element: <DashboardPage /> },
          { path: 'maps', element: <MapsPage /> },
          { path: 'maps/:id', element: <MapDetailsPage /> },
          { path: 'commands', element: <CommandsPage /> },
          { path: 'commands/:id', element: <CommandDetailsPage /> },
          { path: 'sequences', element: <SequencesPage /> },
          { path: 'sequences/:id', element: <SequenceDetailsPage /> },
          {
            path: 'users',
            element: (
              <RoleGate allowed={['admin']}>
                <UsersPage />
              </RoleGate>
            ),
          },
          { path: 'profile', element: <ProfilePage /> },
          { path: 'system', element: <SystemPage /> },
        ],
      },
    ],
  },
  { path: '*', element: <Navigate to="/app" replace /> },
])

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <RealtimeProvider>
          <RouterProvider router={router} />
        </RealtimeProvider>
      </AuthProvider>
    </QueryClientProvider>
  )
}

export default App
