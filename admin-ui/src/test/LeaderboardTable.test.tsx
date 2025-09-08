import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { ConfigProvider } from 'antd'
import LeaderboardTable from '../components/leaderboards/LeaderboardTable'
import type { LeaderboardDto } from '../api/generated/models'
import type { LeaderboardType } from '../api/leaderboards'

// Mock Ant Design icons
vi.mock('@ant-design/icons', () => ({
  TrophyOutlined: () => <span data-testid="trophy-icon" />,
  CrownOutlined: () => <span data-testid="crown-icon" />,
  StarOutlined: () => <span data-testid="star-icon" />,
  NumberOutlined: () => <span data-testid="number-icon" />,
}))

const mockLeaderboardData: LeaderboardDto = {
  query: {
    type: 'points',
    category: 'xp',
    timeRange: 'alltime',
    page: 1,
    pageSize: 50,
  },
  entries: [
    {
      userId: 'user1',
      points: 1500,
      rank: 1,
      displayName: 'Top Player',
    },
    {
      userId: 'user2',
      points: 1200,
      rank: 2,
      displayName: 'Second Player',
    },
    {
      userId: 'user3',
      points: 1000,
      rank: 3,
      displayName: 'Third Player',
    },
    {
      userId: 'user4',
      points: 800,
      rank: 4,
      displayName: 'Fourth Player',
    },
  ],
  totalCount: 4,
  totalPages: 1,
  currentPage: 1,
  pageSize: 50,
  hasNextPage: false,
  hasPreviousPage: false,
  topEntry: {
    userId: 'user1',
    points: 1500,
    rank: 1,
    displayName: 'Top Player',
  },
}

const TestWrapper: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <ConfigProvider>{children}</ConfigProvider>
)

describe('LeaderboardTable', () => {
  it('should render leaderboard table with correct data', () => {
    render(
      <TestWrapper>
        <LeaderboardTable data={mockLeaderboardData} type="points" />
      </TestWrapper>
    )

    // Check if table headers are present
    expect(screen.getByText('Rank')).toBeInTheDocument()
    expect(screen.getByText('User')).toBeInTheDocument()
    expect(screen.getByText('Points')).toBeInTheDocument()

    // Check if entries are rendered
    expect(screen.getByText('Top Player')).toBeInTheDocument()
    expect(screen.getByText('Second Player')).toBeInTheDocument()
    expect(screen.getByText('Third Player')).toBeInTheDocument()
    expect(screen.getByText('Fourth Player')).toBeInTheDocument()

    // Check if ranks are displayed
    expect(screen.getByText('#1')).toBeInTheDocument()
    expect(screen.getByText('#2')).toBeInTheDocument()
    expect(screen.getByText('#3')).toBeInTheDocument()
    expect(screen.getByText('#4')).toBeInTheDocument()

    // Check if points are formatted correctly
    expect(screen.getByText('1,500')).toBeInTheDocument()
    expect(screen.getByText('1,200')).toBeInTheDocument()
    expect(screen.getByText('1,000')).toBeInTheDocument()
    expect(screen.getByText('800')).toBeInTheDocument()
  })

  it('should display correct rank icons', () => {
    render(
      <TestWrapper>
        <LeaderboardTable data={mockLeaderboardData} type="points" />
      </TestWrapper>
    )

    // Check for crown icon for rank 1
    expect(screen.getByTestId('crown-icon')).toBeInTheDocument()

    // Check for trophy icons for ranks 2 and 3
    const trophyIcons = screen.getAllByTestId('trophy-icon')
    expect(trophyIcons).toHaveLength(2)

    // Check for number icon for rank 4
    const numberIcons = screen.getAllByTestId('number-icon')
    expect(numberIcons).toHaveLength(1)
  })

  it('should render badges leaderboard with correct formatting', () => {
    const badgesData: LeaderboardDto = {
      ...mockLeaderboardData,
      entries: [
        {
          userId: 'user1',
          points: 5,
          rank: 1,
          displayName: 'Badge Collector',
        },
        {
          userId: 'user2',
          points: 3,
          rank: 2,
          displayName: 'Badge Enthusiast',
        },
      ],
    }

    render(
      <TestWrapper>
        <LeaderboardTable data={badgesData} type="badges" />
      </TestWrapper>
    )

    expect(screen.getByText('Badges')).toBeInTheDocument()
    expect(screen.getByText('5 badges')).toBeInTheDocument()
    expect(screen.getByText('3 badges')).toBeInTheDocument()
  })

  it('should render trophies leaderboard with correct formatting', () => {
    const trophiesData: LeaderboardDto = {
      ...mockLeaderboardData,
      entries: [
        {
          userId: 'user1',
          points: 2,
          rank: 1,
          displayName: 'Trophy Master',
        },
        {
          userId: 'user2',
          points: 1,
          rank: 2,
          displayName: 'Trophy Hunter',
        },
      ],
    }

    render(
      <TestWrapper>
        <LeaderboardTable data={trophiesData} type="trophies" />
      </TestWrapper>
    )

    expect(screen.getByText('Trophies')).toBeInTheDocument()
    expect(screen.getByText('2 trophies')).toBeInTheDocument()
    expect(screen.getByText('1 trophy')).toBeInTheDocument()
  })

  it('should render levels leaderboard with correct formatting', () => {
    const levelsData: LeaderboardDto = {
      ...mockLeaderboardData,
      entries: [
        {
          userId: 'user1',
          points: 5,
          rank: 1,
          displayName: 'Level Master',
        },
        {
          userId: 'user2',
          points: 3,
          rank: 2,
          displayName: 'Level Up',
        },
      ],
    }

    render(
      <TestWrapper>
        <LeaderboardTable data={levelsData} type="level" />
      </TestWrapper>
    )

    expect(screen.getByText('Level')).toBeInTheDocument()
    expect(screen.getByText('Level 5')).toBeInTheDocument()
    expect(screen.getByText('Level 3')).toBeInTheDocument()
  })

  it('should show loading state', () => {
    render(
      <TestWrapper>
        <LeaderboardTable
          data={mockLeaderboardData}
          type="points"
          loading={true}
        />
      </TestWrapper>
    )

    // The loading spinner should be present
    expect(screen.getByRole('table')).toBeInTheDocument()
  })

  it('should handle empty leaderboard data', () => {
    const emptyData: LeaderboardDto = {
      ...mockLeaderboardData,
      entries: [],
    }

    render(
      <TestWrapper>
        <LeaderboardTable data={emptyData} type="points" />
      </TestWrapper>
    )

    // Table should still render but with no data rows
    expect(screen.getByText('Rank')).toBeInTheDocument()
    expect(screen.getByText('User')).toBeInTheDocument()
    expect(screen.getByText('Points')).toBeInTheDocument()
  })

  it('should display user IDs when display names are not available', () => {
    const dataWithoutDisplayNames: LeaderboardDto = {
      ...mockLeaderboardData,
      entries: [
        {
          userId: 'user1',
          points: 1500,
          rank: 1,
          displayName: null,
        },
      ],
    }

    render(
      <TestWrapper>
        <LeaderboardTable data={dataWithoutDisplayNames} type="points" />
      </TestWrapper>
    )

    // Check that user1 appears in both the display name and user ID fields
    const user1Elements = screen.getAllByText('user1')
    expect(user1Elements).toHaveLength(2) // Display name and user ID
  })
})
