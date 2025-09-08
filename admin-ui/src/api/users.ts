import type {
  UserStateDto,
  UserRewardHistoryDto,
  BadgeDto,
  TrophyDto,
  LevelDto,
} from './generated/models'
import { UsersApiInstance } from './generated-client'

// Users API
export const usersApi = {
  // Get complete user state (points, badges, trophies, levels)
  getUserState: async (userId: string): Promise<UserStateDto> => {
    const response = await UsersApiInstance().apiUsersUserIdStateGet(userId)
    return response.data
  },

  // Get user points for all categories
  getUserPoints: async (userId: string): Promise<{ [key: string]: number }> => {
    const response = await UsersApiInstance().apiUsersUserIdPointsGet(userId)
    return response.data
  },

  // Get user points for a specific category
  getUserPointsByCategory: async (
    userId: string,
    category: string
  ): Promise<number> => {
    const response = await UsersApiInstance().apiUsersUserIdPointsCategoryGet(
      userId,
      category
    )
    return response.data
  },

  // Get user badges
  getUserBadges: async (userId: string): Promise<BadgeDto[]> => {
    const response = await UsersApiInstance().apiUsersUserIdBadgesGet(userId)
    return response.data
  },

  // Get user trophies
  getUserTrophies: async (userId: string): Promise<TrophyDto[]> => {
    const response = await UsersApiInstance().apiUsersUserIdTrophiesGet(userId)
    return response.data
  },

  // Get user levels for all categories
  getUserLevels: async (
    userId: string
  ): Promise<{ [key: string]: LevelDto }> => {
    const response = await UsersApiInstance().apiUsersUserIdLevelsGet(userId)
    return response.data
  },

  // Get user level for a specific category
  getUserLevelByCategory: async (
    userId: string,
    category: string
  ): Promise<LevelDto> => {
    const response = await UsersApiInstance().apiUsersUserIdLevelsCategoryGet(
      userId,
      category
    )
    return response.data
  },

  // Get user reward history
  getUserRewardHistory: async (
    userId: string,
    page: number = 1,
    pageSize: number = 20
  ): Promise<UserRewardHistoryDto> => {
    const response = await UsersApiInstance().apiUsersUserIdRewardsHistoryGet(
      userId,
      page,
      pageSize
    )
    return response.data
  },
}
