import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  leaderboardsApiService,
  type LeaderboardQuery,
  type LeaderboardType,
  type TimeRange,
} from '@/api/leaderboards'
import type { LeaderboardDto, UserRankDto } from '@/api/generated/models'

// Query keys
export const leaderboardsKeys = {
  all: ['leaderboards'] as const,
  list: (query: LeaderboardQuery) =>
    [...leaderboardsKeys.all, 'list', query] as const,
  points: (
    category: string,
    timeRange: TimeRange,
    page: number,
    pageSize: number
  ) =>
    [
      ...leaderboardsKeys.all,
      'points',
      category,
      timeRange,
      page,
      pageSize,
    ] as const,
  badges: (timeRange: TimeRange, page: number, pageSize: number) =>
    [...leaderboardsKeys.all, 'badges', timeRange, page, pageSize] as const,
  trophies: (timeRange: TimeRange, page: number, pageSize: number) =>
    [...leaderboardsKeys.all, 'trophies', timeRange, page, pageSize] as const,
  levels: (
    category: string,
    timeRange: TimeRange,
    page: number,
    pageSize: number
  ) =>
    [
      ...leaderboardsKeys.all,
      'levels',
      category,
      timeRange,
      page,
      pageSize,
    ] as const,
  userRank: (
    userId: string,
    type: LeaderboardType,
    category?: string,
    timeRange?: TimeRange
  ) =>
    [
      ...leaderboardsKeys.all,
      'userRank',
      userId,
      type,
      category,
      timeRange,
    ] as const,
}

// General leaderboard hook
export const useLeaderboard = (query: LeaderboardQuery) => {
  const isEnabled =
    !!query.type &&
    (query.type === 'points' || query.type === 'level'
      ? !!query.category
      : true)

  return useQuery({
    queryKey: leaderboardsKeys.list(query),
    queryFn: () => leaderboardsApiService.getLeaderboard(query),
    staleTime: 2 * 60 * 1000, // 2 minutes
    enabled: isEnabled,
  })
}

// Points leaderboard hook
export const usePointsLeaderboard = (
  category: string,
  timeRange: TimeRange = 'alltime',
  page: number = 1,
  pageSize: number = 50
) => {
  return useQuery({
    queryKey: leaderboardsKeys.points(category, timeRange, page, pageSize),
    queryFn: () =>
      leaderboardsApiService.getPointsLeaderboard(
        category,
        timeRange,
        page,
        pageSize
      ),
    staleTime: 2 * 60 * 1000, // 2 minutes
    enabled: !!category,
  })
}

// Badges leaderboard hook
export const useBadgesLeaderboard = (
  timeRange: TimeRange = 'alltime',
  page: number = 1,
  pageSize: number = 50
) => {
  return useQuery({
    queryKey: leaderboardsKeys.badges(timeRange, page, pageSize),
    queryFn: () =>
      leaderboardsApiService.getBadgesLeaderboard(timeRange, page, pageSize),
    staleTime: 2 * 60 * 1000, // 2 minutes
  })
}

// Trophies leaderboard hook
export const useTrophiesLeaderboard = (
  timeRange: TimeRange = 'alltime',
  page: number = 1,
  pageSize: number = 50
) => {
  return useQuery({
    queryKey: leaderboardsKeys.trophies(timeRange, page, pageSize),
    queryFn: () =>
      leaderboardsApiService.getTrophiesLeaderboard(timeRange, page, pageSize),
    staleTime: 2 * 60 * 1000, // 2 minutes
  })
}

// Levels leaderboard hook
export const useLevelsLeaderboard = (
  category: string,
  timeRange: TimeRange = 'alltime',
  page: number = 1,
  pageSize: number = 50
) => {
  return useQuery({
    queryKey: leaderboardsKeys.levels(category, timeRange, page, pageSize),
    queryFn: () =>
      leaderboardsApiService.getLevelsLeaderboard(
        category,
        timeRange,
        page,
        pageSize
      ),
    staleTime: 2 * 60 * 1000, // 2 minutes
    enabled: !!category,
  })
}

// User rank hook
export const useUserRank = (
  userId: string,
  type: LeaderboardType,
  category?: string,
  timeRange: TimeRange = 'alltime'
) => {
  return useQuery({
    queryKey: leaderboardsKeys.userRank(userId, type, category, timeRange),
    queryFn: () =>
      leaderboardsApiService.getUserRank(userId, type, category, timeRange),
    staleTime: 2 * 60 * 1000, // 2 minutes
    enabled: !!userId && !!type,
  })
}

// Refresh leaderboard mutation
export const useRefreshLeaderboard = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      type,
      category,
      timeRange,
    }: {
      type?: LeaderboardType
      category?: string
      timeRange?: TimeRange
    }) => leaderboardsApiService.refreshLeaderboard(type, category, timeRange),
    onSuccess: () => {
      // Invalidate all leaderboard queries to refetch fresh data
      queryClient.invalidateQueries({ queryKey: leaderboardsKeys.all })
    },
  })
}
