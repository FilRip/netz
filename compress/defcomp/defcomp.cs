using System.IO;

using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

public class DefComp : netz.compress.ICompress
{

    private readonly string head = null;
    private readonly string body = null;

    // a default, no parameter constructor is required
    public DefComp()
    {
        head = defcomp.Properties.Resources.head;
        body = defcomp.Properties.Resources.body;
    }

    // netz.compress.ICompress implementation

    public long Compress(string file, string zipFile)
    {
        long length = -1;
        FileStream ifs = null;
        FileStream ofs = null;
        try
        {
            ifs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            ofs = File.Open(zipFile, FileMode.Create, FileAccess.Write, FileShare.None);
            DeflaterOutputStream dos = new DeflaterOutputStream(ofs, new Deflater(Deflater.BEST_COMPRESSION));
            byte[] buff = new byte[ifs.Length];
            while (true)
            {
                int r = ifs.Read(buff, 0, buff.Length);
                if (r <= 0) break;
                dos.Write(buff, 0, r);
            }
            dos.Flush();
            dos.Finish();
            length = dos.Length;
            dos.Close();
        }
        finally
        {
            if (ifs != null) ifs.Close();
            if (ofs != null) ofs.Close();
        }
        return length;
    }

    // return null if none required
    public string GetRedistributableDLLPath()
    {
        return "zip.dll";
    }

    public string GetHeadTemplate()
    {
        return head;
    }

    public string GetBodyTemplate()
    {
        return body;
    }

    public void AjoutParametres(string parametres)
    {
    }
}
