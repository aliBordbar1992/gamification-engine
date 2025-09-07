import React from 'react'
import { Button, Card, Typography, Space } from 'antd'
import { useRules } from '@/hooks/useRules'

const { Title, Text } = Typography

const TestApi: React.FC = () => {
  const { data: rules, isLoading, error, refetch } = useRules()

  return (
    <div style={{ padding: 24 }}>
      <Title level={2}>API Test Page</Title>

      <Space direction="vertical" style={{ width: '100%' }}>
        <Button type="primary" onClick={() => refetch()}>
          Test API Connection
        </Button>

        <Card title="API Status">
          {isLoading && <Text>Loading...</Text>}
          {error && <Text type="danger">Error: {error.message}</Text>}
          {rules && (
            <div>
              <Text type="success">Success! Found {rules.length} rules</Text>
              <ul>
                {rules.map((rule) => (
                  <li key={rule.id}>
                    <strong>{rule.name}</strong> - {rule.description}
                    <span
                      style={{
                        marginLeft: 8,
                        color: rule.isActive ? 'green' : 'red',
                      }}
                    >
                      ({rule.isActive ? 'Active' : 'Inactive'})
                    </span>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </Card>
      </Space>
    </div>
  )
}

export default TestApi
