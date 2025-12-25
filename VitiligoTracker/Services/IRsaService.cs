namespace VitiligoTracker.Services
{
    public interface IRsaService
    {
        string GetPublicKey();
        string Decrypt(string encryptedText);
    }
}
