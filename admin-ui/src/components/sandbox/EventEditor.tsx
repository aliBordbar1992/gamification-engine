import React, { useState, useEffect } from 'react'
import { Card, Space, Button, Select, Alert, Typography, message } from 'antd'
import {
  PlayCircleOutlined,
  CopyOutlined,
  SaveOutlined,
} from '@ant-design/icons'
import MonacoEditor from '@/components/MonacoEditor'

const { Text } = Typography
const { Option } = Select

interface EventEditorProps {
  eventData: any
  language: 'json' | 'yaml'
  isValid: boolean
  validationErrors: string[]
  isLoading: boolean
  engineContext?: any
  onLanguageChange: (language: 'json' | 'yaml') => void
  onEditorChange: (value: string) => void
  onRunDryRun: () => void
  onSaveToHistory: () => void
  onCopyCurl: () => void
  getCurlCommand: (baseUrl: string) => string
}

const EventEditor: React.FC<EventEditorProps> = ({
  eventData,
  language,
  isValid,
  validationErrors,
  isLoading,
  engineContext,
  onLanguageChange,
  onEditorChange,
  onRunDryRun,
  onSaveToHistory,
  onCopyCurl,
  getCurlCommand,
}) => {
  const [showCurl, setShowCurl] = useState(false)
  const [editorValue, setEditorValue] = useState<string>('')

  // Memoize attributes JSON string for stable dependency
  const eventAttributesJson = React.useMemo(
    () => JSON.stringify(eventData.attributes),
    [eventData.attributes]
  )

  // Initialize editorValue from current eventData when language changes or data loads
  useEffect(() => {
    setEditorValue(
      language === 'json' ? JSON.stringify(eventData, null, 2) : getEventYaml()
    )
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    language,
    eventData.eventType,
    eventData.userId,
    eventData.eventId,
    eventData.occurredAt,
    eventAttributesJson,
  ])

  const handleEditorChange = (value: string) => {
    setEditorValue(value)
    onEditorChange(value)
  }

  const handleRunDryRun = () => {
    if (!isValid) {
      message.error('Please fix validation errors before running dry-run')
      return
    }
    onRunDryRun()
  }

  const handleCopyCurl = () => {
    const curlCommand = getCurlCommand('http://localhost:5000') // TODO: Get from config
    navigator.clipboard
      .writeText(curlCommand)
      .then(() => {
        message.success('cURL command copied to clipboard')
      })
      .catch(() => {
        message.error('Failed to copy cURL command')
      })
  }

  const getEventYaml = () => {
    // Simple YAML conversion - in a real app, you'd use a proper YAML library
    const yaml = `eventType: ${eventData.eventType}
userId: ${eventData.userId}
${eventData.eventId ? `eventId: ${eventData.eventId}` : ''}
${eventData.occurredAt ? `occurredAt: ${eventData.occurredAt}` : ''}
${
  eventData.attributes
    ? `attributes:
${Object.entries(eventData.attributes)
  .map(([key, value]) => `  ${key}: ${JSON.stringify(value)}`)
  .join('\n')}`
    : ''
}`
    return yaml
  }

  return (
    <Card
      title={
        <Space>
          <Text>Event Editor</Text>
          <Select
            value={language}
            onChange={onLanguageChange}
            size="small"
            style={{ width: 100 }}
          >
            <Option value="json">JSON</Option>
            <Option value="yaml">YAML</Option>
          </Select>
        </Space>
      }
      extra={
        <Space>
          <Button
            icon={<SaveOutlined />}
            onClick={onSaveToHistory}
            disabled={!isValid}
            title="Save current event to history"
          >
            Save
          </Button>
          <Button
            type="primary"
            icon={<PlayCircleOutlined />}
            onClick={handleRunDryRun}
            loading={isLoading}
            disabled={!isValid}
          >
            Run Dry Run
          </Button>
          <Button
            icon={<CopyOutlined />}
            onClick={() => setShowCurl(!showCurl)}
          >
            {showCurl ? 'Hide' : 'Show'} cURL
          </Button>
        </Space>
      }
    >
      {!isValid && validationErrors.length > 0 && (
        <Alert
          message="Validation Errors"
          description={
            <ul style={{ margin: 0, paddingLeft: '20px' }}>
              {validationErrors.map((error, index) => (
                <li key={index}>{error}</li>
              ))}
            </ul>
          }
          type="error"
          style={{ marginBottom: '16px' }}
          showIcon
        />
      )}

      <MonacoEditor
        value={editorValue}
        onChange={handleEditorChange}
        language={language}
        height={400}
        placeholder={`Enter your event data in ${language.toUpperCase()} format...`}
        userIdOptions={engineContext?.users?.map((u: any) => u.id) || []}
        onValidationChange={() => {
          // Handle validation if needed
        }}
      />

      {showCurl && (
        <div style={{ marginTop: '16px' }}>
          <Text strong>cURL Command:</Text>
          <div
            style={{
              background: '#f5f5f5',
              padding: '8px',
              borderRadius: '4px',
              marginTop: '8px',
              fontFamily: 'monospace',
              fontSize: '12px',
              whiteSpace: 'pre-wrap',
              wordBreak: 'break-all',
            }}
          >
            {getCurlCommand('http://localhost:5000')}
          </div>
          <Button
            size="small"
            icon={<CopyOutlined />}
            onClick={handleCopyCurl}
            style={{ marginTop: '8px' }}
          >
            Copy cURL
          </Button>
        </div>
      )}
    </Card>
  )
}

export default EventEditor
