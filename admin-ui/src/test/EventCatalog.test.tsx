import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactNode } from 'react'
import EventCatalog from '../components/EventCatalog'
import { useEventCatalog } from '../hooks/useEvents'

// Mock the hooks
vi.mock('../hooks/useEvents', () => ({
  useEventCatalog: vi.fn(),
}))

// Test wrapper with QueryClient
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  })

  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  )
}

describe('EventCatalog', () => {
  const mockEventDefinitions = [
    {
      id: 'USER_COMMENTED',
      description: 'User commented on a post',
      payloadSchema: {
        postId: 'string',
        commentText: 'string',
        userId: 'string',
      },
    },
    {
      id: 'USER_LIKED',
      description: 'User liked a post',
      payloadSchema: {
        postId: 'string',
        userId: 'string',
      },
    },
    {
      id: 'USER_SHARED',
      description: 'User shared a post',
      payloadSchema: null,
    },
  ]

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render loading state', () => {
    ;(useEventCatalog as any).mockReturnValue({
      data: [],
      isLoading: true,
      error: null,
    })

    render(<EventCatalog />, { wrapper: createWrapper() })

    expect(screen.getByRole('table')).toBeInTheDocument()
    // The table should show loading state
  })

  it('should render error state', () => {
    const error = new Error('Failed to load events')
    ;(useEventCatalog as any).mockReturnValue({
      data: [],
      isLoading: false,
      error,
    })

    render(<EventCatalog />, { wrapper: createWrapper() })

    expect(
      screen.getByText('Error loading event catalog: Failed to load events')
    ).toBeInTheDocument()
  })

  it('should render event catalog with stats', async () => {
    ;(useEventCatalog as any).mockReturnValue({
      data: mockEventDefinitions,
      isLoading: false,
      error: null,
    })

    render(<EventCatalog />, { wrapper: createWrapper() })

    // Check stats cards
    expect(screen.getAllByText('3')).toHaveLength(1) // Total events
    expect(screen.getByText('Total Events')).toBeInTheDocument()
    expect(screen.getAllByText('2')).toHaveLength(1) // With schema
    expect(screen.getByText('With Schema')).toBeInTheDocument()
    expect(screen.getAllByText('1')).toHaveLength(2) // Without schema (appears in table row numbers too)
    expect(screen.getByText('Without Schema')).toBeInTheDocument()

    // Check table headers
    expect(screen.getAllByText('Event ID')).toHaveLength(2) // Table header and expanded details
    expect(screen.getAllByText('Description')).toHaveLength(2) // Table header and expanded details
    expect(screen.getAllByText('Schema Status')).toHaveLength(2) // Table header and expanded details
    expect(screen.getAllByText('Schema Fields')).toHaveLength(2) // Table header and expanded details

    // Check event data
    expect(screen.getByText('USER_COMMENTED')).toBeInTheDocument()
    expect(screen.getByText('User commented on a post')).toBeInTheDocument()
    expect(screen.getByText('USER_LIKED')).toBeInTheDocument()
    expect(screen.getByText('User liked a post')).toBeInTheDocument()
    expect(screen.getByText('USER_SHARED')).toBeInTheDocument()
    expect(screen.getByText('User shared a post')).toBeInTheDocument()
  })

  it('should filter events by search text', async () => {
    ;(useEventCatalog as any).mockReturnValue({
      data: mockEventDefinitions,
      isLoading: false,
      error: null,
    })

    render(<EventCatalog />, { wrapper: createWrapper() })

    const searchInput = screen.getByPlaceholderText(
      'Search events by ID or description...'
    )

    // Search for "commented"
    fireEvent.change(searchInput, { target: { value: 'commented' } })

    await waitFor(() => {
      expect(screen.getByText('USER_COMMENTED')).toBeInTheDocument()
      expect(screen.queryByText('USER_LIKED')).not.toBeInTheDocument()
      expect(screen.queryByText('USER_SHARED')).not.toBeInTheDocument()
    })
  })

  it('should show schema fields correctly', async () => {
    ;(useEventCatalog as any).mockReturnValue({
      data: mockEventDefinitions,
      isLoading: false,
      error: null,
    })

    render(<EventCatalog />, { wrapper: createWrapper() })

    // Check schema status badges
    expect(screen.getAllByText('Defined')).toHaveLength(2)
    expect(screen.getByText('None')).toBeInTheDocument()

    // Check schema field tags
    expect(screen.getAllByText('postId: string')).toHaveLength(2) // Appears in both events with schemas
    expect(screen.getAllByText('commentText: string')).toHaveLength(1) // Appears in one event only
    expect(screen.getAllByText('userId: string')).toHaveLength(2) // Appears in both events with schemas
  })

  it('should expand rows to show details', async () => {
    ;(useEventCatalog as any).mockReturnValue({
      data: mockEventDefinitions,
      isLoading: false,
      error: null,
    })

    render(<EventCatalog />, { wrapper: createWrapper() })

    // Find and click the expand button (first row)
    const expandButtons = screen.getAllByRole('button', { name: /expand/i })
    fireEvent.click(expandButtons[0])

    await waitFor(() => {
      expect(screen.getByText('Event Details')).toBeInTheDocument()
      expect(screen.getAllByText('Event ID')).toHaveLength(2) // One in table header, one in expanded details
      expect(screen.getAllByText('Description')).toHaveLength(2) // One in table header, one in expanded details
      expect(screen.getAllByText('Schema Fields')).toHaveLength(2) // One in table header, one in expanded details
    })
  })

  it('should call onViewEvent when provided', async () => {
    const mockOnViewEvent = vi.fn()
    ;(useEventCatalog as any).mockReturnValue({
      data: mockEventDefinitions,
      isLoading: false,
      error: null,
    })

    render(<EventCatalog onViewEvent={mockOnViewEvent} />, {
      wrapper: createWrapper(),
    })

    // Find and click the "View Details" link
    const viewLinks = screen.getAllByText('View Details')
    fireEvent.click(viewLinks[0])

    expect(mockOnViewEvent).toHaveBeenCalledWith(mockEventDefinitions[0])
  })

  it('should handle empty event catalog', async () => {
    ;(useEventCatalog as any).mockReturnValue({
      data: [],
      isLoading: false,
      error: null,
    })

    render(<EventCatalog />, { wrapper: createWrapper() })

    expect(screen.getAllByText('0')).toHaveLength(3) // Total events, With schema, Without schema
    expect(screen.getByText('Total Events')).toBeInTheDocument()
    expect(screen.getByText('With Schema')).toBeInTheDocument()
    expect(screen.getByText('Without Schema')).toBeInTheDocument()
  })
})
