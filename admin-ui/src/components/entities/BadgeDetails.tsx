import React from 'react'
import { Tag } from 'antd'
import { CheckCircleOutlined, EyeInvisibleOutlined } from '@ant-design/icons'
import EntityDetails from '../EntityDetails'
import { useBadge } from '@/hooks/useEntities'
import type { EntityDetailsItem } from '../EntityDetails'

interface BadgeDetailsProps {
  id: string
  onBack: () => void
}

const BadgeDetails: React.FC<BadgeDetailsProps> = ({ id, onBack }) => {
  const { data: badge, isLoading, error } = useBadge(id)

  const detailsData: EntityDetailsItem[] = badge
    ? [
        {
          label: 'ID',
          value: badge.id,
          span: 1,
        },
        {
          label: 'Name',
          value: badge.name,
          span: 2,
        },
        {
          label: 'Description',
          value: badge.description,
          span: 3,
        },
        {
          label: 'Status',
          value: (
            <Tag
              icon={
                badge.visible ? (
                  <CheckCircleOutlined />
                ) : (
                  <EyeInvisibleOutlined />
                )
              }
              color={badge.visible ? 'green' : 'default'}
            >
              {badge.visible ? 'Visible' : 'Hidden'}
            </Tag>
          ),
          span: 1,
        },
        {
          label: 'Image',
          value: badge.image || 'No image',
          span: 2,
        },
      ]
    : []

  return (
    <EntityDetails
      title={badge?.name || 'Badge Details'}
      data={detailsData}
      loading={isLoading}
      error={error}
      onBack={onBack}
    />
  )
}

export default BadgeDetails
