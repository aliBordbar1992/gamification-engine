import React, { createContext, useState, useEffect } from 'react'
import type { AuthContextType, User, LoginCredentials } from '@/types/auth'

const AuthContext = createContext<AuthContextType | undefined>(undefined)

interface AuthProviderProps {
  children: React.ReactNode
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    // Simulate checking for existing auth token
    const checkAuth = async () => {
      setIsLoading(true)
      try {
        // In a real app, this would check localStorage or make an API call
        const token = localStorage.getItem('auth_token')
        if (token) {
          // Mock user data - in real app, this would come from API
          setUser({
            id: '1',
            email: 'admin@example.com',
            name: 'Admin User',
            role: 'Admin',
          })
        }
      } catch (error) {
        console.error('Auth check failed:', error)
      } finally {
        setIsLoading(false)
      }
    }

    checkAuth()
  }, [])

  const login = async (credentials: LoginCredentials) => {
    setIsLoading(true)
    try {
      // Mock login - in real app, this would make an API call
      if (
        credentials.email === 'admin@example.com' &&
        credentials.password === 'admin'
      ) {
        const mockUser: User = {
          id: '1',
          email: credentials.email,
          name: 'Admin User',
          role: 'Admin',
        }
        setUser(mockUser)
        localStorage.setItem('auth_token', 'mock_token')
      } else {
        throw new Error('Invalid credentials')
      }
    } catch (error) {
      console.error('Login failed:', error)
      throw error
    } finally {
      setIsLoading(false)
    }
  }

  const logout = () => {
    setUser(null)
    localStorage.removeItem('auth_token')
  }

  const hasRole = (role: string) => {
    if (!user) return false
    const roleHierarchy = { Viewer: 1, Operator: 2, Admin: 3 }
    return (
      roleHierarchy[user.role as keyof typeof roleHierarchy] >=
      roleHierarchy[role as keyof typeof roleHierarchy]
    )
  }

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user,
    isLoading,
    login,
    logout,
    hasRole,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export { AuthContext }
