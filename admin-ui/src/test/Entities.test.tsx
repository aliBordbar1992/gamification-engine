import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter } from 'react-router-dom'
import { vi } from 'vitest'
import Badges from '@/pages/Badges'
import Trophies from '@/pages/Trophies'
import Levels from '@/pages/Levels'
import PointCategories from '@/pages/PointCategories'

// Mock the entities hooks
vi.mock('@/hooks/useEntities', () => ({
  useBadges: vi.fn(() => ({
    data: [
      {
        id: 'badge1',
        name: 'First Comment',
        description: 'Awarded for first comment',
        image: '/images/badges/default.png',
        visible: true,
      },
    ],
    isLoading: false,
    error: null,
  })),
  useTrophies: vi.fn(() => ({
    data: [
      {
        id: 'trophy1',
        name: 'Champion',
        description: 'Top performer trophy',
        image: '/images/trophies/default.png',
        visible: true,
      },
    ],
    isLoading: false,
    error: null,
  })),
  useLevels: vi.fn(() => ({
    data: [
      {
        id: 'level1',
        name: 'Beginner',
        description: 'Starting level',
        category: 'xp',
        minPoints: 0,
      },
    ],
    isLoading: false,
    error: null,
  })),
  usePointCategories: vi.fn(() => ({
    data: [
      {
        id: 'xp',
        name: 'Experience Points',
        description: 'Main experience currency',
        aggregation: 'sum',
      },
    ],
    isLoading: false,
    error: null,
  })),
  useBadge: vi.fn(),
  useTrophy: vi.fn(),
  useLevel: vi.fn(),
  usePointCategory: vi.fn(),
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
    <BrowserRouter>
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    </BrowserRouter>
  )
}

describe('Entity Pages', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render badges page', () => {
    render(<Badges />, { wrapper: createWrapper() })

    expect(screen.getByText('Badges')).toBeInTheDocument()
    expect(screen.getByText('First Comment')).toBeInTheDocument()
  })

  it('should render trophies page', () => {
    render(<Trophies />, { wrapper: createWrapper() })

    expect(screen.getByText('Trophies')).toBeInTheDocument()
    expect(screen.getByText('Champion')).toBeInTheDocument()
  })

  it('should render levels page', () => {
    render(<Levels />, { wrapper: createWrapper() })

    expect(screen.getByText('Levels')).toBeInTheDocument()
    expect(screen.getByText('Beginner')).toBeInTheDocument()
  })

  it('should render point categories page', () => {
    render(<PointCategories />, { wrapper: createWrapper() })

    expect(screen.getByText('Point Categories')).toBeInTheDocument()
    expect(screen.getByText('Experience Points')).toBeInTheDocument()
  })
})
