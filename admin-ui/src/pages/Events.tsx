import React from 'react'
import { Typography } from 'antd'
import { useParams } from 'react-router-dom'
import EventCatalog from '@/components/EventCatalog'
import EventDetails from '@/components/EventDetails'

const { Title } = Typography

const Events: React.FC = () => {
  const { id } = useParams<{ id: string }>()

  if (id) {
    return <EventDetails eventId={id} />
  }

  return (
    <div>
      <Title level={2}>Event Catalog</Title>
      <EventCatalog />
    </div>
  )
}

export default Events
