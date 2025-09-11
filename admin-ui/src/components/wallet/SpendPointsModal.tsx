import React from 'react'
import { Modal, Form, InputNumber, Input, message, Typography } from 'antd'
import { useSpendPoints } from '@/hooks/useWallet'
import type { WalletDto, SpendPointsRequestDto } from '@/api/wallet'

const { Text } = Typography

interface SpendPointsModalProps {
  visible: boolean
  wallet: WalletDto | null
  onCancel: () => void
  onSuccess?: () => void
}

const SpendPointsModal: React.FC<SpendPointsModalProps> = ({
  visible,
  wallet,
  onCancel,
  onSuccess,
}) => {
  const [form] = Form.useForm()
  const spendPointsMutation = useSpendPoints()

  const handleSubmit = async (values: SpendPointsRequestDto) => {
    if (!wallet?.userId) {
      message.error('Invalid wallet information')
      return
    }

    try {
      await spendPointsMutation.mutateAsync({
        userId: wallet.userId,
        request: {
          pointCategoryId: wallet.pointCategoryId,
          amount: values.amount,
          description: values.description,
          referenceId: values.referenceId,
          metadata: values.metadata,
        },
      })

      message.success('Points spent successfully!')
      form.resetFields()
      onSuccess?.()
      onCancel()
    } catch (error) {
      message.error('Failed to spend points. Please try again.')
      console.error('Spend points error:', error)
    }
  }

  const handleCancel = () => {
    form.resetFields()
    onCancel()
  }

  const currentBalance = wallet?.balance || 0

  return (
    <Modal
      title="Spend Points"
      open={visible}
      onCancel={handleCancel}
      onOk={() => form.submit()}
      confirmLoading={spendPointsMutation.isPending}
      okText="Spend Points"
      cancelText="Cancel"
      width={500}
    >
      {wallet && (
        <div style={{ marginBottom: '16px' }}>
          <Text strong>Wallet: </Text>
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
          name="amount"
          label="Amount to Spend"
          rules={[
            { required: true, message: 'Please enter the amount to spend' },
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
            placeholder="Enter amount to spend"
          />
        </Form.Item>

        <Form.Item
          name="description"
          label="Description"
          rules={[{ required: true, message: 'Please enter a description' }]}
        >
          <Input.TextArea
            rows={3}
            placeholder="Describe what the points are being spent on"
            maxLength={500}
            showCount
          />
        </Form.Item>

        <Form.Item name="referenceId" label="Reference ID (Optional)">
          <Input
            placeholder="External reference ID (e.g., order ID, purchase ID)"
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

export default SpendPointsModal
