import {
  BadgesApiInstance,
  TrophiesApiInstance,
  LevelsApiInstance,
  PointCategoriesApiInstance,
} from './generated-client'
import type {
  BadgeDto,
  TrophyDto,
  PointCategoryDto,
  LevelDto,
} from './generated/models'

// Badges API
export const badgesApi = {
  // Get all badges
  getAllBadges: async (): Promise<BadgeDto[]> => {
    const response = await BadgesApiInstance().apiBadgesGet()
    return response.data
  },

  // Get visible badges only
  getVisibleBadges: async (): Promise<BadgeDto[]> => {
    const response = await BadgesApiInstance().apiBadgesVisibleGet()
    return response.data
  },

  // Get specific badge by ID
  getBadgeById: async (id: string): Promise<BadgeDto> => {
    const response = await BadgesApiInstance().apiBadgesIdGet(id)
    return response.data
  },
}

// Trophies API
export const trophiesApi = {
  // Get all trophies
  getAllTrophies: async (): Promise<TrophyDto[]> => {
    const response = await TrophiesApiInstance().apiTrophiesGet()
    return response.data
  },

  // Get visible trophies only
  getVisibleTrophies: async (): Promise<TrophyDto[]> => {
    const response = await TrophiesApiInstance().apiTrophiesVisibleGet()
    return response.data
  },

  // Get specific trophy by ID
  getTrophyById: async (id: string): Promise<TrophyDto> => {
    const response = await TrophiesApiInstance().apiTrophiesIdGet(id)
    return response.data
  },
}

// Levels API
export const levelsApi = {
  // Get all levels
  getAllLevels: async (): Promise<LevelDto[]> => {
    const response = await LevelsApiInstance().apiLevelsGet()
    return response.data
  },

  // Get levels by category
  getLevelsByCategory: async (category: string): Promise<LevelDto[]> => {
    const response = await LevelsApiInstance().apiLevelsCategoryCategoryGet(
      category
    )
    return response.data
  },

  // Get specific level by ID
  getLevelById: async (id: string): Promise<LevelDto> => {
    const response = await LevelsApiInstance().apiLevelsIdGet(id)
    return response.data
  },
}

// Point Categories API
export const pointCategoriesApi = {
  // Get all point categories
  getAllPointCategories: async (): Promise<PointCategoryDto[]> => {
    const response = await PointCategoriesApiInstance().apiPointCategoriesGet()
    return response.data
  },

  // Get specific point category by ID
  getPointCategoryById: async (id: string): Promise<PointCategoryDto> => {
    const response = await PointCategoriesApiInstance().apiPointCategoriesIdGet(
      id
    )
    return response.data
  },
}
