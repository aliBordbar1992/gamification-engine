import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import type { CreateRuleDto, UpdateRuleDto } from '@/api/generated/models'
import { rulesApi } from '@/api/rules'
// Filter and search types
export interface RulesFilters {
  isActive?: boolean
  triggerType?: string
  search?: string
}

export interface RulesListParams {
  filters?: RulesFilters
  page?: number
  limit?: number
}

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
      return await rulesApi.getAllRules()
    },
    select: (data) => {
      if (!params?.filters) return data

      let filtered = data

      if (params.filters?.isActive !== undefined) {
        filtered = filtered.filter(
          (rule) => rule.isActive === params.filters!.isActive
        )
      }

      if (params.filters?.triggerType) {
        filtered = filtered.filter(
          (rule) =>
            rule.triggers?.includes(params.filters!.triggerType!) ?? false
        )
      }

      if (params.filters?.search) {
        const searchLower = params.filters.search.toLowerCase()
        filtered = filtered.filter(
          (rule) =>
            rule.name?.toLowerCase().includes(searchLower) ||
            rule.description?.toLowerCase().includes(searchLower) ||
            rule.id?.toLowerCase().includes(searchLower)
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
      return await rulesApi.getActiveRules()
    },
  })
}

export const useRulesByTrigger = (eventType: string) => {
  return useQuery({
    queryKey: rulesKeys.byTrigger(eventType),
    queryFn: async () => {
      return await rulesApi.getRulesByTrigger(eventType)
    },
    enabled: !!eventType,
  })
}

export const useRule = (id: string) => {
  return useQuery({
    queryKey: rulesKeys.detail(id),
    queryFn: async () => {
      return await rulesApi.getRuleById(id)
    },
    enabled: !!id,
  })
}

// Rules mutations
export const useCreateRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (rule: CreateRuleDto) => {
      const createRuleDto: CreateRuleDto = {
        id: rule.id,
        name: rule.name,
        description: rule.description,
        isActive: rule.isActive,
        triggers: rule.triggers,
        conditions: rule.conditions?.map((c) => ({
          type: c.type,
          parameters: c.parameters,
        })),
        rewards: rule.rewards?.map((r) => ({
          type: r.type,
          targetId: r.targetId,
          amount: r.amount,
          parameters: r.parameters,
        })),
      }
      const response = await rulesApi.createRule(createRuleDto)
      return response
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.all })
    },
  })
}

export const useUpdateRule = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ id, rule }: { id: string; rule: UpdateRuleDto }) => {
      const updateRuleDto: UpdateRuleDto = {
        name: rule.name,
        description: rule.description,
        isActive: rule.isActive,
        triggers: rule.triggers,
        conditions: rule.conditions?.map((c) => ({
          type: c.type,
          parameters: c.parameters,
        })),
        rewards: rule.rewards?.map((r) => ({
          type: r.type,
          targetId: r.targetId,
          amount: r.amount,
          parameters: r.parameters,
        })),
      }
      const response = await rulesApi.updateRule(id, updateRuleDto)
      return response
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
      await rulesApi.activateRule(id)
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
      await rulesApi.deactivateRule(id)
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
      await rulesApi.deleteRule(id)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.all })
    },
  })
}
