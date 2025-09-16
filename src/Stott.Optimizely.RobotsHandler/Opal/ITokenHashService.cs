namespace Stott.Optimizely.RobotsHandler.Opal;

public interface ITokenHashService
{
    string HashToken(string token, string saltValue);

    bool VerifyToken(string token, string hash, string saltValue);
}
