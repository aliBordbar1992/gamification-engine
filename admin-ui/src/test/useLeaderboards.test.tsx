import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactNode } from 'react'
import {
  useLeaderboard,
  usePointsLeaderboard,
  useBadgesLeaderboard,
  useTrophiesLeaderboard,
  useLevelsLeaderboard,
  useUserRank,
  useRefreshLeaderboard,
} from '../hooks/useLeaderboards'

// Mock the generated API
vi.mock('@/api/generated-client', () => ({
  LeaderboardsApiInstance: vi.fn(() => ({
    apiLeaderboardsGet: vi.fn(),
    apiLeaderboardsPointsCategoryGet: vi.fn(),
    apiLeaderboardsBadgesGet: vi.fn(),
    apiLeaderboardsTrophiesGet: vi.fn(),
    apiLeaderboardsLevelsCategoryGet: vi.fn(),
    apiLeaderboardsUserUserIdRankGet: vi.fn(),
    apiLeaderboardsRefreshPost: vi.fn(),
  })),
}))

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

describe('useLeaderboards hooks', () => {
  let mockApiInstance: any

  beforeEach(() => {
    vi.clearAllMocks()
    mockApiInstance = {
      apiLeaderboardsGet: vi.fn(),
      apiLeaderboardsPointsCategoryGet: vi.fn(),
      apiLeaderboardsBadgesGet: vi.fn(),
      apiLeaderboardsTrophiesGet: vi.fn(),
      apiLeaderboardsLevelsCategoryGet: vi.fn(),
      apiLeaderboardsUserUserIdRankGet: vi.fn(),
      apiLeaderboardsRefreshPost: vi.fn(),
    }

    const { LeaderboardsApiInstance } = vi.mocked(
      require('@/api/generated-client')
    )
    LeaderboardsApiInstance.mockReturnValue(mockApiInstance)
  })

  describe('Query hooks', () => {
    it('should fetch general leaderboard', async () => {
      const mockLeaderboard = {
        query: {
          type: 'points',
          category: 'xp',
          timeRange: 'alltime',
          page: 1,
          pageSize: 50,
        },
        entries: [
          {
            userId: 'user1',
            points: 1500,
            rank: 1,
            displayName: 'Top Player',
          },
          {
            userId: 'user2',
            points: 1200,
            rank: 2,
            displayName: 'Second Player',
          },
        ],
        totalCount: 2,
        totalPages: 1,
        currentPage: 1,
        pageSize: 50,
        hasNextPage: false,
        hasPreviousPage: false,
        topEntry: {
          userId: 'user1',
          points: 1500,
          rank: 1,
          displayName: 'Top Player',
        },
      }

      mockApiInstance.apiLeaderboardsGet.mockResolvedValue({
        data: mockLeaderboard,
      })

      const { result } = renderHook(
        () =>
          useLeaderboard({
            type: 'points',
            category: 'xp',
            timeRange: 'alltime',
            page: 1,
            pageSize: 50,
          }),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockLeaderboard)
      expect(mockApiInstance.apiLeaderboardsGet).toHaveBeenCalledWith(
        'points',
        'xp',
        'alltime',
        1,
        50
      )
    })

    it('should fetch points leaderboard', async () => {
      const mockLeaderboard = {
        entries: [
          {
            userId: 'user1',
            points: 1500,
            rank: 1,
            displayName: 'Top Player',
          },
        ],
        totalCount: 1,
        totalPages: 1,
        currentPage: 1,
        pageSize: 50,
        hasNextPage: false,
        hasPreviousPage: false,
        topEntry: {
          userId: 'user1',
          points: 1500,
          rank: 1,
          displayName: 'Top Player',
        },
      }

      mockApiInstance.apiLeaderboardsPointsCategoryGet.mockResolvedValue({
        data: mockLeaderboard,
      })

      const { result } = renderHook(
        () => usePointsLeaderboard('xp', 'alltime', 1, 50),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockLeaderboard)
      expect(
        mockApiInstance.apiLeaderboardsPointsCategoryGet
      ).toHaveBeenCalledWith('xp', 'alltime', 1, 50)
    })

    it('should fetch badges leaderboard', async () => {
      const mockLeaderboard = {
        entries: [
          {
            userId: 'user1',
            points: 5,
            rank: 1,
            displayName: 'Badge Collector',
          },
        ],
        totalCount: 1,
        totalPages: 1,
        currentPage: 1,
        pageSize: 50,
        hasNextPage: false,
        hasPreviousPage: false,
        topEntry: {
          userId: 'user1',
          points: 5,
          rank: 1,
          displayName: 'Badge Collector',
        },
      }

      mockApiInstance.apiLeaderboardsBadgesGet.mockResolvedValue({
        data: mockLeaderboard,
      })

      const { result } = renderHook(
        () => useBadgesLeaderboard('alltime', 1, 50),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockLeaderboard)
      expect(mockApiInstance.apiLeaderboardsBadgesGet).toHaveBeenCalledWith(
        'alltime',
        1,
        50
      )
    })

    it('should fetch trophies leaderboard', async () => {
      const mockLeaderboard = {
        entries: [
          {
            userId: 'user1',
            points: 3,
            rank: 1,
            displayName: 'Trophy Master',
          },
        ],
        totalCount: 1,
        totalPages: 1,
        currentPage: 1,
        pageSize: 50,
        hasNextPage: false,
        hasPreviousPage: false,
        topEntry: {
          userId: 'user1',
          points: 3,
          rank: 1,
          displayName: 'Trophy Master',
        },
      }

      mockApiInstance.apiLeaderboardsTrophiesGet.mockResolvedValue({
        data: mockLeaderboard,
      })

      const { result } = renderHook(
        () => useTrophiesLeaderboard('alltime', 1, 50),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockLeaderboard)
      expect(mockApiInstance.apiLeaderboardsTrophiesGet).toHaveBeenCalledWith(
        'alltime',
        1,
        50
      )
    })

    it('should fetch levels leaderboard', async () => {
      const mockLeaderboard = {
        entries: [
          {
            userId: 'user1',
            points: 5,
            rank: 1,
            displayName: 'Level Master',
          },
        ],
        totalCount: 1,
        totalPages: 1,
        currentPage: 1,
        pageSize: 50,
        hasNextPage: false,
        hasPreviousPage: false,
        topEntry: {
          userId: 'user1',
          points: 5,
          rank: 1,
          displayName: 'Level Master',
        },
      }

      mockApiInstance.apiLeaderboardsLevelsCategoryGet.mockResolvedValue({
        data: mockLeaderboard,
      })

      const { result } = renderHook(
        () => useLevelsLeaderboard('xp', 'alltime', 1, 50),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockLeaderboard)
      expect(
        mockApiInstance.apiLeaderboardsLevelsCategoryGet
      ).toHaveBeenCalledWith('xp', 'alltime', 1, 50)
    })

    it('should fetch user rank', async () => {
      const mockUserRank = {
        userId: 'user1',
        rank: 5,
        points: 1000,
        displayName: 'Player One',
        isInLeaderboard: true,
      }

      mockApiInstance.apiLeaderboardsUserUserIdRankGet.mockResolvedValue({
        data: mockUserRank,
      })

      const { result } = renderHook(
        () => useUserRank('user1', 'points', 'xp', 'alltime'),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockUserRank)
      expect(
        mockApiInstance.apiLeaderboardsUserUserIdRankGet
      ).toHaveBeenCalledWith('user1', 'points', 'xp', 'alltime')
    })

    it('should not fetch when required parameters are missing', async () => {
      const { result } = renderHook(
        () =>
          useLeaderboard({
            type: 'points',
            category: undefined,
            timeRange: 'alltime',
            page: 1,
            pageSize: 50,
          }),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false)
      })

      expect(result.current.data).toBeUndefined()
      expect(mockApiInstance.apiLeaderboardsGet).not.toHaveBeenCalled()
    })

    it('should not fetch when category is empty for points leaderboard', async () => {
      const { result } = renderHook(
        () => usePointsLeaderboard('', 'alltime', 1, 50),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false)
      })

      expect(result.current.data).toBeUndefined()
      expect(
        mockApiInstance.apiLeaderboardsPointsCategoryGet
      ).not.toHaveBeenCalled()
    })

    it('should not fetch when userId is empty for user rank', async () => {
      const { result } = renderHook(
        () => useUserRank('', 'points', 'xp', 'alltime'),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false)
      })

      expect(result.current.data).toBeUndefined()
      expect(
        mockApiInstance.apiLeaderboardsUserUserIdRankGet
      ).not.toHaveBeenCalled()
    })
  })

  describe('Mutation hooks', () => {
    it('should refresh leaderboard', async () => {
      mockApiInstance.apiLeaderboardsRefreshPost.mockResolvedValue({})

      const { result } = renderHook(() => useRefreshLeaderboard(), {
        wrapper: createWrapper(),
      })

      result.current.mutate({
        type: 'points',
        category: 'xp',
        timeRange: 'alltime',
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(mockApiInstance.apiLeaderboardsRefreshPost).toHaveBeenCalledWith(
        'points',
        'xp',
        'alltime'
      )
    })

    it('should refresh leaderboard without parameters', async () => {
      mockApiInstance.apiLeaderboardsRefreshPost.mockResolvedValue({})

      const { result } = renderHook(() => useRefreshLeaderboard(), {
        wrapper: createWrapper(),
      })

      result.current.mutate({})

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(mockApiInstance.apiLeaderboardsRefreshPost).toHaveBeenCalledWith(
        undefined,
        undefined,
        undefined
      )
    })
  })
})
