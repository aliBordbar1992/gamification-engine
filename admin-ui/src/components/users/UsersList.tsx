import React, { useState, useMemo } from 'react'
import {
  Table,
  Input,
  Button,
  Space,
  Tag,
  Typography,
  Card,
  Row,
  Col,
  Tooltip,
} from 'antd'
import {
  FilterOutlined,
  EyeOutlined,
  UserOutlined,
  TrophyOutlined,
  CrownOutlined,
} from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useUsers } from '@/hooks/useUsers'
import type { UserSummaryDto } from '@/api/generated/models'

// Filter and search types
export interface UsersFilters {
  search?: string
}

const { Title, Text } = Typography
const { Search } = Input

interface UsersListProps {
  onViewUser?: (user: UserSummaryDto) => void
}

const UsersList: React.FC<UsersListProps> = ({ onViewUser }) => {
  const navigate = useNavigate()
  const [filters, setFilters] = useState<UsersFilters>({})
  const [searchText, setSearchText] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)

  const { data: usersData, isLoading, error } = useUsers(currentPage, pageSize)

  const handleViewUser = (user: UserSummaryDto) => {
    if (onViewUser) {
      onViewUser(user)
    } else {
      navigate(`/users/${user.userId}`)
    }
  }

  const clearFilters = () => {
    setFilters({})
    setSearchText('')
  }

  const columns = [
    {
      title: 'User ID',
      dataIndex: 'userId',
      key: 'userId',
      render: (userId: string) => (
        <div>
          <Text strong>{userId}</Text>
          <br />
          <Text type="secondary" style={{ fontSize: '12px' }}>
            <UserOutlined style={{ marginRight: 4 }} />
            User Profile
          </Text>
        </div>
      ),
    },
    {
      title: 'Total Points',
      dataIndex: 'totalPoints',
      key: 'totalPoints',
      render: (points: number) => (
        <Text strong style={{ color: '#1890ff' }}>
          {points?.toLocaleString() || 0}
        </Text>
      ),
      sorter: (a: UserSummaryDto, b: UserSummaryDto) =>
        (a.totalPoints || 0) - (b.totalPoints || 0),
    },
    {
      title: 'Badges',
      dataIndex: 'badgeCount',
      key: 'badgeCount',
      width: 100,
      render: (count: number) => (
        <Space>
          <TrophyOutlined style={{ color: '#faad14' }} />
          <Text>{count || 0}</Text>
        </Space>
      ),
      sorter: (a: UserSummaryDto, b: UserSummaryDto) =>
        (a.badgeCount || 0) - (b.badgeCount || 0),
    },
    {
      title: 'Trophies',
      dataIndex: 'trophyCount',
      key: 'trophyCount',
      width: 100,
      render: (count: number) => (
        <Space>
          <CrownOutlined style={{ color: '#722ed1' }} />
          <Text>{count || 0}</Text>
        </Space>
      ),
      sorter: (a: UserSummaryDto, b: UserSummaryDto) =>
        (a.trophyCount || 0) - (b.trophyCount || 0),
    },
    {
      title: 'Categories',
      dataIndex: 'pointsByCategory',
      key: 'categories',
      render: (pointsByCategory: { [key: string]: number } | null) => {
        if (!pointsByCategory)
          return <Text type="secondary">No categories</Text>

        const categories = Object.keys(pointsByCategory)
        return (
          <Space wrap>
            {categories.slice(0, 2).map((category) => (
              <Tag key={category} color="blue">
                {category}: {pointsByCategory[category]?.toLocaleString() || 0}
              </Tag>
            ))}
            {categories.length > 2 && (
              <Tooltip
                title={categories
                  .slice(2)
                  .map(
                    (cat) =>
                      `${cat}: ${pointsByCategory[cat]?.toLocaleString() || 0}`
                  )
                  .join(', ')}
              >
                <Tag color="default">+{categories.length - 2}</Tag>
              </Tooltip>
            )}
          </Space>
        )
      },
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_: unknown, record: UserSummaryDto) => (
        <Space>
          <Button
            type="link"
            icon={<EyeOutlined />}
            onClick={() => handleViewUser(record)}
          >
            View
          </Button>
        </Space>
      ),
    },
  ]

  const stats = useMemo(() => {
    const users = usersData?.users || []
    const total = usersData?.totalCount || 0
    const withPoints = users.filter(
      (user: UserSummaryDto) => (user.totalPoints || 0) > 0
    ).length
    const withBadges = users.filter(
      (user: UserSummaryDto) => (user.badgeCount || 0) > 0
    ).length
    return { total, withPoints, withBadges }
  }, [usersData])

  if (error) {
    return (
      <Card>
        <Text type="danger">Error loading users: {error.message}</Text>
      </Card>
    )
  }

  return (
    <div>
      {/* Stats Cards */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={8}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#1890ff' }}>
                {stats.total}
              </Title>
              <Text type="secondary">Total Users</Text>
            </div>
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#52c41a' }}>
                {stats.withPoints}
              </Title>
              <Text type="secondary">Users with Points</Text>
            </div>
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#faad14' }}>
                {stats.withBadges}
              </Title>
              <Text type="secondary">Users with Badges</Text>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Filters */}
      <Card style={{ marginBottom: 16 }}>
        <Row gutter={16} align="middle">
          <Col flex="auto">
            <Search
              placeholder="Search users by ID..."
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              onSearch={setSearchText}
              style={{ maxWidth: 400 }}
            />
          </Col>
          <Col>
            <Button
              icon={<FilterOutlined />}
              onClick={clearFilters}
              disabled={!Object.keys(filters).length && !searchText}
            >
              Clear Filters
            </Button>
          </Col>
        </Row>
      </Card>

      {/* Users Table */}
      <Card>
        <Table
          columns={columns}
          dataSource={usersData?.users || []}
          rowKey="userId"
          loading={isLoading}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: usersData?.totalCount || 0,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} of ${total} users`,
            onChange: (page, size) => {
              setCurrentPage(page)
              setPageSize(size || 20)
            },
          }}
          scroll={{ x: 800 }}
        />
      </Card>
    </div>
  )
}

export default UsersList
