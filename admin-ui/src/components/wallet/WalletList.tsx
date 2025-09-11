import React from 'react'
import { Card, List, Typography, Tag, Button, Space, Spin, Alert } from 'antd'
import { WalletOutlined, EyeOutlined, DollarOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useUserWallets } from '@/hooks/useWallet'
import type { WalletDto } from '@/api/wallet'

const { Title, Text } = Typography

interface WalletListProps {
  userId: string
  onViewWallet?: (wallet: WalletDto) => void
  onSpendPoints?: (wallet: WalletDto) => void
  onTransferPoints?: (wallet: WalletDto) => void
}

const WalletList: React.FC<WalletListProps> = ({
  userId,
  onViewWallet,
  onSpendPoints,
  onTransferPoints,
}) => {
  const navigate = useNavigate()
  const { data: wallets, isLoading, error } = useUserWallets(userId)

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
        <div style={{ marginTop: '16px' }}>Loading wallets...</div>
      </div>
    )
  }

  if (error) {
    return (
      <Alert
        message="Error loading wallets"
        description={error.message}
        type="error"
        showIcon
      />
    )
  }

  if (!wallets || wallets.length === 0) {
    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '50px' }}>
          <WalletOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
          <div style={{ marginTop: '16px', color: '#8c8c8c' }}>
            No wallets found for this user
          </div>
        </div>
      </Card>
    )
  }

  return (
    <div>
      <Title level={4}>User Wallets</Title>
      <List
        grid={{ gutter: 16, xs: 1, sm: 1, md: 2, lg: 2, xl: 3, xxl: 3 }}
        dataSource={wallets}
        renderItem={(wallet) => (
          <List.Item>
            <Card
              title={
                <Space>
                  <WalletOutlined />
                  <span
                    style={{ cursor: 'pointer', color: '#1890ff' }}
                    onClick={() =>
                      navigate(`/point-categories/${wallet.pointCategoryId}`)
                    }
                  >
                    {wallet.pointCategoryId}
                  </span>
                </Space>
              }
              extra={
                <Tag
                  color={
                    wallet.balance && wallet.balance >= 0 ? 'green' : 'red'
                  }
                >
                  {wallet.balance?.toLocaleString() || 0} points
                </Tag>
              }
              actions={[
                <Button
                  key="view"
                  type="text"
                  icon={<EyeOutlined />}
                  onClick={() => onViewWallet?.(wallet)}
                >
                  View Details
                </Button>,
                <Button
                  key="spend"
                  type="text"
                  icon={<DollarOutlined />}
                  onClick={() => onSpendPoints?.(wallet)}
                  disabled={!wallet.balance || wallet.balance <= 0}
                >
                  Spend Points
                </Button>,
                <Button
                  key="transfer"
                  type="text"
                  onClick={() => onTransferPoints?.(wallet)}
                  disabled={!wallet.balance || wallet.balance <= 0}
                >
                  Transfer
                </Button>,
              ]}
            >
              <div>
                <Text type="secondary">Balance: </Text>
                <Text
                  strong
                  style={{
                    color:
                      wallet.balance && wallet.balance >= 0
                        ? '#52c41a'
                        : '#ff4d4f',
                  }}
                >
                  {wallet.balance?.toLocaleString() || 0}
                </Text>
              </div>
              {wallet.transactions && wallet.transactions.length > 0 && (
                <div style={{ marginTop: '8px' }}>
                  <Text type="secondary">
                    {wallet.transactions.length} transaction(s)
                  </Text>
                </div>
              )}
            </Card>
          </List.Item>
        )}
      />
    </div>
  )
}

export default WalletList
