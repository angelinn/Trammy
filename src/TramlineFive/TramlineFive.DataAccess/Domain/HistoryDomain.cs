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

            await TramlineFiveContext.AddHistoryAsync(history);
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
            return (await TramlineFiveContext.Take(count)).Select(h => new HistoryDomain(h));
        }

        public static async Task CleanHistoryAsync()
        {
            await TramlineFiveContext.CleanHistoryAsync();
        }
    }
}
