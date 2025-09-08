import React from 'react'
import { Table, Typography, Space, Avatar } from 'antd'
import {
  TrophyOutlined,
  CrownOutlined,
  NumberOutlined,
} from '@ant-design/icons'
import type {
  LeaderboardDto,
  LeaderboardEntryDto,
} from '@/api/generated/models'
import type { LeaderboardType } from '@/api/leaderboards'

const { Text } = Typography

interface LeaderboardTableProps {
  data: LeaderboardDto
  type: LeaderboardType
  loading?: boolean
}

const LeaderboardTable: React.FC<LeaderboardTableProps> = ({
  data,
  type,
  loading = false,
}) => {
  const getRankIcon = (rank: number) => {
    if (rank === 1) return <CrownOutlined style={{ color: '#FFD700' }} />
    if (rank === 2) return <TrophyOutlined style={{ color: '#C0C0C0' }} />
    if (rank === 3) return <TrophyOutlined style={{ color: '#CD7F32' }} />
    return <NumberOutlined />
  }

  const getRankColor = (rank: number) => {
    if (rank === 1) return '#FFD700'
    if (rank === 2) return '#C0C0C0'
    if (rank === 3) return '#CD7F32'
    return undefined
  }

  const formatPoints = (points: number, type: LeaderboardType) => {
    switch (type) {
      case 'points':
        return points.toLocaleString()
      case 'badges':
        return `${points} badge${points !== 1 ? 's' : ''}`
      case 'trophies':
        return `${points} troph${points !== 1 ? 'ies' : 'y'}`
      case 'level':
        return `Level ${points}`
      default:
        return points.toString()
    }
  }

  const columns = [
    {
      title: 'Rank',
      dataIndex: 'rank',
      key: 'rank',
      width: 80,
      render: (rank: number) => (
        <Space>
          {getRankIcon(rank)}
          <Text strong style={{ color: getRankColor(rank) }}>
            #{rank}
          </Text>
        </Space>
      ),
    },
    {
      title: 'User',
      dataIndex: 'userId',
      key: 'userId',
      render: (userId: string, record: LeaderboardEntryDto) => (
        <Space>
          <Avatar size="small" style={{ backgroundColor: '#1890ff' }}>
            {record.displayName?.charAt(0).toUpperCase() ||
              userId.charAt(0).toUpperCase()}
          </Avatar>
          <div>
            <div>{record.displayName || userId}</div>
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {userId}
            </Text>
          </div>
        </Space>
      ),
    },
    {
      title:
        type === 'points'
          ? 'Points'
          : type === 'badges'
          ? 'Badges'
          : type === 'trophies'
          ? 'Trophies'
          : 'Level',
      dataIndex: 'points',
      key: 'points',
      render: (points: number) => (
        <Text strong style={{ fontSize: '16px' }}>
          {formatPoints(points, type)}
        </Text>
      ),
    },
  ]

  return (
    <Table
      columns={columns}
      dataSource={data.entries || []}
      loading={loading}
      pagination={false}
      rowKey="userId"
      size="middle"
      style={{ marginTop: '16px' }}
    />
  )
}

export default LeaderboardTable
