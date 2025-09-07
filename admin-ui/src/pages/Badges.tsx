import React from 'react'
import { Typography } from 'antd'
import { ArrowLeftOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import BadgesList from '@/components/entities/BadgesList'
import BadgeDetails from '@/components/entities/BadgeDetails'

const { Title } = Typography

const Badges: React.FC = () => {
  const navigate = useNavigate()
  const [selectedBadgeId, setSelectedBadgeId] = React.useState<string | null>(
    null
  )

  const handleViewDetails = (id: string) => {
    setSelectedBadgeId(id)
  }

  const handleBack = () => {
    setSelectedBadgeId(null)
  }

  if (selectedBadgeId) {
    return (
      <div>
        <div style={{ marginBottom: 16 }}>
          <ArrowLeftOutlined
            onClick={handleBack}
            style={{ cursor: 'pointer', marginRight: 8 }}
          />
          <Title level={2} style={{ display: 'inline' }}>
            Badge Details
          </Title>
        </div>
        <BadgeDetails id={selectedBadgeId} onBack={handleBack} />
      </div>
    )
  }

  return (
    <div>
      <Title level={2}>Badges</Title>
      <BadgesList onViewDetails={handleViewDetails} />
    </div>
  )
}

export default Badges
