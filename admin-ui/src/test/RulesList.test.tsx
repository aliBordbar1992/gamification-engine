import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import RulesList from '../components/RulesList'
import { useRules } from '../hooks/useGeneratedRules'

// Mock the hooks
vi.mock('../hooks/useGeneratedRules', () => ({
  useRules: vi.fn(),
}))

const mockUseRules = useRules as any

// Test wrapper
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  })

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>{children}</BrowserRouter>
    </QueryClientProvider>
  )
}

const mockRules = [
  {
    id: 'rule1',
    name: 'First Comment Reward',
    description: 'Award 10 XP for first comment',
    isActive: true,
    triggers: ['USER_COMMENTED'],
    conditions: [
      {
        type: 'firstOccurrence',
        parameters: {},
      },
    ],
    rewards: [
      {
        type: 'points',
        targetId: 'xp',
        amount: 10,
        parameters: {},
      },
    ],
    createdAt: '2023-01-01T00:00:00Z',
  },
  {
    id: 'rule2',
    name: 'Like Reward',
    description: 'Award 5 XP for each like',
    isActive: false,
    triggers: ['USER_LIKED'],
    conditions: [],
    rewards: [
      {
        type: 'points',
        targetId: 'xp',
        amount: 5,
        parameters: {},
      },
    ],
    createdAt: '2023-01-02T00:00:00Z',
  },
]

describe('RulesList', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render loading state', () => {
    mockUseRules.mockReturnValue({
      data: [],
      isLoading: true,
      error: null,
    })

    render(<RulesList />, { wrapper: createWrapper() })

    // The table should show loading state
    expect(screen.getByRole('table')).toBeInTheDocument()
  })

  it('should render rules list', async () => {
    mockUseRules.mockReturnValue({
      data: mockRules,
      isLoading: false,
      error: null,
    })

    render(<RulesList />, { wrapper: createWrapper() })

    expect(screen.getByText('First Comment Reward')).toBeInTheDocument()
    expect(screen.getByText('Like Reward')).toBeInTheDocument()
    expect(
      screen.getByText('Award 10 XP for first comment')
    ).toBeInTheDocument()
    expect(screen.getByText('Award 5 XP for each like')).toBeInTheDocument()
  })

  it('should display stats cards', () => {
    mockUseRules.mockReturnValue({
      data: mockRules,
      isLoading: false,
      error: null,
    })

    render(<RulesList />, { wrapper: createWrapper() })

    expect(screen.getByText('Total Rules')).toBeInTheDocument()
    expect(screen.getByText('Active Rules')).toBeInTheDocument()
    expect(screen.getByText('Inactive Rules')).toBeInTheDocument()

    // Check for specific stats values in their containers
    const totalRulesCard = screen.getByText('Total Rules').closest('.ant-card')
    const activeRulesCard = screen
      .getByText('Active Rules')
      .closest('.ant-card')
    const inactiveRulesCard = screen
      .getByText('Inactive Rules')
      .closest('.ant-card')

    expect(totalRulesCard).toHaveTextContent('2')
    expect(activeRulesCard).toHaveTextContent('1')
    expect(inactiveRulesCard).toHaveTextContent('1')
  })

  it('should display rule status badges', () => {
    mockUseRules.mockReturnValue({
      data: mockRules,
      isLoading: false,
      error: null,
    })

    render(<RulesList />, { wrapper: createWrapper() })

    expect(screen.getByText('Active')).toBeInTheDocument()
    expect(screen.getByText('Inactive')).toBeInTheDocument()
  })

  it('should display triggers as tags', () => {
    mockUseRules.mockReturnValue({
      data: mockRules,
      isLoading: false,
      error: null,
    })

    render(<RulesList />, { wrapper: createWrapper() })

    expect(screen.getByText('USER_COMMENTED')).toBeInTheDocument()
    expect(screen.getByText('USER_LIKED')).toBeInTheDocument()
  })

  it('should display rewards as tags', () => {
    mockUseRules.mockReturnValue({
      data: mockRules,
      isLoading: false,
      error: null,
    })

    render(<RulesList />, { wrapper: createWrapper() })

    expect(screen.getByText('points: xp (10)')).toBeInTheDocument()
    expect(screen.getByText('points: xp (5)')).toBeInTheDocument()
  })

  it('should handle search functionality', async () => {
    const mockUseRulesWithFilters = vi.fn()
    mockUseRules.mockImplementation(mockUseRulesWithFilters)

    mockUseRulesWithFilters.mockReturnValue({
      data: mockRules,
      isLoading: false,
      error: null,
    })

    render(<RulesList />, { wrapper: createWrapper() })

    const searchInput = screen.getByPlaceholderText(
      'Search rules by name, description, or ID...'
    )
    fireEvent.change(searchInput, { target: { value: 'comment' } })

    await waitFor(() => {
      expect(mockUseRulesWithFilters).toHaveBeenCalledWith({
        filters: {
          search: 'comment',
        },
      })
    })
  })

  it('should handle status filter', async () => {
    const mockUseRulesWithFilters = vi.fn()
    mockUseRules.mockImplementation(mockUseRulesWithFilters)

    mockUseRulesWithFilters.mockReturnValue({
      data: mockRules,
      isLoading: false,
      error: null,
    })

    render(<RulesList />, { wrapper: createWrapper() })

    // Find the status select by its role and click to open
    const statusSelect = screen.getByRole('combobox')
    fireEvent.mouseDown(statusSelect)

    // Find and click the Active option
    const activeOption = screen.getByText('Active')
    fireEvent.click(activeOption)

    await waitFor(() => {
      expect(mockUseRulesWithFilters).toHaveBeenCalledWith({
        filters: {
          isActive: true,
        },
      })
    })
  })

  it('should clear filters', async () => {
    const mockUseRulesWithFilters = vi.fn()
    mockUseRules.mockImplementation(mockUseRulesWithFilters)

    mockUseRulesWithFilters.mockReturnValue({
      data: mockRules,
      isLoading: false,
      error: null,
    })

    render(<RulesList />, { wrapper: createWrapper() })

    // Set some filters first
    const searchInput = screen.getByPlaceholderText(
      'Search rules by name, description, or ID...'
    )
    fireEvent.change(searchInput, { target: { value: 'test' } })

    const clearButton = screen.getByText('Clear Filters')
    fireEvent.click(clearButton)

    await waitFor(() => {
      expect(mockUseRulesWithFilters).toHaveBeenCalledWith({
        filters: {},
      })
    })
  })

  it('should handle view rule action', () => {
    const mockOnViewRule = vi.fn()
    mockUseRules.mockReturnValue({
      data: mockRules,
      isLoading: false,
      error: null,
    })

    render(<RulesList onViewRule={mockOnViewRule} />, {
      wrapper: createWrapper(),
    })

    const viewButtons = screen.getAllByRole('button')
    const viewButton = viewButtons.find((button) =>
      button.querySelector('[data-icon="eye"]')
    )

    if (viewButton) {
      fireEvent.click(viewButton)
      expect(mockOnViewRule).toHaveBeenCalledWith(mockRules[0])
    }
  })

  it('should display error state', () => {
    const error = new Error('Failed to fetch rules')
    mockUseRules.mockReturnValue({
      data: [],
      isLoading: false,
      error,
    })

    render(<RulesList />, { wrapper: createWrapper() })

    expect(
      screen.getByText('Error loading rules: Failed to fetch rules')
    ).toBeInTheDocument()
  })

  it('should handle empty state', () => {
    mockUseRules.mockReturnValue({
      data: [],
      isLoading: false,
      error: null,
    })

    render(<RulesList />, { wrapper: createWrapper() })

    expect(screen.getByText('Total Rules')).toBeInTheDocument()

    // Check that all stats show 0
    const totalRulesCard = screen.getByText('Total Rules').closest('.ant-card')
    const activeRulesCard = screen
      .getByText('Active Rules')
      .closest('.ant-card')
    const inactiveRulesCard = screen
      .getByText('Inactive Rules')
      .closest('.ant-card')

    expect(totalRulesCard).toHaveTextContent('0')
    expect(activeRulesCard).toHaveTextContent('0')
    expect(inactiveRulesCard).toHaveTextContent('0')
  })

  it('should display pagination', () => {
    mockUseRules.mockReturnValue({
      data: mockRules,
      isLoading: false,
      error: null,
    })

    render(<RulesList />, { wrapper: createWrapper() })

    expect(screen.getByText('1-2 of 2 rules')).toBeInTheDocument()
  })
})
