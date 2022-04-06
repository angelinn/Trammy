using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TramlineFive.DataAccess.Entities
{
    public class Favourite
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public string StopCode { get; set; }
        public int TimesClicked { get; set; }
    }
}
