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
        public static BlockingCollection<Data> Start()
        {
            BlockingCollection<Data> datas = new BlockingCollection<Data>();
            DataCollector dataCollector = new DataCollector(datas);
            dataCollector.Start();
            return datas;
        }

        public static string PrintTime(this DateTime dateTime)
        {
            return dateTime.ToString("HH:mm:ss");
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


        public DataCollector(BlockingCollection<Data> Queue)
        {

            token = new CancellationTokenSource();
            opened = DateTime.Now;

            path = $"{folder}{opened.ToString("yyyy.MM.dd HH.mm.ss")}.csv";

            queue = Queue;

            var domain = AppDomain.CurrentDomain;
            var process = Process.GetCurrentProcess();

            process.Exited += Process_Exited;
            domain.ProcessExit += Process_Exited;
            domain.DomainUnload += Process_Exited;

            t = new Thread(new ThreadStart(Perform_Execution))
            {
                Name = "Data Collector Thread"
            };
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Stopping Process");
            Stop();
        }

        public void Start()
        {

            CreateFile(opened);

            t.Start();
        }

        public void Stop()
        {
            System.Diagnostics.Debug.WriteLine("Stopping Data Collector Thread");
            token.Cancel();
            queue.Add(DataCollectorFactory.PoisonData);
            
        }

        private void CreateFile(DateTime opened)
        {
            using (var writer = File.CreateText(path))
            {
                writer.WriteLine($"CLICK_STATE,X_POS,Y_POS,TIME,PARASITE");
                writer.WriteLine($"start,,,{opened.PrintTime()},");
            }
        }

        private void Perform_Execution()
        {
            while (queue.TryTake(out Data data, Timeout.Infinite, token.Token))
            {
                WriteString(data.ToString());
                if (data.Equals(DataCollectorFactory.PoisonData))
                {
                    break;
                }
                //System.Diagnostics.Trace.WriteLine($"Writing Data : {data.State}");
            }
            System.Diagnostics.Debug.WriteLine("Finishing Writing Objects in Queue ");
            try
            {
                while (true)
                {
                    WriteString(queue.Take().ToString());
                }
            } catch (InvalidOperationException)
            {
                System.Diagnostics.Debug.WriteLine("Finished Writing Queue");
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
