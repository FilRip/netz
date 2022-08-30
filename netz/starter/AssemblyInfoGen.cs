using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;

namespace netz.starter
{

    public class AssemblyInfoGen
    {
        private static Assembly lastEXE = null;

        public AssemblyInfoGen()
        { }

        public static string MakeAssemblyInfo(string file, GenData genData)
        {
            return MakeAssemlbyInfo(Assembly.ReflectionOnlyLoadFrom(file), genData);
        }

        private static void GenStringAttrib(ref StringBuilder sb, string name, string val)
        {
            val = val.Replace("\\", "\\\\");
            val = val.Replace("\t", "\\t");
            val = val.Replace("\r", "\\r");
            val = val.Replace("\n", "\\n");
            val = val.Replace("\"", "\\\"");
            GenAttrib(ref sb, name, "\"" + val + "\"");
        }

        private static void GenAttrib(ref StringBuilder sb, string name, string val)
        {
            string NL = Environment.NewLine;
            sb.Append("[assembly: " + name + "(");
            sb.Append(val);
            sb.Append(")]").Append(NL);
        }

        public static string MakeAssemlbyInfo(Assembly a, GenData genData)
        {
            if (a == null) return null;
            lastEXE = a;
            bool versionSet = false;
            string NL = Environment.NewLine;
            StringBuilder sb = new StringBuilder();
            sb.Append("using System.Reflection;").Append(NL);
            sb.Append("using System.Runtime.CompilerServices;").Append(NL);
            sb.Append("using System.Runtime.Versioning;").Append(NL);
            if (genData.Obfuscateur)
                sb.Append("[assembly: System.Runtime.CompilerServices.SuppressIldasm()]").Append(NL);
            string valeur;
            foreach (CustomAttributeData att in a.CustomAttributes)
            {
                Type typeAttribut = att.AttributeType;
                valeur = "";
                if ((att.ConstructorArguments != null) && (att.ConstructorArguments.Count > 0) && (att.ConstructorArguments[0].Value != null))
                    valeur = att.ConstructorArguments[0].Value.ToString();
                if (typeAttribut == typeof(AssemblyTitleAttribute))
                {
                    GenStringAttrib(ref sb, "AssemblyTitle",
                        valeur);
                }
                else if (typeAttribut == typeof(AssemblyDescriptionAttribute))
                {
                    GenStringAttrib(ref sb, "AssemblyDescription",
                        valeur);
                }
                else if (typeAttribut == typeof(AssemblyConfigurationAttribute))
                {
                    GenStringAttrib(ref sb, "AssemblyConfiguration",
                        valeur);
                }
                else if (typeAttribut == typeof(AssemblyCompanyAttribute))
                {
                    GenStringAttrib(ref sb, "AssemblyCompany",
                        valeur);
                }
                else if (typeAttribut == typeof(AssemblyProductAttribute))
                {
                    GenStringAttrib(ref sb, "AssemblyProduct",
                        valeur);
                }
                else if (typeAttribut == typeof(AssemblyCopyrightAttribute))
                {
                    GenStringAttrib(ref sb, "AssemblyCopyright",
                        valeur);
                }
                else if (typeAttribut == typeof(AssemblyTrademarkAttribute))
                {
                    GenStringAttrib(ref sb, "AssemblyTrademark",
                        valeur);
                }
                else if (typeAttribut == typeof(AssemblyCultureAttribute))
                {
                    GenStringAttrib(ref sb, "AssemblyCulture",
                        valeur);
                }
                else if (typeAttribut == typeof(AssemblyVersionAttribute))
                {
                    versionSet = true;
                    GenStringAttrib(ref sb, "AssemblyVersion",
                        valeur);
                }
                else if (typeAttribut == typeof(AssemblyKeyFileAttribute))
                {
                    if (genData.KeyGetFromAttributes)
                    {
                        GenStringAttrib(ref sb, "AssemblyKeyFile",
                            valeur);
                    }
                }
                else if (typeAttribut == typeof(AssemblyKeyNameAttribute))
                {
                    if (genData.KeyGetFromAttributes)
                    {
                        GenStringAttrib(ref sb, "AssemblyKeyName",
                            valeur);
                    }
                }
                else if (typeAttribut == typeof(AssemblyAlgorithmIdAttribute))
                {
                    if (genData.KeyGetFromAttributes)
                    {
                        GenAttrib(ref sb, "AssemblyAlgorithmId",
                            valeur);
                    }
                }
                else if (typeAttribut == typeof(AssemblyDelaySignAttribute))
                {
                    if (genData.KeyGetFromAttributes)
                    {
                        GenAttrib(ref sb, "AssemblyDelaySign",
                            valeur);
                    }
                }
                else if (typeAttribut == typeof(TargetFrameworkAttribute))
                {
                    GenStringAttrib(ref sb, "TargetFramework",
                        valeur);
                }

                #region removed
                /*
                else if(att is System.Reflection.AssemblyDefaultAliasAttribute)
                {
                    GenStringAttrib(ref sb, "AssemblyDefaultAlias",
                        ((System.Reflection.AssemblyDefaultAliasAttribute)att).DefaultAlias);
                }
                else if(att is System.Reflection.AssemblyFileVersionAttribute)
                {
                    GenStringAttrib(ref sb, "AssemblyFileVersion",
                        ((System.Reflection.AssemblyFileVersionAttribute)att).Version);
                }
                else if(att is System.Reflection.AssemblyInformationalVersionAttribute)
                {
                    GenStringAttrib(ref sb, "AssemblyInformationalVersion",
                        ((System.Reflection.AssemblyInformationalVersionAttribute)att).InformationalVersion);
                }
                else if(att is System.Reflection.AssemblyFlagsAttribute)
                { // int
                    GenAttrib(ref sb, "AssemblyFlags",
                        ((AssemblyFlagsAttribute)att).Flags.ToString("D"));
                }
                else if(att is System.Diagnostics.DebuggableAttribute)
                {
                    System.Diagnostics.DebuggableAttribute da = (System.Diagnostics.DebuggableAttribute)att;
                    string dav = string.Empty + (da.IsJITTrackingEnabled ? "true" : "false") + "," + (da.IsJITOptimizerDisabled ? "true" : "false");
                    GenAttrib(ref sb, "System.Diagnostics.DebuggableAttribute", dav);
                }
                else if(att is System.CLSCompliantAttribute)
                {
                    GenAttrib(ref sb, "System.CLSCompliantAttribute", ((System.CLSCompliantAttribute)att).IsCompliant ? "true" : "false");
                }
                else if(att is System.Runtime.InteropServices.GuidAttribute)
                {
                    GenStringAttrib(ref sb, "System.Runtime.InteropServices.GuidAttribute", ((System.Runtime.InteropServices.GuidAttribute)att).Value);
                }
                */

                #endregion removed
                else
                {
                    string t = att.ToString();
                    try
                    {
                        string prefix = "System.Reflection.";
                        if (t.StartsWith(prefix)) t = t.Substring(prefix.Length, t.Length - prefix.Length);
                    }
                    catch { }
                    if (!MatchUserDefined(t, genData.UserAssemblyAttributes))
                    {
                        if (genData.ReportEXEAttributes)
                        {
                            Netz.PrintWarning("1003 Unhandled main assembly attribute : " + t + " ?", null);
                        }
                    }
                    else
                    {
                        Logger.Log("! Matched user defined attribute name       : " + t);
                    }
                }
            }
            if (!versionSet)
            {
                string[] data = a.FullName.Split(',');
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == null) continue;
                    string ver = data[i].Trim(',', ' ').ToLower();
                    if (ver.StartsWith("version"))
                    {
                        int j = data[i].IndexOf('=');
                        if (j > 0)
                        {
                            ver = ver.Substring(j, ver.Length - j);

                            GenStringAttrib(ref sb, "AssemblyVersion",
                                ver);
                            versionSet = true;
                        }
                    }
                }
            }
            if (!versionSet)
            {
                sb.Append("[assembly: AssemblyVersion(\"");
                sb.Append("1.0.*");
                sb.Append("\")]").Append(NL);
            }
            if (!genData.KeyGetFromAttributes)
            {
                bool keySet = false;
                if (genData.KeyFile != null)
                {
                    keySet = true;
                    GenStringAttrib(ref sb, "AssemblyKeyFile",
                        genData.KeyFile);
                }
                if (genData.KeyName != null)
                {
                    keySet = true;
                    GenStringAttrib(ref sb, "AssemblyKeyName",
                        genData.KeyName);
                }
                if (keySet)
                {
                    GenAttrib(ref sb, "AssemblyDelaySign",
                        (genData.KeyDelay ? "true" : "false"));
                }
            }
            sb.Append(NL).Append("// Add any other attributes here").Append(NL);
            return sb.ToString();
        }

        private static bool MatchUserDefined(string att, string userDefined)
        {
            string t = att;
            int i = t.LastIndexOf('.');
            if ((i >= 0) && (i < (t.Length - 1)))
            {
                t = t.Substring(i + 1, t.Length - i - 1);
            }
            return (userDefined.IndexOf(t + "(") >= 0);
        }

        public static string GetFullDLLName(string file)
        {
            Assembly dll = Assembly.LoadFrom(file);
            return dll.FullName.Trim();
        }

        public static string MakeAssemblyLicense(string file)
        {
            Assembly a = lastEXE; // reuse
            if (a == null) a = Assembly.LoadFrom(file);
            string[] resources = a.GetManifestResourceNames();
            string licenseFile = Path.GetFileName(file) + ".licenses";
            licenseFile = licenseFile.ToUpper();
            if ((resources == null) || (resources.Length <= 0)) return null;
            for (int i = 0; i < resources.Length; ++i)
            {
                if (resources[i].ToUpper().Equals(licenseFile))
                {
                    Stream data = null;
                    FileStream lstr = null;
                    try
                    {
                        data = a.GetManifestResourceStream(resources[i]);
                        if (data == null) return null;
                        licenseFile = OutDirMan.MakeOutFileName(licenseFile.ToLower());
                        lstr = new FileStream(licenseFile, FileMode.Create,
                            FileAccess.Write, FileShare.None, 4096);
                        byte[] buff = new byte[4096];
                        while (true)
                        {
                            int c = data.Read(buff, 0, buff.Length);
                            if (c <= 0) break;
                            lstr.Write(buff, 0, c);
                        }
                    }
                    finally
                    {
                        if (data != null) data.Close();
                        if (lstr != null) lstr.Close();
                    }
                    return licenseFile;
                }
            }
            return null;
        }
    }
}
