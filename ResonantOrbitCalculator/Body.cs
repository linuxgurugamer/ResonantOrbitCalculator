using System;


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
                if (this.rot != 0)
                    return Math.Round(Math.Pow(((this.GM * Math.Pow(this.rot, 2)) / 39.4784176), 1f / 3f));
                else return 0;
            }
        }

        public double geoAlt
        {
            get
            {
                return this.geoSMA - this.eqr;
            }
        }

        public double SOIAlt()
        {
            return this.SOI - this.eqr;

        }

    }
}
