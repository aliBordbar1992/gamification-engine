import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactNode } from 'react'
import {
  useUserState,
  useUserPoints,
  useUserPointsByCategory,
  useUserBadges,
  useUserTrophies,
  useUserLevels,
  useUserLevelByCategory,
  useUserRewardHistory,
} from '../hooks/useUsers'

// Mock the generated API
vi.mock('../api/generated-client', () => ({
  UsersApi: vi.fn(),
  createApiInstance: vi.fn(() => mockApiInstance),
  UsersApiInstance: vi.fn(() => mockApiInstance),
}))

// Create mock API instance
const mockApiInstance = {
  apiUsersUserIdStateGet: vi.fn(),
  apiUsersUserIdPointsGet: vi.fn(),
  apiUsersUserIdPointsCategoryGet: vi.fn(),
  apiUsersUserIdBadgesGet: vi.fn(),
  apiUsersUserIdTrophiesGet: vi.fn(),
  apiUsersUserIdLevelsGet: vi.fn(),
  apiUsersUserIdLevelsCategoryGet: vi.fn(),
  apiUsersUserIdRewardsHistoryGet: vi.fn(),
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

describe('useUsers hooks', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('Query hooks', () => {
    it('should fetch user state', async () => {
      const mockUserState = {
        userId: 'user123',
        pointsByCategory: {
          xp: 1500,
          coins: 250,
        },
        badges: [
          {
            id: 'first-comment',
            name: 'First Comment',
            description: 'Awarded for first comment',
            image: 'badge.png',
            visible: true,
          },
        ],
        trophies: [
          {
            id: 'comment-master',
            name: 'Comment Master',
            description: 'Awarded for 100 comments',
            image: 'trophy.png',
            visible: true,
          },
        ],
        currentLevelsByCategory: {
          xp: {
            id: 'level-2',
            name: 'Intermediate',
            category: 'xp',
            level: 2,
            minPoints: 1000,
            description: 'Intermediate level',
          },
        },
      }

      mockApiInstance.apiUsersUserIdStateGet.mockResolvedValue({
        data: mockUserState,
      })

      const { result } = renderHook(() => useUserState('user123'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockUserState)
      expect(mockApiInstance.apiUsersUserIdStateGet).toHaveBeenCalledWith(
        'user123'
      )
    })

    it('should fetch user points', async () => {
      const mockPoints = {
        xp: 1500,
        coins: 250,
        reputation: 100,
      }

      mockApiInstance.apiUsersUserIdPointsGet.mockResolvedValue({
        data: mockPoints,
      })

      const { result } = renderHook(() => useUserPoints('user123'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockPoints)
      expect(mockApiInstance.apiUsersUserIdPointsGet).toHaveBeenCalledWith(
        'user123'
      )
    })

    it('should fetch user points by category', async () => {
      const mockPoints = 1500

      mockApiInstance.apiUsersUserIdPointsCategoryGet.mockResolvedValue({
        data: mockPoints,
      })

      const { result } = renderHook(
        () => useUserPointsByCategory('user123', 'xp'),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockPoints)
      expect(
        mockApiInstance.apiUsersUserIdPointsCategoryGet
      ).toHaveBeenCalledWith('user123', 'xp')
    })

    it('should fetch user badges', async () => {
      const mockBadges = [
        {
          id: 'first-comment',
          name: 'First Comment',
          description: 'Awarded for first comment',
          image: 'badge.png',
          visible: true,
        },
        {
          id: 'social-butterfly',
          name: 'Social Butterfly',
          description: 'Awarded for 10 social interactions',
          image: 'social-badge.png',
          visible: true,
        },
      ]

      mockApiInstance.apiUsersUserIdBadgesGet.mockResolvedValue({
        data: mockBadges,
      })

      const { result } = renderHook(() => useUserBadges('user123'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockBadges)
      expect(mockApiInstance.apiUsersUserIdBadgesGet).toHaveBeenCalledWith(
        'user123'
      )
    })

    it('should fetch user trophies', async () => {
      const mockTrophies = [
        {
          id: 'comment-master',
          name: 'Comment Master',
          description: 'Awarded for 100 comments',
          image: 'trophy.png',
          visible: true,
        },
      ]

      mockApiInstance.apiUsersUserIdTrophiesGet.mockResolvedValue({
        data: mockTrophies,
      })

      const { result } = renderHook(() => useUserTrophies('user123'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockTrophies)
      expect(mockApiInstance.apiUsersUserIdTrophiesGet).toHaveBeenCalledWith(
        'user123'
      )
    })

    it('should fetch user levels', async () => {
      const mockLevels = {
        xp: {
          id: 'level-2',
          name: 'Intermediate',
          category: 'xp',
          level: 2,
          minPoints: 1000,
          description: 'Intermediate level',
        },
        coins: {
          id: 'level-1',
          name: 'Beginner',
          category: 'coins',
          level: 1,
          minPoints: 0,
          description: 'Beginner level',
        },
      }

      mockApiInstance.apiUsersUserIdLevelsGet.mockResolvedValue({
        data: mockLevels,
      })

      const { result } = renderHook(() => useUserLevels('user123'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockLevels)
      expect(mockApiInstance.apiUsersUserIdLevelsGet).toHaveBeenCalledWith(
        'user123'
      )
    })

    it('should fetch user level by category', async () => {
      const mockLevel = {
        id: 'level-2',
        name: 'Intermediate',
        category: 'xp',
        level: 2,
        minPoints: 1000,
        description: 'Intermediate level',
      }

      mockApiInstance.apiUsersUserIdLevelsCategoryGet.mockResolvedValue({
        data: mockLevel,
      })

      const { result } = renderHook(
        () => useUserLevelByCategory('user123', 'xp'),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockLevel)
      expect(
        mockApiInstance.apiUsersUserIdLevelsCategoryGet
      ).toHaveBeenCalledWith('user123', 'xp')
    })

    it('should fetch user reward history', async () => {
      const mockRewardHistory = {
        entries: [
          {
            id: 'reward1',
            userId: 'user123',
            rewardType: 'points',
            targetId: 'xp',
            amount: 100,
            reason: 'User commented',
            createdAt: '2023-01-01T00:00:00Z',
          },
        ],
        totalCount: 1,
        page: 1,
        pageSize: 20,
        totalPages: 1,
      }

      mockApiInstance.apiUsersUserIdRewardsHistoryGet.mockResolvedValue({
        data: mockRewardHistory,
      })

      const { result } = renderHook(
        () => useUserRewardHistory('user123', 1, 20),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockRewardHistory)
      expect(
        mockApiInstance.apiUsersUserIdRewardsHistoryGet
      ).toHaveBeenCalledWith('user123', 1, 20)
    })

    it('should not fetch when userId is empty', async () => {
      const { result } = renderHook(() => useUserState(''), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false)
      })

      expect(result.current.data).toBeUndefined()
      expect(mockApiInstance.apiUsersUserIdStateGet).not.toHaveBeenCalled()
    })

    it('should not fetch when category is empty', async () => {
      const { result } = renderHook(
        () => useUserPointsByCategory('user123', ''),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false)
      })

      expect(result.current.data).toBeUndefined()
      expect(
        mockApiInstance.apiUsersUserIdPointsCategoryGet
      ).not.toHaveBeenCalled()
    })
  })
})
