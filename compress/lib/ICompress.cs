namespace netz.compress
{
    public interface ICompress
    {
        long Compress(string src, string dst);
        string GetRedistributableDLLPath();
        string GetHeadTemplate();
        string GetBodyTemplate();
        void AjoutParametres(string parametres);
    }
}
