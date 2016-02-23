using System;
using Antigen.AI;
using Antigen.Input;
using Antigen.Logic.Collision;
using Antigen.Logic.Mutation;
using Antigen.Logic.Offensive.Infection;
using Antigen.Logic.Pathfinding;
using Antigen.Logic.Selection;
using Antigen.Objects;
using Antigen.Objects.Properties;
using Antigen.Objects.Units;
using Antigen.Objects.Units.Values;
using Antigen.Sound;
using Antigen.Statistics;
using Microsoft.Xna.Framework;

namespace Antigen.Logic.CellDivision
{
    /// <summary>
    /// Implements a basic division behavior for units.
    /// Can copy the unit and create a new cell.
    /// </summary>
    [Serializable]
    sealed class DivisionBehavior : IDivisionBehavior
    {
        /// <summary>
        /// Unit to divide.
        /// </summary>
        private readonly ICanCellDivision mObj;
        private readonly Map.Map mMap;
        private readonly InputDispatcher mInput;
        /// <summary>
        /// Alternative bool if the object is not a unit with modes.
        /// </summary>
        private bool mIsCellDividing;

        private readonly ObjectCaches mObjectCaches;
        private readonly SelectionManager mSelectionManager;
        private readonly AiBrain mAiBrain;
        [NonSerialized]
        private readonly SoundWrapper mSoundWrapper;
        private readonly IStatisticIncrementer mStats;
        [NonSerialized]
        private Pathfinder mPathfinder;

        /// <summary>
        /// Creates a new basic division behavior with a given cell.
        /// </summary>
        /// <param name="thatCell">Cell to divide</param>
        /// <param name="thatMap">Map with collision and flow data</param>
        /// <param name="thatObjectCaches">The game's object caches.</param>
        /// <param name="aiBrain">Ai brain</param>
        /// <param name="stats">Statistic incrementer</param>
        // Resharper hint: Game engine should not support ICanCellDivision
        // here because a future ICanCellDivision maybe won't fit here.
// ReSharper disable SuggestBaseTypeForParameter
        public DivisionBehavior(Bacterium thatCell, Map.Map thatMap,
// ReSharper restore SuggestBaseTypeForParameter
            ObjectCaches thatObjectCaches, AiBrain aiBrain, IStatisticIncrementer stats)
        {
            mStats = stats;
            mObjectCaches = thatObjectCaches;
            mAiBrain = aiBrain;
            mObj = thatCell;
            mInput = null;
            mMap = thatMap;
            mPathfinder = thatObjectCaches.Pathfinder;
            IsCellDividing = false;
        }

        /// <summary>
        /// Creates a new basic division behavior with a given cell.
        /// </summary>
        /// <param name="thatCell">Cell to divide</param>
        /// <param name="thatInput">Input Manager</param>
        /// <param name="thatMap">Map with collision and flow data</param>
        /// <param name="thatSelectionManager">The game's selection manager</param>
        /// <param name="thatObjectCaches">The game's object caches.</param>
        /// <param name="soundWrapper">Sound wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        // Resharper hint: Game engine should not support ICanCellDivision
        // here because a future ICanCellDivision maybe won't fit here.
// ReSharper disable SuggestBaseTypeForParameter
        public DivisionBehavior(Stemcell thatCell, InputDispatcher thatInput,
// ReSharper restore SuggestBaseTypeForParameter
            Map.Map thatMap, SelectionManager thatSelectionManager, ObjectCaches thatObjectCaches,
            SoundWrapper soundWrapper, IStatisticIncrementer stats)
        {
            mStats = stats;
            mSoundWrapper = soundWrapper;
            mObjectCaches = thatObjectCaches;
            mSelectionManager = thatSelectionManager;
            mObj = thatCell;
            mInput = thatInput;
            mMap = thatMap;
            mPathfinder = thatObjectCaches.Pathfinder;
            IsCellDividing = false;
        }

        /// <summary>
        /// Creates a new basic division behavior with a given cell.
        /// </summary>
        /// <param name="thatCell">Cell to divide</param>
        /// <param name="thatInput">Input Manager</param>
        /// <param name="thatMap">Map with collision and flow data</param>
        /// <param name="thatSelectionManager">The game's selection manager</param>
        /// <param name="thatObjectCaches">The game's object caches.</param>
        /// <param name="soundWrapper">Sound wrapper to play sound with</param>
        /// <param name="stats">Statistic incrementer</param>
        // Resharper hint: Game engine should not support ICanCellDivision
        // here because a future ICanCellDivision maybe won't fit here.
// ReSharper disable SuggestBaseTypeForParameter
        public DivisionBehavior(Bcell thatCell, InputDispatcher thatInput,
// ReSharper restore SuggestBaseTypeForParameter
            Map.Map thatMap, SelectionManager thatSelectionManager, ObjectCaches thatObjectCaches,
            SoundWrapper soundWrapper, IStatisticIncrementer stats)
        {
            mStats = stats;
            mSoundWrapper = soundWrapper;
            mObjectCaches = thatObjectCaches;
            mSelectionManager = thatSelectionManager;
            mObj = thatCell;
            mInput = thatInput;
            mMap = thatMap;
            mPathfinder = thatObjectCaches.Pathfinder;
            IsCellDividing = false;
        }

        /// <summary>
        /// Creates a new basic division behavior with a given cell.
        /// </summary>
        /// <param name="thatCell">Cell to divide</param>
        /// <param name="thatInput">Input Manager</param>
        /// <param name="thatMap">Map with collision and flow data</param>
        /// <param name="thatSelectionManager">The game's selection manager</param>
        /// <param name="thatObjectCaches">The game's object caches.</param>
        /// <param name="soundWrapper">Wrapper to play sounds with</param>
        /// <param name="stats">Statistic incrementer</param>
        // Resharper hint: Game engine should not support ICanCellDivision
        // here because a future ICanCellDivision maybe won't fit here.
// ReSharper disable SuggestBaseTypeForParameter
        public DivisionBehavior(Tcell thatCell, InputDispatcher thatInput,
// ReSharper restore SuggestBaseTypeForParameter
            Map.Map thatMap, SelectionManager thatSelectionManager, ObjectCaches thatObjectCaches,
            SoundWrapper soundWrapper, IStatisticIncrementer stats)
        {
            mStats = stats;
            mSoundWrapper = soundWrapper;
            mObjectCaches = thatObjectCaches;
            mSelectionManager = thatSelectionManager;
            mObj = thatCell;
            mInput = thatInput;
            mMap = thatMap;
            mPathfinder = thatObjectCaches.Pathfinder;
            IsCellDividing = false;
        }

        /// <summary>
        /// Creates a new basic division behavior with a given cell.
        /// </summary>
        /// <param name="thatCell">Cell to divide</param>
        /// <param name="thatMap">Map with collision and flow data</param>
        /// <param name="thatObjectCaches">The game's object caches.</param>
        /// <param name="aiBrain">Ai brain</param>
        /// <param name="stats">Statistic incrementer</param>
        // Resharper hint: Game engine should not support ICanCellDivision
        // here because a future ICanCellDivision maybe won't fit here.
// ReSharper disable SuggestBaseTypeForParameter
        public DivisionBehavior(Virus thatCell, Map.Map thatMap,
// ReSharper restore SuggestBaseTypeForParameter
            ObjectCaches thatObjectCaches, AiBrain aiBrain, IStatisticIncrementer stats)
        {
            mStats = stats;
            mObjectCaches = thatObjectCaches;
            mAiBrain = aiBrain;
            mObj = thatCell;
            mMap = thatMap;
            mPathfinder = thatObjectCaches.Pathfinder;
            IsCellDividing = false;
        }

        /// <summary>
        /// Returns if the object is currently doing cell division.
        /// </summary>
        public bool IsCellDividing
        {
            get
            {
                var obj = mObj as Unit;
                if (obj != null)
                {
                    return obj.Mode == Unit.UnitMode.CellDivision;
                }
                return mIsCellDividing;
            }
            private set { mIsCellDividing = value; }
        }

        /// <inheritdoc />
        public void DivideCell(ICanCellDivision cell, IDivisionContentLoader loader)
        {
            if (!(cell is GameObject && mObj is GameObject))
            {
                return;
            }
            if (mObj is IInfectable && ((IInfectable)mObj).IsInfected())
            {
                return;
            }
            if (mObj is ICanInfect && !((ICanInfect)mObj).IsInfecting())
            {
                return;
            }

            IsCellDividing = true;

            var inheritance = CalcInheritance(mObj);

            Unit clone;
            var stemcell = cell as Stemcell;
            var bcell = cell as Bcell;
            if (stemcell != null)
            {
                switch (stemcell.GetDivisionResult())
                {
                    case Stemcell.DivisionResult.Bcell :
                        clone = new Bcell(mObjectCaches, loader, mInput,
                            mSelectionManager, CalcStartPosition((GameObject)mObj), mMap, inheritance, mSoundWrapper, mStats);
                        break;
                    case Stemcell.DivisionResult.Tcell :
                        clone = new Tcell(mObjectCaches, loader, mInput,
                            mSelectionManager, CalcStartPosition((GameObject)mObj), mMap, mSoundWrapper, mStats, inheritance);
                        break;
                    case Stemcell.DivisionResult.Macrophage :
                        clone = new Macrophage(mObjectCaches, mInput, CalcStartPosition((GameObject)mObj), mMap, mSoundWrapper, mStats, inheritance);
                        break;
                    case Stemcell.DivisionResult.RedBloodcell :
                        clone = new RedBloodcell(mObjectCaches,
                            CalcStartPosition((GameObject)mObj), mMap, inheritance);
                        break;
                    default :
                        clone = new Stemcell(mObjectCaches, loader, mInput,
                            mSelectionManager, CalcStartPosition((GameObject)mObj), mMap, mSoundWrapper, mStats, inheritance);
                        break;
                }
            }
            else if (bcell != null)
            {
                switch (bcell.GetDivisionResult())
                {
                    case Bcell.DivisionResult.Antibody:
                        clone = new Antibody(mObjectCaches, mInput, CalcStartPosition((GameObject)mObj),
                            bcell.GetDebuffTable(), mMap, mSoundWrapper, mStats, inheritance);
                        break;
                    default:
                        clone = new Bcell(mObjectCaches, loader, mInput,
                            mSelectionManager, CalcStartPosition((GameObject)mObj),
                            mMap, inheritance, mSoundWrapper, mStats, bcell.GetDebuffTable());
                        break;
                }
            }
            else if (cell is Tcell)
            {
                clone = new Tcell(mObjectCaches, loader, mInput,
                            mSelectionManager, CalcStartPosition((GameObject)mObj), mMap, mSoundWrapper, mStats, inheritance);
            }
            else if (cell is Bacterium)
            {
                clone = new Bacterium(mObjectCaches, loader, CalcStartPosition((GameObject)mObj), mMap, mAiBrain, mStats, inheritance);
            }
            else if (cell is Virus)
            {
                clone = new Virus(mObjectCaches, loader, CalcStartPosition((GameObject)mObj), mMap, mAiBrain, mStats, inheritance);
            }
            else
            {
                return;
            }

            loader.LoadDivisionContent(clone);
            mObjectCaches.Add(clone);
            IsCellDividing = false;

            //Play sound at units cell division
            var unit = mObj as Unit;
            if (unit != null && mSoundWrapper != null &&
                unit.GetSide() == Unit.UnitSide.Friendly)
            {
                mSoundWrapper.PlayEffect(Effect.Division, unit.Position);
            }

            //Increment division statistic
            if (unit != null && unit.GetSide() == Unit.UnitSide.Friendly)
            {
                mStats.IncrementStatistic(StatName.Performed_celldivisions);
            }
        }

        /// <summary>
        /// Calculates a starting correct position for a clone based on old information.
        /// </summary>
        /// <param name="obj">Object with position information</param>
        private Vector2 CalcStartPosition(ISpatial obj)
        {
            if (mPathfinder == null)
            {
                mPathfinder = mObjectCaches.Pathfinder;
            }
            return mPathfinder.FindNearestValidPoint(obj.Position, true, true);
        }

        /// <summary>
        /// Calculates the inheritance table of a given dividing object.
        /// </summary>
        /// <param name="obj">Object to calculate inheritance from</param>
        /// <returns>Inheritance calculated from object</returns>
        private InheritanceTable CalcInheritance(ICanCellDivision obj)
        {
            string antigen;
            var hasAntigen = obj as IHasAntigen;
            var hasStrain = obj as IHasStrain;
            if (hasAntigen != null)
            {
                antigen = hasAntigen.Antigen;
            }
            else if (hasStrain != null)
            {
                antigen = hasStrain.Strain;
            }
            else
            {
                antigen = "";
            }

            var unit = obj as Unit;
            var mutations = unit != null ? unit.Mutations : MutationTable.CreateEmptyTable();

            var mutationArea = mMap.GetData((int)((GameObject) obj).Position.X, (int)((GameObject) obj).Position.Y).GetMutationArea;

            //Mutation occurs
            if (unit != null && Mutations.ShouldMutationOccur(mutationArea))
            {
                //Strainchange
                if (hasStrain != null)
                {
                    antigen = Mutations.MutateStrain(hasStrain.Strain, mutationArea);
                }

                //DebuffTable
                var bcell = obj as Bcell;
                if (bcell != null)
                {
                    var debuffs = Mutations.MutateDebuffTable(bcell.GetDebuffTable(), mutationArea);
                    //Inherit mutation to parent
                    bcell.SetDebuffTable(debuffs);
                }

                //MutationTable
                mutations = Mutations.MutateMutationTable(mutations, mutationArea);

                //Increment mutation statistic
                if (unit.GetSide() == Unit.UnitSide.Friendly)
                {
                    mStats.IncrementStatistic(StatName.Performed_mutations);
                }
            }
            return new InheritanceTable(antigen, mutations);
        }
    }
}
