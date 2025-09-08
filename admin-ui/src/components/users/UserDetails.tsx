import React from 'react'
import {
  Card,
  Row,
  Col,
  Typography,
  Tag,
  Space,
  Statistic,
  List,
  Avatar,
  Badge,
  Divider,
  Empty,
  Spin,
} from 'antd'
import {
  TrophyOutlined,
  StarOutlined,
  CrownOutlined,
  FireOutlined,
  UserOutlined,
  GiftOutlined,
} from '@ant-design/icons'
import type { UserStateDto } from '@/api/generated/models'

const { Title, Text } = Typography

interface UserDetailsProps {
  userState: UserStateDto
  isLoading?: boolean
}

const UserDetails: React.FC<UserDetailsProps> = ({ userState, isLoading }) => {
  if (isLoading) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px' }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>
            <Text>Loading user data...</Text>
          </div>
        </div>
      </Card>
    )
  }

  if (!userState) {
    return (
      <Card>
        <Empty
          description="No user data found"
          image={Empty.PRESENTED_IMAGE_SIMPLE}
        />
      </Card>
    )
  }

  const {
    userId,
    pointsByCategory,
    badges,
    trophies,
    currentLevelsByCategory,
  } = userState

  const totalPoints = pointsByCategory
    ? Object.values(pointsByCategory).reduce((sum, points) => sum + points, 0)
    : 0

  const pointsEntries = pointsByCategory ? Object.entries(pointsByCategory) : []
  const levelsEntries = currentLevelsByCategory
    ? Object.entries(currentLevelsByCategory)
    : []

  return (
    <div>
      {/* User Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row align="middle" gutter={16}>
          <Col>
            <Avatar size={64} icon={<UserOutlined />} />
          </Col>
          <Col flex="auto">
            <Title level={3} style={{ margin: 0 }}>
              {userId}
            </Title>
            <Text type="secondary">Gamification Profile</Text>
          </Col>
          <Col>
            <Statistic
              title="Total Points"
              value={totalPoints}
              prefix={<FireOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Col>
        </Row>
      </Card>

      <Row gutter={16}>
        {/* Points by Category */}
        <Col span={12}>
          <Card
            title={
              <Space>
                <FireOutlined />
                Points by Category
              </Space>
            }
            style={{ marginBottom: 16 }}
          >
            {pointsEntries.length > 0 ? (
              <List
                dataSource={pointsEntries}
                renderItem={([category, points]) => (
                  <List.Item>
                    <List.Item.Meta
                      title={category}
                      description={`${points.toLocaleString()} points`}
                    />
                    <div>
                      <Statistic
                        value={points}
                        valueStyle={{ fontSize: '18px', color: '#52c41a' }}
                      />
                    </div>
                  </List.Item>
                )}
              />
            ) : (
              <Empty
                description="No points data available"
                image={Empty.PRESENTED_IMAGE_SIMPLE}
              />
            )}
          </Card>
        </Col>

        {/* Current Levels */}
        <Col span={12}>
          <Card
            title={
              <Space>
                <CrownOutlined />
                Current Levels
              </Space>
            }
            style={{ marginBottom: 16 }}
          >
            {levelsEntries.length > 0 ? (
              <List
                dataSource={levelsEntries}
                renderItem={([category, level]) => (
                  <List.Item>
                    <List.Item.Meta
                      title={category}
                      description={level.description || 'Level progression'}
                    />
                    <div>
                      <Tag color="blue" style={{ fontSize: '14px' }}>
                        {level.name || `${category} Level`}
                      </Tag>
                    </div>
                  </List.Item>
                )}
              />
            ) : (
              <Empty
                description="No level data available"
                image={Empty.PRESENTED_IMAGE_SIMPLE}
              />
            )}
          </Card>
        </Col>
      </Row>

      <Row gutter={16}>
        {/* Badges */}
        <Col span={12}>
          <Card
            title={
              <Space>
                <StarOutlined />
                Badges ({badges?.length || 0})
              </Space>
            }
            style={{ marginBottom: 16 }}
          >
            {badges && badges.length > 0 ? (
              <List
                dataSource={badges}
                renderItem={(badge) => (
                  <List.Item>
                    <List.Item.Meta
                      avatar={
                        <Avatar
                          icon={<StarOutlined />}
                          style={{ backgroundColor: '#faad14' }}
                        />
                      }
                      title={badge.name}
                      description={badge.description}
                    />
                    <Tag color="gold">Badge</Tag>
                  </List.Item>
                )}
              />
            ) : (
              <Empty
                description="No badges earned yet"
                image={Empty.PRESENTED_IMAGE_SIMPLE}
              />
            )}
          </Card>
        </Col>

        {/* Trophies */}
        <Col span={12}>
          <Card
            title={
              <Space>
                <TrophyOutlined />
                Trophies ({trophies?.length || 0})
              </Space>
            }
            style={{ marginBottom: 16 }}
          >
            {trophies && trophies.length > 0 ? (
              <List
                dataSource={trophies}
                renderItem={(trophy) => (
                  <List.Item>
                    <List.Item.Meta
                      avatar={
                        <Avatar
                          icon={<TrophyOutlined />}
                          style={{ backgroundColor: '#722ed1' }}
                        />
                      }
                      title={trophy.name}
                      description={trophy.description}
                    />
                    <Tag color="purple">Trophy</Tag>
                  </List.Item>
                )}
              />
            ) : (
              <Empty
                description="No trophies earned yet"
                image={Empty.PRESENTED_IMAGE_SIMPLE}
              />
            )}
          </Card>
        </Col>
      </Row>

      {/* Summary Stats */}
      <Card>
        <Title level={4}>
          <GiftOutlined style={{ marginRight: 8 }} />
          Summary
        </Title>
        <Row gutter={16}>
          <Col span={6}>
            <Statistic
              title="Total Points"
              value={totalPoints}
              prefix={<FireOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Badges Earned"
              value={badges?.length || 0}
              prefix={<StarOutlined />}
              valueStyle={{ color: '#faad14' }}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Trophies Won"
              value={trophies?.length || 0}
              prefix={<TrophyOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Categories"
              value={pointsEntries.length}
              prefix={<CrownOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Col>
        </Row>
      </Card>
    </div>
  )
}

export default UserDetails
