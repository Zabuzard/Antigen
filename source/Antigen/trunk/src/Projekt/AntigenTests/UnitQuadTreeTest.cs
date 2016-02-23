using System.Linq;
using Antigen.Logic.Collision;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace AntigenTests
{
    [TestClass]
    public sealed class UnitQuadTreeTest
    {
        [TestMethod]
        public void Add_Single()
        {
            var tree = new UnitQuadtree();
            var obj = new MockICollidable(new Vector2(100, 100), 50);
            tree.Add(obj);
            tree.ConsistencyCheck();

            var act = tree.CollisionBuckets().Select(bucket => bucket.ToArray()).ToArray();
            Assert.AreEqual(1, act.Length);
            Assert.AreEqual(1, act[0].Length);
            Assert.AreEqual(obj, act[0][0]);
        }

        [TestMethod]
        public void Add_Multiple_OneSplit()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj2 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj3 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj4 = new MockICollidable(new Vector2(2100, 2100), 50); //bottom-right
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            tree.ConsistencyCheck();

            var act = tree.CollisionBuckets().Select(bucket => bucket.ToArray()).ToArray();
            Assert.AreEqual(2, act.Length);
            Assert.AreEqual(3, act[0].Length);
            Assert.AreEqual(1, act[1].Length);
            Assert.AreEqual(obj1, act[0][0]);
            Assert.AreEqual(obj2, act[0][1]);
            Assert.AreEqual(obj3, act[0][2]);
            Assert.AreEqual(obj4, act[1][0]);
        }

        [TestMethod]
        public void Add_Multiple_OneSplit_OnBoundary()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj2 = new MockICollidable(new Vector2(2100, 2100), 50); //bottom-right
            var obj3 = new MockICollidable(new Vector2(1990, 1990), 50); //all quadrants
            var obj4 = new MockICollidable(new Vector2(2100, 2100), 50); //bottom-right
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            tree.ConsistencyCheck();

            var act = tree.CollisionBuckets().Select(bucket => bucket.ToArray()).ToArray();
            Assert.AreEqual(4, act.Length);
            Assert.AreEqual(2, act[0].Length); //top-left
            Assert.AreEqual(1, act[1].Length); //top-right
            Assert.AreEqual(1, act[2].Length); //bottom-left
            Assert.AreEqual(3, act[3].Length); //bottom-right
        }

        [TestMethod]
        public void Remove_Single()
        {
            var tree = new UnitQuadtree();
            var obj = new MockICollidable(new Vector2(100, 100), 50);
            tree.Add(obj);
            tree.Remove(obj);
            tree.ConsistencyCheck();

            var act = tree.CollisionBuckets();
            Assert.AreEqual(0, act.Count());
        }

        [TestMethod]
        public void Remove_Multiple()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj2 = new MockICollidable(new Vector2(2100, 2100), 50); //bottom-right
            var obj3 = new MockICollidable(new Vector2(1990, 1990), 50); //all quadrants
            var obj4 = new MockICollidable(new Vector2(2100, 2100), 50); //bottom-right
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            tree.Remove(obj2);
            tree.Remove(obj4);
            tree.Remove(obj1);
            tree.ConsistencyCheck();

            var act = tree.CollisionBuckets().Select(bucket => bucket.ToArray()).ToArray();
            Assert.AreEqual(4, act.Length);
            Assert.AreEqual(obj3, act[0][0]);
        }

        [TestMethod]
        public void Update_Multiple_QuadrantChanged()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj2 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj3 = new MockICollidable(new Vector2(2100, 2100), 50); //bottom-right
            var obj4 = new MockICollidable(new Vector2(2100, 2100), 50); //bottom-right
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            obj1.Position = new Vector2(2100, 2100); //bottom-right
            tree.Update(obj1);
            tree.ConsistencyCheck();

            var act = tree.CollisionBuckets().Select(bucket => bucket.ToArray()).ToArray();
            Assert.AreEqual(2, act.Length);
            Assert.AreEqual(1, act[0].Length);
            Assert.AreEqual(3, act[1].Length);
        }

        [TestMethod]
        public void Update_Multiple_Stationary()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj2 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj3 = new MockICollidable(new Vector2(2100, 2100), 50); //bottom-right
            var obj4 = new MockICollidable(new Vector2(2100, 2100), 50); //bottom-right
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            tree.Update(obj1);
            tree.ConsistencyCheck();

            var act = tree.CollisionBuckets().Select(bucket => bucket.ToArray()).ToArray();
            Assert.AreEqual(2, act.Length);
            Assert.AreEqual(2, act[0].Length);
            Assert.AreEqual(2, act[1].Length);
        }

        [TestMethod]
        public void Update_Multiple_QuadrantChanged_OnBoundary()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj2 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj3 = new MockICollidable(new Vector2(1990, 1990), 50); //all quadrants
            var obj4 = new MockICollidable(new Vector2(2100, 2100), 50); //bottom-right
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            obj3.Position = new Vector2(2100, 2100); //bottom-right
            tree.Update(obj3);
            tree.ConsistencyCheck();

            var act = tree.CollisionBuckets().Select(bucket => bucket.ToArray()).ToArray();
            Assert.AreEqual(2, act.Length);
            Assert.AreEqual(2, act[0].Length);
            Assert.AreEqual(2, act[1].Length);
        }

        [TestMethod]
        public void Update_Multiple_DepthTwo()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj2 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj3 = new MockICollidable(new Vector2(1100, 1100), 50); //top-left
            var obj4 = new MockICollidable(new Vector2(1100, 1100), 50); //top-left
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            obj3.Position = new Vector2(100, 100); //top-left
            tree.Update(obj3);
            tree.ConsistencyCheck();

            var act = tree.CollisionBuckets().Select(bucket => bucket.ToArray()).ToArray();
            Assert.AreEqual(2, act.Length);
            Assert.AreEqual(3, act[0].Length);
            Assert.AreEqual(1, act[1].Length);
        }

        [TestMethod]
        public void Update_CollapseOneLevel()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left A
            var obj2 = new MockICollidable(new Vector2(1100, 100), 50); //top-left B
            var obj3 = new MockICollidable(new Vector2(100, 1100), 50); //top-left C
            var obj4 = new MockICollidable(new Vector2(1100, 1100), 50); //top-left D
            var obj5 = new MockICollidable(new Vector2(2100, 2100), 50); //bottom-right A
            var obj6 = new MockICollidable(new Vector2(3100, 2100), 50); //bottom-right B
            var obj7 = new MockICollidable(new Vector2(2100, 3100), 50); //bottom-right C
            var obj8 = new MockICollidable(new Vector2(3100, 3100), 50); //bottom-right D
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            tree.Add(obj5);
            tree.Add(obj6);
            tree.Add(obj7);
            tree.Add(obj8);
            obj5.Position = new Vector2(200, 200); //top-left A
            obj6.Position = new Vector2(1200, 200); //top-left B
            obj7.Position = new Vector2(200, 1200); //top-left C
            obj8.Position = new Vector2(1200, 1200); //top-left D
            tree.Update(obj5);
            tree.Update(obj6);
            tree.Update(obj7);
            tree.Update(obj8);
            tree.ConsistencyCheck();

            var act = tree.CollisionBuckets().Select(bucket => bucket.ToArray()).ToArray();
            Assert.AreEqual(4, act.Length);
            Assert.AreEqual(2, act[0].Length); //top-left A
            Assert.AreEqual(2, act[1].Length); //top-left B
            Assert.AreEqual(2, act[2].Length); //top-left C
            Assert.AreEqual(2, act[3].Length); //top-left D
        }

        [TestMethod]
        public void Collisions_ObjectNotInTree()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj2 = new MockICollidable(new Vector2(1100, 100), 50); //top-left
            tree.Add(obj1);
            tree.ConsistencyCheck();

            var obj2Collisions = tree.Collisions(obj2, CollisionDetection.PairwiseCircleCollisionDetection);
            Assert.AreEqual(0, obj2Collisions.Count());
        }

        [TestMethod]
        public void Collisions_None_SameNode()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left
            var obj2 = new MockICollidable(new Vector2(1100, 100), 50); //top-left
            tree.Add(obj1);
            tree.Add(obj2);
            tree.ConsistencyCheck();

            var obj1Collisions = tree.Collisions(obj1, CollisionDetection.PairwiseCircleCollisionDetection);
            var obj2Collisions = tree.Collisions(obj2, CollisionDetection.PairwiseCircleCollisionDetection);
            Assert.AreEqual(0, obj1Collisions.Count());
            Assert.AreEqual(0, obj2Collisions.Count());
        }

        [TestMethod]
        public void Collisions_None_DifferentNodes()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left A
            var obj2 = new MockICollidable(new Vector2(1100, 100), 50); //top-left B
            var obj3 = new MockICollidable(new Vector2(100, 1100), 50); //top-left C
            var obj4 = new MockICollidable(new Vector2(1100, 1100), 50); //top-left D
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            tree.ConsistencyCheck();

            var act = tree.Collisions(obj1, CollisionDetection.PairwiseCircleCollisionDetection);
            Assert.AreEqual(0, act.Count());
        }

        [TestMethod]
        public void Collisions_Two()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left A
            var obj2 = new MockICollidable(new Vector2(90, 90), 50); //top-left A
            var obj3 = new MockICollidable(new Vector2(100, 1100), 50); //top-left C
            var obj4 = new MockICollidable(new Vector2(1100, 1100), 50); //top-left D
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            tree.ConsistencyCheck();

            var obj1Collisions = tree.Collisions(obj1, CollisionDetection.PairwiseCircleCollisionDetection).ToArray();
            var obj2Collisions = tree.Collisions(obj2, CollisionDetection.PairwiseCircleCollisionDetection).ToArray();
            var obj3Collisions = tree.Collisions(obj3, CollisionDetection.PairwiseCircleCollisionDetection).ToArray();
            Assert.AreEqual(1, obj1Collisions.Count());
            Assert.AreEqual(1, obj2Collisions.Count());
            Assert.AreEqual(0, obj3Collisions.Count());
            Assert.AreEqual(obj2, obj1Collisions[0]);
            Assert.AreEqual(obj1, obj2Collisions[0]);
        }

        [TestMethod]
        public void Collisions_Three()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left A
            var obj2 = new MockICollidable(new Vector2(90, 90), 50); //top-left A
            var obj3 = new MockICollidable(new Vector2(80, 80), 50); //top-left A
            var obj4 = new MockICollidable(new Vector2(1100, 1100), 50); //top-left D
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            tree.ConsistencyCheck();

            var obj1Collisions = tree.Collisions(obj1, CollisionDetection.PairwiseCircleCollisionDetection).ToArray();
            var obj2Collisions = tree.Collisions(obj2, CollisionDetection.PairwiseCircleCollisionDetection).ToArray();
            var obj3Collisions = tree.Collisions(obj3, CollisionDetection.PairwiseCircleCollisionDetection).ToArray();
            Assert.AreEqual(2, obj1Collisions.Count());
            Assert.AreEqual(2, obj2Collisions.Count());
            Assert.AreEqual(2, obj3Collisions.Count());
        }

        [TestMethod]
        public void Collisions_MultipleNodes()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left A
            var obj2 = new MockICollidable(new Vector2(90, 90), 50); //top-left A
            var obj3 = new MockICollidable(new Vector2(1000, 1000), 50); //all quadrants
            var obj4 = new MockICollidable(new Vector2(1010, 1010), 50); //top-left D
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            tree.ConsistencyCheck();

            var obj1Collisions = tree.Collisions(obj1, CollisionDetection.PairwiseCircleCollisionDetection).ToArray();
            var obj2Collisions = tree.Collisions(obj2, CollisionDetection.PairwiseCircleCollisionDetection).ToArray();
            var obj3Collisions = tree.Collisions(obj3, CollisionDetection.PairwiseCircleCollisionDetection).ToArray();
            var obj4Collisions = tree.Collisions(obj4, CollisionDetection.PairwiseCircleCollisionDetection).ToArray();
            Assert.AreEqual(1, obj1Collisions.Count());
            Assert.AreEqual(1, obj2Collisions.Count());
            Assert.AreEqual(1, obj3Collisions.Count());
            Assert.AreEqual(1, obj4Collisions.Count());
            Assert.AreEqual(obj2, obj1Collisions[0]);
            Assert.AreEqual(obj1, obj2Collisions[0]);
            Assert.AreEqual(obj4, obj3Collisions[0]);
            Assert.AreEqual(obj3, obj4Collisions[0]);
        }

        [TestMethod]
        public void ObjectsInArea_None()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left A
            var obj2 = new MockICollidable(new Vector2(90, 90), 50); //top-left A
            var obj3 = new MockICollidable(new Vector2(1000, 1000), 50); //all quadrants
            var obj4 = new MockICollidable(new Vector2(1010, 1010), 50); //top-left D
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            tree.ConsistencyCheck();

            var act = tree.CollidableObjectsInArea(new Rectangle(2000, 2000, 2000, 2000));
            Assert.AreEqual(0, act.Count());
        }

        [TestMethod]
        public void ObjectsInArea_Two_OneNode()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(100, 100), 50); //top-left A
            var obj2 = new MockICollidable(new Vector2(90, 90), 50); //top-left A
            var obj3 = new MockICollidable(new Vector2(1000, 1000), 50); //all quadrants
            var obj4 = new MockICollidable(new Vector2(1010, 1010), 50); //top-left D
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            tree.ConsistencyCheck();

            var act = tree.CollidableObjectsInArea(new Rectangle(0, 0, 1000, 1000)).ToList();
            Assert.AreEqual(2, act.Count());
            Assert.IsTrue(act.Contains(obj1));
            Assert.IsTrue(act.Contains(obj2));
        }

        [TestMethod]
        public void ObjectsInArea_Two_TwoNodes()
        {
            var tree = new UnitQuadtree();
            var obj1 = new MockICollidable(new Vector2(0, 0), 50); //top-left A
            var obj2 = new MockICollidable(new Vector2(200, 200), 50); //top-left A
            var obj3 = new MockICollidable(new Vector2(1000, 1000), 50); //all quadrants
            var obj4 = new MockICollidable(new Vector2(1800, 1800), 50); //top-left D
            tree.Add(obj1);
            tree.Add(obj2);
            tree.Add(obj3);
            tree.Add(obj4);
            tree.ConsistencyCheck();

            var act = tree.CollidableObjectsInArea(new Rectangle(200, 200, 1000, 1000)).ToList();
            Assert.AreEqual(2, act.Count());
            Assert.IsTrue(act.Contains(obj2));
            Assert.IsTrue(act.Contains(obj3));
        }

        private sealed class MockICollidable : ICollidable
        {
            public MockICollidable(Vector2 position, int radius)
            {
                OldPosition = position;
                Position = position;
                Radius = radius;
            }

            public bool IsVirtualCollidable { get { return false; } }

            public Vector2 OldPosition { get; set; }

            public Vector2 Position { get; set; }

            public int Radius { get; private set; }

            public Rectangle Hitbox
            {
                get
                {
                    return new Rectangle((int)Position.X, (int)Position.Y, Radius * 2, Radius * 2);
                }
            }

            public bool CollisionInLastTick { get; set; }
        }
    }
}
