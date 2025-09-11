import React from 'react'
import { Card, Table, Button, Space, Typography, Alert } from 'antd'
import { EyeOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import type { ColumnsType } from 'antd/es/table'

const { Title, Text } = Typography

export interface EntityListItem {
  id: string
  name: string
  description: string | undefined
  [key: string]: string | number | boolean | object | undefined
}

interface EntityListProps {
  title: string
  data: EntityListItem[]
  loading: boolean
  error: Error | null
  columns: ColumnsType<EntityListItem>
  onViewDetails: (id: string) => void
  emptyMessage?: string
  entityType?: string // e.g., 'badges', 'trophies', 'levels', 'point-categories'
}

const EntityList: React.FC<EntityListProps> = ({
  title,
  data,
  loading,
  error,
  columns,
  onViewDetails,
  emptyMessage = 'No items found',
  entityType,
}) => {
  const navigate = useNavigate()
  const enhancedColumns: ColumnsType<EntityListItem> = [
    ...columns.map((col) => {
      // Make the name column clickable if entityType is provided
      if (col.dataIndex === 'name' && entityType) {
        return {
          ...col,
          render: (name: string, record: EntityListItem) => (
            <Text
              strong
              style={{ cursor: 'pointer', color: '#1890ff' }}
              onClick={() => navigate(`/${entityType}/${record.id}`)}
            >
              {name}
            </Text>
          ),
        }
      }
      return col
    }),
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      render: (_, record) => (
        <Space>
          <Button
            type="link"
            icon={<EyeOutlined />}
            onClick={() => onViewDetails(record.id)}
            size="small"
          >
            View
          </Button>
        </Space>
      ),
    },
  ]

  if (error) {
    return (
      <Card>
        <Alert
          message="Error loading data"
          description={error.message}
          type="error"
          showIcon
        />
      </Card>
    )
  }

  return (
    <Card>
      <div style={{ marginBottom: 16 }}>
        <Title level={3}>{title}</Title>
        <Text type="secondary">
          {data.length} {data.length === 1 ? 'item' : 'items'} found
        </Text>
      </div>

      <Table
        columns={enhancedColumns}
        dataSource={data}
        loading={loading}
        rowKey="id"
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          showQuickJumper: true,
          showTotal: (total: number, range: [number, number]) =>
            `${range[0]}-${range[1]} of ${total} items`,
        }}
        locale={{
          emptyText: (
            <div style={{ padding: '40px 0' }}>
              <Text type="secondary">{emptyMessage}</Text>
            </div>
          ),
        }}
      />
    </Card>
  )
}

export default EntityList
