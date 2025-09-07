import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactNode } from 'react'
import {
  useRules,
  useActiveRules,
  useRulesByTrigger,
  useRule,
  useCreateRule,
  useUpdateRule,
  useActivateRule,
  useDeactivateRule,
  useDeleteRule,
  usePointCategories,
  useBadges,
  useTrophies,
  useLevels,
} from '../hooks/useRules'

// Mock the generated API
vi.mock('../api/generated-client', () => ({
  RulesApi: vi.fn(),
  createApiInstance: vi.fn(() => ({
    apiRulesGet: vi.fn(),
    apiRulesActiveGet: vi.fn(),
    apiRulesTriggerEventTypeGet: vi.fn(),
    apiRulesIdGet: vi.fn(),
    apiRulesPost: vi.fn(),
    apiRulesIdPut: vi.fn(),
    apiRulesIdActivatePost: vi.fn(),
    apiRulesIdDeactivatePost: vi.fn(),
    apiRulesIdDelete: vi.fn(),
    apiRulesEntitiesPointCategoriesGet: vi.fn(),
    apiRulesEntitiesPointCategoriesIdGet: vi.fn(),
    apiRulesEntitiesBadgesGet: vi.fn(),
    apiRulesEntitiesBadgesIdGet: vi.fn(),
    apiRulesEntitiesTrophiesGet: vi.fn(),
    apiRulesEntitiesTrophiesIdGet: vi.fn(),
    apiRulesEntitiesLevelsGet: vi.fn(),
    apiRulesEntitiesLevelsIdGet: vi.fn(),
  })),
}))

// Create mock API instance
const mockApiInstance = {
  apiRulesGet: vi.fn(),
  apiRulesActiveGet: vi.fn(),
  apiRulesTriggerEventTypeGet: vi.fn(),
  apiRulesIdGet: vi.fn(),
  apiRulesPost: vi.fn(),
  apiRulesIdPut: vi.fn(),
  apiRulesIdActivatePost: vi.fn(),
  apiRulesIdDeactivatePost: vi.fn(),
  apiRulesIdDelete: vi.fn(),
  apiRulesEntitiesPointCategoriesGet: vi.fn(),
  apiRulesEntitiesPointCategoriesIdGet: vi.fn(),
  apiRulesEntitiesBadgesGet: vi.fn(),
  apiRulesEntitiesBadgesIdGet: vi.fn(),
  apiRulesEntitiesTrophiesGet: vi.fn(),
  apiRulesEntitiesTrophiesIdGet: vi.fn(),
  apiRulesEntitiesLevelsGet: vi.fn(),
  apiRulesEntitiesLevelsIdGet: vi.fn(),
}

// Test wrapper with QueryClient
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  })

  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  )
}

describe('useRules hooks', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('Query hooks', () => {
    it('should fetch all rules', async () => {
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

      mockApiInstance.apiRulesGet.mockResolvedValue(mockRules)

      const { result } = renderHook(() => useRules(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockRules)
      expect(mockApiInstance.apiRulesGet).toHaveBeenCalled()
    })

    it('should filter rules by active status', async () => {
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
        {
          id: 'rule2',
          name: 'Inactive Rule',
          description: 'Inactive Description',
          isActive: false,
          triggers: ['USER_COMMENTED'],
          conditions: [],
          rewards: [],
          createdAt: '2023-01-01T00:00:00Z',
        },
      ]

      mockApiInstance.apiRulesGet.mockResolvedValue(mockRules)

      const { result } = renderHook(
        () => useRules({ filters: { isActive: true } }),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual([mockRules[0]])
    })

    it('should filter rules by trigger type', async () => {
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
        {
          id: 'rule2',
          name: 'Like Rule',
          description: 'Like Description',
          isActive: true,
          triggers: ['USER_LIKED'],
          conditions: [],
          rewards: [],
          createdAt: '2023-01-01T00:00:00Z',
        },
      ]

      mockApiInstance.apiRulesGet.mockResolvedValue(mockRules)

      const { result } = renderHook(
        () => useRules({ filters: { triggerType: 'USER_COMMENTED' } }),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual([mockRules[0]])
    })

    it('should search rules by text', async () => {
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
        {
          id: 'rule2',
          name: 'Like Rule',
          description: 'Like Description',
          isActive: true,
          triggers: ['USER_LIKED'],
          conditions: [],
          rewards: [],
          createdAt: '2023-01-01T00:00:00Z',
        },
      ]

      mockApiInstance.apiRulesGet.mockResolvedValue(mockRules)

      const { result } = renderHook(
        () => useRules({ filters: { search: 'comment' } }),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual([mockRules[0]])
    })

    it('should fetch active rules', async () => {
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

      mockApiInstance.apiRulesActiveGet.mockResolvedValue(mockRules)

      const { result } = renderHook(() => useActiveRules(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockRules)
      expect(mockApiInstance.apiRulesActiveGet).toHaveBeenCalled()
    })

    it('should fetch rules by trigger', async () => {
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

      mockApiInstance.apiRulesTriggerEventTypeGet.mockResolvedValue(mockRules)

      const { result } = renderHook(() => useRulesByTrigger('USER_COMMENTED'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockRules)
      expect(mockApiInstance.apiRulesTriggerEventTypeGet).toHaveBeenCalledWith(
        'USER_COMMENTED'
      )
    })

    it('should fetch single rule', async () => {
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

      mockApiInstance.apiRulesIdGet.mockResolvedValue(mockRule)

      const { result } = renderHook(() => useRule('rule1'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockRule)
      expect(mockApiInstance.apiRulesIdGet).toHaveBeenCalledWith('rule1')
    })
  })

  describe('Mutation hooks', () => {
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

      mockApiInstance.apiRulesPost.mockResolvedValue({ data: createdRule })

      const { result } = renderHook(() => useCreateRule(), {
        wrapper: createWrapper(),
      })

      await result.current.mutateAsync(newRule)

      expect(mockApiInstance.apiRulesPost).toHaveBeenCalledWith(newRule)
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

      mockApiInstance.apiRulesIdPut.mockResolvedValue({ data: updatedRule })

      const { result } = renderHook(() => useUpdateRule(), {
        wrapper: createWrapper(),
      })

      await result.current.mutateAsync({ id: 'rule1', rule: updateData })

      expect(mockApiInstance.apiRulesIdPut).toHaveBeenCalledWith(
        'rule1',
        updateData
      )
    })

    it('should activate a rule', async () => {
      mockApiInstance.apiRulesIdActivatePost.mockResolvedValue(undefined)

      const { result } = renderHook(() => useActivateRule(), {
        wrapper: createWrapper(),
      })

      await result.current.mutateAsync('rule1')

      expect(mockApiInstance.apiRulesIdActivatePost).toHaveBeenCalledWith(
        'rule1'
      )
    })

    it('should deactivate a rule', async () => {
      mockApiInstance.apiRulesIdDeactivatePost.mockResolvedValue(undefined)

      const { result } = renderHook(() => useDeactivateRule(), {
        wrapper: createWrapper(),
      })

      await result.current.mutateAsync('rule1')

      expect(mockApiInstance.apiRulesIdDeactivatePost).toHaveBeenCalledWith(
        'rule1'
      )
    })

    it('should delete a rule', async () => {
      mockApiInstance.apiRulesIdDelete.mockResolvedValue(undefined)

      const { result } = renderHook(() => useDeleteRule(), {
        wrapper: createWrapper(),
      })

      await result.current.mutateAsync('rule1')

      expect(mockApiInstance.apiRulesIdDelete).toHaveBeenCalledWith('rule1')
    })
  })

  describe('Entity hooks', () => {
    it('should fetch point categories', async () => {
      const mockCategories = [
        {
          id: 'xp',
          name: 'Experience Points',
          description: 'Points earned through activities',
          aggregation: 'sum',
        },
      ]

      mockApiInstance.apiRulesEntitiesPointCategoriesGet.mockResolvedValue(
        mockCategories
      )

      const { result } = renderHook(() => usePointCategories(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockCategories)
      expect(
        mockApiInstance.apiRulesEntitiesPointCategoriesGet
      ).toHaveBeenCalled()
    })

    it('should fetch badges', async () => {
      const mockBadges = [
        {
          id: 'first-comment',
          name: 'First Comment',
          description: 'Awarded for first comment',
          image: 'badge.png',
          visible: true,
        },
      ]

      mockApiInstance.apiRulesEntitiesBadgesGet.mockResolvedValue(mockBadges)

      const { result } = renderHook(() => useBadges(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockBadges)
      expect(mockApiInstance.apiRulesEntitiesBadgesGet).toHaveBeenCalled()
    })

    it('should fetch trophies', async () => {
      const mockTrophies = [
        {
          id: 'comment-master',
          name: 'Comment Master',
          description: 'Awarded for 100 comments',
          image: 'trophy.png',
          visible: true,
        },
      ]

      mockApiInstance.apiRulesEntitiesTrophiesGet.mockResolvedValue(
        mockTrophies
      )

      const { result } = renderHook(() => useTrophies(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockTrophies)
      expect(mockApiInstance.apiRulesEntitiesTrophiesGet).toHaveBeenCalled()
    })

    it('should fetch levels', async () => {
      const mockLevels = [
        {
          id: 'level-1',
          name: 'Beginner',
          category: 'xp',
          minPoints: 0,
        },
      ]

      mockApiInstance.apiRulesEntitiesLevelsGet.mockResolvedValue(mockLevels)

      const { result } = renderHook(() => useLevels(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockLevels)
      expect(mockApiInstance.apiRulesEntitiesLevelsGet).toHaveBeenCalled()
    })
  })
})
