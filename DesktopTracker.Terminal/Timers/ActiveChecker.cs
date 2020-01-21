using DesktopTracker.Infra.Enums;
using DesktopTracker.Infra.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopTracker.Terminal.Timers
{
    public class ActiveChecker
    {
        private WindowData storageWindow;

        private long invokeCount;
        private readonly long? maxCount;

        //Initializing an empty enumerable of User types

        public readonly Collection<UserData> users = new Collection<UserData>();

        //Optional count for program to run if needed
        public ActiveChecker(long? count)
        {
            invokeCount = 0;
            maxCount = count;
        }

        public async void CheckWindows(object state)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)state;

            IAsyncEnumerable<Process> processlist = GenerateProcesses();

            await foreach (Process process in processlist)
            {
                try
                {
                    process.EnableRaisingEvents = true;
                    process.Exited += Process_Exited;
                }
                catch (Win32Exception)
                {
                    continue;
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                #region USER AND TICK LOGIC

                string curUser = WindowsIdentity.GetCurrent().Name;

                UserData userMatch = users.FirstOrDefault(chk => chk.UserName == curUser);

                UserData newUser = new UserData();

                if (userMatch == null)
                {
                    newUser.UserName = curUser;
                    newUser.WindowDatas = new List<WindowData>();
                    newUser.DeviceDatas = new List<DeviceData>
                    {
                        new DeviceData
                        {
                            //Dunno why but it doesn't work with TickCount64, so expect overflows after 24 days...
                            UpTime = TimeSpan.FromMilliseconds(Environment.TickCount)
                        }
                    };
                    users.Add(newUser);
                }

                #endregion

                #region WINDOW LOGIC

                //Process is running and has a title, these are the ones we care about
                if (!string.IsNullOrEmpty(process.MainWindowTitle) && process.Responding == true)
                {
                    UserData winUserMatch = users.FirstOrDefault(chk => chk.UserName == curUser);
                    WindowData winMatch = winUserMatch.WindowDatas.FirstOrDefault(chk => chk.FullName == process.MainWindowTitle);

                    if (winMatch == null)
                    {
                        WindowData newWindow = new WindowData
                        {
                            WindowsIdentifier = process.Id,
                            FullName = process.MainWindowTitle,
                            RecordedEnteredTime = DateTime.UtcNow,
                            WindowState = WindowState.ACTIVE,
                            UserId = winUserMatch.UserId
                        };

                        winUserMatch.WindowDatas.Add(newWindow);
                    }

                    //Extract data from the window collection within the current user
                    foreach (WindowData window in winUserMatch.WindowDatas)
                    {
                        //Assign for access outside method
                        storageWindow = window;

                        //Check active windows if they go over the assigned idle time, if so, assign idle and record time they were assigned idle
                        if (window.WindowState == WindowState.ACTIVE)
                        {
                            //Maximum time to consider a window idle
                            DateTime maxTimeForIdle = window.RecordedEnteredTime.AddSeconds(20);

                            if (DateTime.UtcNow >= maxTimeForIdle)
                            {
                                Console.WriteLine("GUID: {0} became idle at {1}",
                                    window.WindowId,
                                    DateTime.UtcNow);
                                window.WindowState = WindowState.IDLE;
                                window.RecordedIdleTime = window.RecordedIdleTime;

                                Console.WriteLine(Environment.NewLine);
                            }
                        }
                    }

                    //This foreach is used to update the current device tick time, so its not static
                    foreach (DeviceData device in winUserMatch.DeviceDatas)
                    {
                        if (device.SessionComplete == false)
                        {
                            TimeSpan curTime = TimeSpan.FromMilliseconds(Environment.TickCount);

                            if (device.UpTime != curTime)
                            {
                                device.UpTime = curTime;
                            }
                        }
                    }
                }

                #endregion
            }

            #region End Clause for Timer

            invokeCount++;

            if (invokeCount == maxCount)
            {
                invokeCount = 0;
                autoEvent.Set();
            }

            #endregion

            //Tester method for users
            GetUsers();

            Console.WriteLine("Current invocation count: " + invokeCount);

            Console.WriteLine(Environment.NewLine);

            #region END APPLICATION EVENT

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            #endregion
        }

        //This will work, but only when a database is created
        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            throw new NotImplementedException();

            string curUser = WindowsIdentity.GetCurrent().Name;

            UserData userMatch = users.FirstOrDefault(chk => chk.UserName == curUser);

            DeviceData deviceMatch = userMatch.DeviceDatas.FirstOrDefault(chk => chk.UserData == userMatch);

            deviceMatch.SessionComplete = true;
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            storageWindow.RecordedExitTime = DateTime.UtcNow;
            storageWindow.Exited = true;

            Console.WriteLine("Current process: {0} has exited at {1}",
                storageWindow.WindowId,
                storageWindow.RecordedExitTime);

            Console.WriteLine(Environment.NewLine);
        }

        private async Task<Process[]> GetListProcesses()
        {
            Process[] processes = await Task.Run(() => Process.GetProcesses());

            return processes;
        }

        private async IAsyncEnumerable<Process> GenerateProcesses()
        {
            Process[] processlist = await GetListProcesses();

            foreach (Process process in processlist)
            {
                yield return process;
            }
        }

        private void GetUsers()
        {
            foreach (UserData user in users)
            {
                Console.WriteLine("GUID: {0} - Name: {1}",
                    user.UserId,
                    user.UserName);

                Console.WriteLine(Environment.NewLine);

                foreach (WindowData window in user.WindowDatas)
                {
                    Console.WriteLine("GUID: {0} - Name: {1}",
                        window.WindowId,
                        window.FullName);
                }

                Console.WriteLine(Environment.NewLine);

                foreach (DeviceData devicedata in user.DeviceDatas)
                {
                    Console.WriteLine("GUID: {0} - DeviceTime: {1}",
                        devicedata.DeviceId,
                        devicedata.UpTime);
                }

                Console.WriteLine(Environment.NewLine);
            }
        }
    }
}