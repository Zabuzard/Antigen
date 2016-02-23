using System;

namespace Antigen.Map
{
    /// <summary>
    /// Container which holds flow, direction and mutation area data of an map area.
    /// </summary>
    [Serializable]
    sealed class MapData
    {
        private readonly MutationArea mMutationArea;
        private readonly float mFlow;
        private readonly int mDir;

        /// <summary>
        /// The mutation area.
        /// </summary>
        public MutationArea GetMutationArea
        {
            get { return mMutationArea; }
        }

        /// <summary>
        /// Flow of the area specified by map.
        /// </summary>
        public float Flow
        {
            get { return mFlow; }
        }

        /// <summary>
        /// Average direction of the area specified by map.
        /// </summary>
        public int Dir
        {
            get { return mDir; }
        }

        /// <summary>
        /// Creates a new map data object with given data.
        /// </summary>
        /// <param name="thatMutationArea">The mutation area</param>
        /// <param name="thatFlow">Flow of the area</param>
        /// <param name="thatDir">Dir of the area</param>
        public MapData(MutationArea thatMutationArea, float thatFlow, int thatDir)
        {
            mMutationArea = thatMutationArea;
            mFlow = thatFlow;
            mDir = thatDir;
        }
    }
}
