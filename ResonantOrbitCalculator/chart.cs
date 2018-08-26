using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ProtoTurtle.BitmapDrawing;

namespace ResonantOrbitCalculator
{
    static public class chart
    {
        static bodydef body;
        static double body_r;
        static double bodySOI;
        //static string bodySOIlabel;
        static string SOId;
        //static double geosync;
        //static string geosynclabel;
        static string syncm;
#if false
	"lineofsight": document.getElementById("lineofsight"),
	"lineofsightlabel": document.getElementById("lineofsightlabel"),
	"atmosphere": document.getElementById("atmosphere"),
	"satellite": document.getElementById("satelliteorbit"),
#endif
        static double atmosphere_r;
        static double geosync_r;
        static double satellite_r;
        static bool satelliteWarning = false;
        //static internal bool carrierWarning = false;
        //static string carrier;
        //static string constellation;
        //static double lineofsight;
        //static string msa;
        static Vector2 launchpoint;

        static Color darkGrey = new Color(47f / 255f, 79f / 255f, 79f / 255f);
        static Color planetColor = darkGrey;

        static Color liteGrey = new Color(192f / 255f, 192f / 255f, 192f / 255f);

        static Color satColor = new Color(223f / 182f, 165f / 255f, 80f / 255f);  // goldenrod 
        static Color carrierColor = new Color(4f / 255f, 147f / 255f, 114f / 255f);
        static Color carrierWarningColor = Color.yellow;
        static Color carrierUrgentColor = Color.red;

        static Color medGrey = new Color(59f / 255f, 63f / 255f, 68f / 255f);
        static Color soiColor = Color.white; // new Color(236f / 255f, 236f / 255f, 236f / 255f);

        static Color geosyncColor = new Color(191f / 255f, 66f / 255f, 244f / 255f);  // purple
        // MA = Major Axis
        // SMA = Semi-Major Axis
        // ma = minor axis
        // sma = semi-minor axis
        // geoSMA = the semi-major axis of geosynchronous orbit. 
        internal static void drawchart(orbitdef s, orbitdef c, bodydef b)
        {
            body = b;
            double maxMA = Math.Max(s.MA, c.MA);
            if (maxMA == 0) maxMA = 3 * b.eqr;
            double coordkm = ((double)Math.Max(GraphWindow.GRAPH_HEIGHT, GraphWindow.GRAPH_WIDTH) / maxMA) / 1.5;

            float view = Math.Max(GraphWindow.GRAPH_HEIGHT, GraphWindow.GRAPH_WIDTH);
            float mid = view / 2;


            body_r = b.eqr * coordkm;

            if (b.atm != 0)
            {
                atmosphere_r = (b.eqr + b.atm) * coordkm;
            }
            else
            {
                atmosphere_r = 0;
            }

            // Draw the SOI
            var SOIr = b.SOI * coordkm;
            if (SOIr > 10000) SOIr = 10000;
            bodySOI = SOIr;

            GraphWindow.graph_texture.DrawFilledCircle(GraphWindow.HALF, GraphWindow.HALF, (SOIr <= GraphWindow.MAX_DIST)? (int)SOIr: GraphWindow.MAX_DIST, soiColor);
            GraphWindow.graph_texture.DrawCircle(GraphWindow.HALF, GraphWindow.HALF, (int)SOIr, medGrey, 10, 5);

            // This is the planet
            // Need to fill in the entire planet

            if (atmosphere_r > 0)
                GraphWindow.graph_texture.DrawFilledCircle(GraphWindow.HALF, GraphWindow.HALF, (int)(atmosphere_r), liteGrey);

            if (PlanetSelection.planetImg == null || !HighLogic.CurrentGame.Parameters.CustomParams<CCOLParams>().showPlanetImage)
            {
                //planetColor = HighLogic.CurrentGame.Parameters.CustomParams<CCOLParams>().individualizePlanetColors ? b.body.atmosphericAmbientColor : darkGrey;
                GraphWindow.graph_texture.DrawFilledCircle(GraphWindow.HALF, GraphWindow.HALF, (int)body_r, planetColor);
            } else
            {
                GraphWindow.graph_texture.DrawFilledCircle(GraphWindow.HALF, GraphWindow.HALF, (int)body_r, PlanetSelection.planetImg);
            }

            //bodySOIlabel = ("d", describeArc(mid, mid, (b.SOI * coordkm) + 7, 90, 270, 1));
            SOId = b.SOIAlt().ToString("N0");

            if (body.geoAlt < body.SOI)
            {
                geosync_r = b.geoSMA * coordkm;

                GraphWindow.graph_texture.DrawCircle(GraphWindow.HALF, GraphWindow.HALF, (int)geosync_r, geosyncColor);
                if ((b.geoSMA * coordkm) > 60)
                {
                    //chart.geosynclabel.setAttribute("d", describeArc(mid, mid, (b.geoSMA * coordkm) + 27, 60, 300));
                    syncm = b.geoAlt.ToString("N1");
                }
                else
                {
                    // geosynclabel = "M 0 0";
                }
            }
            else
            {
                //geosynclabel = "M 0 0";
                // geosync = 0;
            }

            satellite_r = s.SMA * coordkm;
            //p = OrbitCalc.DescribeArc(250, 250, (float)satellite_r, 0, 360);
            satelliteWarning = (s.Ap < b.atm);

            GraphWindow.graph_texture.DrawCircle(GraphWindow.HALF, GraphWindow.HALF, (int)satellite_r, satelliteWarning ? Color.red : satColor);

            //OrbitCalc.carrierPeWarning = (c.Pe < b.atm);

            //            carrier = "M " + mid + " " + ((view / 2) - s.SMA * coordkm) +
            //                " a " + (c.sma * coordkm) + "," + (c.SMA * coordkm) + " 0 1,0 1,0";
            double diff = s.SMA - c.SMA;
            double offset = (s.SMA - c.SMA) * coordkm;

            Color ellipseColor;
            if (!OrbitCalc.carrierPeWarning && !OrbitCalc.carrierPEUrgent)
                ellipseColor = carrierColor;
            else
               ellipseColor = !OrbitCalc.carrierPEUrgent ? carrierWarningColor : carrierUrgentColor;

            GraphWindow.graph_texture.DescribeEllipse(ellipseColor, GraphWindow.HALF, GraphWindow.HALF + (float)offset, (float)((c.sma) * coordkm ), (float)((c.SMA) * coordkm), 0, 360); //, 15, 5);
#if false
            if (!OrbitCalc.carrierPeWarning && !OrbitCalc.carrierPEUrgent)
                GraphWindow.DrawArc(GraphWindow.graph_texture, p, carrierColor);
            else
                GraphWindow.DrawArc(GraphWindow.graph_texture, p, !OrbitCalc.carrierPEUrgent ? carrierWarningColor : carrierUrgentColor);
#endif
            //double avg = (s.SMA + c.sma) / 2f;
            //GraphWindow.graph_texture.DrawCircle(GraphWindow.HALF, GraphWindow.HALF + (int)offset, (int)avg, OrbitCalc.carrierPeWarning ? Color.red : carrierColor);


            int sats = GraphWindow.numSats;
            OrbitCalc.minLOS = OrbitCalc.minLOSCalc(b);
            double rad = Math.Round(((b.eqr + OrbitCalc.minLOS)) * coordkm);

            if (sats >= 3 && GraphWindow.showLOSlines)
            {
                double sepang = (2 * Math.PI) / sats;

                double half = Math.Round(sats / 1.9);
                for (var i = 0; i <= sats; i++)
                {
                    double curang = i * sepang;
                    double xc = mid + Math.Sin(curang) * s.SMA * coordkm;
                    double yc = mid - Math.Cos(curang) * s.SMA * coordkm;

                    for (var j = 0; j <= half; j++)
                    {
                        double checkpos = (i + j) * sepang;
                        double checkang = checkpos - curang;
                        double h = Math.Cos(0.5 * checkang) * s.SMA;
                        //                        if (h * 1.001 >= b.eqr)
                        //if (h * 1.5 >= b.eqr)
                        {
                            double cxc = mid + Math.Sin(checkpos) * s.SMA * coordkm;
                            double cyc = mid - Math.Cos(checkpos) * s.SMA * coordkm;
                            if (OrbitCalc.constellationWarning)
                            {
                                GraphWindow.graph_texture.DrawLine((int)xc, (int)yc, (int)cxc, (int)cyc, Color.red, 15, 5);
                            }
                            else
                            {
                                if (h * 1.001  >= b.eqr * OrbitCalc.occmod(OrbitCalc.body))
                                    GraphWindow.graph_texture.DrawLine((int)xc, (int)yc, (int)cxc, (int)cyc, Color.blue);
                            }
                        }
                    }
                    if (i >= 0 && i < sats)
                    {
                        for (var j = 0; j <= sats; j++)
                        {
                            double ix = xc + Math.Sin(j * sepang) * 7.5;
                            double iy = yc + Math.Cos(j * sepang) * 7.5;

                            GraphWindow.graph_texture.DrawLine((int)xc, (int)yc, (int)ix, (int)iy, Color.green);
                        }
                    }
                }
            }
            else
            {
                // constellation = "M 0 0";
            }

            if (sats >= 3 && (OrbitCalc.minLOS * 10 >= s.Ap || Double.NaN == s.Ap) && rad > atmosphere_r)
            {

                //lineofsight = rad;

                GraphWindow.graph_texture.DrawCircle(GraphWindow.HALF, GraphWindow.HALF, (int)rad, Color.cyan, 5, 5);
                //chart.lineofsightlabel.setAttribute("d", describeArc(mid, mid, rad + 27, 60, 300));
                //msa = minLOS.ToString("N1");

            }
            else
            {
                //lineofsight = 0;
                //chart.lineofsightlabel.setAttribute("d", "M 0 0");
            }

            if (c.e == 0)
            {
                launchpoint.y = -1000;
            }
            else
            {
                launchpoint.y = (float)((view / 2) - s.SMA * coordkm);
            }
        }


    }
}
