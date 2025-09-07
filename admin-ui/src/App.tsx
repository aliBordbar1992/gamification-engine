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
  Users,
  Leaderboards,
  Sandbox,
  Settings,
  Login,
  Unauthorized,
} from '@/pages'

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
                path="/*"
                element={
                  <RouteGuard>
                    <MainLayout>
                      <Routes>
                        <Route
                          path="/"
                          element={<Navigate to="/dashboard" replace />}
                        />
                        <Route path="/dashboard" element={<Dashboard />} />
                        <Route path="/rules" element={<Rules />} />
                        <Route path="/entities/*" element={<Entities />} />
                        <Route path="/users" element={<Users />} />
                        <Route
                          path="/leaderboards"
                          element={<Leaderboards />}
                        />
                        <Route path="/sandbox" element={<Sandbox />} />
                        <Route path="/settings" element={<Settings />} />
                      </Routes>
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
