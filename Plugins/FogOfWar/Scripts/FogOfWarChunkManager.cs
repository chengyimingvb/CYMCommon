using UnityEngine;
using System.Collections.Generic;

namespace FoW
{
    class FogOfWarChunk
    {
        public Vector3i coordinate;
        public byte[] fogData;
        public byte[] currentFogData;
    }

    [AddComponentMenu("FogOfWar/FogOfWarChunkManager")]
    [RequireComponent(typeof(FogOfWar))]
    public class FogOfWarChunkManager : MonoBehaviour
    {
        public Transform followTransform;
        public bool rememberFog = true;
        public float verticalChunkSize = 10;
        public float verticalChunkOffset = 0;

        List<FogOfWarChunk> _chunks = new List<FogOfWarChunk>();
        public int loadedChunkCount { get { return _chunks.Count; } }
        Vector3i _loadedChunk;

        FogOfWar _fogOfWar;
        int _mapResolution;
        int _valuesPerMap { get { return _mapResolution * _mapResolution; } }
        int _valuesPerChunk { get { return _valuesPerMap / 4; } }
        Vector3 _followPosition { get { return FogOfWarConversion.WorldToFogPlane3(followTransform.position, _fogOfWar.plane); } }

        void Start()
        {
            _fogOfWar = GetComponent<FogOfWar>();
            if (_fogOfWar.mapResolution.x != _fogOfWar.mapResolution.y)
            {
                Debug.LogError("FogOfWarChunkManager requires FogOfWar Map Resolution to be square and a power of 2!");
                enabled = false;
                return;
            }

            _mapResolution = _fogOfWar.mapResolution.x;
            _fogOfWar.onRenderFogTexture.AddListener(OnRenderFog);

            ForceLoad();
        }

        Vector3i CalculateBestChunk(Vector3 pos)
        {
            Vector3i chunk;

            chunk.x = Mathf.RoundToInt(pos.x / (_fogOfWar.mapSize / 2)) - 1;
            chunk.y = Mathf.RoundToInt(pos.y / (_fogOfWar.mapSize / 2)) - 1;
            chunk.z = Mathf.FloorToInt((pos.z - verticalChunkOffset) / verticalChunkSize);
            if (pos.z - verticalChunkOffset < 0)
                --chunk.z;

            return chunk;
        }

        void SaveChunk(byte[] currentdata, byte[] totaldata, int xc, int yc)
        {
            // reuse chunk if it already exists
            Vector3i coordinate = _loadedChunk + new Vector3i(xc, yc, 0);
            FogOfWarChunk chunk = _chunks.Find(c => c.coordinate == coordinate);
            if (chunk == null)
            {
                chunk = new FogOfWarChunk();
                chunk.coordinate = coordinate;
                chunk.fogData = new byte[_valuesPerChunk];
                chunk.currentFogData = new byte[_valuesPerChunk];
                _chunks.Add(chunk);
            }
            else
            {
                if (chunk.fogData == null || chunk.fogData.Length != _valuesPerChunk)
                    chunk.fogData = new byte[_valuesPerChunk];
                if (chunk.currentFogData == null || chunk.currentFogData.Length != _valuesPerChunk)
                    chunk.currentFogData = new byte[_valuesPerChunk];
            }

            int halfmapsize = _mapResolution / 2;
            int xstart = halfmapsize * xc;
            int ystart = halfmapsize * yc;

            // copy values
            for (int y = 0; y < halfmapsize; ++y)
            {
                System.Array.Copy(totaldata, (ystart + y) * _mapResolution + xstart, chunk.fogData, y * halfmapsize, halfmapsize);
                System.Array.Copy(currentdata, (ystart + y) * _mapResolution + xstart, chunk.currentFogData, y * halfmapsize, halfmapsize);
            }
        }

        void SaveChunks()
        {
            // save all visible chunks
            byte[] currentdata = new byte[_valuesPerMap];
            _fogOfWar.GetCurrentFogValues(ref currentdata);

            byte[] totaldata = new byte[_valuesPerMap];
            _fogOfWar.GetTotalFogValues(ref totaldata);

            for (int y = 0; y < 2; ++y)
            {
                for (int x = 0; x < 2; ++x)
                    SaveChunk(currentdata, totaldata, x, y);
            }
        }

        void LoadChunk(byte[] currentdata, byte[] totaldata, int xc, int yc)
        {
            // only load if the chunk exists
            Vector3i coordinate = _loadedChunk + new Vector3i(xc, yc, 0);
            FogOfWarChunk chunk = _chunks.Find(c => c.coordinate == coordinate);
            if (chunk == null || chunk.fogData == null || chunk.fogData.Length != _valuesPerChunk)
                return;

            int halfmapsize = _mapResolution / 2;
            int xstart = halfmapsize * xc;
            int ystart = halfmapsize * yc;

            // copy values
            for (int y = 0; y < halfmapsize; ++y)
            {
                System.Array.Copy(chunk.fogData, y * halfmapsize, totaldata, (ystart + y) * _mapResolution + xstart, halfmapsize);
                System.Array.Copy(chunk.currentFogData, y * halfmapsize, currentdata, (ystart + y) * _mapResolution + xstart, halfmapsize);
            }
        }

        void LoadChunks()
        {
            byte[] currentdata = new byte[_valuesPerMap];
            byte[] totaldata = new byte[_valuesPerMap];

            // set fog full by default
            for (int i = 0; i < currentdata.Length; ++i)
            {
                currentdata[i] = 255;
                totaldata[i] = 255;
            }

            // load each visible chunk
            for (int y = 0; y < 2; ++y)
            {
                for (int x = 0; x < 2; ++x)
                    LoadChunk(currentdata, totaldata, x, y);
            }

            // put the new map into fow
            _fogOfWar.SetCurrentFogValues(currentdata);
            _fogOfWar.SetTotalFogValues(totaldata);
        }

        void ForceLoad()
        {
            if (followTransform == null)
                return;

            Vector3i desiredchunk = CalculateBestChunk(_followPosition);

            // move fow
            float chunksize = _fogOfWar.mapSize * 0.5f;
            _fogOfWar.mapOffset = desiredchunk.vector2 * chunksize + Vector2.one * (chunksize);
            _loadedChunk = desiredchunk;
            _fogOfWar.Reinitialize();

            LoadChunks();
        }

        void OnRenderFog()
        {
            if (followTransform == null)
                return;

            // is fow in the best position?
            if (CalculateBestChunk(_followPosition) != _loadedChunk)
            {
                SaveChunks();
                ForceLoad();

                // clear memory 
                if (!rememberFog)
                    _chunks.Clear();
            }
        }

        public void Clear()
        {
            _chunks.Clear();
        }
    }
}
