import React from 'react'
import { Typography, Card, Row, Col, Statistic } from 'antd'
import { TrophyOutlined, CrownOutlined, StarOutlined, DollarOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useBadges, useTrophies, useLevels, usePointCategories } from '@/hooks/useEntities'

const { Title, Text } = Typography

const Entities: React.FC = () => {
  const navigate = useNavigate()
  
  const { data: badges = [], isLoading: badgesLoading } = useBadges()
  const { data: trophies = [], isLoading: trophiesLoading } = useTrophies()
  const { data: levels = [], isLoading: levelsLoading } = useLevels()
  const { data: pointCategories = [], isLoading: pointCategoriesLoading } = usePointCategories()

  const entityCards = [
    {
      title: 'Badges',
      icon: <TrophyOutlined style={{ fontSize: '24px', color: '#1890ff' }} />,
      count: badges.length,
      loading: badgesLoading,
      path: '/entities/badges',
      description: 'Award badges to users for achievements',
    },
    {
      title: 'Trophies',
      icon: <CrownOutlined style={{ fontSize: '24px', color: '#52c41a' }} />,
      count: trophies.length,
      loading: trophiesLoading,
      path: '/entities/trophies',
      description: 'Special trophies for major accomplishments',
    },
    {
      title: 'Levels',
      icon: <StarOutlined style={{ fontSize: '24px', color: '#faad14' }} />,
      count: levels.length,
      loading: levelsLoading,
      path: '/entities/levels',
      description: 'User progression levels based on points',
    },
    {
      title: 'Point Categories',
      icon: <DollarOutlined style={{ fontSize: '24px', color: '#722ed1' }} />,
      count: pointCategories.length,
      loading: pointCategoriesLoading,
      path: '/entities/point-categories',
      description: 'Different types of points users can earn',
    },
  ]

  return (
    <div>
      <Title level={2}>Entities Overview</Title>
      <Text type="secondary" style={{ fontSize: '16px', marginBottom: '24px', display: 'block' }}>
        Manage badges, trophies, levels, and point categories
      </Text>
      
      <Row gutter={[16, 16]}>
        {entityCards.map((entity) => (
          <Col xs={24} sm={12} lg={6} key={entity.title}>
            <Card
              hoverable
              onClick={() => navigate(entity.path)}
              style={{ height: '100%' }}
            >
              <div style={{ textAlign: 'center' }}>
                <div style={{ marginBottom: '16px' }}>
                  {entity.icon}
                </div>
                <Title level={4} style={{ marginBottom: '8px' }}>
                  {entity.title}
                </Title>
                <Statistic
                  value={entity.count}
                  loading={entity.loading}
                  valueStyle={{ fontSize: '24px', fontWeight: 'bold' }}
                />
                <Text type="secondary" style={{ fontSize: '12px', display: 'block', marginTop: '8px' }}>
                  {entity.description}
                </Text>
              </div>
            </Card>
          </Col>
        ))}
      </Row>
    </div>
  )
}

export default Entities