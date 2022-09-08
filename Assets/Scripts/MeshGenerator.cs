using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

public class MeshGenerator : MonoBehaviour
{
    public float Power = 10;

    MeshFilter meshFilter;
    Mesh mesh;
    Color[] colors;
    float[] spectrum;
    float[] values;

    VertexColorJob _vertexColorJob;
    JobHandle _jobHandle;

    public NativeArray<Vector2> UVs;
    public NativeArray<Color> Colors;
    public NativeArray<float> float_values;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        colors = new Color[mesh.vertices.Length];   
        spectrum = new float[256];
        values = new float[spectrum.Length * spectrum.Length];
        UVs = GetNativeUVArrays(mesh.uv);
        Colors = GetNativeColorArrays(colors);
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        int index = 0;
        for (int w = 0; w < spectrum.Length; w++)
        {
            
            for (int h = 0; h < spectrum.Length; h++)
            {
                values[index] = spectrum[w] * Power;
            }
        }
        float_values = GetNativeFloatArrays(values);
        _vertexColorJob = new VertexColorJob()
        {
            UVs = this.UVs,
            Colors = this.Colors,
            values = this.float_values
        };
        _jobHandle = _vertexColorJob.Schedule(UVs.Length,1);
        _jobHandle.Complete();
        SetNativeColorArray(colors, Colors);

        mesh.SetColors(colors);
        Debug.Log(colors[52]);

        float_values.Dispose();

        /*
        for (int i= 0; i < mesh.uv.Length; i++)
        {

            int u = Mathf.Abs( Mathf.FloorToInt(mesh.uv[i].x * spectrum.Length));
            int v = Mathf.Abs( Mathf.FloorToInt(mesh.uv[i].y * spectrum.Length));
            float val = values[u,v];

            colors[i] = new Color(val,val,val);
        }
        */
    }

    unsafe void SetNativeColorArray(Color[] colorArray, NativeArray<Color> colorBuffer)
    {
        // pin the target vertex array and get a pointer to it
        fixed (void* vertexArrayPointer = colorArray)
        {
            // memcopy the native array over the top
            UnsafeUtility.MemCpy(vertexArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(colorBuffer), colorArray.Length * (long)UnsafeUtility.SizeOf<Color>());
        }
    }


    unsafe NativeArray<Vector2> GetNativeUVArrays(Vector2[] uvArray)
    {
        // create a destination NativeArray to hold the vertices
        NativeArray<Vector2> verts = new NativeArray<Vector2>(uvArray.Length, Allocator.Persistent,
            NativeArrayOptions.UninitializedMemory);

        // pin the mesh's vertex buffer in place...
        fixed (void* vertexBufferPointer = uvArray)
        {
            // ...and use memcpy to copy the Vector3[] into a NativeArray<floar3> without casting. whould be fast!
            UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(verts),
                vertexBufferPointer, uvArray.Length * (long)UnsafeUtility.SizeOf<Vector2>());
        }
        // we only hve to fix the .net array in place, the NativeArray is allocated in the C++ side of the engine and
        // wont move arround unexpectedly. We have a pointer to it not a reference! thats basically what fixed does,
        // we create a scope where its 'safe' to get a pointer and directly manipulate the array

        return verts;
    }

    unsafe NativeArray<Color> GetNativeColorArrays(Color[] uvArray)
    {
        // create a destination NativeArray to hold the vertices
        NativeArray<Color> verts = new NativeArray<Color>(uvArray.Length, Allocator.Persistent,
            NativeArrayOptions.UninitializedMemory);

        // pin the mesh's vertex buffer in place...
        fixed (void* vertexBufferPointer = uvArray)
        {
            // ...and use memcpy to copy the Vector3[] into a NativeArray<floar3> without casting. whould be fast!
            UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(verts),
                vertexBufferPointer, uvArray.Length * (long)UnsafeUtility.SizeOf<Color>());
        }
        // we only hve to fix the .net array in place, the NativeArray is allocated in the C++ side of the engine and
        // wont move arround unexpectedly. We have a pointer to it not a reference! thats basically what fixed does,
        // we create a scope where its 'safe' to get a pointer and directly manipulate the array

        return verts;
    }

    unsafe NativeArray<float> GetNativeFloatArrays(float[] uvArray)
    {
        // create a destination NativeArray to hold the vertices
        NativeArray<float> verts = new NativeArray<float>(uvArray.Length, Allocator.TempJob,
            NativeArrayOptions.UninitializedMemory);

        // pin the mesh's vertex buffer in place...
        fixed (void* vertexBufferPointer = uvArray)
        {
            // ...and use memcpy to copy the Vector3[] into a NativeArray<floar3> without casting. whould be fast!
            UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(verts),
                vertexBufferPointer, uvArray.Length * (long)UnsafeUtility.SizeOf<float>());
        }
        // we only hve to fix the .net array in place, the NativeArray is allocated in the C++ side of the engine and
        // wont move arround unexpectedly. We have a pointer to it not a reference! thats basically what fixed does,
        // we create a scope where its 'safe' to get a pointer and directly manipulate the array

        return verts;
    }

    private void OnDestroy()
    {
        UVs.Dispose();
        Colors.Dispose();
        float_values.Dispose();
    }
}

public struct VertexColorJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<Vector2> UVs;
    public NativeArray<Color> Colors;

    [ReadOnly]
    public NativeArray<float> values;

    public void Execute(int index)
    {
        float u = UVs[index].x * 255;
        float v = UVs[index].y * 255;


        float val = values[Mathf.Abs(Mathf.FloorToInt(u) * Mathf.FloorToInt(v))];

        Colors[index] = new Color(val, val, val,1);
    }
}
