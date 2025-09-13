import React from 'react'
import { useParams } from 'react-router-dom'
import LevelsList from '@/components/entities/LevelsList'
import EntityDetailPage from '@/components/EntityDetailPage'

const Levels: React.FC = () => {
  const { id } = useParams<{ id: string }>()

  if (id) {
    return <EntityDetailPage entityType="levels" />
  }

  return (
    <div>
      <LevelsList />
    </div>
  )
}

export default Levels
