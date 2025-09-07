import { useQuery } from '@tanstack/react-query'
import {
  badgesApi,
  trophiesApi,
  levelsApi,
  pointCategoriesApi,
} from '@/api/entities'
import type { Badge, Trophy, Level, PointCategory } from '@/types'

// Badges hooks
export const useBadges = () => {
  return useQuery({
    queryKey: ['badges'],
    queryFn: badgesApi.getAllBadges,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

export const useVisibleBadges = () => {
  return useQuery({
    queryKey: ['badges', 'visible'],
    queryFn: badgesApi.getVisibleBadges,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

export const useBadge = (id: string) => {
  return useQuery({
    queryKey: ['badges', id],
    queryFn: () => badgesApi.getBadgeById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

// Trophies hooks
export const useTrophies = () => {
  return useQuery({
    queryKey: ['trophies'],
    queryFn: trophiesApi.getAllTrophies,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

export const useVisibleTrophies = () => {
  return useQuery({
    queryKey: ['trophies', 'visible'],
    queryFn: trophiesApi.getVisibleTrophies,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

export const useTrophy = (id: string) => {
  return useQuery({
    queryKey: ['trophies', id],
    queryFn: () => trophiesApi.getTrophyById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

// Levels hooks
export const useLevels = () => {
  return useQuery({
    queryKey: ['levels'],
    queryFn: levelsApi.getAllLevels,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

export const useLevelsByCategory = (category: string) => {
  return useQuery({
    queryKey: ['levels', 'category', category],
    queryFn: () => levelsApi.getLevelsByCategory(category),
    enabled: !!category,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

export const useLevel = (id: string) => {
  return useQuery({
    queryKey: ['levels', id],
    queryFn: () => levelsApi.getLevelById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

// Point Categories hooks
export const usePointCategories = () => {
  return useQuery({
    queryKey: ['point-categories'],
    queryFn: pointCategoriesApi.getAllPointCategories,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

export const usePointCategory = (id: string) => {
  return useQuery({
    queryKey: ['point-categories', id],
    queryFn: () => pointCategoriesApi.getPointCategoryById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}
