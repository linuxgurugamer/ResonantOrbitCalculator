using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


// This file comes with ABSOLUTELY NO WARRANTY!
// This is free software, and you are welcome to redistribute it
// under certain conditions, as outlined in the full content of
// the GNU General Public License(GNU GPL), version 3, revision
// date 29 June 2007.

// This file contains code from Mechjeb and GravityTurn

namespace ResonantOrbitCalculator
{
    public static class VesselOrbitalCalc
    {
        //normalized vector perpendicular to the orbital plane
        //convention: as you look down along the orbit normal, the satellite revolves counterclockwise
        public static Vector3d SwappedOrbitNormal(this Orbit o)
        {
            return -SwapYZ(o.GetOrbitNormal()).normalized;
        }
        //normalized vector pointing radially outward and perpendicular to prograde
        public static Vector3d RadialPlus(this Orbit o, double UT)
        {
            return Vector3d.Exclude(o.Prograde(UT), o.Up(UT)).normalized;
        }

        //another name for the orbit normal; this form makes it look like the other directions
        public static Vector3d NormalPlus(this Orbit o, double UT)
        {
            return o.SwappedOrbitNormal();
        }
        public static Vector3d DeltaVToManeuverNodeCoordinates(this Orbit o, double UT, Vector3d dV)
        {
            return new Vector3d(Vector3d.Dot(o.RadialPlus(UT), dV),
                                Vector3d.Dot(-o.NormalPlus(UT), dV),
                                Vector3d.Dot(o.Prograde(UT), dV));
        }
        public static bool patchedConicsUnlocked(this Vessel vessel)
        {
            return GameVariables.Instance.GetOrbitDisplayMode(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.TrackingStation)) == GameVariables.OrbitDisplayMode.PatchedConics;
        }
        public static ManeuverNode PlaceManeuverNode(Vessel vessel, Orbit patch, Vector3d dV, double UT)
        {
            if (vessel.patchedConicsUnlocked())
            {
                //placing a maneuver node with bad dV values can really mess up the game, so try to protect against that
                //and log an exception if we get a bad dV vector:
                for (int i = 0; i < 3; i++)
                {
                    if (double.IsNaN(dV[i]) || double.IsInfinity(dV[i]))
                    {
                        throw new Exception("VesselExtensions.PlaceManeuverNode: bad dV: " + dV);
                    }
                }

                if (double.IsNaN(UT) || double.IsInfinity(UT))
                {
                    throw new Exception("VesselExtensions.PlaceManeuverNode: bad UT: " + UT);
                }

                //It seems that sometimes the game can freak out if you place a maneuver node in the past, so this
                //protects against that.
                UT = Math.Max(UT, Planetarium.GetUniversalTime());

                //convert a dV in world coordinates into the coordinate system of the maneuver node,
                //which uses (x, y, z) = (radial+, normal-, prograde)
                Vector3d nodeDV = patch.DeltaVToManeuverNodeCoordinates(UT, dV);

                ManeuverNode mn = vessel.patchedConicSolver.AddManeuverNode(UT);
                mn.DeltaV = nodeDV;
                vessel.patchedConicSolver.UpdateFlightPlan();
                return mn;
            }
            else
                return null;
        }
        public static Vector3d CircularizeAtAP(Vessel vessel)
        {
            double UT = Planetarium.GetUniversalTime();
            UT += vessel.orbit.timeToAp;
            Vector3d deltav = (Vector3d)DeltaVToCircularize(vessel.orbit, UT);
            return deltav;
            //Log.Info("CircularizeAtAP, deltav: " + deltav);
            //vessel.PlaceManeuverNode(vessel.orbit, deltav, UT);

        }
        public static Vector3d CircularizeAtPE(Vessel vessel)
        {
            double UT = Planetarium.GetUniversalTime();
            UT += vessel.orbit.timeToPe;
            Vector3d deltav = (Vector3d)DeltaVToCircularize(vessel.orbit, UT);
            return deltav;
            //Log.Info("CircularizeAtPE, deltav: " + deltav);

            //vessel.PlaceManeuverNode(vessel.orbit, deltav, UT);

        }
        //normalized vector pointing radially outward from the planet
        public static Vector3d Up(this Orbit o, double UT)
        {
            return o.SwappedRelativePositionAtUT(UT).normalized;
        }
        public static Vector3d Reorder(this Vector3d vector, int order)
        {
            switch (order)
            {
                case 123:
                    return new Vector3d(vector.x, vector.y, vector.z);
                case 132:
                    return new Vector3d(vector.x, vector.z, vector.y);
                case 213:
                    return new Vector3d(vector.y, vector.x, vector.z);
                case 231:
                    return new Vector3d(vector.y, vector.z, vector.x);
                case 312:
                    return new Vector3d(vector.z, vector.x, vector.y);
                case 321:
                    return new Vector3d(vector.z, vector.y, vector.x);
            }
            throw new ArgumentException("Invalid order", "order");
        }

        //can probably be replaced with Vector3d.xzy?
        public static Vector3d SwapYZ(Vector3d v)
        {
            return v.Reorder(132);
        }
        //normalized vector along the orbital velocity
        public static Vector3d Prograde(this Orbit o, double UT)
        {
            return o.SwappedOrbitalVelocityAtUT(UT).normalized;
        }
        //
        // These "Swapped" functions translate preexisting Orbit class functions into world
        // space. For some reason, Orbit class functions seem to use a coordinate system
        // in which the Y and Z coordinates are swapped.
        //
        public static Vector3d SwappedOrbitalVelocityAtUT(this Orbit o, double UT)
        {
            return SwapYZ(o.getOrbitalVelocityAtUT(UT));
        }

        //position relative to the primary
        public static Vector3d SwappedRelativePositionAtUT(this Orbit o, double UT)
        {
            return SwapYZ(o.getRelativePositionAtUT(UT));
        }

        //distance from the center of the planet
        public static double Radius(this Orbit o, double UT)
        {
            return o.SwappedRelativePositionAtUT(UT).magnitude;
        }
        //normalized vector parallel to the planet's surface, and pointing in the same general direction as the orbital velocity
        //(parallel to an ideally spherical planet's surface, anyway)
        public static Vector3d Horizontal(this Orbit o, double UT)
        {
            return Vector3d.Exclude(o.Up(UT), o.Prograde(UT)).normalized;
        }
        //Computes the speed of a circular orbit of a given radius for a given body.
        public static double CircularOrbitSpeed(CelestialBody body, double radius)
        {
            //v = sqrt(GM/r)
            return Math.Sqrt(body.gravParameter / radius);
        }

        //Computes the deltaV of the burn needed to circularize an orbit at a given UT.
        public static Vector3d DeltaVToCircularize(Orbit o, double UT)
        {
            Vector3d desiredVelocity = CircularOrbitSpeed(o.referenceBody, o.Radius(UT)) * o.Horizontal(UT);
            Vector3d actualVelocity = o.SwappedOrbitalVelocityAtUT(UT);
            return desiredVelocity - actualVelocity;
        }
    }
}
