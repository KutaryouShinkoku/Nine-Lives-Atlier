using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WanFramework.Sequence
{
    internal class CallingChainSequencePlaying : SequencePlaying<CallingChainSequencePlaying>
    {
        public CallingChainSequencePlaying(IEnumerable<Action<CallingChainSequencePlaying, Action>> actions) 
            : base(actions.Select(WaitForCallback).ToArray())
        {
        }
    }
    public static class SequenceHelper
    {
        public static ISequencePlaying BuildCallingChain(IEnumerable<Action<Action>> actions)
        {
            var converted = actions.Select(action =>
                (Action<CallingChainSequencePlaying, Action>)((c, callback) => action.Invoke(callback)));
            return new CallingChainSequencePlaying(converted);
        }

        public static ISequencePlaying BuildCallingChain(params Action<Action>[] actions)
        {
            return BuildCallingChain(actions.AsEnumerable());
        }
        
        public static ISequencePlaying BuildPlayingChain(Behaviour owner, params ISequencePlaying[] playings)
        {
            return BuildCallingChain(playings.Select(playing =>
                (Action<Action>)(callback => playing.Play(callback, owner))));
        }
    }
}