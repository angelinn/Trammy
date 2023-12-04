
using QuikGraph.Algorithms.MaximumFlow;
using SkgtService;
using SkgtService.Models;
using System.Diagnostics;
using System.IO;
using System.Text;
using TramlineFive.Common.Services;

StopsLoader.Initialize(".");

PublicTransport pb = new PublicTransport();

Console.WriteLine("Loading data...");
pb.LoadData().Wait();

DirectionsService directions = new DirectionsService(pb);

Stopwatch sw = new Stopwatch();
sw.Start();

Console.WriteLine("Building...");
directions.Build();

sw.Stop();

Console.WriteLine($"Build took {sw.Elapsed} time");

sw.Reset();
sw.Start();

Console.WriteLine("Finding path...");
Console.OutputEncoding = Encoding.UTF8;
var path = directions.GetShortestPath(pb.FindStop("2193"), pb.FindStop("2327")).ToList();

List<LineInformation> previousBuses = null;
LineInformation singleActive = null;
float walkingDistance = 0;
string previousStop = "";

foreach (var line in path)
{
    List<LineInformation> currentLine = pb.FindLineByTwoStops(line.FromStop.Code, line.ToStop.Code);
    List<LineInformation> sameLines = new List<LineInformation>();
    if (previousBuses != null)
    {
        if (currentLine != null)
        {
            sameLines = currentLine.Where(l => previousBuses.Contains(l)).ToList();
        }
        else
            sameLines = previousBuses;
    }

    string lineNames = "";
    if (sameLines.Count == 0)
    {
        if (previousStop == line.FromStop.Code && currentLine.Count > 0)
            lineNames = string.Join(", ", currentLine.Select(s => s.VehicleType + " " + s.Name));
        else
            lineNames = $"Пеша {walkingDistance / 2} км";
    }

    else
    {
        if (sameLines.Count > 1 && singleActive != null && sameLines.Contains(singleActive))
        {
            sameLines.Clear();
            sameLines.Add(singleActive);
        }
        else if (sameLines.Count == 1)
            singleActive = sameLines[0];

        lineNames = string.Join(", ", sameLines.Select(s => s.VehicleType + " " + s.Name));
    }

    previousBuses = currentLine;
    previousStop = line.ToStop.Code;

    //Console.WriteLine(costs[line]);

    //Console.Write($" [{lineNames}]");
    //Console.WriteLine();

    string realLine = line.FromLine == null ? "Пеша" : line.FromLine.Name;
    if (line == path.First())
        realLine = "Начало";
    if (line.FromLine == null && line.ToLine != null)
        realLine = "Качване на " + line.ToLine.Name;
    else if (line.FromLine != null && line.ToLine == null)
        realLine = "Слизане от " + line.FromLine.Name;

    Console.WriteLine($"[{realLine}] {pb.FindStop(line.FromStop.Code).PublicName} - {line.FromStop.Code} to {pb.FindStop(line.ToStop.Code).PublicName} - {line.ToStop.Code}");

}
//foreach (var shortestPath in res)
//Console.WriteLine($"{pb.FindStop(shortestPath.Source.Code).PublicName} - {shortestPath.Source.Code} to {pb.FindStop(shortestPath.Target.Code).PublicName} - {shortestPath.Target.Code}");

sw.Stop();

Console.WriteLine($"Search took {sw.Elapsed} time");

