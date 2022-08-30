namespace dnlib.DotNet.Pdb
{
    static class PdbUtils
    {
        public static bool IsEndInclusive(PdbFileKind pdbFileKind, Compiler compiler) =>
            pdbFileKind == PdbFileKind.WindowsPDB && compiler == Compiler.VisualBasic;
    }
}
