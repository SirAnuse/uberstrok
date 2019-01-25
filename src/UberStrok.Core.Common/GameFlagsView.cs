using System;
namespace UberStrok.Core.Common
{
    public class GameFlagsView
    {
        public bool LowGravity
        {
            get
            {
                return IsFlagSet(GameFlags.LowGravity);
            }
            set
            {
                SetFlag(GameFlags.LowGravity, value);
            }
        }

        public bool NoArmor
        {
            get
            {
                return IsFlagSet(GameFlags.NoArmor);
            }
            set
            {
                SetFlag(GameFlags.NoArmor, value);
            }
        }

        public bool QuickSwitch
        {
            get
            {
                return IsFlagSet(GameFlags.QuickSwitch);
            }
            set
            {
                SetFlag(GameFlags.QuickSwitch, value);
            }
        }

        public bool MeleeOnly
        {
            get
            {
                return IsFlagSet(GameFlags.MeleeOnly);
            }
            set
            {
                SetFlag(GameFlags.MeleeOnly, value);
            }
        }

        public bool DefenseBonus
        {
            get
            {
                return IsFlagSet(GameFlags.DefenseBonus);
            }
            set
            {
                SetFlag(GameFlags.DefenseBonus, value);
            }
        }

        public static bool IsFlagSet(GameFlags flag, int state)
        {
            return (state & (int)flag) != 0;
        }

        public int GetFlags()
        {
            return (int)gameFlags;
        }

        private bool IsFlagSet(GameFlags f)
        {
            return (gameFlags & f) == f;
        }

        private void SetFlag(GameFlags f, bool b)
        {
            gameFlags = ((!b) ? (gameFlags & ~f) : (gameFlags | f));
        }

        public void SetFlags(int flags)
        {
            gameFlags = (GameFlags)flags;
        }

        public void ResetFlags()
        {
            gameFlags = GameFlags.None;
        }

        private GameFlags gameFlags;

        [Flags]
        public enum GameFlags
        {
            None = 0,
            LowGravity = 1,
            NoArmor = 2,
            QuickSwitch = 4,
            MeleeOnly = 8,
            DefenseBonus = 16
        }
    }
}
