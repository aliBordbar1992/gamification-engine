import React from 'react'
import { render, screen } from '@testing-library/react'
import VisualTrace from '../VisualTrace'

describe('VisualTrace', () => {
  it('renders empty state when no evaluation result', () => {
    render(<VisualTrace evaluationResult={null} />)

    expect(
      screen.getByText('Run a dry-run to see the visual trace')
    ).toBeInTheDocument()
    expect(screen.getByRole('img', { hidden: true })).toHaveClass(
      'anticon-play-circle'
    )
  })

  it('renders event trace with basic information', () => {
    const mockResult = {
      triggerEventId: 'evt_123',
      userId: 'user_456',
      eventType: 'login',
      evaluatedAt: '2024-01-01T10:00:00Z',
      rules: [],
      summary: null,
    }

    render(<VisualTrace evaluationResult={mockResult} />)

    expect(screen.getByText('Event Trace')).toBeInTheDocument()
    expect(screen.getByText('Type: login')).toBeInTheDocument()
    expect(screen.getByText('User: user_456')).toBeInTheDocument()
    expect(screen.getByText('ID: evt_123')).toBeInTheDocument()
  })

  it('renders rules timeline when rules are present', () => {
    const mockResult = {
      triggerEventId: 'evt_123',
      userId: 'user_456',
      eventType: 'login',
      evaluatedAt: '2024-01-01T10:00:00Z',
      rules: [
        { name: 'First Login Rule', triggers: ['login'] },
        { name: 'Welcome Bonus Rule', triggers: ['login'] },
      ],
      summary: null,
    }

    render(<VisualTrace evaluationResult={mockResult} />)

    expect(screen.getByText('Rule Evaluation Timeline')).toBeInTheDocument()
    expect(screen.getByText('Rule 1: First Login Rule')).toBeInTheDocument()
    expect(screen.getByText('Rule 2: Welcome Bonus Rule')).toBeInTheDocument()
  })

  it('renders rewards section when rewards are present', () => {
    const mockResult = {
      triggerEventId: 'evt_123',
      userId: 'user_456',
      eventType: 'login',
      evaluatedAt: '2024-01-01T10:00:00Z',
      rules: [],
      summary: {
        rewards: [
          { type: 'badge', name: 'First Login', value: '100' },
          { type: 'points', name: 'Welcome Points', value: '50' },
        ],
      },
    }

    render(<VisualTrace evaluationResult={mockResult} />)

    expect(screen.getByText('Rewards Issued')).toBeInTheDocument()
    expect(screen.getByText('First Login')).toBeInTheDocument()
    expect(screen.getByText('Welcome Points')).toBeInTheDocument()
  })

  it('renders summary statistics', () => {
    const mockResult = {
      triggerEventId: 'evt_123',
      userId: 'user_456',
      eventType: 'login',
      evaluatedAt: '2024-01-01T10:00:00Z',
      rules: [{ name: 'Test Rule' }],
      summary: {
        pointsAwarded: 100,
        rewards: [{ type: 'badge', name: 'Test Badge' }],
      },
    }

    render(<VisualTrace evaluationResult={mockResult} />)

    expect(screen.getByText('Summary Statistics')).toBeInTheDocument()
    expect(screen.getByText('Rules Matched')).toBeInTheDocument()
    expect(screen.getByText('Rewards Issued')).toBeInTheDocument()
    expect(screen.getByText('Points Awarded')).toBeInTheDocument()
  })
})
