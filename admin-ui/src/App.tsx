import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ConfigProvider } from 'antd'
import { AuthProvider } from '@/stores/AuthContext'
import MainLayout from '@/layouts/MainLayout'
import RouteGuard from '@/components/RouteGuard'
import {
  Dashboard,
  Rules,
  Entities,
  Badges,
  Trophies,
  Levels,
  PointCategories,
  Users,
  Leaderboards,
  Sandbox,
  Settings,
  Login,
  Unauthorized,
} from '@/pages'
import TestApi from '@/pages/TestApi'

// Create a client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 3,
      retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
      staleTime: 5 * 60 * 1000, // 5 minutes
    },
  },
})

// Ant Design theme configuration
const theme = {
  token: {
    colorPrimary: '#1890ff',
    borderRadius: 6,
  },
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ConfigProvider theme={theme}>
        <AuthProvider>
          <Router>
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route path="/unauthorized" element={<Unauthorized />} />
              <Route
                path="/"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Navigate to="/dashboard" replace />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/dashboard"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Dashboard />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/rules"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Rules />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/rules/:id"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Rules />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/test-api"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <TestApi />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/entities"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Entities />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/entities/badges"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Badges />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/entities/trophies"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Trophies />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/entities/levels"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Levels />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/entities/point-categories"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <PointCategories />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/users"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Users />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/users/:id/:action"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Users />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/users/:id"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Users />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/leaderboards"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Leaderboards />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/sandbox"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Sandbox />
                    </MainLayout>
                  </RouteGuard>
                }
              />
              <Route
                path="/settings"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Settings />
                    </MainLayout>
                  </RouteGuard>
                }
              />
            </Routes>
          </Router>
        </AuthProvider>
      </ConfigProvider>
    </QueryClientProvider>
  )
}

export default App
