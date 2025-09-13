import React from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { Typography } from 'antd'
import { ArrowLeftOutlined } from '@ant-design/icons'
import BadgeDetails from './entities/BadgeDetails'
import TrophyDetails from './entities/TrophyDetails'
import LevelDetails from './entities/LevelDetails'
import PointCategoryDetails from './entities/PointCategoryDetails'

const { Title } = Typography

interface EntityDetailPageProps {
  entityType: 'badges' | 'trophies' | 'levels' | 'point-categories'
}

const EntityDetailPage: React.FC<EntityDetailPageProps> = ({ entityType }) => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()

  const handleBack = () => {
    navigate(`/entities/${entityType}`)
  }

  const getEntityTitle = () => {
    switch (entityType) {
      case 'badges':
        return 'Badge Details'
      case 'trophies':
        return 'Trophy Details'
      case 'levels':
        return 'Level Details'
      case 'point-categories':
        return 'Point Category Details'
      default:
        return 'Entity Details'
    }
  }

  const renderEntityDetails = () => {
    if (!id) return null

    switch (entityType) {
      case 'badges':
        return <BadgeDetails id={id} onBack={handleBack} />
      case 'trophies':
        return <TrophyDetails id={id} onBack={handleBack} />
      case 'levels':
        return <LevelDetails id={id} onBack={handleBack} />
      case 'point-categories':
        return <PointCategoryDetails id={id} onBack={handleBack} />
      default:
        return null
    }
  }

  return (
    <div>
      <div style={{ marginBottom: 16 }}>
        <Title level={2} style={{ display: 'inline' }}>
          {getEntityTitle()}
        </Title>
      </div>
      {renderEntityDetails()}
    </div>
  )
}

export default EntityDetailPage
