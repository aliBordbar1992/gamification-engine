import React, { useState } from 'react'
import { Input, Button, Card, Typography, Space, Alert } from 'antd'
import { SearchOutlined, UserOutlined } from '@ant-design/icons'

const { Title, Text } = Typography
const { Search } = Input

interface UserLookupProps {
  onUserFound: (userId: string) => void
  isLoading?: boolean
  error?: string
}

const UserLookup: React.FC<UserLookupProps> = ({
  onUserFound,
  isLoading,
  error,
}) => {
  const [userId, setUserId] = useState('')

  const handleSearch = () => {
    if (userId.trim()) {
      onUserFound(userId.trim())
    }
  }

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault()
      handleSearch()
    }
  }

  return (
    <Card>
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        <div>
          <Title level={4}>
            <UserOutlined style={{ marginRight: 8 }} />
            User Lookup
          </Title>
          <Text type="secondary">
            Enter a user ID to view their gamification data including points,
            badges, trophies, and levels.
          </Text>
        </div>

        {error && (
          <Alert message="Error" description={error} type="error" showIcon />
        )}

        <Space.Compact style={{ width: '100%' }}>
          <Search
            placeholder="Enter user ID..."
            value={userId}
            onChange={(e) => setUserId(e.target.value)}
            onKeyPress={handleKeyPress}
            onSearch={handleSearch}
            enterButton={
              <Button
                type="primary"
                icon={<SearchOutlined />}
                loading={isLoading}
                disabled={!userId.trim()}
              >
                Lookup
              </Button>
            }
            size="large"
          />
        </Space.Compact>

        <div style={{ fontSize: '12px', color: '#8c8c8c' }}>
          <Text type="secondary">
            Example user IDs: user123, player456, gamer789
          </Text>
        </div>
      </Space>
    </Card>
  )
}

export default UserLookup
