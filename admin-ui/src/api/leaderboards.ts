import { LeaderboardsApiInstance } from './generated-client'
import type { LeaderboardDto, UserRankDto } from './generated/models'
import apiClient from './client'

// Create API instance
const leaderboardsApi = LeaderboardsApiInstance()

// Leaderboard types
export type LeaderboardType = 'points' | 'badges' | 'trophies' | 'level'
export type TimeRange = 'daily' | 'weekly' | 'monthly' | 'alltime'

// Leaderboard query parameters
export interface LeaderboardQuery {
  type: LeaderboardType
  category?: string
  timeRange?: TimeRange
  page?: number
  pageSize?: number
}

// API service functions
export const leaderboardsApiService = {
  // Get general leaderboard with query parameters
  getLeaderboard: async (query: LeaderboardQuery): Promise<LeaderboardDto> => {
    // Route to specific endpoints for better backend compatibility
    if (query.type === 'points' && query.category) {
      return leaderboardsApiService.getPointsLeaderboard(
        query.category,
        query.timeRange || 'alltime',
        query.page || 1,
        query.pageSize || 50
      )
    } else if (query.type === 'badges') {
      return leaderboardsApiService.getBadgesLeaderboard(
        query.timeRange || 'alltime',
        query.page || 1,
        query.pageSize || 50
      )
    } else if (query.type === 'trophies') {
      return leaderboardsApiService.getTrophiesLeaderboard(
        query.timeRange || 'alltime',
        query.page || 1,
        query.pageSize || 50
      )
    } else if (query.type === 'level' && query.category) {
      return leaderboardsApiService.getLevelsLeaderboard(
        query.category,
        query.timeRange || 'alltime',
        query.page || 1,
        query.pageSize || 50
      )
    }

    // Fallback to general endpoint for other cases
    const params = new URLSearchParams({
      type: query.type,
      ...(query.category && { category: query.category }),
      timeRange: query.timeRange || 'alltime',
      page: (query.page || 1).toString(),
      pageSize: (query.pageSize || 50).toString(),
    })
    const response = await apiClient.get(`/api/Leaderboards?${params}`)
    return response.data
  },

  // Get points leaderboard for specific category
  getPointsLeaderboard: async (
    category: string,
    timeRange: TimeRange = 'alltime',
    page: number = 1,
    pageSize: number = 50
  ): Promise<LeaderboardDto> => {
    // Workaround for malformed URL issue in generated client
    const params = new URLSearchParams({
      timeRange,
      page: page.toString(),
      pageSize: pageSize.toString(),
    })
    const response = await apiClient.get(
      `/api/Leaderboards/points/${category}?${params}`
    )
    return response.data
  },

  // Get badges leaderboard
  getBadgesLeaderboard: async (
    timeRange: TimeRange = 'alltime',
    page: number = 1,
    pageSize: number = 50
  ): Promise<LeaderboardDto> => {
    // Workaround for malformed URL issue in generated client
    const params = new URLSearchParams({
      timeRange,
      page: page.toString(),
      pageSize: pageSize.toString(),
    })
    const response = await apiClient.get(`/api/Leaderboards/badges?${params}`)
    return response.data
  },

  // Get trophies leaderboard
  getTrophiesLeaderboard: async (
    timeRange: TimeRange = 'alltime',
    page: number = 1,
    pageSize: number = 50
  ): Promise<LeaderboardDto> => {
    // Workaround for malformed URL issue in generated client
    const params = new URLSearchParams({
      timeRange,
      page: page.toString(),
      pageSize: pageSize.toString(),
    })
    const response = await apiClient.get(`/api/Leaderboards/trophies?${params}`)
    return response.data
  },

  // Get levels leaderboard for specific category
  getLevelsLeaderboard: async (
    category: string,
    timeRange: TimeRange = 'alltime',
    page: number = 1,
    pageSize: number = 50
  ): Promise<LeaderboardDto> => {
    // Workaround for malformed URL issue in generated client
    const params = new URLSearchParams({
      timeRange,
      page: page.toString(),
      pageSize: pageSize.toString(),
    })
    const response = await apiClient.get(
      `/api/Leaderboards/levels/${category}?${params}`
    )
    return response.data
  },

  // Get user rank in specific leaderboard
  getUserRank: async (
    userId: string,
    type: LeaderboardType,
    category?: string,
    timeRange: TimeRange = 'alltime'
  ): Promise<UserRankDto> => {
    // Workaround for malformed URL issue in generated client
    const params = new URLSearchParams({
      type,
      ...(category && { category }),
      timeRange,
    })
    const response = await apiClient.get(
      `/api/Leaderboards/user/${userId}/rank?${params}`
    )
    return response.data
  },

  // Refresh leaderboard cache
  refreshLeaderboard: async (
    type?: LeaderboardType,
    category?: string,
    timeRange?: TimeRange
  ): Promise<void> => {
    // Workaround for malformed URL issue in generated client
    const params = new URLSearchParams()
    if (type) params.append('type', type)
    if (category) params.append('category', category)
    if (timeRange) params.append('timeRange', timeRange)

    const queryString = params.toString()
    const url = queryString
      ? `/api/Leaderboards/refresh?${queryString}`
      : '/api/Leaderboards/refresh'
    await apiClient.post(url)
  },
}
