import React from 'react'
import { useParams } from 'react-router-dom'
import TrophiesList from '@/components/entities/TrophiesList'
import EntityDetailPage from '@/components/EntityDetailPage'

const Trophies: React.FC = () => {
  const { id } = useParams<{ id: string }>()

  if (id) {
    return <EntityDetailPage entityType="trophies" />
  }

  return (
    <div>
      <TrophiesList />
    </div>
  )
}

export default Trophies
