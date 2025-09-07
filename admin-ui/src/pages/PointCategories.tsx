import React from 'react'
import { Typography } from 'antd'
import { ArrowLeftOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import PointCategoriesList from '@/components/entities/PointCategoriesList'
import PointCategoryDetails from '@/components/entities/PointCategoryDetails'

const { Title } = Typography

const PointCategories: React.FC = () => {
  const navigate = useNavigate()
  const [selectedPointCategoryId, setSelectedPointCategoryId] = React.useState<
    string | null
  >(null)

  const handleViewDetails = (id: string) => {
    setSelectedPointCategoryId(id)
  }

  const handleBack = () => {
    setSelectedPointCategoryId(null)
  }

  if (selectedPointCategoryId) {
    return (
      <div>
        <div style={{ marginBottom: 16 }}>
          <ArrowLeftOutlined
            onClick={handleBack}
            style={{ cursor: 'pointer', marginRight: 8 }}
          />
          <Title level={2} style={{ display: 'inline' }}>
            Point Category Details
          </Title>
        </div>
        <PointCategoryDetails
          id={selectedPointCategoryId}
          onBack={handleBack}
        />
      </div>
    )
  }

  return (
    <div>
      <Title level={2}>Point Categories</Title>
      <PointCategoriesList onViewDetails={handleViewDetails} />
    </div>
  )
}

export default PointCategories
