using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkgtService.Models.Json
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Details
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("route_id")]
        public int RouteId { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("is_active")]
        public int IsActive { get; set; }

        [JsonProperty("polyline")]
        public string Polyline { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("continious_pickup")]
        public object ContiniousPickup { get; set; }

        [JsonProperty("continious_drop_off")]
        public object ContiniousDropOff { get; set; }
    }

    public class EndStop
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("ext_id")]
        public string ExtId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("is_active")]
        public int IsActive { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }
    }

    public class LineResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("ext_id")]
        public string ExtId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("is_active")]
        public int IsActive { get; set; }

        [JsonProperty("has_single_direction")]
        public int HasSingleDirection { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("tr_name")]
        public string TrName { get; set; }

        [JsonProperty("tr_icon")]
        public string TrIcon { get; set; }

        [JsonProperty("tr_color")]
        public string TrColor { get; set; }
    }

    public class ScheduleResponse
    {
        [JsonProperty("line")]
        public LineResponse Line { get; set; }

        [JsonProperty("routes")]
        public List<RouteResponse> Routes { get; set; }
    }

    public class RouteResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("line_id")]
        public int LineId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("ext_id")]
        public string ExtId { get; set; }

        [JsonProperty("route_ref")]
        public int RouteRef { get; set; }

        [JsonProperty("details")]
        public Details Details { get; set; }

        [JsonProperty("segments")]
        public List<Segment> Segments { get; set; }
    }

    public class Segment
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("route_id")]
        public int RouteId { get; set; }

        [JsonProperty("sequence")]
        public int Sequence { get; set; }

        [JsonProperty("start_stop_id")]
        public int StartStopId { get; set; }

        [JsonProperty("end_stop_id")]
        public int EndStopId { get; set; }

        [JsonProperty("polyline")]
        public string Polyline { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }

        [JsonProperty("stop")]
        public StopSchResponse Stop { get; set; }

        [JsonProperty("end_stop")]
        public EndStop EndStop { get; set; }

        [JsonProperty("priority")]
        public int? Priority { get; set; }
    }

    public class StopSchResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("ext_id")]
        public string ExtId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("is_active")]
        public int IsActive { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("times")]
        public List<TimeResponse> Times { get; set; }
    }

    public class TimeResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("weekend")]
        public int Weekend { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("item_id")]
        public int ItemId { get; set; }

        [JsonProperty("route_id")]
        public int RouteId { get; set; }

        [JsonProperty("stop_id")]
        public int StopId { get; set; }

        [JsonProperty("secondary")]
        public bool? Secondary { get; set; }
    }


}
