import React from 'react'
import { Tag } from 'antd'
import { CheckCircleOutlined, EyeInvisibleOutlined } from '@ant-design/icons'
import EntityList from '../EntityList'
import { useTrophies } from '@/hooks/useEntities'
import type { CreateTrophyDto } from '@/api/generated/models'

// Type alias for better readability
type Trophy = CreateTrophyDto

interface TrophiesListProps {
  onViewDetails?: (id: string) => void
}

const TrophiesList: React.FC<TrophiesListProps> = ({ onViewDetails }) => {
  const { data: trophies = [], isLoading, error } = useTrophies()

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a: Trophy, b: Trophy) => a.name.localeCompare(b.name),
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
    },
    {
      title: 'Status',
      dataIndex: 'visible',
      key: 'visible',
      width: 100,
      render: (visible: boolean) => (
        <Tag
          icon={visible ? <CheckCircleOutlined /> : <EyeInvisibleOutlined />}
          color={visible ? 'green' : 'default'}
        >
          {visible ? 'Visible' : 'Hidden'}
        </Tag>
      ),
    },
  ]

  return (
    <EntityList
      title="Trophies"
      data={trophies}
      loading={isLoading}
      error={error}
      columns={columns}
      onViewDetails={onViewDetails}
      emptyMessage="No trophies found"
      entityType="trophies"
    />
  )
}

export default TrophiesList
