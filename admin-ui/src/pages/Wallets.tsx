import React, { useState } from 'react'
import { Input, Button, Space, Typography, Card, message } from 'antd'
import { SearchOutlined, UserOutlined } from '@ant-design/icons'
import WalletList from '@/components/wallet/WalletList'
import WalletDetails from '@/components/wallet/WalletDetails'
import WalletTransactionList from '@/components/wallet/WalletTransactionList'
import SpendPointsModal from '@/components/wallet/SpendPointsModal'
import TransferPointsModal from '@/components/wallet/TransferPointsModal'
import type { WalletDto } from '@/api/wallet'

const { Title } = Typography

type ViewMode = 'search' | 'wallet-details'

const Wallets: React.FC = () => {
  const [userId, setUserId] = useState<string>('')
  const [searchUserId, setSearchUserId] = useState<string>('')
  const [viewMode, setViewMode] = useState<ViewMode>('search')
  const [selectedWallet, setSelectedWallet] = useState<WalletDto | null>(null)
  const [spendModalVisible, setSpendModalVisible] = useState(false)
  const [transferModalVisible, setTransferModalVisible] = useState(false)

  const handleSearch = () => {
    if (!userId.trim()) {
      message.warning('Please enter a user ID')
      return
    }
    setSearchUserId(userId.trim())
    setViewMode('search')
    setSelectedWallet(null)
  }

  const handleViewWallet = (wallet: WalletDto) => {
    setSelectedWallet(wallet)
    setViewMode('wallet-details')
  }

  const handleSpendPoints = (wallet: WalletDto) => {
    setSelectedWallet(wallet)
    setSpendModalVisible(true)
  }

  const handleTransferPoints = (wallet: WalletDto) => {
    setSelectedWallet(wallet)
    setTransferModalVisible(true)
  }

  const handleBackToSearch = () => {
    setViewMode('search')
    setSelectedWallet(null)
  }

  const handleModalSuccess = () => {
    // Refresh data will be handled by React Query invalidation in the hooks
    message.success('Operation completed successfully!')
  }

  const renderContent = () => {
    if (viewMode === 'wallet-details' && selectedWallet) {
      return (
        <div>
          <WalletDetails
            userId={selectedWallet.userId!}
            pointCategoryId={selectedWallet.pointCategoryId!}
            onBack={handleBackToSearch}
            onSpendPoints={handleSpendPoints}
            onTransferPoints={handleTransferPoints}
          />
          <div style={{ marginTop: '24px' }}>
            <WalletTransactionList
              userId={selectedWallet.userId!}
              pointCategoryId={selectedWallet.pointCategoryId!}
            />
          </div>
        </div>
      )
    }

    if (searchUserId) {
      return (
        <WalletList
          userId={searchUserId}
          onViewWallet={handleViewWallet}
          onSpendPoints={handleSpendPoints}
          onTransferPoints={handleTransferPoints}
        />
      )
    }

    return (
      <Card>
        <div style={{ textAlign: 'center', padding: '50px' }}>
          <UserOutlined style={{ fontSize: '48px', color: '#d9d9d9' }} />
          <div style={{ marginTop: '16px', color: '#8c8c8c' }}>
            Enter a user ID to view their wallets
          </div>
        </div>
      </Card>
    )
  }

  return (
    <div>
      <div style={{ marginBottom: '24px' }}>
        <Title level={2}>Wallet Management</Title>
        <p>View and manage user wallets, transactions, and transfers.</p>
      </div>

      <Card style={{ marginBottom: '24px' }}>
        <Space.Compact style={{ width: '100%' }}>
          <Input
            placeholder="Enter User ID"
            value={userId}
            onChange={(e) => setUserId(e.target.value)}
            onPressEnter={handleSearch}
            prefix={<UserOutlined />}
            style={{ flex: 1 }}
          />
          <Button
            type="primary"
            icon={<SearchOutlined />}
            onClick={handleSearch}
          >
            Search Wallets
          </Button>
        </Space.Compact>
      </Card>

      {renderContent()}

      <SpendPointsModal
        visible={spendModalVisible}
        wallet={selectedWallet}
        onCancel={() => setSpendModalVisible(false)}
        onSuccess={handleModalSuccess}
      />

      <TransferPointsModal
        visible={transferModalVisible}
        wallet={selectedWallet}
        onCancel={() => setTransferModalVisible(false)}
        onSuccess={handleModalSuccess}
      />
    </div>
  )
}

export default Wallets
