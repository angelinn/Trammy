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
        public string Line { get; set; }
        public string StopCode { get; set; }
        public DateTime TimeStamp { get; set; }

        public HistoryDomain(History entity)
        {
            Line = entity.Line;
            StopCode = entity.StopCode;
            TimeStamp = entity.TimeStamp;
        }

        public static async Task AddAsync(string line, string stopCode)
        {
            await TramlineFiveContext.AddHistoryAsync(new History
            {
                Line = line,
                StopCode = stopCode,
                TimeStamp = DateTime.Now
            });
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
    }
}
