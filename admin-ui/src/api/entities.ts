import apiClient from './client'
import type {
  CreateBadgeDto,
  CreateTrophyDto,
  CreatePointCategoryDto,
  CreateLevelDto,
} from './generated/models'
import {
  mockPointCategories,
  mockBadges,
  mockTrophies,
  mockLevels,
} from '../test/mockRules'

// Type aliases for better readability
type Badge = CreateBadgeDto
type Trophy = CreateTrophyDto
type PointCategory = CreatePointCategoryDto
type Level = CreateLevelDto

// Badges API
export const badgesApi = {
  // Get all badges
  getAllBadges: async (): Promise<Badge[]> => {
    try {
      const response = await apiClient.get('/api/Badges')
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      return mockBadges.map((badge) => ({
        ...badge,
        image: '/images/badges/default.png',
        visible: true,
      }))
    }
  },

  // Get visible badges only
  getVisibleBadges: async (): Promise<Badge[]> => {
    try {
      const response = await apiClient.get('/api/Badges/visible')
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      return mockBadges.map((badge) => ({
        ...badge,
        image: '/images/badges/default.png',
        visible: true,
      }))
    }
  },

  // Get specific badge by ID
  getBadgeById: async (id: string): Promise<Badge> => {
    try {
      const response = await apiClient.get(`/api/Badges/${id}`)
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      const mockBadge = mockBadges.find((badge) => badge.id === id)
      if (!mockBadge) {
        throw new Error(`Badge with id ${id} not found`)
      }
      return {
        ...mockBadge,
        image: '/images/badges/default.png',
        visible: true,
      }
    }
  },
}

// Trophies API
export const trophiesApi = {
  // Get all trophies
  getAllTrophies: async (): Promise<Trophy[]> => {
    try {
      const response = await apiClient.get('/api/Trophies')
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      return mockTrophies.map((trophy) => ({
        ...trophy,
        image: '/images/trophies/default.png',
        visible: true,
      }))
    }
  },

  // Get visible trophies only
  getVisibleTrophies: async (): Promise<Trophy[]> => {
    try {
      const response = await apiClient.get('/api/Trophies/visible')
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      return mockTrophies.map((trophy) => ({
        ...trophy,
        image: '/images/trophies/default.png',
        visible: true,
      }))
    }
  },

  // Get specific trophy by ID
  getTrophyById: async (id: string): Promise<Trophy> => {
    try {
      const response = await apiClient.get(`/api/Trophies/${id}`)
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      const mockTrophy = mockTrophies.find((trophy) => trophy.id === id)
      if (!mockTrophy) {
        throw new Error(`Trophy with id ${id} not found`)
      }
      return {
        ...mockTrophy,
        image: '/images/trophies/default.png',
        visible: true,
      }
    }
  },
}

// Levels API
export const levelsApi = {
  // Get all levels
  getAllLevels: async (): Promise<Level[]> => {
    try {
      const response = await apiClient.get('/api/Levels')
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      return mockLevels.map((level) => ({
        ...level,
        category: 'xp',
      }))
    }
  },

  // Get levels by category
  getLevelsByCategory: async (category: string): Promise<Level[]> => {
    try {
      const response = await apiClient.get(`/api/Levels/category/${category}`)
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      return mockLevels
        .filter((level) => level.category === category)
        .map((level) => ({
          ...level,
          category,
        }))
    }
  },

  // Get specific level by ID
  getLevelById: async (id: string): Promise<Level> => {
    try {
      const response = await apiClient.get(`/api/Levels/${id}`)
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      const mockLevel = mockLevels.find((level) => level.id === id)
      if (!mockLevel) {
        throw new Error(`Level with id ${id} not found`)
      }
      return {
        ...mockLevel,
        category: 'xp',
      }
    }
  },
}

// Point Categories API
export const pointCategoriesApi = {
  // Get all point categories
  getAllPointCategories: async (): Promise<PointCategory[]> => {
    try {
      const response = await apiClient.get('/api/PointCategories')
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      return mockPointCategories.map((category) => ({
        ...category,
        aggregation: 'sum',
      }))
    }
  },

  // Get specific point category by ID
  getPointCategoryById: async (id: string): Promise<PointCategory> => {
    try {
      const response = await apiClient.get(`/api/PointCategories/${id}`)
      return response.data
    } catch (error) {
      console.warn('API not available, using mock data:', error)
      const mockCategory = mockPointCategories.find(
        (category) => category.id === id
      )
      if (!mockCategory) {
        throw new Error(`Point category with id ${id} not found`)
      }
      return {
        ...mockCategory,
        aggregation: 'sum',
      }
    }
  },
}
