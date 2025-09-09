import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactNode } from 'react'
import {
  useEventCatalog,
  useEvent,
  useEventsByType,
  useEventsByUser,
  useIngestEvent,
} from '../hooks/useEvents'

// Create mock API instance
const mockApiInstance = {
  apiEventsCatalogGet: vi.fn(),
  apiEventsEventIdGet: vi.fn(),
  apiEventsTypeEventTypeGet: vi.fn(),
  apiEventsUserUserIdGet: vi.fn(),
  apiEventsPost: vi.fn(),
}

// Mock the generated API
vi.mock('../api/generated-client', () => ({
  EventsApi: vi.fn(),
  EventsApiInstance: vi.fn(() => mockApiInstance),
  createApiInstance: vi.fn(() => mockApiInstance),
}))

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

  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  )
}

describe('useEvents hooks', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('Query hooks', () => {
    it('should fetch event catalog', async () => {
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
      ]

      mockApiInstance.apiEventsCatalogGet.mockResolvedValue({
        data: mockEventDefinitions,
      })

      const { result } = renderHook(() => useEventCatalog(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockEventDefinitions)
      expect(mockApiInstance.apiEventsCatalogGet).toHaveBeenCalled()
    })

    it('should fetch single event by ID', async () => {
      const mockEvent = {
        id: 'event-123',
        eventType: 'USER_COMMENTED',
        userId: 'user-456',
        timestamp: '2023-01-01T00:00:00Z',
        payload: {
          postId: 'post-789',
          commentText: 'Great post!',
          userId: 'user-456',
        },
        metadata: {
          source: 'web',
          version: '1.0',
        },
      }

      mockApiInstance.apiEventsEventIdGet.mockResolvedValue({
        data: mockEvent,
      })

      const { result } = renderHook(() => useEvent('event-123'), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockEvent)
      expect(mockApiInstance.apiEventsEventIdGet).toHaveBeenCalledWith(
        'event-123'
      )
    })

    it('should fetch events by type', async () => {
      const mockEvents = [
        {
          id: 'event-1',
          eventType: 'USER_COMMENTED',
          userId: 'user-1',
          timestamp: '2023-01-01T00:00:00Z',
          payload: { postId: 'post-1', commentText: 'Comment 1' },
        },
        {
          id: 'event-2',
          eventType: 'USER_COMMENTED',
          userId: 'user-2',
          timestamp: '2023-01-01T01:00:00Z',
          payload: { postId: 'post-2', commentText: 'Comment 2' },
        },
      ]

      mockApiInstance.apiEventsTypeEventTypeGet.mockResolvedValue({
        data: mockEvents,
      })

      const { result } = renderHook(
        () => useEventsByType('USER_COMMENTED', 10, 0),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockEvents)
      expect(mockApiInstance.apiEventsTypeEventTypeGet).toHaveBeenCalledWith(
        'USER_COMMENTED',
        10,
        0
      )
    })

    it('should fetch events by user', async () => {
      const mockEvents = [
        {
          id: 'event-1',
          eventType: 'USER_COMMENTED',
          userId: 'user-123',
          timestamp: '2023-01-01T00:00:00Z',
          payload: { postId: 'post-1', commentText: 'Comment 1' },
        },
        {
          id: 'event-2',
          eventType: 'USER_LIKED',
          userId: 'user-123',
          timestamp: '2023-01-01T01:00:00Z',
          payload: { postId: 'post-2' },
        },
      ]

      mockApiInstance.apiEventsUserUserIdGet.mockResolvedValue({
        data: mockEvents,
      })

      const { result } = renderHook(() => useEventsByUser('user-123', 10, 0), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockEvents)
      expect(mockApiInstance.apiEventsUserUserIdGet).toHaveBeenCalledWith(
        'user-123',
        10,
        0
      )
    })

    it('should not fetch event when ID is empty', () => {
      const { result } = renderHook(() => useEvent(''), {
        wrapper: createWrapper(),
      })

      expect(result.current.isLoading).toBe(false)
      expect(mockApiInstance.apiEventsEventIdGet).not.toHaveBeenCalled()
    })

    it('should not fetch events by type when eventType is empty', () => {
      const { result } = renderHook(() => useEventsByType(''), {
        wrapper: createWrapper(),
      })

      expect(result.current.isLoading).toBe(false)
      expect(mockApiInstance.apiEventsTypeEventTypeGet).not.toHaveBeenCalled()
    })

    it('should not fetch events by user when userId is empty', () => {
      const { result } = renderHook(() => useEventsByUser(''), {
        wrapper: createWrapper(),
      })

      expect(result.current.isLoading).toBe(false)
      expect(mockApiInstance.apiEventsUserUserIdGet).not.toHaveBeenCalled()
    })
  })

  describe('Mutation hooks', () => {
    it('should ingest a new event', async () => {
      const newEvent = {
        eventType: 'USER_COMMENTED',
        userId: 'user-123',
        payload: {
          postId: 'post-456',
          commentText: 'Great post!',
        },
        metadata: {
          source: 'web',
          version: '1.0',
        },
      }

      const createdEvent = {
        id: 'event-789',
        ...newEvent,
        timestamp: '2023-01-01T00:00:00Z',
      }

      mockApiInstance.apiEventsPost.mockResolvedValue({
        data: createdEvent,
      })

      const { result } = renderHook(() => useIngestEvent(), {
        wrapper: createWrapper(),
      })

      await result.current.mutateAsync(newEvent)

      expect(mockApiInstance.apiEventsPost).toHaveBeenCalledWith(newEvent)
    })

    it('should handle ingest event error', async () => {
      const newEvent = {
        eventType: 'USER_COMMENTED',
        userId: 'user-123',
        payload: {
          postId: 'post-456',
          commentText: 'Great post!',
        },
      }

      const error = new Error('Failed to ingest event')
      mockApiInstance.apiEventsPost.mockRejectedValue(error)

      const { result } = renderHook(() => useIngestEvent(), {
        wrapper: createWrapper(),
      })

      await expect(result.current.mutateAsync(newEvent)).rejects.toThrow(error)
    })
  })
})
