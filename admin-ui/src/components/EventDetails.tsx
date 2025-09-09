import React from 'react'
import {
  Card,
  Typography,
  Descriptions,
  Tag,
  Space,
  Button,
  Spin,
  Alert,
} from 'antd'
import { ArrowLeftOutlined, InfoCircleOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useEvent } from '@/hooks/useEvents'

const { Title, Text } = Typography

interface EventDetailsProps {
  eventId: string
}

const EventDetails: React.FC<EventDetailsProps> = ({ eventId }) => {
  const navigate = useNavigate()
  const { data: event, isLoading, error } = useEvent(eventId)

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (error) {
    return (
      <Alert
        message="Error"
        description={`Failed to load event: ${error.message}`}
        type="error"
        showIcon
      />
    )
  }

  if (!event) {
    return (
      <Alert
        message="Not Found"
        description="Event not found"
        type="warning"
        showIcon
      />
    )
  }

  return (
    <div>
      <div style={{ marginBottom: 16 }}>
        <Button
          icon={<ArrowLeftOutlined />}
          onClick={() => navigate('/events')}
          style={{ marginRight: 16 }}
        >
          Back to Event Catalog
        </Button>
      </div>

      <Card>
        <Title level={3}>Event Details</Title>

        <Descriptions column={1} bordered>
          <Descriptions.Item label="Event ID">
            <Text code>{event.id}</Text>
          </Descriptions.Item>

          <Descriptions.Item label="Event Type">
            <Tag color="blue">{event.eventType}</Tag>
          </Descriptions.Item>

          <Descriptions.Item label="User ID">
            <Text code>{event.userId}</Text>
          </Descriptions.Item>

          <Descriptions.Item label="Timestamp">
            <Text>{new Date(event.timestamp).toLocaleString()}</Text>
          </Descriptions.Item>

          <Descriptions.Item label="Payload">
            <pre
              style={{
                background: '#f5f5f5',
                padding: '8px',
                borderRadius: '4px',
                fontSize: '12px',
                maxHeight: '200px',
                overflow: 'auto',
              }}
            >
              {JSON.stringify(event.payload, null, 2)}
            </pre>
          </Descriptions.Item>

          {event.metadata && Object.keys(event.metadata).length > 0 && (
            <Descriptions.Item label="Metadata">
              <div>
                {Object.entries(event.metadata).map(([key, value]) => (
                  <div key={key} style={{ marginBottom: 4 }}>
                    <Tag color="green">{key}</Tag>
                    <Text type="secondary">: {String(value)}</Text>
                  </div>
                ))}
              </div>
            </Descriptions.Item>
          )}
        </Descriptions>
      </Card>
    </div>
  )
}

export default EventDetails
