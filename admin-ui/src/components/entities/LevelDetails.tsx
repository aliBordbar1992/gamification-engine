import React from 'react'
import { Tag } from 'antd'
import EntityDetails from '../EntityDetails'
import { useLevel } from '@/hooks/useEntities'
import type { EntityDetailsItem } from '../EntityDetails'

interface LevelDetailsProps {
  id: string
  onBack: () => void
}

const LevelDetails: React.FC<LevelDetailsProps> = ({ id, onBack }) => {
  const { data: level, isLoading, error } = useLevel(id)

  const detailsData: EntityDetailsItem[] = level
    ? [
        {
          label: 'ID',
          value: level.id ?? '',
          span: 1,
        },
        {
          label: 'Name',
          value: level.name ?? '',
          span: 2,
        },
        {
          label: 'Category',
          value: <Tag color="blue">{level.category}</Tag>,
          span: 1,
        },
        {
          label: 'Minimum Points',
          value: (
            <span
              style={{
                fontFamily: 'monospace',
                fontSize: '16px',
                fontWeight: 'bold',
              }}
            >
              {level.minPoints?.toLocaleString() ?? ''}
            </span>
          ),
          span: 2,
        },
      ]
    : []

  return (
    <EntityDetails
      title={level?.name || 'Level Details'}
      data={detailsData}
      loading={isLoading}
      error={error}
      onBack={onBack}
    />
  )
}

export default LevelDetails
