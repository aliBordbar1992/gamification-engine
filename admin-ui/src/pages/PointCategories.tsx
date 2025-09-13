import React from 'react'
import { useParams } from 'react-router-dom'
import PointCategoriesList from '@/components/entities/PointCategoriesList'
import EntityDetailPage from '@/components/EntityDetailPage'

const PointCategories: React.FC = () => {
  const { id } = useParams<{ id: string }>()

  if (id) {
    return <EntityDetailPage entityType="point-categories" />
  }

  return (
    <div>
      <PointCategoriesList />
    </div>
  )
}

export default PointCategories
