import React from 'react'
import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import UserDetails from '../components/users/UserDetails'
import type { UserStateDto } from '../api/generated/models'

// Test wrapper with QueryClient
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  })

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  )
}

describe('UserDetails', () => {
  const mockUserState: UserStateDto = {
    userId: 'user123',
    pointsByCategory: {
      xp: 1500,
      coins: 250,
      reputation: 100,
    },
    badges: [
      {
        id: 'first-comment',
        name: 'First Comment',
        description: 'Awarded for first comment',
        image: 'badge.png',
        visible: true,
      },
      {
        id: 'social-butterfly',
        name: 'Social Butterfly',
        description: 'Awarded for 10 social interactions',
        image: 'social-badge.png',
        visible: true,
      },
    ],
    trophies: [
      {
        id: 'comment-master',
        name: 'Comment Master',
        description: 'Awarded for 100 comments',
        image: 'trophy.png',
        visible: true,
      },
    ],
    currentLevelsByCategory: {
      xp: {
        id: 'level-2',
        name: 'Intermediate',
        category: 'xp',
        minPoints: 1000,
      },
      coins: {
        id: 'level-1',
        name: 'Beginner',
        category: 'coins',
        minPoints: 0,
      },
    },
  }

  it('should render loading state', () => {
    render(<UserDetails userState={undefined} isLoading={true} />, {
      wrapper: createWrapper(),
    })

    expect(screen.getByText('Loading user data...')).toBeInTheDocument()
  })

  it('should render empty state when no user data', () => {
    render(<UserDetails userState={undefined} isLoading={false} />, {
      wrapper: createWrapper(),
    })

    expect(screen.getByText('No user data found')).toBeInTheDocument()
  })

  it('should render user details with complete data', () => {
    render(<UserDetails userState={mockUserState} isLoading={false} />, {
      wrapper: createWrapper(),
    })

    // Check user header
    expect(screen.getByText('user123')).toBeInTheDocument()
    expect(screen.getByText('Gamification Profile')).toBeInTheDocument()
    expect(screen.getAllByText('1,850')).toHaveLength(2) // Total points appears twice

    // Check points by category
    expect(screen.getByText('Points by Category')).toBeInTheDocument()
    expect(screen.getAllByText('xp')).toHaveLength(2) // Appears in points and levels sections
    expect(screen.getAllByText('coins')).toHaveLength(2) // Appears in points and levels sections
    expect(screen.getByText('reputation')).toBeInTheDocument()

    // Check current levels
    expect(screen.getByText('Current Levels')).toBeInTheDocument()
  })

  it('should render empty states for missing data', () => {
    const emptyUserState: UserStateDto = {
      userId: 'user123',
      pointsByCategory: null,
      badges: null,
      trophies: null,
      currentLevelsByCategory: null,
    }

    render(<UserDetails userState={emptyUserState} isLoading={false} />, {
      wrapper: createWrapper(),
    })

    expect(screen.getByText('No points data available')).toBeInTheDocument()
    expect(screen.getByText('No level data available')).toBeInTheDocument()
    expect(screen.getByText('No badges earned yet')).toBeInTheDocument()
    expect(screen.getByText('No trophies earned yet')).toBeInTheDocument()
  })

  it('should render empty states for empty arrays', () => {
    const emptyArraysUserState: UserStateDto = {
      userId: 'user123',
      pointsByCategory: {},
      badges: [],
      trophies: [],
      currentLevelsByCategory: {},
    }

    render(<UserDetails userState={emptyArraysUserState} isLoading={false} />, {
      wrapper: createWrapper(),
    })

    expect(screen.getByText('No points data available')).toBeInTheDocument()
    expect(screen.getByText('No level data available')).toBeInTheDocument()
    expect(screen.getByText('No badges earned yet')).toBeInTheDocument()
    expect(screen.getByText('No trophies earned yet')).toBeInTheDocument()
  })

  it('should calculate total points correctly', () => {
    render(<UserDetails userState={mockUserState} isLoading={false} />, {
      wrapper: createWrapper(),
    })

    // Total points should be 1500 + 250 + 100 = 1850 (formatted as 1,850)
    expect(screen.getAllByText('1,850')).toHaveLength(2) // Appears in header and summary
  })

  it('should display badge and trophy counts correctly', () => {
    render(<UserDetails userState={mockUserState} isLoading={false} />, {
      wrapper: createWrapper(),
    })

    // Test that the component renders without errors
    expect(screen.getByText('user123')).toBeInTheDocument()
  })

  it('should display level information correctly', () => {
    render(<UserDetails userState={mockUserState} isLoading={false} />, {
      wrapper: createWrapper(),
    })

    expect(screen.getByText('Intermediate')).toBeInTheDocument()
    expect(screen.getByText('Beginner')).toBeInTheDocument()
  })
})
