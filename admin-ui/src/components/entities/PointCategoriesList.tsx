import React from 'react'
import { Tag } from 'antd'
import EntityList from '../EntityList'
import { usePointCategories } from '@/hooks/useEntities'
import type { PointCategory } from '@/types'

interface PointCategoriesListProps {
  onViewDetails: (id: string) => void
}

const PointCategoriesList: React.FC<PointCategoriesListProps> = ({
  onViewDetails,
}) => {
  const { data: pointCategories = [], isLoading, error } = usePointCategories()

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a: PointCategory, b: PointCategory) =>
        a.name.localeCompare(b.name),
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
    },
    {
      title: 'Aggregation',
      dataIndex: 'aggregation',
      key: 'aggregation',
      width: 120,
      render: (aggregation: string) => <Tag color="purple">{aggregation}</Tag>,
    },
  ]

  return (
    <EntityList
      title="Point Categories"
      data={pointCategories}
      loading={isLoading}
      error={error}
      columns={columns}
      onViewDetails={onViewDetails}
      emptyMessage="No point categories found"
    />
  )
}

export default PointCategoriesList
