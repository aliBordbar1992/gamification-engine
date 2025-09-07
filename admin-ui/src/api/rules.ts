import apiClient from './client'
import type {
  Rule,
  CreateRule,
  UpdateRule,
  Badge,
  Trophy,
  PointCategory,
  Level,
  RulesListParams,
} from '@/types'
import {
  mockRules,
  mockPointCategories,
  mockBadges,
  mockTrophies,
  mockLevels,
} from '../test/mockRules'

// Rules API
export const rulesApi = {
  // Get all rules
  getAllRules: async (): Promise<Rule[]> => {
    try {
      const response = await apiClient.get('/rules')
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      return mockRules
    }
  },

  // Get active rules only
  getActiveRules: async (): Promise<Rule[]> => {
    const response = await apiClient.get('/rules/active')
    return response.data
  },

  // Get rules by trigger event type
  getRulesByTrigger: async (eventType: string): Promise<Rule[]> => {
    const response = await apiClient.get(`/rules/trigger/${eventType}`)
    return response.data
  },

  // Get specific rule by ID
  getRuleById: async (id: string): Promise<Rule> => {
    try {
      const response = await apiClient.get(`/rules/${id}`)
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      const mockRule = mockRules.find((rule) => rule.id === id)
      if (!mockRule) {
        throw new Error(`Rule with id ${id} not found`)
      }
      return mockRule
    }
  },

  // Create a new rule
  createRule: async (rule: CreateRule): Promise<Rule> => {
    const response = await apiClient.post('/rules', rule)
    return response.data
  },

  // Update an existing rule
  updateRule: async (id: string, rule: UpdateRule): Promise<Rule> => {
    const response = await apiClient.put(`/rules/${id}`, rule)
    return response.data
  },

  // Activate a rule
  activateRule: async (id: string): Promise<void> => {
    await apiClient.post(`/rules/${id}/activate`)
  },

  // Deactivate a rule
  deactivateRule: async (id: string): Promise<void> => {
    await apiClient.post(`/rules/${id}/deactivate`)
  },

  // Delete a rule
  deleteRule: async (id: string): Promise<void> => {
    await apiClient.delete(`/rules/${id}`)
  },
}

// Entity APIs
export const entitiesApi = {
  // Point Categories
  getPointCategories: async (): Promise<PointCategory[]> => {
    const response = await apiClient.get('/rules/entities/point-categories')
    return response.data
  },

  getPointCategoryById: async (id: string): Promise<PointCategory> => {
    const response = await apiClient.get(
      `/rules/entities/point-categories/${id}`
    )
    return response.data
  },

  // Badges
  getBadges: async (): Promise<Badge[]> => {
    const response = await apiClient.get('/rules/entities/badges')
    return response.data
  },

  getBadgeById: async (id: string): Promise<Badge> => {
    const response = await apiClient.get(`/rules/entities/badges/${id}`)
    return response.data
  },

  // Trophies
  getTrophies: async (): Promise<Trophy[]> => {
    const response = await apiClient.get('/rules/entities/trophies')
    return response.data
  },

  getTrophyById: async (id: string): Promise<Trophy> => {
    const response = await apiClient.get(`/rules/entities/trophies/${id}`)
    return response.data
  },

  // Levels
  getLevels: async (): Promise<Level[]> => {
    const response = await apiClient.get('/rules/entities/levels')
    return response.data
  },

  getLevelById: async (id: string): Promise<Level> => {
    const response = await apiClient.get(`/rules/entities/levels/${id}`)
    return response.data
  },
}
