import React from 'react'
import {
  Card,
  Descriptions,
  Button,
  Space,
  Typography,
  Spin,
  Alert,
  Tag,
} from 'antd'
import { ArrowLeftOutlined } from '@ant-design/icons'

const { Title, Text } = Typography

export interface EntityDetailsItem {
  label: string
  value: React.ReactNode
  span?: number
}

interface EntityDetailsProps {
  title: string
  data: EntityDetailsItem[]
  loading: boolean
  error: Error | null
  onBack: () => void
  actions?: React.ReactNode
}

const EntityDetails: React.FC<EntityDetailsProps> = ({
  title,
  data,
  loading,
  error,
  onBack,
  actions,
}) => {
  if (loading) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Spin size="large" />
          <div style={{ marginTop: 16 }}>
            <Text>Loading...</Text>
          </div>
        </div>
      </Card>
    )
  }

  if (error) {
    return (
      <Card>
        <Alert
          message="Error loading data"
          description={error.message}
          type="error"
          showIcon
        />
        <div style={{ marginTop: 16 }}>
          <Button icon={<ArrowLeftOutlined />} onClick={onBack}>
            Back to List
          </Button>
        </div>
      </Card>
    )
  }

  return (
    <Card>
      <div style={{ marginBottom: 24 }}>
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={onBack}>
            Back to List
          </Button>
          {actions}
        </Space>
      </div>

      <div style={{ marginBottom: 24 }}>
        <Title level={2}>{title}</Title>
      </div>

      <Descriptions bordered column={{ xs: 1, sm: 2, md: 3 }} items={data} />
    </Card>
  )
}

export default EntityDetails
