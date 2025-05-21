import React, { useState, useEffect } from 'react';
import { 
  Box, 
  Card, 
  CardContent, 
  Typography, 
  Grid, 
  CircularProgress, 
  LinearProgress,
  Divider,
  Button,
  Alert,
  Paper,
  Chip
} from '@mui/material';
import { 
  DataGrid 
} from '@mui/x-data-grid';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer
} from 'recharts';

/**
 * Dashboard component for monitoring AI model performance
 */
const ModelStatsDashboard = () => {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [recentFeedback, setRecentFeedback] = useState([]);
  const [performanceHistory, setPerformanceHistory] = useState([]);
  
  const fetchModelStats = async () => {
    try {
      setLoading(true);
      const response = await fetch('/api/feedback/model/status');
      
      if (!response.ok) {
        throw new Error('Failed to fetch model statistics');
      }
      
      const data = await response.json();
      setStats(data);
      
      // Fetch recent feedback data
      const feedbackResponse = await fetch('/api/feedback/recent?limit=10');
      if (feedbackResponse.ok) {
        const feedbackData = await feedbackResponse.json();
        setRecentFeedback(feedbackData);
      }
      
      // Fetch performance history data
      const historyResponse = await fetch('/api/feedback/performance-history?days=30');
      if (historyResponse.ok) {
        const historyData = await historyResponse.json();
        setPerformanceHistory(historyData);
      }
      
      setError(null);
    } catch (err) {
      setError(err.message);
      console.error('Error fetching model stats:', err);
    } finally {
      setLoading(false);
    }
  };
  
  useEffect(() => {
    fetchModelStats();
    
    // Poll for updates every 30 seconds
    const intervalId = setInterval(fetchModelStats, 30000);
    
    return () => clearInterval(intervalId);
  }, []);
  
  const feedbackColumns = [
    { field: 'resultId', headerName: 'Result ID', width: 150 },
    { 
      field: 'rating', 
      headerName: 'Rating', 
      width: 100,
      renderCell: (params) => (
        <Box sx={{ width: '100%' }}>
          <LinearProgress 
            variant="determinate" 
            value={params.value * 100} 
            color={params.value > 0.7 ? "success" : params.value > 0.4 ? "warning" : "error"} 
            sx={{ height: 10, borderRadius: 5 }}
          />
          <Typography variant="caption">{Math.round(params.value * 100)}%</Typography>
        </Box>
      )
    },
    { 
      field: 'date', 
      headerName: 'Date', 
      width: 200,
      valueFormatter: (params) => new Date(params.value).toLocaleString()
    },
    { 
      field: 'tags', 
      headerName: 'Tags', 
      width: 300,
      renderCell: (params) => (
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
          {params.value.map((tag) => (
            <Chip key={tag} label={tag} size="small" />
          ))}
        </Box>
      )
    },
  ];
  
  if (loading && !stats) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
        <Typography variant="h6" sx={{ ml: 2 }}>Loading model statistics...</Typography>
      </Box>
    );
  }
  
  if (error) {
    return (
      <Alert severity="error" sx={{ m: 2 }}>
        Error loading model statistics: {error}
        <Button variant="outlined" size="small" sx={{ ml: 2 }} onClick={fetchModelStats}>
          Retry
        </Button>
      </Alert>
    );
  }
  
  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        AI Learning Engine Dashboard
      </Typography>
      
      <Grid container spacing={3}>
        {/* Stats Cards */}
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Feedback Count
              </Typography>
              <Typography variant="h3">
                {stats?.feedbackCount || 0}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Total feedback entries used for training
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Model Status
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                <Chip 
                  label={stats?.modelActive ? "Active" : "Inactive"} 
                  color={stats?.modelActive ? "success" : "error"} 
                  sx={{ mr: 1 }}
                />
                <Typography variant="h5">
                  v1.0.0
                </Typography>
              </Box>
              <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
                Last Updated: {new Date(stats?.lastUpdated).toLocaleString()}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Model Accuracy
              </Typography>
              <Box sx={{ position: 'relative', display: 'inline-flex' }}>
                <CircularProgress 
                  variant="determinate" 
                  value={stats?.accuracy ? stats.accuracy * 100 : 0} 
                  size={80} 
                  thickness={4} 
                  color="success" 
                />
                <Box
                  sx={{
                    top: 0,
                    left: 0,
                    bottom: 0,
                    right: 0,
                    position: 'absolute',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  <Typography variant="h6" component="div" color="text.secondary">
                    {stats?.accuracy ? Math.round(stats.accuracy * 100) : 0}%
                  </Typography>
                </Box>
              </Box>
              <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
                Based on cross-validation testing
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        
        {/* Performance Chart */}
        <Grid item xs={12}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Performance History
            </Typography>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart
                data={performanceHistory}
                margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="date" />
                <YAxis yAxisId="left" domain={[0.5, 1]} />
                <YAxis yAxisId="right" orientation="right" domain={[0, 100]} />
                <Tooltip />
                <Legend />
                <Line 
                  yAxisId="left"
                  type="monotone" 
                  dataKey="accuracy" 
                  stroke="#8884d8" 
                  name="Accuracy"
                  activeDot={{ r: 8 }} 
                />
                <Line 
                  yAxisId="right"
                  type="monotone" 
                  dataKey="feedbackCount" 
                  stroke="#82ca9d" 
                  name="Feedback Count"
                />
              </LineChart>
            </ResponsiveContainer>
          </Paper>
        </Grid>
        
        {/* Recent Feedback */}
        <Grid item xs={12}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Recent Feedback
            </Typography>
            <Box sx={{ height: 400, width: '100%' }}>
              <DataGrid
                rows={recentFeedback}
                columns={feedbackColumns}
                pageSize={5}
                rowsPerPageOptions={[5]}
                disableSelectionOnClick
              />
            </Box>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default ModelStatsDashboard; 