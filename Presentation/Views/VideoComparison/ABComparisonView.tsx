import React, { useState, useEffect } from 'react';
import { Box, Slider, Typography, Paper, Grid } from '@mui/material';
import { styled } from '@mui/material/styles';

const VideoContainer = styled(Box)(({ theme }) => ({
  display: 'flex',
  flexDirection: 'row',
  gap: theme.spacing(2),
  width: '100%',
  height: 'calc(100vh - 100px)', // Full height minus some padding
  padding: theme.spacing(2),
}));

const VideoPlayer = styled(Box)({
  flex: 1,
  position: 'relative',
  aspectRatio: '16/9',
  '& video': {
    width: '100%',
    height: '100%',
    objectFit: 'contain',
  },
});

const ControlPanel = styled(Paper)(({ theme }) => ({
  position: 'fixed',
  right: theme.spacing(2),
  top: '50%',
  transform: 'translateY(-50%)',
  padding: theme.spacing(2),
  width: '300px',
  maxHeight: '80vh',
  overflowY: 'auto',
}));

interface Control {
  name: string;
  value: number;
  min: number;
  max: number;
  step: number;
}

interface ABComparisonViewProps {
  originalUrl: string;
  enhancedUrl: string;
  onChange: (controls: { [key: string]: number }) => void;
}

export const ABComparisonView: React.FC<ABComparisonViewProps> = ({
  originalUrl,
  enhancedUrl,
  onChange,
}) => {
  const [controls, setControls] = useState<Control[]>([
    { name: 'Noise Reduction', value: 65, min: 0, max: 100, step: 1 },
    { name: 'Bass Boost', value: 30, min: 0, max: 100, step: 1 },
    { name: 'Treble Boost', value: 40, min: 0, max: 100, step: 1 },
    // Add more  controls as needed
  ]);

  const handleControlChange = (index: number, newValue: number) => {
    const updatedControls = [...controls];
    updatedControls[index].value = newValue;
    setControls(updatedControls);

    // Convert controls to key-value pairs for the service
    const Values = controls.reduce((acc, control) => {
      acc[control.name] = control.value;
      return acc;
    }, {} as { [key: string]: number });

    onChange(Values);
  };

  return (
    <Box sx={{ position: 'relative', width: '100%', height: '100%' }}>
      <VideoContainer>
        <VideoPlayer>
          <Typography variant="subtitle1" align="center" gutterBottom>
            Original
          </Typography>
          <video
            src={originalUrl}
            controls
            autoPlay={false}
            loop
            style={{ backgroundColor: '#000' }}
          />
        </VideoPlayer>
        
        <VideoPlayer>
          <Typography variant="subtitle1" align="center" gutterBottom>
            Enhanced
          </Typography>
          <video
            src={enhancedUrl}
            controls
            autoPlay={false}
            loop
            style={{ backgroundColor: '#000' }}
          />
        </VideoPlayer>
      </VideoContainer>

      <ControlPanel elevation={3}>
        <Typography variant="h6" gutterBottom>
           Controls
        </Typography>
        {controls.map((control, index) => (
          <Box key={control.name} sx={{ my: 2 }}>
            <Typography gutterBottom>{control.name}</Typography>
            <Grid container spacing={2} alignItems="center">
              <Grid item xs>
                <Slider
                  value={control.value}
                  min={control.min}
                  max={control.max}
                  step={control.step}
                  onChange={(_, value) => handleControlChange(index, value as number)}
                  valueLabelDisplay="auto"
                />
              </Grid>
              <Grid item>
                <Typography variant="body2">{control.value}</Typography>
              </Grid>
            </Grid>
          </Box>
        ))}
      </ControlPanel>
    </Box>
  );
}; 