import { useQuery } from '@tanstack/react-query'
import { usersApi } from '@/api/users'

// Query keys
export const usersKeys = {
  all: ['users'] as const,
  list: (page?: number, pageSize?: number) => [...usersKeys.all, 'list', page, pageSize] as const,
  state: (userId: string) => [...usersKeys.all, 'state', userId] as const,
  points: (userId: string) => [...usersKeys.all, 'points', userId] as const,
  pointsByCategory: (userId: string, category: string) =>
    [...usersKeys.all, 'points', userId, category] as const,
  badges: (userId: string) => [...usersKeys.all, 'badges', userId] as const,
  trophies: (userId: string) => [...usersKeys.all, 'trophies', userId] as const,
  levels: (userId: string) => [...usersKeys.all, 'levels', userId] as const,
  levelByCategory: (userId: string, category: string) =>
    [...usersKeys.all, 'levels', userId, category] as const,
  rewardHistory: (userId: string, page?: number, pageSize?: number) =>
    [...usersKeys.all, 'rewardHistory', userId, page, pageSize] as const,
}

// User hooks
export const useUsers = (page: number = 1, pageSize: number = 20) => {
  return useQuery({
    queryKey: usersKeys.list(page, pageSize),
    queryFn: async () => {
      return await usersApi.getUsers(page, pageSize)
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

export const useUserState = (userId: string) => {
  return useQuery({
    queryKey: usersKeys.state(userId),
    queryFn: async () => {
      return await usersApi.getUserState(userId)
    },
    enabled: !!userId,
  })
}

export const useUserPoints = (userId: string) => {
  return useQuery({
    queryKey: usersKeys.points(userId),
    queryFn: async () => {
      return await usersApi.getUserPoints(userId)
    },
    enabled: !!userId,
  })
}

export const useUserPointsByCategory = (userId: string, category: string) => {
  return useQuery({
    queryKey: usersKeys.pointsByCategory(userId, category),
    queryFn: async () => {
      return await usersApi.getUserPointsByCategory(userId, category)
    },
    enabled: !!userId && !!category,
  })
}

export const useUserBadges = (userId: string) => {
  return useQuery({
    queryKey: usersKeys.badges(userId),
    queryFn: async () => {
      return await usersApi.getUserBadges(userId)
    },
    enabled: !!userId,
  })
}

export const useUserTrophies = (userId: string) => {
  return useQuery({
    queryKey: usersKeys.trophies(userId),
    queryFn: async () => {
      return await usersApi.getUserTrophies(userId)
    },
    enabled: !!userId,
  })
}

export const useUserLevels = (userId: string) => {
  return useQuery({
    queryKey: usersKeys.levels(userId),
    queryFn: async () => {
      return await usersApi.getUserLevels(userId)
    },
    enabled: !!userId,
  })
}

export const useUserLevelByCategory = (userId: string, category: string) => {
  return useQuery({
    queryKey: usersKeys.levelByCategory(userId, category),
    queryFn: async () => {
      return await usersApi.getUserLevelByCategory(userId, category)
    },
    enabled: !!userId && !!category,
  })
}

export const useUserRewardHistory = (
  userId: string,
  page: number = 1,
  pageSize: number = 20
) => {
  return useQuery({
    queryKey: usersKeys.rewardHistory(userId, page, pageSize),
    queryFn: async () => {
      return await usersApi.getUserRewardHistory(userId, page, pageSize)
    },
    enabled: !!userId,
  })
}
