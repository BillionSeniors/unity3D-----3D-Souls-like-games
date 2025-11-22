using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QHZ
{
    public abstract class State : MonoBehaviour
    {
        public abstract State Tick(AICharacterManager aiCharacter);
    }
}