import React, { useState } from 'react'
import { Alert, Typography, Card, Tabs } from 'antd'
import { EyeOutlined, CodeOutlined } from '@ant-design/icons'
import VisualTrace from './VisualTrace'

const { Title, Text } = Typography

interface ResultsDisplayProps {
  error?: string
  evaluationResult?: any
  isLoading: boolean
}

const ResultsDisplay: React.FC<ResultsDisplayProps> = ({
  error,
  evaluationResult,
  isLoading,
}) => {
  const [activeTab, setActiveTab] = useState('visual')

  const renderJsonView = () => (
    <div>
      <Card size="small" style={{ marginBottom: '12px' }}>
        <div>
          <Text strong>Event Details:</Text>
          <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
            <li>Event ID: {evaluationResult.triggerEventId || 'N/A'}</li>
            <li>User ID: {evaluationResult.userId}</li>
            <li>Event Type: {evaluationResult.eventType}</li>
            <li>
              Evaluated At:{' '}
              {evaluationResult.evaluatedAt
                ? new Date(evaluationResult.evaluatedAt).toLocaleString()
                : 'N/A'}
            </li>
          </ul>
        </div>
      </Card>

      {evaluationResult.rules && evaluationResult.rules.length > 0 && (
        <div>
          <Text strong>Matched Rules ({evaluationResult.rules.length}):</Text>
          {evaluationResult.rules.map((rule: unknown, index: number) => (
            <Card
              key={index}
              size="small"
              style={{ marginBottom: '8px' }}
              title={`Rule ${index + 1}`}
            >
              <pre
                style={{
                  fontSize: '12px',
                  background: '#f5f5f5',
                  padding: '8px',
                  borderRadius: '4px',
                  margin: 0,
                  whiteSpace: 'pre-wrap',
                  wordBreak: 'break-word',
                }}
              >
                {JSON.stringify(rule, null, 2)}
              </pre>
            </Card>
          ))}
        </div>
      )}

      {evaluationResult.summary && (
        <div style={{ marginTop: '16px' }}>
          <Text strong>Summary:</Text>
          <pre
            style={{
              fontSize: '12px',
              background: '#f5f5f5',
              padding: '8px',
              borderRadius: '4px',
              margin: '8px 0 0 0',
              whiteSpace: 'pre-wrap',
              wordBreak: 'break-word',
            }}
          >
            {JSON.stringify(evaluationResult.summary, null, 2)}
          </pre>
        </div>
      )}

      {(!evaluationResult.rules || evaluationResult.rules.length === 0) &&
        !evaluationResult.summary && (
          <Text type="secondary">No rules were matched for this event.</Text>
        )}
    </div>
  )

  return (
    <>
      {error && (
        <Alert
          message="Dry Run Failed"
          description={error}
          type="error"
          style={{ marginBottom: '16px' }}
          showIcon
        />
      )}

      {evaluationResult ? (
        <div>
          <Title level={4}>Evaluation Results</Title>

          <Tabs
            activeKey={activeTab}
            onChange={setActiveTab}
            items={[
              {
                key: 'visual',
                label: (
                  <span>
                    <EyeOutlined />
                    Visual Trace
                  </span>
                ),
                children: <VisualTrace evaluationResult={evaluationResult} />,
              },
              {
                key: 'json',
                label: (
                  <span>
                    <CodeOutlined />
                    JSON View
                  </span>
                ),
                children: renderJsonView(),
              },
            ]}
          />
        </div>
      ) : !isLoading ? (
        <Text type="secondary">
          Run a dry-run to see evaluation results here.
        </Text>
      ) : null}
    </>
  )
}

export default ResultsDisplay
