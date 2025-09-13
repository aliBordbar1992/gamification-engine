// Integration layer for generated API client
// This file bridges the generated API with your existing axios configuration

import { Configuration } from './generated/configuration'
import apiClient from './client' // Your existing axios instance
import type {
  BadgesApiInterface,
  LevelsApiInterface,
  RulesApiInterface,
  TrophiesApiInterface,
  PointCategoriesApiInterface,
  UsersApiInterface,
  LeaderboardsApiInterface,
  EventsApiInterface,
  EventDefinitionApiInterface,
  WalletApiInterface,
} from './generated/api'
import { BadgesApi } from './generated/apis/badges-api'
import { TrophiesApi } from './generated/apis/trophies-api'
import { LevelsApi } from './generated/apis/levels-api'
import { RulesApi } from './generated/apis/rules-api'
import { PointCategoriesApi } from './generated/apis/point-categories-api'
import { UsersApi } from './generated/apis/users-api'
import { LeaderboardsApi } from './generated/apis/leaderboards-api'
import { EventsApi } from './generated/apis/events-api'
import { EventDefinitionApi } from './generated/apis/event-definition-api'
import { WalletApi } from './generated/apis/wallet-api'
import type { AxiosInstance } from 'axios'

// Create configuration for the generated API client
export const apiConfiguration = new Configuration({
  basePath: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5046',
  accessToken: () => {
    const token = localStorage.getItem('auth_token')
    return token ? `Bearer ${token}` : ''
  },
})

// Export configured API instances

// Export all models/DTOs
export * from '@/api/generated/models'

// Helper function to create API instances with your axios client
export function createApiInstance<
  T extends new (
    configuration?: Configuration,
    basePath?: string,
    axios?: AxiosInstance
  ) => unknown
>(ApiClass: T): InstanceType<T> {
  return new ApiClass(apiConfiguration, undefined, apiClient) as InstanceType<T>
}

export function RulesApiInstance(): RulesApiInterface {
  return createApiInstance(RulesApi)
}

export function BadgesApiInstance(): BadgesApiInterface {
  return createApiInstance(BadgesApi)
}

export function TrophiesApiInstance(): TrophiesApiInterface {
  return createApiInstance(TrophiesApi)
}

export function LevelsApiInstance(): LevelsApiInterface {
  return createApiInstance(LevelsApi)
}

export function PointCategoriesApiInstance(): PointCategoriesApiInterface {
  return createApiInstance(PointCategoriesApi)
}

export function UsersApiInstance(): UsersApiInterface {
  return createApiInstance(UsersApi)
}

export function LeaderboardsApiInstance(): LeaderboardsApiInterface {
  return createApiInstance(LeaderboardsApi)
}

export function EventsApiInstance(): EventsApiInterface {
  return createApiInstance(EventsApi)
}

export function EventDefinitionApiInstance(): EventDefinitionApiInterface {
  return createApiInstance(EventDefinitionApi)
}

export function WalletApiInstance(): WalletApiInterface {
  return createApiInstance(WalletApi)
}
