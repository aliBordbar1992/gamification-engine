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
  Tooltip,
  Divider,
} from 'antd'
import {
  ArrowLeftOutlined,
  CopyOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined,
  EditOutlined,
} from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useRule } from '@/hooks/useGeneratedRules'
import type { Rule } from '@/types'

const { Title, Text, Paragraph } = Typography
const { TabPane } = Tabs

interface RuleDetailsProps {
  ruleId: string
  onEdit?: (rule: Rule) => void
}

const RuleDetails: React.FC<RuleDetailsProps> = ({ ruleId, onEdit }) => {
  const navigate = useNavigate()
  const { data: rule, isLoading, error } = useRule(ruleId)
  const [activeTab, setActiveTab] = useState('overview')

  const handleBack = () => {
    navigate('/rules')
  }

  const handleCopyJson = () => {
    if (rule) {
      navigator.clipboard.writeText(JSON.stringify(rule, null, 2))
    }
  }

  const handleCopyYaml = () => {
    if (rule) {
      // Simple YAML conversion (in a real app, you'd use a proper YAML library)
      const yaml = convertToYaml(rule)
      navigator.clipboard.writeText(yaml)
    }
  }

  const convertToYaml = (obj: any, indent = 0): string => {
    const spaces = '  '.repeat(indent)
    let yaml = ''

    for (const [key, value] of Object.entries(obj)) {
      if (Array.isArray(value)) {
        yaml += `${spaces}${key}:\n`
        value.forEach((item, index) => {
          if (typeof item === 'object' && item !== null) {
            yaml += `${spaces}  - ${key === 'triggers' ? item : ''}\n`
            if (key !== 'triggers') {
              yaml += convertToYaml(item, indent + 2)
            }
          } else {
            yaml += `${spaces}  - ${item}\n`
          }
        })
      } else if (typeof value === 'object' && value !== null) {
        yaml += `${spaces}${key}:\n`
        yaml += convertToYaml(value, indent + 1)
      } else {
        yaml += `${spaces}${key}: ${value}\n`
      }
    }

    return yaml
  }

  if (isLoading) {
    return <Card loading />
  }

  if (error) {
    return (
      <Card>
        <Text type="danger">Error loading rule: {error.message}</Text>
      </Card>
    )
  }

  if (!rule) {
    return (
      <Card>
        <Text type="secondary">Rule not found</Text>
      </Card>
    )
  }

  return (
    <div>
      {/* Header */}
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle">
          <Col>
            <Space>
              <Button
                icon={<ArrowLeftOutlined />}
                onClick={handleBack}
                type="text"
              >
                Back to Rules
              </Button>
              <Divider type="vertical" />
              <Title level={3} style={{ margin: 0 }}>
                {rule.name}
              </Title>
              <Badge
                status={rule.isActive ? 'success' : 'default'}
                text={rule.isActive ? 'Active' : 'Inactive'}
              />
            </Space>
          </Col>
          <Col>
            <Space>
              <Button
                icon={<CopyOutlined />}
                onClick={handleCopyJson}
                type="text"
              >
                Copy JSON
              </Button>
              <Button
                icon={<CopyOutlined />}
                onClick={handleCopyYaml}
                type="text"
              >
                Copy YAML
              </Button>
              {onEdit && (
                <Button
                  icon={<EditOutlined />}
                  onClick={() => onEdit(rule)}
                  type="primary"
                >
                  Edit Rule
                </Button>
              )}
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Content Tabs */}
      <Card>
        <Tabs activeKey={activeTab} onChange={setActiveTab}>
          <TabPane tab="Overview" key="overview">
            <Row gutter={24}>
              <Col span={12}>
                <Card title="Basic Information" size="small">
                  <Descriptions column={1} size="small">
                    <Descriptions.Item label="ID">
                      <Text code>{rule.id}</Text>
                    </Descriptions.Item>
                    <Descriptions.Item label="Name">
                      {rule.name}
                    </Descriptions.Item>
                    <Descriptions.Item label="Description">
                      <Paragraph>{rule.description}</Paragraph>
                    </Descriptions.Item>
                    <Descriptions.Item label="Status">
                      <Badge
                        status={rule.isActive ? 'success' : 'default'}
                        text={rule.isActive ? 'Active' : 'Inactive'}
                      />
                    </Descriptions.Item>
                    <Descriptions.Item label="Created">
                      {new Date(rule.createdAt).toLocaleString()}
                    </Descriptions.Item>
                    {rule.updatedAt && (
                      <Descriptions.Item label="Updated">
                        {new Date(rule.updatedAt).toLocaleString()}
                      </Descriptions.Item>
                    )}
                  </Descriptions>
                </Card>
              </Col>
              <Col span={12}>
                <Card title="Triggers" size="small">
                  <Space wrap>
                    {rule.triggers.map((trigger) => (
                      <Tag key={trigger} color="blue">
                        {trigger}
                      </Tag>
                    ))}
                  </Space>
                </Card>
              </Col>
            </Row>

            <Row gutter={24} style={{ marginTop: 16 }}>
              <Col span={12}>
                <Card title="Conditions" size="small">
                  {rule.conditions.length > 0 ? (
                    <Space direction="vertical" style={{ width: '100%' }}>
                      {rule.conditions.map((condition, index) => (
                        <Card
                          key={index}
                          size="small"
                          style={{ backgroundColor: '#fafafa' }}
                        >
                          <Text strong>{condition.type}</Text>
                          {Object.keys(condition.parameters).length > 0 && (
                            <div style={{ marginTop: 8 }}>
                              <Text type="secondary">Parameters:</Text>
                              <pre
                                style={{
                                  marginTop: 4,
                                  fontSize: '12px',
                                  backgroundColor: '#fff',
                                  padding: 8,
                                  borderRadius: 4,
                                  border: '1px solid #d9d9d9',
                                }}
                              >
                                {JSON.stringify(condition.parameters, null, 2)}
                              </pre>
                            </div>
                          )}
                        </Card>
                      ))}
                    </Space>
                  ) : (
                    <Text type="secondary">No conditions defined</Text>
                  )}
                </Card>
              </Col>
              <Col span={12}>
                <Card title="Rewards" size="small">
                  {rule.rewards.length > 0 ? (
                    <Space direction="vertical" style={{ width: '100%' }}>
                      {rule.rewards.map((reward, index) => (
                        <Card
                          key={index}
                          size="small"
                          style={{ backgroundColor: '#f6ffed' }}
                        >
                          <Space>
                            <Tag color="green">{reward.type}</Tag>
                            <Text strong>{reward.targetId}</Text>
                            {reward.amount && (
                              <Text type="secondary">({reward.amount})</Text>
                            )}
                          </Space>
                          {Object.keys(reward.parameters).length > 0 && (
                            <div style={{ marginTop: 8 }}>
                              <Text type="secondary">Parameters:</Text>
                              <pre
                                style={{
                                  marginTop: 4,
                                  fontSize: '12px',
                                  backgroundColor: '#fff',
                                  padding: 8,
                                  borderRadius: 4,
                                  border: '1px solid #d9d9d9',
                                }}
                              >
                                {JSON.stringify(reward.parameters, null, 2)}
                              </pre>
                            </div>
                          )}
                        </Card>
                      ))}
                    </Space>
                  ) : (
                    <Text type="secondary">No rewards defined</Text>
                  )}
                </Card>
              </Col>
            </Row>
          </TabPane>

          <TabPane tab="JSON" key="json">
            <Card>
              <pre
                style={{
                  backgroundColor: '#f5f5f5',
                  padding: 16,
                  borderRadius: 6,
                  overflow: 'auto',
                  maxHeight: '600px',
                }}
              >
                {JSON.stringify(rule, null, 2)}
              </pre>
            </Card>
          </TabPane>

          <TabPane tab="YAML" key="yaml">
            <Card>
              <pre
                style={{
                  backgroundColor: '#f5f5f5',
                  padding: 16,
                  borderRadius: 6,
                  overflow: 'auto',
                  maxHeight: '600px',
                }}
              >
                {convertToYaml(rule)}
              </pre>
            </Card>
          </TabPane>
        </Tabs>
      </Card>
    </div>
  )
}

export default RuleDetails
