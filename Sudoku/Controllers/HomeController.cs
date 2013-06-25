using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Sudoku.Models;
using SudokuSolver2010;


namespace Sudoku.Controllers
{
    public class HomeController : Controller
    {
        //failsafe - currently 10sec as total time to sovle one puzzle
        static readonly int TimeoutMillis = int.Parse(ConfigurationManager.AppSettings["solverTimeoutInMilliseconds"]);


        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            //StorageTraceListener.Instance.TraceRecord(TraceEventType.Error, filterContext.Exception.Message, "AllUsers", filterContext.Exception.ToString());
        }

        // default page 
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Solves a puzzle
        /// handles either GET or POST
        /// Use GET for a single puzzle and POST for batch
        /// </summary>
        /// <returns></returns>
        public ActionResult Solve()
        {
            SudokuSolutionModel model = new SudokuSolutionModel();
            bool useJson = Request.HttpMethod == "POST";

            //for demo - trace only if trace message is on
            var toTrace = Request.QueryString["trace"] ?? new StreamReader(Request.InputStream).ReadToEnd();
            if (toTrace != null && System.String.CompareOrdinal(toTrace, "1")==0)
            {
                System.Diagnostics.Trace.TraceInformation(string.Format("Server name {0}", Server.MachineName));
            }


            var puzzleText = Request.QueryString["puzzle"] ?? new StreamReader(Request.InputStream).ReadToEnd();

            //handle puzzle input error
            if (string.IsNullOrEmpty(puzzleText))
            {
                model.Error = "You need to provide a puzzle either via a query parameter for a GET in the format '?puzzle=<puzzle> or via the content for a POST where the format of the puzzle is like '.94...13..............76..2.8..1.....32.........2...6.....5.4.......8..7..63.4..8' where . represents a missing value and the values are added from left to right, starting from the top and working down.";
                if(useJson== true)
                {
                    return (ActionResult)Json(model);
                }
                else {
                    return (ActionResult)View(model);
                }
            }
            else
            {
                puzzleText = puzzleText.Replace(".", "0");
            }

            // start solving the puzzle 
            SudokuPuzzle puzzle;

            try
            {
                puzzle = PuzzleParser.ParsePuzzle(puzzleText);
            }
            catch (Exception )
            {
                model.Error = "Not a valid puzzle: " + puzzleText;
                return useJson ? (ActionResult)Json(model) : (ActionResult)View(model);
            }

            model = Solve(new SudokuPuzzle[] { puzzle });

            if (model.Solutions[0].Solution == null)
            {
                model.Error = "We had a problem finding a solution to that puzzle.";
                if (model.Solutions[0].Error != null)
                {
                    model.Error += " --------------> "+ model.Solutions[0].Error;
                }
            }

            return useJson ? (ActionResult)Json(model) : (ActionResult)View(model);
        }

        public ActionResult SolveBatch()
        {
            SudokuSolutionModel model = new SudokuSolutionModel();
            SudokuPuzzle[] puzzles;

            //var puzzleText = new StreamReader(Request.InputStream).ReadToEnd();
            var puzzleText = HttpContext.Request.Params["puzzle"];
            if (string.IsNullOrEmpty(puzzleText))
            {
                model.Error = "bad input - wrong puzzle batch format";
                return Json(model, JsonRequestBehavior.AllowGet);
            }

            try
            {
                puzzles = PuzzleParser.ParsePuzzles(puzzleText);
            }
            catch (Exception ex)
            {
                return Json(new SudokuSolutionModel() { Error = "Could not parse puzzles: "+ex.ToString(), });
            }

            var ret = Solve(puzzles);
            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="puzzles"></param>
        /// <returns></returns>
        private SudokuSolutionModel Solve(SudokuPuzzle[] puzzles)
        {
            SudokuSolutionModel ret = new SudokuSolutionModel();
            ret.Solutions = new SudokuSolution[puzzles.Length];

            Stopwatch outerSW = new Stopwatch();
            var solver = new Action(() =>
            {
                outerSW.Start();
                Parallel.ForEach(puzzles, (puz) =>
                {
                    Stopwatch sw = new Stopwatch();
                    var inputString = ToBasicString(puz);

                    try
                    {
                        var heuristics = new SudokuHeuristics(puz);

                        //StorageTraceListener.Instance.TraceRecord(TraceEventType.Verbose, "About to solve puzzle", "AllUsers", inputString);

                        sw.Start();
                        var solution = SudokuSolver.DefaultSolver.Solve(puz, heuristics);
                        sw.Stop();

                        bool isValid, isComplete;
                        SudokuValidator.Validate(solution, out isValid, out isComplete);
                        var solutionString = ToBasicString(solution);

                        var result = new SudokuSolution
                            {
                            PuzzleText = inputString,
                            SolutionText = solutionString,
                            SolveTimeInMilliseconds = sw.Elapsed.TotalMilliseconds,
                            Solution = solution,
                            IsValid = isValid,
                            IsComplete = isComplete,
                        };

                        ret.Solutions[puz.OriginalIndex] = result;
                        sw.Reset();
                    }
                    catch (Exception ex)
                    {
                        //TraceInformation(string.Format("Server name {0}", Server.MachineName));
                        System.Diagnostics.Trace.TraceError(string.Format("Exception while solving puzzle {0}| exception {1}", inputString, ex.Message));

                        ret.Solutions[puz.OriginalIndex] = new SudokuSolution {Error = ex.ToString()};
                    }
                });
                outerSW.Stop();
            });

            int isDone = 0;
            Exception asyncError = null;
            solver.BeginInvoke(res =>
            {
                try
                {
                    solver.EndInvoke(res);
                }
                catch (Exception asyncEx)
                {
                    asyncError = asyncEx;
                }
                finally
                {
                    Interlocked.Add(ref isDone, 1);
                }
            }, null);

            DateTime start = DateTime.UtcNow;
            while (isDone == 0)
            {
                TimeSpan soFar = DateTime.UtcNow - start;
                if (soFar.TotalMilliseconds > TimeoutMillis)
                {
                    asyncError = new TimeoutException("Stopped trying to solve after " + TimeoutMillis + " ms");
                    break;
                }
                Thread.Sleep(25);
            }
            ret.Error = asyncError != null ? asyncError.ToString() : null;
            ret.NumberOfPuzzles = puzzles.Length;
            ret.TotalSolveTimeInMilliseconds = outerSW.Elapsed.TotalMilliseconds;

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="puzzle"></param>
        /// <returns></returns>
        private static string ToBasicString(SudokuPuzzle puzzle)
        {
            var ret = "";
            for (int i = 0; i < puzzle.Width; i++)
            {
                for (int j = 0; j < puzzle.Width; j++)
                {
                    ret += puzzle.Cells[i][j].Value;
                }
            }

            return ret;
        }
    }
}
