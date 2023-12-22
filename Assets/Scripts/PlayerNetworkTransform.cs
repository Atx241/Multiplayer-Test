using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
namespace Unity.Netcode.Components
{
    public class PlayerNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
