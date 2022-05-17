using Talabatak.Helpers;
using Talabatak.Models.Data;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Talabatak.Jobs
{
    public class StoresJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var Stores = db.Stores.Where(x => x.IsAccepted == true && x.IsDeleted == false && x.IsHidden == false && x.IsBlocked == false && x.IsClosingManual == false).ToList();
            if (Stores != null && Stores.Count() > 0)
            {
                foreach (var store in Stores)
                {
                    try
                    {
                        if (store.Is24HourOpen == true)
                        {
                            store.IsOpen = true;
                        }
                        else
                        {
                            var DateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                            if (Time.IsTimeBetween(DateTimeNow, store.OpenFrom.Value, store.OpenTo.Value) == true)
                            {
                                store.IsOpen = true;
                            }
                            else
                            {
                                store.IsOpen = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                db.SaveChanges();
            }
        }

        public static async Task RunProgram()
        {
            try
            {
                // Grab the Scheduler instance from the Factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();

                // and start it off
                await scheduler.Start();

                // define the job and tie it to our HelloJob class
                IJobDetail job = JobBuilder.Create<StoresJob>()
                    .WithIdentity("job1", "group1")
                    .Build();

                // Trigger the job to run now, and then repeat every n seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(1)
                        .RepeatForever())
                    .Build();

                // Tell quartz to schedule the job using our trigger
                await scheduler.ScheduleJob(job, trigger);

                // some sleep to show what's happening

                // and last shut down the scheduler when you are ready to close your program
            }
            catch (SchedulerException se)
            {
            }
        }
    }
}