using System;
using UnityEngine;
using GamesKeystoneFramework.Core.Interface;
using System.Collections.Generic;
namespace GamesKeystoneFramework.Input
{
    public class InputSystemBase : MonoBehaviour, IInputInterface<(Action, float)>
    {
        public List<(Action, float)> InputBuffer { get; set; } = new List<(Action, float)> ();
        public bool CanInput { get; set; } = false;
        public float TypeAhead { get; set; } = 0;
    }
}
