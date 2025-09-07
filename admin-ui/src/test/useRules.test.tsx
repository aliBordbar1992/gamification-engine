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
import { rulesApi, entitiesApi } from '../api/rules'

// Mock the API
vi.mock('../api/rules', () => ({
  rulesApi: {
    getAllRules: vi.fn(),
    getActiveRules: vi.fn(),
    getRulesByTrigger: vi.fn(),
    getRuleById: vi.fn(),
    createRule: vi.fn(),
    updateRule: vi.fn(),
    activateRule: vi.fn(),
    deactivateRule: vi.fn(),
    deleteRule: vi.fn(),
  },
  entitiesApi: {
    getPointCategories: vi.fn(),
    getPointCategoryById: vi.fn(),
    getBadges: vi.fn(),
    getBadgeById: vi.fn(),
    getTrophies: vi.fn(),
    getTrophyById: vi.fn(),
    getLevels: vi.fn(),
    getLevelById: vi.fn(),
  },
}))

const mockRulesApi = rulesApi as any
const mockEntitiesApi = entitiesApi as any

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
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
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

      mockRulesApi.getAllRules.mockResolvedValue(mockRules)

      const { result } = renderHook(() => useRules(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockRules)
      expect(mockRulesApi.getAllRules).toHaveBeenCalled()
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

      mockRulesApi.getAllRules.mockResolvedValue(mockRules)

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

      mockRulesApi.getAllRules.mockResolvedValue(mockRules)

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

      mockRulesApi.getAllRules.mockResolvedValue(mockRules)

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

      mockRulesApi.getActiveRules.mockResolvedValue(mockRules)

      const { result } = renderHook(() => useActiveRules(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockRules)
      expect(mockRulesApi.getActiveRules).toHaveBeenCalled()
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

      mockRulesApi.getRulesByTrigger.mockResolvedValue(mockRules)

      const { result } = renderHook(() => useRulesByTrigger('USER_COMMENTED'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockRules)
      expect(mockRulesApi.getRulesByTrigger).toHaveBeenCalledWith('USER_COMMENTED')
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

      mockRulesApi.getRuleById.mockResolvedValue(mockRule)

      const { result } = renderHook(() => useRule('rule1'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockRule)
      expect(mockRulesApi.getRuleById).toHaveBeenCalledWith('rule1')
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

      mockRulesApi.createRule.mockResolvedValue(createdRule)

      const { result } = renderHook(() => useCreateRule(), {
        wrapper: createWrapper(),
      })

      await result.current.mutateAsync(newRule)

      expect(mockRulesApi.createRule).toHaveBeenCalledWith(newRule)
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

      mockRulesApi.updateRule.mockResolvedValue(updatedRule)

      const { result } = renderHook(() => useUpdateRule(), {
        wrapper: createWrapper(),
      })

      await result.current.mutateAsync({ id: 'rule1', rule: updateData })

      expect(mockRulesApi.updateRule).toHaveBeenCalledWith('rule1', updateData)
    })

    it('should activate a rule', async () => {
      mockRulesApi.activateRule.mockResolvedValue(undefined)

      const { result } = renderHook(() => useActivateRule(), {
        wrapper: createWrapper(),
      })

      await result.current.mutateAsync('rule1')

      expect(mockRulesApi.activateRule).toHaveBeenCalledWith('rule1')
    })

    it('should deactivate a rule', async () => {
      mockRulesApi.deactivateRule.mockResolvedValue(undefined)

      const { result } = renderHook(() => useDeactivateRule(), {
        wrapper: createWrapper(),
      })

      await result.current.mutateAsync('rule1')

      expect(mockRulesApi.deactivateRule).toHaveBeenCalledWith('rule1')
    })

    it('should delete a rule', async () => {
      mockRulesApi.deleteRule.mockResolvedValue(undefined)

      const { result } = renderHook(() => useDeleteRule(), {
        wrapper: createWrapper(),
      })

      await result.current.mutateAsync('rule1')

      expect(mockRulesApi.deleteRule).toHaveBeenCalledWith('rule1')
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

      mockEntitiesApi.getPointCategories.mockResolvedValue(mockCategories)

      const { result } = renderHook(() => usePointCategories(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockCategories)
      expect(mockEntitiesApi.getPointCategories).toHaveBeenCalled()
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

      mockEntitiesApi.getBadges.mockResolvedValue(mockBadges)

      const { result } = renderHook(() => useBadges(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockBadges)
      expect(mockEntitiesApi.getBadges).toHaveBeenCalled()
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

      mockEntitiesApi.getTrophies.mockResolvedValue(mockTrophies)

      const { result } = renderHook(() => useTrophies(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockTrophies)
      expect(mockEntitiesApi.getTrophies).toHaveBeenCalled()
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

      mockEntitiesApi.getLevels.mockResolvedValue(mockLevels)

      const { result } = renderHook(() => useLevels(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockLevels)
      expect(mockEntitiesApi.getLevels).toHaveBeenCalled()
    })
  })
})
