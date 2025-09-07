import React from 'react'
import { Typography } from 'antd'
import { ArrowLeftOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import LevelsList from '@/components/entities/LevelsList'
import LevelDetails from '@/components/entities/LevelDetails'

const { Title } = Typography

const Levels: React.FC = () => {
  const navigate = useNavigate()
  const [selectedLevelId, setSelectedLevelId] = React.useState<string | null>(
    null
  )

  const handleViewDetails = (id: string) => {
    setSelectedLevelId(id)
  }

  const handleBack = () => {
    setSelectedLevelId(null)
  }

  if (selectedLevelId) {
    return (
      <div>
        <div style={{ marginBottom: 16 }}>
          <ArrowLeftOutlined
            onClick={handleBack}
            style={{ cursor: 'pointer', marginRight: 8 }}
          />
          <Title level={2} style={{ display: 'inline' }}>
            Level Details
          </Title>
        </div>
        <LevelDetails id={selectedLevelId} onBack={handleBack} />
      </div>
    )
  }

  return (
    <div>
      <Title level={2}>Levels</Title>
      <LevelsList onViewDetails={handleViewDetails} />
    </div>
  )
}

export default Levels
