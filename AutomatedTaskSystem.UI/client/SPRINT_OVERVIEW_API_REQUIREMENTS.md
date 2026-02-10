# Sprint Overview API Requirements

## Endpoint

**GET** `/sprints/{sprintId}/analytics/overview`

## Description

This endpoint provides pre-calculated analytics data for the Sprint Overview page, including task summaries, learning objectives summaries, and tag distributions.

## Authentication

Requires authentication headers (Bearer token).

## Request Parameters

| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| sprintId | string/number | Path | Yes | The unique identifier of the sprint |

## Response Format

The endpoint should return a JSON response with the following structure:

```json
{
  "data": {
    "tags": [
      {
        "label": "string",
        "value": number,
        "color": "string (hex color)",
        "isFilled": boolean (optional)
      }
    ],
    "taskSummary": {
      "active": number,
      "completed": number,
      "rollback": number,
      "flagged": number,
      "notStarted": number,
      "total": number
    },
    "loSummary": {
      "completed": number,
      "notStarted": number,
      "total": number
    },
    "sprintSummary": {
      "active": number,
      "completed": number,
      "rollback": number,
      "flagged": number,
      "notStarted": number,
      "total": number
    }
  },
  "error": false,
  "message": "string (optional)"
}
```

## Data Structure Details

### tags (Array of TagData)
Represents learning objectives with their task counts and visual properties.

- **label**: The name/abbreviation of the learning objective (e.g., "ID", "SME", "Proofreading")
- **value**: Number of tasks associated with this learning objective
- **color**: Hex color code for visual representation (e.g., "#ef4444")
- **isFilled**: Optional boolean to indicate if the tag should be filled with color (default: false)

### taskSummary (TaskSummary)
Breakdown of all tasks in the sprint by their status.

- **active**: Number of tasks currently in progress (Status: 1)
- **completed**: Number of tasks that are done (Status: 2)
- **rollback**: Number of tasks sent back for revision
- **flagged**: Number of tasks marked for attention/issues
- **notStarted**: Number of tasks not yet started (Status: 0)
- **total**: Total number of tasks in the sprint

### loSummary (LearningObjectiveSummary)
Overall learning objectives completion status.

- **completed**: Number of learning objectives fully completed
- **notStarted**: Number of learning objectives not yet started
- **total**: Total number of learning objectives in the sprint

### sprintSummary (TaskSummary)
Overall sprint task breakdown (same structure as taskSummary).
This typically mirrors taskSummary but can be used for different aggregation logic if needed.

## Example Response

```json
{
  "data": {
    "tags": [
      { "label": "ID", "value": 50, "color": "#ef4444", "isFilled": false },
      { "label": "SME", "value": 50, "color": "#f59e0b", "isFilled": false },
      { "label": "Proofreading", "value": 50, "color": "#10b981", "isFilled": false },
      { "label": "Graphic Designer", "value": 50, "color": "#3b82f6", "isFilled": false },
      { "label": "VO", "value": 50, "color": "#8b5cf6", "isFilled": false },
      { "label": "Animation", "value": 45, "color": "#ec4899", "isFilled": false },
      { "label": "Multimedia Developers", "value": 50, "color": "#14b8a6", "isFilled": false },
      { "label": "Native Developers", "value": 50, "color": "#f97316", "isFilled": false },
      { "label": "Quality Assurance", "value": 50, "color": "#06b6d4", "isFilled": false },
      { "label": "Translation", "value": 14, "color": "#84cc16", "isFilled": false }
    ],
    "taskSummary": {
      "active": 54,
      "completed": 254,
      "rollback": 44,
      "flagged": 52,
      "notStarted": 55,
      "total": 459
    },
    "loSummary": {
      "completed": 6,
      "notStarted": 4,
      "total": 10
    },
    "sprintSummary": {
      "active": 54,
      "completed": 254,
      "rollback": 44,
      "flagged": 52,
      "notStarted": 55,
      "total": 459
    }
  },
  "error": false,
  "message": "Sprint overview data retrieved successfully"
}
```

## Error Response

```json
{
  "data": null,
  "error": true,
  "message": "Error message describing what went wrong"
}
```

## Status Codes

- **200 OK**: Successfully retrieved sprint overview data
- **401 Unauthorized**: Missing or invalid authentication token
- **404 Not Found**: Sprint with the specified ID does not exist
- **500 Internal Server Error**: Server error occurred while processing the request

## Frontend Integration

The frontend expects the response to follow the `ResponseService<SprintOverviewData>` interface:

```typescript
interface ResponseService<T> {
  data?: T;
  error: boolean;
  message?: string;
}
```

The frontend will:
1. Check if `error` is `false` and `data` is present
2. Display loading state while fetching
3. Show error message if `error` is `true`
4. Render empty state if data arrays are empty
5. Display charts and tags with the received data

