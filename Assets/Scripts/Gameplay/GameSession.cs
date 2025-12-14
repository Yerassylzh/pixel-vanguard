using System;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Represents a single game session's runtime statistics.
    /// Created when game starts, finalized when player dies.
    /// </summary>
    [Serializable]
    public class GameSession
    {
        public float survivalTime;
        public int killCount;
        public int levelReached;
        public int goldCollected;
        public DateTime sessionDate;

        public GameSession()
        {
            survivalTime = 0f;
            killCount = 0;
            levelReached = 1;
            goldCollected = 0;
            sessionDate = DateTime.Now;
        }

        /// <summary>
        /// Format survival time as MM:SS
        /// </summary>
        public string GetFormattedTime()
        {
            int minutes = (int)(survivalTime / 60f);
            int seconds = (int)(survivalTime % 60f);
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}
