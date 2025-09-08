import React, { useState } from 'react'
import { Typography, Space, Alert } from 'antd'
import { UserOutlined } from '@ant-design/icons'
import UserLookup from '@/components/users/UserLookup'
import UserDetails from '@/components/users/UserDetails'
import { useUserState } from '@/hooks/useUsers'

const { Title } = Typography

const Users: React.FC = () => {
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null)
  
  const { 
    data: userState, 
    isLoading, 
    error,
    refetch 
  } = useUserState(selectedUserId || '')

  const handleUserFound = (userId: string) => {
    setSelectedUserId(userId)
  }

  const handleClearUser = () => {
    setSelectedUserId(null)
  }

  return (
    <div>
      <Space direction="vertical" size="large" style={{ width: '100%' }}>
        <div>
          <Title level={2}>
            <UserOutlined style={{ marginRight: 8 }} />
            User Management
          </Title>
          <Typography.Text type="secondary">
            Look up user gamification data including points, badges, trophies, and levels.
          </Typography.Text>
        </div>

        <UserLookup 
          onUserFound={handleUserFound}
          isLoading={isLoading}
          error={error?.message}
        />

        {selectedUserId && (
          <div>
            <UserDetails 
              userState={userState}
              isLoading={isLoading}
            />
          </div>
        )}
      </Space>
    </div>
  )
}

export default Users
