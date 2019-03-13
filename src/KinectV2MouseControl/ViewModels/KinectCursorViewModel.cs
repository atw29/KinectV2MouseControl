using KinectV2MouseControl.Models;
using KinectV2MouseControl.Views.Tasks;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ControlMode = KinectV2MouseControl.KinectCursor.ControlMode;

namespace KinectV2MouseControl
{
    public class KinectCursorViewModel : INotifyPropertyChanged
    {

        #region Their Code

        KinectCursor kinectCursor;

        /// PUT USER'S NAME HERE
        const string USER = "ALEX_TEST";

        // CHANGE THE TASK NUMBER 
        const int TASK_NUM = 2;

        internal void Create_Task()
        {
            Run_Task(USER, TASK_NUM);
        }

        private int windowcount;

        private void Create_Window(Window w, bool maximised = false)
        {
            w.Title = w.Title + " " + windowcount;
            if (maximised) w.WindowState = WindowState.Maximized;
            w.Show();
            windowcount++;
        }

        private void Run_Task(string USER, int TASK_NUM)
        {
            windowcount = 0;
            new Background().Show();

            Create_Window(new Menu_Task());
            
            kinectCursor.NeedGrabbing = TASK_NUM != 3;
            
            switch (TASK_NUM)
            {
                case 0: // Testing
                    break;
                case 1:
                    break;

                case 2: // Switching
                    Create_Window( new MockUp(), true);
                    
                    Create_Window( new X_Rays(), true);
                    Create_Window( new X_Rays(), true);
                    break;

                case 3: // Window Actions
                    Create_Window(new X_Rays());
                    break;

                case 4: // Snapping
                    Create_Window(new MockUp());
                    break;

                case 5: // 2D Manipulation
                    Create_Window(new Lighting_Control());
                    break;

                case 6: // Scrolling
                    Create_Window(new MockUp());
                    break;

                case 7: // Pan and Zoom
                    Create_Window(new X_Rays());
                    break;

                default:
                    throw new Exception("No Task for number " + TASK_NUM);

            }
        }

        const double DEFAULT_MOVE_SCALE = 1f;
        const double DEFAULT_SMOOTHING = 0.2f;
        const double DEFAULT_HOVER_RANGE = 20f;
        const double DEFAULT_HOVER_DURATION = 2;

        public event EventHandler<Data> WriteData;

        internal void StopData()
        {
            kinectCursor.StopData();   
        }

        public static KinectCursorViewModel Instance { get; private set; }
        static KinectCursorViewModel()
        {
            Instance = new KinectCursorViewModel();
        }

        public KinectCursorViewModel()
        {
            kinectCursor = new KinectCursor(USER, TASK_NUM);
            kinectCursor.PositionDataUpdated += KinectCursor_PositionDataUpdated;
        }

        private void KinectCursor_PositionDataUpdated(object sender, Data e)
        {
            WriteData?.Invoke(sender, e);
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region UI Comps

        private string pos;
        public string Pos
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
                RaisePropertyChanged("pos");
            }
        }

        public double MoveScale
        {
            get
            {
                return kinectCursor.MoveScale;
            }
            set
            {
                kinectCursor.MoveScale = value;
                RaisePropertyChanged();
            }
        }

        public double Smoothing
        {
            get
            {
                return kinectCursor.Smoothing;
            }
            set
            {
                kinectCursor.Smoothing = value;
                RaisePropertyChanged();
            }
        }

        public double HoverRange
        {
            get
            {
                return kinectCursor.HoverRange;
            }
            set
            {
                kinectCursor.HoverRange = value;
                RaisePropertyChanged();
            }
        }

        public double HoverDuration
        {
            get
            {
                return kinectCursor.HoverDuration;
            }
            set
            {
                kinectCursor.HoverDuration = value;
                RaisePropertyChanged();
            }
        }

        public int ControlModeIndex
        {
            get
            {
                return (int)kinectCursor.Mode;
            }
            set
            {
                kinectCursor.Mode = (ControlMode)value;
                RaisePropertyChanged();
            }
        }

        #endregion

        public void LoadSettings()
        {
            MoveScale = Properties.Settings.Default.MoveScale;
            HoverDuration = Properties.Settings.Default.HoverDuration;
            HoverRange = Properties.Settings.Default.HoverRange;
            Smoothing = Properties.Settings.Default.Smoothing;

            ControlModeIndex = Properties.Settings.Default.Mode;
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.MoveScale = MoveScale;
            Properties.Settings.Default.Smoothing = Smoothing;
            Properties.Settings.Default.HoverRange = HoverRange;
            Properties.Settings.Default.HoverDuration = HoverDuration;
            Properties.Settings.Default.Mode = ControlModeIndex;

            Properties.Settings.Default.Save();
        }

        public void ResetToDefault()
        {
            MoveScale = DEFAULT_MOVE_SCALE;
            Smoothing = DEFAULT_SMOOTHING;
            HoverRange = DEFAULT_HOVER_RANGE;
            HoverDuration = DEFAULT_HOVER_DURATION;
        }

        public void Quit()
        {
            SaveSettings();
            ControlModeIndex = 0;
        }

        #endregion 



    }
}
