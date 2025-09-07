import React from 'react'
import { Tag } from 'antd'
import EntityList from '../EntityList'
import { useLevels } from '@/hooks/useEntities'
import type { Level } from '@/types'

interface LevelsListProps {
  onViewDetails: (id: string) => void
}

const LevelsList: React.FC<LevelsListProps> = ({ onViewDetails }) => {
  const { data: levels = [], isLoading, error } = useLevels()

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a: Level, b: Level) => a.name.localeCompare(b.name),
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
    },
    {
      title: 'Category',
      dataIndex: 'category',
      key: 'category',
      width: 120,
      render: (category: string) => <Tag color="blue">{category}</Tag>,
    },
    {
      title: 'Min Points',
      dataIndex: 'minPoints',
      key: 'minPoints',
      width: 120,
      sorter: (a: Level, b: Level) => a.minPoints - b.minPoints,
      render: (points: number) => (
        <span style={{ fontFamily: 'monospace' }}>
          {points.toLocaleString()}
        </span>
      ),
    },
  ]

  return (
    <EntityList
      title="Levels"
      data={levels}
      loading={isLoading}
      error={error}
      columns={columns}
      onViewDetails={onViewDetails}
      emptyMessage="No levels found"
    />
  )
}

export default LevelsList
