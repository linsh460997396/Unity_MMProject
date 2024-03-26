namespace Uniblocks
{
    /// <summary>
    /// 体素信息（存储体素索引、体素所在团块及体素在团块的位置信息）
    /// </summary>
    public class VoxelInfo {

	public Index index;
	public Index adjacentIndex;
	
	public Chunk chunk;
	
	
	public VoxelInfo ( int setX, int setY, int setZ, Chunk setChunk ) {
		this.index.x = setX;
		this.index.y = setY;
		this.index.z = setZ;
		
		this.chunk = setChunk;
	}
	
	public VoxelInfo ( int setX, int setY, int setZ, int setXa, int setYa, int setZa, Chunk setChunk ) {
		this.index.x = setX;
		this.index.y = setY;
		this.index.z = setZ;
		
		this.adjacentIndex.x = setXa;
		this.adjacentIndex.y = setYa;
		this.adjacentIndex.z = setZa;
		
		this.chunk = setChunk;
	}
	
	public VoxelInfo ( Index setIndex, Chunk setChunk ) {
		this.index = setIndex;
		
		this.chunk = setChunk;
	}
	
	public VoxelInfo ( Index setIndex, Index setAdjacentIndex, Chunk setChunk ) {
		this.index = setIndex;
		this.adjacentIndex = setAdjacentIndex;
		
		this.chunk = setChunk;
	}
	
		
	public ushort GetVoxel () {
		return chunk.GetVoxel(index);
	}
	
	public Voxel GetVoxelType () {
		return Engine.GetVoxelType(chunk.GetVoxel(index));
	}
	
	public ushort GetAdjacentVoxel () {
		return chunk.GetVoxel(adjacentIndex);
	}
	
	public Voxel GetAdjacentVoxelType () {
		return Engine.GetVoxelType(chunk.GetVoxel(adjacentIndex));
	}
	
	public void SetVoxel ( ushort data, bool updateMesh ) {
		chunk.SetVoxel (index, data, updateMesh);
	}

}

}