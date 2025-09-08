import React from 'react'
import { Card, Select, Space, Typography, Button, Row, Col } from 'antd'
import { ReloadOutlined } from '@ant-design/icons'
import type { LeaderboardType, TimeRange } from '@/api/leaderboards'
import { usePointCategories } from '@/hooks/useEntities'

const { Text } = Typography
const { Option } = Select

interface LeaderboardSelectorProps {
  type: LeaderboardType
  category?: string
  timeRange: TimeRange
  onTypeChange: (type: LeaderboardType) => void
  onCategoryChange: (category: string | undefined) => void
  onTimeRangeChange: (timeRange: TimeRange) => void
  onRefresh: () => void
  loading?: boolean
}

const LeaderboardSelector: React.FC<LeaderboardSelectorProps> = ({
  type,
  category,
  timeRange,
  onTypeChange,
  onCategoryChange,
  onTimeRangeChange,
  onRefresh,
  loading = false,
}) => {
  const { data: pointCategories } = usePointCategories()

  const getCategoryOptions = () => {
    if (type === 'points' || type === 'level') {
      return pointCategories?.map((cat) => (
        <Option key={cat.id} value={cat.id}>
          {cat.name} ({cat.id})
        </Option>
      ))
    }
    return null
  }

  const shouldShowCategory = type === 'points' || type === 'level'

  return (
    <Card>
      <Row gutter={[16, 16]} align="middle">
        <Col xs={24} sm={6}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <Text strong>Leaderboard Type</Text>
            <Select
              value={type}
              onChange={onTypeChange}
              style={{ width: '100%' }}
              size="large"
            >
              <Option value="points">Points</Option>
              <Option value="badges">Badges</Option>
              <Option value="trophies">Trophies</Option>
              <Option value="level">Levels</Option>
            </Select>
          </Space>
        </Col>

        {shouldShowCategory && (
          <Col xs={24} sm={6}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text strong>Category</Text>
              <Select
                value={category}
                onChange={onCategoryChange}
                style={{ width: '100%' }}
                size="large"
                placeholder="Select category"
                allowClear
              >
                {getCategoryOptions()}
              </Select>
            </Space>
          </Col>
        )}

        <Col xs={24} sm={6}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <Text strong>Time Range</Text>
            <Select
              value={timeRange}
              onChange={onTimeRangeChange}
              style={{ width: '100%' }}
              size="large"
            >
              <Option value="daily">Daily</Option>
              <Option value="weekly">Weekly</Option>
              <Option value="monthly">Monthly</Option>
              <Option value="alltime">All Time</Option>
            </Select>
          </Space>
        </Col>

        <Col xs={24} sm={6}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <Text strong>Actions</Text>
            <Button
              icon={<ReloadOutlined />}
              onClick={onRefresh}
              loading={loading}
              size="large"
              style={{ width: '100%' }}
            >
              Refresh
            </Button>
          </Space>
        </Col>
      </Row>
    </Card>
  )
}

export default LeaderboardSelector
