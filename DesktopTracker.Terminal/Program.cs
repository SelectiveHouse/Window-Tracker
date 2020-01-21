using DesktopTracker.Terminal.Timers;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopTracker.Terminal
{
    internal class Program
    {
        private static EventHubClient eventHubClient;

        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            #region STRING/AZURE SETUP

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(configuration.GetSection("ConnectionStrings:DesktopTrackerHub").Value)
            {
                EntityPath = configuration.GetSection("DesktopTrackerEventHub:CloseMessage").Value
            };

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            #endregion

            AutoResetEvent autoEvent = new AutoResetEvent(false);

            Console.WriteLine("Checking Active Windows at: {0:h:mm:ss.fff} \n",
                DateTime.UtcNow);

            #region WINDOW TIMER
            //Enter null for no break clause, e.g. program goes on forever
            ActiveChecker activeWindows = new ActiveChecker(null);

            //1000 = 1 second etc.
            Timer foregroundTimer = new Timer(activeWindows.CheckWindows, autoEvent, 0, 5000);

            #endregion

            #region HIBERNATE TIMER
            //Enter max time for Kiosk to hibernate
            DateTime maxTimeForHibernate = DateTime.UtcNow.AddMinutes(30);

            HibernateChecker hibernateChecker = new HibernateChecker(maxTimeForHibernate);

            //Checks silently every 0.25 seconds
            Timer hibernateTimer = new Timer(hibernateChecker.CheckHibernate, autoEvent, 0, 250);
            #endregion

#if DEBUG
            //Keeps console open
            Console.ReadLine();
#endif
        }

    }
}