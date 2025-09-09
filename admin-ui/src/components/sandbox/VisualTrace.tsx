import React from 'react'
import {
  Card,
  Typography,
  Tag,
  Space,
  Divider,
  Badge,
  Row,
  Col,
  Statistic,
} from 'antd'
import {
  PlayCircleOutlined,
  TrophyOutlined,
  StarOutlined,
  GiftOutlined,
  UserOutlined,
  ClockCircleOutlined,
  ThunderboltOutlined,
  CheckCircleOutlined,
} from '@ant-design/icons'
import RuleCard from './RuleCard'

const { Title, Text } = Typography

interface VisualTraceProps {
  evaluationResult: any
}

const VisualTrace: React.FC<VisualTraceProps> = ({ evaluationResult }) => {
  if (!evaluationResult) {
    return (
      <div style={{ textAlign: 'center', padding: '40px' }}>
        <PlayCircleOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
        <div style={{ marginTop: '16px' }}>
          <Text type="secondary">Run a dry-run to see the visual trace</Text>
        </div>
      </div>
    )
  }

  const {
    rules = [],
    summary,
    triggerEventId,
    userId,
    eventType,
    evaluatedAt,
  } = evaluationResult

  const getTotalRewards = () => {
    if (!summary?.rewards) return 0
    return summary.rewards.length
  }

  const getTotalPoints = () => {
    if (!summary?.pointsAwarded) return 0
    return summary.pointsAwarded
  }

  const getExecutableRules = () => {
    return rules.filter((rule: any) => rule.wouldExecute).length
  }

  const getTriggeredRules = () => {
    return rules.filter((rule: any) => rule.triggerMatched).length
  }

  const getRewardIcon = (rewardType: string) => {
    switch (rewardType?.toLowerCase()) {
      case 'badge':
        return <TrophyOutlined />
      case 'trophy':
        return <StarOutlined />
      case 'points':
        return <GiftOutlined />
      default:
        return <GiftOutlined />
    }
  }

  const formatRewards = (summary: any) => {
    if (!summary?.rewards) return []

    return summary.rewards.map((reward: any, index: number) => ({
      key: index,
      type: reward.type || 'unknown',
      name: reward.name || `Reward ${index + 1}`,
      value: reward.value || reward.amount || 'N/A',
      description: reward.description || '',
    }))
  }

  const rewards = formatRewards(summary)

  return (
    <div style={{ padding: '16px' }}>
      {/* Event Header */}
      <Card
        size="small"
        style={{
          marginBottom: '16px',
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          color: 'white',
          border: 'none',
        }}
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <UserOutlined />
            <Text strong style={{ color: 'white', fontSize: '16px' }}>
              Event Trace
            </Text>
          </div>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
            <Tag color="blue" style={{ margin: 0 }}>
              <strong>Type:</strong> {eventType}
            </Tag>
            <Tag color="green" style={{ margin: 0 }}>
              <strong>User:</strong> {userId}
            </Tag>
            {triggerEventId && (
              <Tag color="orange" style={{ margin: 0 }}>
                <strong>ID:</strong> {triggerEventId}
              </Tag>
            )}
          </div>
          {evaluatedAt && (
            <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
              <ClockCircleOutlined />
              <Text style={{ color: 'white', fontSize: '12px' }}>
                Evaluated: {new Date(evaluatedAt).toLocaleString()}
              </Text>
            </div>
          )}
        </Space>
      </Card>

      {/* Rules Evaluation */}
      <Card size="small" style={{ marginBottom: '16px' }}>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            marginBottom: '16px',
          }}
        >
          <Title level={5} style={{ margin: 0 }}>
            Rule Evaluation Results
          </Title>
          <div style={{ display: 'flex', gap: '8px' }}>
            <Tag color="blue">{rules.length} Total Rules</Tag>
            <Tag color="green">{getTriggeredRules()} Triggered</Tag>
            <Tag color="purple">{getExecutableRules()} Will Execute</Tag>
          </div>
        </div>

        {rules.length > 0 ? (
          <div>
            {rules.map((rule: any, index: number) => (
              <RuleCard key={index} rule={rule} index={index} />
            ))}
          </div>
        ) : (
          <div style={{ textAlign: 'center', padding: '40px' }}>
            <CheckCircleOutlined
              style={{ fontSize: '48px', color: '#d9d9d9' }}
            />
            <div style={{ marginTop: '16px' }}>
              <Text type="secondary">No rules matched this event</Text>
            </div>
          </div>
        )}
      </Card>

      {/* Rewards Section */}
      {rewards.length > 0 && (
        <Card size="small" style={{ marginBottom: '16px' }}>
          <Title level={5} style={{ marginBottom: '16px' }}>
            Rewards Issued
          </Title>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
            {rewards.map((reward) => (
              <Card
                key={reward.key}
                size="small"
                style={{
                  background:
                    'linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%)',
                  border: '1px solid #faad14',
                  minWidth: '120px',
                }}
              >
                <Space
                  direction="vertical"
                  align="center"
                  style={{ width: '100%' }}
                >
                  <div style={{ fontSize: '20px', color: '#faad14' }}>
                    {getRewardIcon(reward.type)}
                  </div>
                  <Text
                    strong
                    style={{ fontSize: '12px', textAlign: 'center' }}
                  >
                    {reward.name}
                  </Text>
                  <Text style={{ fontSize: '10px', color: '#666' }}>
                    {reward.value}
                  </Text>
                </Space>
              </Card>
            ))}
          </div>
        </Card>
      )}

      {/* Summary Stats */}
      <Card
        size="small"
        style={{
          background: 'linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%)',
        }}
      >
        <Title level={5} style={{ marginBottom: '16px', textAlign: 'center' }}>
          Evaluation Summary
        </Title>
        <Row gutter={[16, 16]}>
          <Col xs={12} sm={6}>
            <Statistic
              title="Total Rules"
              value={rules.length}
              prefix={<CheckCircleOutlined style={{ color: '#1890ff' }} />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Col>
          <Col xs={12} sm={6}>
            <Statistic
              title="Triggered"
              value={getTriggeredRules()}
              prefix={<ThunderboltOutlined style={{ color: '#52c41a' }} />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Col>
          <Col xs={12} sm={6}>
            <Statistic
              title="Will Execute"
              value={getExecutableRules()}
              prefix={<TrophyOutlined style={{ color: '#faad14' }} />}
              valueStyle={{ color: '#faad14' }}
            />
          </Col>
          <Col xs={12} sm={6}>
            <Statistic
              title="Total Rewards"
              value={getTotalRewards()}
              prefix={<GiftOutlined style={{ color: '#722ed1' }} />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Col>
        </Row>
        {getTotalPoints() > 0 && (
          <div
            style={{
              textAlign: 'center',
              marginTop: '16px',
              padding: '12px',
              background: 'rgba(24, 144, 255, 0.1)',
              borderRadius: '6px',
            }}
          >
            <Text strong style={{ fontSize: '16px', color: '#1890ff' }}>
              🎯 {getTotalPoints()} Total Points Awarded
            </Text>
          </div>
        )}
      </Card>
    </div>
  )
}

export default VisualTrace
