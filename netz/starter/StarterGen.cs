using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

using Microsoft.CSharp;

namespace netz.starter
{
    public class StarterGen
    {
        private readonly GenData genData = null;

        public StarterGen(GenData genData)
        {
            this.genData = genData;
        }

        public void Make(string resourceFile)
        {
            string starterTemplate = GetStarterTemplate();
            if (genData.MtaAttribute)
            {
                starterTemplate = starterTemplate.Replace("[STAThread()]", "[MTAThread()]");
            }
            starterTemplate = starterTemplate.Replace("// This is a template. Do not remove //# strings", string.Empty);
            starterTemplate = SetStarterVersion(starterTemplate);
            starterTemplate = SetNETVersion(starterTemplate);
            starterTemplate = AppendPrivatePath(starterTemplate);
            starterTemplate = RemoveZipCode(starterTemplate);
            starterTemplate = SetZipTemplate(starterTemplate, genData);
            starterTemplate = OptimizeSelf(starterTemplate);
            starterTemplate = SetEntryPoint(starterTemplate);
            starterTemplate = starterTemplate.Replace("//#EXTENSION", genData.Extension);
            if (genData.Console)
            {
                starterTemplate = starterTemplate.Replace(
                    "using System.Windows.Forms;", string.Empty);
                starterTemplate = starterTemplate.Replace(
                    "MessageBox.Show(null, s, \"Error\")", "Console.WriteLine(s)");
            }
            if (genData.BatchMode)
            {
                BatchMode(genData.Console, starterTemplate);
            }
            else
            {
                CodeCompileUnit cu = new CodeSnippetCompileUnit(MixTemplate(starterTemplate, genData.AssemblyInfo));
                CompileCU(genData.Console, resourceFile, cu);
            }
        }

        private string SetEntryPoint(string starterTemplate)
        {
            if (genData.IsService)
            {
                string serviceData = Properties.Resources.serviceData;
                serviceData = ProcessServiceTemplate(serviceData);
                starterTemplate = starterTemplate.Replace("//#NTSERVICE", serviceData);
                return starterTemplate.Replace("//#MAINENTRYPOINT",
                    @"System.ServiceProcess.ServiceBase.Run(new System.ServiceProcess.ServiceBase[] { new NetzService(args) });
                    return 0;");
            }
            return starterTemplate.Replace("//#MAINENTRYPOINT", "return StartApp(args);");
        }

        private string ProcessServiceTemplate(string serviceData)
        {
            string defName = "\"netz service " + DateTime.Now.Ticks.ToString("X") + "\"";
            serviceData = ReplaceServiceTemplateParams(serviceData, "servicename", "//#NTS_NAME", defName, true);
            serviceData = ReplaceServiceTemplateParams(serviceData, "displayname", "//#NTS_DISP_NAME", defName, true);
            serviceData = ReplaceServiceTemplateParams(serviceData, "username", "//#NTS_USER", "null", true);
            serviceData = ReplaceServiceTemplateParams(serviceData, "password", "//#NTS_PASS", "null", true);
            serviceData = ReplaceServiceTemplateParams(serviceData, "helptext", "//#NTS_HELP", "string.Empty", true);
            serviceData = ReplaceServiceTemplateParams(serviceData, "starttype", "//#NTS_START_TYPE", "System.ServiceProcess.ServiceStartMode.Automatic", false);
            return serviceData;
        }

        private string ReplaceServiceTemplateParams(string serviceData, string id, string templateId, string defaultValue, bool quote)
        {
            string val = (string)genData.serviceParams[id];
            if (val != null)
            {
                if (quote) val = "\"" + val + "\"";
            }
            else
            {
                val = defaultValue;
            }
            return serviceData.Replace(templateId, val);
        }

        private string GetStarterTemplate()
        {
            return Properties.Resources.data;
        }

        private string SetStarterVersion(string starterTemplate)
        {
            string ver = string.Empty;
            if (genData.SetNetzVersion)
            {
                ver = "_" + Utils.NetzVersion.Replace('.', '_');
            }
            return starterTemplate.Replace("//#VER", ver);
        }

        private string SetNETVersion(string starterTemplate)
        {
            return starterTemplate.Replace("//#CTNETVER", "\"" + Environment.Version.ToString() + "\"");
        }

        private string OptimizeSelf(string starterTemplate)
        {
            if (genData.SingleExe && genData.Optimize && (genData.PrivatePath == null))
            {
                starterTemplate = RemoveRegion(starterTemplate, "#region probe", "#endregion probe");
                starterTemplate = RemoveRegion(starterTemplate, "//#OPSSTART", "//#OPSEND");
            }
            return starterTemplate;
        }

        private string RemoveRegion(string starterTemplate, string start, string end)
        {
            int i = starterTemplate.IndexOf(start);
            if (i >= 0)
            {
                int j = starterTemplate.IndexOf(end);
                if (j >= 0)
                {
                    j += end.Length;
                    string temp = starterTemplate.Substring(0, i);
                    temp += starterTemplate.Substring(j, starterTemplate.Length - j);
                    starterTemplate = temp;
                }
            }
            return starterTemplate;
        }

        private string AppendPrivatePath(string starterTemplate)
        {
            if (genData.PrivatePath != null)
            {
                string[] paths = genData.PrivatePath.Split(';');
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < paths.Length; i++)
                {
                    string t = paths[i].Trim(' ', ';');
                    if (t.Length <= 0) continue;
                    if (Path.IsPathRooted(t))
                    {
                        throw new Exception("E1012 Absolute private paths are not allowed: " + t);
                    }
                    if (i != 0) sb.Append('\t', 3);
                    sb.Append("currentDomain.AppendPrivatePath(\"" + t + "\");");
                    if (i != (paths.Length - 1))
                        sb.Append(Environment.NewLine);
                }
                starterTemplate = starterTemplate.Replace("//#PATH",
                    sb.ToString());
            }
            return starterTemplate;
        }

        private string RemoveZipCode(string starterTemplate)
        {
            if (genData.PackZipDll && (genData.ZipDllName != null))
            {
                return SetZipDllName(starterTemplate);
            }
            starterTemplate = RemoveZipCommentedCode(starterTemplate);
            starterTemplate = RemoveZipCommentedCode(starterTemplate);
            return starterTemplate;
        }

        private string SetZipDllName(string starterTemplate)
        {
            if (genData.ZipDllName == null) return starterTemplate;
            return starterTemplate.Replace("//#ZIPDLL", Path.GetFileNameWithoutExtension(genData.ZipDllName).ToLower());

        }

        private static string RemoveZipCommentedCode(string starterTemplate)
        {
            string ZIPSTART = "//#ZIPSTART";
            string ZIPEND = "//#ZIPEND";
            int i = starterTemplate.IndexOf(ZIPSTART);
            if (i <= 0) return starterTemplate;
            int j = starterTemplate.IndexOf(ZIPEND, i);
            if (j <= 0) return starterTemplate;
            return starterTemplate.Remove(i, j + ZIPEND.Length - i);
        }

        private static string SetZipTemplate(string starterTemplate, GenData genData)
        {
            string ZIPHEAD = "//#ZIPHEAD";
            string ZIPBODY = "//#ZIPBODY";
            string head = genData.CompressProvider.Provider.GetHeadTemplate();
            string body = genData.CompressProvider.Provider.GetBodyTemplate();
            if (head != null) starterTemplate = starterTemplate.Replace(ZIPHEAD, head);
            if (body != null) starterTemplate = starterTemplate.Replace(ZIPBODY, body);
            return starterTemplate;
        }

        private string MixTemplate(string starterTempl, string assemblyInfo)
        {
            if (assemblyInfo == null) return starterTempl;
            assemblyInfo = assemblyInfo.Replace("using System.Reflection;", "");
            int i = starterTempl.IndexOf("//#AI");
            string temp = starterTempl.Insert(i + 7, assemblyInfo);
            if ((genData.UserAssemblyAttributes != null) && (!genData.UserAssemblyAttributes.Equals(string.Empty)))
            {
                i = temp.IndexOf("//#UAI");
                temp = temp.Insert(i + 8, genData.UserAssemblyAttributes);
            }
            return temp;
        }

        private void BatchMode(bool console, string starterTemplate)
        {
            starterTemplate = "// .NETZ Version " + Utils.NetzVersion + Environment.NewLine + starterTemplate;
            string compile = "csc /target:" + (console ? "exe" : "winexe")
                + (genData.XPlatform != null ? (" /platform:" + genData.XPlatform) : string.Empty)
                + " /out:\"" + genData.ExeFileName + "\""
                + " starter.cs AssemblyInfo.cs /res:app.resources /r:\"" + genData.ZipDllName + "\"";
            if (genData.IconFile != null) compile += MakeIconArg(false);
            if (genData.LicenseResourceFile != null) compile += MakeLicArg(false);
            if (genData.Optimize)
                compile += " -optimize+ -checked-";
            if (genData.Obfuscateur)
                compile += " -debug-";
            if (genData.AutoriseCodeUnsafe)
                compile += " -unsafe+";
            if (!string.IsNullOrWhiteSpace(genData.ManifestResourceFile))
                compile += " -win32manifest:" + genData.ManifestResourceFile;
            if ((genData.OtherCompOptions != null) && (genData.OtherCompOptions.Length > 0))
            {
                compile += " " + genData.OtherCompOptions;
            }
            Utils.WriteTextFile("makefile.bat", compile);
            if (genData.AssemblyInfo != null)
            {
                string temp = genData.AssemblyInfo;
                if (genData.UserAssemblyAttributes != null)
                    temp += genData.UserAssemblyAttributes;
                Utils.WriteTextFile("AssemblyInfo.cs", temp);
            }
            Utils.WriteTextFile("starter.cs", starterTemplate);
        }

        private void CompileCU(bool console, string resourceFile, CodeCompileUnit cu)
        {
            CSharpCodeProvider p = new CSharpCodeProvider();
            CompilerParameters options = new CompilerParameters();
            options.ReferencedAssemblies.Add(genData.ZipDllName);
            // required
            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Core.dll");
            if (genData.IsService)
            {
                options.ReferencedAssemblies.Add("System.Configuration.Install.dll");
                options.ReferencedAssemblies.Add("System.ServiceProcess.dll");
            }
            if (!console)
            {
                options.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            }
            options.GenerateExecutable = true;
            options.OutputAssembly = OutDirMan.MakeOutFileName(genData.ExeFileName);
            options.CompilerOptions += "/res:\"" + resourceFile + "\"";
            if (!console)
            {
                options.CompilerOptions += " /target:winexe";
            }
            if (genData.XPlatform != null)
            {
                options.CompilerOptions += " /platform:" + genData.XPlatform;
            }
            if (genData.IconFile != null)
            {
                options.CompilerOptions += MakeIconArg(true);
            }
            if (genData.LicenseResourceFile != null)
            {
                options.CompilerOptions += MakeLicArg(true);
            }
            if ((genData.OtherCompOptions != null) && (genData.OtherCompOptions.Length > 0))
            {
                options.CompilerOptions += " " + genData.OtherCompOptions;
            }
            if (genData.Optimize)
            {
                options.CompilerOptions += " -optimize+ -checked-";
            }
            if (genData.Obfuscateur)
            {
                options.IncludeDebugInformation = false;
                options.CompilerOptions += " -debug-";
            }
            if (genData.AutoriseCodeUnsafe)
            {
                options.CompilerOptions += " -unsafe+";
            }
            if (!string.IsNullOrWhiteSpace(genData.ManifestResourceFile))
            {
                options.CompilerOptions += " -win32manifest:" + genData.ManifestResourceFile;
            }
            CompilerResults cr = p.CompileAssemblyFromDom(options, cu);
            if (cr.Errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                Logger.Log("\r\n# NetzStarter compilation errors:\r\n");
                foreach (CompilerError err in cr.Errors)
                {
                    string t = err.ErrorNumber + " " + err.ErrorText + " " + err.FileName + " L:" + err.Line + " C:" + err.Column;
                    sb.Append(t).Append(Environment.NewLine);
                    Logger.Log("  " + t);
                }
                sb.Append(Environment.NewLine);
                sb.Append(((CodeSnippetCompileUnit)cu).Value);
                if (sb.Length > 0)
                {
                    try
                    {
                        string errFile = "error.txt";
                        sb.Append(Environment.NewLine).Append(".NETZ Version: ").Append(System.Reflection.Assembly.GetExecutingAssembly().FullName);
                        sb.Append(Environment.NewLine).Append(".NET  Version: ").Append(Environment.Version.ToString());
                        sb.Append(Environment.NewLine).Append(InputParser.GetInnerTemplatesFingerPrint(this.genData.CompressProviderDLL));
                        sb.Append(Environment.NewLine).Append(Environment.StackTrace);
                        sb.Append(Environment.NewLine).Append(DateTime.Now.ToString());
                        sb.Append(Environment.NewLine);

                        Utils.WriteTextFile(errFile, sb.ToString());
                        Logger.Log(string.Empty);
                        errFile = OutDirMan.MakeOutFileName("error.txt");
                        ColorConsole c = null;
                        try
                        {
                            c = new ColorConsole();
                            c.SetColor(ColorConsole.FOREGROUND_RED | ColorConsole.FOREGROUND_INTENSITY);
                        }
                        catch { }

                        Logger.Log("! Please report the error file to filrip@gmail.com : " + errFile);

                        Logger.Log(string.Empty);
                        Logger.Log("! To help locate better the errors add the -v option to");
                        Logger.Log("! your current netz command-line and email also the screen output!");
                        Logger.Log("! No support is provided for custom startup and compress templates!");
                        Logger.Log("! Check http://madebits.com/netz/ for more help on how to report a bug!");

                        try
                        {
                            if (c != null) c.Reset();
                        }
                        catch { }
                    }
                    catch { }
                }
            }
        }

        private string MakeIconArg(bool includePath)
        {
            string icon = genData.IconFile;
            if (!includePath)
                icon = Path.GetFileName(icon);
            return " /win32icon:\"" + icon + "\"";
        }

        private string MakeLicArg(bool includePath)
        {
            string icon = genData.LicenseResourceFile;
            if (!includePath)
                icon = Path.GetFileName(icon);
            return " /res:\"" + icon + "\"";
        }

    }
}
