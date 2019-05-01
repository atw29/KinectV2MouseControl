using Microsoft.Kinect;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static KinectV2MouseControl.KinectCursor;

namespace KinectV2MouseControl.Models
{
    public static class DataCollectorFactory
    {
        public static readonly Data PoisonData = new Data(-10000, -10000, MouseControlState.None, 0.0, 0.0, HandState.Unknown, TrackingConfidence.Low, TrackingState.NotTracked);
        public static DataCollector Start(string USER, int TASK_NUM)
        {
            string dir = Path.Combine("C:\\Users","Alex","Google Drive","University Drive","Bath Drive", "Third Year","Diss","Other","Data",USER,"MI",TASK_NUM.ToString());

            Directory.CreateDirectory(dir);

            Debug.WriteLine($"Data Writing To : {dir}");

            BlockingCollection<Data> datas = new BlockingCollection<Data>();

            DataCollector dataCollector = new DataCollector(datas, dir, USER, TASK_NUM);

            dataCollector.Start();
            return dataCollector;
        }

        public static string PrintTime(this DateTime dateTime)
        {
            return dateTime.ToString("HH:mm:ss:fff");
        }
    }

    public class DataCollector
    {
        private BlockingCollection<Data> Queue;

        private readonly string User;
        private readonly int Task_Num;

        private readonly string DataPath;
        private readonly string InfoPath;
        private readonly DateTime WhenOpened;

        public DataCollector(BlockingCollection<Data> Queue, string dir, string USER, int TASK_NUM)
        {
            WhenOpened = DateTime.Now;

            User = USER;
            Task_Num = TASK_NUM;

            string FileName = WhenOpened.ToString("yyyy.MM.dd HH.mm.ss");
            DataPath = Path.Combine(dir, $"{FileName}.csv");
            InfoPath = Path.Combine(dir, $"{FileName}.info");

            this.Queue = Queue;
        }

        public void Start()
        {
            CreateInfoFile();
            CreateDataFile();

            Task.Factory.StartNew(Perform_Execution);
        }
        public void Stop()
        {
            Debug.WriteLine("Stopping Data Collector Thread");
            Queue.Add(DataCollectorFactory.PoisonData);
        }

        private void CreateDataFile()
        {
            using (var writer = File.CreateText(DataPath))
            {
                writer.WriteLine($"CLICK_STATE,X_POS,Y_POS,X_HAND,Y_HAND,HAND_STATE,HAND_CONFIDENCE,TRACKING_STATE,TIME,PARASITE");
            }
        }

        private void CreateInfoFile()
        {
            using (var writer = File.CreateText(InfoPath))
            {
                writer.WriteLine($"start: {WhenOpened.PrintTime()}");
                writer.WriteLine($"user: {User}");
                writer.WriteLine($"type: MI");
                writer.WriteLine($"task_num: {Task_Num}");
            }
        }
        void Perform_Execution()
        {
            while (Queue.TryTake(out Data data, Timeout.Infinite))
            {
                if (data.Equals(DataCollectorFactory.PoisonData))
                {
                    break;
                }
                WriteString(DataPath, data.ToString());
            }
            if (Queue.Count > 0)
            {
                try
                {
                    while (true)
                    {
                        WriteString(DataPath, Queue.Take().ToString());
                    }
                } catch (InvalidOperationException)
                {
                    Debug.WriteLine("Finished Writing Queue");
                }
            }

            Debug.WriteLine("Finishing Writing Objects in Queue ");
            WriteString(InfoPath, $"end: {DateTime.Now.PrintTime()}");
        }

        private void WriteString(string Path, string toWrite)
        {
            using (var writer = new StreamWriter(Path, true))
            {
                writer.WriteLine(toWrite);
            }
        }

        internal void CollectData(Data d)
        {
            Queue.Add(d);
        }
    }

    public class Data
    {
        public Data(double xPos, double yPos, MouseControlState state, Body body)
        {
            Joint RightHand = body.Joints[JointType.HandRight];

            XHand = RightHand.Position.X;
            YHand = RightHand.Position.Y;
            TrackingState = RightHand.TrackingState;
            

            HandState = body.HandRightState;
            HandConfidence = body.HandRightConfidence;

            XPos = xPos;
            YPos = yPos;
            State = state;
        }

        public Data(double xPos, double yPos, MouseControlState state, double xHand, double yHand, HandState handState, TrackingConfidence trackingConfidence, TrackingState trackingState)
        {
            XHand = xHand;
            YHand = yHand;
            HandState = handState;
            HandConfidence = trackingConfidence;
            TrackingState = trackingState;

            XPos = xPos;
            YPos = yPos;
            State = state;
        }

        private readonly double XHand;
        private readonly double YHand;

        private readonly HandState HandState;
        private readonly TrackingConfidence HandConfidence;

        private readonly TrackingState TrackingState;

        private readonly MouseControlState State;
        private readonly double YPos;
        private readonly double XPos;

        public override string ToString()
        {
            return $"{State},{XPos},{YPos},{XHand},{YHand},{HandState.ToString()},{HandConfidence.ToString()},{TrackingState.ToString()},{DateTime.Now.PrintTime()},";
        }
    }
}
