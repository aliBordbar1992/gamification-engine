import React, { useState } from 'react'
import { Typography, Space, Divider } from 'antd'
import { UserOutlined } from '@ant-design/icons'
import UserLookup from '@/components/users/UserLookup'
import UserDetails from '@/components/users/UserDetails'
import UsersList from '@/components/users/UsersList'
import { useUserState } from '@/hooks/useUsers'

const { Title } = Typography

const Users: React.FC = () => {
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null)
  
  const { 
    data: userState, 
    isLoading, 
    error
  } = useUserState(selectedUserId || '')

  const handleUserFound = (userId: string) => {
    setSelectedUserId(userId)
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
            Browse all users or look up specific user gamification data including points, badges, trophies, and levels.
          </Typography.Text>
        </div>

        <UsersList onUserSelect={handleUserFound} />

        <Divider />

        <UserLookup 
          onUserFound={handleUserFound}
          isLoading={isLoading}
          error={error?.message}
        />

        {selectedUserId && userState && (
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
