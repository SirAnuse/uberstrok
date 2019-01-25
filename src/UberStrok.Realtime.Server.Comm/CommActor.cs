using System;

namespace UberStrok.Realtime.Server.Comm
{
    public class CommActor : Actor
    {
        /* Current room the actor is in. */
        private readonly CommPeer _peer;

        public CommPeer Peer => _peer;
        public new LobbyRoom Room => (LobbyRoom)base.Room;

        public CommActor(CommPeer peer)
        {
            if (peer == null)
                throw new ArgumentNullException(nameof(peer));

            _peer = peer;
        }

        public override Command Receive()
        {
            return null;
        }

        public override void Send(Command command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            /* TODO: Encode and send through peer. */
        }
    }
}
