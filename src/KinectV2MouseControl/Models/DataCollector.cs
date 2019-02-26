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
        public static readonly Data PoisonData = new Data(-10000, -10000, MouseControlState.None);
        public static DataCollector Start(string USER, int TASK_NUM)
        {
            string dir = Path.Combine("C:\\Users","Alex","Google Drive","University Drive","Bath Drive", "Third Year","Diss","Other","Data",USER,"MI",TASK_NUM.ToString());

            Directory.CreateDirectory(dir);

            Debug.WriteLine($"Data Writing To : {dir}");

            BlockingCollection<Data> datas = new BlockingCollection<Data>();

            DataCollector dataCollector = new DataCollector(datas, dir);

            dataCollector.Start();
            return dataCollector;
        }

        public static string PrintTime(this DateTime dateTime)
        {
            return dateTime.ToString("HH:mm:ss");
        }
    }

    public class DataCollector
    {
        private BlockingCollection<Data> Queue;
        private readonly string path;

        private readonly DateTime WhenOpened;

        public DataCollector(BlockingCollection<Data> Queue, string dir)
        {
            WhenOpened = DateTime.Now;

            path = Path.Combine(dir, $"{WhenOpened.ToString("yyyy.MM.dd HH.mm.ss")}.csv");

            this.Queue = Queue;
        }

        public void Start()
        {
            CreateFile();

            Task.Factory.StartNew(Perform_Execution);
        }

        public void Stop()
        {
            Debug.WriteLine("Stopping Data Collector Thread");
            Queue.Add(DataCollectorFactory.PoisonData);
            //Queue.CompleteAdding();
        }

        private void CreateFile()
        {
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine($"CLICK_STATE,X_POS,Y_POS,TIME,PARASITE");
                writer.WriteLine($"start,,,{WhenOpened.PrintTime()},");
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
                WriteString(data.ToString());
            }
            if (Queue.Count > 0)
            {
                try
                {
                    while (true)
                    {
                        WriteString(Queue.Take().ToString());
                    }
                } catch (InvalidOperationException)
                {
                    Debug.WriteLine("Finished Writing Queue");
                }
            }
            Debug.WriteLine("Finishing Writing Objects in Queue ");
            WriteString($"end,,,{DateTime.Now.PrintTime()},");
        }

        private void WriteString(string toWrite)
        {
            using (var writer = new StreamWriter(path, true))
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
        public Data(double xPos, double yPos, MouseControlState state)
        {
            XPos = xPos;
            YPos = yPos;
            State = state;
        }

        public MouseControlState State { get; }

        public double YPos { get; }

        public double XPos { get; }

        public override string ToString()
        {
            return $"{State},{XPos},{YPos},{DateTime.Now.ToString("HH:mm:ss")},";
        }
    }
}
