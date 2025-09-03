// Serializable helper (colócalo en un archivo Utilities/SerializableVector3.cs)
[System.Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }

    public SerializableVector3(UnityEngine.Vector3 v) { x = v.x; y = v.y; z = v.z; }

    public UnityEngine.Vector3 ToVector3() => new UnityEngine.Vector3(x, y, z);
}
