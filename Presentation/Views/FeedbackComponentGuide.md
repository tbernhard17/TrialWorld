# Search Feedback Component Integration Guide

This guide explains how to integrate the search feedback component into your search results pages.

## Overview

The search feedback system allows users to provide feedback on search results, which helps train the AI learning engine to improve future search rankings. The system consists of:

1. A backend API for submitting feedback and retrieving model status
2. A React component for collecting user feedback

## Backend API Endpoints

The feedback API provides two endpoints:

### Submit Feedback

```
POST /api/feedback/searchresult
```

Request body:
```json
{
  "resultId": "string",
  "relevanceRating": 0.0-1.0,
  "wasSelected": boolean,
  "comment": "string",
  "userTags": ["string"]
}
```

Response:
```json
{
  "success": true,
  "message": "Feedback recorded successfully"
}
```

### Get Model Status

```
GET /api/feedback/model/status
```

Response:
```json
{
  "modelActive": true,
  "feedbackCount": 123,
  "lastUpdated": "2023-05-01T12:34:56Z"
}
```

## Frontend Component Usage

### Installation

Make sure you have the required dependencies:

```bash
npm install @mui/material @mui/icons-material
```

### Basic Usage

Import the component:

```jsx
import SearchFeedbackForm from './SearchFeedback';
```

Use it in your search results component:

```jsx
function SearchResultItem({ result }) {
  const handleFeedbackSubmitted = (feedbackData) => {
    console.log('Feedback submitted:', feedbackData);
    // Update UI or take additional actions after submission
  };

  return (
    <div className="search-result-item">
      <h3>{result.title}</h3>
      <p>{result.snippet}</p>
      
      {/* Only show feedback form when user expands it */}
      <Accordion>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography>Give Feedback</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <SearchFeedbackForm 
            resultId={result.id} 
            onSubmit={handleFeedbackSubmitted} 
          />
        </AccordionDetails>
      </Accordion>
    </div>
  );
}
```

### Props

The `SearchFeedbackForm` component accepts the following props:

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| resultId | string | Yes | The ID of the search result being rated |
| onSubmit | function | No | Callback function called after successful submission |

### Customization

You can customize the appearance of the feedback form by wrapping it and applying your own styles:

```jsx
function CustomizedFeedbackForm({ resultId }) {
  return (
    <div className="custom-feedback-container">
      <h4 className="custom-header">We value your opinion!</h4>
      <SearchFeedbackForm resultId={resultId} />
    </div>
  );
}
```

## Testing

You can test the API endpoints using the provided HTTP test file located at:
`WebApi/Controllers/FeedbackControllerTests.http`

## Model Dashboard

In addition to collecting feedback, the system includes a dashboard for monitoring the AI model's performance over time.

### Dashboard Overview

The dashboard displays key metrics about the AI model, including:

- Total feedback count
- Model status (active/inactive)
- Model accuracy
- Performance history over time
- Recent feedback submissions

### Using the Dashboard Component

Import the dashboard component:

```jsx
import ModelStatsDashboard from './ModelStatsDashboard';
```

Then use it in your admin or monitoring pages:

```jsx
function ModelMonitoringPage() {
  return (
    <div className="admin-page">
      <h1>AI Model Monitoring</h1>
      <ModelStatsDashboard />
    </div>
  );
}
```

### Dashboard API Endpoints

The dashboard uses the following API endpoints:

- `GET /api/feedback/model/status` - Get current model statistics
- `GET /api/feedback/recent?limit=10` - Get recent feedback submissions
- `GET /api/feedback/performance-history?days=30` - Get historical performance data

### Required Dependencies

The dashboard component requires additional dependencies:

```bash
npm install @mui/material @mui/icons-material @mui/x-data-grid recharts
```

## Troubleshooting

- If the feedback submission is failing, check the browser console for error messages
- Ensure the backend API is running and accessible
- Verify the correct result ID is being passed to the component 