// Integration layer for generated API client
// This file bridges the generated API with your existing axios configuration

import { Configuration } from './generated'
import apiClient from './client' // Your existing axios instance

// Create configuration for the generated API client
export const apiConfiguration = new Configuration({
  basePath: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5046',
  accessToken: () => {
    const token = localStorage.getItem('auth_token')
    return token ? `Bearer ${token}` : ''
  },
})

// Export configured API instances
export {
  RulesApi,
  BadgesApi,
  TrophiesApi,
  LevelsApi,
  PointCategoriesApi,
  UsersApi,
  EventsApi,
  LeaderboardsApi,
  WebhooksApi,
} from './generated/apis'

// Export all models/DTOs
export * from './generated/models'

// Helper function to create API instances with your axios client
export function createApiInstance<
  T extends new (configuration?: any, basePath?: string, axios?: any) => any
>(ApiClass: T): InstanceType<T> {
  return new ApiClass(apiConfiguration, undefined, apiClient) as InstanceType<T>
}
