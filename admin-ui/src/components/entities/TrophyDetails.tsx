import React from 'react'
import { Tag } from 'antd'
import { CheckCircleOutlined, EyeInvisibleOutlined } from '@ant-design/icons'
import EntityDetails from '../EntityDetails'
import { useTrophy } from '@/hooks/useEntities'
import type { EntityDetailsItem } from '../EntityDetails'

interface TrophyDetailsProps {
  id: string
  onBack: () => void
}

const TrophyDetails: React.FC<TrophyDetailsProps> = ({ id, onBack }) => {
  const { data: trophy, isLoading, error } = useTrophy(id)

  const detailsData: EntityDetailsItem[] = trophy
    ? [
        {
          label: 'ID',
          value: trophy.id,
          span: 1,
        },
        {
          label: 'Name',
          value: trophy.name,
          span: 2,
        },
        {
          label: 'Description',
          value: trophy.description,
          span: 3,
        },
        {
          label: 'Status',
          value: (
            <Tag
              icon={
                trophy.visible ? (
                  <CheckCircleOutlined />
                ) : (
                  <EyeInvisibleOutlined />
                )
              }
              color={trophy.visible ? 'green' : 'default'}
            >
              {trophy.visible ? 'Visible' : 'Hidden'}
            </Tag>
          ),
          span: 1,
        },
        {
          label: 'Image',
          value: trophy.image || 'No image',
          span: 2,
        },
      ]
    : []

  return (
    <EntityDetails
      title={trophy?.name || 'Trophy Details'}
      data={detailsData}
      loading={isLoading}
      error={error}
      onBack={onBack}
    />
  )
}

export default TrophyDetails
