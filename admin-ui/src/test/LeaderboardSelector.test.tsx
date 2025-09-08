import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { ConfigProvider } from 'antd'
import LeaderboardSelector from '../components/leaderboards/LeaderboardSelector'
import type { LeaderboardType, TimeRange } from '../api/leaderboards'

// Mock the useEntities hook
vi.mock('@/hooks/useEntities', () => ({
  usePointCategories: vi.fn(),
}))

const mockPointCategories = [
  { id: 'xp', name: 'Experience Points' },
  { id: 'coins', name: 'Coins' },
  { id: 'reputation', name: 'Reputation' },
]

const TestWrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <ConfigProvider>{children}</ConfigProvider>
)

describe('LeaderboardSelector', () => {
  const mockOnTypeChange = vi.fn()
  const mockOnCategoryChange = vi.fn()
  const mockOnTimeRangeChange = vi.fn()
  const mockOnRefresh = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()

    // Mock the usePointCategories hook
    const { usePointCategories } = vi.mocked(require('@/hooks/useEntities'))
    usePointCategories.mockReturnValue({
      data: mockPointCategories,
    })
  })

  const defaultProps = {
    type: 'points' as LeaderboardType,
    category: 'xp',
    timeRange: 'alltime' as TimeRange,
    onTypeChange: mockOnTypeChange,
    onCategoryChange: mockOnCategoryChange,
    onTimeRangeChange: mockOnTimeRangeChange,
    onRefresh: mockOnRefresh,
    loading: false,
  }

  it('should render all selector components', () => {
    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} />
      </TestWrapper>
    )

    expect(screen.getByText('Leaderboard Type')).toBeInTheDocument()
    expect(screen.getByText('Category')).toBeInTheDocument()
    expect(screen.getByText('Time Range')).toBeInTheDocument()
    expect(screen.getByText('Actions')).toBeInTheDocument()
  })

  it('should display current values correctly', () => {
    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} />
      </TestWrapper>
    )

    // Check if current values are displayed
    expect(screen.getByDisplayValue('Points')).toBeInTheDocument()
    expect(
      screen.getByDisplayValue('Experience Points (xp)')
    ).toBeInTheDocument()
    expect(screen.getByDisplayValue('All Time')).toBeInTheDocument()
  })

  it('should show category options for points type', () => {
    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} type="points" />
      </TestWrapper>
    )

    // Click on category dropdown to open options
    const categorySelect = screen.getByDisplayValue('Experience Points (xp)')
    fireEvent.click(categorySelect)

    // Check if category options are available
    expect(screen.getByText('Experience Points (xp)')).toBeInTheDocument()
    expect(screen.getByText('Coins (coins)')).toBeInTheDocument()
    expect(screen.getByText('Reputation (reputation)')).toBeInTheDocument()
  })

  it('should show category options for level type', () => {
    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} type="level" />
      </TestWrapper>
    )

    // Category selector should be visible for level type
    expect(screen.getByText('Category')).toBeInTheDocument()
  })

  it('should hide category selector for badges type', () => {
    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} type="badges" />
      </TestWrapper>
    )

    // Category selector should not be visible for badges type
    expect(screen.queryByText('Category')).not.toBeInTheDocument()
  })

  it('should hide category selector for trophies type', () => {
    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} type="trophies" />
      </TestWrapper>
    )

    // Category selector should not be visible for trophies type
    expect(screen.queryByText('Category')).not.toBeInTheDocument()
  })

  it('should call onTypeChange when type is changed', async () => {
    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} />
      </TestWrapper>
    )

    const typeSelect = screen.getByDisplayValue('Points')
    fireEvent.click(typeSelect)

    // Select badges option
    const badgesOption = screen.getByText('Badges')
    fireEvent.click(badgesOption)

    expect(mockOnTypeChange).toHaveBeenCalledWith('badges')
  })

  it('should call onCategoryChange when category is changed', async () => {
    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} />
      </TestWrapper>
    )

    const categorySelect = screen.getByDisplayValue('Experience Points (xp)')
    fireEvent.click(categorySelect)

    // Select coins option
    const coinsOption = screen.getByText('Coins (coins)')
    fireEvent.click(coinsOption)

    expect(mockOnCategoryChange).toHaveBeenCalledWith('coins')
  })

  it('should call onTimeRangeChange when time range is changed', async () => {
    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} />
      </TestWrapper>
    )

    const timeRangeSelect = screen.getByDisplayValue('All Time')
    fireEvent.click(timeRangeSelect)

    // Select weekly option
    const weeklyOption = screen.getByText('Weekly')
    fireEvent.click(weeklyOption)

    expect(mockOnTimeRangeChange).toHaveBeenCalledWith('weekly')
  })

  it('should call onRefresh when refresh button is clicked', () => {
    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} />
      </TestWrapper>
    )

    const refreshButton = screen.getByText('Refresh')
    fireEvent.click(refreshButton)

    expect(mockOnRefresh).toHaveBeenCalled()
  })

  it('should show loading state on refresh button', () => {
    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} loading={true} />
      </TestWrapper>
    )

    const refreshButton = screen.getByText('Refresh')
    expect(refreshButton.closest('button')).toHaveClass('ant-btn-loading')
  })

  it('should handle empty point categories', () => {
    const { usePointCategories } = vi.mocked(require('@/hooks/useEntities'))
    usePointCategories.mockReturnValue({
      data: [],
    })

    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} />
      </TestWrapper>
    )

    // Should still render without errors
    expect(screen.getByText('Leaderboard Type')).toBeInTheDocument()
  })

  it('should handle undefined point categories', () => {
    const { usePointCategories } = vi.mocked(require('@/hooks/useEntities'))
    usePointCategories.mockReturnValue({
      data: undefined,
    })

    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} />
      </TestWrapper>
    )

    // Should still render without errors
    expect(screen.getByText('Leaderboard Type')).toBeInTheDocument()
  })

  it('should allow clearing category selection', async () => {
    render(
      <TestWrapper>
        <LeaderboardSelector {...defaultProps} />
      </TestWrapper>
    )

    const categorySelect = screen.getByDisplayValue('Experience Points (xp)')
    fireEvent.click(categorySelect)

    // Look for clear button (X icon)
    const clearButton = screen.getByRole('button', { name: /clear/i })
    fireEvent.click(clearButton)

    expect(mockOnCategoryChange).toHaveBeenCalledWith(undefined)
  })
})
