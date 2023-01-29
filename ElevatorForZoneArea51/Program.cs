using System;
using System.Threading;

namespace ElevatorForZoneArea51
{
    class Program
    {
        // Инициализация
        static Semaphore signal;
        private static object someLock = new object();

        static public Random random = new Random();
        static int countOfAgents = random.Next(1, 10);
        static int securityLevel;
        static int currentFloor;
        static int likeFloor;

        static int initialFloor = 0;

        static string[] floors = { "0", "G", "S", "T1", "T2" };

        static void Main(string[] args)
        {

            signal = new Semaphore(1, 1);
            var cts = new CancellationTokenSource();
            Thread elevator = new Thread(Elevator);

            Console.WriteLine("Press Esc to cancel threads.");
            Console.WriteLine();
            Console.WriteLine($"Number of agents waiting for the elevator: {countOfAgents}");

            // Отмяна на команда
            elevator.Start(cts.Token);
            if (Console.ReadKey().KeyChar == (char)ConsoleKey.Escape)
            {
                Console.WriteLine();
                Console.WriteLine("SStart cancelling.");
                cts.Cancel();
                elevator.Join();
                Console.WriteLine(" Cancelled successfully.");
                Console.WriteLine(" Press any key to exit.");
            }

            Console.ReadKey();
        }


        static public void Elevator(object tag)
        {
            // Инициализация
            CancellationToken token = (CancellationToken)tag;
            var cts = new CancellationTokenSource();


            for (int i = 1; i <= countOfAgents; i++)
            {
                // Проверка за отказване
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine();
                    Console.WriteLine("Elevator - STOP.");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine($"Agent {i} enters the elevator.");

                lock (someLock)
                {
                    Thread agent = new Thread(Agent);
                    agent.Start();
                    agent.Join();
                }

                // Проверка на достъпа
                while (((securityLevel == 1) && (likeFloor > 1)) || ((securityLevel == 2) && (likeFloor > 2)))
                {
                    Console.WriteLine($"Agent {i} doesn't have access to floor {floors[likeFloor]}.");

                    do
                    {
                        likeFloor = random.Next(0, 5);
                    } while (likeFloor == currentFloor);

                    Console.WriteLine($"Agent {i} chose new floor: {floors[likeFloor]}.");
                }

                // Асансьора отива към агента
                if (initialFloor < currentFloor)
                {
                    for (int j = initialFloor; j <= currentFloor; j++)
                    {
                        Thread.Sleep(500); // времезакъснение
                    }
                }
                else
                {
                    for (int j = initialFloor; j >= currentFloor; j--)
                    {
                        Thread.Sleep(500);
                    }
                }

                signal.WaitOne();

                // Асансьора отива към избрания етаж
                if (currentFloor < likeFloor)
                {
                    for (int j = currentFloor; j <= likeFloor; j++)
                    {
                        Thread.Sleep(500);
                    }
                    initialFloor = likeFloor;
                }
                else
                {
                    for (int j = currentFloor; j >= likeFloor; j--)
                    {
                        Thread.Sleep(500);
                    }
                }

                // Асансьора е освободен
                lock (someLock)
                {
                    signal.Release();
                }

                // Печат
                Console.WriteLine($"Agent {i} goes off the elevator.");

                initialFloor = likeFloor;
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            return;
        }

        static public void Agent()
        {
            // Инициализация
            string[] securityLevelAbbreviation = { "", "CONFIDENTIAL", "SECRET", "TOP SECRET" };

            // Генерират се random променливи
            securityLevel = random.Next(1, 4);
            currentFloor = random.Next(0, 5);
            likeFloor = random.Next(0, 5);

            // Проверка на секретно ниво
            switch (securityLevel)
            {
                case 1:
                case 2:
                    while (currentFloor > securityLevel)
                    {
                        currentFloor = random.Next(0, 5);
                        while (likeFloor == currentFloor)
                        {
                            likeFloor = random.Next(0, 5);
                        }
                    }
                    break;
                case 3:
                    while (likeFloor == currentFloor)
                    {
                        likeFloor = random.Next(0, 5);
                    }
                    break;
                default:
                    Console.WriteLine("ALERT! ALERT! ALERT!");
                    return;
            }
            Console.WriteLine($"Security level {securityLevelAbbreviation[securityLevel]}. Current floor {floors[currentFloor]}, desired floor {floors[likeFloor]}.");
        }


    }
}
