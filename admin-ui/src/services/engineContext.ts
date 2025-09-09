import { useQuery } from '@tanstack/react-query'
import { eventsApi } from '@/api/events'
import type { EventDefinitionDto } from '@/api/generated/models'
import { rulesApi } from '@/api/rules'
import { usersApi } from '@/api/users'
import {
  badgesApi,
  trophiesApi,
  levelsApi,
  pointCategoriesApi,
} from '@/api/entities'

export interface EngineContext {
  eventCatalog: EventDefinitionDto[]
  eventTypes: string[]
  rules: Array<{
    id: string
    name: string
    triggers: string[]
    isActive: boolean
  }>
  users: Array<{
    id: string
    name?: string
  }>
  entities: {
    badges: Array<{ id: string; name: string }>
    trophies: Array<{ id: string; name: string }>
    levels: Array<{ id: string; name: string }>
    pointCategories: Array<{ id: string; name: string }>
  }
}

// Hook to fetch engine context data
export const useEngineContext = () => {
  // Fetch event catalog
  const eventCatalogQuery = useQuery({
    queryKey: ['eventCatalog'],
    queryFn: eventsApi.getEventCatalog,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })

  // Fetch rules
  const rulesQuery = useQuery({
    queryKey: ['rules'],
    queryFn: rulesApi.getAllRules,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })

  // Fetch users (summaries) for suggestions
  const usersListQuery = useQuery({
    queryKey: ['users', 'list', 1, 200],
    queryFn: async () => usersApi.getUsers(1, 200),
    staleTime: 5 * 60 * 1000, // 5 minutes
  })

  // Fetch entities
  const badgesQuery = useQuery({
    queryKey: ['badges'],
    queryFn: badgesApi.getAllBadges,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })

  const trophiesQuery = useQuery({
    queryKey: ['trophies'],
    queryFn: trophiesApi.getAllTrophies,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })

  const levelsQuery = useQuery({
    queryKey: ['levels'],
    queryFn: levelsApi.getAllLevels,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })

  const pointCategoriesQuery = useQuery({
    queryKey: ['pointCategories'],
    queryFn: pointCategoriesApi.getAllPointCategories,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })

  const isLoading =
    eventCatalogQuery.isLoading ||
    rulesQuery.isLoading ||
    usersListQuery.isLoading ||
    badgesQuery.isLoading ||
    trophiesQuery.isLoading ||
    levelsQuery.isLoading ||
    pointCategoriesQuery.isLoading

  const error =
    eventCatalogQuery.error ||
    rulesQuery.error ||
    usersListQuery.error ||
    badgesQuery.error ||
    trophiesQuery.error ||
    levelsQuery.error ||
    pointCategoriesQuery.error

  const context: EngineContext = {
    eventCatalog: eventCatalogQuery.data || [],
    eventTypes: eventCatalogQuery.data?.map((event) => event.id || '') || [],
    rules:
      rulesQuery.data?.map((rule) => ({
        id: rule.id || '',
        name: rule.name || '',
        triggers: rule.triggers || [],
        isActive: rule.isActive || false,
      })) || [],
    users:
      usersListQuery.data?.users?.map((u) => ({
        id: u.userId || '',
        name: undefined,
      })) || [],
    entities: {
      badges:
        badgesQuery.data?.map((badge) => ({
          id: badge.id || '',
          name: badge.name || '',
        })) || [],
      trophies:
        trophiesQuery.data?.map((trophy) => ({
          id: trophy.id || '',
          name: trophy.name || '',
        })) || [],
      levels:
        levelsQuery.data?.map((level) => ({
          id: level.id || '',
          name: level.name || '',
        })) || [],
      pointCategories:
        pointCategoriesQuery.data?.map((category) => ({
          id: category.id || '',
          name: category.name || '',
        })) || [],
    },
  }

  return {
    context,
    isLoading,
    error,
    refetch: () => {
      eventCatalogQuery.refetch()
      rulesQuery.refetch()
      badgesQuery.refetch()
      trophiesQuery.refetch()
      levelsQuery.refetch()
      pointCategoriesQuery.refetch()
    },
  }
}

// Utility functions for working with engine context
export const engineContextUtils = {
  // Get event type suggestions based on current rules
  getEventTypeSuggestions: (context: EngineContext): string[] => {
    const ruleEventTypes = context.rules
      .filter((rule) => rule.isActive)
      .flatMap((rule) => rule.triggers)
      .filter((type, index, arr) => arr.indexOf(type) === index) // Remove duplicates

    const catalogEventTypes = context.eventTypes

    // Combine and deduplicate
    return [...new Set([...ruleEventTypes, ...catalogEventTypes])].sort()
  },

  // Get payload schema map for a given event ID
  getPayloadSchema: (
    context: EngineContext,
    eventId: string
  ): Record<string, string> => {
    const def = context.eventCatalog.find((e) => e.id === eventId)
    return (def?.payloadSchema as Record<string, string> | null) || {}
  },

  // Build default attributes from payload schema types
  buildAttributesFromSchema: (
    payloadSchema: Record<string, string>
  ): Record<string, unknown> => {
    const attrs: Record<string, unknown> = {}
    for (const [key, type] of Object.entries(payloadSchema)) {
      const lower = (type || '').toLowerCase()
      if (lower.includes('string')) {
        if (key.toLowerCase().endsWith('id')) attrs[key] = `${key}-123`
        else if (key.toLowerCase().includes('text')) attrs[key] = 'example text'
        else attrs[key] = `${key} value`
      } else if (
        lower.includes('number') ||
        lower.includes('int') ||
        lower.includes('float')
      ) {
        attrs[key] = 0
      } else if (lower.includes('bool')) {
        attrs[key] = false
      } else if (lower.includes('date')) {
        attrs[key] = new Date().toISOString()
      } else if (lower.includes('array') || lower.endsWith('[]')) {
        attrs[key] = []
      } else if (lower.includes('object') || lower.includes('map')) {
        attrs[key] = {}
      } else {
        attrs[key] = null
      }
    }
    return attrs
  },

  // Get user suggestions
  getUserSuggestions: (context: EngineContext): string[] => {
    return context.users.map((user) => user.id)
  },

  // Get entity suggestions for attributes
  getEntitySuggestions: (context: EngineContext) => {
    return {
      badges: context.entities.badges.map((badge) => ({
        id: badge.id,
        name: badge.name,
        type: 'badge',
      })),
      trophies: context.entities.trophies.map((trophy) => ({
        id: trophy.id,
        name: trophy.name,
        type: 'trophy',
      })),
      levels: context.entities.levels.map((level) => ({
        id: level.id,
        name: level.name,
        type: 'level',
      })),
      pointCategories: context.entities.pointCategories.map((category) => ({
        id: category.id,
        name: category.name,
        type: 'pointCategory',
      })),
    }
  },

  // Generate realistic event attributes based on event type and available entities
  generateEventAttributes: (eventType: string): Record<string, unknown> => {
    const baseAttributes: Record<string, unknown> = {
      timestamp: new Date().toISOString(),
      source: 'sandbox',
    }

    // Add type-specific attributes
    if (eventType.includes('login')) {
      baseAttributes.platform = 'web'
      baseAttributes.userAgent =
        'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
      baseAttributes.ipAddress = '192.168.1.100'
    } else if (eventType.includes('purchase')) {
      baseAttributes.amount = 29.99
      baseAttributes.currency = 'USD'
      baseAttributes.paymentMethod = 'credit_card'
    } else if (eventType.includes('achievement')) {
      baseAttributes.achievementType = 'streak'
      baseAttributes.value = 7
    }

    return baseAttributes
  },
}
