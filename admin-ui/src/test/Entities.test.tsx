import { render, screen, fireEvent } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { vi } from 'vitest'
import Entities from '@/pages/Entities'
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

describe('Entities Page', () => {
  beforeEach(() => {
    vi.clearAllMocks()

    // Mock successful API responses
    vi.mocked(entitiesApi.badgesApi.getAllBadges).mockResolvedValue([
      {
        id: 'badge1',
        name: 'First Comment',
        description: 'Awarded for first comment',
        image: '/images/badges/default.png',
        visible: true,
      },
    ])

    vi.mocked(entitiesApi.trophiesApi.getAllTrophies).mockResolvedValue([
      {
        id: 'trophy1',
        name: 'Champion',
        description: 'Top performer trophy',
        image: '/images/trophies/default.png',
        visible: true,
      },
    ])

    vi.mocked(entitiesApi.levelsApi.getAllLevels).mockResolvedValue([
      {
        id: 'level1',
        name: 'Beginner',
        description: 'Starting level',
        category: 'xp',
        minPoints: 0,
      },
    ])

    vi.mocked(
      entitiesApi.pointCategoriesApi.getAllPointCategories
    ).mockResolvedValue([
      {
        id: 'xp',
        name: 'Experience Points',
        description: 'Main experience currency',
        aggregation: 'sum',
      },
    ])
  })

  it('should render entities page with tabs', () => {
    render(<Entities />, { wrapper: createWrapper() })

    expect(screen.getByText('Entities Management')).toBeInTheDocument()
    expect(screen.getByText('Badges')).toBeInTheDocument()
    expect(screen.getByText('Trophies')).toBeInTheDocument()
    expect(screen.getByText('Levels')).toBeInTheDocument()
    expect(screen.getByText('Point Categories')).toBeInTheDocument()
  })

  it('should switch between tabs', () => {
    render(<Entities />, { wrapper: createWrapper() })

    // Click on Trophies tab
    fireEvent.click(screen.getByText('Trophies'))

    // Should show trophies content
    expect(screen.getByText('Trophies')).toBeInTheDocument()
  })

  it('should show badges list by default', () => {
    render(<Entities />, { wrapper: createWrapper() })

    // Should show badges tab as active
    expect(screen.getByText('Badges')).toBeInTheDocument()
  })
})
