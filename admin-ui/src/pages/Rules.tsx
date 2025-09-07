import React from 'react'
import { Typography } from 'antd'
import { useParams } from 'react-router-dom'
import RulesList from '@/components/RulesList'
import RuleDetails from '@/components/RuleDetails'

const { Title } = Typography

const Rules: React.FC = () => {
  const { id } = useParams<{ id: string }>()

  if (id) {
    return <RuleDetails ruleId={id} />
  }

  return (
    <div>
      <Title level={2}>Rules Management</Title>
      <RulesList />
    </div>
  )
}

export default Rules
