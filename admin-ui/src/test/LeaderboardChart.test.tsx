import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { ConfigProvider } from 'antd'
import LeaderboardChart from '../components/leaderboards/LeaderboardChart'
import type { LeaderboardDto } from '../api/generated/models'
import type { LeaderboardType } from '../api/leaderboards'

// Mock Ant Design icons
vi.mock('@ant-design/icons', () => ({
  TrendingUpOutlined: () => <span data-testid="trending-icon" />,
  TrophyOutlined: () => <span data-testid="trophy-icon" />,
  UserOutlined: () => <span data-testid="user-icon" />,
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
  ],
  totalCount: 25,
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

describe('LeaderboardChart', () => {
  it('should render leaderboard statistics for points', () => {
    render(
      <TestWrapper>
        <LeaderboardChart data={mockLeaderboardData} type="points" />
      </TestWrapper>
    )

    // Check if all statistics are displayed
    expect(screen.getByText('Total Participants')).toBeInTheDocument()
    expect(screen.getByText('25')).toBeInTheDocument()

    expect(screen.getByText('Top Points')).toBeInTheDocument()
    expect(screen.getByText('1,500')).toBeInTheDocument()

    expect(screen.getByText('Leader')).toBeInTheDocument()
    expect(screen.getByText('Top Player')).toBeInTheDocument()
  })

  it('should render leaderboard statistics for badges', () => {
    const badgesData: LeaderboardDto = {
      ...mockLeaderboardData,
      entries: [
        {
          userId: 'user1',
          points: 5,
          rank: 1,
          displayName: 'Badge Collector',
        },
      ],
      topEntry: {
        userId: 'user1',
        points: 5,
        rank: 1,
        displayName: 'Badge Collector',
      },
    }

    render(
      <TestWrapper>
        <LeaderboardChart data={badgesData} type="badges" />
      </TestWrapper>
    )

    expect(screen.getByText('Top Badges')).toBeInTheDocument()
    expect(screen.getByText('5 badges')).toBeInTheDocument()
    expect(screen.getByText('Badge Collector')).toBeInTheDocument()
  })

  it('should render leaderboard statistics for trophies', () => {
    const trophiesData: LeaderboardDto = {
      ...mockLeaderboardData,
      entries: [
        {
          userId: 'user1',
          points: 3,
          rank: 1,
          displayName: 'Trophy Master',
        },
      ],
      topEntry: {
        userId: 'user1',
        points: 3,
        rank: 1,
        displayName: 'Trophy Master',
      },
    }

    render(
      <TestWrapper>
        <LeaderboardChart data={trophiesData} type="trophies" />
      </TestWrapper>
    )

    expect(screen.getByText('Top Trophies')).toBeInTheDocument()
    expect(screen.getByText('3 trophies')).toBeInTheDocument()
    expect(screen.getByText('Trophy Master')).toBeInTheDocument()
  })

  it('should render leaderboard statistics for levels', () => {
    const levelsData: LeaderboardDto = {
      ...mockLeaderboardData,
      entries: [
        {
          userId: 'user1',
          points: 5,
          rank: 1,
          displayName: 'Level Master',
        },
      ],
      topEntry: {
        userId: 'user1',
        points: 5,
        rank: 1,
        displayName: 'Level Master',
      },
    }

    render(
      <TestWrapper>
        <LeaderboardChart data={levelsData} type="level" />
      </TestWrapper>
    )

    expect(screen.getByText('Top Level')).toBeInTheDocument()
    expect(screen.getByText('Level 5')).toBeInTheDocument()
    expect(screen.getByText('Level Master')).toBeInTheDocument()
  })

  it('should display correct icons', () => {
    render(
      <TestWrapper>
        <LeaderboardChart data={mockLeaderboardData} type="points" />
      </TestWrapper>
    )

    expect(screen.getByTestId('user-icon')).toBeInTheDocument()
    expect(screen.getByTestId('trophy-icon')).toBeInTheDocument()
    expect(screen.getByTestId('trending-icon')).toBeInTheDocument()
  })

  it('should handle missing top entry', () => {
    const dataWithoutTopEntry: LeaderboardDto = {
      ...mockLeaderboardData,
      topEntry: undefined,
    }

    render(
      <TestWrapper>
        <LeaderboardChart data={dataWithoutTopEntry} type="points" />
      </TestWrapper>
    )

    expect(screen.getByText('Leader')).toBeInTheDocument()
    expect(screen.getByText('N/A')).toBeInTheDocument()
  })

  it('should handle zero total count', () => {
    const emptyData: LeaderboardDto = {
      ...mockLeaderboardData,
      totalCount: 0,
      entries: [],
      topEntry: undefined,
    }

    render(
      <TestWrapper>
        <LeaderboardChart data={emptyData} type="points" />
      </TestWrapper>
    )

    expect(screen.getByText('Total Participants')).toBeInTheDocument()
    expect(screen.getAllByText('0')).toHaveLength(2) // Total count and top points
    expect(screen.getByText('Top Points')).toBeInTheDocument()
  })

  it('should format single vs plural correctly for badges', () => {
    const singleBadgeData: LeaderboardDto = {
      ...mockLeaderboardData,
      topEntry: {
        userId: 'user1',
        points: 1,
        rank: 1,
        displayName: 'First Badge',
      },
    }

    render(
      <TestWrapper>
        <LeaderboardChart data={singleBadgeData} type="badges" />
      </TestWrapper>
    )

    expect(screen.getByText('1 badge')).toBeInTheDocument()
  })

  it('should format single vs plural correctly for trophies', () => {
    const singleTrophyData: LeaderboardDto = {
      ...mockLeaderboardData,
      topEntry: {
        userId: 'user1',
        points: 1,
        rank: 1,
        displayName: 'First Trophy',
      },
    }

    render(
      <TestWrapper>
        <LeaderboardChart data={singleTrophyData} type="trophies" />
      </TestWrapper>
    )

    expect(screen.getByText('1 trophy')).toBeInTheDocument()
  })
})
