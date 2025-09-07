import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import RuleDetails from '../components/RuleDetails'
import { useRule } from '../hooks/useRules'

// Mock the hooks
vi.mock('../hooks/useRules', () => ({
  useRule: vi.fn(),
}))

// Mock react-router-dom
const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

// Mock clipboard API
Object.assign(navigator, {
  clipboard: {
    writeText: vi.fn(),
  },
})

const mockUseRule = useRule as any

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

const mockRule = {
  id: 'rule1',
  name: 'First Comment Reward',
  description: 'Award 10 XP for first comment',
  isActive: true,
  triggers: ['USER_COMMENTED'],
  conditions: [
    {
      type: 'firstOccurrence',
      parameters: {
        maxOccurrences: 1,
      },
    },
  ],
  rewards: [
    {
      type: 'points',
      targetId: 'xp',
      amount: 10,
      parameters: {
        multiplier: 1,
      },
    },
    {
      type: 'badge',
      targetId: 'first-comment',
      parameters: {},
    },
  ],
  createdAt: '2023-01-01T00:00:00Z',
  updatedAt: '2023-01-02T00:00:00Z',
}

describe('RuleDetails', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render loading state', () => {
    mockUseRule.mockReturnValue({
      data: null,
      isLoading: true,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    expect(screen.getByRole('img', { hidden: true })).toBeInTheDocument() // Loading spinner
  })

  it('should render rule details', () => {
    mockUseRule.mockReturnValue({
      data: mockRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    expect(screen.getByText('First Comment Reward')).toBeInTheDocument()
    expect(
      screen.getByText('Award 10 XP for first comment')
    ).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
    expect(screen.getByText('rule1')).toBeInTheDocument()
  })

  it('should display basic information', () => {
    mockUseRule.mockReturnValue({
      data: mockRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    expect(screen.getByText('Basic Information')).toBeInTheDocument()
    expect(screen.getByText('rule1')).toBeInTheDocument()
    expect(screen.getByText('First Comment Reward')).toBeInTheDocument()
    expect(
      screen.getByText('Award 10 XP for first comment')
    ).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
  })

  it('should display triggers', () => {
    mockUseRule.mockReturnValue({
      data: mockRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    expect(screen.getByText('Triggers')).toBeInTheDocument()
    expect(screen.getByText('USER_COMMENTED')).toBeInTheDocument()
  })

  it('should display conditions', () => {
    mockUseRule.mockReturnValue({
      data: mockRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    expect(screen.getByText('Conditions')).toBeInTheDocument()
    expect(screen.getByText('firstOccurrence')).toBeInTheDocument()
    expect(screen.getByText('Parameters:')).toBeInTheDocument()
  })

  it('should display rewards', () => {
    mockUseRule.mockReturnValue({
      data: mockRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    expect(screen.getByText('Rewards')).toBeInTheDocument()
    expect(screen.getByText('points')).toBeInTheDocument()
    expect(screen.getByText('xp')).toBeInTheDocument()
    expect(screen.getByText('(10)')).toBeInTheDocument()
    expect(screen.getByText('badge')).toBeInTheDocument()
    expect(screen.getByText('first-comment')).toBeInTheDocument()
  })

  it('should handle back navigation', () => {
    mockUseRule.mockReturnValue({
      data: mockRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    const backButton = screen.getByText('Back to Rules')
    fireEvent.click(backButton)

    expect(mockNavigate).toHaveBeenCalledWith('/rules')
  })

  it('should copy JSON to clipboard', async () => {
    mockUseRule.mockReturnValue({
      data: mockRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    const copyJsonButton = screen.getByText('Copy JSON')
    fireEvent.click(copyJsonButton)

    expect(navigator.clipboard.writeText).toHaveBeenCalledWith(
      JSON.stringify(mockRule, null, 2)
    )
  })

  it('should copy YAML to clipboard', async () => {
    mockUseRule.mockReturnValue({
      data: mockRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    const copyYamlButton = screen.getByText('Copy YAML')
    fireEvent.click(copyYamlButton)

    expect(navigator.clipboard.writeText).toHaveBeenCalled()
  })

  it('should handle edit action', () => {
    const mockOnEdit = vi.fn()
    mockUseRule.mockReturnValue({
      data: mockRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" onEdit={mockOnEdit} />, {
      wrapper: createWrapper(),
    })

    const editButton = screen.getByText('Edit Rule')
    fireEvent.click(editButton)

    expect(mockOnEdit).toHaveBeenCalledWith(mockRule)
  })

  it('should not show edit button when onEdit is not provided', () => {
    mockUseRule.mockReturnValue({
      data: mockRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    expect(screen.queryByText('Edit Rule')).not.toBeInTheDocument()
  })

  it('should display JSON tab content', () => {
    mockUseRule.mockReturnValue({
      data: mockRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    const jsonTab = screen.getByText('JSON')
    fireEvent.click(jsonTab)

    // Check that JSON content is displayed in a pre element
    const preElements = screen.getAllByRole('generic', { hidden: true })
    const jsonPre = preElements.find((el) =>
      el.textContent?.includes('"id": "rule1"')
    )
    expect(jsonPre).toBeInTheDocument()
    expect(jsonPre).toHaveTextContent('"name": "First Comment Reward"')
  })

  it('should display YAML tab content', () => {
    mockUseRule.mockReturnValue({
      data: mockRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    const yamlTab = screen.getByText('YAML')
    fireEvent.click(yamlTab)

    // YAML content should be displayed (basic check)
    const preElements = screen.getAllByRole('generic', { hidden: true })
    const yamlPre = preElements.find((el) =>
      el.textContent?.includes('id: rule1')
    )
    expect(yamlPre).toBeInTheDocument()
    expect(yamlPre).toHaveTextContent('name: First Comment Reward')
  })

  it('should display error state', () => {
    const error = new Error('Failed to fetch rule')
    mockUseRule.mockReturnValue({
      data: null,
      isLoading: false,
      error,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    expect(
      screen.getByText('Error loading rule: Failed to fetch rule')
    ).toBeInTheDocument()
  })

  it('should display not found state', () => {
    mockUseRule.mockReturnValue({
      data: null,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    expect(screen.getByText('Rule not found')).toBeInTheDocument()
  })

  it('should display inactive status correctly', () => {
    const inactiveRule = { ...mockRule, isActive: false }
    mockUseRule.mockReturnValue({
      data: inactiveRule,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    // Look for the inactive status badge specifically in the header
    const inactiveBadges = screen.getAllByText('Inactive')
    expect(inactiveBadges.length).toBeGreaterThan(0)
    expect(inactiveBadges[0]).toBeInTheDocument()
  })

  it('should handle rules with no conditions', () => {
    const ruleWithoutConditions = { ...mockRule, conditions: [] }
    mockUseRule.mockReturnValue({
      data: ruleWithoutConditions,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    expect(screen.getByText('No conditions defined')).toBeInTheDocument()
  })

  it('should handle rules with no rewards', () => {
    const ruleWithoutRewards = { ...mockRule, rewards: [] }
    mockUseRule.mockReturnValue({
      data: ruleWithoutRewards,
      isLoading: false,
      error: null,
    })

    render(<RuleDetails ruleId="rule1" />, { wrapper: createWrapper() })

    expect(screen.getByText('No rewards defined')).toBeInTheDocument()
  })
})
