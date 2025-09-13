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
import { useEventDefinition } from '@/hooks/useEvents'

const { Title, Text } = Typography

interface EventDetailsProps {
  eventId: string
}

const EventDetails: React.FC<EventDetailsProps> = ({ eventId }) => {
  const navigate = useNavigate()
  const {
    data: eventDefinition,
    isLoading,
    error,
  } = useEventDefinition(eventId)

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

  if (!eventDefinition) {
    return (
      <Alert
        message="Not Found"
        description="Event definition not found"
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
        <Title level={3}>Event Definition Details</Title>

        <Descriptions column={1} bordered>
          <Descriptions.Item label="Event Definition ID">
            <Text code>{eventDefinition.id}</Text>
          </Descriptions.Item>

          <Descriptions.Item label="Description">
            <Text>
              {eventDefinition.description || 'No description available'}
            </Text>
          </Descriptions.Item>

          <Descriptions.Item label="Payload Schema">
            {eventDefinition.payloadSchema &&
            Object.keys(eventDefinition.payloadSchema).length > 0 ? (
              <div>
                {Object.entries(eventDefinition.payloadSchema).map(
                  ([key, value]) => (
                    <div key={key} style={{ marginBottom: 4 }}>
                      <Tag color="blue">{key}</Tag>
                      <Text type="secondary">: {String(value)}</Text>
                    </div>
                  )
                )}
              </div>
            ) : (
              <Text type="secondary">No payload schema defined</Text>
            )}
          </Descriptions.Item>
        </Descriptions>
      </Card>
    </div>
  )
}

export default EventDetails
