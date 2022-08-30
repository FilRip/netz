using System.IO;
using System.IO.Compression;

public class Net20Comp : netz.compress.ICompress
{

    private readonly string head = null;
    private readonly string body = null;

    // a default, no parameter constructor is required
    public Net20Comp()
    {
        head = net20comp.Properties.Resources.head;
        body = net20comp.Properties.Resources.body;
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
            DeflateStream dos = new DeflateStream(ofs, CompressionMode.Compress, true);
            byte[] buff = new byte[ifs.Length];
            while (true)
            {
                int r = ifs.Read(buff, 0, buff.Length);
                if (r <= 0) break;
                dos.Write(buff, 0, r);
            }
            dos.Flush();
            dos.Close();
            length = ofs.Length;
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
        return null;
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
