using System;
using System.Linq;

using Confuser.Core;
using Confuser.Core.Project;

namespace netz.obfuscateurs
{
    public class Obfuscateur
    {
        public static void Obfuscation(string nomAssembly)
        {
            MonLogger logger = new MonLogger();
            ConfuserParameters confuserParameters = new ConfuserParameters();
            ProjectModule project = new ProjectModule();
            Rule rule = new Rule
            {
                Preset = (ProtectionPreset)Netz.genData.NiveauObfuscation,
            };
            project.Path = System.IO.Path.GetFileName(nomAssembly);
            project.Rules.Add(rule);
            ConfuserProject projects = new ConfuserProject()
            {
                BaseDirectory = System.IO.Path.GetDirectoryName(nomAssembly),
                OutputDirectory = System.IO.Path.GetDirectoryName(nomAssembly) + @"\Confused",
            };
            projects.ProbePaths.Add(System.IO.Path.GetDirectoryName(typeof(Obfuscateur).Assembly.Location));

            projects.Add(project);
            confuserParameters.Project = projects;
            confuserParameters.Logger = logger;
            ConfuserEngine.Run(confuserParameters).Wait();
        }

        private class MonLogger : ILogger
        {
            public void Debug(string msg)
            {
#if DEBUG
                Console.WriteLine(msg);
#endif
            }

            public void DebugFormat(string format, params object[] args)
            {
#if DEBUG
                Console.WriteLine(format, args);
#endif
            }

            public void EndProgress()
            {
            }

            public void Error(string msg)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Erreur : " + msg);
                Console.ForegroundColor = ConsoleColor.White;
            }

            public void ErrorException(string msg, Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exception : " + msg);
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                Console.ForegroundColor = ConsoleColor.White;
            }

            public void ErrorFormat(string format, params object[] args)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Erreur : " + format, args);
                Console.ForegroundColor = ConsoleColor.White;
            }

            public void Finish(bool successful)
            {
                Console.WriteLine("Terminé" + (successful ? "" : " avec erreur(s)"));
            }

            public void Info(string msg)
            {
                Console.WriteLine("Info : " + msg);
            }

            public void InfoFormat(string format, params object[] args)
            {
                Console.WriteLine("Info : " + format, args);
            }

            public void Progress(int progress, int overall)
            {
            }

            public void Warn(string msg)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning : " + msg);
                Console.ForegroundColor = ConsoleColor.White;
            }

            public void WarnException(string msg, Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning : " + msg);
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                Console.ForegroundColor = ConsoleColor.White;
            }

            public void WarnFormat(string format, params object[] args)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning : " + format, args);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static string RandomString(int taille = 10)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, taille).Select(s => s[new Random(Guid.NewGuid().GetHashCode()).Next(s.Length)]).ToArray());
        }
    }
}
