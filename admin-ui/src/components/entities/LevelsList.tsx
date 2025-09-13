import React from 'react'
import { Tag } from 'antd'
import EntityList from '../EntityList'
import { useLevels } from '@/hooks/useEntities'
import type { LevelDto } from '@/api/generated/models'

interface LevelsListProps {
  onViewDetails?: (id: string) => void
}

const LevelsList: React.FC<LevelsListProps> = ({ onViewDetails }) => {
  const { data: levels = [], isLoading, error } = useLevels()

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a: LevelDto, b: LevelDto) =>
        a.name?.localeCompare(b.name ?? '') ?? 0,
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
      sorter: (a: LevelDto, b: LevelDto) =>
        a.minPoints ? (b.minPoints ? a.minPoints - b.minPoints : 1) : -1,
      render: (points: number | undefined) => (
        <span style={{ fontFamily: 'monospace' }}>
          {points?.toLocaleString() ?? ''}
        </span>
      ),
    },
  ]

  return (
    <EntityList
      title="Levels"
      data={levels.map((level: LevelDto | undefined) => ({
        id: level?.id ?? '',
        name: level?.name ?? '',
        description: undefined,
        category: level?.category ?? '',
        minPoints: level?.minPoints ?? 0,
      }))}
      loading={isLoading}
      error={error}
      columns={columns}
      onViewDetails={onViewDetails}
      emptyMessage="No levels found"
      entityType="levels"
    />
  )
}

export default LevelsList
