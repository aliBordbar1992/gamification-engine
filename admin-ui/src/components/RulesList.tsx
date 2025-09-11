import React, { useState, useMemo } from 'react'
import {
  Table,
  Input,
  Select,
  Button,
  Space,
  Tag,
  Typography,
  Card,
  Row,
  Col,
  Tooltip,
  Badge,
} from 'antd'
import { FilterOutlined, EyeOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useRules } from '@/hooks/useRules'
import type { RuleDto } from '@/api/generated/models'

// Filter and search types
export interface RulesFilters {
  isActive?: boolean
  triggerType?: string
  search?: string
}

const { Title, Text } = Typography
const { Search } = Input
const { Option } = Select

interface RulesListProps {
  onViewRule?: (rule: RuleDto) => void
}

const RulesList: React.FC<RulesListProps> = ({ onViewRule }) => {
  const navigate = useNavigate()
  const [filters, setFilters] = useState<RulesFilters>({})
  const [searchText, setSearchText] = useState('')

  const { data: rules = [], isLoading, error } = useRules()

  const handleViewRule = (rule: RuleDto) => {
    if (onViewRule) {
      onViewRule(rule)
    } else {
      navigate(`/rules/${rule.id}`)
    }
  }

  const handleFilterChange = (key: keyof RulesFilters, value: boolean) => {
    setFilters((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  const clearFilters = () => {
    setFilters({})
    setSearchText('')
  }

  const columns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record: RuleDto) => (
        <div>
          <Text strong>{name}</Text>
          <br />
          <Text type="secondary" style={{ fontSize: '12px' }}>
            ID: {record.id}
          </Text>
        </div>
      ),
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (description: string) => (
        <Text type="secondary">{description}</Text>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 100,
      render: (isActive: boolean) => (
        <Badge
          status={isActive ? 'success' : 'default'}
          text={isActive ? 'Active' : 'Inactive'}
        />
      ),
    },
    {
      title: 'Triggers',
      dataIndex: 'triggers',
      key: 'triggers',
      render: (triggers: string[]) => (
        <Space wrap>
          {triggers.slice(0, 2).map((trigger) => (
            <Tag
              key={trigger}
              color="blue"
              style={{ cursor: 'pointer' }}
              onClick={() => navigate(`/events/${trigger}`)}
            >
              {trigger}
            </Tag>
          ))}
          {triggers.length > 2 && (
            <Tooltip title={triggers.slice(2).join(', ')}>
              <Tag color="default">+{triggers.length - 2}</Tag>
            </Tooltip>
          )}
        </Space>
      ),
    },

    {
      title: 'Created',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 120,
      render: (date: string) => (
        <Text type="secondary">{new Date(date).toLocaleDateString()}</Text>
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_: unknown, record: RuleDto) => (
        <Space>
          <Button
            type="link"
            icon={<EyeOutlined />}
            onClick={() => handleViewRule(record)}
          >
            View
          </Button>
        </Space>
      ),
    },
  ]

  const stats = useMemo(() => {
    const total = rules.length
    const active = rules.filter((rule: RuleDto) => rule.isActive).length
    const inactive = total - active
    const withSpendings = rules.filter(
      (rule: RuleDto) => rule.spendings && rule.spendings.length > 0
    ).length
    return { total, active, inactive, withSpendings }
  }, [rules])

  if (error) {
    return (
      <Card>
        <Text type="danger">Error loading rules: {error.message}</Text>
      </Card>
    )
  }

  return (
    <div>
      {/* Stats Cards */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#1890ff' }}>
                {stats.total}
              </Title>
              <Text type="secondary">Total Rules</Text>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#52c41a' }}>
                {stats.active}
              </Title>
              <Text type="secondary">Active Rules</Text>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#d9d9d9' }}>
                {stats.inactive}
              </Title>
              <Text type="secondary">Inactive Rules</Text>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Title level={3} style={{ margin: 0, color: '#fa8c16' }}>
                {stats.withSpendings}
              </Title>
              <Text type="secondary">With Spendings</Text>
            </div>
          </Card>
        </Col>
      </Row>

      {/* Filters */}
      <Card style={{ marginBottom: 16 }}>
        <Row gutter={16} align="middle">
          <Col flex="auto">
            <Search
              placeholder="Search rules by name, description, or ID..."
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              onSearch={setSearchText}
              style={{ maxWidth: 400 }}
            />
          </Col>
          <Col>
            <Select
              placeholder="Status"
              value={filters.isActive}
              onChange={(value) => handleFilterChange('isActive', value)}
              style={{ width: 120 }}
              allowClear
            >
              <Option value={true}>Active</Option>
              <Option value={false}>Inactive</Option>
            </Select>
          </Col>
          <Col>
            <Button
              icon={<FilterOutlined />}
              onClick={clearFilters}
              disabled={!Object.keys(filters).length && !searchText}
            >
              Clear Filters
            </Button>
          </Col>
        </Row>
      </Card>

      {/* Rules Table */}
      <Card>
        <Table
          columns={columns}
          dataSource={rules}
          rowKey="id"
          loading={isLoading}
          pagination={{
            pageSize: 20,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} of ${total} rules`,
          }}
          scroll={{ x: 800 }}
        />
      </Card>
    </div>
  )
}

export default RulesList
