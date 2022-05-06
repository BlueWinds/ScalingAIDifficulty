using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using BattleTech;

namespace ScalingAIDifficulty {
    public class Points {
      public float victory = 0;
      public float retreat = 0;
      public float defeat = 0;
      public float pilotInjured = 0;
      public float pilotKilled = 0;
      public float unitDestroyed = 0;
    }

    public class PointEffect {
      public string statName;
      public StatCollection.StatOperation operation = StatCollection.StatOperation.Float_Add;
      public float modValue = 0;
    }

    public class Settings {
        public bool debug = false;
        public bool trace = false;
        public float minPoints = -20;
        public float maxPoints = 20;
        public Points points = new Points();
        public PointEffect[] EnemyEffectsPerPoint;
        public PointEffect[] SelfEffectsPerPoint;
    }
}
