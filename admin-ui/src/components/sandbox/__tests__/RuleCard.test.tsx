import React from 'react'
import { render, screen } from '@testing-library/react'
import RuleCard from '../RuleCard'

describe('RuleCard', () => {
  const mockRule = {
    ruleId: 'rule-comment-points',
    name: 'Comment gives XP',
    description: 'Award points for commenting',
    triggerMatched: true,
    conditions: [
      {
        conditionId: '0edcf646-6b42-4d13-b800-44367374f6d9',
        type: 'alwaysTrue',
        parameters: {},
        result: true,
        details: 'Condition Type: alwaysTrue; Parameters: ; Result: True',
        evaluationTimeMs: 0,
      },
    ],
    predictedRewards: [
      {
        type: 'points',
        targetId: 'xp',
        amount: 10,
        parameters: { category: 'xp', amount: '10' },
        name: '10 xp Points',
        description: 'Award 10 points in category xp',
      },
    ],
    wouldExecute: true,
    evaluationTimeMs: 0,
  }

  it('renders rule information correctly', () => {
    render(<RuleCard rule={mockRule} index={0} />)

    expect(screen.getByText('Rule 1: Comment gives XP')).toBeInTheDocument()
    expect(screen.getByText('Award points for commenting')).toBeInTheDocument()
    expect(screen.getByText('ID: rule-comment-points')).toBeInTheDocument()
    expect(screen.getByText('Will Execute')).toBeInTheDocument()
  })

  it('shows correct status for executable rule', () => {
    render(<RuleCard rule={mockRule} index={0} />)

    expect(screen.getByText('Will Execute')).toBeInTheDocument()
    expect(screen.getByText('Trigger Matched')).toBeInTheDocument()
  })

  it('shows conditions with progress', () => {
    render(<RuleCard rule={mockRule} index={0} />)

    expect(screen.getByText('Conditions (1/1)')).toBeInTheDocument()
    expect(screen.getByText('alwaysTrue')).toBeInTheDocument()
  })

  it('shows predicted rewards', () => {
    render(<RuleCard rule={mockRule} index={0} />)

    expect(screen.getByText('Predicted Rewards (1)')).toBeInTheDocument()
    expect(screen.getByText('10 xp Points')).toBeInTheDocument()
    expect(screen.getByText('10 points')).toBeInTheDocument()
  })

  it('handles rule that will not execute', () => {
    const nonExecutableRule = {
      ...mockRule,
      wouldExecute: false,
      triggerMatched: true,
    }

    render(<RuleCard rule={nonExecutableRule} index={0} />)

    expect(screen.getByText('Triggered (Blocked)')).toBeInTheDocument()
    expect(screen.getByText('Will Not Execute')).toBeInTheDocument()
  })

  it('handles rule that is not triggered', () => {
    const notTriggeredRule = {
      ...mockRule,
      triggerMatched: false,
      wouldExecute: false,
    }

    render(<RuleCard rule={notTriggeredRule} index={0} />)

    expect(screen.getByText('Not Triggered')).toBeInTheDocument()
    expect(screen.getByText('Will Not Execute')).toBeInTheDocument()
  })

  it('handles rule without conditions', () => {
    const ruleWithoutConditions = {
      ...mockRule,
      conditions: [],
    }

    render(<RuleCard rule={ruleWithoutConditions} index={0} />)

    expect(screen.getByText('Conditions (0/0)')).toBeInTheDocument()
  })

  it('handles rule without rewards', () => {
    const ruleWithoutRewards = {
      ...mockRule,
      predictedRewards: [],
    }

    render(<RuleCard rule={ruleWithoutRewards} index={0} />)

    expect(screen.queryByText('Predicted Rewards')).not.toBeInTheDocument()
  })
})
