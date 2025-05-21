using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TrialWorld.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for MediaTimelineControl.xaml
    /// </summary>
    public partial class MediaTimelineControl : UserControl
    {
        #region Fields

        private readonly DispatcherTimer _tooltipTimer;
        private bool _isDraggingTimeline;
        private TimeSpan _mediaDuration = TimeSpan.Zero;
        private TimeSpan _currentPosition = TimeSpan.Zero;
        private double _pixelsPerSecond = 10.0;
        private List<TimelineMarker> _markers = new List<TimelineMarker>();
        private List<TimelineSegment> _segments = new List<TimelineSegment>();
        private ObservableCollection<FrameworkElement> _timeScaleElements = new ObservableCollection<FrameworkElement>();

        #endregion

        #region Events

        /// <summary>
        /// Event raised when the user seeks to a new position in the timeline
        /// </summary>
        public event EventHandler<TimelinePositionChangedEventArgs>? PositionChanged;

        /// <summary>
        /// Event raised when a marker is clicked
        /// </summary>
        public event EventHandler<TimelineMarkerEventArgs>? MarkerClicked;

        /// <summary>
        /// Event raised when a segment is clicked
        /// </summary>
        public event EventHandler<TimelineSegmentEventArgs>? SegmentClicked;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current position in the media timeline
        /// </summary>
        public TimeSpan CurrentPosition
        {
            get => _currentPosition;
            set
            {
                if (_currentPosition != value)
                {
                    _currentPosition = value;
                    UpdateCurrentPositionIndicator();
                }
            }
        }

        /// <summary>
        /// Gets or sets the total duration of the media
        /// </summary>
        public TimeSpan MediaDuration
        {
            get => _mediaDuration;
            set
            {
                if (_mediaDuration != value)
                {
                    _mediaDuration = value;
                    UpdateTimelineScale();
                    UpdateTimelineWidth();
                }
            }
        }

        /// <summary>
        /// Gets or sets the zoom level of the timeline (pixels per second)
        /// </summary>
        public double PixelsPerSecond
        {
            get => _pixelsPerSecond;
            set
            {
                if (_pixelsPerSecond != value && value >= 1.0 && value <= 100.0)
                {
                    _pixelsPerSecond = value;
                    UpdateTimelineWidth();
                    UpdateTimelineScale();
                    UpdateMarkers();
                    UpdateSegments();
                    UpdateCurrentPositionIndicator();
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTimelineControl"/> class
        /// </summary>
        public MediaTimelineControl()
        {
            InitializeComponent();

            // Initialize tooltip timer
            _tooltipTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.5)
            };
            _tooltipTimer.Tick += (s, e) =>
            {
                timeTooltip.Visibility = Visibility.Collapsed;
                _tooltipTimer.Stop();
            };

            // Setup event handlers
            Loaded += MediaTimelineControl_Loaded;
            SizeChanged += MediaTimelineControl_SizeChanged;

            // Setup timeline interaction
            timelineTrack.MouseDown += TimelineTrack_MouseDown;
            timelineTrack.MouseMove += TimelineTrack_MouseMove;
            timelineTrack.MouseUp += TimelineTrack_MouseUp;
            timelineTrack.MouseLeave += TimelineTrack_MouseLeave;

            segmentsCanvas.MouseDown += SegmentsCanvas_MouseDown;
            segmentsCanvas.MouseMove += SegmentsCanvas_MouseMove;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a marker to the timeline at the specified position
        /// </summary>
        /// <param name="position">Position of the marker in the timeline</param>
        /// <param name="label">Optional label for the marker</param>
        /// <param name="color">Color of the marker</param>
        /// <param name="data">Optional data associated with the marker</param>
        /// <returns>The created marker</returns>
        public TimelineMarker AddMarker(TimeSpan position, string? label = null, Color? color = null, object? data = null)
        {
            var marker = new TimelineMarker
            {
                Position = position,
                Label = label ?? position.ToString(@"hh\:mm\:ss"),
                Color = color ?? Colors.Red,
                Data = data ?? new object()
            };

            _markers.Add(marker);
            AddMarkerToCanvas(marker);
            return marker;
        }

        /// <summary>
        /// Removes a marker from the timeline
        /// </summary>
        /// <param name="marker">The marker to remove</param>
        /// <returns>True if the marker was removed, false otherwise</returns>
        public bool RemoveMarker(TimelineMarker marker)
        {
            var result = _markers.Remove(marker);
            if (result)
            {
                RemoveMarkerFromCanvas(marker);
            }
            return result;
        }

        /// <summary>
        /// Adds a segment to the timeline
        /// </summary>
        /// <param name="startPosition">Start position of the segment</param>
        /// <param name="endPosition">End position of the segment</param>
        /// <param name="label">Label for the segment</param>
        /// <param name="color">Color of the segment</param>
        /// <param name="data">Optional data associated with the segment</param>
        /// <returns>The created segment</returns>
        public TimelineSegment AddSegment(TimeSpan startPosition, TimeSpan endPosition, string? label, Color color, object? data = null)
        {
            var segment = new TimelineSegment
            {
                StartPosition = startPosition,
                EndPosition = endPosition,
                Label = label ?? startPosition.ToString(@"hh\:mm\:ss"),
                Color = color,
                Data = data ?? new object()
            };

            _segments.Add(segment);
            AddSegmentToCanvas(segment);
            return segment;
        }

        /// <summary>
        /// Removes a segment from the timeline
        /// </summary>
        /// <param name="segment">The segment to remove</param>
        /// <returns>True if the segment was removed, false otherwise</returns>
        public bool RemoveSegment(TimelineSegment segment)
        {
            var result = _segments.Remove(segment);
            if (result)
            {
                RemoveSegmentFromCanvas(segment);
            }
            return result;
        }

        /// <summary>
        /// Clears all markers from the timeline
        /// </summary>
        public void ClearMarkers()
        {
            _markers.Clear();
            markersCanvas.Children.Clear();
        }

        /// <summary>
        /// Clears all segments from the timeline
        /// </summary>
        public void ClearSegments()
        {
            _segments.Clear();
            segmentsCanvas.Children.Clear();
        }

        /// <summary>
        /// Zooms the timeline by the specified factor
        /// </summary>
        /// <param name="factor">Zoom factor</param>
        public void Zoom(double factor)
        {
            PixelsPerSecond = Math.Clamp(_pixelsPerSecond * factor, 1.0, 100.0);
        }

        /// <summary>
        /// Resets the zoom level to the default
        /// </summary>
        public void ResetZoom()
        {
            PixelsPerSecond = 10.0;
        }

        /// <summary>
        /// Scrolls the timeline to ensure the current position is visible
        /// </summary>
        public void ScrollToCurrentPosition()
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(this);
            if (scrollViewer != null)
            {
                double positionX = _currentPosition.TotalSeconds * _pixelsPerSecond;
                double viewportWidth = scrollViewer.ViewportWidth;

                // Calculate scroll position to center current position if possible
                double scrollTarget = Math.Max(0, positionX - (viewportWidth / 2));
                scrollViewer.ScrollToHorizontalOffset(scrollTarget);
            }
        }

        #endregion

        #region Private Methods

        private void MediaTimelineControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTimelineWidth();
            UpdateTimelineScale();
        }

        private void MediaTimelineControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTimelineScale();
        }

        private void TimelineTrack_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDraggingTimeline = true;
                timelineTrack.CaptureMouse();

                Point mousePos = e.GetPosition(timelineTrack);
                UpdatePositionFromMouseX(mousePos.X);
            }
        }

        private void TimelineTrack_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(timelineTrack);

            if (_isDraggingTimeline && e.LeftButton == MouseButtonState.Pressed)
            {
                UpdatePositionFromMouseX(mousePos.X);
            }
            else
            {
                // Show tooltip with time at mouse position
                TimeSpan hoverPosition = TimeSpan.FromSeconds(mousePos.X / _pixelsPerSecond);
                if (hoverPosition.TotalSeconds <= _mediaDuration.TotalSeconds)
                {
                    tooltipText.Text = hoverPosition.ToString(@"hh\:mm\:ss\.fff");
                    timeTooltip.Visibility = Visibility.Visible;

                    // Position tooltip above mouse
                    Canvas.SetLeft(timeTooltip, mousePos.X);
                    Canvas.SetTop(timeTooltip, mousePos.Y - timeTooltip.ActualHeight - 5);

                    _tooltipTimer.Stop();
                    _tooltipTimer.Start();
                }
            }
        }

        private void TimelineTrack_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingTimeline)
            {
                timelineTrack.ReleaseMouseCapture();
                _isDraggingTimeline = false;
            }
        }

        private void TimelineTrack_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isDraggingTimeline)
            {
                timelineTrack.ReleaseMouseCapture();
                _isDraggingTimeline = false;
            }

            timeTooltip.Visibility = Visibility.Collapsed;
            _tooltipTimer.Stop();
        }

        private void SegmentsCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null && element.Tag is TimelineSegment segment)
            {
                SegmentClicked?.Invoke(this, new TimelineSegmentEventArgs(segment));

                // Also update position to beginning of segment
                CurrentPosition = segment.StartPosition;
                PositionChanged?.Invoke(this, new TimelinePositionChangedEventArgs(CurrentPosition));
            }
            else
            {
                // Handle direct click on the segments canvas
                Point mousePos = e.GetPosition(segmentsCanvas);
                UpdatePositionFromMouseX(mousePos.X);
            }
        }

        private void SegmentsCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(segmentsCanvas);

            // Show tooltip with time at mouse position
            TimeSpan hoverPosition = TimeSpan.FromSeconds(mousePos.X / _pixelsPerSecond);
            if (hoverPosition.TotalSeconds <= _mediaDuration.TotalSeconds)
            {
                tooltipText.Text = hoverPosition.ToString(@"hh\:mm\:ss\.fff");
                timeTooltip.Visibility = Visibility.Visible;

                // Position tooltip above mouse
                Canvas.SetLeft(timeTooltip, mousePos.X);
                Canvas.SetTop(timeTooltip, mousePos.Y - timeTooltip.ActualHeight - 5);

                _tooltipTimer.Stop();
                _tooltipTimer.Start();
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Use mouse wheel with Ctrl key to zoom
                if (e.Delta > 0)
                {
                    Zoom(1.2);
                }
                else
                {
                    Zoom(0.8);
                }

                e.Handled = true;
            }
        }

        private void UpdateTimelineWidth()
        {
            double totalWidth = _mediaDuration.TotalSeconds * _pixelsPerSecond;
            timelineTrack.Width = Math.Max(totalWidth, ActualWidth);
            segmentsCanvas.Width = timelineTrack.Width;

            if (markersCanvas != null)
            {
                markersCanvas.Width = timelineTrack.Width;
            }
        }

        private void UpdateTimelineScale()
        {
            timelineScale.Children.Clear();

            if (_mediaDuration.TotalSeconds <= 0)
                return;

            double totalWidth = _mediaDuration.TotalSeconds * _pixelsPerSecond;
            double visibleWidth = Math.Max(ActualWidth, totalWidth);

            // Determine appropriate tick interval based on zoom level
            TimeSpan tickInterval = DetermineTickInterval();

            for (double seconds = 0; seconds <= _mediaDuration.TotalSeconds; seconds += tickInterval.TotalSeconds)
            {
                double xPos = seconds * _pixelsPerSecond;

                // Create tick line
                var tickLine = new Line
                {
                    X1 = xPos,
                    Y1 = 0,
                    X2 = xPos,
                    Y2 = 6,
                    Stroke = new SolidColorBrush(Color.FromArgb(153, 255, 255, 255)),
                    StrokeThickness = 1
                };
                timelineScale.Children.Add(tickLine);

                // Create tick label
                var tickTime = TimeSpan.FromSeconds(seconds);
                var tickLabel = new TextBlock
                {
                    Text = FormatTickLabel(tickTime),
                    Foreground = new SolidColorBrush(Color.FromArgb(170, 255, 255, 255)),
                    FontSize = 10
                };

                timelineScale.Children.Add(tickLabel);
                Canvas.SetLeft(tickLabel, xPos - (tickLabel.ActualWidth / 2));
                Canvas.SetTop(tickLabel, 8);
            }
        }

        private TimeSpan DetermineTickInterval()
        {
            // Choose an appropriate tick interval based on zoom level
            if (_pixelsPerSecond >= 50)
                return TimeSpan.FromSeconds(1); // 1 second
            else if (_pixelsPerSecond >= 20)
                return TimeSpan.FromSeconds(5); // 5 seconds
            else if (_pixelsPerSecond >= 10)
                return TimeSpan.FromSeconds(10); // 10 seconds
            else if (_pixelsPerSecond >= 5)
                return TimeSpan.FromSeconds(30); // 30 seconds
            else if (_pixelsPerSecond >= 2)
                return TimeSpan.FromMinutes(1); // 1 minute
            else
                return TimeSpan.FromMinutes(5); // 5 minutes
        }

        private string FormatTickLabel(TimeSpan time)
        {
            if (time.TotalHours >= 1)
                return time.ToString(@"h\:mm\:ss");
            else
                return time.ToString(@"m\:ss");
        }

        private void UpdateCurrentPositionIndicator()
        {
            double positionX = _currentPosition.TotalSeconds * _pixelsPerSecond;
            Canvas.SetLeft(currentPositionIndicator, positionX);
        }

        private void AddMarkerToCanvas(TimelineMarker marker)
        {
            // Create marker visual elements
            var markerControl = new ContentControl
            {
                Template = (ControlTemplate)FindResource("TimelineMarkerTemplate"),
                Tag = marker
            };

            // Position the marker
            double markerX = marker.Position.TotalSeconds * _pixelsPerSecond;
            Canvas.SetLeft(markerControl, markerX);
            Canvas.SetTop(markerControl, 0);

            // Set marker color
            var markerVisual = markerControl.Template.LoadContent() as Grid;
            if (markerVisual != null)
            {
                foreach (var child in markerVisual.Children)
                {
                    if (child is Shape shape)
                    {
                        shape.Fill = new SolidColorBrush(marker.Color);
                        shape.Stroke = new SolidColorBrush(marker.Color);
                    }
                }
            }

            // Add event handler for clicking marker
            markerControl.MouseDown += (s, e) =>
            {
                MarkerClicked?.Invoke(this, new TimelineMarkerEventArgs(marker));
                e.Handled = true;

                // Update position to marker
                CurrentPosition = marker.Position;
                PositionChanged?.Invoke(this, new TimelinePositionChangedEventArgs(CurrentPosition));
            };

            // Add tooltip for marker
            ToolTipService.SetToolTip(markerControl, marker.Label);

            // Add to canvas
            marker.Visual = markerControl;
            markersCanvas.Children.Add(markerControl);
        }

        private void RemoveMarkerFromCanvas(TimelineMarker marker)
        {
            if (marker.Visual != null && markersCanvas.Children.Contains(marker.Visual))
            {
                markersCanvas.Children.Remove(marker.Visual);
                marker.Visual = null;
            }
        }

        private void AddSegmentToCanvas(TimelineSegment segment)
        {
            // Calculate segment position and size
            double startX = segment.StartPosition.TotalSeconds * _pixelsPerSecond;
            double endX = segment.EndPosition.TotalSeconds * _pixelsPerSecond;
            double width = endX - startX;

            // Create segment rectangle
            var segmentRect = new Border
            {
                Width = Math.Max(1, width),
                Height = 20,
                Background = new SolidColorBrush(Color.FromArgb(150, segment.Color.R, segment.Color.G, segment.Color.B)),
                BorderBrush = new SolidColorBrush(segment.Color),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2),
                Tag = segment
            };

            Canvas.SetLeft(segmentRect, startX);
            Canvas.SetTop(segmentRect, 5);

            // Create segment label
            var segmentLabel = new TextBlock
            {
                Text = segment.Label,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 11,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(4, 0, 4, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Tag = segment
            };

            // Add label to segment
            segmentRect.Child = segmentLabel;

            // Add event handler for clicking segment
            segmentRect.MouseDown += (s, e) =>
            {
                SegmentClicked?.Invoke(this, new TimelineSegmentEventArgs(segment));
                e.Handled = true;

                // Update position to start of segment
                CurrentPosition = segment.StartPosition;
                PositionChanged?.Invoke(this, new TimelinePositionChangedEventArgs(CurrentPosition));
            };

            // Add segment to canvas
            segment.Visual = segmentRect;
            segmentsCanvas.Children.Add(segmentRect);
        }

        private void RemoveSegmentFromCanvas(TimelineSegment segment)
        {
            if (segment.Visual != null && segmentsCanvas.Children.Contains(segment.Visual))
            {
                segmentsCanvas.Children.Remove(segment.Visual);
                segment.Visual = null;
            }
        }

        private void UpdateMarkers()
        {
            // Update position of all markers
            foreach (var marker in _markers)
            {
                if (marker.Visual is FrameworkElement element)
                {
                    double markerX = marker.Position.TotalSeconds * _pixelsPerSecond;
                    Canvas.SetLeft(element, markerX);
                }
            }
        }

        private void UpdateSegments()
        {
            // Update position and size of all segments
            foreach (var segment in _segments)
            {
                if (segment.Visual is FrameworkElement element)
                {
                    double startX = segment.StartPosition.TotalSeconds * _pixelsPerSecond;
                    double endX = segment.EndPosition.TotalSeconds * _pixelsPerSecond;
                    double width = endX - startX;

                    Canvas.SetLeft(element, startX);
                    element.Width = Math.Max(1, width);
                }
            }
        }

        private void UpdatePositionFromMouseX(double mouseX)
        {
            // Calculate time from mouse X position
            double seconds = mouseX / _pixelsPerSecond;

            // Clamp to valid range
            seconds = Math.Max(0, Math.Min(seconds, _mediaDuration.TotalSeconds));

            // Update position
            CurrentPosition = TimeSpan.FromSeconds(seconds);

            // Raise event
            PositionChanged?.Invoke(this, new TimelinePositionChangedEventArgs(CurrentPosition));
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }
                else
                {
                    var result = FindVisualChild<T>(child);
                    if (result != null)
                        return result;
                }
            }
            return null!;
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Represents a marker on the timeline
    /// </summary>
    public class TimelineMarker
    {
        /// <summary>
        /// Gets or sets the position of the marker in the timeline
        /// </summary>
        public TimeSpan Position { get; set; }

        /// <summary>
        /// Gets or sets the label for the marker
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the color of the marker
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets custom data associated with this marker
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Gets or sets the visual element representing this marker
        /// </summary>
        internal FrameworkElement? Visual { get; set; }
    }

    /// <summary>
    /// Represents a segment on the timeline
    /// </summary>
    public class TimelineSegment
    {
        /// <summary>
        /// Gets or sets the start position of the segment
        /// </summary>
        public TimeSpan StartPosition { get; set; }

        /// <summary>
        /// Gets or sets the end position of the segment
        /// </summary>
        public TimeSpan EndPosition { get; set; }

        /// <summary>
        /// Gets or sets the label for the segment
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the color of the segment
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets custom data associated with this segment
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Gets or sets the visual element representing this segment
        /// </summary>
        internal FrameworkElement? Visual { get; set; }

        /// <summary>
        /// Gets the duration of the segment
        /// </summary>
        public TimeSpan Duration => EndPosition - StartPosition;
    }

    /// <summary>
    /// Event arguments for timeline position changes
    /// </summary>
    public class TimelinePositionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the new position in the timeline
        /// </summary>
        public TimeSpan Position { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelinePositionChangedEventArgs"/> class
        /// </summary>
        /// <param name="position">The new position</param>
        public TimelinePositionChangedEventArgs(TimeSpan position)
        {
            Position = position;
        }
    }

    /// <summary>
    /// Event arguments for timeline marker events
    /// </summary>
    public class TimelineMarkerEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the marker associated with the event
        /// </summary>
        public TimelineMarker Marker { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineMarkerEventArgs"/> class
        /// </summary>
        /// <param name="marker">The marker</param>
        public TimelineMarkerEventArgs(TimelineMarker marker)
        {
            Marker = marker;
        }
    }

    /// <summary>
    /// Event arguments for timeline segment events
    /// </summary>
    public class TimelineSegmentEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the segment associated with the event
        /// </summary>
        public TimelineSegment Segment { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineSegmentEventArgs"/> class
        /// </summary>
        /// <param name="segment">The segment</param>
        public TimelineSegmentEventArgs(TimelineSegment segment)
        {
            Segment = segment;
        }
    }

    #endregion
}