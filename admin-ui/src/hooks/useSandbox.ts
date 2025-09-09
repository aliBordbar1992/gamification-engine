import { useState, useCallback, useEffect } from 'react'
import { useMutation } from '@tanstack/react-query'
import { sandboxApi } from '@/api/sandbox'
import { sandboxHistory } from '@/utils/sandboxHistory'
import type {
  IngestEventRequest,
  ApiEventsSandboxDryRunPost200Response,
} from '@/api/generated/models'
import type { SandboxHistoryItem } from '@/utils/sandboxHistory'

export interface SandboxState {
  eventData: IngestEventRequest
  language: 'json' | 'yaml'
  isValid: boolean
  validationErrors: string[]
  evaluationResult: ApiEventsSandboxDryRunPost200Response | null
  isLoading: boolean
  error: string | null
  history: SandboxHistoryItem[]
  currentHistoryId: string | null
}

export const useSandbox = () => {
  const [eventData, setEventData] = useState<IngestEventRequest>({
    eventType: '',
    userId: '',
    eventId: undefined,
    occurredAt: new Date().toISOString(),
    attributes: {},
  })

  const [language, setLanguage] = useState<'json' | 'yaml'>('json')
  const [isValid, setIsValid] = useState(false)
  const [validationErrors, setValidationErrors] = useState<string[]>([])
  const [history, setHistory] = useState<SandboxHistoryItem[]>([])
  const [currentHistoryId, setCurrentHistoryId] = useState<string | null>(null)

  // Load history on mount
  useEffect(() => {
    const loadedHistory = sandboxHistory.getAll()
    setHistory(loadedHistory)
  }, [])

  // Dry-run mutation
  const dryRunMutation = useMutation({
    mutationFn: sandboxApi.dryRunEvent,
    onSuccess: (data) => {
      console.log('Dry run successful:', data)
    },
    onError: (error) => {
      console.error('Dry run failed:', error)
    },
  })

  // Update event data
  const updateEventData = useCallback(
    (newData: Partial<IngestEventRequest>) => {
      setEventData((prev) => ({ ...prev, ...newData }))
    },
    []
  )

  // Update event data from editor content
  const updateEventFromEditor = useCallback(
    (content: string) => {
      try {
        let parsed: any

        if (language === 'yaml') {
          // For YAML, we'll need to parse it
          // For now, we'll assume it's JSON-compatible YAML
          parsed = JSON.parse(content)
        } else {
          parsed = JSON.parse(content)
        }

        // Validate required fields
        if (!parsed.eventType || !parsed.userId) {
          setIsValid(false)
          setValidationErrors(['eventType and userId are required'])
          return
        }

        setEventData(parsed)
        setIsValid(true)
        setValidationErrors([])
      } catch (error) {
        setIsValid(false)
        setValidationErrors([
          `Invalid ${language.toUpperCase()}: ${
            error instanceof Error ? error.message : 'Unknown error'
          }`,
        ])
      }
    },
    [language]
  )

  // Run dry-run evaluation
  const runDryRun = useCallback(() => {
    if (!isValid) return

    dryRunMutation.mutate(eventData)
  }, [eventData, isValid, dryRunMutation])

  // Get cURL command
  const getCurlCommand = useCallback(
    (baseUrl: string) => {
      return sandboxApi.getCurlCommand(eventData, baseUrl)
    },
    [eventData]
  )

  // Load example event
  const loadExample = useCallback(
    (exampleType: 'login' | 'purchase' | 'achievement') => {
      const examples = {
        login: {
          eventType: 'user.login',
          userId: 'user-123',
          eventId: `login-${Date.now()}`,
          occurredAt: new Date().toISOString(),
          attributes: {
            platform: 'web',
            userAgent: 'Mozilla/5.0...',
            ipAddress: '192.168.1.1',
          },
        },
        purchase: {
          eventType: 'user.purchase',
          userId: 'user-123',
          eventId: `purchase-${Date.now()}`,
          occurredAt: new Date().toISOString(),
          attributes: {
            productId: 'prod-456',
            amount: 29.99,
            currency: 'USD',
            category: 'electronics',
          },
        },
        achievement: {
          eventType: 'user.achievement',
          userId: 'user-123',
          eventId: `achievement-${Date.now()}`,
          occurredAt: new Date().toISOString(),
          attributes: {
            achievementType: 'streak',
            value: 7,
            description: '7-day login streak',
          },
        },
      }

      const example = examples[exampleType]
      setEventData(example)
      setIsValid(true)
      setValidationErrors([])
      setCurrentHistoryId(null) // Clear current history ID when loading example
    },
    []
  )

  // Save current event to history
  const saveToHistory = useCallback(
    (name?: string) => {
      if (!isValid) return null

      const historyName = name || sandboxHistory.generateName(eventData)
      const savedItem = sandboxHistory.save({
        name: historyName,
        eventData,
        language,
      })

      // Refresh history list
      const updatedHistory = sandboxHistory.getAll()
      setHistory(updatedHistory)
      setCurrentHistoryId(savedItem.id)

      return savedItem
    },
    [eventData, language, isValid]
  )

  // Load event from history
  const loadFromHistory = useCallback((historyId: string) => {
    const historyItem = sandboxHistory.load(historyId)
    if (historyItem) {
      setEventData(historyItem.eventData)
      setLanguage(historyItem.language)
      setIsValid(true)
      setValidationErrors([])
      setCurrentHistoryId(historyId)

      // Refresh history list to update last used timestamps
      const updatedHistory = sandboxHistory.getAll()
      setHistory(updatedHistory)
    }
  }, [])

  // Delete from history
  const deleteFromHistory = useCallback(
    (historyId: string) => {
      sandboxHistory.delete(historyId)
      const updatedHistory = sandboxHistory.getAll()
      setHistory(updatedHistory)

      // Clear current history ID if we deleted the current item
      if (currentHistoryId === historyId) {
        setCurrentHistoryId(null)
      }
    },
    [currentHistoryId]
  )

  // Clear all history
  const clearHistory = useCallback(() => {
    sandboxHistory.clear()
    setHistory([])
    setCurrentHistoryId(null)
  }, [])

  return {
    eventData,
    language,
    setLanguage,
    isValid,
    validationErrors,
    evaluationResult: dryRunMutation.data,
    isLoading: dryRunMutation.isPending,
    error: dryRunMutation.error?.message || null,
    history,
    currentHistoryId,
    updateEventData,
    updateEventFromEditor,
    runDryRun,
    getCurlCommand,
    loadExample,
    saveToHistory,
    loadFromHistory,
    deleteFromHistory,
    clearHistory,
  }
}
