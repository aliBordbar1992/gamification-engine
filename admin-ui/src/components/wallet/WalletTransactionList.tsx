import React from 'react'
import { Table, Tag, Typography, DatePicker, Space, Button, Card } from 'antd'
import { ReloadOutlined } from '@ant-design/icons'
import { useWalletTransactions } from '@/hooks/useWallet'
import type { WalletTransactionDto } from '@/api/wallet'
import dayjs from 'dayjs'

const { Title, Text } = Typography
const { RangePicker } = DatePicker

interface WalletTransactionListProps {
  userId: string
  pointCategoryId: string
}

const WalletTransactionList: React.FC<WalletTransactionListProps> = ({
  userId,
  pointCategoryId,
}) => {
  const [dateRange, setDateRange] = React.useState<
    [dayjs.Dayjs | null, dayjs.Dayjs | null] | null
  >(null)

  const fromDate = dateRange?.[0]?.toISOString()
  const toDate = dateRange?.[1]?.toISOString()

  const {
    data: transactions,
    isLoading,
    error,
    refetch,
  } = useWalletTransactions(userId, pointCategoryId, fromDate, toDate)

  const getTransactionTypeColor = (type: string) => {
    switch (type?.toLowerCase()) {
      case 'earn':
        return 'green'
      case 'spend':
        return 'red'
      case 'transfer':
        return 'blue'
      case 'refund':
        return 'orange'
      case 'penalty':
        return 'red'
      case 'adjustment':
        return 'purple'
      default:
        return 'default'
    }
  }

  const getAmountColor = (amount: number) => {
    return amount > 0 ? '#52c41a' : '#ff4d4f'
  }

  const columns = [
    {
      title: 'ID',
      dataIndex: 'id',
      key: 'id',
      width: 100,
      render: (id: string) => (
        <Text code style={{ fontSize: '12px' }}>
          {id?.substring(0, 8)}...
        </Text>
      ),
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      width: 100,
      render: (type: string) => (
        <Tag color={getTransactionTypeColor(type)}>{type?.toUpperCase()}</Tag>
      ),
    },
    {
      title: 'Amount',
      dataIndex: 'amount',
      key: 'amount',
      width: 120,
      render: (amount: number) => (
        <Text strong style={{ color: getAmountColor(amount) }}>
          {amount > 0 ? '+' : ''}
          {amount?.toLocaleString()}
        </Text>
      ),
    },
    {
      title: 'Description',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
    },
    {
      title: 'Reference ID',
      dataIndex: 'referenceId',
      key: 'referenceId',
      width: 120,
      render: (referenceId: string) =>
        referenceId ? (
          <Text code style={{ fontSize: '12px' }}>
            {referenceId.substring(0, 8)}...
          </Text>
        ) : (
          '-'
        ),
    },
    {
      title: 'Timestamp',
      dataIndex: 'timestamp',
      key: 'timestamp',
      width: 180,
      render: (timestamp: string) => (
        <Text type="secondary">
          {dayjs(timestamp).format('YYYY-MM-DD HH:mm:ss')}
        </Text>
      ),
      sorter: (a: WalletTransactionDto, b: WalletTransactionDto) =>
        dayjs(a.timestamp).unix() - dayjs(b.timestamp).unix(),
    },
  ]

  const handleDateRangeChange = (
    dates: [dayjs.Dayjs | null, dayjs.Dayjs | null] | null
  ) => {
    setDateRange(dates)
  }

  const handleRefresh = () => {
    refetch()
  }

  return (
    <Card>
      <div style={{ marginBottom: '16px' }}>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            marginBottom: '16px',
          }}
        >
          <Title level={4}>Transaction History</Title>
          <Space>
            <RangePicker
              value={dateRange}
              onChange={handleDateRangeChange}
              placeholder={['Start Date', 'End Date']}
            />
            <Button icon={<ReloadOutlined />} onClick={handleRefresh}>
              Refresh
            </Button>
          </Space>
        </div>

        {transactions && transactions.length > 0 && (
          <Text type="secondary">
            Showing {transactions.length} transaction(s)
            {dateRange && (
              <span>
                {' '}
                from {dateRange[0]?.format('YYYY-MM-DD')} to{' '}
                {dateRange[1]?.format('YYYY-MM-DD')}
              </span>
            )}
          </Text>
        )}
      </div>

      <Table
        columns={columns}
        dataSource={transactions || []}
        loading={isLoading}
        rowKey="id"
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          showQuickJumper: true,
          showTotal: (total, range) =>
            `${range[0]}-${range[1]} of ${total} transactions`,
        }}
        scroll={{ x: 800 }}
        size="small"
      />

      {error && (
        <div style={{ marginTop: '16px' }}>
          <Text type="danger">Error loading transactions: {error.message}</Text>
        </div>
      )}
    </Card>
  )
}

export default WalletTransactionList
