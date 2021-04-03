using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Geometry.Polygons;
using Main;
using Managers;
using UnityEngine;
using UnityEngine.Assertions;

namespace FileIO
{
    public class PolygonDataReader
    {
        public List<Polygon> Polygons { get; private set; }

        public PolygonDataReader()
        {
            Polygons = new List<Polygon>();
            ReadData();
        }

        private void ReadData()
        {
            var mapPath = $"{Properties.MapsPath}{GameManager.Instance.CurrentMap}{Properties.MapFileDotExt}";

            using (var reader = new StreamReader(File.OpenRead(mapPath)))
            {
                var lineIndex = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    AddPolygon(line, lineIndex);
                    lineIndex++;
                }
            }
        }

        private void AddPolygon(string verticesData, int lineIndex)
        {
            var data = verticesData.Split(Properties.MapFileSeparator);
            var polygon = new Polygon();
            //data must be even in order for everything to be properly initialized
            Assert.IsTrue(data.Length % 2 == 0);
            
            for (var i = 0; i < data.Length; i += 2)
            {
                var x = float.Parse(data[i]);
                var y = float.Parse(data[i + 1]);
                polygon.AddPoint(new Vector2(x, y));
            }

            if (lineIndex != 0)
            {
                polygon.EnsureWindingOrder(Properties.InnerPolygonWinding);
            }
            else
            {
                polygon.EnsureWindingOrder(Properties.OuterPolygonWinding);
            }

            Polygons.Add(polygon);
        }
    }
}
