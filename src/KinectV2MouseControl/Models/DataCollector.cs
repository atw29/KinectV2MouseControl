using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public static BlockingCollection<Data> Start()
        {
            BlockingCollection<Data> datas = new BlockingCollection<Data>();
            DataCollector dataCollector = new DataCollector(datas);
            return datas;
        }

        
    }

    public class DataCollector
    {
        private BlockingCollection<Data> queue;
        private CancellationTokenSource token;
        private Thread t;
        private readonly DateTime opened;
        private const string folder = "C:\\Users\\Alex\\Google Drive\\University Drive\\Bath Drive\\Third Year\\Diss\\Other\\data\\";
        private readonly string path;


        public DataCollector(BlockingCollection<Data> queue)
        {
            token = new CancellationTokenSource();

            opened = DateTime.Now;

            path = $"{folder}{opened.ToString("yyyy.MM.dd HH.mm.ss")}.csv";

            CreateFile(opened);
            t = new Thread(new ThreadStart(Perform_Execution));
        }

        public void Start()
        {
            t.Start();
        }

        public void Stop()
        {
            token.Cancel();
        }

        private void CreateFile(DateTime opened)
        {
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine($"CLICK_STATE, X_POS, Y_POS, TIME, PARASITE");
                writer.WriteLine($"start,,,{opened.ToString("HH:mm:ss")},");
            }
        }

        private void Perform_Execution()
        {
            while (queue.TryTake(out Data data, Timeout.Infinite, token.Token))
            {
                WriteString($"{data.State}, {data.XPos}, {data.YPos}, {DateTime.Now.ToString("HH:mm:ss")},");
            }
        }

        private void WriteString(string toWrite)
        {
            using (var writer = new StreamWriter(path, true))
            {
                writer.WriteLine(toWrite);
            }
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
    }
}
