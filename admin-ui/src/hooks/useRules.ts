import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { RulesApi, createApiInstance } from '@/api/generated-client'
import type { CreateRuleDto, UpdateRuleDto } from '@/api/generated/models'
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

// Create API instance
const rulesApi = createApiInstance(RulesApi)

// Query keys
export const rulesKeys = {
  all: ['rules'] as const,
  lists: () => [...rulesKeys.all, 'list'] as const,
  list: (params?: RulesListParams) => [...rulesKeys.lists(), params] as const,
  details: () => [...rulesKeys.all, 'detail'] as const,
  detail: (id: string) => [...rulesKeys.details(), id] as const,
  active: () => [...rulesKeys.all, 'active'] as const,
  byTrigger: (eventType: string) =>
    [...rulesKeys.all, 'trigger', eventType] as const,
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
    queryFn: async () => {
      const response = await rulesApi.apiRulesGet()
      // The API returns RuleDto[], but we need to map it to our Rule type
      return response.data as any as Rule[]
    },
    select: (data) => {
      if (!params?.filters) return data

      let filtered = data

      // Filter by active status
      if (params.filters?.isActive !== undefined) {
        filtered = filtered.filter(
          (rule) => rule.isActive === params.filters!.isActive
        )
      }

      // Filter by trigger type
      if (params.filters?.triggerType) {
        filtered = filtered.filter((rule) =>
          rule.triggers.includes(params.filters!.triggerType!)
        )
      }

      // Search by name or description
      if (params.filters?.search) {
        const searchLower = params.filters.search.toLowerCase()
        filtered = filtered.filter(
          (rule) =>
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
    queryFn: async () => {
      const response = await rulesApi.apiRulesActiveGet()
      return response.data as any as Rule[]
    },
  })
}

export const useRulesByTrigger = (eventType: string) => {
  return useQuery({
    queryKey: rulesKeys.byTrigger(eventType),
    queryFn: async () => {
      const response = await rulesApi.apiRulesTriggerEventTypeGet(eventType)
      return response.data as any as Rule[]
    },
    enabled: !!eventType,
  })
}

export const useRule = (id: string) => {
  return useQuery({
    queryKey: rulesKeys.detail(id),
    queryFn: async () => {
      const response = await rulesApi.apiRulesIdGet(id)
      return response.data as any as Rule
    },
    enabled: !!id,
  })
}

// Rules mutations
export const useCreateRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (rule: CreateRule) => {
      const createRuleDto: CreateRuleDto = {
        id: rule.id,
        name: rule.name,
        description: rule.description,
        isActive: rule.isActive,
        triggers: rule.triggers,
        conditions: rule.conditions.map((c) => ({
          type: c.type,
          parameters: c.parameters,
        })),
        rewards: rule.rewards.map((r) => ({
          type: r.type,
          targetId: r.targetId,
          amount: r.amount,
          parameters: r.parameters,
        })),
      }
      const response = await rulesApi.apiRulesPost(createRuleDto)
      return response.data as any as Rule
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.all })
    },
  })
}

export const useUpdateRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ id, rule }: { id: string; rule: UpdateRule }) => {
      const updateRuleDto: UpdateRuleDto = {
        name: rule.name,
        description: rule.description,
        isActive: rule.isActive,
        triggers: rule.triggers,
        conditions: rule.conditions.map((c) => ({
          type: c.type,
          parameters: c.parameters,
        })),
        rewards: rule.rewards.map((r) => ({
          type: r.type,
          targetId: r.targetId,
          amount: r.amount,
          parameters: r.parameters,
        })),
      }
      const response = await rulesApi.apiRulesIdPut(id, updateRuleDto)
      return response.data as any as Rule
    },
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.all })
      queryClient.invalidateQueries({ queryKey: rulesKeys.detail(id) })
    },
  })
}

export const useActivateRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await rulesApi.apiRulesIdActivatePost(id)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.all })
    },
  })
}

export const useDeactivateRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await rulesApi.apiRulesIdDeactivatePost(id)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.all })
    },
  })
}

export const useDeleteRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await rulesApi.apiRulesIdDelete(id)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.all })
    },
  })
}

// Entity hooks
export const usePointCategories = () => {
  return useQuery({
    queryKey: entitiesKeys.pointCategories(),
    queryFn: async () => {
      const response = await rulesApi.apiRulesEntitiesPointCategoriesGet()
      return response.data as any as PointCategory[]
    },
  })
}

export const usePointCategory = (id: string) => {
  return useQuery({
    queryKey: [...entitiesKeys.pointCategories(), id],
    queryFn: async () => {
      const response = await rulesApi.apiRulesEntitiesPointCategoriesIdGet(id)
      return response.data as any as PointCategory
    },
    enabled: !!id,
  })
}

export const useBadges = () => {
  return useQuery({
    queryKey: entitiesKeys.badges(),
    queryFn: async () => {
      const response = await rulesApi.apiRulesEntitiesBadgesGet()
      return response.data as any as Badge[]
    },
  })
}

export const useBadge = (id: string) => {
  return useQuery({
    queryKey: [...entitiesKeys.badges(), id],
    queryFn: async () => {
      const response = await rulesApi.apiRulesEntitiesBadgesIdGet(id)
      return response.data as any as Badge
    },
    enabled: !!id,
  })
}

export const useTrophies = () => {
  return useQuery({
    queryKey: entitiesKeys.trophies(),
    queryFn: async () => {
      const response = await rulesApi.apiRulesEntitiesTrophiesGet()
      return response.data as any as Trophy[]
    },
  })
}

export const useTrophy = (id: string) => {
  return useQuery({
    queryKey: [...entitiesKeys.trophies(), id],
    queryFn: async () => {
      const response = await rulesApi.apiRulesEntitiesTrophiesIdGet(id)
      return response.data as any as Trophy
    },
    enabled: !!id,
  })
}

export const useLevels = () => {
  return useQuery({
    queryKey: entitiesKeys.levels(),
    queryFn: async () => {
      const response = await rulesApi.apiRulesEntitiesLevelsGet()
      return response.data as any as Level[]
    },
  })
}

export const useLevel = (id: string) => {
  return useQuery({
    queryKey: [...entitiesKeys.levels(), id],
    queryFn: async () => {
      const response = await rulesApi.apiRulesEntitiesLevelsIdGet(id)
      return response.data as any as Level
    },
    enabled: !!id,
  })
}
