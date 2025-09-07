import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { rulesApi, entitiesApi } from '@/api/rules'
import type {
  Rule,
  CreateRule,
  UpdateRule,
  Badge,
  Trophy,
  PointCategory,
  Level,
  RulesListParams,
} from '@/types'

// Query keys
export const rulesKeys = {
  all: ['rules'] as const,
  lists: () => [...rulesKeys.all, 'list'] as const,
  list: (params?: RulesListParams) => [...rulesKeys.lists(), params] as const,
  details: () => [...rulesKeys.all, 'detail'] as const,
  detail: (id: string) => [...rulesKeys.details(), id] as const,
  active: () => [...rulesKeys.all, 'active'] as const,
  byTrigger: (eventType: string) => [...rulesKeys.all, 'trigger', eventType] as const,
}

export const entitiesKeys = {
  all: ['entities'] as const,
  pointCategories: () => [...entitiesKeys.all, 'point-categories'] as const,
  badges: () => [...entitiesKeys.all, 'badges'] as const,
  trophies: () => [...entitiesKeys.all, 'trophies'] as const,
  levels: () => [...entitiesKeys.all, 'levels'] as const,
}

// Rules hooks
export const useRules = (params?: RulesListParams) => {
  return useQuery({
    queryKey: rulesKeys.list(params),
    queryFn: () => rulesApi.getAllRules(),
    select: (data) => {
      if (!params?.filters) return data

      let filtered = data

      // Filter by active status
      if (params.filters.isActive !== undefined) {
        filtered = filtered.filter(rule => rule.isActive === params.filters.isActive)
      }

      // Filter by trigger type
      if (params.filters.triggerType) {
        filtered = filtered.filter(rule => 
          rule.triggers.includes(params.filters.triggerType!)
        )
      }

      // Search by name or description
      if (params.filters.search) {
        const searchLower = params.filters.search.toLowerCase()
        filtered = filtered.filter(rule => 
          rule.name.toLowerCase().includes(searchLower) ||
          rule.description.toLowerCase().includes(searchLower) ||
          rule.id.toLowerCase().includes(searchLower)
        )
      }

      return filtered
    },
  })
}

export const useActiveRules = () => {
  return useQuery({
    queryKey: rulesKeys.active(),
    queryFn: () => rulesApi.getActiveRules(),
  })
}

export const useRulesByTrigger = (eventType: string) => {
  return useQuery({
    queryKey: rulesKeys.byTrigger(eventType),
    queryFn: () => rulesApi.getRulesByTrigger(eventType),
    enabled: !!eventType,
  })
}

export const useRule = (id: string) => {
  return useQuery({
    queryKey: rulesKeys.detail(id),
    queryFn: () => rulesApi.getRuleById(id),
    enabled: !!id,
  })
}

// Rules mutations
export const useCreateRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (rule: CreateRule) => rulesApi.createRule(rule),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.all })
    },
  })
}

export const useUpdateRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, rule }: { id: string; rule: UpdateRule }) =>
      rulesApi.updateRule(id, rule),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.all })
      queryClient.invalidateQueries({ queryKey: rulesKeys.detail(id) })
    },
  })
}

export const useActivateRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => rulesApi.activateRule(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.all })
    },
  })
}

export const useDeactivateRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => rulesApi.deactivateRule(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.all })
    },
  })
}

export const useDeleteRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => rulesApi.deleteRule(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.all })
    },
  })
}

// Entity hooks
export const usePointCategories = () => {
  return useQuery({
    queryKey: entitiesKeys.pointCategories(),
    queryFn: () => entitiesApi.getPointCategories(),
  })
}

export const usePointCategory = (id: string) => {
  return useQuery({
    queryKey: [...entitiesKeys.pointCategories(), id],
    queryFn: () => entitiesApi.getPointCategoryById(id),
    enabled: !!id,
  })
}

export const useBadges = () => {
  return useQuery({
    queryKey: entitiesKeys.badges(),
    queryFn: () => entitiesApi.getBadges(),
  })
}

export const useBadge = (id: string) => {
  return useQuery({
    queryKey: [...entitiesKeys.badges(), id],
    queryFn: () => entitiesApi.getBadgeById(id),
    enabled: !!id,
  })
}

export const useTrophies = () => {
  return useQuery({
    queryKey: entitiesKeys.trophies(),
    queryFn: () => entitiesApi.getTrophies(),
  })
}

export const useTrophy = (id: string) => {
  return useQuery({
    queryKey: [...entitiesKeys.trophies(), id],
    queryFn: () => entitiesApi.getTrophyById(id),
    enabled: !!id,
  })
}

export const useLevels = () => {
  return useQuery({
    queryKey: entitiesKeys.levels(),
    queryFn: () => entitiesApi.getLevels(),
  })
}

export const useLevel = (id: string) => {
  return useQuery({
    queryKey: [...entitiesKeys.levels(), id],
    queryFn: () => entitiesApi.getLevelById(id),
    enabled: !!id,
  })
}
