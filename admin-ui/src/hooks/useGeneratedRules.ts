// Example hook using generated API client
// This demonstrates how to integrate the generated API with React Query

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { RulesApi, createApiInstance } from '@/api/generated-client'
import type { CreateRuleDto, UpdateRuleDto } from '@/api/generated/models'

// Create API instance
const rulesApi = createApiInstance(RulesApi)

// Query keys
export const rulesKeys = {
  all: ['rules'] as const,
  lists: () => [...rulesKeys.all, 'list'] as const,
  list: (filters: Record<string, unknown>) =>
    [...rulesKeys.lists(), { filters }] as const,
  details: () => [...rulesKeys.all, 'detail'] as const,
  detail: (id: string) => [...rulesKeys.details(), id] as const,
  active: () => [...rulesKeys.all, 'active'] as const,
  byTrigger: (eventType: string) =>
    [...rulesKeys.all, 'trigger', eventType] as const,
}

// Hooks using generated API
export function useRules() {
  return useQuery({
    queryKey: rulesKeys.lists(),
    queryFn: () => rulesApi.apiRulesGet(),
    select: (response) => response.data,
  })
}

export function useActiveRules() {
  return useQuery({
    queryKey: rulesKeys.active(),
    queryFn: () => rulesApi.apiRulesActiveGet(),
    select: (response) => response.data,
  })
}

export function useRule(id: string) {
  return useQuery({
    queryKey: rulesKeys.detail(id),
    queryFn: () => rulesApi.apiRulesIdGet(id),
    select: (response) => response.data,
    enabled: !!id,
  })
}

export function useRulesByTrigger(eventType: string) {
  return useQuery({
    queryKey: rulesKeys.byTrigger(eventType),
    queryFn: () => rulesApi.apiRulesTriggerEventTypeGet(eventType),
    select: (response) => response.data,
    enabled: !!eventType,
  })
}

export function useCreateRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (rule: CreateRuleDto) => rulesApi.apiRulesPost(rule),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.lists() })
      queryClient.invalidateQueries({ queryKey: rulesKeys.active() })
    },
  })
}

export function useUpdateRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, rule }: { id: string; rule: UpdateRuleDto }) =>
      rulesApi.apiRulesIdPut(id, rule),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.lists() })
      queryClient.invalidateQueries({ queryKey: rulesKeys.detail(id) })
      queryClient.invalidateQueries({ queryKey: rulesKeys.active() })
    },
  })
}

export function useDeleteRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => rulesApi.apiRulesIdDelete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.lists() })
      queryClient.invalidateQueries({ queryKey: rulesKeys.active() })
    },
  })
}

export function useActivateRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => rulesApi.apiRulesIdActivatePost(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.lists() })
      queryClient.invalidateQueries({ queryKey: rulesKeys.active() })
    },
  })
}

export function useDeactivateRule() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => rulesApi.apiRulesIdDeactivatePost(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: rulesKeys.lists() })
      queryClient.invalidateQueries({ queryKey: rulesKeys.active() })
    },
  })
}
