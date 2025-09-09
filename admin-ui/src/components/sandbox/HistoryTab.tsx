import React from 'react'
import { Space, Typography, Button, Card, message } from 'antd'
import { ClearOutlined, DeleteOutlined } from '@ant-design/icons'

const { Text } = Typography

interface HistoryItem {
  id: string
  name: string
  eventData: any
  language: 'json' | 'yaml'
  lastUsed: string
}

interface HistoryTabProps {
  history: HistoryItem[]
  currentHistoryId?: string
  onLoadFromHistory: (historyId: string) => void
  onDeleteFromHistory: (historyId: string, event: React.MouseEvent) => void
  onClearHistory: () => void
}

const HistoryTab: React.FC<HistoryTabProps> = ({
  history,
  currentHistoryId,
  onLoadFromHistory,
  onDeleteFromHistory,
  onClearHistory,
}) => {
  const handleDeleteFromHistory = (
    historyId: string,
    event: React.MouseEvent
  ) => {
    event.stopPropagation()
    onDeleteFromHistory(historyId, event)
    message.success('Deleted from history')
  }

  const handleClearHistory = () => {
    onClearHistory()
    message.success('History cleared')
  }

  return (
    <Space direction="vertical" style={{ width: '100%' }}>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
        }}
      >
        <Text strong>Saved Events ({history.length})</Text>
        {history.length > 0 && (
          <Button
            size="small"
            icon={<ClearOutlined />}
            onClick={handleClearHistory}
            danger
          >
            Clear All
          </Button>
        )}
      </div>

      {history.length === 0 ? (
        <Text type="secondary">
          No saved events yet. Save your current event to see it here.
        </Text>
      ) : (
        <div style={{ maxHeight: '300px', overflowY: 'auto' }}>
          {history.map((item) => (
            <Card
              key={item.id}
              size="small"
              style={{
                marginBottom: '8px',
                cursor: 'pointer',
                border:
                  currentHistoryId === item.id
                    ? '2px solid #1890ff'
                    : '1px solid #d9d9d9',
              }}
              onClick={() => onLoadFromHistory(item.id)}
              title={
                <div
                  style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                  }}
                >
                  <span
                    style={{
                      fontSize: '12px',
                      fontWeight: 'bold',
                    }}
                  >
                    {item.name}
                  </span>
                  <Button
                    size="small"
                    icon={<DeleteOutlined />}
                    onClick={(e) => handleDeleteFromHistory(item.id, e)}
                    danger
                    style={{ marginLeft: '8px' }}
                  />
                </div>
              }
            >
              <div style={{ fontSize: '11px', color: '#666' }}>
                <div>
                  <strong>Type:</strong> {item.eventData.eventType}
                </div>
                <div>
                  <strong>User:</strong> {item.eventData.userId}
                </div>
                <div>
                  <strong>Language:</strong> {item.language.toUpperCase()}
                </div>
                <div>
                  <strong>Last used:</strong>{' '}
                  {new Date(item.lastUsed).toLocaleString()}
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}
    </Space>
  )
}

export default HistoryTab
