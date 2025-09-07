import React, { useState } from 'react'
import { Tabs, Typography } from 'antd'
import { TrophyOutlined, CrownOutlined, StarOutlined, DollarOutlined } from '@ant-design/icons'

// Import entity list components
import BadgesList from '@/components/entities/BadgesList'
import TrophiesList from '@/components/entities/TrophiesList'
import LevelsList from '@/components/entities/LevelsList'
import PointCategoriesList from '@/components/entities/PointCategoriesList'

// Import entity details components
import BadgeDetails from '@/components/entities/BadgeDetails'
import TrophyDetails from '@/components/entities/TrophyDetails'
import LevelDetails from '@/components/entities/LevelDetails'
import PointCategoryDetails from '@/components/entities/PointCategoryDetails'

const { Title } = Typography

type EntityType = 'badges' | 'trophies' | 'levels' | 'point-categories'
type ViewMode = 'list' | 'details'

const Entities: React.FC = () => {
  const [viewMode, setViewMode] = useState<ViewMode>('list')
  const [selectedEntityType, setSelectedEntityType] = useState<EntityType>('badges')
  const [selectedEntityId, setSelectedEntityId] = useState<string>('')

  const handleViewDetails = (entityType: EntityType, id: string) => {
    setSelectedEntityType(entityType)
    setSelectedEntityId(id)
    setViewMode('details')
  }

  const handleBackToList = () => {
    setViewMode('list')
    setSelectedEntityId('')
  }

  const renderEntityDetails = () => {
    switch (selectedEntityType) {
      case 'badges':
        return <BadgeDetails id={selectedEntityId} onBack={handleBackToList} />
      case 'trophies':
        return <TrophyDetails id={selectedEntityId} onBack={handleBackToList} />
      case 'levels':
        return <LevelDetails id={selectedEntityId} onBack={handleBackToList} />
      case 'point-categories':
        return <PointCategoryDetails id={selectedEntityId} onBack={handleBackToList} />
      default:
        return null
    }
  }

  const tabItems = [
    {
      key: 'badges',
      label: (
        <span>
          <TrophyOutlined />
          Badges
        </span>
      ),
      children: (
        <BadgesList
          onViewDetails={(id) => handleViewDetails('badges', id)}
        />
      ),
    },
    {
      key: 'trophies',
      label: (
        <span>
          <CrownOutlined />
          Trophies
        </span>
      ),
      children: (
        <TrophiesList
          onViewDetails={(id) => handleViewDetails('trophies', id)}
        />
      ),
    },
    {
      key: 'levels',
      label: (
        <span>
          <StarOutlined />
          Levels
        </span>
      ),
      children: (
        <LevelsList
          onViewDetails={(id) => handleViewDetails('levels', id)}
        />
      ),
    },
    {
      key: 'point-categories',
      label: (
        <span>
          <DollarOutlined />
          Point Categories
        </span>
      ),
      children: (
        <PointCategoriesList
          onViewDetails={(id) => handleViewDetails('point-categories', id)}
        />
      ),
    },
  ]

  if (viewMode === 'details') {
    return (
      <div>
        <Title level={2}>Entities Management</Title>
        {renderEntityDetails()}
      </div>
    )
  }

  return (
    <div>
      <Title level={2}>Entities Management</Title>
      <Tabs
        defaultActiveKey="badges"
        items={tabItems}
        size="large"
        tabPosition="top"
      />
    </div>
  )
}

export default Entities
