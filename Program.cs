// Copyright (C) 2016 Maxim Gumin, The MIT License (MIT)

using System;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;

static class Program
{
    
    static void Main()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var folder = System.IO.Directory.CreateDirectory("output");
        foreach (var file in folder.GetFiles()) file.Delete();

        Random random = new(324234);
        XDocument xdoc = XDocument.Load("samples.xml");

        foreach (XElement xelem in xdoc.Root.Elements("overlapping", "simpletiled"))
        {
            Model model;
            string name = xelem.Get<string>("name");
            Console.WriteLine($"< {name}");

            bool isOverlapping = xelem.Name == "overlapping";
            int size = xelem.Get("size", isOverlapping ? 48 : 24);
            int width = xelem.Get("width", size);
            int height = xelem.Get("height", size);
            bool periodic = xelem.Get("periodic", false);
            string heuristicString = xelem.Get<string>("heuristic");
            var heuristic = heuristicString == "Scanline" ? Model.Heuristic.Scanline : (heuristicString == "MRV" ? Model.Heuristic.MRV : Model.Heuristic.Entropy);

            if (isOverlapping)
            {
                int N = xelem.Get("N", 3);
                bool periodicInput = xelem.Get("periodicInput", true);
                int symmetry = xelem.Get("symmetry", 8);
                bool ground = xelem.Get("ground", false);

                model = new OverlappingModel(name, N, width, height, periodicInput, periodic, symmetry, ground, heuristic);
            }
            else
            {
                string subset = xelem.Get<string>("subset");
                bool blackBackground = xelem.Get("blackBackground", false);

                model = new SimpleTiledModel(name, subset, width, height, periodic, blackBackground, heuristic);
            }

            int successfulRuns = 0;
            double totalTime = 0;

            for (int i = 0; i < 30/*xelem.Get("screenshots", 2)*/; i++)
            {
                for (int k = 0; k < 10; k++)
                {
                    Console.Write("> ");
                    int seed = random.Next();

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    bool success = model.Run(seed, xelem.Get("limit", -1));
                    stopwatch.Stop(); 

                    if (success)
                    {
                        double elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                        totalTime += elapsedMilliseconds;
                        successfulRuns++;

                        Console.WriteLine($"DONE ({elapsedMilliseconds:F2} ms)");

                        model.Save($"output/{name} {seed}.png");

                        if (model is SimpleTiledModel stmodel && xelem.Get("textOutput", false))
                            System.IO.File.WriteAllText($"output/{name} {seed}.txt", stmodel.TextOutput());

                        break;
                    }
                    else
                    {
                        Console.WriteLine("CONTRADICTION");
                        stopwatch.Reset();
                    }
                }
            }

            if (successfulRuns > 0)
            {
                double averageTime = totalTime / successfulRuns;
                string className = model is SimpleTiledModel ? "Simpletiled" : "Overlapping";
                Console.WriteLine($"\nСреднее время успешных запусков для {className}: {averageTime:F2} мс");
            }
            else
            {
                Console.WriteLine("\nНе было успешных запусков.");
            }
            /*using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                //writer.WriteLine($"name = {name}");
                //writer.WriteLine($"time stopWatchClear_first3for = {model.stopWatchClear_first3for.Elapsed.TotalMilliseconds * 1000000}");
                //writer.WriteLine($"time stopWatchClear_secondfor = {model.stopWatchClear_secondfor.Elapsed.TotalMilliseconds * 1000000}");
                //writer.WriteLine($"time stopWatchPropagate_for_in_for = {model.stopWatchPropagate_for_in_for.Elapsed.TotalMilliseconds * 1000000}");
                //writer.WriteLine($"time stopWatchNextUnobservedNode_if = {model.stopWatchNextUnobservedNode_if.Elapsed.TotalMilliseconds * 1000000}");
                //writer.WriteLine($"time = stopWatchNextUnobservedNode_for {model.stopWatchNextUnobservedNode_for.Elapsed.TotalMilliseconds * 1000000}");
                //writer.WriteLine($"time = stopWatchRun_3for {model.stopWatchRun_3for.Elapsed.TotalMilliseconds * 1000000}");
                //writer.WriteLine($"time = stopWatchInit_for_in_for {model.stopWatchInit_for_in_for.Elapsed.TotalMilliseconds * 1000000}");
            }

            Console.WriteLine("Данные записаны в файл: " + filePath);*/
        }

        Console.WriteLine($"time = {sw.ElapsedMilliseconds}");
    }
}
