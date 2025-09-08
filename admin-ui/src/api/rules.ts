import type { RuleDto, CreateRuleDto, UpdateRuleDto } from './generated/models'
import { RulesApiInstance } from './generated-client'

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

// Rules API
export const rulesApi = {
  // Get all rules
  getAllRules: async (): Promise<RuleDto[]> => {
    const response = await RulesApiInstance().apiRulesGet()
    return response.data
  },

  // Get active rules only
  getActiveRules: async (): Promise<RuleDto[]> => {
    const response = await RulesApiInstance().apiRulesActiveGet()
    return response.data
  },

  // Get rules by trigger event type
  getRulesByTrigger: async (eventType: string): Promise<RuleDto[]> => {
    const response = await RulesApiInstance().apiRulesTriggerEventTypeGet(
      eventType
    )
    return response.data
  },

  // Get specific rule by ID
  getRuleById: async (id: string): Promise<RuleDto> => {
    const response = await RulesApiInstance().apiRulesIdGet(id)
    return response.data
  },

  // Create a new rule
  createRule: async (rule: CreateRuleDto): Promise<RuleDto> => {
    const response = await RulesApiInstance().apiRulesPost(rule)
    return response.data
  },

  // Update an existing rule
  updateRule: async (id: string, rule: UpdateRuleDto): Promise<RuleDto> => {
    const response = await RulesApiInstance().apiRulesIdPut(id, rule)
    return response.data
  },

  // Activate a rule
  activateRule: async (id: string): Promise<void> => {
    await RulesApiInstance().apiRulesIdActivatePost(id)
  },

  // Deactivate a rule
  deactivateRule: async (id: string): Promise<void> => {
    await RulesApiInstance().apiRulesIdDeactivatePost(id)
  },

  // Delete a rule
  deleteRule: async (id: string): Promise<void> => {
    await RulesApiInstance().apiRulesIdDelete(id)
  },
}
