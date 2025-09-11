import React from 'react'
import { Tag, Switch } from 'antd'
import EntityDetails from '../EntityDetails'
import { usePointCategory } from '@/hooks/useEntities'
import type { EntityDetailsItem } from '../EntityDetails'
import type { CreatePointCategoryDto } from '@/api/generated/models'

// Type alias for better readability
type PointCategory = CreatePointCategoryDto

interface PointCategoryDetailsProps {
  id: string
  onBack: () => void
}

const PointCategoryDetails: React.FC<PointCategoryDetailsProps> = ({
  id,
  onBack,
}) => {
  const { data: pointCategory, isLoading, error } = usePointCategory(id)

  const detailsData: EntityDetailsItem[] = pointCategory
    ? [
        {
          label: 'ID',
          value: pointCategory.id,
          span: 1,
        },
        {
          label: 'Name',
          value: pointCategory.name,
          span: 2,
        },
        {
          label: 'Description',
          value: pointCategory.description,
          span: 3,
        },
        {
          label: 'Aggregation Method',
          value: <Tag color="purple">{pointCategory.aggregation}</Tag>,
          span: 3,
        },
        {
          label: 'Is Spendable',
          value: (
            <Switch
              checked={pointCategory.isSpendable}
              disabled
              checkedChildren="Yes"
              unCheckedChildren="No"
            />
          ),
          span: 1,
        },
        {
          label: 'Negative Balance Allowed',
          value: (
            <Switch
              checked={pointCategory.negativeBalanceAllowed}
              disabled
              checkedChildren="Yes"
              unCheckedChildren="No"
            />
          ),
          span: 2,
        },
      ]
    : []

  return (
    <EntityDetails
      title={pointCategory?.name || 'Point Category Details'}
      data={detailsData}
      loading={isLoading}
      error={error}
      onBack={onBack}
    />
  )
}

export default PointCategoryDetails
