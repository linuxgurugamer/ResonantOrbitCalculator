using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResonantOrbitCalculator
{
    public class bodydef
    {
        //int color;
        public double eqr;
        public double mass;
        public double rot;
        public double atm;
        public double SOI;
        public CelestialBody body;

        public bodydef(CelestialBody body)
        {
            this.body = body;  
            eqr = body.Radius;
            mass = body.Mass;
            rot = body.rotationPeriod;
            atm = body.atmosphereDepth;
            SOI = body.sphereOfInfluence;
        }

        public double GM
        {
            get { return this.mass * 6.67408E-11; }
        }

        public double geoSMA
        {
            get
            {
                Log.Info("geoSMA, rot: " + rot);
                
                Log.Info("mass: " + mass + ", GM: " + GM);
                Log.Info("Math.Pow(((this.GM * Math.Pow(this.rot, 2)) / 39.4784176), 1 / 3): " + Math.Pow(((this.GM * Math.Pow(this.rot, 2)) / 39.4784176), 1f / 3f));
                Log.Info("Math.Round(Math.Pow(((this.GM * Math.Pow(this.rot, 2)) / 39.4784176), 1 / 3)): " + Math.Round(Math.Pow(((this.GM * Math.Pow(this.rot, 2)) / 39.4784176), 1 / 3)));
                if (this.rot != 0)
                    return Math.Round(Math.Pow(((this.GM * Math.Pow(this.rot, 2)) / 39.4784176), 1f / 3f));
                else return 0;
            }
        }

        public double geoAlt
        {
            get
            {
                Log.Info("geoAlt, geoSMA: " + geoSMA + ",   eqr: " + eqr);
                return this.geoSMA - this.eqr;
            }
        }

        public double SOIAlt()
        {
            return this.SOI - this.eqr;

        }

    }
}
