import { Rule } from '@/types'

// Mock data for development when backend is not available
export const mockRules: Rule[] = [
  {
    id: 'rule1',
    name: 'First Comment Reward',
    description: 'Award 10 XP for first comment',
    isActive: true,
    triggers: ['USER_COMMENTED'],
    conditions: [
      {
        type: 'firstOccurrence',
        parameters: {
          maxOccurrences: 1,
        },
      },
    ],
    rewards: [
      {
        type: 'points',
        targetId: 'xp',
        amount: 10,
        parameters: {
          multiplier: 1,
        },
      },
      {
        type: 'badge',
        targetId: 'first-comment',
        parameters: {},
      },
    ],
    createdAt: '2023-01-01T00:00:00Z',
    updatedAt: '2023-01-02T00:00:00Z',
  },
  {
    id: 'rule2',
    name: 'Like Reward',
    description: 'Award 5 XP for each like',
    isActive: false,
    triggers: ['USER_LIKED'],
    conditions: [
      {
        type: 'always',
        parameters: {},
      },
    ],
    rewards: [
      {
        type: 'points',
        targetId: 'xp',
        amount: 5,
        parameters: {
          multiplier: 1,
        },
      },
    ],
    createdAt: '2023-01-02T00:00:00Z',
    updatedAt: '2023-01-03T00:00:00Z',
  },
  {
    id: 'rule3',
    name: 'Daily Login Bonus',
    description: 'Award 20 XP for daily login',
    isActive: true,
    triggers: ['USER_LOGIN'],
    conditions: [
      {
        type: 'dailyLimit',
        parameters: {
          maxPerDay: 1,
        },
      },
    ],
    rewards: [
      {
        type: 'points',
        targetId: 'xp',
        amount: 20,
        parameters: {
          multiplier: 1,
        },
      },
    ],
    createdAt: '2023-01-03T00:00:00Z',
    updatedAt: '2023-01-04T00:00:00Z',
  },
]

export const mockPointCategories = [
  {
    id: 'xp',
    name: 'Experience Points',
    description: 'Main experience currency',
  },
  { id: 'coins', name: 'Coins', description: 'In-game currency' },
]

export const mockBadges = [
  {
    id: 'first-comment',
    name: 'First Comment',
    description: 'Awarded for first comment',
  },
  {
    id: 'veteran',
    name: 'Veteran',
    description: 'Awarded for long-term participation',
  },
]

export const mockTrophies = [
  { id: 'champion', name: 'Champion', description: 'Top performer trophy' },
]

export const mockLevels = [
  {
    id: 'beginner',
    name: 'Beginner',
    description: 'Starting level',
    minPoints: 0,
  },
  {
    id: 'intermediate',
    name: 'Intermediate',
    description: 'Intermediate level',
    minPoints: 100,
  },
]
