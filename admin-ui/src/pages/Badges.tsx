import React from 'react'
import { useParams } from 'react-router-dom'
import BadgesList from '@/components/entities/BadgesList'
import EntityDetailPage from '@/components/EntityDetailPage'

const Badges: React.FC = () => {
  const { id } = useParams<{ id: string }>()

  if (id) {
    return <EntityDetailPage entityType="badges" />
  }

  return (
    <div>
      <BadgesList />
    </div>
  )
}

export default Badges
