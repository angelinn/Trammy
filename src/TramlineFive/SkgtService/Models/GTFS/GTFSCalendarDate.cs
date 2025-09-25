using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace SkgtService.Models.GTFS;

public class GTFSCalendarDate
{
    [Name("service_id")]
    public string ServiceId { get; set; } = null!;

    [Name("date")]
    public string Date { get; set; } = null!; // YYYYMMDD format

    [Name("exception_type")]
    public int ExceptionType { get; set; } // 1 = added, 2 = removed
}
