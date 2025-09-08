import React from 'react'
import { Card, Typography, Row, Col, Statistic } from 'antd'
import { RiseOutlined, TrophyOutlined, UserOutlined } from '@ant-design/icons'
import type { LeaderboardDto } from '@/api/generated/models'
import type { LeaderboardType } from '@/api/leaderboards'

const { Title } = Typography

interface LeaderboardChartProps {
  data: LeaderboardDto
  type: LeaderboardType
}

const LeaderboardChart: React.FC<LeaderboardChartProps> = ({ data, type }) => {
  const topEntries = data.entries?.slice(0, 5) || []
  const totalParticipants = data.totalCount || 0
  const topScore = data.topEntry?.points || 0

  const getTypeLabel = (type: LeaderboardType) => {
    switch (type) {
      case 'points':
        return 'Points'
      case 'badges':
        return 'Badges'
      case 'trophies':
        return 'Trophies'
      case 'level':
        return 'Level'
      default:
        return 'Score'
    }
  }

  const formatValue = (value: number, type: LeaderboardType) => {
    switch (type) {
      case 'points':
        return value.toLocaleString()
      case 'badges':
        return `${value} badge${value !== 1 ? 's' : ''}`
      case 'trophies':
        return `${value} troph${value !== 1 ? 'ies' : 'y'}`
      case 'level':
        return `Level ${value}`
      default:
        return value.toString()
    }
  }

  return (
    <Row gutter={[16, 16]}>
      <Col xs={24} sm={8}>
        <Card size="small">
          <Statistic
            title="Total Participants"
            value={totalParticipants}
            prefix={<UserOutlined />}
            valueStyle={{ color: '#1890ff' }}
          />
        </Card>
      </Col>
      <Col xs={24} sm={8}>
        <Card size="small">
          <Statistic
            title={`Top ${getTypeLabel(type)}`}
            value={topScore}
            prefix={<TrophyOutlined />}
            valueStyle={{ color: '#FFD700' }}
            formatter={(value) => formatValue(Number(value), type)}
          />
        </Card>
      </Col>
      <Col xs={24} sm={8}>
        <Card size="small">
          <Statistic
            title="Leader"
            value={data.topEntry?.displayName || data.topEntry?.userId || 'N/A'}
            prefix={<RiseOutlined />}
            valueStyle={{ color: '#52c41a' }}
          />
        </Card>
      </Col>
    </Row>
  )
}

export default LeaderboardChart
