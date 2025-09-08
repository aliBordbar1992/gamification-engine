import React from 'react'
import { Typography } from 'antd'
import { useParams } from 'react-router-dom'
import UsersList from '@/components/users/UsersList'
import UserDetails from '@/components/users/UserDetails'

const { Title } = Typography

const Users: React.FC = () => {
  const { id } = useParams<{ id: string }>()

  if (id) {
    return <UserDetails userId={id} />
  }

  return (
    <div>
      <Title level={2}>User Management</Title>
      <UsersList />
    </div>
  )
}

export default Users
