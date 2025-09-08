import React, { useState } from 'react'
import { Table, Card, Typography, Tag, Space, Button, Spin, Alert } from 'antd'
import { UserOutlined, TrophyOutlined, CrownOutlined } from '@ant-design/icons'
import { useUsers } from '@/hooks/useUsers'
import type { UserSummaryDto } from '@/api/generated/models'

const { Title, Text } = Typography

interface UsersListProps {
  onUserSelect?: (userId: string) => void
}

const UsersList: React.FC<UsersListProps> = ({ onUserSelect }) => {
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)

  const { data: usersData, isLoading, error } = useUsers(currentPage, pageSize)

  const handleUserClick = (userId: string) => {
    if (onUserSelect) {
      onUserSelect(userId)
    }
  }

  const columns = [
    {
      title: 'User ID',
      dataIndex: 'userId',
      key: 'userId',
      render: (userId: string) => (
        <Button 
          type="link" 
          onClick={() => handleUserClick(userId)}
          style={{ padding: 0, height: 'auto' }}
        >
          <UserOutlined style={{ marginRight: 4 }} />
          {userId}
        </Button>
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
      sorter: (a: UserSummaryDto, b: UserSummaryDto) => (a.totalPoints || 0) - (b.totalPoints || 0),
    },
    {
      title: 'Badges',
      dataIndex: 'badgeCount',
      key: 'badgeCount',
      render: (count: number) => (
        <Space>
          <TrophyOutlined style={{ color: '#faad14' }} />
          <Text>{count || 0}</Text>
        </Space>
      ),
      sorter: (a: UserSummaryDto, b: UserSummaryDto) => (a.badgeCount || 0) - (b.badgeCount || 0),
    },
    {
      title: 'Trophies',
      dataIndex: 'trophyCount',
      key: 'trophyCount',
      render: (count: number) => (
        <Space>
          <CrownOutlined style={{ color: '#722ed1' }} />
          <Text>{count || 0}</Text>
        </Space>
      ),
      sorter: (a: UserSummaryDto, b: UserSummaryDto) => (a.trophyCount || 0) - (b.trophyCount || 0),
    },
    {
      title: 'Categories',
      dataIndex: 'pointsByCategory',
      key: 'categories',
      render: (pointsByCategory: { [key: string]: number } | null) => {
        if (!pointsByCategory) return <Text type="secondary">No categories</Text>
        
        const categories = Object.keys(pointsByCategory)
        return (
          <Space wrap>
            {categories.slice(0, 3).map((category) => (
              <Tag key={category} color="blue">
                {category}: {pointsByCategory[category]?.toLocaleString() || 0}
              </Tag>
            ))}
            {categories.length > 3 && (
              <Tag color="default">+{categories.length - 3} more</Tag>
            )}
          </Space>
        )
      },
    },
  ]

  if (error) {
    return (
      <Card>
        <Alert
          message="Error loading users"
          description={error.message || 'Failed to fetch users data'}
          type="error"
          showIcon
        />
      </Card>
    )
  }

  return (
    <Card>
      <div style={{ marginBottom: 16 }}>
        <Title level={4}>
          <UserOutlined style={{ marginRight: 8 }} />
          Users Overview
        </Title>
        <Text type="secondary">
          Showing {usersData?.users?.length || 0} of {usersData?.totalCount || 0} users
        </Text>
      </div>

      <Spin spinning={isLoading}>
        <Table
          columns={columns}
          dataSource={usersData?.users || []}
          rowKey="userId"
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
      </Spin>
    </Card>
  )
}

export default UsersList
