using System;
using System.Threading;

namespace ThreadPoolServer
{
    class Program
    {
        // Параметры моделирования
        static int poolSize = 10;
        static int requestCount = 100;
        static double mu = 10;
        static double lambda = 10;
        static int requestInterval = (int)(1000 / lambda);
        static int processingTime = (int)(1000 / mu);
        static List<SimulationResult> results = new List<SimulationResult>();

        class SimulationResult
        {
            public double Lambda { get; set; }
            public double P0_Theory { get; set; }
            public double P0_Experiment { get; set; }
            public double PReject_Theory { get; set; }
            public double PReject_Experiment { get; set; }
            public double Q_Theory { get; set; }
            public double Q_Experiment { get; set; }
            public double A_Theory { get; set; }
            public double A_Experiment { get; set; }
            public double K_Theory { get; set; }
            public double K_Experiment { get; set; }
        }

        static void Main()
        {
            int cnt = 1;
            // Запускаем серию экспериментов с разной интенсивностью
            for (double lam = 5; lam <= 15; lam += 1)
            {
                lambda = lam;
                Console.WriteLine("Starting experiment " + cnt);
                RunExperiment();
                Console.WriteLine("Complete experimnet" + cnt);
                cnt++;
            }


            SaveResultsToCSV();
            Console.WriteLine("Эксперименты завершены. Результаты сохранены в results.csv");
        }

        static void RunExperiment()
        {
            requestInterval = (int)(1000 / lambda);
            processingTime = (int)(1000 / mu);

            var server = new Server(poolSize, processingTime);
            var client = new Client(server);

            for (int id = 1; id <= requestCount; id++)
            {
                client.Send(id);
                Thread.Sleep(requestInterval);
            }

            Thread.Sleep(processingTime * 2); // Ожидание завершения обработки

            // Расчет показателей
            double rho = lambda / mu;
            double p0_theory = CalculateP0(rho, poolSize);
            double p_reject_theory = CalculateRejectProbability(rho, poolSize, p0_theory);

            double p_reject_exp = (double)server.RejectedRequests / server.TotalRequests;
            double idleTime = server.IdleTime / (poolSize * server.TotalTimeMs);

            // Сохранение результатов
            results.Add(new SimulationResult
            {
                Lambda = lambda,
                P0_Theory = p0_theory,
                P0_Experiment = idleTime,
                PReject_Theory = p_reject_theory,
                PReject_Experiment = p_reject_exp,
                Q_Theory = 1 - p_reject_theory,
                Q_Experiment = 1 - p_reject_exp,
                A_Theory = lambda * (1 - p_reject_theory),
                A_Experiment = lambda * (1 - p_reject_exp),
                K_Theory = rho * (1 - p_reject_theory),
                K_Experiment = lambda / mu * (1 - p_reject_exp)
            });
        }

        static double CalculateP0(double rho, int n)
        {
            double sum = 1.0;
            double fact = 1.0;

            for (int i = 1; i <= n; i++)
            {
                fact *= i;
                sum += Math.Pow(rho, i) / fact;
            }

            return 1.0 / sum;
        }

        static double CalculateRejectProbability(double rho, int n, double p0)
        {
            double fact = 1.0;
            for (int i = 1; i <= n; i++) fact *= i;
            return (Math.Pow(rho, n) / fact) * p0;
        }

        static void SaveResultsToCSV()
        {
            using (var writer = new StreamWriter("../../../../result/results.csv"))
            {
                writer.WriteLine("Lambda,P0_Theory,P0_Experiment,PReject_Theory,PReject_Experiment," +
                               "Q_Theory,Q_Experiment,A_Theory,A_Experiment,K_Theory,K_Experiment");

                foreach (var r in results)
                {
                    writer.WriteLine($"{r.Lambda} {r.P0_Theory} {r.P0_Experiment} " +
                                    $"{r.PReject_Theory} {r.PReject_Experiment}, " +
                                    $"{r.Q_Theory} {r.Q_Experiment} " +
                                    $"{r.A_Theory} {r.A_Experiment} " +
                                    $"{r.K_Theory} {r.K_Experiment}");
                }
            }
        }
    }

    struct PoolRecord
    {
        public Thread Thread;
        public bool InUse;
    }

    class Server
    {
        private PoolRecord[] pool;
        private object threadLock = new object();
        private int processingTime;
        private DateTime startTime;
        public long TotalIdleTicks { get; private set; }
        public long TotalTimeMs => (long)(DateTime.Now - startTime).TotalMilliseconds;

        public int TotalRequests { get; private set; }
        public int ProcessedRequests { get; private set; }
        public int RejectedRequests { get; private set; }
        public double IdleTime => (double)TotalIdleTicks / TimeSpan.TicksPerMillisecond;

        public Server(int poolSize, int processingTime)
        {
            pool = new PoolRecord[poolSize];
            this.processingTime = processingTime;
            startTime = DateTime.Now;

            for (int i = 0; i < pool.Length; i++)
            {
                pool[i].InUse = false;
            }
        }

        public void ProcessRequest(object sender, RequestEventArgs e)
        {
            lock (threadLock)
            {
                TotalRequests++;
                UpdateIdleTime();

                for (int i = 0; i < pool.Length; i++)
                {
                    if (!pool[i].InUse)
                    {
                        pool[i].InUse = true;
                        pool[i].Thread = new Thread(Process) { IsBackground = true };
                        pool[i].Thread.Start(e.Id);
                        ProcessedRequests++;
                        return;
                    }
                }

                RejectedRequests++;
            }
        }

        private void Process(object requestId)
        {
            Thread.Sleep(processingTime);

            lock (threadLock)
            {
                for (int i = 0; i < pool.Length; i++)
                {
                    if (pool[i].Thread == Thread.CurrentThread)
                    {
                        pool[i].InUse = false;
                        break;
                    }
                }
            }
        }

        private void UpdateIdleTime()
        {
            int idleChannels = 0;
            foreach (var channel in pool)
            {
                if (!channel.InUse) idleChannels++;
            }
            TotalIdleTicks += idleChannels * (DateTime.Now - startTime).Ticks;
            startTime = DateTime.Now;
        }
    }

    class Client
    {
        private Server server;

        public Client(Server server)
        {
            this.server = server;
            RequestSent += server.ProcessRequest;
        }

        public void Send(int id)
        {
            OnRequestSent(new RequestEventArgs(id));
        }

        protected virtual void OnRequestSent(RequestEventArgs e)
        {
            RequestSent?.Invoke(this, e);
        }

        public event EventHandler<RequestEventArgs> RequestSent;
    }

    public class RequestEventArgs : EventArgs
    {
        public int Id { get; }

        public RequestEventArgs(int id)
        {
            Id = id;
        }
    }
}