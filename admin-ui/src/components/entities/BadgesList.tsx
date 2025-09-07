import React from 'react'
import { Tag } from 'antd'
import { CheckCircleOutlined, EyeInvisibleOutlined } from '@ant-design/icons'
import EntityList from '../EntityList'
import { useBadges } from '@/hooks/useEntities'
import type { CreateBadgeDto } from '@/api/generated/models'

// Type alias for better readability
type Badge = CreateBadgeDto

interface BadgesListProps {
  onViewDetails: (id: string) => void
}

const BadgesList: React.FC<BadgesListProps> = ({ onViewDetails }) => {
  const { data: badges = [], isLoading, error } = useBadges()

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a: Badge, b: Badge) => a.name.localeCompare(b.name),
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
      title="Badges"
      data={badges}
      loading={isLoading}
      error={error}
      columns={columns}
      onViewDetails={onViewDetails}
      emptyMessage="No badges found"
    />
  )
}

export default BadgesList
