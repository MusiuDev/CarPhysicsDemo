using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [SerializeField] private int _lookAhead = 3;
    [SerializeField] private float _maxMapAngle = 60f;

    private CheckpointGroup[] _checkpointPrefabs;

    private TrackChain _currentChain;
    private List<CheckpointGroup> _activeGroups = new List<CheckpointGroup>();

    void Awake()
    {

    }

    void Start()
    {
        _checkpointPrefabs = Resources.LoadAll<CheckpointGroup>("CheckpointGroups");
        if (_checkpointPrefabs == null || _checkpointPrefabs.Length == 0)
        {
            Debug.LogError("No checkpoint groups found in resources.");
        }
        _currentChain = new TrackChain();

        for (int i = 0; i < 10; i++)
        {
            TrySpawn();
        }
    }

    void Update()
    {

    }

    public void TrySpawn()
    {
        if (_checkpointPrefabs == null || _checkpointPrefabs.Length == 0)
        {
            Debug.LogError("Trying to spawn checkpoints without valid prefabs");
            return;
        }

        (CheckpointGroup group, bool flipped, TrackChainLink newLink) = TryChoosePrefabWithLookAhead(_lookAhead);
        if (!group || newLink == null)
        {
            Debug.LogError("Spawner wasn't able to find a valid link. Picking one at random");
            flipped = Random.value > 0.5f;
            group = _checkpointPrefabs[Random.Range(0, _checkpointPrefabs.Length - 1)];
            newLink = group.GetTrackChainLinkAt(_currentChain.exitPosition, _currentChain.exitAngle, flipped);
        }

        Quaternion rotation = Quaternion.Euler(0, _currentChain.exitAngle, 0);
        Vector3 position = _currentChain.exitPosition;
        CheckpointGroup newActiveGroup = GameObject.Instantiate(group, position, rotation, this.transform);
        if (flipped) newActiveGroup.Flip();

        float oldAngle = _currentChain.exitAngle;

        _currentChain.Add(newLink);
        _activeGroups.Add(newActiveGroup);
        Debug.Log($"Spawned new group at: {position:F2} with angle {oldAngle} - to a new angle: {_currentChain.exitAngle}");
    }

    private (CheckpointGroup group, bool flipped, TrackChainLink newLink) TryChoosePrefabWithLookAhead(int lookAhead)
    {
        TrackChain virtualChain = _currentChain.Clone();
        CheckpointGroup pickedGroup = null;
        bool pickedFlipped = false;
        TrackChainLink pickedLink = null;
        for (int i = 0; i < lookAhead; i++)
        {
            (CheckpointGroup group, bool flipped, TrackChainLink newLink) = TryChoosePrefab(virtualChain);

            //early return with null if any depth is invalid.
            if (!group || newLink == null) return (null, false, null);

            virtualChain.Add(newLink);

            //if this is the first check, this will be the actual result
            // but we still need to look further.
            if (i == 0)
            {
                pickedGroup = group;
                pickedFlipped = flipped;
                pickedLink = newLink;
            }
        }

        return (pickedGroup, pickedFlipped, pickedLink);
    }

    private (CheckpointGroup group, bool flipped, TrackChainLink newLink) TryChoosePrefab(TrackChain chain)
    {
        List<CheckpointGroup> shuffledGroups = _checkpointPrefabs.OrderBy(a => Random.value).ToList();
        Vector3 currentExitPos = chain.exitPosition;
        float currentExitAngle = chain.exitAngle;

        foreach (var candidate in shuffledGroups)
        {
            bool flipped = Random.value > 0.5f;
            TrackChainLink newLink = candidate.GetTrackChainLinkAt(currentExitPos, currentExitAngle, flipped);

            bool collides = chain.CheckCollision(newLink);

            if (Mathf.Abs(newLink.exitAngle) <= _maxMapAngle && !collides)
            {
                return (candidate, flipped, newLink);
            }
        }
        return (null, false, null);
    }
}

public class TrackChainLink
{
    public Vector3 entryPosition;
    public Vector3 exitPosition;
    public float exitAngle;
    public RotatedRectangle bounds;

    public TrackChainLink(Vector3 entryPosition, Vector3 exitPosition, float exitAngle, RotatedRectangle bounds)
    {
        this.entryPosition = entryPosition;
        this.exitPosition = exitPosition;
        this.exitAngle = exitAngle;
        this.bounds = bounds;
    }
}

public class TrackChain
{
    private List<TrackChainLink> _links;
    public TrackChainLink First => _links.Count > 0 ? _links[0] : null;
    public TrackChainLink Last => _links.Count > 0 ? _links[^1] : null;

    public Vector3 exitPosition => Last != null ? Last.exitPosition : Vector3.zero;
    public float exitAngle => Last != null ? Last.exitAngle : 0;

    public TrackChain()
    {
        _links = new();
    }

    public TrackChain(List<TrackChainLink> links)
    {
        this._links = links;
    }

    public void Add(TrackChainLink newLink)
    {
        _links.Add(newLink);
    }

    public bool CheckCollision(TrackChainLink newLink)
    {
        foreach (var link in _links)
        {
            bool collides = RectangleIntersection.CheckIntersection(newLink.bounds, link.bounds);
            if (collides) return true;
        }
        return false;
    }

    public TrackChain Clone()
    {
        return new TrackChain(new List<TrackChainLink>(_links));
    }
}