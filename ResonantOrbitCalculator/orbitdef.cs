﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResonantOrbitCalculator
{
    // MA = Major Axis
    // SMA = Semi-Major Axis
    // ma = minor axis
    // sma = semi-minor axis
    // geoSMA = the semi-major axis of geosynchronous orbit. 

    public class orbitdef
    {
        public double Ap;
        public double Pe;
        bodydef body;

        public orbitdef(double Ap, double Pe, bodydef body)
        {
            this.Ap = Ap;
            this.Pe = Pe;
            this.body = body;
        }

        public double GM { get { return this.body.GM; } }

        public double eqr { get { return this.body.eqr; } }

        public double MA { get { return this.Ap + this.Pe + 2 * this.eqr; } }
        public double SMA { get { return this.MA / 2; } }
        public double ma { get { return this.MA * Math.Sqrt(1 - (Math.Pow(this.e, 2))); } }
        public double sma { get { return this.ma / 2; } }
        public double ApR { get { return this.Ap + this.eqr; } }
        public double PeR { get { return this.Pe + this.eqr; } }
        public double PeV { get { return Math.Sqrt((2 * this.ApR * this.GM) / (this.MA * this.PeR)); } }
        public double ApV { get { return Math.Sqrt((2 * this.PeR * this.GM) / (this.MA * this.ApR)); } }
        public double e { get { return ((this.PeR * Math.Pow(this.PeV, 2)) / this.GM) - 1; } }
        public double F { get { return Math.Sqrt(Math.Pow(this.SMA, 2) - Math.Pow(this.sma, 2)); } }
        public double T { get { return 2 * Math.PI * Math.Sqrt(Math.Pow(this.SMA, 3) / this.GM); } }
        public double op { get { return this.T / 3600; } }
        public string oph
        {
            get
            {
                double hours = Math.Floor(this.op);
                double min = Math.Floor((this.op - hours) * 60);
                double sec = Math.Round(10 * (this.op - hours - min / 60) * 3600) / 10;
                string time = hours + "h:" + min + "m:" + sec + "s";
                return time;
            }
        }
 
 //       public double newMAfromT(double newT) { return 2 * OrbitCalc.Cbrt((this.GM * Math.Pow(newT, 2)) / 39.4784176); }
        // 39.4784176 = 4π^2
        public void setAp(double alt) { this.Ap = alt; }

        public void setPe(double alt) { this.Pe = alt; }

        public void modifyAp(double newMA) { this.setAp(this.Ap + (newMA - this.MA)); }

        public void modifyPe(double newMA) { this.setPe(this.Pe - (this.MA - newMA)); }

    }
}
