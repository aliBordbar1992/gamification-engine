import React from 'react'
import { Modal, Form, InputNumber, Input, message, Typography } from 'antd'
import { useTransferPoints } from '@/hooks/useWallet'
import type { WalletDto, TransferPointsRequestDto } from '@/api/wallet'

const { Text } = Typography

interface TransferPointsModalProps {
  visible: boolean
  wallet: WalletDto | null
  onCancel: () => void
  onSuccess?: () => void
}

const TransferPointsModal: React.FC<TransferPointsModalProps> = ({
  visible,
  wallet,
  onCancel,
  onSuccess,
}) => {
  const [form] = Form.useForm()
  const transferPointsMutation = useTransferPoints()

  const handleSubmit = async (values: TransferPointsRequestDto) => {
    if (!wallet?.userId) {
      message.error('Invalid wallet information')
      return
    }

    try {
      await transferPointsMutation.mutateAsync({
        fromUserId: wallet.userId,
        request: {
          toUserId: values.toUserId,
          pointCategoryId: wallet.pointCategoryId,
          amount: values.amount,
          description: values.description,
          referenceId: values.referenceId,
          metadata: values.metadata,
        },
      })

      message.success('Points transferred successfully!')
      form.resetFields()
      onSuccess?.()
      onCancel()
    } catch (error) {
      message.error('Failed to transfer points. Please try again.')
      console.error('Transfer points error:', error)
    }
  }

  const handleCancel = () => {
    form.resetFields()
    onCancel()
  }

  const currentBalance = wallet?.balance || 0

  return (
    <Modal
      title="Transfer Points"
      open={visible}
      onCancel={handleCancel}
      onOk={() => form.submit()}
      confirmLoading={transferPointsMutation.isPending}
      okText="Transfer Points"
      cancelText="Cancel"
      width={500}
    >
      {wallet && (
        <div style={{ marginBottom: '16px' }}>
          <Text strong>From Wallet: </Text>
          <Text>{wallet.pointCategoryId}</Text>
          <br />
          <Text strong>Current Balance: </Text>
          <Text style={{ color: currentBalance >= 0 ? '#52c41a' : '#ff4d4f' }}>
            {currentBalance.toLocaleString()} points
          </Text>
        </div>
      )}

      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        initialValues={{
          amount: 0,
        }}
      >
        <Form.Item
          name="toUserId"
          label="Recipient User ID"
          rules={[
            { required: true, message: 'Please enter the recipient user ID' },
            {
              validator: (_, value) => {
                if (value === wallet?.userId) {
                  return Promise.reject(
                    new Error('Cannot transfer to yourself')
                  )
                }
                return Promise.resolve()
              },
            },
          ]}
        >
          <Input placeholder="Enter recipient user ID" maxLength={100} />
        </Form.Item>

        <Form.Item
          name="amount"
          label="Amount to Transfer"
          rules={[
            { required: true, message: 'Please enter the amount to transfer' },
            {
              type: 'number',
              min: 0.01,
              message: 'Amount must be greater than 0',
            },
            {
              validator: (_, value) => {
                if (value > currentBalance) {
                  return Promise.reject(
                    new Error('Amount cannot exceed current balance')
                  )
                }
                return Promise.resolve()
              },
            },
          ]}
        >
          <InputNumber
            style={{ width: '100%' }}
            min={0.01}
            max={currentBalance}
            step={0.01}
            precision={2}
            placeholder="Enter amount to transfer"
          />
        </Form.Item>

        <Form.Item
          name="description"
          label="Description"
          rules={[{ required: true, message: 'Please enter a description' }]}
        >
          <Input.TextArea
            rows={3}
            placeholder="Describe the purpose of this transfer"
            maxLength={500}
            showCount
          />
        </Form.Item>

        <Form.Item name="referenceId" label="Reference ID (Optional)">
          <Input
            placeholder="External reference ID (e.g., transaction ID, gift ID)"
            maxLength={100}
          />
        </Form.Item>

        <Form.Item name="metadata" label="Metadata (Optional)">
          <Input.TextArea
            rows={2}
            placeholder="Additional metadata (JSON format)"
            maxLength={1000}
          />
        </Form.Item>
      </Form>
    </Modal>
  )
}

export default TransferPointsModal
