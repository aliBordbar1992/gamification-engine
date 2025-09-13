import React, { useState, useMemo } from 'react'
import {
  Table,
  Input,
  Card,
  Row,
  Col,
  Typography,
  Tag,
  Space,
  Tooltip,
  Collapse,
  Descriptions,
  Badge,
} from 'antd'
import { SearchOutlined, InfoCircleOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useEventDefinitions } from '@/hooks/useEvents'
import type { EventDefinitionDto } from '@/api/generated/models'

const { Title, Text } = Typography
const { Search } = Input
const { Panel } = Collapse

interface EventCatalogProps {
  onViewEvent?: (event: EventDefinitionDto) => void
}

const EventCatalog: React.FC<EventCatalogProps> = ({ onViewEvent }) => {
  const navigate = useNavigate()
  const [searchText, setSearchText] = useState('')
  const {
    data: eventDefinitions = [],
    isLoading,
    error,
  } = useEventDefinitions()

  const filteredEvents = useMemo(() => {
    if (!searchText) return eventDefinitions

    const searchLower = searchText.toLowerCase()
    return eventDefinitions.filter(
      (event) =>
        event.id?.toLowerCase().includes(searchLower) ||
        event.description?.toLowerCase().includes(searchLower)
    )
  }, [eventDefinitions, searchText])

  const stats = useMemo(() => {
    const total = eventDefinitions.length
    const withSchema = eventDefinitions.filter(
      (event) =>
        event.payloadSchema && Object.keys(event.payloadSchema).length > 0
    ).length
    const withoutSchema = total - withSchema
    return { total, withSchema, withoutSchema }
  }, [eventDefinitions])

  const columns = [
    {
      title: 'Event ID',
      dataIndex: 'id',
      key: 'id',
      width: 200,
      render: (id: string, record: EventDefinitionDto) => (
        <div>
          <Text
            strong
            code
            style={{ cursor: 'pointer', color: '#1890ff' }}
            onClick={() => navigate(`/events/${id}`)}
          >
            {id}
          </Text>
          {onViewEvent && (
            <div style={{ marginTop: 4 }}>
              <Text
                style={{
                  fontSize: '12px',
                  color: '#1890ff',
                  cursor: 'pointer',
                }}
                onClick={() => onViewEvent(record)}
              >
                View Details
              </Text>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (description: string) => (
        <Text type="secondary">{description || 'No description'}</Text>
      ),
    },
    {
      title: 'Schema Status',
      dataIndex: 'payloadSchema',
      key: 'payloadSchema',
      width: 120,
      render: (schema: { [key: string]: string } | null) => (
        <Badge
          status={
            schema && Object.keys(schema).length > 0 ? 'success' : 'default'
          }
          text={schema && Object.keys(schema).length > 0 ? 'Defined' : 'None'}
        />
      ),
    },
    {
      title: 'Schema Fields',
      dataIndex: 'payloadSchema',
      key: 'schemaFields',
      render: (schema: { [key: string]: string } | null) => {
        if (!schema || Object.keys(schema).length === 0) {
          return <Text type="secondary">No schema</Text>
        }

        const fields = Object.keys(schema)
        return (
          <Space wrap>
            {fields.slice(0, 3).map((field) => (
              <Tag key={field} color="blue">
                {field}: {schema[field]}
              </Tag>
            ))}
            {fields.length > 3 && (
              <Tooltip
                title={fields
                  .slice(3)
                  .map((f) => `${f}: ${schema[f]}`)
                  .join(', ')}
              >
                <Tag color="default">+{fields.length - 3}</Tag>
              </Tooltip>
            )}
          </Space>
        )
      },
    },
  ]

  if (error) {
    return (
      <Card>
        <Text type="danger">Error loading event catalog: {error.message}</Text>
      </Card>
    )
  }

  return (
    <div>
      {/* Stats Cards */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={8}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#1890ff' }}>
                {stats.total}
              </Title>
              <Text type="secondary">Total Events</Text>
            </div>
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#52c41a' }}>
                {stats.withSchema}
              </Title>
              <Text type="secondary">With Schema</Text>
            </div>
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#d9d9d9' }}>
                {stats.withoutSchema}
              </Title>
              <Text type="secondary">Without Schema</Text>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Search */}
      <Card style={{ marginBottom: 16 }}>
        <Search
          placeholder="Search events by ID or description..."
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          onSearch={setSearchText}
          style={{ maxWidth: 400 }}
          prefix={<SearchOutlined />}
        />
      </Card>

      {/* Events Table */}
      <Card>
        <Table
          columns={columns}
          dataSource={filteredEvents}
          rowKey="id"
          loading={isLoading}
          pagination={{
            pageSize: 20,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} of ${total} events`,
          }}
          scroll={{ x: 800 }}
          expandable={{
            expandedRowRender: (record: EventDefinitionDto) => (
              <div style={{ margin: 0 }}>
                <Collapse size="small" ghost>
                  <Panel
                    header={
                      <Space>
                        <InfoCircleOutlined />
                        <Text strong>Event Details</Text>
                      </Space>
                    }
                    key="details"
                  >
                    <Descriptions size="small" column={1}>
                      <Descriptions.Item label="Event ID">
                        <Text code>{record.id}</Text>
                      </Descriptions.Item>
                      <Descriptions.Item label="Description">
                        {record.description || 'No description provided'}
                      </Descriptions.Item>
                      <Descriptions.Item label="Payload Schema">
                        {record.payloadSchema &&
                        Object.keys(record.payloadSchema).length > 0 ? (
                          <div>
                            {Object.entries(record.payloadSchema).map(
                              ([key, value]) => (
                                <div key={key} style={{ marginBottom: 4 }}>
                                  <Tag color="blue">{key}</Tag>
                                  <Text type="secondary">: {value}</Text>
                                </div>
                              )
                            )}
                          </div>
                        ) : (
                          <Text type="secondary">No schema defined</Text>
                        )}
                      </Descriptions.Item>
                    </Descriptions>
                  </Panel>
                </Collapse>
              </div>
            ),
            rowExpandable: () => true,
          }}
        />
      </Card>
    </div>
  )
}

export default EventCatalog
