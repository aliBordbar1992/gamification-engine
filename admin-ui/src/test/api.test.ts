import { describe, it, expect, vi } from 'vitest'
import apiClient from '../api/client'

// Mock axios
vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      interceptors: {
        request: { use: vi.fn() },
        response: { use: vi.fn() },
      },
    })),
  },
}))

describe('API Client', () => {
  it('should be defined', () => {
    expect(apiClient).toBeDefined()
  })

  it('should have interceptors configured', () => {
    expect(apiClient.interceptors).toBeDefined()
    expect(apiClient.interceptors.request).toBeDefined()
    expect(apiClient.interceptors.response).toBeDefined()
  })
})
