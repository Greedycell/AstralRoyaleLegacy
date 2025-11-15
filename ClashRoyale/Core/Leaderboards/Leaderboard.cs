using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ClashRoyale.Database;
using ClashRoyale.Files;
using ClashRoyale.Files.CsvLogic;
using ClashRoyale.Logic;
using ClashRoyale.Logic.Clan;
using ClashRoyale.Logic.Home;
using ClashRoyale.Utilities;
using SharpRaven.Data;

namespace ClashRoyale.Core.Leaderboards
{
    public class Leaderboard
    {
        private readonly Timer _timer = new Timer(20000);

        public List<Alliance> GlobalAllianceRanking = new List<Alliance>(200);
        public List<Player> GlobalPlayerRanking = new List<Player>(18);
        public Dictionary<string, List<Player>> LocalPlayerRanking = new Dictionary<string, List<Player>>(18);

        public Leaderboard()
        {
            _timer.Elapsed += Update;
            _timer.AutoReset = true;
            _timer.Start();

            foreach (var locales in Csv.Tables.Get(Csv.Files.Locales).GetDatas())
                LocalPlayerRanking.Add(((Locales) locales).Name, new List<Player>(999));

            Update(null, null);
        }

        /// <summary>
        ///     Update all Leaderboards
        /// </summary>
        /// <param name="state"></param>
        /// <param name="args"></param>
        public async void Update(object state, ElapsedEventArgs args)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var currentGlobalPlayerRanking = await PlayerDb.GetGlobalPlayerRankingAsync();
                    for (var i = 0; i < currentGlobalPlayerRanking.Count; i++)
                        GlobalPlayerRanking.UpdateOrInsert(i, currentGlobalPlayerRanking[i]);

                    foreach (var (key, value) in LocalPlayerRanking)
                    {
                        var currentLocalPlayerRanking = await PlayerDb.GetLocalPlayerRankingAsync(key);
                        for (var i = 0; i < currentLocalPlayerRanking.Count; i++)
                            value.UpdateOrInsert(i, currentLocalPlayerRanking[i]);
                    }

                    var currentGlobalAllianceRanking = await AllianceDb.GetGlobalAlliancesAsync();
                    for (var i = 0; i < currentGlobalAllianceRanking.Count; i++)
                        GlobalAllianceRanking.UpdateOrInsert(i, currentGlobalAllianceRanking[i]);                                     
                }
                catch (Exception exception)
                {
                    Logger.Log($"Error while updating leaderboads {exception}", GetType(), ErrorLevel.Error);
                }
            });
        }
        public int GetPlayerRankingById(int id)
        {
            Player playerToFind = GlobalPlayerRanking.FirstOrDefault(player => player.Id == id);

            if (playerToFind != null)
            {
                int playerIndex = GlobalPlayerRanking.IndexOf(playerToFind);
                return playerIndex + 1;
            }
            return -1;
        }
        public int GetPlayerLocalRankingById(int id)
        {
            foreach (var kvp in LocalPlayerRanking)
            {
                Player playerToFind = kvp.Value.FirstOrDefault(player => player.Id == id);

                if (playerToFind != null)
                {
                    int playerIndex = kvp.Value.IndexOf(playerToFind);
                    return playerIndex + 1;
                }
            }
            return -1;
        }
    }
}