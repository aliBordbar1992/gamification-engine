// Integration layer for generated API client
// This file bridges the generated API with your existing axios configuration

import { Configuration } from './generated/configuration'
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
export { RulesApi } from './generated/apis/rules-api'
export { BadgesApi } from './generated/apis/badges-api'
export { TrophiesApi } from './generated/apis/trophies-api'
export { LevelsApi } from './generated/apis/levels-api'
export { PointCategoriesApi } from './generated/apis/point-categories-api'
export { UsersApi } from './generated/apis/users-api'
export { EventsApi } from './generated/apis/events-api'
export { LeaderboardsApi } from './generated/apis/leaderboards-api'
export { WebhooksApi } from './generated/apis/webhooks-api'

// Export all models/DTOs
export * from '@/api/generated/models'

// Helper function to create API instances with your axios client
export function createApiInstance<
  T extends new (configuration?: any, basePath?: string, axios?: any) => any
>(ApiClass: T): InstanceType<T> {
  return new ApiClass(apiConfiguration, undefined, apiClient) as InstanceType<T>
}
