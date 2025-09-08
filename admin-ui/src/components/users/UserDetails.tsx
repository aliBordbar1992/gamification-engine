import React, { useState } from 'react'
import {
  Card,
  Typography,
  Tag,
  Space,
  Button,
  Tabs,
  Row,
  Col,
  Descriptions,
  Badge,
  Divider,
  Spin,
  Alert,
} from 'antd'
import {
  ArrowLeftOutlined,
  CopyOutlined,
  UserOutlined,
  TrophyOutlined,
  CrownOutlined,
  StarOutlined,
} from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useUserState } from '@/hooks/useUsers'
import type { UserStateDto } from '@/api/generated/models'

const { Title, Text, Paragraph } = Typography
const { TabPane } = Tabs

interface UserDetailsProps {
  userId: string
}

const UserDetails: React.FC<UserDetailsProps> = ({ userId }) => {
  const navigate = useNavigate()
  const { data: userState, isLoading, error } = useUserState(userId)
  const [activeTab, setActiveTab] = useState('overview')

  const handleBack = () => {
    navigate('/users')
  }

  const handleCopyJson = () => {
    if (userState) {
      navigator.clipboard.writeText(JSON.stringify(userState, null, 2))
    }
  }

  if (error) {
    return (
      <Card>
        <Alert
          message="Error loading user details"
          description={error.message || 'Failed to fetch user data'}
          type="error"
          showIcon
        />
        <Button
          type="primary"
          icon={<ArrowLeftOutlined />}
          onClick={handleBack}
          style={{ marginTop: 16 }}
        >
          Back to Users
        </Button>
      </Card>
    )
  }

  if (isLoading) {
    return (
      <Card>
        <Spin
          size="large"
          style={{ display: 'block', textAlign: 'center', padding: '50px' }}
        />
      </Card>
    )
  }

  if (!userState) {
    return (
      <Card>
        <Alert
          message="User not found"
          description={`No data found for user: ${userId}`}
          type="warning"
          showIcon
        />
        <Button
          type="primary"
          icon={<ArrowLeftOutlined />}
          onClick={handleBack}
          style={{ marginTop: 16 }}
        >
          Back to Users
        </Button>
      </Card>
    )
  }

  // Calculate total points from categories
  const totalPoints = userState.pointsByCategory
    ? Object.values(userState.pointsByCategory).reduce(
        (sum, points) => sum + (points || 0),
        0
      )
    : 0

  // Get the highest level from currentLevelsByCategory
  const currentLevel = userState.currentLevelsByCategory
    ? Object.values(userState.currentLevelsByCategory)[0] // Get first level (assuming single category for now)
    : null

  return (
    <div>
      {/* Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Space>
              <Button
                type="text"
                icon={<ArrowLeftOutlined />}
                onClick={handleBack}
              >
                Back to Users
              </Button>
              <Divider type="vertical" />
              <Title level={2} style={{ margin: 0 }}>
                <UserOutlined style={{ marginRight: 8 }} />
                {userId}
              </Title>
            </Space>
          </Col>
          <Col>
            <Space>
              <Button icon={<CopyOutlined />} onClick={handleCopyJson}>
                Copy JSON
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Stats Cards */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#1890ff' }}>
                {totalPoints.toLocaleString()}
              </Title>
              <Text type="secondary">Total Points</Text>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#faad14' }}>
                {userState.badges?.length || 0}
              </Title>
              <Text type="secondary">Badges</Text>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#722ed1' }}>
                {userState.trophies?.length || 0}
              </Title>
              <Text type="secondary">Trophies</Text>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#52c41a' }}>
                {currentLevel?.name || 'No Level'}
              </Title>
              <Text type="secondary">Current Level</Text>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Details Tabs */}
      <Card>
        <Tabs activeKey={activeTab} onChange={setActiveTab}>
          <TabPane tab="Overview" key="overview">
            <Row gutter={24}>
              <Col span={12}>
                <Title level={4}>User Information</Title>
                <Descriptions column={1} size="small">
                  <Descriptions.Item label="User ID">
                    <Text code>{userId}</Text>
                  </Descriptions.Item>
                  <Descriptions.Item label="Total Points">
                    <Text strong style={{ color: '#1890ff' }}>
                      {totalPoints.toLocaleString()}
                    </Text>
                  </Descriptions.Item>
                  <Descriptions.Item label="Current Level">
                    <Space>
                      <StarOutlined style={{ color: '#52c41a' }} />
                      <Text strong>{currentLevel?.name || 'No Level'}</Text>
                      {currentLevel?.minPoints && (
                        <Text type="secondary">
                          (Min: {currentLevel.minPoints.toLocaleString()} pts)
                        </Text>
                      )}
                    </Space>
                  </Descriptions.Item>
                </Descriptions>
              </Col>
              <Col span={12}>
                <Title level={4}>Points by Category</Title>
                {userState.pointsByCategory &&
                Object.keys(userState.pointsByCategory).length > 0 ? (
                  <Space wrap>
                    {Object.entries(userState.pointsByCategory).map(
                      ([category, points]) => (
                        <Tag key={category} color="blue">
                          {category}: {points?.toLocaleString() || 0}
                        </Tag>
                      )
                    )}
                  </Space>
                ) : (
                  <Text type="secondary">No category points</Text>
                )}
              </Col>
            </Row>
          </TabPane>

          <TabPane tab="Badges" key="badges">
            <Title level={4}>
              <TrophyOutlined style={{ marginRight: 8 }} />
              Earned Badges ({userState.badges?.length || 0})
            </Title>
            {userState.badges && userState.badges.length > 0 ? (
              <Row gutter={16}>
                {userState.badges.map((badge, index) => (
                  <Col span={8} key={index} style={{ marginBottom: 16 }}>
                    <Card size="small">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Text strong>{badge.name || badge.id}</Text>
                        {badge.description && (
                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            {badge.description}
                          </Text>
                        )}
                        <Tag color="green">Badge</Tag>
                      </Space>
                    </Card>
                  </Col>
                ))}
              </Row>
            ) : (
              <Text type="secondary">No badges earned yet</Text>
            )}
          </TabPane>

          <TabPane tab="Trophies" key="trophies">
            <Title level={4}>
              <CrownOutlined style={{ marginRight: 8 }} />
              Earned Trophies ({userState.trophies?.length || 0})
            </Title>
            {userState.trophies && userState.trophies.length > 0 ? (
              <Row gutter={16}>
                {userState.trophies.map((trophy, index) => (
                  <Col span={8} key={index} style={{ marginBottom: 16 }}>
                    <Card size="small">
                      <Space direction="vertical" style={{ width: '100%' }}>
                        <Text strong>{trophy.name || trophy.id}</Text>
                        {trophy.description && (
                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            {trophy.description}
                          </Text>
                        )}
                        <Tag color="purple">Trophy</Tag>
                      </Space>
                    </Card>
                  </Col>
                ))}
              </Row>
            ) : (
              <Text type="secondary">No trophies earned yet</Text>
            )}
          </TabPane>

          <TabPane tab="Raw Data" key="raw">
            <Title level={4}>Raw User State Data</Title>
            <Card>
              <pre
                style={{
                  background: '#f5f5f5',
                  padding: '16px',
                  borderRadius: '6px',
                  overflow: 'auto',
                  maxHeight: '400px',
                }}
              >
                {JSON.stringify(userState, null, 2)}
              </pre>
            </Card>
          </TabPane>
        </Tabs>
      </Card>
    </div>
  )
}

export default UserDetails
