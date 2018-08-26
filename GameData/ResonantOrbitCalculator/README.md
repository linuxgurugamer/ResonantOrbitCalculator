
KSP Resonant Orbit Calculator

Strongly based on : https://meyerweb.com/eric/ksp/resonant-orbits/
Permission granted both by the license AND by the original author here:
    https://forum.kerbalspaceprogram.com/index.php?/topic/156018-resonant-orbit-calculator-v14/&do=findComment&comment=3342782

The original webpage has an MIT license.  I based a lot of the UI code on another mod, CorrectCoL,
(originally written by Boris-Barboris, now maintained by Linuxgurugamer) which also has an MIT license.

Therefore, this mod has an MIT license


"Jebediah", Bob said, "We need to be able to have constant contact with our probes.  As you know we  recently lost two because they lost communication with us at a crucial moment".
"Fine, but what do you want me to do about it?", asked Jeb
"We need you to launch a series of relay satellites which will blanket the globe; bonus would be to cover Mun and Minmus as well"
"That's going to be a lot of launches, can we afford it?"
"Yes, it's really quite simple.  Each launch will carry multiple satellites, we just have to drop them off in the right orbit"
Jebediah looked puzzled: "Huh?  And how do you expect us to do that accurately?"
"Well, we just happen to have this handy, dandy resonant orbit calculator, makes the entire job simple to do"

Jebediah frowned even more, "What's a resonant orbit?"
Bob sighed, rolled his eyes and said "Don't worry about it, just use the calculator and get the orbits"

Calculate the resonant orbit needed for a carrier craft to inject craft it carries, 
like satellites, into equidistant positions of a shared circular orbit. This is 
useful for setting up things like CommNet and GPS constellations. The “Injection Δv” 
value is the delta-v required to move from the resonant orbit to the final (circular) orbit.

In case you’re wondering “what the heck is this?”, a resonant orbit is most commonly used 
to set up CommNet constellations around non-Kerbin bodies.  Suppose you want to put three 
relay satellites into circular polar orbit around Minmus.  You could launch them one at a 
time from Kerbin and do all kinds of shenanigans to get them into a common orbit (say, 
100,000 km above Minmus) at 120-degree intervals along that shared orbit.  Which requires 
matching inclination and LAN and all manner of stuff, and then trying to jostle them into 
the right places along the circle.

Or, you could build a carrier craft that hauls three satellites to Minmus, then release 
them one at a time.  That solves inclination and LAN problems, but what about timing?  The 
easiest thing is to put the carrier craft into an eccentric orbit with its periapsis at the 
altitude the satellites should share, and an orbital period 4/3rds the length the satellites 
will have in their circular orbit.  In this example, the satellites’ final orbits at 100,000m 
above Minmus will have a period of 2 hours, 39 minutes, 29.5 seconds.  So you put the carrier 
into an orbit with a periapsis of 100,000m and an apoapsis of 167,652.4m.  That has an orbital 
period of 3 hours, 32 minutes, 39.3 seconds—exactly 133% the orbital period of the circular orbit.

Having done that, you just release one satellite from the carrier as it passes periapsis on 
each of three successive orbits.  Hey presto!  You now have three satellites in a polar triangle, 
sufficient to cover the entirety of Minmus and maintain a network back to Kerbin.  Quick, deorbit 
or otherwise move the carrier’s orbit so it won’t smack into the first satellite you released on 
its next periapsis.

This works for all planet packs, it reads the necessary information of the currently available planets.