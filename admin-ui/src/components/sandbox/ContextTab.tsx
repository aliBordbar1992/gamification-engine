import React from 'react'
import { Space, Typography, Divider, Tag } from 'antd'

const { Text } = Typography

interface ContextTabProps {
  isEngineContextLoading: boolean
  engineContext: {
    eventTypes: string[]
    rules: Array<{
      name: string
      triggers: string[]
      isActive: boolean
    }>
    entities: {
      badges: any[]
      trophies: any[]
      levels: any[]
      pointCategories: any[]
    }
  }
}

const ContextTab: React.FC<ContextTabProps> = ({
  isEngineContextLoading,
  engineContext,
}) => {
  if (isEngineContextLoading) {
    return <Text type="secondary">Loading engine context...</Text>
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }}>
      <div>
        <Text strong>
          Available Event Types ({engineContext.eventTypes.length}):
        </Text>
        <div
          style={{
            marginTop: '8px',
            maxHeight: '100px',
            overflowY: 'auto',
          }}
        >
          {engineContext.eventTypes.length > 0 ? (
            engineContext.eventTypes.map((eventType, index) => (
              <Tag key={index} style={{ margin: '2px' }}>
                {eventType}
              </Tag>
            ))
          ) : (
            <Text type="secondary">No event types found</Text>
          )}
        </div>
      </div>

      <Divider style={{ margin: '8px 0' }} />

      <div>
        <Text strong>
          Active Rules ({engineContext.rules.filter((r) => r.isActive).length}):
        </Text>
        <div
          style={{
            marginTop: '8px',
            maxHeight: '100px',
            overflowY: 'auto',
          }}
        >
          {engineContext.rules.filter((r) => r.isActive).length > 0 ? (
            engineContext.rules
              .filter((r) => r.isActive)
              .map((rule, index) => (
                <div
                  key={index}
                  style={{
                    fontSize: '11px',
                    marginBottom: '4px',
                  }}
                >
                  <Text strong>{rule.name}</Text>
                  <br />
                  <Text type="secondary">
                    Triggers on: {rule.triggers.join(', ')}
                  </Text>
                </div>
              ))
          ) : (
            <Text type="secondary">No active rules found</Text>
          )}
        </div>
      </div>

      <Divider style={{ margin: '8px 0' }} />

      <div>
        <Text strong>Available Entities:</Text>
        <div style={{ marginTop: '8px', fontSize: '11px' }}>
          <div>Badges: {engineContext.entities.badges.length}</div>
          <div>Trophies: {engineContext.entities.trophies.length}</div>
          <div>Levels: {engineContext.entities.levels.length}</div>
          <div>
            Point Categories: {engineContext.entities.pointCategories.length}
          </div>
        </div>
      </div>
    </Space>
  )
}

export default ContextTab
