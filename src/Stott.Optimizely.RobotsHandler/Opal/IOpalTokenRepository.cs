using System;
using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Opal;

public interface IOpalTokenRepository
{
    List<TokenModel> List();

    void Save(TokenModel saveModel);

    void Delete(Guid id);

    TokenModel GetByToken(string token);
}