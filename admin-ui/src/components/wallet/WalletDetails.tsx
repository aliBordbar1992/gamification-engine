import React from 'react'
import {
  Card,
  Descriptions,
  Button,
  Space,
  Typography,
  Tag,
  Spin,
  Alert,
} from 'antd'
import {
  ArrowLeftOutlined,
  DollarOutlined,
  SwapOutlined,
} from '@ant-design/icons'
import { useWallet } from '@/hooks/useWallet'
import type { WalletDto } from '@/api/wallet'

const { Title, Text } = Typography

interface WalletDetailsProps {
  userId: string
  pointCategoryId: string
  onBack: () => void
  onSpendPoints?: (wallet: WalletDto) => void
  onTransferPoints?: (wallet: WalletDto) => void
}

const WalletDetails: React.FC<WalletDetailsProps> = ({
  userId,
  pointCategoryId,
  onBack,
  onSpendPoints,
  onTransferPoints,
}) => {
  const { data: wallet, isLoading, error } = useWallet(userId, pointCategoryId)

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" />
        <div style={{ marginTop: '16px' }}>Loading wallet details...</div>
      </div>
    )
  }

  if (error) {
    return (
      <Alert
        message="Error loading wallet"
        description={error.message}
        type="error"
        showIcon
      />
    )
  }

  if (!wallet) {
    return (
      <Alert
        message="Wallet not found"
        description="The requested wallet could not be found."
        type="warning"
        showIcon
      />
    )
  }

  const canSpend = wallet.balance && wallet.balance > 0

  return (
    <div>
      <div style={{ marginBottom: '16px' }}>
        <Button icon={<ArrowLeftOutlined />} onClick={onBack}>
          Back
        </Button>
      </div>

      <Card>
        <div style={{ marginBottom: '24px' }}>
          <Title level={3}>Wallet: {wallet.pointCategoryId}</Title>
          <Text type="secondary">User: {wallet.userId}</Text>
        </div>

        <Descriptions bordered column={2}>
          <Descriptions.Item label="User ID" span={1}>
            {wallet.userId}
          </Descriptions.Item>
          <Descriptions.Item label="Point Category" span={1}>
            <Tag color="blue">{wallet.pointCategoryId}</Tag>
          </Descriptions.Item>
          <Descriptions.Item label="Current Balance" span={2}>
            <Text
              strong
              style={{
                fontSize: '18px',
                color:
                  wallet.balance && wallet.balance >= 0 ? '#52c41a' : '#ff4d4f',
              }}
            >
              {wallet.balance?.toLocaleString() || 0} points
            </Text>
          </Descriptions.Item>
          <Descriptions.Item label="Transaction Count" span={2}>
            {wallet.transactions?.length || 0} transactions
          </Descriptions.Item>
        </Descriptions>

        <div style={{ marginTop: '24px' }}>
          <Space>
            <Button
              type="primary"
              icon={<DollarOutlined />}
              onClick={() => onSpendPoints?.(wallet)}
              disabled={!canSpend}
            >
              Spend Points
            </Button>
            <Button
              icon={<SwapOutlined />}
              onClick={() => onTransferPoints?.(wallet)}
              disabled={!canSpend}
            >
              Transfer Points
            </Button>
          </Space>
        </div>
      </Card>
    </div>
  )
}

export default WalletDetails
