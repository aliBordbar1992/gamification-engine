import type {
  EventDefinitionDto,
  EventDto,
  IngestEventRequest,
} from './generated/models'
import {
  EventsApiInstance,
  EventDefinitionApiInstance,
} from './generated-client'

// Event catalog API
export const eventsApi = {
  // Get event catalog (all available event definitions)
  getEventCatalog: async (): Promise<EventDefinitionDto[]> => {
    const response = await EventsApiInstance().apiEventsCatalogGet()
    return response.data
  },

  // Get specific event by ID
  getEventById: async (eventId: string): Promise<EventDto> => {
    const response = await EventsApiInstance().apiEventsEventIdGet(eventId)
    return response.data
  },

  // Get events by type
  getEventsByType: async (
    eventType: string,
    limit?: number,
    offset?: number
  ): Promise<EventDto[]> => {
    const response = await EventsApiInstance().apiEventsTypeEventTypeGet(
      eventType,
      limit,
      offset
    )
    return response.data
  },

  // Get events by user
  getEventsByUser: async (
    userId: string,
    limit?: number,
    offset?: number
  ): Promise<EventDto[]> => {
    const response = await EventsApiInstance().apiEventsUserUserIdGet(
      userId,
      limit,
      offset
    )
    return response.data
  },

  // Ingest a new event
  ingestEvent: async (event: IngestEventRequest): Promise<EventDto> => {
    const response = await EventsApiInstance().apiEventsPost(event)
    return response.data
  },
}

// Event Definition API
export const eventDefinitionsApi = {
  // Get all event definitions
  getAllEventDefinitions: async (): Promise<EventDefinitionDto[]> => {
    const response = await EventDefinitionApiInstance().apiEventDefinitionGet()
    return response.data
  },

  // Get specific event definition by ID
  getEventDefinitionById: async (id: string): Promise<EventDefinitionDto> => {
    const response = await EventDefinitionApiInstance().apiEventDefinitionIdGet(
      id
    )
    return response.data
  },
}
