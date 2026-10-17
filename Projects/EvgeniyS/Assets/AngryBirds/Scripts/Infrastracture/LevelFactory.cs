using System.Collections.Generic;
using UnityEngine;
using AngryBirds.Core;

namespace AngryBirds.Infrastracture
{
    public sealed class LevelFactory : MonoBehaviour, ILevel
    {
        private static readonly Color WoodColor = new Color(0.65f, 0.44f, 0.22f);
        private static readonly Color PigColor = new Color(0.36f, 0.78f, 0.32f);
        private static readonly Color PigSnoutColor = new Color(0.24f, 0.62f, 0.22f);

        private const float GroundTop = -3f;
        private const float PigDiameter = 0.7f;
        private const float LegWidth = 0.5f;
        private const float LegHeight = 1f;
        private const float BeamWidth = 1.3f;
        private const float BeamHeight = 0.3f;

        private readonly List<Pig> _pigs = new List<Pig>();
        private readonly List<GameObject> _objects = new List<GameObject>();

        private Transform _root;
        private PhysicsMaterial2D _stableMaterial;

        public void Initialize(Transform parentRoot)
        {
            _root = parentRoot;
        }

        public int CountAlivePigs()
        {
            int count = 0;
            for (int i = 0; i < _pigs.Count; i++)
            {
                if (_pigs[i] != null && _pigs[i].IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        public void ResetLevel()
        {
            Clear();

            Build(2.6f);
            Build(5.6f);
            CreatePig(4.1f);
        }

        private void Clear()
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                if (_objects[i] != null)
                {
                    Destroy(_objects[i]);
                }
            }

            _objects.Clear();
            _pigs.Clear();
        }

        private void Build(float centerX)
        {
            float leftX = centerX - LegWidth;
            float rightX = centerX + LegWidth;
            float plankY = GroundTop + LegHeight * 0.5f;
            float beamY = GroundTop + LegHeight + BeamHeight * 0.5f;
            float pigY = beamY + BeamHeight * 0.5f + PigDiameter * 0.5f;

            CreateBlock(leftX, plankY, LegWidth, LegHeight);
            CreateBlock(rightX, plankY, LegWidth, LegHeight);
            CreateBlock(centerX, beamY, BeamWidth, BeamHeight);
            CreatePig(centerX, pigY);
        }

        private void CreateBlock(float x, float y, float width, float height)
        {
            var go = new GameObject("Block", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
            go.transform.SetParent(_root, false);
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(width, height, 1f);

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Square;
            renderer.color = WoodColor;

            var body = go.GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.mass = Mathf.Max(0.5f, width * height * 2f);
            body.linearDamping = 0.3f;
            body.angularDamping = 0.3f;

            var collider = go.GetComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.sharedMaterial = StableMaterial;

            _objects.Add(go);
        }

        private Pig CreatePig(float x)
        {
            return CreatePig(x, GroundTop + PigDiameter * 0.5f);
        }

        private Pig CreatePig(float x, float y)
        {
            var go = new GameObject("Pig", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(Pig));
            go.transform.SetParent(_root, false);
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = Vector3.one * PigDiameter;

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Circle;
            renderer.color = PigColor;
            renderer.sortingOrder = 2;

            var body = go.GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.mass = 0.6f;
            body.linearDamping = 0.1f;
            body.angularDamping = 0.1f;

            var collider = go.GetComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.sharedMaterial = StableMaterial;

            CreateSnout(go.transform);

            var pig = go.GetComponent<Pig>();
            _pigs.Add(pig);
            _objects.Add(go);
            return pig;
        }

        private void CreateSnout(Transform pig)
        {
            var snout = new GameObject("Snout", typeof(SpriteRenderer));
            snout.transform.SetParent(pig, false);
            snout.transform.localPosition = new Vector3(0.18f, -0.05f, 0f);
            snout.transform.localScale = Vector3.one * 0.34f;

            var renderer = snout.GetComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Circle;
            renderer.color = PigSnoutColor;
            renderer.sortingOrder = 3;
        }

        private PhysicsMaterial2D StableMaterial
        {
            get
            {
                if (_stableMaterial == null)
                {
                    _stableMaterial = new PhysicsMaterial2D("Stable")
                    {
                        friction = 0.7f,
                        bounciness = 0f
                    };
                }

                return _stableMaterial;
            }
        }
    }
}
