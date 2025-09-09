import React from 'react'
import { render, screen, fireEvent } from '@testing-library/react'
import ResultsDisplay from '../ResultsDisplay'

describe('ResultsDisplay', () => {
  it('renders empty state when no evaluation result and not loading', () => {
    render(
      <ResultsDisplay
        error={undefined}
        evaluationResult={undefined}
        isLoading={false}
      />
    )

    expect(
      screen.getByText('Run a dry-run to see evaluation results here.')
    ).toBeInTheDocument()
  })

  it('renders error when present', () => {
    render(
      <ResultsDisplay
        error="Test error"
        evaluationResult={undefined}
        isLoading={false}
      />
    )

    expect(screen.getByText('Dry Run Failed')).toBeInTheDocument()
    expect(screen.getByText('Test error')).toBeInTheDocument()
  })

  it('renders tabs for visual and json views when evaluation result is present', () => {
    const mockResult = {
      triggerEventId: 'evt_123',
      userId: 'user_456',
      eventType: 'login',
      evaluatedAt: '2024-01-01T10:00:00Z',
      rules: [],
      summary: null,
    }

    render(
      <ResultsDisplay
        error={undefined}
        evaluationResult={mockResult}
        isLoading={false}
      />
    )

    expect(screen.getByText('Evaluation Results')).toBeInTheDocument()
    expect(screen.getByText('Visual Trace')).toBeInTheDocument()
    expect(screen.getByText('JSON View')).toBeInTheDocument()
  })

  it('switches between tabs when clicked', () => {
    const mockResult = {
      triggerEventId: 'evt_123',
      userId: 'user_456',
      eventType: 'login',
      evaluatedAt: '2024-01-01T10:00:00Z',
      rules: [],
      summary: null,
    }

    render(
      <ResultsDisplay
        error={undefined}
        evaluationResult={mockResult}
        isLoading={false}
      />
    )

    // Should start with visual tab active
    expect(screen.getByText('Event Trace')).toBeInTheDocument()

    // Click on JSON tab
    fireEvent.click(screen.getByText('JSON View'))

    // Should show JSON view content
    expect(screen.getByText('Event Details:')).toBeInTheDocument()
    expect(screen.getByText('Event ID: evt_123')).toBeInTheDocument()
  })

  it('renders rules in JSON view when present', () => {
    const mockResult = {
      triggerEventId: 'evt_123',
      userId: 'user_456',
      eventType: 'login',
      evaluatedAt: '2024-01-01T10:00:00Z',
      rules: [{ name: 'Test Rule', triggers: ['login'] }],
      summary: null,
    }

    render(
      <ResultsDisplay
        error={undefined}
        evaluationResult={mockResult}
        isLoading={false}
      />
    )

    // Switch to JSON view
    fireEvent.click(screen.getByText('JSON View'))

    expect(screen.getByText('Matched Rules (1):')).toBeInTheDocument()
    expect(screen.getByText('Rule 1')).toBeInTheDocument()
  })

  it('renders summary in JSON view when present', () => {
    const mockResult = {
      triggerEventId: 'evt_123',
      userId: 'user_456',
      eventType: 'login',
      evaluatedAt: '2024-01-01T10:00:00Z',
      rules: [],
      summary: {
        pointsAwarded: 100,
        rewards: [],
      },
    }

    render(
      <ResultsDisplay
        error={undefined}
        evaluationResult={mockResult}
        isLoading={false}
      />
    )

    // Switch to JSON view
    fireEvent.click(screen.getByText('JSON View'))

    expect(screen.getByText('Summary:')).toBeInTheDocument()
  })
})
