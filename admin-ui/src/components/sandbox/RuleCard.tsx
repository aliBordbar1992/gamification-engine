import React from 'react'
import { Card, Typography, Tag, Space, Divider, Progress, Tooltip } from 'antd'
import {
  CheckCircleOutlined,
  CloseCircleOutlined,
  ClockCircleOutlined,
  ThunderboltOutlined,
  InfoCircleOutlined,
} from '@ant-design/icons'

const { Text, Title } = Typography

interface Condition {
  conditionId: string
  type: string
  parameters: Record<string, unknown>
  result: boolean
  details: string
  evaluationTimeMs: number
}

interface PredictedReward {
  type: string
  targetId: string
  amount: number
  parameters: Record<string, unknown>
  name: string
  description: string
}

interface PredictedSpending {
  type: string
  category: string
  amount?: number
  destinationUserId?: string
  attributes: Record<string, string>
}

interface RuleCardProps {
  rule: {
    ruleId: string
    name: string
    description: string
    triggerMatched: boolean
    conditions: Condition[]
    predictedRewards: PredictedReward[]
    predictedSpendings?: PredictedSpending[]
    wouldExecute: boolean
    evaluationTimeMs: number
  }
  index: number
}

const RuleCard: React.FC<RuleCardProps> = ({ rule, index }) => {
  const {
    ruleId,
    name,
    description,
    triggerMatched,
    conditions = [],
    predictedRewards = [],
    predictedSpendings = [],
    wouldExecute,
    evaluationTimeMs,
  } = rule

  const getRuleStatusColor = () => {
    if (wouldExecute && triggerMatched) return '#52c41a'
    if (triggerMatched) return '#faad14'
    return '#ff4d4f'
  }

  const getRuleStatusIcon = () => {
    if (wouldExecute && triggerMatched) return <CheckCircleOutlined />
    if (triggerMatched) return <ClockCircleOutlined />
    return <CloseCircleOutlined />
  }

  const getRuleStatusText = () => {
    if (wouldExecute && triggerMatched) return 'Will Execute'
    if (triggerMatched) return 'Triggered (Blocked)'
    return 'Not Triggered'
  }

  const getConditionIcon = (condition: Condition) => {
    return condition.result ? (
      <CheckCircleOutlined style={{ color: '#52c41a' }} />
    ) : (
      <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
    )
  }

  const getRewardIcon = (type: string) => {
    switch (type?.toLowerCase()) {
      case 'points':
        return '🎯'
      case 'badge':
        return '🏆'
      case 'trophy':
        return '⭐'
      case 'level':
        return '📈'
      default:
        return '🎁'
    }
  }

  const getSpendingIcon = (type: string) => {
    switch (type?.toLowerCase()) {
      case 'transaction':
        return '💸'
      case 'transfer':
        return '🔄'
      default:
        return '💰'
    }
  }

  const totalConditions = conditions.length
  const passedConditions = conditions.filter((c) => c.result).length
  const conditionSuccessRate =
    totalConditions > 0 ? (passedConditions / totalConditions) * 100 : 0

  return (
    <Card
      size="small"
      style={{
        marginBottom: '12px',
        border: `2px solid ${getRuleStatusColor()}`,
        borderRadius: '8px',
      }}
    >
      {/* Rule Header */}
      <div style={{ marginBottom: '12px' }}>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'flex-start',
            marginBottom: '8px',
          }}
        >
          <div style={{ flex: 1 }}>
            <Title level={5} style={{ margin: 0, color: getRuleStatusColor() }}>
              {getRuleStatusIcon()} Rule {index + 1}: {name}
            </Title>
            {description && (
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {description}
              </Text>
            )}
          </div>
          <Tag color={getRuleStatusColor()} style={{ marginLeft: '8px' }}>
            {getRuleStatusText()}
          </Tag>
        </div>

        <div style={{ display: 'flex', gap: '12px', alignItems: 'center' }}>
          <Text style={{ fontSize: '11px', color: '#666' }}>ID: {ruleId}</Text>
          <Text style={{ fontSize: '11px', color: '#666' }}>
            <ClockCircleOutlined /> {evaluationTimeMs}ms
          </Text>
        </div>
      </div>

      {/* Conditions Section */}
      {conditions.length > 0 && (
        <div style={{ marginBottom: '12px' }}>
          <div
            style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              marginBottom: '8px',
            }}
          >
            <Text strong style={{ fontSize: '13px' }}>
              Conditions ({passedConditions}/{totalConditions})
            </Text>
            <Progress
              percent={conditionSuccessRate}
              size="small"
              style={{ width: '100px' }}
              strokeColor={conditionSuccessRate === 100 ? '#52c41a' : '#faad14'}
            />
          </div>

          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '6px' }}>
            {conditions.map((condition, idx) => (
              <Tooltip key={idx} title={condition.details}>
                <Tag
                  color={condition.result ? 'green' : 'red'}
                  style={{ cursor: 'pointer', fontSize: '11px' }}
                >
                  {getConditionIcon(condition)} {condition.type}
                </Tag>
              </Tooltip>
            ))}
          </div>
        </div>
      )}

      {/* Predicted Rewards Section */}
      {predictedRewards.length > 0 && (
        <div>
          <Text
            strong
            style={{ fontSize: '13px', marginBottom: '8px', display: 'block' }}
          >
            Predicted Rewards ({predictedRewards.length})
          </Text>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
            {predictedRewards.map((reward, idx) => (
              <Card
                key={idx}
                size="small"
                style={{
                  background:
                    'linear-gradient(135deg, #e6f7ff 0%, #bae7ff 100%)',
                  border: '1px solid #91d5ff',
                  borderRadius: '6px',
                  minWidth: '140px',
                }}
              >
                <Space
                  direction="vertical"
                  align="center"
                  style={{ width: '100%' }}
                >
                  <div style={{ fontSize: '18px' }}>
                    {getRewardIcon(reward.type)}
                  </div>
                  <Text
                    strong
                    style={{ fontSize: '11px', textAlign: 'center' }}
                  >
                    {reward.name}
                  </Text>
                  <Text
                    style={{
                      fontSize: '10px',
                      color: '#666',
                      textAlign: 'center',
                    }}
                  >
                    {reward.amount} {reward.type}
                  </Text>
                  {reward.description && (
                    <Text
                      style={{
                        fontSize: '9px',
                        color: '#999',
                        textAlign: 'center',
                      }}
                    >
                      {reward.description}
                    </Text>
                  )}
                </Space>
              </Card>
            ))}
          </div>
        </div>
      )}

      {/* Predicted Spendings Section */}
      {predictedSpendings.length > 0 && (
        <div>
          <Text
            strong
            style={{ fontSize: '13px', marginBottom: '8px', display: 'block' }}
          >
            Predicted Spendings ({predictedSpendings.length})
          </Text>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
            {predictedSpendings.map((spending, idx) => (
              <Card
                key={idx}
                size="small"
                style={{
                  background:
                    'linear-gradient(135deg, #fff2e8 0%, #ffd591 100%)',
                  border: '1px solid #ffb366',
                  borderRadius: '6px',
                  minWidth: '140px',
                }}
              >
                <Space
                  direction="vertical"
                  align="center"
                  style={{ width: '100%' }}
                >
                  <div style={{ fontSize: '18px' }}>
                    {getSpendingIcon(spending.type)}
                  </div>
                  <Text
                    strong
                    style={{ fontSize: '11px', textAlign: 'center' }}
                  >
                    {spending.type}: {spending.category}
                  </Text>
                  {spending.amount && (
                    <Text
                      style={{
                        fontSize: '10px',
                        color: '#666',
                        textAlign: 'center',
                      }}
                    >
                      Amount: {spending.amount}
                    </Text>
                  )}
                  {spending.destinationUserId && (
                    <Text
                      style={{
                        fontSize: '9px',
                        color: '#999',
                        textAlign: 'center',
                      }}
                    >
                      To: {spending.destinationUserId}
                    </Text>
                  )}
                </Space>
              </Card>
            ))}
          </div>
        </div>
      )}

      {/* Execution Summary */}
      <Divider style={{ margin: '12px 0 8px 0' }} />
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
          <ThunderboltOutlined
            style={{ color: wouldExecute ? '#52c41a' : '#ff4d4f' }}
          />
          <Text style={{ fontSize: '12px', fontWeight: 'bold' }}>
            {wouldExecute ? 'Will Execute' : 'Will Not Execute'}
          </Text>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
          <InfoCircleOutlined style={{ color: '#1890ff' }} />
          <Text style={{ fontSize: '11px', color: '#666' }}>
            {triggerMatched ? 'Trigger Matched' : 'Trigger Not Matched'}
          </Text>
        </div>
      </div>
    </Card>
  )
}

export default RuleCard
