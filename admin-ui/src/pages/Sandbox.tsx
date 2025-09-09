import React, { useState } from 'react'
import {
  Typography,
  Card,
  Row,
  Col,
  Button,
  Space,
  Select,
  Alert,
  Divider,
  Tabs,
  message,
  Tooltip,
} from 'antd'
import {
  PlayCircleOutlined,
  CopyOutlined,
  FileTextOutlined,
  CodeOutlined,
  DownloadOutlined,
  UploadOutlined,
  SaveOutlined,
  HistoryOutlined,
  DeleteOutlined,
  ClearOutlined,
} from '@ant-design/icons'
import MonacoEditor from '@/components/MonacoEditor'
import { useSandbox } from '@/hooks/useSandbox'
import type { IngestEventRequest } from '@/api/generated/models'

const { Title, Text } = Typography
const { Option } = Select
const { TabPane } = Tabs

const Sandbox: React.FC = () => {
  const {
    eventData,
    language,
    setLanguage,
    isValid,
    validationErrors,
    evaluationResult,
    isLoading,
    error,
    history,
    currentHistoryId,
    updateEventFromEditor,
    runDryRun,
    getCurlCommand,
    loadExample,
    saveToHistory,
    loadFromHistory,
    deleteFromHistory,
    clearHistory,
  } = useSandbox()

  const [showCurl, setShowCurl] = useState(false)
  const [activeTab, setActiveTab] = useState('editor')

  const handleEditorChange = (value: string) => {
    updateEventFromEditor(value)
  }

  const handleRunDryRun = () => {
    if (!isValid) {
      message.error('Please fix validation errors before running dry-run')
      return
    }
    runDryRun()
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

  const getEventJson = () => {
    return JSON.stringify(eventData, null, 2)
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

  const getEditorValue = () => {
    return language === 'json' ? getEventJson() : getEventYaml()
  }

  const handleLanguageChange = (newLanguage: 'json' | 'yaml') => {
    setLanguage(newLanguage)
  }

  const handleLoadExample = (
    exampleType: 'login' | 'purchase' | 'achievement'
  ) => {
    loadExample(exampleType)
    setActiveTab('editor')
  }

  const handleSaveToHistory = () => {
    if (!isValid) {
      message.error('Please fix validation errors before saving')
      return
    }

    const savedItem = saveToHistory()
    if (savedItem) {
      message.success(`Saved as "${savedItem.name}"`)
    }
  }

  const handleLoadFromHistory = (historyId: string) => {
    loadFromHistory(historyId)
    setActiveTab('editor')
    message.success('Loaded from history')
  }

  const handleDeleteFromHistory = (
    historyId: string,
    event: React.MouseEvent
  ) => {
    event.stopPropagation()
    deleteFromHistory(historyId)
    message.success('Deleted from history')
  }

  const handleClearHistory = () => {
    clearHistory()
    message.success('History cleared')
  }

  return (
    <div style={{ padding: '24px' }}>
      <div style={{ marginBottom: '24px' }}>
        <Title level={2}>Sandbox - Event Dry Run</Title>
        <Text type="secondary">
          Test event processing without side effects. See what rules would
          trigger and what rewards would be issued.
        </Text>
      </div>

      <Row gutter={[24, 24]}>
        {/* Event Editor */}
        <Col xs={24} lg={16}>
          <Card
            title={
              <Space>
                <FileTextOutlined />
                Event Editor
                <Select
                  value={language}
                  onChange={handleLanguageChange}
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
                  onClick={handleSaveToHistory}
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
              value={getEditorValue()}
              onChange={handleEditorChange}
              language={language}
              height={400}
              placeholder={`Enter your event data in ${language.toUpperCase()} format...`}
              onValidationChange={(valid, errors) => {
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
        </Col>

        {/* Examples, History and Results */}
        <Col xs={24} lg={8}>
          <Tabs activeKey={activeTab} onChange={setActiveTab}>
            <TabPane tab="Examples" key="examples">
              <Space direction="vertical" style={{ width: '100%' }}>
                <Text strong>Load Example Events:</Text>
                <Button
                  block
                  onClick={() => handleLoadExample('login')}
                  icon={<CodeOutlined />}
                >
                  User Login Event
                </Button>
                <Button
                  block
                  onClick={() => handleLoadExample('purchase')}
                  icon={<CodeOutlined />}
                >
                  Purchase Event
                </Button>
                <Button
                  block
                  onClick={() => handleLoadExample('achievement')}
                  icon={<CodeOutlined />}
                >
                  Achievement Event
                </Button>
              </Space>
            </TabPane>

            <TabPane tab="History" key="history">
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
                        onClick={() => handleLoadFromHistory(item.id)}
                        title={
                          <div
                            style={{
                              display: 'flex',
                              justifyContent: 'space-between',
                              alignItems: 'center',
                            }}
                          >
                            <span
                              style={{ fontSize: '12px', fontWeight: 'bold' }}
                            >
                              {item.name}
                            </span>
                            <Button
                              size="small"
                              icon={<DeleteOutlined />}
                              onClick={(e) =>
                                handleDeleteFromHistory(item.id, e)
                              }
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
                            <strong>Language:</strong>{' '}
                            {item.language.toUpperCase()}
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
            </TabPane>

            <TabPane tab="Results" key="results">
              {error && (
                <Alert
                  message="Dry Run Failed"
                  description={error}
                  type="error"
                  style={{ marginBottom: '16px' }}
                  showIcon
                />
              )}

              {evaluationResult && (
                <div>
                  <Title level={4}>Evaluation Results</Title>

                  <Card size="small" style={{ marginBottom: '12px' }}>
                    <div>
                      <Text strong>Event Details:</Text>
                      <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
                        <li>
                          Event ID: {evaluationResult.triggerEventId || 'N/A'}
                        </li>
                        <li>User ID: {evaluationResult.userId}</li>
                        <li>Event Type: {evaluationResult.eventType}</li>
                        <li>
                          Evaluated At:{' '}
                          {evaluationResult.evaluatedAt
                            ? new Date(
                                evaluationResult.evaluatedAt
                              ).toLocaleString()
                            : 'N/A'}
                        </li>
                      </ul>
                    </div>
                  </Card>

                  {evaluationResult.rules &&
                    evaluationResult.rules.length > 0 && (
                      <div>
                        <Text strong>
                          Matched Rules ({evaluationResult.rules.length}):
                        </Text>
                        {evaluationResult.rules.map(
                          (rule: any, index: number) => (
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
                          )
                        )}
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

                  {(!evaluationResult.rules ||
                    evaluationResult.rules.length === 0) &&
                    !evaluationResult.summary && (
                      <Text type="secondary">
                        No rules were matched for this event.
                      </Text>
                    )}
                </div>
              )}

              {!evaluationResult && !isLoading && (
                <Text type="secondary">
                  Run a dry-run to see evaluation results here.
                </Text>
              )}
            </TabPane>
          </Tabs>
        </Col>
      </Row>
    </div>
  )
}

export default Sandbox
