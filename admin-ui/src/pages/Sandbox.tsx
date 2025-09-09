import React, { useState } from 'react'
import { Typography, Row, Col, Tabs, message } from 'antd'
import { useSandbox } from '@/hooks/useSandbox'
import EventEditor from '@/components/sandbox/EventEditor'
import ResultsDisplay from '@/components/sandbox/ResultsDisplay'
import ExamplesTab from '@/components/sandbox/ExamplesTab'
import ContextTab from '@/components/sandbox/ContextTab'
import HistoryTab from '@/components/sandbox/HistoryTab'

const { Title } = Typography

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
    engineContext,
    isEngineContextLoading,
    updateEventFromEditor,
    runDryRun,
    getCurlCommand,
    loadExample,
    saveToHistory,
    loadFromHistory,
    deleteFromHistory,
    clearHistory,
    generateSmartAttributes,
    loadTemplateFromEventId,
  } = useSandbox()

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

  const handleLanguageChange = (newLanguage: 'json' | 'yaml') => {
    setLanguage(newLanguage)
  }

  const handleLoadExample = (
    exampleType: 'login' | 'purchase' | 'achievement' | 'smart'
  ) => {
    loadExample(exampleType)
  }

  const handleGenerateSmartAttributes = () => {
    generateSmartAttributes()
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
  }

  const handleClearHistory = () => {
    clearHistory()
  }

  return (
    <div style={{ padding: '24px' }}>
      <div style={{ marginBottom: '24px' }}>
        <Title level={2}>Sandbox - Event Dry Run</Title>
        <Typography.Text type="secondary">
          Test event processing without side effects. See what rules would
          trigger and what rewards would be issued.
        </Typography.Text>
      </div>

      <Row gutter={[24, 24]}>
        {/* Event Editor */}
        <Col xs={24} lg={16}>
          <EventEditor
            eventData={eventData}
            language={language}
            isValid={isValid}
            validationErrors={validationErrors}
            isLoading={isLoading}
            engineContext={engineContext}
            onLanguageChange={handleLanguageChange}
            onEditorChange={handleEditorChange}
            onRunDryRun={handleRunDryRun}
            onSaveToHistory={handleSaveToHistory}
            onCopyCurl={handleCopyCurl}
            getCurlCommand={getCurlCommand}
          />
          <div style={{ marginTop: '16px' }}>
            <ResultsDisplay
              error={error || undefined}
              evaluationResult={evaluationResult}
              isLoading={isLoading}
            />
          </div>
        </Col>

        {/* Examples, History and Results */}
        <Col xs={24} lg={8}>
          <Tabs
            activeKey={activeTab}
            onChange={setActiveTab}
            items={[
              {
                key: 'examples',
                label: 'Examples',
                children: (
                  <ExamplesTab
                    eventData={eventData}
                    isEngineContextLoading={isEngineContextLoading}
                    engineContext={engineContext}
                    onLoadExample={handleLoadExample}
                    onGenerateSmartAttributes={handleGenerateSmartAttributes}
                    onLoadTemplateFromEventId={loadTemplateFromEventId}
                  />
                ),
              },
              {
                key: 'context',
                label: 'Engine Context',
                children: (
                  <ContextTab
                    isEngineContextLoading={isEngineContextLoading}
                    engineContext={engineContext}
                  />
                ),
              },
              {
                key: 'history',
                label: 'History',
                children: (
                  <HistoryTab
                    history={history}
                    currentHistoryId={currentHistoryId || undefined}
                    onLoadFromHistory={handleLoadFromHistory}
                    onDeleteFromHistory={handleDeleteFromHistory}
                    onClearHistory={handleClearHistory}
                  />
                ),
              },
            ]}
          />
        </Col>
      </Row>
    </div>
  )
}

export default Sandbox
