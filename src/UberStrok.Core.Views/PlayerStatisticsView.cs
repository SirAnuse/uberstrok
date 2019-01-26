using System;
using System.Text;

namespace UberStrok.Core.Views
{
	[Serializable]
	public class PlayerStatisticsView
	{
		public PlayerStatisticsView()
		{
            PersonalRecord = new PlayerPersonalRecordStatisticsView();
            WeaponStatistics = new PlayerWeaponStatisticsView();
		}

		public PlayerStatisticsView(int cmid, int splats, int splatted, long shots, long hits, int headshots, int nutshots, PlayerPersonalRecordStatisticsView personalRecord, PlayerWeaponStatisticsView weaponStatistics)
		{
            Cmid = cmid;
            Xp = 0;
            Level = 0;

            /* THE GREAT LINE OF UBERSTRIKE-DOESNT-CARE-ABOUT-THESE BEGINS */
            // These values appear to be calculated manually on the clientside.
            // As such, there is no use in setting them, but they must be sent to the client.
            Hits = hits;
            Shots = shots;
			Splats = splats;
			Splatted = splatted;
			Headshots = headshots;
			Nutshots = nutshots;
			/* THE GREAT LINE OF UBERSTRIKE-DOESNT-CARE-ABOUT-THESE ENDS */

			PersonalRecord = personalRecord;
			WeaponStatistics = weaponStatistics;
		}

		public PlayerStatisticsView(int cmid, int splats, int splatted, long shots, long hits, int headshots, int nutshots, int xp, int level, PlayerPersonalRecordStatisticsView personalRecord, PlayerWeaponStatisticsView weaponStatistics)
		{
			Cmid = cmid;
			Level = level;
			Xp = xp;

            /* THE GREAT LINE OF UBERSTRIKE-DOESNT-CARE-ABOUT-THESE BEGINS */
            // These values appear to be calculated manually on the clientside.
            // As such, there is no use in setting them, but they must be sent to the client.
            Hits = hits;
            Shots = shots;
            Splats = splats;
            Splatted = splatted;
            Headshots = headshots;
            Nutshots = nutshots;
            /* THE GREAT LINE OF UBERSTRIKE-DOESNT-CARE-ABOUT-THESE ENDS */

            PersonalRecord = personalRecord;
			WeaponStatistics = weaponStatistics;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.Append("[PlayerStatisticsView: ");
			builder.Append("[Cmid: ");
			builder.Append(Cmid);
			builder.Append("][Hits: ");
			builder.Append(Hits);
			builder.Append("][Level: ");
			builder.Append(Level);
			builder.Append("][Shots: ");
			builder.Append(Shots);
			builder.Append("][Splats: ");
			builder.Append(Splats);
			builder.Append("][Splatted: ");
			builder.Append(Splatted);
			builder.Append("][Headshots: ");
			builder.Append(Headshots);
			builder.Append("][Nutshots: ");
			builder.Append(Nutshots);
			builder.Append("][Xp: ");
			builder.Append(Xp);
			builder.Append("]");
			builder.Append(PersonalRecord);
			builder.Append(WeaponStatistics);
			builder.Append("]");
			return builder.ToString();
		}

		public int Cmid { get; set; }
		public int Headshots { get; set; }
		public long Hits { get; set; }
		public int Level { get; set; }
		public int Nutshots { get; set; }
		public PlayerPersonalRecordStatisticsView PersonalRecord { get; set; }
		public long Shots { get; set; }
		public int Splats { get; set; }
		public int Splatted { get; set; }
		public int TimeSpentInGame { get; set; }
		public PlayerWeaponStatisticsView WeaponStatistics { get; set; }
		public int Xp { get; set; }
	}
}
