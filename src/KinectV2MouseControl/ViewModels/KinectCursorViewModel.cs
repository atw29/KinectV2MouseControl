using KinectV2MouseControl.Models;
using KinectV2MouseControl.Views.Tasks;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        const int TASK_NUM = 1;

        internal void Create_Task()
        {
            Run_Task(USER, TASK_NUM);
        }

        private void Run_Task(string USER, int TASK_NUM)
        {
            new Background().Show();
            new Menu_Task().Show();
            
            kinectCursor.NeedGrabbing = TASK_NUM > 3;
            
            switch (TASK_NUM)
            {
                case 2:
                    new MockUp().Show();
                    break;

                case 3:
                    new X_Rays().Show();
                    break;

                case 4:
                    new MockUp().Show();
                    break;

                case 5:
                    new Lighting_Control().Show();
                    break;

                case 6:
                    new MockUp().Show();
                    break;

                case 7:
                    new X_Rays().Show();
                    break;

                default:
                    break;
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
