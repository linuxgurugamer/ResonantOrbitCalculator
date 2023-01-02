using System;

namespace ResonantOrbitCalculator
{
    public static class OrbitCalc
    {
        static public double Cbrt(double d)
        {
            return Math.Pow(d, 1.0f / 3.0f);
        }

        static public double newMAfromT(double T, bodydef body)
        {
            return 2 * Cbrt((body.GM * Math.Pow(T, 2f)) / 39.4784176);  // 39.4784176 = 4π^2
        }


        static double burnCalc(orbitdef s, orbitdef c, bodydef b)
        {
            double sta = 0;
            double cta = 0;
            if (c.Ap == s.Ap) cta = 180;
            double sr = s.SMA * (1 - Math.Pow(s.e, 2)) / (1 + (s.e * Math.Cos(sta)));
            double sdv = Math.Sqrt(b.GM * ((2 / sr) - (1 / s.SMA)));

            double cr = c.SMA * (1 - Math.Pow(c.e, 2)) / (1 + (c.e * Math.Cos(cta)));
            double cdv = Math.Sqrt(b.GM * ((2 / sr) - (1 / c.SMA)));

            return Math.Round(100 * Math.Abs(sdv - cdv)) / 100;
        }


        static internal double minLOSCalc(bodydef body)
        {
            var sat = GraphWindow.numSats;
            if (sat >= 3) return (body.eqr * occmod(body)) / (Math.Cos(0.5 * (2f * Math.PI / sat))) - body.eqr;
            return 0;
        }

        static internal double occmod(bodydef body)
        {
            if (!GraphWindow.occlusionModifiers) return 1;
            if (body.atm > 0)
            {
                return GraphWindow.atmOcclusion;
            }
            else
            {
                return GraphWindow.vacOcclusion;
            }
        }

        public static bool disabled = false;
        public static string synchrorbit = "";
        public static string losorbit = "";
        public static bool losOrbitWarning = false;
        public static bool constellationWarning = false;
        public static string period = "";
        public static string periodHour = "";
        public static string periodMin = "";
        public static string periodSec = "";
        public static bool periodEntry = false;
        public static string carrierAp = "";
        public static bool carrierApWarning = false;
        public static string carrierPe = "";
        public static bool carrierPeWarning = false;
        public static bool carrierPEUrgent = false;
        public static string carrierT = "";
        public static string burnDV = "";
        public static double dBurnDV = 0f;
        public static double minLOS;
        public static string actualLOSlength;
        public static string[] header = new string[2];
        public static bodydef body;

        public static orbitdef satelliteorbit;
        public static orbitdef carrierorbit;

        public enum Units { m, km, Mm, Gm};
        public static Units units = Units.m;
        //public static bool m = true, km = false, Mm = false, Gm = false;

        public static string GetFormattedDistance(double v, string format)
        {
            switch (units)
            {
                case Units.m:
                    return v.ToString(format) + " m";
                case Units.km:
                    return (v / 1000).ToString(format) + " km";
                case Units.Mm:
                    return (v / 1000000).ToString(format) + " Mm";
                default:
                    return (v / 1000000000).ToString(format) + " Gm";
            }
        }
        public static void Update()
        {

            body = new bodydef(PlanetSelection.selectedBody);

            var sataltitude = GraphWindow.orbitAltitude;

            if (sataltitude > body.SOIAlt())
            {
                disabled = true;
            }
            if (body.geoAlt > 0 && body.geoSMA < body.SOI)
            {
                //synchrorbit = body.geoAlt.ToString("N") + " m";
                synchrorbit = GetFormattedDistance(body.geoAlt, "N");
            }
            else
            {
                synchrorbit = "n/a";
            }

            minLOS = minLOSCalc(body);
            if (minLOS < body.SOIAlt() && minLOS > 0)
            {
                //losorbit = minLOS.ToString("N") + " m";
                losorbit = GetFormattedDistance(minLOS,"N");
                losOrbitWarning = (minLOS < body.atm);

                constellationWarning = sataltitude * 1.01 < minLOS;
            }
            else
            {
                losorbit = "n/a";
            }


            double satcount = GraphWindow.numSats;
            double satratio = ((satcount + 1) / satcount);
            satelliteorbit = new orbitdef(sataltitude, sataltitude, body);
            carrierorbit = new orbitdef(sataltitude, sataltitude, body);


            if (satcount > 0 && sataltitude > 0)
            {
                carrierorbit.modifyAp(newMAfromT(satelliteorbit.T * satratio, body));

                if (carrierorbit.Ap > body.SOIAlt() || GraphWindow.flipOrbit == true)
                {
                    satratio = ((satcount - 1) / satcount);
                    carrierorbit = new orbitdef(sataltitude, sataltitude, body);
                    carrierorbit.modifyPe(newMAfromT(satelliteorbit.T * satratio, body));
                }

                period = satelliteorbit.oph;
                if (!periodEntry)
                {
                    periodHour = satelliteorbit.op_p(orbitdef.timePos.hours).ToString();
                    periodMin = satelliteorbit.op_p(orbitdef.timePos.min).ToString();
                    periodSec = satelliteorbit.op_p(orbitdef.timePos.sec).ToString();
                }
                /* if (carrierorbit.Ap > 0) */
                //carrierAp = carrierorbit.Ap.ToString("N1") + " m";
                carrierAp = GetFormattedDistance(carrierorbit.Ap,"N1");
                carrierApWarning = (carrierorbit.Ap < body.atm || carrierorbit.Ap < 0);

                /*if (carrierorbit.Pe > 0)*/
                //carrierPe = carrierorbit.Pe.ToString("N1") + " m";
                carrierPe = GetFormattedDistance(carrierorbit.Pe, "N1");


                carrierPeWarning = (carrierorbit.Pe < body.atm);
                carrierPEUrgent = (carrierorbit.Pe <= 0);

                carrierT = carrierorbit.oph;

                dBurnDV = burnCalc(satelliteorbit, carrierorbit, body);
                burnDV = dBurnDV.ToString("N2") + " m/s";

                // actualLOSlength = (2 * (satelliteorbit.a(satelliteorbit.T) + satelliteorbit.body.body.Radius) * Math.Sin(Math.PI / satcount)).ToString("N1");
                actualLOSlength = GetFormattedDistance(2 * (satelliteorbit.a(satelliteorbit.T) + satelliteorbit.body.body.Radius) * Math.Sin(Math.PI / satcount),"N1");
            }


            if (period == "")
            {
                header[0] = "";
                header[1] = "";
            }
            else
            {
                string s = body.body.displayName.Substring(0, body.body.displayName.Length - 2);
                header[0] = s;
                header[0] += " " + satcount + "-satellite constellation";
                header[1] = "Ap " + carrierAp;
                header[1] += "     Pe " + carrierPe;
                header[1] += "     Δv " + burnDV;
            }

        }

    }

}
