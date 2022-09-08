using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[CreateAssetMenu]
public class TimelineControllable : PlayableAsset
{

    public Color Color_A;
    public Color Color_B;
    public float power;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        throw new System.NotImplementedException();
    }
    // Start is called before the first frame update

}
