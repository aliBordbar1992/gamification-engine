import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { vi } from 'vitest'
import {
  useBadges,
  useTrophies,
  useLevels,
  usePointCategories,
  useBadge,
  useTrophy,
  useLevel,
  usePointCategory,
} from '@/hooks/useEntities'
import * as entitiesApi from '@/api/entities'

// Mock the entities API
vi.mock('@/api/entities', () => ({
  badgesApi: {
    getAllBadges: vi.fn(),
    getVisibleBadges: vi.fn(),
    getBadgeById: vi.fn(),
  },
  trophiesApi: {
    getAllTrophies: vi.fn(),
    getVisibleTrophies: vi.fn(),
    getTrophyById: vi.fn(),
  },
  levelsApi: {
    getAllLevels: vi.fn(),
    getLevelsByCategory: vi.fn(),
    getLevelById: vi.fn(),
  },
  pointCategoriesApi: {
    getAllPointCategories: vi.fn(),
    getPointCategoryById: vi.fn(),
  },
}))

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  })
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  )
}

describe('useEntities hooks', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('Badges hooks', () => {
    it('should fetch all badges', async () => {
      const mockBadges = [
        {
          id: 'badge1',
          name: 'First Comment',
          description: 'Awarded for first comment',
          image: '/images/badges/default.png',
          visible: true,
        },
      ]

      vi.mocked(entitiesApi.badgesApi.getAllBadges).mockResolvedValue(
        mockBadges
      )

      const { result } = renderHook(() => useBadges(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockBadges)
      expect(entitiesApi.badgesApi.getAllBadges).toHaveBeenCalledOnce()
    })

    it('should fetch badge by id', async () => {
      const mockBadge = {
        id: 'badge1',
        name: 'First Comment',
        description: 'Awarded for first comment',
        image: '/images/badges/default.png',
        visible: true,
      }

      vi.mocked(entitiesApi.badgesApi.getBadgeById).mockResolvedValue(mockBadge)

      const { result } = renderHook(() => useBadge('badge1'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockBadge)
      expect(entitiesApi.badgesApi.getBadgeById).toHaveBeenCalledWith('badge1')
    })
  })

  describe('Trophies hooks', () => {
    it('should fetch all trophies', async () => {
      const mockTrophies = [
        {
          id: 'trophy1',
          name: 'Champion',
          description: 'Top performer trophy',
          image: '/images/trophies/default.png',
          visible: true,
        },
      ]

      vi.mocked(entitiesApi.trophiesApi.getAllTrophies).mockResolvedValue(
        mockTrophies
      )

      const { result } = renderHook(() => useTrophies(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockTrophies)
      expect(entitiesApi.trophiesApi.getAllTrophies).toHaveBeenCalledOnce()
    })

    it('should fetch trophy by id', async () => {
      const mockTrophy = {
        id: 'trophy1',
        name: 'Champion',
        description: 'Top performer trophy',
        image: '/images/trophies/default.png',
        visible: true,
      }

      vi.mocked(entitiesApi.trophiesApi.getTrophyById).mockResolvedValue(
        mockTrophy
      )

      const { result } = renderHook(() => useTrophy('trophy1'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockTrophy)
      expect(entitiesApi.trophiesApi.getTrophyById).toHaveBeenCalledWith(
        'trophy1'
      )
    })
  })

  describe('Levels hooks', () => {
    it('should fetch all levels', async () => {
      const mockLevels = [
        {
          id: 'level1',
          name: 'Beginner',
          description: 'Starting level',
          category: 'xp',
          minPoints: 0,
        },
      ]

      vi.mocked(entitiesApi.levelsApi.getAllLevels).mockResolvedValue(
        mockLevels
      )

      const { result } = renderHook(() => useLevels(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockLevels)
      expect(entitiesApi.levelsApi.getAllLevels).toHaveBeenCalledOnce()
    })

    it('should fetch level by id', async () => {
      const mockLevel = {
        id: 'level1',
        name: 'Beginner',
        description: 'Starting level',
        category: 'xp',
        minPoints: 0,
      }

      vi.mocked(entitiesApi.levelsApi.getLevelById).mockResolvedValue(mockLevel)

      const { result } = renderHook(() => useLevel('level1'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockLevel)
      expect(entitiesApi.levelsApi.getLevelById).toHaveBeenCalledWith('level1')
    })
  })

  describe('Point Categories hooks', () => {
    it('should fetch all point categories', async () => {
      const mockPointCategories = [
        {
          id: 'xp',
          name: 'Experience Points',
          description: 'Main experience currency',
          aggregation: 'sum',
        },
      ]

      vi.mocked(
        entitiesApi.pointCategoriesApi.getAllPointCategories
      ).mockResolvedValue(mockPointCategories)

      const { result } = renderHook(() => usePointCategories(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockPointCategories)
      expect(
        entitiesApi.pointCategoriesApi.getAllPointCategories
      ).toHaveBeenCalledOnce()
    })

    it('should fetch point category by id', async () => {
      const mockPointCategory = {
        id: 'xp',
        name: 'Experience Points',
        description: 'Main experience currency',
        aggregation: 'sum',
      }

      vi.mocked(
        entitiesApi.pointCategoriesApi.getPointCategoryById
      ).mockResolvedValue(mockPointCategory)

      const { result } = renderHook(() => usePointCategory('xp'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockPointCategory)
      expect(
        entitiesApi.pointCategoriesApi.getPointCategoryById
      ).toHaveBeenCalledWith('xp')
    })
  })
})
