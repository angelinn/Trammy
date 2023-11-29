using KdTree.Math;
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
            if (Line != null)
                return Stop.Code == other.Stop.Code && Line.Name == other.Line.Name && Line.VehicleType == other.Line.VehicleType;

            return Stop.Code == other.Stop.Code;
        }
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
                            var anotherStop = publicTransport.FindStop(route.Codes[i + 1]);

                            List<Node> localNodesStop = BuildNode(stop);
                            List<Node> localNodesAnotherStop = BuildNode(anotherStop);
                            foreach (var currentLine in localNodesStop)
                            {
                                foreach (var anotherNode in localNodesAnotherStop)
                                {
                                    if (currentLine.Line != null && anotherNode.Line != null && currentLine.Line.Name == anotherNode.Line.Name)
                                    {
                                        var edge = new Edge<Node>(currentLine, anotherNode);
                                        float distance = (float)Math.Sqrt(math.DistanceSquaredBetweenPoints(new float[] { (float)stop.Lat, (float)stop.Lon }, new float[] { (float)anotherStop.Lat, (float)anotherStop.Lon }));
                                        graph.AddVerticesAndEdge(edge);

                                        // if you have to change vehicles add cost
                                        if (currentLine.Line?.Name != anotherNode.Line?.Name)
                                            costs.Add(edge, distance * 1.1f);
                                        else
                                            costs.Add(edge, distance);

                                        // walking distance indicated by null line should be more cost
                                        var emptyEdge = new Edge<Node>(currentLine, new Node(anotherNode.Stop));
                                        graph.AddVerticesAndEdge(emptyEdge);
                                        costs[emptyEdge] = distance * 2;


                                        var emptyEdgeStart = new Edge<Node>(new Node(currentLine.Stop), anotherNode);
                                        graph.AddVerticesAndEdge(emptyEdgeStart);
                                        costs[emptyEdgeStart] = distance * 2;
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

        foreach (StopInformation stop in publicTransport.Stops)
        {
            foreach (StopInformation anotherStop in publicTransport.Stops)
            {
                if (anotherStop != stop)
                {
                    if (anotherStop.Lines.Count == 0)
                        continue;

                    float distance = (float)Math.Sqrt(math.DistanceSquaredBetweenPoints(new float[] { (float)stop.Lat, (float)stop.Lon }, new float[] { (float)anotherStop.Lat, (float)anotherStop.Lon }));

                    if (float.IsNaN(distance))
                        distance = 0;

                    if (distance < 0.3 && !stop.Lines.Any(a => anotherStop.Lines.Contains(a)))
                    {
                        List<Node> nodes = BuildNode(anotherStop);
                        foreach (Node node in nodes)
                        {
                            var edge = new Edge<Node>(new Node(stop), node);
                            graph.AddVerticesAndEdge(edge);

                            costs.Add(edge, distance * 5);
                        }
                    }
                }
            }
        }
    }

    public IEnumerable<Edge<Node>> GetShortestPath(StopInformation from, StopInformation to)
    {
        var searchFunction = graph.ShortestPathsDijkstra((s) =>
        {
            return costs[s];
        }, new Node(from));

        searchFunction(new Node(to), out var path);

        //var edge = graph.Edges.Where(e => e.Source.Stop.Code == "0889" && e.Target.Stop.Code == "0876");
        //foreach (var a in edge)
        //    Console.WriteLine($"{a.Source.Line?.Name} - {a.Target.Line?.Name} {costs[a]}");

        //Console.WriteLine("0636");
        //var edgea = graph.Edges.Where(e => e.Source.Stop.Code == "0876" && e.Target.Stop.Code == "0636");
        //foreach (var a in edgea)
        //    Console.WriteLine($"{a.Source.Line?.Name} - {a.Target.Line?.Name} {costs[a]}");


        return path;
    }

    private List<Node> BuildNode(StopInformation stop)
    {
        List<Node> result = new List<Node>();
        foreach (var line in stop.Lines)
        {
            if (line.Name[0] == 'N')
                continue;

            result.Add(new Node(stop, line));
        }

        result.Add(new Node(stop, null));

        return result;
    }
}
