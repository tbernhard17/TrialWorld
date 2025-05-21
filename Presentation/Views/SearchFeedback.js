import React, { useState } from 'react';
import { Rating, TextField, Button, Chip, Box, Typography, Snackbar, Alert } from '@mui/material';
import { ThumbUp, ThumbDown, Add } from '@mui/icons-material';

/**
 * Component for collecting user feedback on search results
 */
const SearchFeedbackForm = ({ resultId, onSubmit }) => {
  const [rating, setRating] = useState(3);
  const [wasSelected, setWasSelected] = useState(true);
  const [comment, setComment] = useState('');
  const [tags, setTags] = useState([]);
  const [currentTag, setCurrentTag] = useState('');
  const [snackbarOpen, setSnackbarOpen] = useState(false);
  const [snackbarMessage, setSnackbarMessage] = useState('');
  const [snackbarSeverity, setSnackbarSeverity] = useState('success');

  const handleAddTag = () => {
    if (currentTag.trim() !== '' && !tags.includes(currentTag.trim())) {
      setTags([...tags, currentTag.trim()]);
      setCurrentTag('');
    }
  };

  const handleRemoveTag = (tagToRemove) => {
    setTags(tags.filter(tag => tag !== tagToRemove));
  };

  const handleSubmit = async () => {
    try {
      const feedbackData = {
        resultId,
        relevanceRating: rating / 5.0, // Convert to 0-1 scale
        wasSelected,
        comment,
        userTags: tags
      };

      // Send feedback to API
      const response = await fetch('/api/feedback/searchresult', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(feedbackData)
      });

      if (response.ok) {
        setSnackbarMessage('Thank you for your feedback!');
        setSnackbarSeverity('success');
        setSnackbarOpen(true);
        
        // Optional: Reset form
        setRating(3);
        setWasSelected(true);
        setComment('');
        setTags([]);
        
        // Call external onSubmit handler if provided
        if (onSubmit) {
          onSubmit(feedbackData);
        }
      } else {
        setSnackbarMessage('Failed to submit feedback. Please try again.');
        setSnackbarSeverity('error');
        setSnackbarOpen(true);
      }
    } catch (error) {
      console.error('Error submitting feedback:', error);
      setSnackbarMessage('An error occurred while submitting feedback.');
      setSnackbarSeverity('error');
      setSnackbarOpen(true);
    }
  };

  return (
    <Box sx={{ p: 2, border: '1px solid #e0e0e0', borderRadius: 2, mb: 2 }}>
      <Typography variant="h6" gutterBottom>
        How helpful was this result?
      </Typography>
      
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
        <Rating
          name="relevance-rating"
          value={rating}
          onChange={(_, newValue) => {
            setRating(newValue || 0);
          }}
        />
        <Typography variant="body2" sx={{ ml: 1 }}>
          ({rating}/5)
        </Typography>
      </Box>

      <Box sx={{ display: 'flex', mb: 2 }}>
        <Button 
          variant={wasSelected ? "contained" : "outlined"} 
          color="primary"
          startIcon={<ThumbUp />}
          onClick={() => setWasSelected(true)}
          sx={{ mr: 1 }}
        >
          Helpful
        </Button>
        <Button 
          variant={!wasSelected ? "contained" : "outlined"} 
          color="secondary"
          startIcon={<ThumbDown />}
          onClick={() => setWasSelected(false)}
        >
          Not Helpful
        </Button>
      </Box>

      <TextField
        label="Comments"
        multiline
        rows={3}
        value={comment}
        onChange={(e) => setComment(e.target.value)}
        fullWidth
        margin="normal"
        placeholder="Tell us why this result was or wasn't helpful..."
      />

      <Box sx={{ mt: 2, mb: 1 }}>
        <Typography variant="body2" gutterBottom>
          Add tags to describe this result:
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <TextField
            value={currentTag}
            onChange={(e) => setCurrentTag(e.target.value)}
            onKeyPress={(e) => {
              if (e.key === 'Enter') {
                e.preventDefault();
                handleAddTag();
              }
            }}
            size="small"
            placeholder="Add a tag"
            sx={{ mr: 1 }}
          />
          <Button
            variant="outlined"
            startIcon={<Add />}
            onClick={handleAddTag}
          >
            Add
          </Button>
        </Box>
      </Box>

      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, my: 2 }}>
        {tags.map((tag) => (
          <Chip
            key={tag}
            label={tag}
            onDelete={() => handleRemoveTag(tag)}
          />
        ))}
      </Box>

      <Button 
        variant="contained" 
        color="primary" 
        onClick={handleSubmit}
        fullWidth
      >
        Submit Feedback
      </Button>

      <Snackbar
        open={snackbarOpen}
        autoHideDuration={6000}
        onClose={() => setSnackbarOpen(false)}
      >
        <Alert 
          onClose={() => setSnackbarOpen(false)} 
          severity={snackbarSeverity}
        >
          {snackbarMessage}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default SearchFeedbackForm; 