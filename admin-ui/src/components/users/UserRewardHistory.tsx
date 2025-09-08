import React, { useState } from 'react'
import {
  Card,
  Table,
  Typography,
  Space,
  Tag,
  Button,
  Spin,
  Alert,
  Row,
  Col,
  Divider,
} from 'antd'
import {
  ArrowLeftOutlined,
  HistoryOutlined,
  TrophyOutlined,
  CrownOutlined,
  GiftOutlined,
} from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useUserRewardHistory } from '@/hooks/useUsers'
import type { UserRewardHistoryEntryDto } from '@/api/generated/models'

const { Title, Text } = Typography

interface UserRewardHistoryProps {
  userId: string
  userName?: string
}

const UserRewardHistory: React.FC<UserRewardHistoryProps> = ({
  userId,
  userName,
}) => {
  const navigate = useNavigate()
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)

  const {
    data: rewardHistoryData,
    isLoading,
    error,
  } = useUserRewardHistory(userId, currentPage, pageSize)

  const handleBack = () => {
    navigate('/users')
  }

  const getRewardTypeIcon = (rewardType: string) => {
    switch (rewardType.toLowerCase()) {
      case 'badge':
        return <TrophyOutlined style={{ color: '#faad14' }} />
      case 'trophy':
        return <CrownOutlined style={{ color: '#722ed1' }} />
      case 'points':
        return <GiftOutlined style={{ color: '#1890ff' }} />
      default:
        return <HistoryOutlined />
    }
  }

  const getRewardTypeColor = (rewardType: string) => {
    switch (rewardType.toLowerCase()) {
      case 'badge':
        return 'gold'
      case 'trophy':
        return 'purple'
      case 'points':
        return 'blue'
      default:
        return 'default'
    }
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString()
  }

  const columns = [
    {
      title: 'Type',
      dataIndex: 'rewardType',
      key: 'rewardType',
      width: 100,
      render: (rewardType: string) => (
        <Space>
          {getRewardTypeIcon(rewardType)}
          <Tag color={getRewardTypeColor(rewardType)}>
            {rewardType?.charAt(0).toUpperCase() + rewardType?.slice(1) ||
              'Unknown'}
          </Tag>
        </Space>
      ),
    },
    {
      title: 'Reward',
      dataIndex: 'rewardName',
      key: 'rewardName',
      render: (rewardName: string, record: UserRewardHistoryEntryDto) => (
        <div>
          <Text strong>
            {rewardName || record.rewardId || 'Unknown Reward'}
          </Text>
          {record.rewardId && (
            <div>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                ID: {record.rewardId}
              </Text>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Amount',
      dataIndex: 'pointsAmount',
      key: 'pointsAmount',
      width: 120,
      render: (pointsAmount: number, record: UserRewardHistoryEntryDto) => (
        <div>
          <Text strong style={{ color: '#1890ff' }}>
            {pointsAmount?.toLocaleString() || 0}
          </Text>
          {record.pointCategory && (
            <div>
              <Tag color="blue">{record.pointCategory}</Tag>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Trigger',
      dataIndex: 'triggerEventType',
      key: 'triggerEventType',
      render: (triggerEventType: string, record: UserRewardHistoryEntryDto) => (
        <div>
          <Text>{triggerEventType || 'Unknown'}</Text>
          {record.triggerEventId && (
            <div>
              <Text type="secondary" style={{ fontSize: '12px' }}>
                Event: {record.triggerEventId}
              </Text>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Awarded At',
      dataIndex: 'awardedAt',
      key: 'awardedAt',
      width: 180,
      render: (awardedAt: string) => (
        <Text type="secondary">{formatDate(awardedAt)}</Text>
      ),
      sorter: (a: UserRewardHistoryEntryDto, b: UserRewardHistoryEntryDto) =>
        new Date(a.awardedAt || '').getTime() -
        new Date(b.awardedAt || '').getTime(),
    },
  ]

  const stats = React.useMemo(() => {
    if (!rewardHistoryData?.entries) return null

    const entries = rewardHistoryData.entries
    const totalRewards = entries.length
    const totalPoints = entries
      .filter((e) => e.rewardType === 'points')
      .reduce((sum, e) => sum + (e.pointsAmount || 0), 0)
    const badgesEarned = entries.filter((e) => e.rewardType === 'badge').length
    const trophiesEarned = entries.filter(
      (e) => e.rewardType === 'trophy'
    ).length

    return { totalRewards, totalPoints, badgesEarned, trophiesEarned }
  }, [rewardHistoryData])

  if (error) {
    return (
      <Card>
        <Alert
          message="Error loading reward history"
          description={error.message || 'Failed to fetch reward history data'}
          type="error"
          showIcon
        />
        <Button
          type="primary"
          icon={<ArrowLeftOutlined />}
          onClick={handleBack}
          style={{ marginTop: 16 }}
        >
          Back to Users
        </Button>
      </Card>
    )
  }

  if (isLoading) {
    return (
      <Card>
        <Spin
          size="large"
          style={{ display: 'block', textAlign: 'center', padding: '50px' }}
        />
      </Card>
    )
  }

  return (
    <div>
      {/* Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Space>
              <Button
                type="text"
                icon={<ArrowLeftOutlined />}
                onClick={handleBack}
              >
                Back to Users
              </Button>
              <Divider type="vertical" />
              <Title level={2} style={{ margin: 0 }}>
                <HistoryOutlined style={{ marginRight: 8 }} />
                Reward History - {userName || userId}
              </Title>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Stats Cards */}
      {stats && (
        <Row gutter={16} style={{ marginBottom: 24 }}>
          <Col span={6}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <Title level={3} style={{ margin: 0, color: '#1890ff' }}>
                  {stats.totalRewards.toLocaleString()}
                </Title>
                <Text type="secondary">Total Rewards</Text>
              </div>
            </Card>
          </Col>
          <Col span={6}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <Title level={3} style={{ margin: 0, color: '#52c41a' }}>
                  {stats.totalPoints.toLocaleString()}
                </Title>
                <Text type="secondary">Total Points</Text>
              </div>
            </Card>
          </Col>
          <Col span={6}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <Title level={3} style={{ margin: 0, color: '#faad14' }}>
                  {stats.badgesEarned}
                </Title>
                <Text type="secondary">Badges</Text>
              </div>
            </Card>
          </Col>
          <Col span={6}>
            <Card>
              <div style={{ textAlign: 'center' }}>
                <Title level={3} style={{ margin: 0, color: '#722ed1' }}>
                  {stats.trophiesEarned}
                </Title>
                <Text type="secondary">Trophies</Text>
              </div>
            </Card>
          </Col>
        </Row>
      )}

      {/* Reward History Table */}
      <Card>
        <Title level={4}>
          <HistoryOutlined style={{ marginRight: 8 }} />
          Reward History ({rewardHistoryData?.totalCount || 0})
        </Title>
        <Table
          columns={columns}
          dataSource={rewardHistoryData?.entries || []}
          rowKey="id"
          loading={isLoading}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: rewardHistoryData?.totalCount || 0,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} of ${total} rewards`,
            onChange: (page, size) => {
              setCurrentPage(page)
              setPageSize(size || 20)
            },
          }}
          scroll={{ x: 1000 }}
        />
      </Card>
    </div>
  )
}

export default UserRewardHistory
