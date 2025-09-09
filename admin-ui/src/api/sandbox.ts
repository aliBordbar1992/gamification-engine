import type {
  IngestEventRequest,
  ApiEventsSandboxDryRunPostRequest,
  ApiEventsSandboxDryRunPost200Response,
} from './generated/models'
import { EventsApiInstance } from './generated-client'

// Sandbox API for dry-run testing
export const sandboxApi = {
  // Dry-run event evaluation (no persistence/side effects)
  dryRunEvent: async (
    event: IngestEventRequest
  ): Promise<ApiEventsSandboxDryRunPost200Response> => {
    // Convert IngestEventRequest to ApiEventsSandboxDryRunPostRequest
    const dryRunRequest: ApiEventsSandboxDryRunPostRequest = {
      eventId: event.eventId || undefined,
      eventType: event.eventType,
      userId: event.userId,
      occurredAt: event.occurredAt || undefined,
      attributes: event.attributes || {},
    }

    const response = await EventsApiInstance().apiEventsSandboxDryRunPost(
      dryRunRequest
    )
    return response.data
  },

  // Get cURL command for the dry-run event
  getCurlCommand: (event: IngestEventRequest, baseUrl: string): string => {
    const dryRunRequest = {
      eventId: event.eventId || undefined,
      eventType: event.eventType,
      userId: event.userId,
      occurredAt: event.occurredAt || undefined,
      attributes: event.attributes || {},
    }
    const eventJson = JSON.stringify(dryRunRequest, null, 2)
    return `curl -X POST "${baseUrl}/api/Events/sandbox/dry-run" \\
  -H "Content-Type: application/json" \\
  -H "X-API-Key: YOUR_API_KEY" \\
  -d '${eventJson}'`
  },
}
