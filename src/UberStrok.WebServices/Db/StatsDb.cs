using Newtonsoft.Json;
using System;
using System.IO;
using UberStrok.Core.Views;
using log4net;

namespace UberStrok.WebServices.Db
{
    public class StatsDb
    {
        private const string ROOT_DIR = "data/stats";
        private readonly static ILog Log = LogManager.GetLogger(typeof(StatsDb).Name);

        public StatsDb()
        {
            if (!Directory.Exists(ROOT_DIR))
                Directory.CreateDirectory(ROOT_DIR);
        }

        public PlayerStatisticsView Load(int cmid)
        {
            if (cmid <= 0)
                throw new ArgumentException("CMID must be greater than 0.");

            var path = Path.Combine(ROOT_DIR, cmid + ".json");
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<PlayerStatisticsView>(json);
        }

        public void Save(PlayerStatisticsView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            var cmid = view.Cmid;
            if (cmid <= 0)
                throw new ArgumentException("CMID must be greater than 0.");

            var path = Path.Combine(ROOT_DIR, cmid + ".json");
            var json = JsonConvert.SerializeObject(view, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}
