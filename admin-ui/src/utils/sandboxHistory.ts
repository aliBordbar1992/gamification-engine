import type { IngestEventRequest } from '@/api/generated/models'

export interface SandboxHistoryItem {
  id: string
  name: string
  eventData: IngestEventRequest
  language: 'json' | 'yaml'
  createdAt: string
  lastUsed: string
}

const STORAGE_KEY = 'sandbox-history'
const MAX_HISTORY_ITEMS = 10

export const sandboxHistory = {
  // Get all saved sandbox inputs
  getAll: (): SandboxHistoryItem[] => {
    try {
      const stored = localStorage.getItem(STORAGE_KEY)
      if (!stored) return []

      const history = JSON.parse(stored) as SandboxHistoryItem[]
      return history.sort(
        (a, b) =>
          new Date(b.lastUsed).getTime() - new Date(a.lastUsed).getTime()
      )
    } catch (error) {
      console.error('Failed to load sandbox history:', error)
      return []
    }
  },

  // Save a new sandbox input
  save: (
    item: Omit<SandboxHistoryItem, 'id' | 'createdAt' | 'lastUsed'>
  ): SandboxHistoryItem => {
    try {
      const existingHistory = sandboxHistory.getAll()
      const newItem: SandboxHistoryItem = {
        ...item,
        id: `sandbox-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
        createdAt: new Date().toISOString(),
        lastUsed: new Date().toISOString(),
      }

      // Remove any existing item with the same name
      const filteredHistory = existingHistory.filter(
        (h) => h.name !== item.name
      )

      // Add new item at the beginning
      const updatedHistory = [newItem, ...filteredHistory].slice(
        0,
        MAX_HISTORY_ITEMS
      )

      localStorage.setItem(STORAGE_KEY, JSON.stringify(updatedHistory))
      return newItem
    } catch (error) {
      console.error('Failed to save sandbox history:', error)
      throw error
    }
  },

  // Update last used timestamp for an existing item
  updateLastUsed: (id: string): void => {
    try {
      const history = sandboxHistory.getAll()
      const itemIndex = history.findIndex((item) => item.id === id)

      if (itemIndex !== -1) {
        history[itemIndex].lastUsed = new Date().toISOString()
        localStorage.setItem(STORAGE_KEY, JSON.stringify(history))
      }
    } catch (error) {
      console.error('Failed to update sandbox history:', error)
    }
  },

  // Load a specific sandbox input
  load: (id: string): SandboxHistoryItem | null => {
    try {
      const history = sandboxHistory.getAll()
      const item = history.find((h) => h.id === id)

      if (item) {
        sandboxHistory.updateLastUsed(id)
        return item
      }

      return null
    } catch (error) {
      console.error('Failed to load sandbox history item:', error)
      return null
    }
  },

  // Delete a specific sandbox input
  delete: (id: string): void => {
    try {
      const history = sandboxHistory.getAll()
      const filteredHistory = history.filter((item) => item.id !== id)
      localStorage.setItem(STORAGE_KEY, JSON.stringify(filteredHistory))
    } catch (error) {
      console.error('Failed to delete sandbox history item:', error)
    }
  },

  // Clear all sandbox history
  clear: (): void => {
    try {
      localStorage.removeItem(STORAGE_KEY)
    } catch (error) {
      console.error('Failed to clear sandbox history:', error)
    }
  },

  // Generate a default name for an event
  generateName: (eventData: IngestEventRequest): string => {
    const eventType = eventData.eventType || 'Unknown Event'
    const userId = eventData.userId || 'Unknown User'
    const timestamp = new Date().toLocaleString()
    return `${eventType} - ${userId} (${timestamp})`
  },
}
