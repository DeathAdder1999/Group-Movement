using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ClipperLib;
using Geometry.Polygons;
using Geometry.Primitives;
using Main;
using Managers;
using NPCs;
using UnityEngine;
using Utility;

namespace Maps
{
    public class Map : MonoBehaviour
    {
        [SerializeField] private bool _renderMap = true;
        [SerializeField] private bool _renderShadowVolume;
        [SerializeField] private bool _generateShadowMap;

        public GameObject PolygonLinePrefab;

        private LineRenderer _lineRenderer;
        private List<Line> _mapLines = new List<Line>();
        private List<Line> _shadowLines = new List<Line>();

        private bool _instantiated = false;

        void Start()
        {
            InstantiatePolygons(_mapLines, true);
        }

        void Update()
        {
            if (_mapLines.Any())
            {
                if (_mapLines[0].IsRendering && !_renderMap)
                {
                    SetRenderMap(false);
                }

                if (!_mapLines[0].IsRendering && _renderMap)
                {
                    SetRenderMap(true);
                }
            }

            if (_shadowLines.Any())
            {
                if (_shadowLines[0].IsRendering && !_renderShadowVolume)
                {
                    SetRenderShadowMap(false);
                }

                if (!_shadowLines[0].IsRendering && _renderShadowVolume)
                {
                    SetRenderShadowMap(true);
                }
            }

            if (_generateShadowMap)
            {
                InstantiateShadowPolygons();
                _generateShadowMap = false;
            }
        }

        private void InstantiatePolygons(List<Line> lineContainer, bool isPartOfMap, bool enableColliders = true)
        {
            var polygons = GameManager.Instance.MapData;

            foreach (var polygon in polygons)
            {
                InstantiatePolygon(polygon, lineContainer, isPartOfMap, enableColliders);
            }

            EventManager.RaiseMapGeneratedEvent(this, EventArgs.Empty);
        }

        private void InstantiatePolygonLine((Vector2, Vector2) lineCoordinates, List<Line> lineContainer, bool isPartOfMap, bool enableColliders = true)
        {
            var line = Instantiate(PolygonLinePrefab, transform);
            var polygonLine = line.GetComponent<Line>();
            polygonLine.Initialize(lineCoordinates.Item1, lineCoordinates.Item2);
            polygonLine.EnableCollider = enableColliders;

            if (isPartOfMap)
            {
                line.layer = LayerMask.NameToLayer("Wall");
            }
            else
            {
                line.gameObject.tag = "FieldOfView";
            }

            lineContainer.Add(polygonLine);
        }

        private void InstantiatePolygon(Polygon polygon, List<Line> lineContainer, bool isPartOfMap, bool enableColliders = true)
        {
            var lines = polygon.GetLines();

            foreach (var line in lines)
            {
                InstantiatePolygonLine(line, lineContainer, isPartOfMap, enableColliders);
            }
        }

        private void InstantiateShadowPolygons()
        {
            DestroyShadowPolygon();
            var visiblePolygons = NPCManager.Instance.GetFovPolygons();

            foreach (var visiblePolygon in visiblePolygons)
            {
                InstantiatePolygon(visiblePolygon, _shadowLines, false, false);
            }

            EventManager.RaiseShadowVolumeChangedEvent(this, EventArgs.Empty);
        }

        private IEnumerator CreateShadowPolygon()
        {
            yield return new WaitForSeconds(1.0f);
            InstantiateShadowPolygons();
        }

        private void SetRenderMap(bool value)
        {
            foreach (var line in _mapLines)
            {
                line.IsRendering = value;
            }
        }

        private void SetRenderShadowMap(bool value)
        {
            foreach (var line in _shadowLines)
            {
                line.IsRendering = value;
            }
        }

        private void DestroyShadowPolygon()
        {
            foreach (var line in _shadowLines)
            {
                Destroy(line.gameObject);
            }

            _shadowLines.Clear();
        }
    }
}
