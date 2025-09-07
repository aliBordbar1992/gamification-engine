import { describe, it, expect, vi, beforeEach } from 'vitest'
import { rulesApi, entitiesApi } from '../api/rules'
import apiClient from '../api/client'

// Mock the API client
vi.mock('../api/client', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}))

const mockApiClient = apiClient as any

describe('Rules API', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('rulesApi', () => {
    it('should get all rules', async () => {
      const mockRules = [
        {
          id: 'rule1',
          name: 'Test Rule',
          description: 'Test Description',
          isActive: true,
          triggers: ['USER_COMMENTED'],
          conditions: [],
          rewards: [],
          createdAt: '2023-01-01T00:00:00Z',
        },
      ]

      mockApiClient.get.mockResolvedValue({ data: mockRules })

      const result = await rulesApi.getAllRules()

      expect(mockApiClient.get).toHaveBeenCalledWith('/rules')
      expect(result).toEqual(mockRules)
    })

    it('should get active rules', async () => {
      const mockRules = [
        {
          id: 'rule1',
          name: 'Active Rule',
          description: 'Active Description',
          isActive: true,
          triggers: ['USER_COMMENTED'],
          conditions: [],
          rewards: [],
          createdAt: '2023-01-01T00:00:00Z',
        },
      ]

      mockApiClient.get.mockResolvedValue({ data: mockRules })

      const result = await rulesApi.getActiveRules()

      expect(mockApiClient.get).toHaveBeenCalledWith('/rules/active')
      expect(result).toEqual(mockRules)
    })

    it('should get rules by trigger', async () => {
      const mockRules = [
        {
          id: 'rule1',
          name: 'Comment Rule',
          description: 'Comment Description',
          isActive: true,
          triggers: ['USER_COMMENTED'],
          conditions: [],
          rewards: [],
          createdAt: '2023-01-01T00:00:00Z',
        },
      ]

      mockApiClient.get.mockResolvedValue({ data: mockRules })

      const result = await rulesApi.getRulesByTrigger('USER_COMMENTED')

      expect(mockApiClient.get).toHaveBeenCalledWith('/rules/trigger/USER_COMMENTED')
      expect(result).toEqual(mockRules)
    })

    it('should get rule by ID', async () => {
      const mockRule = {
        id: 'rule1',
        name: 'Test Rule',
        description: 'Test Description',
        isActive: true,
        triggers: ['USER_COMMENTED'],
        conditions: [],
        rewards: [],
        createdAt: '2023-01-01T00:00:00Z',
      }

      mockApiClient.get.mockResolvedValue({ data: mockRule })

      const result = await rulesApi.getRuleById('rule1')

      expect(mockApiClient.get).toHaveBeenCalledWith('/rules/rule1')
      expect(result).toEqual(mockRule)
    })

    it('should create a rule', async () => {
      const newRule = {
        id: 'rule1',
        name: 'New Rule',
        description: 'New Description',
        isActive: true,
        triggers: ['USER_COMMENTED'],
        conditions: [],
        rewards: [],
      }

      const createdRule = {
        ...newRule,
        createdAt: '2023-01-01T00:00:00Z',
      }

      mockApiClient.post.mockResolvedValue({ data: createdRule })

      const result = await rulesApi.createRule(newRule)

      expect(mockApiClient.post).toHaveBeenCalledWith('/rules', newRule)
      expect(result).toEqual(createdRule)
    })

    it('should update a rule', async () => {
      const updateData = {
        name: 'Updated Rule',
        description: 'Updated Description',
        isActive: false,
        triggers: ['USER_COMMENTED'],
        conditions: [],
        rewards: [],
      }

      const updatedRule = {
        id: 'rule1',
        ...updateData,
        createdAt: '2023-01-01T00:00:00Z',
        updatedAt: '2023-01-02T00:00:00Z',
      }

      mockApiClient.put.mockResolvedValue({ data: updatedRule })

      const result = await rulesApi.updateRule('rule1', updateData)

      expect(mockApiClient.put).toHaveBeenCalledWith('/rules/rule1', updateData)
      expect(result).toEqual(updatedRule)
    })

    it('should activate a rule', async () => {
      mockApiClient.post.mockResolvedValue({})

      await rulesApi.activateRule('rule1')

      expect(mockApiClient.post).toHaveBeenCalledWith('/rules/rule1/activate')
    })

    it('should deactivate a rule', async () => {
      mockApiClient.post.mockResolvedValue({})

      await rulesApi.deactivateRule('rule1')

      expect(mockApiClient.post).toHaveBeenCalledWith('/rules/rule1/deactivate')
    })

    it('should delete a rule', async () => {
      mockApiClient.delete.mockResolvedValue({})

      await rulesApi.deleteRule('rule1')

      expect(mockApiClient.delete).toHaveBeenCalledWith('/rules/rule1')
    })
  })

  describe('entitiesApi', () => {
    it('should get point categories', async () => {
      const mockCategories = [
        {
          id: 'xp',
          name: 'Experience Points',
          description: 'Points earned through activities',
          aggregation: 'sum',
        },
      ]

      mockApiClient.get.mockResolvedValue({ data: mockCategories })

      const result = await entitiesApi.getPointCategories()

      expect(mockApiClient.get).toHaveBeenCalledWith('/rules/entities/point-categories')
      expect(result).toEqual(mockCategories)
    })

    it('should get badges', async () => {
      const mockBadges = [
        {
          id: 'first-comment',
          name: 'First Comment',
          description: 'Awarded for first comment',
          image: 'badge.png',
          visible: true,
        },
      ]

      mockApiClient.get.mockResolvedValue({ data: mockBadges })

      const result = await entitiesApi.getBadges()

      expect(mockApiClient.get).toHaveBeenCalledWith('/rules/entities/badges')
      expect(result).toEqual(mockBadges)
    })

    it('should get trophies', async () => {
      const mockTrophies = [
        {
          id: 'comment-master',
          name: 'Comment Master',
          description: 'Awarded for 100 comments',
          image: 'trophy.png',
          visible: true,
        },
      ]

      mockApiClient.get.mockResolvedValue({ data: mockTrophies })

      const result = await entitiesApi.getTrophies()

      expect(mockApiClient.get).toHaveBeenCalledWith('/rules/entities/trophies')
      expect(result).toEqual(mockTrophies)
    })

    it('should get levels', async () => {
      const mockLevels = [
        {
          id: 'level-1',
          name: 'Beginner',
          category: 'xp',
          minPoints: 0,
        },
      ]

      mockApiClient.get.mockResolvedValue({ data: mockLevels })

      const result = await entitiesApi.getLevels()

      expect(mockApiClient.get).toHaveBeenCalledWith('/rules/entities/levels')
      expect(result).toEqual(mockLevels)
    })
  })
})
