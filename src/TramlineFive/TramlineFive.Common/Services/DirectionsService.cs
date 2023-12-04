using KdTree.Math;
using Mapsui.UI.Maui;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Noding;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Serialization;
using SkgtService;
using SkgtService.Models;
using SkgtService.Models.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TramlineFive.Common.Models;

namespace TramlineFive.Common.Services;

public class DirectionsService
{
    private readonly PublicTransport publicTransport;
    private readonly IApplicationService applicationService;
    private BidirectionalGraph<Node, Edge<Node>> graph = new();
    private Dictionary<Edge<Node>, float> costs = new();

    private GeoMath math = new();

    public DirectionsService(PublicTransport publicTransport)
    {
        this.publicTransport = publicTransport;
        //this.applicationService = ServiceContainer.ServiceProvider.GetService<IApplicationService>();
    }

    public class Node : IEquatable<Node>
    {
        public StopInformation Stop { get; set; }
        public LineInformation Line { get; set; }

        public Node(StopInformation stop, LineInformation line = null)
        {
            Stop = stop;
            Line = line;
        }

        public override int GetHashCode()
        {
            if (Line == null)
                return Stop.GetHashCode();

            return Stop.GetHashCode() + Line.GetHashCode();
        }

        public bool Equals(Node other)
        {
            if (Line != null && other.Line != null)
                return Stop.Code == other.Stop.Code && Line.Name == other.Line.Name && Line.VehicleType == other.Line.VehicleType;

            if ((Line != null && other.Line == null) || (Line == null && other.Line != null))
                return false;

            return Stop.Code == other.Stop.Code;
        }
    }

    public async Task BuildAsync()
    {
        await Task.Run(() =>
        {
            Build();
        });
    }

    public void Build()
    {
        foreach (var type in publicTransport.Lines)
        {
            foreach (var line in type.Value)
            {
                foreach (var route in line.Value.Routes)
                {
                    for (int i = 0; i < route.Codes.Count; ++i)
                    {
                        if (i + 1 < route.Codes.Count)
                        {
                            var stop = publicTransport.FindStop(route.Codes[i]);
                            var anotherStopInfo = publicTransport.FindStop(route.Codes[i + 1]);

                            List<Node> localNodesStop = BuildNode(stop);
                            List<Node> localNodesAnotherStop = BuildNode(anotherStopInfo);
                            
                            // build nodes for every route of every line
                            foreach (var currentStop in localNodesStop)
                            {
                                foreach (var anotherStop in localNodesAnotherStop)
                                {
                                    // line not null means not starting or ending trip or walking 
                                    if (currentStop.Line != null && anotherStop.Line != null && currentStop.Line.Name == anotherStop.Line.Name)
                                    {
                                        var edge = new Edge<Node>(currentStop, anotherStop);
                                        float distance = (float)Math.Sqrt(math.DistanceSquaredBetweenPoints(new float[] { (float)stop.Lat, (float)stop.Lon }, 
                                            new float[] { (float)anotherStopInfo.Lat, (float)anotherStopInfo.Lon }));

                                        graph.AddVerticesAndEdge(edge);

                                        // cost is the distance between the stops
                                        costs.Add(edge, distance);
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

        // build nodes for when you walk from one stop to a different one to catch a different bus
        foreach (StopInformation stop in publicTransport.Stops)
        {
            foreach (StopInformation anotherStop in publicTransport.Stops)
            {
                // if the stop is different, that means the user has to walk from one stop to another
                if (anotherStop != stop)
                {
                    if (anotherStop.Lines.Count == 0)
                        continue;

                    float distance = (float)Math.Sqrt(math.DistanceSquaredBetweenPoints(new float[] { (float)stop.Lat, (float)stop.Lon }, new float[] { (float)anotherStop.Lat, (float)anotherStop.Lon }));

                    if (float.IsNaN(distance))
                        distance = 0;

                    // Прекачване пеша до 300м
                    if (distance < 0.3 /*&& !stop.Lines.Any(a => anotherStop.Lines.Contains(a))*/)
                    {
                        List<Node> nodes = BuildNode(anotherStop);
                        foreach (Node node in nodes)
                        {
                            var edge = new Edge<Node>(new Node(stop), node);
                            graph.AddVerticesAndEdge(edge);

                            costs.Add(edge, distance * 5f);
                        }
                    }
                }
                else
                {
                    // Прекачване на същата спирка
                    List<Node> nodes = BuildNode(stop);
                    foreach (Node node in nodes)
                    {
                        foreach (Node sameNode in nodes)
                        {
                            // if line from start node is null covers 2 cases:
                            // * when a user starts trip goes from null to a line
                            // * when a user gets off bus (line to null) and then changes bus (null to line)
                            if (node.Line == null)
                            { 
                                var edge = new Edge<Node>(node, sameNode);
                                graph.AddVerticesAndEdge(edge);

                                costs.Add(edge, 0.05f);
                            }
                            else if (sameNode.Line == null)
                            {
                                var edge = new Edge<Node>(node, sameNode);
                                graph.AddVerticesAndEdge(edge);

                                costs.Add(edge, 0.05f);
                            }
                            //else if (node.Line != sameNode.Line && node.Line != null && sameNode.Line != null)
                            //{
                            //    // 
                            //    var edge = new Edge<Node>(node, sameNode);
                            //    graph.AddEdge(edge);

                            //    costs.Add(edge, 0.5f);
                            //}
                        }
                    }
                }
            }
        }
    }

    public List<DirectionsStep> GetShortestPath(StopInformation from, StopInformation to)
    {
        var searchFunction = graph.ShortestPathsDijkstra((s) =>
        {
            return costs[s];
        }, new Node(from));

        if (searchFunction(new Node(to), out IEnumerable<Edge<Node>> path))
        {
            var edge = graph.Edges.Where(e => e.Source.Stop.Code == "0849" && e.Target.Stop.Code == "6644");
            foreach (var a in edge)
                Console.WriteLine($"{a.Source.Line?.Name} - {a.Target.Line?.Name} {costs[a]}");

            return path.Select(p => new DirectionsStep
            {
                FromStop = p.Source.Stop,
                FromLine = p.Source.Line,
                ToStop = p.Target.Stop,
                ToLine = p.Target.Line
            }).ToList();
        }

        return new List<DirectionsStep>();

        //var edge = graph.Edges.Where(e => e.Source.Stop.Code == "0889" && e.Target.Stop.Code == "0876");
        //foreach (var a in edge)
        //    Console.WriteLine($"{a.Source.Line?.Name} - {a.Target.Line?.Name} {costs[a]}");

        //Console.WriteLine("0636");
        //var edgea = graph.Edges.Where(e => e.Source.Stop.Code == "0876" && e.Target.Stop.Code == "0636");
        //foreach (var a in edgea)
        //    Console.WriteLine($"{a.Source.Line?.Name} - {a.Target.Line?.Name} {costs[a]}");
    }

    private List<Node> BuildNode(StopInformation stop)
    {
        List<Node> result = new List<Node>();
        foreach (var line in stop.Lines)
        {
            if (line.Name[0] == 'N')
                continue;

            // 800 lines do not work on weekends
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                if (line.VehicleType == TransportType.Additional)
                    continue;
            }
            // 103 does only work on weekends
            else if (line.Name == "103")
                continue;

            result.Add(new Node(stop, line));
        }

        result.Add(new Node(stop, null));

        return result;
    }
}
