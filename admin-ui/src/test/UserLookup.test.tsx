import React from 'react'
import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import UserLookup from '../components/users/UserLookup'

// Test wrapper with QueryClient
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
      mutations: {
        retry: false,
      },
    },
  })

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  )
}

describe('UserLookup', () => {
  it('should render user lookup form', () => {
    const mockOnUserFound = vi.fn()

    render(<UserLookup onUserFound={mockOnUserFound} />, {
      wrapper: createWrapper(),
    })

    expect(screen.getByText('User Lookup')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('Enter user ID...')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /lookup/i })).toBeInTheDocument()
    expect(
      screen.getByText(/Enter a user ID to view their gamification data/)
    ).toBeInTheDocument()
  })

  it('should call onUserFound when search button is clicked', async () => {
    const mockOnUserFound = vi.fn()

    render(<UserLookup onUserFound={mockOnUserFound} />, {
      wrapper: createWrapper(),
    })

    const input = screen.getByPlaceholderText('Enter user ID...')
    const button = screen.getByRole('button', { name: /lookup/i })

    fireEvent.change(input, { target: { value: 'user123' } })
    fireEvent.click(button)

    await waitFor(() => {
      expect(mockOnUserFound).toHaveBeenCalledWith('user123')
    })
  })

  it('should call onUserFound when Enter key is pressed', async () => {
    const mockOnUserFound = vi.fn()

    render(<UserLookup onUserFound={mockOnUserFound} />, {
      wrapper: createWrapper(),
    })

    const input = screen.getByPlaceholderText('Enter user ID...')

    fireEvent.change(input, { target: { value: 'user123' } })
    fireEvent.keyDown(input, { key: 'Enter', code: 'Enter' })

    await waitFor(() => {
      expect(mockOnUserFound).toHaveBeenCalledWith('user123')
    })
  })

  it('should not call onUserFound when input is empty', async () => {
    const mockOnUserFound = vi.fn()

    render(<UserLookup onUserFound={mockOnUserFound} />, {
      wrapper: createWrapper(),
    })

    const button = screen.getByRole('button', { name: /lookup/i })

    fireEvent.click(button)

    await waitFor(() => {
      expect(mockOnUserFound).not.toHaveBeenCalled()
    })
  })

  it('should not call onUserFound when input is only whitespace', async () => {
    const mockOnUserFound = vi.fn()

    render(<UserLookup onUserFound={mockOnUserFound} />, {
      wrapper: createWrapper(),
    })

    const input = screen.getByPlaceholderText('Enter user ID...')
    const button = screen.getByRole('button', { name: /lookup/i })

    fireEvent.change(input, { target: { value: '   ' } })
    fireEvent.click(button)

    await waitFor(() => {
      expect(mockOnUserFound).not.toHaveBeenCalled()
    })
  })

  it('should trim whitespace from user ID', async () => {
    const mockOnUserFound = vi.fn()

    render(<UserLookup onUserFound={mockOnUserFound} />, {
      wrapper: createWrapper(),
    })

    const input = screen.getByPlaceholderText('Enter user ID...')
    const button = screen.getByRole('button', { name: /lookup/i })

    fireEvent.change(input, { target: { value: '  user123  ' } })
    fireEvent.click(button)

    await waitFor(() => {
      expect(mockOnUserFound).toHaveBeenCalledWith('user123')
    })
  })

  it('should disable button when loading', () => {
    const mockOnUserFound = vi.fn()

    render(<UserLookup onUserFound={mockOnUserFound} isLoading={true} />, {
      wrapper: createWrapper(),
    })

    const button = screen.getByRole('button', { name: /lookup/i })
    expect(button).toBeDisabled()
  })

  it('should show error message when error is provided', () => {
    const mockOnUserFound = vi.fn()
    const errorMessage = 'User not found'

    render(<UserLookup onUserFound={mockOnUserFound} error={errorMessage} />, {
      wrapper: createWrapper(),
    })

    expect(screen.getByText('Error')).toBeInTheDocument()
    expect(screen.getByText(errorMessage)).toBeInTheDocument()
  })

  it('should show example user IDs', () => {
    const mockOnUserFound = vi.fn()

    render(<UserLookup onUserFound={mockOnUserFound} />, {
      wrapper: createWrapper(),
    })

    expect(
      screen.getByText(/Example user IDs: user123, player456, gamer789/)
    ).toBeInTheDocument()
  })
})
