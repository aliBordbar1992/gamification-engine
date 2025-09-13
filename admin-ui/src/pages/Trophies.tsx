import React from 'react'
import { Typography } from 'antd'
import { ArrowLeftOutlined } from '@ant-design/icons'
import TrophiesList from '@/components/entities/TrophiesList'
import TrophyDetails from '@/components/entities/TrophyDetails'

const { Title } = Typography

const Trophies: React.FC = () => {
  const [selectedTrophyId, setSelectedTrophyId] = React.useState<string | null>(
    null
  )

  const handleViewDetails = (id: string) => {
    setSelectedTrophyId(id)
  }

  const handleBack = () => {
    setSelectedTrophyId(null)
  }

  if (selectedTrophyId) {
    return (
      <div>
        <div style={{ marginBottom: 16 }}>
          <ArrowLeftOutlined
            onClick={handleBack}
            style={{ cursor: 'pointer', marginRight: 8 }}
          />
          <Title level={2} style={{ display: 'inline' }}>
            Trophy Details
          </Title>
        </div>
        <TrophyDetails id={selectedTrophyId} onBack={handleBack} />
      </div>
    )
  }

  return (
    <div>
      <TrophiesList onViewDetails={handleViewDetails} />
    </div>
  )
}

export default Trophies
