import React from 'react'
import { Space, Button, Select, Divider, Typography, message } from 'antd'
import { CodeOutlined } from '@ant-design/icons'

const { Text } = Typography

interface ExamplesTabProps {
  eventData: any
  isEngineContextLoading: boolean
  engineContext?: any
  onLoadExample: (
    exampleType: 'login' | 'purchase' | 'achievement' | 'smart'
  ) => void
  onGenerateSmartAttributes: () => void
  onLoadTemplateFromEventId: (eventId: string) => void
}

const ExamplesTab: React.FC<ExamplesTabProps> = ({
  eventData,
  isEngineContextLoading,
  engineContext,
  onLoadExample,
  onGenerateSmartAttributes,
  onLoadTemplateFromEventId,
}) => {
  const handleGenerateSmartAttributes = () => {
    onGenerateSmartAttributes()
    message.success('Generated smart attributes based on event type')
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }}>
      <Text strong>Load Example Events:</Text>

      {!isEngineContextLoading && (
        <Button
          block
          onClick={() => onLoadExample('smart')}
          icon={<CodeOutlined />}
          type="primary"
        >
          Smart Example (from Engine)
        </Button>
      )}

      <Divider style={{ margin: '8px 0' }} />
      <Text strong>Load From Event Catalog:</Text>
      <Select
        showSearch
        placeholder="Select an event"
        style={{ width: '100%' }}
        onChange={(value) => {
          onLoadTemplateFromEventId(value)
        }}
        options={(engineContext?.eventTypes || []).map((id: string) => ({
          label: id,
          value: id,
        }))}
      />

      <Button
        block
        onClick={() => onLoadExample('login')}
        icon={<CodeOutlined />}
      >
        User Login Event
      </Button>
      <Button
        block
        onClick={() => onLoadExample('purchase')}
        icon={<CodeOutlined />}
      >
        Purchase Event
      </Button>
      <Button
        block
        onClick={() => onLoadExample('achievement')}
        icon={<CodeOutlined />}
      >
        Achievement Event
      </Button>

      {eventData.eventType && (
        <>
          <Divider style={{ margin: '8px 0' }} />
          <Button
            block
            onClick={handleGenerateSmartAttributes}
            icon={<CodeOutlined />}
            size="small"
          >
            Generate Smart Attributes
          </Button>
        </>
      )}
    </Space>
  )
}

export default ExamplesTab
