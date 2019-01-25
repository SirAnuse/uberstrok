using System.Runtime.CompilerServices;

namespace UberStrok
{
    public abstract class Command
    {
        /* Tick at which the command was enqueued in a Game's command queue. */
        internal int _tick;
        /* Game the command was enqueued in. */
        internal GameWorld _game;

        public int Tick => _tick;
        public GameWorld Game => _game;


        protected abstract void OnExecute();

        /* Wrapper. */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void DoExecute() => OnExecute();
    }
}
