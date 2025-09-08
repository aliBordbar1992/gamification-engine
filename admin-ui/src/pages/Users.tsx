import React from 'react'
import { Typography } from 'antd'
import { useParams, useNavigate } from 'react-router-dom'
import UsersList from '@/components/users/UsersList'
import UserDetails from '@/components/users/UserDetails'
import UserRewardHistory from '@/components/users/UserRewardHistory'
import type { UserSummaryDto } from '@/api/generated/models'

const { Title } = Typography

const Users: React.FC = () => {
  const { id, action } = useParams<{ id: string; action?: string }>()
  const navigate = useNavigate()

  const handleViewRewardHistory = (user: UserSummaryDto) => {
    // Navigate to reward history route
    navigate(`/users/${user.userId}/history`)
  }

  // Handle reward history route
  if (id && action === 'history') {
    return <UserRewardHistory userId={id} userName={id} />
  }

  // Handle user details route
  if (id) {
    return <UserDetails userId={id} />
  }

  // Default users list
  return (
    <div>
      <Title level={2}>User Management</Title>
      <UsersList onViewRewardHistory={handleViewRewardHistory} />
    </div>
  )
}

export default Users
