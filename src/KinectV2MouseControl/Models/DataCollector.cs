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
        public static DataCollector Start()
        {
            BlockingCollection<Data> datas = new BlockingCollection<Data>();
            DataCollector dataCollector = new DataCollector(datas);
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
        private readonly DateTime opened;
        private const string folder = "C:\\Users\\Alex\\Google Drive\\University Drive\\Bath Drive\\Third Year\\Diss\\Other\\data\\";
        private readonly string path;

        public DataCollector(BlockingCollection<Data> Queue)
        {
            opened = DateTime.Now;

            path = $"{folder}{opened.ToString("yyyy.MM.dd HH.mm.ss")}.csv";

            this.Queue = Queue;
        }

        public void Start()
        {
            CreateFile(opened);

            Task.Factory.StartNew(Perform_Execution);
        }

        public void Stop()
        {
            Debug.WriteLine("Stopping Data Collector Thread");
            Queue.Add(DataCollectorFactory.PoisonData);
            //Queue.CompleteAdding();
        }

        private void CreateFile(DateTime opened)
        {
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine($"CLICK_STATE,X_POS,Y_POS,TIME,PARASITE");
                writer.WriteLine($"start,,,{opened.PrintTime()},");
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
            Debug.WriteLine("Finishing Writing Objects in Queue ");
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
