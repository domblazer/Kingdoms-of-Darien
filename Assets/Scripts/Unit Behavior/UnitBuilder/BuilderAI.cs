using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class BuilderAI : UnitBuilderAI
{
    // @TODO: BuilderAIs shouldn't just run out of the gates and start building. They need to search for a good location to build
    // whatever they are about to start building. Moreover, newly instantiated AI units probably need some delay between their initial
    // patrolling routing and when they can start searching for a good build location
}
