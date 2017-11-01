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

        public static void Add(string line, string stopCode)
        {
            using (TramlineFiveContext context = new TramlineFiveContext())
            {
                context.History.Add(new History
                {
                    Line = line,
                    StopCode = stopCode,
                    TimeStamp = DateTime.Now
                });

                context.SaveChanges();
            }
        }

        public static void Remove(History history)
        {
            using (TramlineFiveContext context = new TramlineFiveContext())
            {
                context.History.Remove(history);
                context.SaveChanges();
            }
        }

        public static async Task<IEnumerable<HistoryDomain>> TakeAsync(int count = 10)
        {
            IEnumerable<HistoryDomain> recent = null;
            await Task.Run(() =>
            {
                using (TramlineFiveContext context = new TramlineFiveContext())
                {
                    recent = context.History.Take(count).ToList().Select(h => new HistoryDomain(h));
                }
            });

            return recent;
        }
    }
}
