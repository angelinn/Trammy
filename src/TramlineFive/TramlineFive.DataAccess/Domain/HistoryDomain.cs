using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.DataAccess.Entities;

namespace TramlineFive.DataAccess.Domain
{
    public class HistoryDomain
    {
        public static event EventHandler HistoryAdded;
        
        public string StopCode { get; set; }
        public string Name { get; set; }
        public DateTime TimeStamp { get; set; }

        public HistoryDomain(History entity)
        {
            Name = entity.Name;
            StopCode = entity.StopCode;
            TimeStamp = entity.TimeStamp;
        }

        public static async Task AddAsync(string stopCode, string name)
        {
            History history = new History
            {
                StopCode = stopCode,
                Name = name,
                TimeStamp = DateTime.Now
            };

            await TramlineFiveContext.AddAsync(history);
            HistoryAdded?.Invoke(new HistoryDomain(history), new EventArgs());
        }

        //public static void Remove(History history)
        //{
        //    using (TramlineFiveContext context = new TramlineFiveContext())
        //    {
        //        context.History.Remove(history);
        //        context.SaveChanges();
        //    }
        //}

        public static async Task<IEnumerable<HistoryDomain>> TakeAsync(int count = 10)
        {
            return (await TramlineFiveContext.TakeByDescending<History, DateTime>(h => h.TimeStamp, count)).Select(h => new HistoryDomain(h));
        }

        public static async Task<HistoryDomain> GetMostFrequentStopForCurrentHour()
        {
            List<History> histories = await TramlineFiveContext.TakeForLastDays(10);
            var groups = histories.GroupBy(h => h.TimeStamp.Hour);
            var group = groups.FirstOrDefault(g => new int[] { DateTime.Now.Hour, DateTime.Now.Hour + 1, DateTime.Now.Hour - 1 }.Contains(g.Key));

            if (group != null)
            {
                History most = group.GroupBy(i => i.StopCode).OrderByDescending(grp => grp.Count()).Select(grp => grp.First()).First();
                return new HistoryDomain(most);
            }

            return null;
        }

        public static async Task CleanHistoryAsync()
        {
            await TramlineFiveContext.CleanHistoryAsync();
        }
    }
}
