// Types for Rules and related entities based on backend DTOs

export interface Rule {
  id: string
  name: string
  description: string
  isActive: boolean
  triggers: string[]
  conditions: Condition[]
  rewards: Reward[]
  createdAt: string
  updatedAt?: string
}

export interface Condition {
  type: string
  parameters: Record<string, any>
}

export interface Reward {
  type: string
  targetId: string
  amount?: number
  parameters: Record<string, any>
}

export interface CreateRule {
  id: string
  name: string
  description: string
  isActive: boolean
  triggers: string[]
  conditions: Condition[]
  rewards: Reward[]
}

export interface UpdateRule {
  name: string
  description: string
  isActive: boolean
  triggers: string[]
  conditions: Condition[]
  rewards: Reward[]
}

// Entity types
export interface Badge {
  id: string
  name: string
  description: string
  image: string
  visible: boolean
}

export interface Trophy {
  id: string
  name: string
  description: string
  image: string
  visible: boolean
}

export interface PointCategory {
  id: string
  name: string
  description: string
  aggregation: string
}

export interface Level {
  id: string
  name: string
  category: string
  minPoints: number
}

// API response types
export interface RulesResponse {
  rules: Rule[]
}

export interface RuleResponse {
  rule: Rule
}

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
