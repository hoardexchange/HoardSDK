using Hoard.BC;
using Hoard.Interfaces;
using System;

namespace Hoard.GameItemProviders
{
    /// <summary>
    /// </summary>
    public class GameItemProviderFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="bcComm"></param>
        /// <returns></returns>
        public static IGameItemProvider CreateSecureProvider(GameID game, IBCComm bcComm)
        {
            if (bcComm is PlasmaComm)
                return new PlasmaGameItemProvider(game, bcComm);
            else if (bcComm is BCComm)
                return new BCGameItemProvider(game, bcComm); 
            else
                throw new NotSupportedException();
        }
    }
}
