import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import type { IngestEventRequest } from '@/api/generated/models'
import { eventsApi } from '@/api/events'

// Query keys
export const eventsKeys = {
  all: ['events'] as const,
  catalog: () => [...eventsKeys.all, 'catalog'] as const,
  details: () => [...eventsKeys.all, 'detail'] as const,
  detail: (id: string) => [...eventsKeys.details(), id] as const,
  byType: (eventType: string, limit?: number, offset?: number) =>
    [...eventsKeys.all, 'type', eventType, { limit, offset }] as const,
  byUser: (userId: string, limit?: number, offset?: number) =>
    [...eventsKeys.all, 'user', userId, { limit, offset }] as const,
}

// Event catalog hooks
export const useEventCatalog = () => {
  return useQuery({
    queryKey: eventsKeys.catalog(),
    queryFn: async () => {
      return await eventsApi.getEventCatalog()
    },
  })
}

// Event detail hooks
export const useEvent = (id: string) => {
  return useQuery({
    queryKey: eventsKeys.detail(id),
    queryFn: async () => {
      return await eventsApi.getEventById(id)
    },
    enabled: !!id,
  })
}

// Events by type
export const useEventsByType = (
  eventType: string,
  limit?: number,
  offset?: number
) => {
  return useQuery({
    queryKey: eventsKeys.byType(eventType, limit, offset),
    queryFn: async () => {
      return await eventsApi.getEventsByType(eventType, limit, offset)
    },
    enabled: !!eventType,
  })
}

// Events by user
export const useEventsByUser = (
  userId: string,
  limit?: number,
  offset?: number
) => {
  return useQuery({
    queryKey: eventsKeys.byUser(userId, limit, offset),
    queryFn: async () => {
      return await eventsApi.getEventsByUser(userId, limit, offset)
    },
    enabled: !!userId,
  })
}

// Event mutations
export const useIngestEvent = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (event: IngestEventRequest) => {
      return await eventsApi.ingestEvent(event)
    },
    onSuccess: () => {
      // Invalidate all event queries since we don't know which specific ones to update
      queryClient.invalidateQueries({ queryKey: eventsKeys.all })
    },
  })
}
