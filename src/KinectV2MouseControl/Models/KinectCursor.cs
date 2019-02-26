using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using KinectV2MouseControl.Models;
using Microsoft.Kinect;

namespace KinectV2MouseControl
{
    public class KinectCursor
    {
        private KinectReader sensorReader;
        private CursorMapper cursorMapper;

        private readonly bool NeedGrabbing = true;

        public enum ControlMode
        {
            Disabled = 0,
            MoveOnly,
            GripToPress,
            HoverToClick,
            MoveGripPressing,
            MoveLiftClicking
        }

        private ControlMode _mode = ControlMode.Disabled;
        public ControlMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                if (value == ControlMode.Disabled)
                {
                    ToggleHoverTimer(false);
                    sensorReader.Close();
                }
                else
                {
                    sensorReader.Open();
                }
            }
        }

        public double Smoothing
        {
            get
            {
                return cursorMapper.Smoothing;
            }
            set
            {
                cursorMapper.Smoothing = value;
            }
        }

        /// <summary>
        /// Rect value worked out by pointing to left, top, right, bottom spot with my hand as the ideal edges, and noting down the x, y values got from GetHandRelativePosition.
        /// This may only fit more for me, and you can test out your rect value. Approximately it works fine for most people.
        /// </summary>
        //private MRect gestureRect = new MRect(-0.18, 1.65, 0.18, -1.65); //// Theirs 
        // Decrease Top = Better Grip Recognition up top
        // Increase Right - Worse recognition right
        private MRect gestureRect = new MRect(-0.1, 1.2, 0.15, -1.6);  // Left Top Right Bottom 

        private bool[] handGrips = new bool[2] { false, false };

        private double _hoverDuration = 2;

        /// <summary>
        /// Wait time for a hover gesture.
        /// </summary>
        public double HoverDuration {
            get
            {
                return _hoverDuration;
            }
            set
            {
                _hoverDuration = value;
                hoverTimer.Interval = TimeSpan.FromSeconds(value);
            }
        }

        /// <summary>
        /// Hand moves further than distance will cause a hover fail.
        /// Default as 20, this needs to be modified according to usual movement distance/speed in your specific case.
        /// </summary>
        public double HoverRange { get; set; } = 20;
        
        public double MoveScale
        {
            get
            {
                return cursorMapper.MoveScale;
            }
            set
            {
                cursorMapper.MoveScale = value;
            }
        }

        public double HandLiftYForClick { get; set; } = 0.02f;

        private MVector2 lastCursorPos = MVector2.Zero;

        /// <summary>
        /// Timer for hover detection.
        /// </summary>
        private DispatcherTimer hoverTimer = new DispatcherTimer();

        private const int NONE_USED = -1;

        /// <summary>
        /// Used to keep track of the controlling hand. So when another hand lift up forward, the cursor would still follow the first controlling hand.
        /// </summary>
        private int usedHandIndex = NONE_USED;
        private bool hoverClicked = false;

        public event EventHandler<Data> PositionDataUpdated;

        public KinectCursor()
        {
            MRect screenRect = new MRect(0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            cursorMapper = new CursorMapper(gestureRect, screenRect, CursorMapper.ScaleAlignment.LongerRange);

            sensorReader = new KinectReader(false);
            sensorReader.OnTrackedBody += Kinect_OnTrackedBody;
            sensorReader.OnLostTracking += Kinect_OnLostTracking;

            hoverTimer.Interval = TimeSpan.FromSeconds(HoverDuration);
            hoverTimer.Tick += new EventHandler(HoverTimer_Tick);

            DataCollector = DataCollectorFactory.Start();
            
        }

        public void StopData()
        {
            DataCollector.Stop();
        }

        private void Kinect_OnLostTracking(object sender, EventArgs e)
        {
            ToggleHoverTimer(false);
            ReleaseGrip(0);
            ReleaseGrip(1);
            usedHandIndex = NONE_USED;
        }

        private void Kinect_OnTrackedBody(object sender, BodyEventArgs e)
        {
            Body body = e.BodyData;

            if (Mode == ControlMode.Disabled)
            {
                return;
            }
            
            for(int i = 1; i >= 0; i--) // Starts looking from right hand.
            {
                bool isLeft = i == 0;
                if (body.IsHandLiftForward(isLeft))
                {
                    if (usedHandIndex == -1)
                    {
                        usedHandIndex = i;
                    } else if (usedHandIndex != i)
                    {
                        // In two-hand control mode, non-used hand would be used for pressing/releasing mouse button.
                        if (Mode == ControlMode.MoveGripPressing)
                        {
                            DoMouseControlByHandState(i, body.GetHandState(isLeft));
                        } 

                        continue;
                    }

                    MVector2 handPos = body.GetHandRelativePosition(isLeft);
                    MVector2 targetPos = cursorMapper.GetSmoothedOutputPosition(handPos);

                    MouseControl.MoveTo(targetPos.X, targetPos.Y);

                    if (Mode == ControlMode.GripToPress)
                    {
                        MouseControlState state = DoMouseControlByHandState(i, body.GetHandState(isLeft));
                        Data d = new Data(targetPos.X, targetPos.Y, state);
                        DataCollector.CollectData(d);
                        PositionDataUpdated?.Invoke(this, d);
                    }
                    else if (Mode == ControlMode.HoverToClick)
                    {
                        if ((targetPos - lastCursorPos).Length() > HoverRange)
                        {
                            ToggleHoverTimer(false);
                            hoverClicked = false;
                        }

                        lastCursorPos = targetPos;
                    }
                }
                else
                {
                    if(usedHandIndex == i)
                    {
                        // Reset to none.
                        usedHandIndex = NONE_USED;
                        ReleaseGrip(i);
                    }
                    else  if (Mode == ControlMode.MoveLiftClicking)
                    {
                        DoMouseClickByHandLifting(i, body.GetHandRelativePosition(isLeft));
                        //System.Diagnostics.Trace.WriteLine(body.GetHandRelativePosition(isLeft).Y);
                    }
                    else // Release mouse button when it's not regularly released, such as hand tracking lost.
                    {
                        ReleaseGrip(i);
                    }
                    
                }
                
            }

            ToggleHoverTimer(Mode == ControlMode.HoverToClick && usedHandIndex != -1);
        }
        private int NumberTimesClosed = 0;
        private DataCollector DataCollector;

        private MouseControlState OnlyClick()
        {
            MouseControlState controlState;
            if (NumberTimesClosed == 0)
            {
                controlState = MouseControlState.ShouldClick;
                System.Diagnostics.Debug.WriteLine("Hand Closed for First Time");
            }
            else if (NumberTimesClosed > 8)
            {
                controlState = MouseControlState.ShouldPress;
                System.Diagnostics.Debug.WriteLine("Hand Closed 8 Times. Pressing");
            }
            else
            {
                controlState = MouseControlState.None;
            }
            System.Diagnostics.Debug.WriteLine("Hand Closed " + NumberTimesClosed);
            //} else
            //{
            //    controlState = MouseControlState.None;
            //}
            NumberTimesClosed++;
            // Doesn't Appear to Work
            return controlState;
        }

        private MouseControlState DoMouseControlByHandState(int handIndex, HandState handState)
        {
            MouseControlState controlState; 
            switch (handState)
            {
                case HandState.Closed:
                    //controlState = OnlyClick();
                    NumberTimesClosed++;
                    System.Diagnostics.Debug.WriteLine($"Closed : {NumberTimesClosed}");
                    controlState = MouseControlState.ShouldPress;
                    break;
                case HandState.Open:
                    if (NumberTimesClosed > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Hand Open");
                        NumberTimesClosed = 0;
                    }
                    controlState = MouseControlState.ShouldRelease;
                    break;
                default:
                    controlState = MouseControlState.None;
                    break;
            }
            return UpdateHandMouseControl(handIndex, controlState);
        }

        private void DoMouseClickByHandLifting(int handIndex, MVector2 handRelativePos)
        {
            UpdateHandMouseControl(handIndex, handRelativePos.Y > HandLiftYForClick ? MouseControlState.ShouldClick : MouseControlState.ShouldRelease);
            
            //DoMouseControlByHandLifting(with press and releas rather than just a click):
            //MouseControlState controlState = handRelativePos.Y > HandLiftYForClick ? MouseControlState.ShouldPress : MouseControlState.ShouldRelease;
            //UpdateHandMouseControl(handIndex, controlState);
        }

        public enum MouseControlState
        {
            None,
            ShouldPress,
            ShouldRelease,
            ShouldClick,
            Clicked,
            PressedDown,
            PressedUp
        }

        private MouseControlState UpdateHandMouseControl(int handIndex, MouseControlState controlState)
        {
            if (controlState == MouseControlState.ShouldClick)
            {
                if (!handGrips[handIndex])
                {
                    MouseControl.Click();
                    handGrips[handIndex] = true;
                    return MouseControlState.Clicked;
                }
            }else if (controlState == MouseControlState.ShouldPress)
            {
                if (!handGrips[handIndex])
                {
                    MouseControl.PressDown();
                    handGrips[handIndex] = true;
                    return MouseControlState.PressedDown;
                }
            }
            else if (controlState == MouseControlState.ShouldRelease && handGrips[handIndex])
            {
                ReleaseGrip(handIndex);
                return MouseControlState.PressedUp;
            }
            return MouseControlState.None;
        }

        private void ReleaseGrip(int index)
        {
            if (!NeedGrabbing)
            {
                MouseControl.PressUp(); // Here for ability to click on maximise/minimise but not drag
            }
            if (handGrips[index])
            {
                if (NeedGrabbing) MouseControl.PressUp(); // Here for dragging but no max/min
                handGrips[index] = false;
            }
        }

        private void ToggleHoverTimer(bool isOn)
        {
            if(hoverTimer.IsEnabled != isOn)
            {
                if (isOn)
                {
                    hoverTimer.Start();
                }
                else
                {
                    hoverTimer.Stop();
                }
            }
        }

        void HoverTimer_Tick(object sender, EventArgs e)
        {
            if (!hoverClicked)
            {
                MouseControl.Click();
                hoverTimer.Stop();
                hoverClicked = true;
            }
        }

    }
    
}
